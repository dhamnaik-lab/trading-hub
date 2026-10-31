[← Back to documentation](README.md)

# Development

Trading Hub is a .NET 10 / C# desktop application using Avalonia (the original documentation describes 11.3) and SkiaSharp (2.88) for chart rendering. The original code contains Spanish comments and hardcoded user-facing strings; this English fork is being localized incrementally.

## Solution structure

The `TradingHub.slnx` solution contains ten projects:

| Project | Responsibility |
|---|---|
| `src/TradingHub.Core` | Market data types, ticks, bars, CSV imports, history providers, replay, broker feeds and accounts, simulator, protective orders and fills |
| `src/TradingHub.Sdk` | ATAS-style indicator API: indicators, series, drawing, attributes, localization |
| `src/TradingHub.Charting` | Skia-based chart engine, candles, footprint, DOM, time and sales, drawings and themes |
| `src/TradingHub.Scripting` | Roslyn compilation of strategy and indicator C#, plus namespace adaptations |
| `src/TradingHub.Strategies` | Strategy API, backtests, optimization, walk-forward, robustness validation, portfolios and live strategy execution |
| `src/TradingHub.Indicators.Native` | Built-in indicators |
| `src/TradingHub.Indicators.Technical` | Locally ported technical indicators and compatibility editor types |
| `src/TradingHub.App` | Avalonia UI, windows, controls, services, connection/credential management, Claude integration, MCP lab and reports |
| `tests/TradingHub.Tests` | xUnit tests |
| `tools/TradingHub.UiSnapshot` | Headless UI scenarios and screenshot verification |

Project versions and framework settings are in `Directory.Build.props`. The displayed product name comes from `TradingHub.Common.Product.Name`. Application data locations are managed by `Product` and `AppPaths`.

## Build and run

```bash
dotnet build TradingHub.slnx
dotnet test tests/TradingHub.Tests
```

On macOS, add the user-local SDK to `PATH` if required, then:

```bash
export PATH="$HOME/.dotnet:$PATH"
./build-app.sh
./build-app.sh --install
```

The Mac script produces a self-contained `osx-arm64` app, signs it (ad hoc without an available certificate), and can install it to Applications. A local, ignored `build.local.env` can specify `BUNDLE_ID` and `SIGN_IDENTITY`. `TH_OUT` and `TH_STAGE` change build/output folders.

On Windows, use:

```powershell
.\build-windows.ps1
```

This requires the .NET 10 SDK and NSIS for installer creation. For user guidance see [Installation](instalacion.md).

### Isolated development execution

On macOS, an isolated example:

```bash
TRADINGHUB_DATA_DIR=/tmp/th-dev TRADINGHUB_KEYCHAIN_FILE=/tmp/th-dev/keychain-test.json dotnet run --project src/TradingHub.App
```

On Windows, use a dedicated temporary location and PowerShell environment variables:

```powershell
$env:TRADINGHUB_DATA_DIR = Join-Path $env:TEMP 'tradinghub-dev-data'
$env:TRADINGHUB_KEYCHAIN_FILE = Join-Path $env:TEMP 'tradinghub-test-secrets.json'
dotnet run --project src/TradingHub.App
```

Ensure the temporary directory contains **no production account data**. Never run a development process concurrently on a real user's saved data; multiple writers can corrupt the store.

## Tests

Run all unit and integration tests:

```bash
dotnet test tests/TradingHub.Tests
```

Focused examples:

```bash
dotnet test tests/TradingHub.Tests --filter FullyQualifiedName~SmartWalkForward
```

Test projects cover strategy fills, protective OCO orders, market-context filters, walk-forward/robustness, live strategy runner, accounts, data integrity, history, CSV imports, drawing/legend, brokers and API signatures. Tests use temporary files and fake provider servers such as `FakeRithmicServer`, `FakeHttpServer`, and `FakeBrokers`.

## Headless UI snapshots

Run:

```bash
dotnet run --project tools/TradingHub.UiSnapshot -- <output-folder> 4 <scene> [<scene>…] [--open connection:SYMBOL] [--size 1600x950]
```

The first argument is an output folder for PNGs; `4` is the initial wait interval in seconds. Omit scenes to capture the Trading page. Verification scenes report OK/failure and exit with a failure count.

| Scene | Purpose |
|---|---|
| `trading`, `accounts`, `strategies`, `live`, `data`, `connections`, `settings` | Capture main sections |
| `conexiones` | Test provider setup (Tradovate, cTrader, OANDA, Alpaca, Databento) using fake servers |
| `panel` | Chart Trader, DOM, tape and detached panels |
| `footprint` | Footprint ladders and editor |
| `mercado` | Market-condition backtest results |
| `theme` | Light/dark theme and chart appearance |
| `choice`, `picker`, `esquinas` | History button, instrument picker, rounded-corner diagnostics |
| `data+rithmic`, `data+import` | Simulated provider downloads and CSV imports |
| `accounts+demo` | Account screens with simulated/fake broker orders |
| `connections+rithmic`, `connections+amp` | Rithmic/AMP profile server list |
| `trading+...` | Layout, historical ranges, drawings, bracket, toolbar, trading, strategy, DOM floating panels |
| `strategies+...` | Backtest, optimization, walk-forward, code, history, portfolio, reports, Claude and laboratory |

