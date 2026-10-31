<div align="center">

# Trading Hub

**A free-to-use, source-available desktop trading platform for macOS and Windows.**

Footprint charts, DOM trading, professional backtesting with overfitting checks, and AI-assisted C# strategies—all running locally, with market data and credentials kept on your computer.

Original project created by [**@ralphtradesz**](https://x.com/ralphtradesz). This is an independent English-language fork maintained at [dhamnaik-lab/trading-hub](https://github.com/dhamnaik-lab/trading-hub).

[![License: MIT + Commons Clause](https://img.shields.io/badge/license-MIT%20%2B%20Commons%20Clause-amber.svg)](LICENSE)
![macOS Apple Silicon](https://img.shields.io/badge/macOS-Apple%20Silicon-black.svg)
![Windows 10 and 11](https://img.shields.io/badge/Windows-10%20%7C%2011-0078D6.svg)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4.svg)
![Avalonia](https://img.shields.io/badge/UI-Avalonia-8B44AC.svg)

**English UI evidence:** [View the latest screenshot verification runs](https://github.com/dhamnaik-lab/trading-hub/actions/workflows/english-ui-snapshots.yml). Screenshots are generated from isolated test data.

</div>

> **Fork status:** The Markdown documentation is available in English, while application UI localization, report text and screenshot replacement remain in progress. A fully English, runtime-verified application release has **not** been published or confirmed. The original project's [Windows installers](https://github.com/ralphtradesz/trading-hub/releases) are **upstream builds**, not builds of this English fork.

## Overview

Trading Hub is a desktop platform for charting, trading, and strategy research. Its application, downloaded market data, strategies, backtests, and broker secrets are designed to stay on your own computer. It does not require a Trading Hub account or operate a proprietary backend.

Supported market/data integrations in the source include CME futures (Rithmic, Tradovate, Databento historical data), crypto (Binance and Bybit), forex/CFDs (OANDA, cTrader, CSV), and US equities (Alpaca). Capabilities vary by integration; some connections supply data only and do **not** support live order placement.

Claude Code integration uses your own signed-in Claude Code CLI session, not a Trading Hub API key. The application can assist with writing, compiling, and testing C# strategies, and includes tools for strategy reporting and portfolio-risk analysis.

## Features

### Charts and market data

- Time-, tick-, volume-, and range-based bars, with candlestick, OHLC, line, and Heikin Ashi views.
- Configurable footprint charts with bid/ask, delta, volume, POC, and per-bar profile columns.
- Depth of market (DOM), time and sales, chart drawings, templates, and multiple chart layouts.
- Built-in indicators including moving averages, VWAP with bands, Bollinger Bands, RSI, MACD, volume, delta, cumulative delta, session volume profile, and large trades.
- Custom C# indicators, including compatibility helpers for publicly licensed ATAS indicator code.
- Replay of previously recorded trading sessions.

### Trading and accounts

- Chart Trader and DOM order entry: market, limit, stop, bracket, and OCO orders.
- Simulated accounts and supported real accounts, with account trade history and statistics.
- Optional confirmation before real-account orders, alongside configurable risk controls.
- **Risk warning:** Trading can result in financial losses. Always verify the account, instrument, quantity, and order type before using a real account.

### Storage and historical downloads

- Locally recorded tick-by-tick data, including aggressor side when available.
- Public Binance and Bybit data, supported connection history, and CSV imports from sources such as Dukascopy, HistData, MetaTrader, and TradingView.
- Databento historical CME futures data (usage charges apply; the app presents a cost estimate before downloading).
- Backtests can select recorded Rithmic data, Databento data, or a combination where supported.

### Strategies and backtesting

- C# strategy editor powered by Roslyn, with compile diagnostics and entry/exit API helpers.
- Historical tests based on recorded ticks, including transaction costs, slippage, and modeled limit-order fills.
- Bayesian parameter optimization (TPE), walk-forward validation, Monte Carlo analysis, and market-regime breakdowns.
- Overfitting and robustness tools including PBO/CSCV, CPCV with embargo, and deflated Sharpe.
- Timeframe optimization and strategy favorites.

### Live strategy management

- Run supported strategies from the backtester or a chart, with position sizing, daily-loss limits, and maximum drawdown controls.
- Portfolio monitoring and a Claude-assisted risk agent, with human approval required for changes affecting real accounts.
- Optional resumption of strategies on application launch; review these settings carefully for real-money accounts.

### Claude Code integration

- Strategy assistant for creating and correcting C# code.
- Local MCP laboratory tools for backtesting, optimization, walk-forward analysis, and professional reports.

## English UI screenshot evidence

The seven original images in `docs/img/` are **historical upstream screenshots** and may contain Spanish. They are **not evidence** that the current fork has been translated or runtime-tested. For this reason, they are no longer embedded in this README.

For current visual evidence, use the [English UI snapshot workflow](https://github.com/dhamnaik-lab/trading-hub/actions/workflows/english-ui-snapshots.yml) and the [footprint/DOM verification workflow](https://github.com/dhamnaik-lab/trading-hub/actions/workflows/english-localization-phase6-ui.yml). Choose a successful run for the relevant source commit and download its image artifact. These headless screenshots use isolated, synthetic data and do not prove broker connectivity or safe order execution. GitHub Actions artifacts expire according to their retention settings.

See the [English release verification checklist](docs/english-release-verification.md) for the exact steps needed before publishing a verified Windows installer or replacing historical screenshot assets.

## Installation

### Build this English fork

Clone **this fork**, not the original repository:

```bash
git clone https://github.com/dhamnaik-lab/trading-hub.git
cd trading-hub
```

**Windows 10/11 (x64 or ARM64):** Install Git, the [.NET 10 SDK](https://dotnet.microsoft.com/download), and [NSIS](https://nsis.sourceforge.io). In PowerShell, run:

```powershell
.\build-windows.ps1
```

The build script builds self-contained x64 and ARM64 Windows installers into `dist`. An installer produced from this fork must be built from the updated source; installing an upstream release will **not** include the fork's English changes.

**macOS (Apple Silicon, macOS 12+):** Use the repository's build script:

```bash
./build-app.sh --install
```

The script builds and installs `Trading Hub.app`. It can install the required .NET SDK locally. See [installation instructions](docs/instalacion.md) for further details; portions of that guide may still be in Spanish.

### Original upstream releases

The original author's [published Windows installers](https://github.com/ralphtradesz/trading-hub/releases) are available separately. They are not generated from this fork, so do not expect them to contain the translations made here.

## Data, secrets, and updates

On Windows, application data is stored under `%APPDATA%\Trading Hub`, user strategies/indicators under `Documents\Trading Hub`, and secrets are handled through Windows Credential Manager. On macOS, application data is stored under `~/Library/Application Support/Trading Hub`, user code under `~/Documents/Trading Hub`, and credentials use macOS Keychain.

Updating the installed application should preserve the user-data directories, but always back up valuable user files and confirm compatibility before upgrading. Never add API keys, broker passwords, or account tokens to the repository.

## Additional indicators

The repository contains built-in indicators and tools for adding custom C# indicators. To obtain ATAS indicator sources separately, subject to their individual licenses:

```bash
git clone https://github.com/AtasPlatform/Indicators.git third_party/ATAS-Indicators
python3 tools/port_atas.py
./build-app.sh --install
```

Alternatively, place a compatible indicator `.cs` file in the user indicators directory. See the [indicator guide](docs/indicadores.md). Do not redistribute third-party code without checking its license.

## Documentation

| Topic | Guide |
|---|---|
| Documentation index | [docs/README.md](docs/README.md) |
| Installation | [docs/instalacion.md](docs/instalacion.md) |
| Getting started | [docs/primeros-pasos.md](docs/primeros-pasos.md) |
| Connections | [docs/conexiones.md](docs/conexiones.md) |
| Charts | [docs/graficos.md](docs/graficos.md) |
| Trading | [docs/operar.md](docs/operar.md) |
| Data | [docs/datos.md](docs/datos.md) |
| C# strategies | [docs/estrategias.md](docs/estrategias.md) |
| Backtesting | [docs/backtesting.md](docs/backtesting.md) |
| Live trading | [docs/en-vivo.md](docs/en-vivo.md) |
| Claude | [docs/asistente-claude.md](docs/asistente-claude.md) |
| Indicators | [docs/indicadores.md](docs/indicadores.md) |
| Settings | [docs/ajustes.md](docs/ajustes.md) |
| Privacy | [docs/privacidad.md](docs/privacidad.md) |
| Development | [docs/desarrollo.md](docs/desarrollo.md) |
| Windows porting notes | [PORTAR_A_WINDOWS.md](PORTAR_A_WINDOWS.md) |

The documentation filenames are intentionally unchanged for backward-compatible links. All 19 Markdown guides have been translated, but their technical instructions still require cross-platform build and runtime verification.

## Acknowledgments and support

Original author: [@ralphtradesz](https://x.com/ralphtradesz). The upstream project includes donation details in its [README](https://github.com/ralphtradesz/trading-hub#readme). They support the original author, not the maintainer of this fork.

## Disclaimer

Market trading carries a risk of losing money. Trading Hub is a software tool, not investment advice. Historical backtest results cannot guarantee future returns. Test with a simulated account before considering live orders. Software is provided **as is**, without warranties.

## License

See [LICENSE](LICENSE). The repository contains code offered under **MIT with Commons Clause**, which restricts commercial selling of the software or services substantially based on it. This is **source-available**, not an OSI-approved open-source license. Preserve all copyright, author, and license notices when modifying or distributing it.

---

<div align="center">

Originally created by [@ralphtradesz](https://x.com/ralphtradesz). English fork: [dhamnaik-lab/trading-hub](https://github.com/dhamnaik-lab/trading-hub).

</div>
