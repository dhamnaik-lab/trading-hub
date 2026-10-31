[← Back to documentation](README.md)

# Claude in Trading Hub

Trading Hub integrates with your installed **Claude Code CLI** and its authenticated session. No separate Trading Hub account is required. Claude-based functionality consumes your available Claude Code usage entitlement; without Claude Code, the rest of the platform can still run.

| Location | What Claude does |
|---|---|
| **Backtesting → Code**, assistant panel | Draft, explain, and correct C# strategies and indicators |
| **Laboratory** mode in the same panel | Run backtests, optimization, walk-forward, Monte Carlo, and produce reports |
| **Live → Portfolios** | Review strategy/portfolio risk and propose sizing adjustments |
| **Live → Proposals** | Analyze the running portfolio through the laboratory |

See [Live strategies](en-vivo.md#claude-risk-agent).

## Finding Claude Code

On macOS, the application searches common installation folders such as `/usr/local/bin`, `/opt/homebrew/bin`, `~/.claude/local`, `~/.local/bin`, and `~/.npm-global/bin`, plus your `PATH` and Claude desktop app's CLI files. On Windows, it searches common executable and npm paths as supported by the application.

Set `TRADINGHUB_CLAUDE_CLI` to select a specific CLI binary. For installation and authorization see [Installation](instalacion.md#claude-code-optional).

- The assistant panel displays the detected Claude Code version or an unavailable-CLI message.
- If login is needed, use the panel's **Sign in to Claude Code** action, or launch `claude` and follow its login flow.
- If a particular model or feature requires a newer CLI, update using your provider's current recommended installation method.

## Model, effort, and permissions

The original source documentation referenced Claude Opus 5.5 and an extra-high reasoning effort for the assistant, with configurable models and effort for the risk agent. **Models, labels, entitlements, and CLI flags change over time**; treat these as historical defaults and inspect the running app's current options rather than assuming a model remains available.

The assistant is launched as an external `claude -p` process from a user strategy directory. It is configured with file-reading/editing tools for strategy, indicator, and laboratory capture folders, but without ordinary terminal/web tools. Laboratory mode uses a restricted MCP configuration and enables only the Trading Hub laboratory server.

The portfolio risk agent is more restricted: it receives selected portfolio statistics and returns suggestions without tools to submit orders. Never send API secrets or broker credentials in prompts.

## Strategy assistant

1. Open **Backtesting → Code** and the Claude panel on the right. The panel can be detached into its own window.
2. Enter a request and press **Send** (Ctrl/⌘+Enter). Example requests: generate a 30-minute opening-range breakout, explain a selected strategy, add a session-time filter, or implement an ATR-based stop.
3. During the run, the UI shows file read/write/edit activity; the source editor becomes read-only and reflects updates. **Stop** cancels the active request.
4. When Claude finishes, the edited/new strategy is compiled. If it succeeds, you can **Test in Backtest**; otherwise use **Ask to fix errors** and inspect compiler diagnostics.

The prompt can include the selected strategy source, indicator folder, compilation errors, and the internal indicator/strategy APIs. Expected code is standalone C# without new external dependencies, file/network/thread access, or nondeterministic random behavior. Prefer short explanatory comments in English in this fork.

Conversations are stored under the app's local `Chats` directory. **New conversation** starts a clean session; the history controls can reopen previous sessions.

## Laboratory mode

When **Laboratory** is enabled, Trading Hub exposes its local model-context tools as an MCP server by launching the application with `--mcp`. The server communicates using JSON-RPC on standard input/output. Its configuration is stored as `claude-lab-mcp.json` in the local app data folder and the server name is `tradinghub`.

Supported tools in the documented implementation include:

| Tool | Purpose |
|---|---|
| `list_strategies` | Strategy list, compile state, parameters, optimize ranges, warm-up |
| `compile_strategy` | Compile, return diagnostics, parameter and warm-up details |
| `chart_snapshot` | Run over a range and save a chart snapshot with entries, exits, and drawings |
| `list_indicators`, `compile_indicator` | Inspect and compile custom indicators |
| `list_data` | Inspect saved tick coverage for backtesting |
| `download_data` | Download Binance Futures/Spot and Bybit public daily archives (up to 62 days per request) |
| `run_backtest` | Tick backtesting with modeled commission and slippage, optionally Monte Carlo |
| `run_optimization` | Grid search with a default ceiling of 5,000 combinations; show top/plateau candidates |
| `run_walk_forward` | Walk-forward optimization, plateau selection, PBO and CPCV; can estimate workload |
| `list_runs` | Read saved strategy test runs |
| `analyze_portfolio`, `suggest_portfolio` | Analyze correlation and candidate portfolio combinations |
| `live_portfolio` | Read-only metrics and positions for live strategies |
| `propose_sizing` | Create pending sizing proposals; does not apply them |
| `save_report` | Generate and save structured research reports |

Individual tool operations may run for extended periods, especially optimization. The source's laboratory policy directs the assistant to inspect data, establish a baseline, optimize within sensible ranges, validate walk-forward, review Monte Carlo drawdowns, compare source revisions, and call `save_report` with evidence-backed conclusions. It must not invent trading results.

Documented verdict categories are:

| Verdict | Interpretation |
|---|---|
| **Winner** | Positive out-of-sample results with sufficient trades, robust walk-forward efficiency, acceptable drawdowns and stable windows |
| **Promising** | Positive indications but limited samples or unstable segments |
| **Loser** | Poor OOS performance or gains explainable by overfitting |
| **Rejected** | Cannot compile, does not trade, or lacks a viable evidence basis |

For the original project's *Winner* threshold, the documentation describes WF efficiency ≥ 0.5, OOS profit factor ≥ 1.2, at least 30 OOS trades, and reasonable Monte Carlo 95th-percentile drawdown. These are heuristics, not profit guarantees.

All laboratory test runs are recorded in strategy history. A live risk analysis uses `live_portfolio` and can create proposed adjustments through `propose_sizing`, but proposed live changes wait for review in **Live → Proposals**.

## Reports

`save_report` builds a report with executive summary, strategy source/data assumptions, performance tables, monthly results, trades, drawdowns, parameter sensitivity, walk-forward, Monte Carlo, portfolios, and a glossary. Claude contributes the judgment, analysis, risks, and next actions.

- Reports are stored in the user strategy folder's `Reportes` subdirectory as Markdown files and chart-image folders.
- The **Backtesting → Reports** page displays reports and can export an HTML document for browser printing or saving as PDF.
- Report labels generated by the current application may still be Spanish even when this documentation is English. See translation status in the [main README](../README.md).

## Privacy and safety

Claude prompts, source code, trade logs, portfolio statistics, and tool results that it receives may be processed outside your machine by Anthropic. Consult [Privacy and security](privacidad.md) and the provider's data policies.

Never include passwords, access tokens, or secret API keys in Claude conversations. Laboratory tests must not use live order endpoints; any portfolio change with real-money impact requires deliberate human review.
