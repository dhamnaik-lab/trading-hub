[← Back to documentation](README.md)

# C# strategies

Trading Hub strategies are C# classes extending `Strategy`. The same source is used by the tick backtester, optimization, walk-forward, chart overlays, and supported live accounts. This guide documents the API and provides two original examples with their displayed messages translated into English.

## File locations and compilation

- Store each strategy as a `.cs` file in the user strategy directory (macOS `~/Documents/Trading Hub/Estrategias`; Windows equivalent in Documents). Open it from **Backtesting**, **Code**, or **Settings → Data**.
- The application creates example strategies in an initially empty folder, including moving average crossover, opening range breakout, delta absorption, and VWAP reversion.
- Source is compiled using Roslyn. Use **Compile** (Ctrl/⌘+B) or **Compile and test** after editing. **Save** only writes source: the next backtest uses the most recently compiled version until you recompile.
- A strategy requires one public nonabstract class inheriting `Strategy`, a parameterless constructor and `using TradingHub.Strategies;`. Keep each class in its own file. Saved test history is associated with the class name, so renaming it may create a separate history.
- The same ATAS namespace compatibility adaptations and compile symbols apply to indicators and strategies; see [Indicators](indicadores.md).

## Attributes and parameters

- `[Strategy("Display name", Description = "...")]` defines the visible class title.
- `[Parameter("Name", Description = "...")]` decorates public get/set properties shown in Backtesting, Live, and chart strategy controls. Supported types include `int`, `long`, `double`, `decimal`, `bool`, enums, and `string`.
- `[Optimize(min, max, step)]` supplies a default optimization range.

All strategies also support built-in position-management parameters:

| Property | Default | Purpose |
|---|---|---|
| `Quantity` | 1 | Default order quantity |
| `StopLossTicks` | 0 | Stop distance from average entry |
| `TakeProfitTicks` | 0 | Target distance from average entry |
| `TrailingStopTicks` | 0 | Favorable-price trailing stop distance |
| `RegimeFilter` | All | Allowed market regimes for new entries |
| `MinVolatilityRank`, `MaxVolatilityRank` | 0, 100 | Allowed percentile range for new entries |

## Lifecycle

| Method | When it runs |
|---|---|
| `OnInit()` | Startup; initialize indicators and enable `UseFootprint` if price-level volume is required |
| `OnBar(Bar bar)` | On completion of each candle; typical location for trading signals |
| `OnTick(Trade trade)` | Every market trade; requires `TradingHub.Market` and increases compute load |
| `OnOrderFilled(StrategyOrder order)` | Order fill callback; modeled automatic stops/targets in backtests, strategy-submitted orders in live trading |
| `OnSessionStart(DateOnly session)` | Start of a new subsequent session |
| `OnStop()` | Backtest completion or strategy stop |

Indicators and context update before `OnBar`. A bar is considered closed when the first trade of the next bar arrives; a market order created by `OnBar` is simulated on that next trade (an order from `OnTick` is eligible on the following trade).

Time helpers use the configured session zone and start time in backtesting; chart strategies use the chart time zone and a midnight session start; live strategies use their saved backtest session rules.

## Bars, market data, and position

- `Bars[0]` is the latest **completed** candle; `Bars[1]` the previous completed candle. `Bars.At(i)` counts from the first historical bar. `Bar` is the latest completed candle, `FormingBar` the current incomplete candle, and `CurrentBar` its zero-based index.
- Bar fields include `Time`, `CloseTime`, `Open`, `High`, `Low`, `Close`, `Volume`, `BuyVolume`, `SellVolume`, `Delta`, `MaxDelta`, `MinDelta`, `Trades`, `IsBullish`, `IsBearish`, `Range`, `Body`, `Typical`, `Index`. Set `UseFootprint = true` to access `PocPrice` and price-level `Levels`.
- Runtime properties: `Price`, `Time` (UTC), `LocalTime`, `TickSize`, `Instrument`, `Period`, `IsBacktest`.
- Position properties: `Position` (+long, −short), `IsFlat`, `IsLong`, `IsShort`, `AveragePrice`, `OpenPnL`, `NetProfit`, and `ActiveOrders`.

