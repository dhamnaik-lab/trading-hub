# English fork release verification

This checklist applies **only** to [dhamnaik-lab/trading-hub](https://github.com/dhamnaik-lab/trading-hub), not the original upstream project's binaries.

## Release states (do not conflate them)

| State | Evidence required |
|---|---|
| Source checks passed | The exact PR head passed Windows .NET 10 build/test and Markdown checks. |
| Installer built | Both self-contained `win-x64` and `win-arm64` builds packaged by NSIS, with a published SHA-256 manifest. |
| Installer smoke-tested | Silent **x64** install/uninstall on an isolated GitHub-hosted Windows runner; installed EXE SHA-256 equals the published x64 EXE; no test user data was removed. |
| GUI runtime reviewed | Manually install and launch the fork build on a Windows desktop, inspect English screens, exercise simulated orders, and verify upgrade behavior. |
| ARM64 runtime verified | Run the ARM64 installer and app on a **real Windows ARM64 device**. Cross-compilation alone is not sufficient. |
| Release published | A separately approved GitHub Release on **this fork**, with installers, hashes, version and limitations. A CI artifact is not a release. |

## Automated checks

PR checks:

1. `English localization verification`: .NET 10 Release solution build, full unit/integration tests with fake providers, Markdown anchors and links.
2. `English UI snapshot verification`: isolated headless Avalonia scenes.
3. `English localization phase 6 UI verification`: footprint and DOM metadata/screenshot checks.
4. `English localization residual-string inventory`: read-only Spanish-literal candidate report. Matches can be intentional data keys, not necessarily translation defects.
5. `English fork Windows installer verification`: builds both Windows installers from the exact checked-out fork commit, exercises x64 install/uninstall and tests user-data preservation, then uploads binaries and SHA-256 JSON to Actions artifacts.
6. `English fork documentation screenshots`: captures synthetic English UI images using the headless runner and reviews them as binary changes to the Phase 9 PR. No live broker accounts or credentials are used.

### Verify downloaded installer hashes on Windows

After obtaining the **fork** installers and `installer-sha256.json` from the **same GitHub Actions run**, compare the manifest's SHA-256 to:

```powershell
Get-FileHash -Algorithm SHA256 .\TradingHub-Setup-x64.exe
Get-FileHash -Algorithm SHA256 .\TradingHub-Setup-arm64.exe
```

Only install the architecture appropriate for the machine. An archived upstream installer does not include the translation changes made here.

## Manual Windows x64 acceptance (simulated accounts only)

Run these checks on a **disposable Windows profile or VM**, or back up your Trading Hub data first. Never enter real brokerage credentials just for localization testing.

- [ ] Install x64 package from this fork; verify Windows Installed apps entry, Start shortcut and executable startup.
- [ ] Confirm main navigation and secondary dialogs are legible in English at 100% and 125% display scaling; review light/dark themes.
- [ ] Check Accounts, Data, Connections and Trading, including broker safety warnings. Use demo/simulated data; no real broker sign-in or live orders.
- [ ] From the simulated account, test market, limit, stop, target/bracket, cancel, flatten and order-risk warning text. Confirm quantities and state transitions.
- [ ] Review Backtesting, optimization, walk-forward, report headings and chart/indicator settings. Confirm no crashes or lost layout state.
- [ ] Check the Claude/risk-agent UI with **no active real accounts**; any generated JSON must retain its `summary`, `allocations`, `key`, `size`, `max_daily_loss`, `max_drawdown`, `reason` keys.
- [ ] Upgrade a disposable prior installation using a Phase 9 installer and verify existing workspace, layouts and simulated data are preserved.
- [ ] Uninstall and confirm user documents/settings are still present. **Do not delete folders containing user strategies or broker credentials.**

## Limits and non-goals

- CI snapshots are headless renderings, **not proof of an interactively usable installed GUI**.
- An installer build and silent install/uninstall test do not certify real brokerage order routing, data entitlements, security or trading performance.
- No production orders, exchange connections, paid data requests or credentials should be used in these checks.
- Native ARM64 runtime, macOS build/notarization, real-account permissions, runtime AI reliability and a fully English language-coverage audit remain separate acceptance gates.
- Names of strategy classes, stored footprint presets, JSON/CSV fields and other compatibility identifiers must not be renamed merely to translate display text.

## Release handoff

Record the fork commit SHA, exact successful CI run URL, both installer SHA-256 values, x64 manual review and ARM64 runtime review (if applicable). Only after those gates and explicit approval should an English fork GitHub Release be created. Until then the CI installer ZIP is **candidate evidence only**, not a recommended production download.