**Scene identifiers remain in their original form for compatibility. Do not translate scene keys merely because they contain Spanish.**

### Snapshot isolation

The snapshot runner uses `TRADINGHUB_DATA_DIR`, `TRADINGHUB_STRATEGIES_DIR`, `TRADINGHUB_INDICATORS_DIR`, and test-only credential storage. It disables real order permissions and uses fake broker connections. A lock prevents concurrent scenarios from sharing a working folder.

**Claude warning:** Scenes `strategies+claude` and `strategies+claudelab` can invoke a real Claude Code request and consume usage. Do not run them unintentionally against a real CLI account.

Helpful diagnostics include `TRADINGHUB_SNAP_INDICATORS`, `TRADINGHUB_SCRIPTS_DEBUG=1`, and `TRADINGHUB_SNAP_DEBUG=1`.

## Environment variables

| Variable | Purpose |
|---|---|
| `TRADINGHUB_DATA_DIR` | Overrides the application data directory |
| `TRADINGHUB_STRATEGIES_DIR`, `TRADINGHUB_INDICATORS_DIR` | Override custom source directories |
| `TRADINGHUB_KEYCHAIN_FILE` | Use a test JSON secret store instead of actual OS secrets |
| `TRADINGHUB_BACKTESTS_DIR`, `TRADINGHUB_LIVE_DIR` | Override saved research/live-state directories |
| `TRADINGHUB_SLEEP_GUARD=off` | Prevents system power-policy changes |
| `TRADINGHUB_CLAUDE_CLI` | Choose a Claude Code executable |
| `TRADINGHUB_MCP_COMMAND` | Executable used by the local laboratory server |
| `TRADINGHUB_OANDA_REST`, `TRADINGHUB_OANDA_STREAM`, `TRADINGHUB_ALPACA_DATA`, `TRADINGHUB_ALPACA_TRADING`, `TRADINGHUB_ALPACA_STREAM`, `TRADINGHUB_TRADOVATE_REST`, `TRADINGHUB_TRADOVATE_WS`, `TRADINGHUB_TRADOVATE_MD`, `TRADINGHUB_CTRADER_URL`, `TRADINGHUB_DATABENTO_URL` | Provider endpoints used for local fake-server tests |

Never use testing overrides with production credentials or accounts.

## Add a market-data provider

1. Implement `IMarketDataFeed` within `src/TradingHub.Core/Feeds/<Provider>/`, including `GetInstrumentsAsync`, `LoadTradesAsync`, and `Subscribe` through `IMarketDataSink`. Add `IKlineHistory`, `IDayDownloader`, `ISessionAwareFeed`, or `ITickerStatsProvider` where applicable.
2. If supported, implement `ITradingAccount` with market/limit/stop order methods, cancellation, modification, position state, and correct `IsReal` behavior.
3. Register the provider in `src/TradingHub.App/Services/Connections.cs`: `ConnectionKind`, `ConnectionProfile`, feed factory, and connection test. Store secrets only with the credential service. Add test-only endpoint override support.
4. Build the setup UI in `ConnectionsView.cs` and, if applicable, supported accounts in `AppServices.AccountsFor` and `AccountFor`.
5. Add fake-server tests for the provider. Never enable real-order permissions in automated checks.

## Add an indicator or laboratory tool

- **Built-in indicator:** Create a parameterless `Indicator` subclass in `TradingHub.Indicators.Native`, with `[DisplayName]` and `[Category]`. The application scans built-in assemblies at startup. Custom user indicators require no product source changes.
- **Laboratory tool:** Define its schema in `LabServer.Tools()`, handle it in `LabServer.CallAsync`, and explicitly allow it in `ClaudeCodeBridge.LabTools`. Keep laboratory operations isolated from real orders.

## Validation from code

`ValidationGate.Evaluate` evaluates a walk-forward result against the rules described in [Backtesting](backtesting.md). See `tests/TradingHub.Tests/RobustnessTests.cs` for examples.

```csharp
var gate = ValidationGate.Evaluate(new GateInput
{
    WalkForward = walkForward,
    Optimization = optimization,
    Trials = 12,
    TrialVariance = 0.01,
    MinTrades = 30,
});
Console.WriteLine($"{gate.StatusText} · Deflated Sharpe {gate.Deflated:P0}");
foreach (var check in gate.Checks)
    Console.WriteLine($"{check.Status} · {check.Name}: {check.Text}");
```

`LiveStrategyRunner.Guard` also accepts an `ILiveOrderGuard` to inspect orders that open or increase exposure. Position-reducing orders remain permitted. Unit tests illustrate usage.

## Rules for contributions

- Never store passwords, tokens, API keys or personal brokerage records in the repository.
- Never operate on production data or connect tests to live-order endpoints. Use isolated directories and fake servers.
- Preserve real-account permissions, order confirmations, sizing, risk controls, and emergency stop behavior. Treat any behavioral change separately from UI translation.
- Keep trading controls from shifting position as values update. The `trading+jitter` scenario checks size/position stability.
- Run tests against simulated accounts before considering changes to provider integrations.
- New user-facing labels, warnings, Markdown guides, and help text in this fork should be in **English**. Do not change serialized keys, API types, test scene identifiers, or previously saved state to translate their names.