## Orders

| Method | Behavior |
|---|---|
| `EnterLong(qty?, name?)` / `EnterShort(...)` | Enter at market, reverse an opposite position, do nothing if already positioned in the requested direction |
| `Buy(qty?, name?)` / `Sell(...)` | Submit a market order |
| `BuyLimit(price, qty?, name?)` / `SellLimit(...)` | Limit order |
| `BuyStop(price, qty?, name?)` / `SellStop(...)` | Stop trigger that becomes a market order |
| `ClosePosition(name?)` | Cancel pending entry orders and close at market |
| `CancelAll()`, `Cancel(order)`, `Modify(order, price)` | Cancel or reprice active limit/stop orders |

Quantity defaults to `Quantity`, and prices are rounded to tick size. Returned `StrategyOrder` properties include `Id`, `Side`, `Type`, `Quantity`, `Price`, `Name`, `State`, `IsActive`, `FillPrice`, `FillTime`, and `IsProtective`. See [Backtesting](backtesting.md) for fill assumptions.

## Automatic stops, targets and trailing

Setting `StopLossTicks` and/or `TakeProfitTicks` above zero attaches protective exits to position changes. OCO logic cancels the alternative exit after a fill. Trailing stops move favorably and do not loosen. Without an initial stop distance, a trailing distance can provide the initial stop. In live trading, protective orders are sent to the broker and OCO management is coordinated by Trading Hub. The original design limits trailing updates to at most once per second.

`ClosePosition` and `CancelAll` do not directly manipulate the automatic protective-order list; those orders are handled when the position becomes flat.

## Indicator helpers

| Method | Meaning |
|---|---|
| `Sma(n, source)`, `Ema(n, source)` | Simple and exponential moving averages |
| `Rsi(n = 14, source)`, `Atr(n = 14)`, `StdDev(n, source)` | RSI, ATR, standard deviation |
| `Highest(n, source = High)`, `Lowest(n, source = Low)` | Extreme over latest n bars, **including** the latest bar |
| `Bollinger(n = 20, k = 2, source)` | Middle, upper/lower bands, history helpers |
| `Macd(12, 26, 9)` | MACD, signal and histogram |
| `Vwap()`, `CumulativeDelta(resetEachSession = true)` | Session VWAP, cumulative delta |
| `Custom("name", bars => value)` | Custom series evaluated at every completed candle |

`PriceSource` values include Close, Open, High, Low, Typical, Median, Volume, and Delta. `indicator[0]` reads the latest completed candle, `indicator[1]` the one before; each has `Value`, `Count`, and `IsReady`.

**Important:** `Highest` and `Lowest` include the latest bar, so a breakout should compare the current close against the previous extreme, such as `Highest(n)[1]`.

Other helpers include `CrossAbove`, `CrossBelow`, `Ticks(n)`, `Round(price)`, `TimeBetween(from, to)`, and `Log(text)`. Logs appear in Backtesting (up to 5,000 lines) and in live Activity.

## Drawing API

Drawings appear with strategies attached to charts. Timestamps use UTC. Color strings support named colors or hex notation. Reusing an ID replaces the existing drawing; older drawings are removed when the documented 3,000-drawing cap is reached, and a strategy can plot up to 20 continuous series.

| Method | Purpose |
|---|---|
| `DrawBox(id, from, price1, to, price2, color, fillOpacity, text)` | Rectangle |
| `DrawLine(id, t1, p1, t2, p2, color, width, style)` | Line |
| `DrawHorizontalLine(id, price, from, color, width, style, text)` | Horizontal price level |
| `DrawArrowUp`, `DrawArrowDown` | Direction arrows |
| `DrawMarker` | Circle, square, diamond, cross, arrow shapes |
| `DrawText` | Text at a chart coordinate |
| `Plot("name", value, color, width, style)` | Continuous series; null breaks a segment |
| `RemoveDrawing(id)`, `ClearDrawings()` | Remove one/all drawings |

