[← Back to documentation](README.md)

# Backtesting

The **Backtesting** section (Ctrl+2 on Windows, ⌘2 on macOS) contains **Code**, **Backtest**, **Optimization**, **Walk-forward**, **History**, **Portfolio**, and **Reports**. For the C# strategy API see [Strategies](estrategias.md); for real-time execution see [Live strategies](en-vivo.md).

## Selecting historical data

A backtest replays saved trades one by one: live recordings, downloaded files, and imported data from [Data](datos.md). Without saved data, the instrument list is empty.

- **Instrument (saved data)** shows a symbol, source, stored day count, date range, and disk size.
- If a futures instrument has [Databento](datos.md#databento) coverage, the selector can display **Rithmic + Databento**, **Rithmic only**, or **Databento**. In combined mode, Rithmic takes priority and Databento fills minutes that contain no saved Rithmic trades. Tradovate supports the equivalent choices.
- Set **From** and **To**, including the final date in full; presets include 1 week, 1/3/6 months, 1 year, and all history.
- Switching instruments initially selects the latest seven stored days. The default percentage commission becomes 0.04% for crypto sources and zero otherwise.

**Bar period** accepts codes such as `M5`. Quick selections include `M1`, `M5`, `M15`, `H1`, `H4`, `D1`, `Tick500`, and `Range20`. Parsing is case-insensitive. Some English and Spanish aliases remain accepted for compatibility.

| Code | Bar aggregation |
|---|---|
| `S30`, `M5`, `H1`, `D1`, `W1`, `Mo1` | Time (seconds through months) |
| `Tick500` | 500 trades per bar |
| `Volume1000` | 1,000 units of volume |
| `Range20` | 20 ticks of high–low range |
| `Delta300` | Close after delta reaches ±300 |

Renko bars are not supported by the backtesting engine.

## Execution assumptions and costs

| Setting | Initial value | Description |
|---|---|---|
| Starting capital | 10,000 | Base for percentage returns, drawdown and risk-adjusted ratios |
| Commission % per side | 0.04% for crypto, 0 elsewhere | Per-entry and per-exit percentage of notional: quantity × price × point value |
| Fixed commission per unit | 0 | Per contract/unit, per side; additive to percentage commission |
| Slippage | 1 tick | Adverse price adjustment for simulated market and stop orders |
| Limit order fills | Price must trade through the limit | Optional optimistic mode fills on a touch |
| Session time zone / start hour | UTC / 0 | Controls `LocalTime`, `TimeBetween`, session boundaries, VWAP, CVD, and hourly metrics |

Available session zones include UTC, Buenos Aires, New York, Chicago (CME), and London. Parameter settings are saved for each strategy and may carry into chart-based or live execution.

### Order fill rules

| Order | Simulation |
|---|---|
| Market | Next trade after the order, adjusted for slippage; a signal from `OnBar` fills on the next bar's first trade |
| Limit | At the limit price when a trade crosses it, or touches it in optimistic mode; immediately marketable limits cannot fill at a price worse than the limit |
| Stop | When traded price reaches the trigger, filled at the triggering trade adjusted for slippage; gaps can worsen the fill |
| Automatic stop/target | Stop and limit semantics with OCO behavior |

An order never fills on the same trade event that created it. At the backtest's end, remaining open positions are closed at the last price. Profit is in the instrument's currency, using price difference × quantity × point value (NQ has a point value of 20). The engine warms market context from previously saved history when available.

These are simulation assumptions, not a guarantee of exchange/broker fills.

## Results

Each run is saved under **History**, including the exact strategy source used. The summary displays net result, percentage return, profit factor, win rate, maximum drawdown, Sharpe, Sortino, expectancy, and equity curve.

Result tabs include:

- **Trades:** Side, entry/exit timestamps and prices, quantity, net result, MAE, MFE, bars in position, and exit-order name.
- **Metrics:** Commissions, returns, average result, streaks, exposure, long/short breakdown, simulated ticks and bars, and other statistical measures.
- **By market condition:** Results segmented by market regime and volatility.
- **Monte Carlo:** Reordered trade sequences and drawdown scenarios.
- **Log:** Output of the strategy's `Log`, with UTC timestamps, up to 5,000 lines.

| Metric | Definition |
|---|---|
| Profit factor | Gross profit / gross loss, infinite when there are no losing trades |
| Win rate | Percentage of trades with positive net result |
| Expectancy | Win rate × average win + loss rate × average loss |
| Maximum drawdown | Largest peak-to-trough decline, including marked-to-market open positions at bar close; percentage relative to the peak |
| Recovery factor | Net result / maximum drawdown |
| Sharpe / Sortino | Daily mean return divided by return deviation (Sortino uses downside deviation), annualized by √252 or √365 for crypto |

### By market condition

Every trade stores the market context known at entry: range, mixed, uptrend, or downtrend based on recent trend efficiency, plus low/normal/high volatility based on the prior month's rank (approximately bottom 33%, middle third, top third).

The table summarizes trade count, net result, profit factor, win rate, and average return. Low-sample regimes are flagged.

Select a subset of conditions and choose **Rerun in selected conditions**. This runs a **new backtest** with a market-context entry filter evaluated as each bar closes, rather than merely hiding losing historical trades. Exits remain allowed.

**Overfitting warning:** Selecting conditions because they performed well on the same sample is in-sample optimization. Validate on fresh out-of-sample data or a walk-forward process.

### Monte Carlo

**Simulate 1,000 random trade orders** rearranges the trades and reports drawdown percentiles (median, 95th, 99th, worst observed) plus estimated **risk of ruin**, defined here as paths with drawdown of at least 50% of starting capital. The chart plots 40 selected simulated paths.

## History

Filter saved runs by All, Backtests, Optimizations, Walk-forward, or favorites. Each run includes its metrics, equity curve, parameters, configuration, and editable notes.

Actions include favorite, delete, **View code** (the exact historical source), **Report**, and **Use parameters** (copies them back into Backtest and runs it).

History is saved under the application data folder's `Backtests` directory, grouped by strategy.

## Optimization

1. Under **What to test**, choose parameters with a minimum, maximum, and step. Parameters decorated with `[Optimize]` are preselected. The UI warns above 3,000 combinations and refuses more than 20,000.
2. Choose a ranking measure: **Result / drawdown** (default), Net result, Profit factor (capped at 100), Sharpe, Sortino, Recovery factor, Expectancy, or Win rate.
3. Set **Minimum trades** (default 10). Lower-sample combinations do not qualify for the ranking.
4. Run **Optimize**. The engine evaluates a grid of parameter combinations on the selected backtest data, in parallel using all but one available CPU core.

**Timeframe optimization:** You can include bar periods such as `M1`, `M5`, `M15`, `M30`, `H1`, `H4`, and `D1`. Each parameter combination is tested on each selected period, with tick data loaded once and bars reconstructed for the selected period.

The results table displays up to 500 ranked rows. **Use in Backtest** or double-click copies the configuration and tests it. The highest-ranked setting may be a chance winner—use walk-forward validation.

## Walk-forward

Walk-forward optimizes parameters on one **in-sample** window, then evaluates the selected configuration on the next previously unseen **out-of-sample** days, advances the window, and repeats. The primary outcome is the **stitched out-of-sample equity curve**, not the best historical optimization score.

- Initial **In-sample** and **Out-of-sample** lengths: 14 and 7 days.
- **Rolling:** Fixed-width in-sample window advances.
- **Anchored:** In-sample starts at the same first date and grows.
- Uses the optimization parameters, ranking metric, minimum trade count, data, and costs already configured.
- Parameter search modes: automatic (grid up to 300 combinations, Bayesian above that), full grid, or Bayesian (80 combinations per window).
- Results include OOS net profit and trades, OOS profit factor, walk-forward efficiency, positive window ratio, OOS drawdown, overfitting estimates, CPCV paths, and per-window selected parameters.

### Internal methodology

1. **Reuse parameter runs:** Each combination is evaluated across available history with warmed indicators and market context; window statistics are then computed for trades entering those windows. The design aims to avoid using future data to select an earlier parameter choice.
2. **Tick compression:** Consecutive trades at the same price, side, and second may be merged with summed volume for time/range bars when strategies do not implement `OnTick`. The intended OHLC/volume/delta/footprint and fill-price equivalence should be independently validated; `Bar.Trades` will differ.
3. **Bayesian search (TPE):** After stratified initial samples, a Tree-structured Parzen Estimator models parameter values in the best approximately 20% of outcomes versus weaker outcomes, and proposes configurations with high `l(x)/g(x)`.
4. **Plateau selection:** Prefer a stable neighborhood of profitable parameters over an isolated maximum. The score mixes the selected candidate, neighboring grid points, and the search-wide median. Unmeasured/invalid neighbors are penalized.
5. **Walk-forward efficiency:** Aggregate OOS daily gain divided by aggregate IS daily gain; 0.5 or greater is treated by the project as a positive signal. This is a heuristic, not proof of profitability.
6. **PBO / CSCV:** Combinatorially Symmetric Cross-Validation splits history into up to ten groups, selecting IS winners and checking their relative OOS ranks across half-splits (up to 252 combinations). PBO measures how often the IS winner ranks in the worse half OOS. The app also reports how often the winner loses money OOS. Requires at least four combinations and 20 days.
7. **CPCV with embargo:** Combinatorial Purged Cross-Validation splits history into up to eight chronological groups, tests group pairs (28 pairs at eight groups), excludes observations close to test groups using an embargo based on holding time (1–5 days), and constructs several complete OOS paths. The project treats at least 70% profitable paths with positive median as encouraging.

These validation measures are susceptible to data quality issues, leakage, regime shifts, and repeated human tuning. Preserve a genuinely untouched validation period.

## Deflated Sharpe and validation

Testing many strategy variants increases the probability of finding an apparently strong Sharpe by chance. The **Deflated Sharpe Ratio** estimates how significant a result remains after accounting for multiple trials, non-normal returns, and sample size.

Let `SR` be the per-trade OOS Sharpe, `γ3` skewness, `γ4` kurtosis, `n` trade count, `N` trial count, `V` Sharpe variance, and `Φ` the standard normal CDF. The documented calculation is:

```text
PSR(SR*) = Φ( (SR − SR*) · √(n − 1) / √(1 − γ3·SR + (γ4 − 1)/4 · SR²) )
SR*      = √(max(V, 1/n)) · E[max Z_N]
E[max Z_N] ≈ (1 − γ)·Φ⁻¹(1 − 1/N) + γ·Φ⁻¹(1 − 1/(N·e)), γ ≈ 0.5772
```

Project-defined `ValidationGate` rules for OOS results:

| Test | Pass | Caution | Fail |
|---|---|---|---|
| Sample | At least 30 trades | — | Fewer |
| Net result | Positive | — | Zero/negative |
| Profit factor | ≥ 1.2 | ≥ 1.0 | Below 1.0 |
| WF efficiency | ≥ 0.5 | ≥ 0.3 | Below 0.3 |
| Winning windows | ≥ 60% | ≥ 40% | Below 40% |
| Window count | ≥ 4 | Fewer | — |
| Parameter stability | Dispersion/range ≤ 0.35 | Higher | — |
| Plateau | ≥ 50% of best candidate's neighbors profitable | Lower | — |
| Robustness | ≥ 25% of all combinations profitable | Lower | — |
| Overfitting | Less than 20% OOS losses | Less than 50% | ≥ 50% |
| CPCV paths | ≥ 70% profitable | Lower | Nonpositive median |
| Deflated Sharpe | ≥ 90% confidence | ≥ 50% | Below 50% |
| Degradation | Performance persists | Late third underperforms early third, or t ≤ −1.5 | Former winner turns negative with t ≤ −2 |
| Market regimes | Profitable across qualifying groups | Loss in any group | — |

The overall verdict is **Fail** if any hard failure is present, **Caution** if a caution condition is present, otherwise **Pass**. If the only failures reflect insufficient OOS trades, the gate can report **Insufficient sample**, including an estimate of the additional trades required.

Some criteria are exposed in the Walk-forward UI; the full `ValidationGate` is in the strategy library, covered by tests. See [Development](desarrollo.md).

## Portfolio

Combine stored backtests from multiple strategies/instruments. Weighting choices include **Equal**, **Risk parity** (inverse daily P&L volatility), and **Manual**. The portfolio view displays the combined curve, correlation matrix, portfolio metrics, and drawdown diversification.

**Suggest combination** searches recent backtests for a group (up to five strategies) with correlation below approximately 0.5 and favorable Sharpe impact. This is a heuristic: verify out-of-sample compatibility and capital assumptions.

## Reports

History and Portfolio provide **Report** generation. Claude's laboratory can also create research reports and judgments.

Reports are stored in the user strategies folder's `Reportes` subdirectory and include Markdown tables and charts. **Export PDF / HTML** uses the browser's print/save function. See [Claude integration](asistente-claude.md).

**No backtest, statistical gate, or optimization can guarantee future trading profits.**
