# Trading Hub documentation

Trading Hub is a desktop platform for charting, market data, trading, strategy research, and C# indicators. The original project was created by [@ralphtradesz](https://x.com/ralphtradesz); this documentation is for the [English-language fork](../README.md).

> **Translation status:** This index and the fork's main README are in English. The linked guides are being translated in stages and some still contain Spanish. Their existing filenames are retained so links do not break. For Windows, always check that installation instructions refer to a build of **this fork**, not an upstream release.

## Getting started

- [Installation](instalacion.md) — Supported operating systems, prerequisites, building, updating, and uninstalling.
- [First steps](primeros-pasos.md) — Main window, simulated account, charts, and navigation.
- [Connections](conexiones.md) — Market-data providers, brokers, read-only versus trading access, and credentials.

## Charting and trading

- [Charts](graficos.md) — Bar periods, footprint, DOM, chart drawings, layouts, templates, indicators, and replay.
- [Trading](operar.md) — Chart Trader, order types, brackets, DOM, accounts, and history.
- [Indicators](indicadores.md) — Built-in indicators, custom C#, and compatibility with ATAS APIs.

## Market data

- [Data management](datos.md) — Local recordings, downloads, imports, Databento, and replay.

## Strategies and backtesting

- [C# strategies](estrategias.md) — Strategy API and examples.
- [Backtesting](backtesting.md) — Tick backtests, optimization, walk-forward, PBO/CPCV, and Monte Carlo.
- [Live strategies](en-vivo.md) — Risk limits, portfolios, and risk-agent workflows.
- [Claude integration](asistente-claude.md) — Strategy assistant and local MCP laboratory.

## Settings, safety, and development

- [Settings](ajustes.md) — Appearance, data, hotkeys, live-strategy startup, and power settings.
- [Privacy and security](privacidad.md) — Local data, credentials, network access, and safe testing.
- [Development](desarrollo.md) — Source structure, build steps, tests, and UI snapshots.
- [Windows porting notes](../PORTAR_A_WINDOWS.md) — Historical porting plan and platform-specific implementation details.
- [Back to the main README](../README.md).

## English localization work

The application still has untranslated hardcoded UI strings. Generated reports, the installer, and inherited screenshots may also contain Spanish. If contributing translations, preserve code behavior and identifiers, verify all relative Markdown links after changing headings, and check trade-related warnings with particular care. Do not change broker order routing or risk settings as part of translation alone.