## Market regime and entry filter

Market context is computed from **completed bars**, using recent hourly closes or a longer-period fallback. It includes:

| Property | Description |
|---|---|
| `MarketTrend` | Absolute net move divided by total movement, 0–1 |
| `Regime` | `Range` (≤0.15), `Mixed`, `Trend` (≥0.35), or `Unknown` |
| `MarketDirection`, `MarketChange` | Direction (+1/−1) and net percentage move |
| `MarketVolatility`, `VolatilityRank` | Return volatility and trailing 30-day percentile |
| `ContextAllowsEntries` | Whether current filters allow new positions |

Warm-up may be insufficient at startup. The engine can incorporate historical hourly bars, including exchange candles during live runs. `RegimeFilter` supports combinations such as `All`, `RangeOnly`, `NoTrend`, `TrendOnly`, `NoRange`, `UpTrendOnly`, `DownTrendOnly`, `MixedOnly`, and mixed/range-direction combinations.

The context filter blocks **new/increasing entries** outside the selected regimes while allowing position-reducing exits. If context is unavailable, the documented behavior is permissive. Filters can be changed live; see [Live strategies](en-vivo.md).

## Example 1: time-filtered EMA crossover

The source class and public property identifiers are preserved so the original examples remain compatible; the displayed texts are in English.

```csharp
using TradingHub.Strategies;

[Strategy("Time-filtered moving average crossover", Description = "Go long when fast EMA crosses above slow EMA and short on the reverse cross, during the session only.")]
public class CruceDeMediasHorario : Strategy
{
    [Parameter("Fast EMA"), Optimize(5, 30, 5)] public int Rapida { get; set; } = 10;
    [Parameter("Slow EMA"), Optimize(20, 100, 10)] public int Lenta { get; set; } = 40;
    [Parameter("From hour (session)")] public int HoraDesde { get; set; } = 9;
    [Parameter("To hour (session)")] public int HoraHasta { get; set; } = 16;

    private Ema _rapida = null!, _lenta = null!;

    // automatic 40-tick stop and 80-tick target, linked as OCO
    public CruceDeMediasHorario() { StopLossTicks = 40; TakeProfitTicks = 80; }

    protected override void OnInit() { _rapida = Ema(Rapida); _lenta = Ema(Lenta); }

    protected override void OnBar(Bar bar)
    {
        if (!_lenta.IsReady) return;
        Plot("Fast", _rapida[0], "cyan"); // plot both moving averages for each closed bar
        Plot("Slow", _lenta[0], "orange");

        // outside the trading session flatten and do not enter
        if (!TimeBetween(new TimeSpan(HoraDesde, 0, 0), new TimeSpan(HoraHasta, 0, 0)))
        {
            if (!IsFlat) ClosePosition("Session end");
        }
        else if (CrossAbove(_rapida, _lenta)) EnterLong(); // reverse if currently short
        else if (CrossBelow(_rapida, _lenta)) EnterShort();
    }
}
```

## Example 2: opening-range breakout

Run this example on index futures with **New York** as the session zone and a 09:30 session open. It draws the range, places stop entries on both sides after range completion, cancels the opposite side after a fill, and closes at the session end.

