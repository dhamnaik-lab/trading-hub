[← Documentation index](README.md) · [Installation](instalacion.md)

# English fork — release verification

This checklist applies to **dhamnaik-lab/trading-hub**. An upstream installer from
`ralphtradesz/trading-hub` does not contain this fork's localization changes.
A successful GitHub Actions job produces a **candidate**, not an approved release.

## 1. Identify and validate the source

- [ ] Confirm the target PR is merged, and record the exact full `main` commit SHA.
- [ ] Run the Windows .NET 10 Release build and full unit/integration tests.
- [ ] Check Markdown links, the residual Spanish-string inventory and the scope of remaining non-English text.
- [ ] Verify that persistent strategy identifiers, serialized footprint preset names, SDK generated keys and CSV schemas are unchanged.
- [ ] Recheck high-risk warning text (broker account selection, bracket entry/exit, stop-loss and maximum daily loss).
- [ ] Check the licensing and attribution before distributing any binary.

## 2. Verify both Windows installer candidates

The [Windows installer verification workflow](https://github.com/dhamnaik-lab/trading-hub/actions/workflows/english-windows-installer-validation.yml)
uses a disposable Windows runner. It installs .NET 10 SDK and NSIS, builds
self-contained **x64 and ARM64** payloads, compiles NSIS installers and uploads
a ZIP artifact containing both installers and `phase9-windows-sha256.json`.
It runs an **x64 silent install/uninstall** smoke test and verifies that user-data
and strategy-folder sentinels are not removed.

- [ ] Confirm the workflow passed on the exact candidate commit; ignore older green runs.
- [ ] Download the candidate ZIP and independently check its SHA-256 values against the manifest.
- [ ] Confirm the `source_commit` and expected architecture in the manifest.
- [ ] Treat the packages as **unsigned CI candidates** unless independently signed and verified.
- [ ] Verify installer/Windows Defender reputation and security policies for the actual release environment.
- [ ] Confirm the x64 installed executable, uninstall registration and data-preservation checks passed.
- [ ] Verify **ARM64 installation and startup on an actual Windows-on-ARM device**. Building the ARM64 installer on x64 does not certify that runtime.
- [ ] Do not publish an unsigned or untested candidate as a fully certified production release.

The workflow does **not** verify installed GUI operation, any real broker connection,
network market-data entitlements, real order routing, or secrets migration.
The installer deliberately retains the historical uninstaller filename
`Desinstalar Trading Hub.exe` for compatibility; the installation wizard
and Installed Apps display name are English.

## 3. Inspect English visual evidence

- [ ] Review the latest artifacts from the [eight-screen UI workflow](https://github.com/dhamnaik-lab/trading-hub/actions/workflows/english-ui-snapshots.yml):
  Trading, Accounts, Connections, Settings, Live, strategy code, history and reports.
- [ ] Review the [footprint/DOM workflow](https://github.com/dhamnaik-lab/trading-hub/actions/workflows/english-localization-phase6-ui.yml).
- [ ] Inspect the actual PNGs, not just the workflow's success mark, for Spanish labels, clipping, missing text and layout issues.
- [ ] Keep old `docs/img/` screenshots explicitly labeled historical until replacement images are reviewed, copied into the repository, and rendered correctly in Markdown.
- [ ] Never substitute headless synthetic screenshots for actual broker-order validation.

**Screenshot publication procedure:** Use English screenshots generated from an accepted
commit; review them for secrets and account numbers; replace the seven historical
assets (or give the replacements new filenames); update links; then run the Markdown
link checker and inspect the README on desktop and mobile. Actions artifacts expire
and should not be linked as permanent documentation images.

## 4. Manual installed-app smoke test (simulation only)

Use a test Windows profile or a fresh VM without configured broker credentials.
Back up any prior user data before an upgrade test. Do not use a real account or
place a real order for localization verification.

- [ ] Install the correct architecture and launch the installed `TradingHub.exe`.
- [ ] Verify startup, English navigation, data/connection screens, chart selection, indicators, footprint presets, DOM settings and dialogs.
- [ ] With a **simulated** account, validate display wording for order type, side, quantity, stop/target placement, cancel, and error conditions. Verify position size and order behavior have not changed.
- [ ] Run an isolated backtest, optimization and report generation; inspect English report headings and warnings.
- [ ] Check live-strategy pause, daily-loss and drawdown warning messages against test-only data. Do not enable real broker authentication.
- [ ] If Claude Code is installed, verify the risk-agent English prompt and JSON response parsing with a simulated portfolio, and confirm that approvals, sizing limits and real-account guards remain intact.
- [ ] Exit cleanly, test updating over a previous *fork* build, confirm existing workspace and custom strategy files still load, and uninstall.
- [ ] Confirm app data, custom strategies and credential-store entries were not unintentionally deleted.
- [ ] Record OS version/architecture, app version, exact source SHA, tester, screenshots, failures and final result.

## 5. Publishing gate

Publish from the fork only when all intended platform checks are complete.
Record version, exact SHA, binary SHA-256, signing status, test results,
known limitations and change log. Do not use the upstream author's installer or
label a CI artifact as a signed or production-verified release.

**Current scope:** GitHub Actions can validate a build, installer packaging, x64
silent installation/removal, and synthetic screenshot capture. Manual installed-app
runtime, Windows-on-ARM testing, real broker execution, macOS runtime testing,
and permanent replacement of all historical screenshots require separate evidence.
