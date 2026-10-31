[← Back to documentation](README.md)

# Live strategies

**Live** (Ctrl+7 on Windows or ⌘7 on macOS) manages real-time strategies started from Live, Backtesting, or a chart. Tabs include **Strategies**, **Market**, **Portfolios**, **Trades**, and **Proposals**, with **New live strategy** and **Stop all** controls.

Start with the **Simulated account** and a fully verified strategy. The platform can submit real-money orders when a compatible broker account has been explicitly authorized.

## Simulated, practice, and real accounts

- The built-in simulated account models fills using available order book and trade information.
- Binance testnet, Tradovate demo, and Rithmic practice systems do not risk real money.
- Real accounts are available only when the connection explicitly permits real orders, and are clearly marked **REAL**. See [Connections](conexiones.md#real-and-practice-accounts).
- Starting or manually resuming a strategy on a real account requires confirmation. The individual strategy orders then run without further confirmation; review position sizing and risk limits first.
- **Automatic startup resumption does not prompt.** Auto-resume for real accounts is therefore disabled by default and should usually remain so.

## Start a strategy

### From Backtesting or Live

1. From **Backtesting → Backtest**, choose **Run live…** from the live deployment card, or select **New live strategy** in Live.
2. Under **What and where**, choose strategy, instrument, account, and bar period (`M1`, `M2`, `M3`, `M5`, `M10`, `M15`, `M30`, `H1`, `H4`, or the prior backtest period).
3. Review parameters inherited from the most recently configured backtest.
4. Choose an **entry context filter** for market regime (range, mixed, rising/falling trend) and volatility (low, normal, high relative to recent history).
5. Set **Risk**: multiplier, maximum daily loss and maximum drawdown (amounts, with 0 meaning no limit), and whether hitting a threshold should also close the position.
6. Choose an existing portfolio or let the strategy join the automatic one.
7. Confirm the account and choose **Start live**.

The session time zone and start hour follow the strategy's backtest configuration and determine the daily-loss session boundary. Before placing orders, the strategy warms bars and indicators from existing history without trading. Market-context history is prepared from recent hourly candles. If there are too few bars, indicators may need additional time to become ready.

The same strategy cannot run twice simultaneously on the same instrument and account. For futures, the launch confirmation identifies the contract and quantity to be sent.

### From a chart

The chart's robot icon lists strategies from Backtesting. **Add to active chart** is another entry route.

A newly attached chart strategy begins in **Visual only** mode, showing hypothetical historical entry/exit markers and running against its own visual simulation account without changing your actual simulated account. Its chart-history visualization does not include the same full commissions and slippage model as a normal backtest.

From the chart's strategy settings you can choose **Run live with an account**, select an account, and apply. Once enabled, it appears in **Live** with origin **Chart**. Changing the chart's instrument pauses an executing chart strategy; removing it stops that strategy, leaving its current position unchanged unless you separately close it.

See [Strategies on a chart](graficos.md#strategies-on-a-chart).

## Sizing and intelligent futures contracts

- **Size multiplier:** 1 uses programmed quantity; 0 prevents new position entries. The strategy retains its own logical position units while translating order deltas into scaled physical orders.
- Quantity is rounded down to instrument step size. The implementation can send at least one whole unit when a position is intended, so do not assume a small multiplier means zero orders.
- **Intelligent contract selection** in Settings (enabled by default) can replace fractional mini futures exposure with equivalent micros: for example 0.5 × 1 NQ becomes 5 MNQ. Other examples include ES/MES, YM/MYM, RTY/M2K, GC/MGC and CL/MCL.
- An open position stays on the current contract until flat. The live strategy then transitions to the appropriate contract as specified by the saved settings.

The strategy table, detail view, system tray/menu bar, and chart legend display the contract and quantity being routed.

## Risk limits and pausing

| Limit | When reached |
|---|---|
| Maximum daily loss | Closed-session P&L plus open P&L reaches the negative threshold: cancel pending entries and block new entries until the next session |
| Maximum drawdown | Equity declines by the configured amount from its peak, including open P&L: stop the strategy |

**Close position when a limit is reached** is enabled by default. The application issues visible warnings when a limit is triggered.

**Pause** is different from Stop: it prevents new entries and cancels pending entry orders while continuing to manage an already open position until it closes.

**Stop all** is the emergency action: after confirmation, it stops all running strategies, cancels their orders, and requests that their positions be closed at market. Verify fills and remaining exposure at the broker, particularly during outages.

## Strategies list and details

Summary tiles include total result, today, active strategy count, closed trades, and pending changes. The strategy table shows account, state (including paused by market context), scaled contract size, current position, net and daily P&L, profit factor, drawdown, and limits.

Selecting a strategy opens its detail panel:

- **Actions:** Pause/Resume, Close position, Stop, Stop and close, Restart, View on chart, View trades, Reset history (flat only), Delete.
- **Risk:** Adjust size and thresholds. Changes to size affect subsequent orders.
- **Market context:** Update entry filters immediately without restarting, showing whether current conditions permit entries.
- **Parameters:** Apply and restart with data warm-up; open positions continue to be managed.
- **Portfolio / Activity:** Assigned group and strategy logs.

Each strategy maintains its own execution ledger even if multiple strategies and manual orders share the broker account. On restart, if its ledger and broker position agree, it attempts to resume position management and stops/targets without duplicating existing protective orders. If they disagree, the implementation reconciles its ledger. With an external broker it may wait up to 45 seconds for position reports.

**Always reconcile the application's position against the broker after restart or connection loss.**

## Trades

The live trades page lists closed strategy trades: strategy, market, side, timestamps/prices, duration, quantity, points, P&L, MAE, MFE, and capture of favorable excursion. You can filter, sort and jump to the relevant chart.

The strategy ledger can record fills at the contemporaneous last market price rather than the broker-reported execution price, and the original documentation states that this ledger does **not** deduct broker commissions. Use broker statements for realized-account reconciliation.

## Portfolios

- The **Automatic** portfolio groups running strategies not assigned to a user-created portfolio.
- **New portfolio** creates a named group with reference capital (default 25,000) and selected strategies. A strategy belongs to at most one user-created portfolio.
- Portfolio analytics include realized/open P&L, trade count, profit factor, drawdown, Sharpe (with sufficient data), inter-strategy daily-P&L correlations, a combined equity curve, and a correlation matrix.
- Actions include add strategy, start all (extra confirmation for real accounts), stop all (positions remain as they are under that action), edit, and delete (strategies continue under the automatic group).

## Claude risk agent

Each portfolio can configure a Claude risk agent that reviews performance, recent losses, drawdown, daily-result volatility, and strategy correlation. It **proposes** strategy size multipliers and, where appropriate, daily-loss and drawdown limits.

| Option | Documented initial setting |
|---|---|
| Mode | **Propose** by default; **Automatic** applies only where no strategy in the portfolio trades a real account |
| Portfolio daily risk / drawdown budget | 2% / 10% of reference capital |
| Minimum / maximum strategy size | ×0.25 / ×3; size 0 blocks new entries |
| Review cadence | 60 minutes (minimum 10), subject to trade-count and periodic review conditions |
| Model and effort | Configurable in the UI; available models may differ from those named by older source documentation |
| Instructions | Free text for desired limits, priorities and restrictions |

**Review now** runs a manual review. The original implementation describes a maximum of eight automatic reviews daily, with a time and cost budget where supported by the Claude CLI. The agent receives portfolio metrics and no direct order-routing tools. Suggestions appear in **Proposals** for approval.

See [Claude integration](asistente-claude.md).

## Proposals

Suggested position-size changes wait for explicit approval individually or through **Approve all**. Real-account proposals require an additional confirmation and apply to subsequent orders. Proposal sources include:

- **Rule-based suggestions:** With enough trade history (at least ten trades), a recent profit-factor and drawdown heuristic may suggest pausing, halving risk, or increasing size by up to 25% within an upper cap.
- **Ask Claude:** Opens the research assistant to analyze the running portfolio.
- **Portfolio risk agent:** Periodic or manual risk proposals.

A proposal is not independent proof that the suggested size is safe or profitable.

## Market conditions

The Market page summarizes each active instrument's regime (range/mixed/uptrend/downtrend), recent trend efficiency, volatility rank versus the previous month, and the last 48 hours of conditions. It also shows which strategies permit entries under their configured filters. The model is recalculated periodically from completed hourly candles.

## Closing and reopening

- **Tray/menu-bar behavior:** With the default setting, closing the main window leaves the application and live strategies running in the Windows system tray or macOS menu bar. Use **Quit Trading Hub** to exit fully. See [Getting started](primeros-pasos.md#system-tray-and-menu-bar).
- **Auto-resume:** Running strategies can resume a few seconds after reopening with their saved size and risk settings; paused strategies remain paused. Real-money strategies are held for manual resume unless explicitly authorized otherwise.
- **Sleep prevention:** The application can prevent idle sleep while strategies or agents are running, and optional Mac closed-lid operation is configurable under [Settings](ajustes.md).

**Before leaving a trading system unattended, verify broker session stability, order-state reconciliation, hard limits, and external emergency controls.**