```csharp
using TradingHub.Strategies;

[Strategy("Opening range breakout", Description = "Measure the early session range, enter on a stop breakout, and flatten at session close.")]
public class RupturaRangoApertura : Strategy
{
    [Parameter("Session open hour")] public int HoraApertura { get; set; } = 9;
    [Parameter("Session open minute")] public int MinutoApertura { get; set; } = 30;
    [Parameter("Range minutes"), Optimize(15, 60, 15)] public int MinutosRango { get; set; } = 30;
    [Parameter("Close hour")] public int HoraCierre { get; set; } = 16;
    [Parameter("Breakout buffer (ticks)"), Optimize(0, 8, 2)] public int Margen { get; set; } = 2;

    private DateOnly _dia;
    private DateTime _inicioRango;
    private decimal _techo, _piso;
    private bool _hayRango, _ordenesPuestas;
    private StrategyOrder? _compra, _venta;

    // without StopLossTicks, trailing stop begins at 60 ticks and follows price
    public RupturaRangoApertura() => TrailingStopTicks = 60;

    protected override void OnBar(Bar bar)
    {
        // convert the bar opening from UTC to the configured session time zone
        DateTime abrio = bar.Time + (LocalTime - Time);
        var apertura = new TimeSpan(HoraApertura, MinutoApertura, 0);
        var finRango = apertura + new TimeSpan(0, MinutosRango, 0);
        var cierre = new TimeSpan(HoraCierre, 0, 0);
        var ahora = LocalTime.TimeOfDay;

        var dia = DateOnly.FromDateTime(abrio);
        if (dia != _dia) // new day: clear the previous session state
        {
            CancelAll();
            if (!IsFlat) ClosePosition("End of day");
            (_dia, _techo, _piso, _hayRango, _ordenesPuestas) = (dia, decimal.MinValue, decimal.MaxValue, false, false);
        }

        // 1) accumulate the opening range from eligible bars
        if (abrio.TimeOfDay >= apertura && abrio.TimeOfDay < finRango)
        {
            if (!_hayRango) _inicioRango = bar.Time;
            _hayRango = true;
            _techo = Math.Max(_techo, bar.High);
            _piso = Math.Min(_piso, bar.Low);
            DrawBox($"rango-{dia:yyyyMMdd}", _inicioRango, _techo, bar.CloseTime, _piso, "blue", 15, "range");
        }

        // 2) after the range, submit one stop order on each side once per day
        if (_hayRango && !_ordenesPuestas && ahora >= finRango && ahora < cierre && IsFlat)
        {
            _compra = BuyStop(_techo + Ticks(Margen), name: "Breakout up");
            _venta = SellStop(_piso - Ticks(Margen), name: "Breakout down");
            _ordenesPuestas = true;
            Log($"Rango del día: {_piso} a {_techo}");
        }

        bool niveles = _ordenesPuestas && ahora < cierre; // plot range levels during the trading session
        Plot("Range high", niveles ? _techo : (decimal?)null, "green");
        Plot("Range low", niveles ? _piso : (decimal?)null, "red");

        // 3) at the close, cancel orders and flatten
        if (ahora >= cierre) { CancelAll(); if (!IsFlat) ClosePosition("End of day"); }
    }

    protected override void OnOrderFilled(StrategyOrder order)
    {
        if (order != _compra && order != _venta) return;
        var otra = order == _compra ? _venta : _compra; // one side filled; cancel the opposite order
        if (otra is { IsActive: true }) Cancel(otra);
        if (order.Side == OrderSide.Buy) DrawArrowUp($"in-{order.Id}", order.FillTime, order.FillPrice, "green", "breakout");
        else DrawArrowDown($"in-{order.Id}", order.FillTime, order.FillPrice, "red", "breakout");
    }
}
```

The conversion `bar.Time + (LocalTime - Time)` maps the bar's UTC opening time to session-local time. Drawing ranges from this bar-open time avoids basing the range on when the next bar's first trade arrives.

## Strategy editor

Under **Backtesting → Code**, choose or create a strategy using available templates, then **Save**, **Compile**, or **Compile and test**. The editor lists compilation diagnostics linked to source lines. **API reference** shows available methods. Favorites are managed through the strategy selector.

## Claude Code assistance

In the Claude panel, describe the desired rules (for example, opening-range breakout with ATR stop). Claude can create/edit the C# source, compile it, and offer to test it. Laboratory mode can also run backtests, optimize parameters, validate walk-forward, and generate a report. See [Claude integration](asistente-claude.md).

**Do not deploy unvalidated example strategies to live trading.**
