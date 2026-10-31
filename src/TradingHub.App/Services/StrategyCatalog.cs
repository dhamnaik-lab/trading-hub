using TradingHub.Scripting;
using TradingHub.Strategies;

namespace TradingHub.App.Services;

/// <summary>Una estrategia en disco (un archivo .cs) y su última compilación.</summary>
public sealed class StrategyFile
{
    public required string Path { get; init; }
    public string FileName => System.IO.Path.GetFileNameWithoutExtension(Path);
    public Type? Type { get; set; }
    public ScriptCompileResult? LastCompile { get; set; }
    public DateTime CompiledFileTime { get; set; }
    public string DisplayName => Type != null ? Strategy.DisplayName(Type) : FileName;

    /// <summary>Id estable para el historial (el nombre de la clase).</summary>
    public string Id => Type?.FullName ?? FileName;

    public Strategy Create() => (Strategy)Activator.CreateInstance(Type ?? throw new InvalidOperationException("Strategy has not been compiled."))!;
}

/// <summary>Estrategias del usuario: archivos en ~/Documents/Trading Hub/Estrategias, compilados con Roslyn.</summary>
public sealed class StrategyCatalog
{
    private readonly ScriptCompiler _compiler = new();

    public StrategyCatalog()
    {
        _ = typeof(Strategy); // asegura que TradingHub.Strategies esté cargado para referenciarlo al compilar
        // TRADINGHUB_BACKTESTS_DIR: historial alternativo (pruebas)
        StrategyStore.Root = Environment.GetEnvironmentVariable("TRADINGHUB_BACKTESTS_DIR") is { Length: > 0 } dir ? dir : AppPaths.Backtests;
        Directory.CreateDirectory(AppPaths.Strategies);
        if (!Directory.EnumerateFiles(AppPaths.Strategies, "*.cs").Any())
        {
            foreach (var t in StrategyTemplates.All.Where(t => t.Example))
                File.WriteAllText(System.IO.Path.Combine(AppPaths.Strategies, t.FileName + ".cs"), t.Code(t.FileName).Replace("__NOMBRE__", t.Name));
        }

        Refresh();
    }

    public List<StrategyFile> Files { get; } = new();

    public event Action? Changed;

    public void Refresh()
    {
        var existing = Files.ToDictionary(f => f.Path);
        Files.Clear();
        var extra = Enumerable.Empty<string>();
        foreach (var path in Directory.EnumerateFiles(AppPaths.Strategies, "*.cs").Concat(extra).OrderBy(p => p))
            Files.Add(existing.TryGetValue(path, out var f) ? f : new StrategyFile { Path = path });
        Changed?.Invoke();
    }

    /// <summary>Compila un archivo (si cambió desde la última vez o si se fuerza).</summary>
    public ScriptCompileResult Compile(StrategyFile file, bool force = false)
    {
        var stamp = File.GetLastWriteTimeUtc(file.Path);
        if (!force && file.LastCompile != null && stamp == file.CompiledFileTime) return file.LastCompile;
        var res = _compiler.Compile(new[] { file.Path }, "Estrategia_" + file.FileName);
        file.LastCompile = res;
        file.CompiledFileTime = stamp;
        if (res.Success && res.Assembly != null)
        {
            var types = res.Assembly.GetTypes().Where(t => !t.IsAbstract && typeof(Strategy).IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null).ToList();
            file.Type = types.FirstOrDefault();
            if (file.Type == null)
            {
                var diag = new ScriptDiagnostic(file.Path, 1, 1, "error", "TH01", "File contains no public nonabstract Strategy subclass with a parameterless constructor.");
                file.LastCompile = new ScriptCompileResult { Success = false, Diagnostics = res.Diagnostics.Append(diag).ToList(), Elapsed = res.Elapsed };
            }
        }
        else
        {
            file.Type = null;
        }

        Changed?.Invoke();
        return file.LastCompile;
    }

    /// <summary>Crea un archivo nuevo desde una plantilla y devuelve su ruta.</summary>
    public StrategyFile Create(string name, StrategyTemplate template)
    {
        string className = new string(name.Where(char.IsLetterOrDigit).ToArray());
        if (className.Length == 0 || char.IsDigit(className[0])) className = "Estrategia" + className;
        var path = System.IO.Path.Combine(AppPaths.Strategies, className + ".cs");
        for (int i = 2; File.Exists(path); i++) path = System.IO.Path.Combine(AppPaths.Strategies, $"{className}{i}.cs");
        className = System.IO.Path.GetFileNameWithoutExtension(path);
        File.WriteAllText(path, template.Code(className).Replace("__NOMBRE__", name.Replace("\"", "'")));
        Refresh();
        return Files.First(f => f.Path == path);
    }

