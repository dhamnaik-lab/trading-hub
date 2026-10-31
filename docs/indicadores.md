[← Back to documentation](README.md)

# Indicators

Add chart indicators from the **Indicators** dialog (toolbar button or **Chart → Indicators**, Ctrl/⌘+I). The left-hand **Library** supports searching by name or category. Select an indicator and choose **Add to chart**, or double-click it. Select an existing indicator to edit its parameters.

**Templates** lets you save parameter presets, apply a saved preset, or set defaults for newly added indicators. See [Charts](graficos.md#indicators).

Strategy indicators such as `Sma`, `Ema`, and `Rsi` use a separate, simplified API. See [C# strategies](estrategias.md).

## Built-in indicators

| Indicator | Category | Main options |
|---|---|---|
| Moving Average | Trend | Period, simple/exponential/weighted calculation, source |
| Bollinger Bands | Trend | Period and standard-deviation multiplier |
| VWAP with deviation bands | Volume / order flow | Reset by session, week or month; up to three volume-weighted deviation bands |
| Volume | Volume / order flow | Color by price candle or delta |
| Delta | Volume / order flow | Histogram or candles, including delta extrema |
| Cumulative Delta (CVD) | Volume / order flow | Candle display, optional session reset |
| Big Trades | Volume / order flow | Bubbles for large aggressive orders; aggregate fills spanning multiple price levels; fixed/adaptive size threshold, colors and placement |
| Session Volume Profile | Volume / order flow | Session, weekly or visible-history profiles; volume/delta/Bid–Ask; value area, POC, VAH and VAL |
| RSI | Oscillator | Period |
| MACD | Oscillator | Fast, slow, and signal periods |

These live in `TradingHub.Indicators.Native` and are grouped by category in the library.

### Technical indicator collection

`TradingHub.Indicators.Technical` supports the public source collection from ATAS (around 250 indicators in the upstream description). That third-party source is **not redistributed** here because its licensing does not grant unrestricted redistribution. This repository contains compatibility infrastructure for optionally building compatible indicators locally.

See [Adding ATAS indicators](#adding-atas-indicators).

## Custom indicators

Trading Hub compiles custom C# indicators from your user indicators folder.

- macOS: `~/Documents/Trading Hub/Indicadores`.
- Windows: The corresponding `Documents\Trading Hub\Indicadores` folder.
- Subfolders are accepted; `bin` and `obj` are ignored.
- The empty folder is initialized with an example, `EjemploDeltaPorcentual.cs` (the filename is retained for compatibility).
- Saving, adding, deleting or renaming files triggers compilation after approximately 0.6 seconds without edits. **Code → Recompile indicators** can force it.
- All custom files are compiled together. If an error occurs, the last successful compiled indicators remain available and the status bar reports the failure.
- Custom indicators appear under **My indicators** in the library and are marked with a star. Remove/re-add an already-open indicator to load a changed compiled version.

### The editor

Open **Code → Indicator editor** (Shift+Ctrl/⌘+E) or the code icon in the Indicators dialog. **New** creates a C# class from a template. **Save and compile** saves and compiles; diagnostics below the editor navigate to the affected line. The folder button opens Explorer or Finder.

### Example: average candle range

The following example retains C# symbol names but uses English displayed labels:

```csharp
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using TradingHub.Indicators;

[DisplayName("Average Range")]
[Category("My indicators")]
public class AverageRangeIndicator : Indicator
{
    private readonly ValueDataSeries _series = new("Range", "Average range") { Color = Color.Orange, Width = 2 };
    private int _period = 14;

    [Display(Name = "Period", GroupName = "Settings")]
    [Range(1, 1000)]
    public int Period
    {
        get => _period;
        set { _period = value; RecalculateValues(); }
    }

    public AverageRangeIndicator()
    {
        Panel = IndicatorDataProvider.NewPanel;
        DataSeries[0] = _series;
    }

    protected override void OnCalculate(int bar, decimal value)
    {
        if (bar < Period - 1) return;
        decimal sum = 0;
        for (int i = 0; i < Period; i++)
        {
            var candle = GetCandle(bar - i);
            sum += candle.High - candle.Low;
        }
        _series[bar] = sum / Period;
    }
}
```

### Indicator API overview

- A public class extending `Indicator` with a parameterless constructor, `[DisplayName]`, and `[Category]`. Useful namespaces include `System.ComponentModel`, `System.ComponentModel.DataAnnotations`, `System.Drawing`, and `TradingHub.Indicators`.
- **Data series:** `ValueDataSeries` for lines and histograms (`VisualType = VisualMode.Histogram`), `RangeDataSeries` for bands (`RangeValue { Upper, Lower }`), and `PaintbarsDataSeries` for candle coloring.
- **Series registration:** The main data series is `DataSeries[0]`; add others with `DataSeries.Add(…)`.
- **Panel placement:** Default overlay is the price panel; `Panel = IndicatorDataProvider.NewPanel` requests a dedicated indicator pane.
- **Parameters:** Public properties decorated with `[Display]` and optionally `[Range]`. Call `RecalculateValues()` after changing inputs.
- **Calculation:** `OnCalculate(int bar, decimal value)` receives bar indexes from zero and the closing price. `GetCandle(bar)` exposes Open, High, Low, Close, Volume, Ask, Bid, Delta, MaxDelta, MinDelta and Time. Handle warm-up indexes carefully.
- **Optional drawing:** Enable custom drawing and subscribe to `DrawingLayouts.Final`; override `OnRender(RenderContext context, DrawingLayouts layout)`. Use `ChartInfo.GetXByBar`, `ChartInfo.GetYByPrice`, `context.DrawLine`, `FillRectangle`, `DrawString`, and the visible-bar range.

Claude Code can help write indicator files using the assistant in Backtesting. See [Claude integration](asistente-claude.md).

## Additional indicator DLLs

Compiled assemblies containing classes extending `TradingHub.Indicators.Indicator` can be placed in the application's data directory under `Indicators` or in an `Indicators` folder alongside the executable. They are loaded at startup. Assemblies that fail to load are reported without preventing other indicators from loading.

## Adding ATAS indicators

ATAS publishes indicator sources at [AtasPlatform/Indicators](https://github.com/AtasPlatform/Indicators). Observe the original authors' licenses and redistribution restrictions.

To add the collection locally on a supported development machine:

```bash
git clone https://github.com/AtasPlatform/Indicators.git third_party/ATAS-Indicators
python3 tools/port_atas.py
./build-app.sh --install
```

The script copies selected `Technical` sources into `src/TradingHub.Indicators.Technical/Atas`, remaps namespaces for Trading Hub, and applies compatibility fixes. Indicators relying on unavailable ATAS internals are excluded. `TRADINGHUB_PORT_EXCLUDE=File1.cs,File2.cs` can exclude other files.

To refresh the third-party sources:

```bash
git -C third_party/ATAS-Indicators pull
python3 tools/port_atas.py
./build-app.sh --install
```

The `third_party/` and generated local `Atas/` folders are excluded from Git to avoid redistributing third-party code.

Alternatively, copy an individual compatible C# file into the user indicator directory. Some ATAS indicator classes depend on other indicators (for example CCI/Momentum on SMA), so include their dependencies.

On Windows, use the appropriate local .NET build workflow rather than the macOS `build-app.sh` install command.

## ATAS API compatibility

Trading Hub exposes familiar indicator methods and types such as `Indicator`, `OnCalculate`, `DataSeries`, `OnRender`, `GetCandle`, and `ChartInfo`.

At compilation time it maps several namespaces:

| ATAS source namespace | Trading Hub target |
|---|---|
| `ATAS.Indicators`, `ATAS.Indicators.Drawing`, `ATAS.Indicators.Technical` | `TradingHub.Indicators`, `TradingHub.Indicators.Drawing`, `TradingHub.Indicators.Technical` |
| `ATAS.DataFeedsCore` | `TradingHub.Trading` |
| `OFT.Rendering` and rendering subnamespaces | `TradingHub.Rendering` and related equivalents |
| `OFT.Attributes`, `OFT.Attributes.Editors`, `OFT.Localization` | `TradingHub.Attributes`, `TradingHub.Attributes.Editors`, `TradingHub.Localization` |
| `Utils.Common` and related logging/collection subnamespaces | `TradingHub.Common` equivalents |

Additional compatibility types include `CrossColor`, `CrossColors`, `CrossPen`, `CrossPens`, `CrossSolidBrush`, `CrossKey`, and `CrossKeyEventArgs`. The source symbols `TRADINGHUB` and `CROSS_PLATFORM` can be used with conditional compilation. Not every ATAS API is supported: compile diagnostics identify unavailable features.
