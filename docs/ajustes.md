[← Back to documentation](README.md)

# Settings

**Settings** (`Ctrl+,` on Windows or `⌘,` on macOS) contains the general Trading Hub preferences. Most changes save immediately. Provider-specific settings are in [Connections](conexiones.md), chart preferences in [Charts](graficos.md), order templates in [Trading](operar.md), and portfolio risk-agent settings in [Live strategies](en-vivo.md).

## General

| Setting | Default | Meaning |
|---|---|---|
| Appearance | Dark | Use **Dark** or **Light** colors across the application. The sun/moon icon in the top bar provides the same switch. Per-chart color settings are preserved. |
| Chart time zone | This computer's time | Controls the chart time axis, time and sales, and session labels. The list includes Buenos Aires, New York, Chicago, London, Madrid, UTC, and available system zones. Check strategy-time handling separately from display labels. |
| Alert sounds | Enabled | Enables price alerts, large-trade notifications, and indicator sounds. |

## Trading

| Setting | Default | Meaning |
|---|---|---|
| Confirm live orders | Enabled | Manual real-account orders placed through Chart Trader, DOM, or hotkeys request confirmation. This works together with each connection's setting; both confirmation controls must be disabled for a manual real order to bypass confirmation. Live strategy orders have their own controls. |
| Simulated account | — | Displays the initial simulator balance, which can be changed from **Accounts**. Simulated fills model order book queues and actual trade tape where available. |

**Safety:** A confirmation dialog is not a substitute for verifying the selected account, contract, side, size, stop, and exchange status.

## Live strategies

| Setting | Default | Meaning |
|---|---|---|
| Keep running in the system tray / menu bar when the window closes | Enabled | Closing the main window can leave Trading Hub and its live strategies running in the Windows system tray or macOS menu bar. Use **Quit Trading Hub** or the OS-specific exit shortcut to stop the app completely. |
| Automatically resume when Trading Hub opens | Enabled | Strategies that were running at shutdown can restart shortly after startup using their saved parameters, size, and limits. Previously paused strategies remain paused. Disable to resume manually in **Live**. |
| Include live accounts | Disabled | When disabled, real-money strategies wait for explicit manual resume after startup. Enabling this option permits automatic restart; it depends on the general auto-resume setting. **Leave disabled unless the operational risk is understood.** |
| Automatically select mini or micro contracts | Enabled | For CME futures, a fractional mini exposure (for example NQ × 0.5) may be represented with the equivalent number of micros (5 MNQ). Whole mini quantities continue using minis. Open positions remain on the original contract until flat. |

## Prevent sleep

| Setting | Platform | Meaning |
|---|---|---|
| Keep this computer awake | Windows/macOS | Prevents idle sleep while live strategies or Claude agents require processing. The display may turn off. |
| Keep running with the lid closed | macOS | Can enable an administrator-approved mechanism to keep the Mac running with the lid closed while Trading Hub is active. Default is disabled. |
| Only when plugged in | macOS | When enabled, closed-lid operation is restricted to external power. |
| With the lid closed / Power options | Windows | Windows determines lid-close behavior using its own Power settings. Trading Hub cannot independently override Windows' lid-close configuration. |

**Heat risk:** A laptop running with its lid closed may have reduced ventilation. Never leave a running laptop enclosed in a bag.

On macOS, the keep-awake mechanism uses a power assertion; optional lid-closed mode adjusts the OS sleep policy temporarily and requests authorization. Review that setting before using unattended live trading.

## Keyboard shortcuts

Shortcuts apply to the active chart in **Trading** and detached chart windows. Text inputs and OS shortcut conventions can take precedence.

| Group | Actions |
|---|---|
| Trading | Buy/sell market, buy limit at bid, sell limit at ask, close/reverse position, cancel all orders, close partial position, move stop to breakeven, apply stop/target templates, and cycle quantity presets. **Trading shortcuts are unassigned by default for safety.** |
| Chart | Select common periods (1m, 5m, 15m, 1h, 4h, daily), chart type, auto-scale, return to latest price, horizontal line, and cursor tool. |
| Windows and layout | One/four-chart layouts, next/previous tabs, detach chart, DOM, Chart Trader, and side panel. |

Click a shortcut control and press the desired key combination. **Esc** cancels; **Backspace** clears the assignment. If the combination belongs to another action, the app transfers it and reports the change. **Restore defaults** resets bindings.

Account confirmations remain governed by the real-order configuration even when a trading shortcut is assigned.

## Data

| Setting | Description |
|---|---|
| Record live data | Enabled by default. Stores received trades for fast chart reopening, replay, and backtesting. |
| Data folder | Opens the local application data directory. |
| Custom indicators (C#) | Opens the user indicator source directory. |
| Strategies (C#) | Opens the user strategy source directory. |

For path details see [Installation](instalacion.md#where-data-is-stored), [Data](datos.md), and [Privacy](privacidad.md).

## About

The bottom of Settings shows the Trading Hub version, .NET details, and a link to the original author, [@ralphtradesz](https://x.com/ralphtradesz).

## Where preferences are stored

Preferences are saved in the local data directory's `settings.json`. The file also holds hotkeys, window dimensions, favorite instruments and periods, panel/DOM configuration, order templates, default drawing styles, chart and indicator templates, and UI window positions. If configuration is damaged, the app may fall back to defaults.

Do not manually edit or replace settings while the application is running. Back up customized settings before making changes.