    public void Delete(StrategyFile file)
    {
        if (File.Exists(file.Path)) File.Delete(file.Path);
        Refresh();
    }
}

/// <summary>Plantilla de estrategia.</summary>
public sealed record StrategyTemplate(string Name, string Description, string FileName, bool Example, Func<string, string> Code);

/// <summary>Plantillas incluidas (también se usan como ejemplos la primera vez).</summary>
public static class StrategyTemplates
{
    public static readonly StrategyTemplate[] All =
    {
        new("Moving average crossover", "Go long when the fast EMA crosses above the slow EMA and short on the opposite cross. Good introductory example.", "CruceDeMedias", true, c => $$"""
            using TradingHub.Strategies;

            [Strategy("__NOMBRE__", Description = "Two exponential moving averages with configurable stop-loss and take-profit ticks.")]
            public class {{c}} : Strategy
            {
                [Parameter("Fast EMA"), Optimize(5, 30, 5)]
                public int Rapida { get; set; } = 10;

                [Parameter("Slow EMA"), Optimize(20, 100, 10)]
                public int Lenta { get; set; } = 40;

                private Ema _rapida = null!, _lenta = null!;

                protected override void OnInit()
                {
                    _rapida = Ema(Rapida);
                    _lenta = Ema(Lenta);
                }

                protected override void OnBar(Bar bar)
                {
                    if (!_lenta.IsReady) return;

                    if (CrossAbove(_rapida, _lenta)) EnterLong();
                    else if (CrossBelow(_rapida, _lenta)) EnterShort();
                }
            }
            """),

        new("Range breakout", "Buy a breakout above the prior N-bar high or sell below its low with an ATR-based stop.", "RupturaDeRango", true, c => $$"""
            using TradingHub.Strategies;

            [Strategy("__NOMBRE__", Description = "Donchian channel breakout with ATR-based exit.")]
            public class {{c}} : Strategy
            {
                [Parameter("Channel bars"), Optimize(10, 60, 10)]
                public int Canal { get; set; } = 20;

                [Parameter("ATR stop multiplier"), Optimize(1, 4, 0.5)]
                public double MultiploAtr { get; set; } = 2;

                private Highest _maximo = null!;
                private Lowest _minimo = null!;
                private Atr _atr = null!;
                private decimal _stop;

                protected override void OnInit()
                {
                    _maximo = Highest(Canal);
                    _minimo = Lowest(Canal);
                    _atr = Atr(14);
                }

                protected override void OnBar(Bar bar)
                {
                    if (Bars.Count <= Canal || !_atr.IsReady) return;
                    decimal techo = _maximo[1], piso = _minimo[1];
                    decimal distancia = _atr.Value * (decimal)MultiploAtr;

                    if (IsFlat)
                    {
                        if (bar.Close > techo) { EnterLong(); _stop = bar.Close - distancia; }
                        else if (bar.Close < piso) { EnterShort(); _stop = bar.Close + distancia; }
                    }
                    else if (IsLong)
                    {
                        _stop = Math.Max(_stop, bar.Close - distancia);
                        if (bar.Close < _stop) ClosePosition("Stop ATR");
                    }
                    else
                    {
                        _stop = Math.Min(_stop, bar.Close + distancia);
                        if (bar.Close > _stop) ClosePosition("Stop ATR");
                    }
                }
            }
            """),

        new("Delta absorption", "Order flow: bullish candle despite strongly negative delta can indicate sell-side absorption; reverse for shorts.", "AbsorcionConDelta", true, c => $$"""
            using TradingHub.Strategies;

            [Strategy("__NOMBRE__", Description = "Detects high aggressive volume that fails to move price in its direction.")]
            public class {{c}} : Strategy
            {
                [Parameter("Volume average bars")]
                public int Promedio { get; set; } = 20;

                [Parameter("Minimum volume (× average)"), Optimize(1.0, 3.0, 0.5)]
                public double VolumenMinimo { get; set; } = 1.5;

                [Parameter("Minimum delta (% of volume)"), Optimize(10, 40, 10)]
                public int DeltaMinimo { get; set; } = 20;

                [Parameter("Maximum bars in trade")]
                public int VelasMaximas { get; set; } = 6;

                private Sma _volumen = null!;
                private int _barEntrada;

                public {{c}}()
                {
                    StopLossTicks = 40;
                    TakeProfitTicks = 80;
                }

                protected override void OnInit()
                {
                    UseFootprint = true; // price-level volume per bar (bar.Levels, bar.PocPrice)
                    _volumen = Sma(Promedio, PriceSource.Volume);
                }

                protected override void OnBar(Bar bar)
                {
                    if (!_volumen.IsReady) return;

                    if (!IsFlat && CurrentBar - _barEntrada >= VelasMaximas)
                    {
                        ClosePosition("Tiempo");
                        return;
                    }

                    bool volumenAlto = bar.Volume >= _volumen[1] * (decimal)VolumenMinimo;
                    decimal deltaPct = bar.Volume > 0 ? 100 * bar.Delta / bar.Volume : 0;

                    // strong aggressive selling, but bar closes above POC: potential absorption
                    if (IsFlat && volumenAlto && deltaPct <= -DeltaMinimo && bar.Close > bar.PocPrice && bar.IsBullish)
                    {
                        EnterLong();
                        _barEntrada = CurrentBar;
                    }
                    else if (IsFlat && volumenAlto && deltaPct >= DeltaMinimo && bar.Close < bar.PocPrice && bar.IsBearish)
                    {
                        EnterShort();
                        _barEntrada = CurrentBar;
                    }
                }
            }
            """),

        new("VWAP mean reversion", "Trades reversion to session VWAP after an ATR-based excursion, within configured hours.", "ReversionVwap", true, c => $$"""
            using TradingHub.Strategies;

            [Strategy("__NOMBRE__", Description = "Sells extensions above VWAP, buys below, and exits on return to VWAP.")]
            public class {{c}} : Strategy
            {
                [Parameter("Distance (× ATR)"), Optimize(1.0, 3.0, 0.5)]
                public double Distancia { get; set; } = 2;

                [Parameter("From (local hour)")]
                public int HoraDesde { get; set; } = 10;

                [Parameter("Until (local hour)")]
                public int HoraHasta { get; set; } = 20;

                private Vwap _vwap = null!;
                private Atr _atr = null!;

                public {{c}}()
                {
                    StopLossTicks = 60;
                }

                protected override void OnInit()
                {
                    _vwap = Vwap();
                    _atr = Atr(20);
                }

                protected override void OnBar(Bar bar)
                {
                    if (!_atr.IsReady) return;
                    bool horario = TimeBetween(TimeSpan.FromHours(HoraDesde), TimeSpan.FromHours(HoraHasta));
                    decimal banda = _atr.Value * (decimal)Distancia;

                    if (IsLong && bar.Close >= _vwap.Value) ClosePosition("VWAP");
                    else if (IsShort && bar.Close <= _vwap.Value) ClosePosition("VWAP");
                    else if (IsFlat && horario)
                    {
                        if (bar.Close < _vwap.Value - banda) EnterLong();
                        else if (bar.Close > _vwap.Value + banda) EnterShort();
                    }

                    // close any remaining position outside the configured session
                    if (!horario && !IsFlat) ClosePosition("Fin del horario");
                }
            }
            """),

        new("Blank", "Minimal template showing the available lifecycle methods.", "MiEstrategia", false, c => $$"""
            using TradingHub.Strategies;

            [Strategy("__NOMBRE__", Description = "")]
            public class {{c}} : Strategy
            {
                // [Parameter("Name"), Optimize(min, max, step)] exposes parameters for optimization
                [Parameter("Period"), Optimize(10, 50, 10)]
                public int Periodo { get; set; } = 20;

                private Sma _media = null!;

                protected override void OnInit()
                {
                    // indicators: Sma, Ema, Rsi, Atr, Vwap, Bollinger, Macd, Highest, Lowest, CumulativeDelta…
                    _media = Sma(Periodo);
                }

                protected override void OnBar(Bar bar)
                {
                    // bar is the newly completed candle (Open, High, Low, Close, Volume, BuyVolume, SellVolume, Delta…)
                    // orders: EnterLong(), EnterShort(), Buy(), Sell(), BuyLimit(price), SellStop(price), ClosePosition()
                    // automatic exits: StopLossTicks, TakeProfitTicks, TrailingStopTicks
                }
            }
            """),
    };
}
