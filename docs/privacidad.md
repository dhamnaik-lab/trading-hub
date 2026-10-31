[← Back to documentation](README.md)

# Privacy and security

Trading Hub is designed to run locally: the application, market data, strategy files, historical tests, and saved connection configuration live on your computer. The source code does not describe a proprietary Trading Hub account or built-in usage telemetry; connections to exchanges, brokers, and AI services depend on the features you configure and use.

## What stays on your computer

| Location | Data |
|---|---|
| macOS: `~/Library/Application Support/Trading Hub` | Settings, non-secret connection configuration, workspace, tick data, candles, drawings, templates, fills, backtests, live strategies, and portfolios |
| Windows: `%APPDATA%\Trading Hub` | Corresponding application data on Windows |
| `~/Documents/Trading Hub` (macOS) or `Documents\Trading Hub` (Windows) | Your custom strategy and indicator source files; strategy reports |
| macOS Keychain / Windows Credential Manager | Broker passwords, tokens, and API secrets, managed by the OS credential store |

For a directory-by-directory explanation, see [Installation](instalacion.md#where-data-is-stored).

The application data folder also contains Claude conversations, the laboratory's chart snapshots (up to the most recent 60 as described by the source documentation), and laboratory configuration. Unexpected failures are recorded in a local `crash.log`.

## Connection credentials

- Secret values such as broker passwords, API secrets, access tokens, and Databento keys belong in the OS credential store, **not** source files or GitHub commits.
- `connections.json` stores connection configuration, including non-secret fields such as provider, username, server, public API key identifiers, and selected options.
- Removing a connection through the application is designed to remove its associated secrets.
- Development tools and reports should not read or export stored credentials.

On macOS, the application uses the system Keychain; Windows uses Windows Credential Manager. Review [Connections](conexiones.md#where-credentials-are-stored) for provider-specific behavior. Do not assume an imported or copied data directory also safely transfers credentials between machines.

## Network access

| Activity | Trigger | External service |
|---|---|---|
| Market data | Opening a chart for an enabled connection | The selected exchange, broker, or data provider (for example Binance, Bybit, Rithmic, Tradovate, OANDA, Alpaca, or cTrader) |
| Historical downloads | Downloading days from Data or invoking laboratory data tools | Public exchange archives or the configured provider |
| Historical candles | Fetching missing candle history for charts or market conditions | Exchange or market-data service |
| Real orders | Manually trading or running an authorized live strategy | The broker associated with the selected real account |
| Databento | Testing the connection, requesting a cost estimate, or downloading | Databento using the configured key; paid data may incur charges |
| Claude Code | Using strategy assistance, laboratory analysis, or the risk agent | Anthropic via your signed-in Claude Code CLI |
| Interactive Brokers | Testing local gateway connectivity | TWS or IB Gateway |
| External links | Explicitly opening signup/help links | Your web browser and the target site |

Building the app also requires NuGet package downloads. Source behavior, third-party services, and telemetry policies can change; audit the relevant provider and current code if privacy guarantees are critical.

## Claude and external AI

When you invoke Claude features, prompts and data supplied through the CLI can be processed by Anthropic under your account and applicable policies. This can include strategy/indicator source, backtest metrics, fills, summary market data, screenshots, and live portfolio state when requested.

The original design constrains the strategy assistant to relevant working directories and avoids exposing credential stores through its tools. The risk agent receives portfolio figures and proposes changes rather than autonomously changing real-account risk settings. Treat the generated output as a proposal that requires review.

See [Claude integration](asistente-claude.md) for details. Never paste broker credentials into prompts.

## Real-money account safety

- The application starts with a **simulated account**. Live orders require configuring a supported broker and allowing real order placement for that connection.
- Manual real-account orders can require confirmation, depending on [Trading](operar.md#safety) and connection settings.
- Starting or manually resuming a real-money strategy has a separate confirmation flow. Automatic resumption does not prompt on startup; therefore auto-resumption of **real-account strategies is disabled by default**.
- Live strategies expose position sizing, daily-loss and drawdown controls. The **Stop all** action stops running strategies.
- The Claude risk agent should not automatically apply size changes to a portfolio that includes real accounts.

These measures reduce risk; they do not guarantee that a strategy is safe or profitable. Confirm the actual account, side, contract, quantity, and safety controls before connecting to live trading.

## Operating-system permissions

On macOS, enabling **Keep running with the lid closed** may request an administrator password so the app can change sleep behavior temporarily. Starting Claude login may open Terminal. A local macOS build is signed according to the process described under [Code signing](instalacion.md#code-signing).

On Windows, lid behavior is configured through Windows Power settings. The installer is intended to run per user without requiring administrator privileges.

## Development and testing isolation

Automated tests and UI snapshots should use isolated temporary directories such as `TRADINGHUB_DATA_DIR`, `TRADINGHUB_STRATEGIES_DIR`, and `TRADINGHUB_INDICATORS_DIR`, with test-only secret storage where supported. Avoid reading, overwriting, or testing with actual user credentials and market data. Do not send live broker orders during automated testing.

See [Development](desarrollo.md).

## Remove application data

1. Quit Trading Hub completely. Closing the main window may leave the application running in the Windows system tray or macOS menu bar.
2. Back up any strategy code, research results, trade history, or account records you want to keep.
3. Uninstall the application through Windows Settings or remove the macOS app bundle.
4. Remove the applicable application data directories only if you intend to delete all local app data.
5. Review and remove Trading Hub entries from Keychain Access (macOS) or Credential Manager (Windows) if desired.

Deletion may be irreversible. See [Uninstalling](instalacion.md#uninstalling).
