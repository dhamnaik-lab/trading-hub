using System.Globalization;
using System.Text;
using System.Text.Json;
using TradingHub.Strategies;

namespace TradingHub.App.Services;

/// <summary>
/// Agente de riesgo de un portafolio en vivo: Claude (Opus, esfuerzo extra) mira cómo viene cada estrategia —resultado,
/// racha reciente, drawdown contra su historia, volatilidad del resultado diario y correlación con las demás— y reparte
/// el riesgo: tamaño de cada una y, si hace falta, su pérdida máxima del día y su drawdown máximo, dentro de los topes que
/// le pusiste. Por defecto deja una propuesta y vos aprobás cada cambio; en modo automático (solo si ninguna opera una
/// cuenta real) los aplica solo. Las bloqueadas (las que gestiona otro) no las toca.
/// </summary>
internal sealed class LiveRiskAgent
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private readonly AppServices _svc;
    private readonly LiveCenter _center;
    private readonly Dictionary<string, string> _activity = new();
    private readonly Dictionary<string, CancellationTokenSource> _running = new();

    public LiveRiskAgent(AppServices svc, LiveCenter center)
    {
        _svc = svc;
        _center = center;
    }

    /// <summary>Sube con cada cambio (para refrescar la vista).</summary>
    public int Version { get; private set; }

    public bool IsRunning(string portfolioId) => _running.ContainsKey(portfolioId);

    public bool AnyRunning => _running.Count > 0;

    /// <summary>Qué está haciendo con ese portafolio (o null).</summary>
    public string? Activity(string portfolioId) => _activity.GetValueOrDefault(portfolioId);

    public void Cancel(string portfolioId)
    {
        if (_running.TryGetValue(portfolioId, out var cts)) cts.Cancel();
    }

    private void Touch() => Version++;

    /// <summary>Revisa qué portafolios toca revisar (si hubo operaciones nuevas o pasó mucho tiempo).</summary>
    public void Tick()
    {
        if (!LiveStore.LoadPortfolios().Any(d => d.Agent.Enabled)) return;
        var entries = _center.Entries();
        foreach (var p in _center.Portfolios(entries))
        {
            var a = p.Def.Agent;
            if (!a.Enabled || IsRunning(p.Def.Id)) continue;
            if (!p.Members.Any(m => m.Running && !m.Locked)) continue;
            if (RunsToday(a) >= a.MaxRunsPerDay) continue;
            int trades = p.Members.Sum(m => m.Ledger.Trades.Count);
            var now = DateTime.UtcNow;
            bool due = a.LastRunUtc is not { } last
                ? trades >= a.MinNewTrades
                : (now - last).TotalMinutes >= Math.Max(10, a.EveryMinutes) &&
                  (trades - a.TradesAtLastRun >= a.MinNewTrades || (now - last).TotalHours >= Math.Max(1, a.ForceEveryHours));
            if (due) _ = RunAsync(p.Def.Id, manual: false);
        }
    }

    public static int RunsToday(LiveRiskAgentSettings a) => a.RunsUtc.Count(t => t.ToLocalTime().Date == DateTime.Now.Date);

    /// <summary>Corre el agente sobre un portafolio. Devuelve null si salió bien, o el motivo.</summary>
    public async Task<string?> RunAsync(string portfolioId, bool manual)
    {
        if (IsRunning(portfolioId)) return "This portfolio is already under review.";
        var p = _center.Portfolios().FirstOrDefault(x => x.Def.Id == portfolioId);
        if (p == null) return "Portfolio not found.";
        var a = p.Def.Agent;
        var adjustable = p.Members.Where(m => !m.Locked && (m.Running || m.WaitingResume)).ToList();
        if (adjustable.Count == 0) return "No adjustable strategies are available.";
        if (!manual && RunsToday(a) >= a.MaxRunsPerDay) return "Today's review limit has been reached.";
        if (ClaudeCodeBridge.FindCli() == null) return $"Claude Code was not found on this {Os.Device}.";

        var cts = new CancellationTokenSource();
        _running[portfolioId] = cts;
        _activity[portfolioId] = "Reviewing portfolio risk…";
        Touch();
        try
        {
            var prompt = BuildPrompt(p, adjustable);
            var dir = Path.Combine(LiveStore.Root, "agente-riesgo", p.Def.Id);
            Directory.CreateDirectory(dir);
            var agent = new ClaudeAgent();
            var req = new ClaudeAgentRequest
            {
                Role = "live-risk",
                Prompt = prompt,
                SystemPrompt = SystemPrompt,
                WorkDir = dir,
                Tools = Array.Empty<string>(),
                LabTools = Array.Empty<string>(),
                PermissionMode = "dontAsk",
                Model = string.IsNullOrWhiteSpace(a.Model) ? ClaudeCodeBridge.Model : a.Model,
                Effort = string.IsNullOrWhiteSpace(a.Effort) ? ClaudeCodeBridge.Effort : a.Effort,
                MaxUsd = a.MaxUsdPerRun,
                Timeout = TimeSpan.FromMinutes(12),
            };
            var res = await agent.RunAsync(req, cts.Token);
            a.LastRunUtc = DateTime.UtcNow;
            a.TradesAtLastRun = p.Members.Sum(m => m.Ledger.Trades.Count);
            a.RunsUtc.Add(DateTime.UtcNow);
            a.RunsUtc.RemoveAll(t => t < DateTime.UtcNow.AddDays(-7));
            a.CostTotal += res.CostUsd;
            if (!res.Ok || res.Json is not { } json)
            {
                a.LastError = res.Error ?? "Risk agent did not return the required JSON block.";
                if ((res.Error ?? "").Contains("or newer is required", StringComparison.OrdinalIgnoreCase))
                    a.LastError = "Your Claude Code version is too old for the selected model. Update Claude Code and retry.";
                Save(p.Def);
                _center.Notify($"⚠ Risk agent for '{p.Def.Name}': {a.LastError}");
                return a.LastError;
            }

            a.LastError = null;
            var proposal = Parse(json, p, adjustable);
            a.LastSummary = proposal.Summary;
            Save(p.Def);
            if (proposal.Items.Count == 0)
            {
                if (manual) _center.Notify($"Risk agent for '{p.Def.Name}': no changes. {Short(proposal.Summary, 160)}");
                return null;
            }

            bool auto = a.Mode == "auto" && !p.HasReal && adjustable.All(m => !m.Info.IsReal);
            if (auto)
            {
                foreach (var item in proposal.Items)
                {
                    var e = adjustable.First(m => m.Key == item.Key);
                    _center.ApplyRisk(e, item.ProposedSize, item.ProposedMaxDailyLoss ?? item.CurrentMaxDailyLoss, item.ProposedMaxDrawdown ?? item.CurrentMaxDrawdown,
                        e.Runner?.CloseOnLimit ?? e.Info.CloseOnLimit);
                    item.Status = "approved";
                }

                proposal.Source = "agent (automatic)";
                LiveStore.AddProposal(proposal);
                _center.Notify($"Risk agent adjusted {proposal.Items.Count} strategies in '{p.Def.Name}' (automatic mode, simulated accounts only).");
            }
            else
            {
                LiveStore.AddProposal(proposal);
                _center.Notify($"Risk agent proposed {proposal.Items.Count} changes in '{p.Def.Name}': review them under Live → Portfolios.");
            }

            return null;
        }
        catch (Exception ex)
        {
            CrashLog.Write(ex);
            a.LastError = ex.Message;
            Save(p.Def);
            return ex.Message;
        }
        finally
        {
            _running.Remove(portfolioId);
            _activity.Remove(portfolioId);
            cts.Dispose();
            Touch();
        }
    }

    private static void Save(LivePortfolioDef def)
    {
        var defs = LiveStore.LoadPortfolios();
        var stored = defs.FirstOrDefault(d => d.Id == def.Id);
        if (stored == null) defs.Add(def);
        else stored.Agent = def.Agent;
        LiveStore.SavePortfolios(defs);
    }

    private static string Short(string s, int max) => s.Length <= max ? s : s[..(max - 1)] + "…";

    private static string N(decimal v) => v.ToString("0.##", Inv);

    // ===================================================================== pedido

    internal const string SystemPrompt = """
        You are the risk manager for a portfolio of live algorithmic strategies in Trading Hub.
        There is no human participating in the agent conversation. Explain your reasoning professionally and clearly.

        YOUR TASK: allocate risk between strategies while respecting the portfolio's risk budget.
        Available controls are each strategy's SIZE (order multiplier: 1 = original programmed quantity,
        0 = block new entries) and LIMITS (maximum daily loss and maximum drawdown, in account currency;
        hitting these limits stops entries or stops the strategy according to its configured behavior).

        Risk-management rules:
        - Sample size: fewer than about 20 trades are noisy; prefer small adjustments near 1×.
          Do not chase short winning or losing streaks.
        - Volatility: strategies with more volatile daily P&L should generally receive less size
          (approximately inverse-volatility weights), unless evidence for their edge is strong.
        - Correlation: daily P&L correlation above 0.6 suggests overlapping risk; reduce combined exposure.
        - Deterioration: current drawdown near or above the historical worst, weak recent profit factor,
          and sufficient data can justify a substantial reduction or 0 to pause new entries. Explain why.
        - Risk budget: total strategy daily-loss limits should not exceed the portfolio daily-risk
          budget; reduce total risk when portfolio drawdown approaches its maximum.
        - Gradual changes: normally do not change size by more than ±50% per review, except when
          halting a substantially deteriorated strategy.
        - Whole contracts: size multiplies the strategy's original contract quantity. Futures with
          micro contracts and smart-contract execution enabled can express fractional mini exposure
          using micros (NQ→MNQ, ES→MES, 1 mini = 10 micros; 0.5 NQ = 5 MNQ).
          Without a micro, any nonzero size smaller than one whole contract still submits one
          contract; if a further reduction is necessary, use 0 and explain.
        - Always respect supplied size bounds and trader instructions.
        - Never invent figures. Use only the supplied evidence. Propose no changes when none are justified.

        Respond in clear English, with 3–8 lines of analysis and exactly one final JSON code block:
        ```json
        {"summary":"1–3 sentences about portfolio condition and recommended actions",
         "allocations":[{"key":"exact strategy key","size":1.25,"max_daily_loss":250,"max_drawdown":1500,
                         "reason":"evidence-based explanation using provided figures"}]}
        ```
        Include in allocations only strategies whose size or limits should change.
        max_daily_loss and max_drawdown are optional: supply them only when changing them
        (0 means unlimited). Output valid JSON with no comments.
        [[ROL:live-risk]]
        """;

    private string BuildPrompt(LivePortfolioView p, List<LiveEntry> adjustable)
    {
        var a = p.Def.Agent;
        decimal capital = p.Def.Capital > 0 ? p.Def.Capital : 25_000m;
        var analysis = LiveCenter.Analyze(p);
        var sb = new StringBuilder();
        sb.AppendLine($"Portfolio '{p.Def.Name}' · reference capital {N(capital)} · time {DateTime.Now:yyyy-MM-dd HH:mm} (local).");
        sb.AppendLine($"Budget: total daily risk {N(a.DailyRiskPercent)}% of capital ({N(capital * a.DailyRiskPercent / 100)}); maximum portfolio drawdown {N(a.MaxDrawdownPercent)}% ({N(capital * a.MaxDrawdownPercent / 100)}).");
        sb.AppendLine($"Size bounds per strategy: {N(a.MinSize)} to {N(a.MaxSize)} (or 0 to block new entries).");
        if (!string.IsNullOrWhiteSpace(a.Instructions)) sb.AppendLine($"Trader instructions: {a.Instructions.Trim()}");
        if (a.LastSummary.Length > 0 && a.LastRunUtc is { } last) sb.AppendLine($"Previous review ({last.ToLocalTime():dd/MM HH:mm}): {a.LastSummary}");
        sb.AppendLine();
        var m = analysis.Metrics;
        sb.AppendLine($"PORTFOLIO (closed trades): net {N(m.NetProfit)} · {m.TotalTrades} trades · PF {N(Math.Min(m.ProfitFactor, 99))} · max drawdown {N(m.MaxDrawdown)} · " +
                      $"Sharpe {m.SharpeRatio.ToString("0.00", Inv)} · current open P&L {N(p.Members.Sum(x => x.OpenPnL))}");
        sb.AppendLine();
        sb.AppendLine("STRATEGIES:");
        for (int i = 0; i < p.Members.Count; i++)
        {
            var e = p.Members[i];
            var stats = LiveStore.StatsOf(e.Info, e.Ledger, capital);
            var sm = stats.Metrics;
            bool canChange = adjustable.Contains(e);
            var r = e.Runner;
            decimal size = r?.SizeMultiplier ?? e.Info.Size;
            decimal dl = r?.MaxDailyLoss ?? e.Info.MaxDailyLoss, dd = r?.MaxDrawdown ?? e.Info.MaxDrawdown;
            double days = e.Ledger.Fills.Count > 0 ? (DateTime.UtcNow - e.Ledger.Fills[0].Time).TotalDays : 0;
            sb.AppendLine($"- key: {e.Key}");
            sb.AppendLine($"  {e.Info.Strategy} · {e.Info.Symbol} {e.Info.Period} · account {e.Info.Account}{(e.Info.IsReal ? " (REAL)" : " (simulated)")} · {e.StateLabel}" +
                          $"{(canChange ? "" : $" · NOT adjustable ({(e.Locked ? "locked" : "stopped")})")}");
            sb.AppendLine($"  size ×{N(size)} · max daily loss {(dl > 0 ? N(dl) : "unlimited")} · max drawdown {(dd > 0 ? N(dd) : "unlimited")} · position {N(e.Ledger.Position)} · open P&L {N(e.OpenPnL)}");
            sb.AppendLine("  " + ContractLine(e, size));
            sb.AppendLine($"  {sm.TotalTrades} trades over {days:0.#} days · net {N(stats.Net)} · PF {N(Math.Min(sm.ProfitFactor, 99))} · win rate {sm.WinRate:0}% · " +
                          $"avg/trade {N(sm.TotalTrades > 0 ? e.Ledger.Trades.Average(t => t.NetPnl) : 0)} · average win {N(sm.AverageWin)} · average loss {N(sm.AverageLoss)}");
            sb.AppendLine($"  current drawdown {N(stats.CurrentDrawdown)} · worst drawdown {N(sm.MaxDrawdown)} · last {stats.RecentTrades} trades: PF {N(Math.Min(stats.RecentProfitFactor, 99))}, media {N(stats.RecentExpectancy)}" +
                          $"{(r != null ? $" · today's P&L {N(r.TodayPnL)}" : "")}");
            if (i < analysis.DailyPnl.Count)
            {
                var daily = analysis.DailyPnl[i];
                var vals = daily.Where(v => v != 0).ToList();
                double vol = vals.Count > 1 ? Math.Sqrt(vals.Sum(v => Math.Pow((double)v - vals.Average(x => (double)x), 2)) / (vals.Count - 1)) : 0;
                sb.AppendLine($"  daily P&L (last {Math.Min(20, daily.Length)} days): {string.Join(" ", daily.TakeLast(20).Select(N))} · daily volatility {vol.ToString("0.##", Inv)}");
            }
        }

        if (p.Members.Count > 1 && analysis.Correlation.GetLength(0) == p.Members.Count)
        {
            sb.AppendLine();
            sb.AppendLine("DAILY P&L CORRELATION (Pearson):");
            for (int i = 0; i < p.Members.Count; i++)
                for (int j = i + 1; j < p.Members.Count; j++)
                    sb.AppendLine($"  {p.Members[i].Info.Strategy}·{p.Members[i].Info.Symbol} ↔ {p.Members[j].Info.Strategy}·{p.Members[j].Info.Symbol}: {analysis.Correlation[i, j].ToString("0.00", Inv)}");
        }

        sb.AppendLine();
        sb.AppendLine("Allocate risk and return the JSON block (only changed strategies).");
        return sb.ToString();
    }

    /// <summary>En qué contratos quedan las órdenes con el tamaño actual y con los de al lado (para que razone en contratos enteros).</summary>
    private string ContractLine(LiveEntry e, decimal size)
    {
        decimal qty = LiveCenter.QuantityOf(e.Info.Parameters);
        string Of(decimal s)
        {
            if (s <= 0) return "0 (no new entries)";
            var (symbol, ratio) = _center.PreferredContract(e.Info.FeedId, e.Info.Symbol, qty, s);
            decimal units = Math.Floor(qty * s * ratio + 0.0000001m);
            return $"×{N(s)} = {N(Math.Max(1, units))} {symbol}{(units < 1 ? " (minimum)" : "")}";
        }

        string micro = MarketInfo.IsFutures(e.Info.FeedId) && MarketInfo.MicroOf(e.Info.Symbol) is { } m
            ? _svc.Settings.SmartContracts ? $"micro available ({m}): fractional mini exposure uses micros" : $"micro available ({m}), but smart-contract execution is disabled"
            : "no micro: a nonzero quantity below 1 still submits 1 contract";
        var steps = new[] { size * 0.5m, size, size * 1.5m }.Select(v => Math.Round(v, 2)).Distinct();
        return $"contracts: strategy requests {N(qty)} {e.Info.Symbol} per order · {micro} · {string.Join(" · ", steps.Select(Of))}";
    }

    /// <summary>Lee lo que devolvió el agente y lo deja como propuesta (dentro de los topes; solo las que puede tocar).</summary>
    internal static SizingProposal Parse(JsonElement json, LivePortfolioView p, List<LiveEntry> adjustable)
    {
        var a = p.Def.Agent;
        var proposal = new SizingProposal
        {
            Source = "agente",
            PortfolioId = p.Def.Id,
            PortfolioName = p.Def.Name,
            Summary = json.TryGetProperty("summary", out var s) && s.ValueKind == JsonValueKind.String ? Short(s.GetString() ?? "", 600) : "",
        };
        if (!json.TryGetProperty("allocations", out var allocs) || allocs.ValueKind != JsonValueKind.Array) return proposal;
        foreach (var el in allocs.EnumerateArray())
        {
            if (el.ValueKind != JsonValueKind.Object) continue;
            string key = el.TryGetProperty("key", out var k) && k.ValueKind == JsonValueKind.String ? k.GetString() ?? "" : "";
            var e = adjustable.FirstOrDefault(x => x.Key == key);
            if (e == null) continue;
            decimal current = e.Runner?.SizeMultiplier ?? e.Info.Size;
            decimal curDl = e.Runner?.MaxDailyLoss ?? e.Info.MaxDailyLoss, curDd = e.Runner?.MaxDrawdown ?? e.Info.MaxDrawdown;
            decimal size = Num(el, "size") ?? current;
            size = size <= 0 ? 0 : Math.Clamp(Math.Round(size, 2), a.MinSize, a.MaxSize);
            decimal? dl = Num(el, "max_daily_loss") is { } d1 ? Math.Max(0, Math.Round(d1, 2)) : null;
            decimal? dd = Num(el, "max_drawdown") is { } d2 ? Math.Max(0, Math.Round(d2, 2)) : null;
            if (dl == curDl) dl = null;
            if (dd == curDd) dd = null;
            if (size == current && dl == null && dd == null) continue;
            proposal.Items.Add(new SizingItem
            {
                Key = key,
                Strategy = $"{e.Info.Strategy} · {e.Info.Symbol}",
                CurrentSize = current,
                ProposedSize = size,
                CurrentMaxDailyLoss = curDl,
                CurrentMaxDrawdown = curDd,
                ProposedMaxDailyLoss = dl,
                ProposedMaxDrawdown = dd,
                Reason = el.TryGetProperty("reason", out var r) && r.ValueKind == JsonValueKind.String ? Short(r.GetString() ?? "", 500) : "",
            });
        }

        return proposal;
    }

    private static decimal? Num(JsonElement el, string name)
    {
        if (!el.TryGetProperty(name, out var v)) return null;
        if (v.ValueKind == JsonValueKind.Number && v.TryGetDecimal(out var d)) return d;
        if (v.ValueKind == JsonValueKind.String && decimal.TryParse(v.GetString(), NumberStyles.Float, Inv, out var ds)) return ds;
        return null;
    }
}
