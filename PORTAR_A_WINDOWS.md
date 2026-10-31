# Trading Hub Windows port — technical reference

> **Historical note:** Trading Hub already supports Windows 10 and 11 in the current source. The initial port was completed in the upstream project's 1.1.0 timeframe. This document preserves the porting architecture and regression checklist, **not** a request to port the app again. Upstream installers are published by the [original author](https://github.com/ralphtradesz/trading-hub/releases) and do not automatically include this fork's English-language changes.

Trading Hub uses C#/.NET 10, Avalonia, and SkiaSharp. Most projects—data, market feeds, charts, backtesting, strategies, indicators, and the MCP laboratory—are platform-independent. The operating-system-specific concerns are secure credentials, power management, menus, process launching, paths, shortcuts, and packaging.

For current installation commands see [Installation](docs/instalacion.md); for contribution constraints see [Development](docs/desarrollo.md).

## Platform-specific components

| Area | macOS implementation | Windows implementation / requirement |
|---|---|---|
| Build and packaging | `build-app.sh`, `osx-arm64` app bundle, `codesign`, `ditto` | `build-windows.ps1`, self-contained `win-x64` and `win-arm64`, NSIS installer |
| Secure credentials | Keychain through `/usr/bin/security` | Windows Credential Manager / dedicated secure store through `WindowsCredentialStore` |
| Sleep policy | `SleepGuard.cs`, IOKit power assertions and optional `pmset` for lid-closed operation | `SetThreadExecutionState`; Windows itself manages lid-close behavior |
| Startup | macOS active-display checks | Platform guard to avoid invoking macOS-only display APIs on Windows |
| Menu and tray | Avalonia `TrayIcon`, macOS pointer positioning via Objective-C | Windows system tray and native pointer positioning via `GetCursorPos` |
| Open URLs/files | macOS `open` | `Process.Start` with `UseShellExecute = true`; `explorer.exe /select,...` where appropriate |
| Sound playback | macOS playback helpers | Windows-supported audio playback |
| Claude Code | Common Mac CLI folders and Terminal authorization | `claude.exe` / `claude.cmd` in user/npm paths and Windows terminal login |
| Window chrome | Mac traffic-light buttons on left | Windows title-bar buttons on right |
| Shortcuts | Command/Option keys | Ctrl/Alt keys; use common `Keys.Command` mapping |
| Fonts | Mac monospace fonts | Cascadia Mono / Consolas fallback |
| Application data | `~/Library/Application Support/Trading Hub` | `%APPDATA%\Trading Hub` |
| User strategies and indicators | `~/Documents/Trading Hub` | `Documents\Trading Hub` |
| Platform-specific wording | Mac, Keychain, menu bar, Command-Q | PC, Credential Manager, system tray, Alt+F4 |

Core source locations include `src/TradingHub.App/Services/Connections.cs`, `SleepGuard.cs`, `MenuBar.cs`, `ClaudeCode.cs`, `src/TradingHub.App/Windows/MainWindow.cs`, and `src/TradingHub.App/Program.cs`.

## Historical porting sequence

The original porting plan was:

1. **Build and launch:** Run `dotnet build TradingHub.slnx` and `dotnet test tests/TradingHub.Tests` on Windows, isolating Mac-only APIs behind platform guards or small interfaces.
2. **Secure secrets:** Implement a Windows credential store without exporting secrets to plaintext files. Use test-only credential storage in automated checks.
3. **OS integration:** Add Windows sleep handling, system tray, URL/file opening, sounds, and Claude CLI discovery.
4. **UI:** Adapt window chrome, command-key mapping, fonts, and platform-specific text. Verify UI snapshot scenes.
5. **Packaging:** Publish self-contained x64/ARM64 builds with app icon; build an installer.
6. **Final checks:** Run unit tests, UI snapshots, and manual simulated-account flows without real broker orders.

These are regression categories now that Windows support exists; they should not be reimplemented without evidence of defects.

## Source-maintenance prompt for Claude Code

The following is a **maintenance prompt for the existing cross-platform implementation**, not a new porting request. Run it from a clone of **this fork**, `https://github.com/dhamnaik-lab/trading-hub`:

> Inspect CLAUDE.md, README.md, docs/desarrollo.md, and PORTAR_A_WINDOWS.md before changing source. The project is a C#/.NET 10 Avalonia/SkiaSharp trading app supporting Windows and macOS. Preserve current behavior, platform support, existing UI layouts, broker integrations, trading/risk logic, and user-data paths. Audit cross-platform compatibility for secure credentials, sleep guard, system tray, file/URL opening, sounds, CLI discovery, window chrome, keyboard shortcuts, and packaging. Identify actual problems and implement only narrowly scoped corrections. Never use production data or real credentials in tests; use isolated folders, fake servers, and the simulated account. Do not add dependencies without approval. Verify with dotnet build TradingHub.slnx, dotnet test tests/TradingHub.Tests, relevant headless UI snapshots, and the platform build script. Translate only user-facing text into English; preserve serialized identifiers, API fields, and source compatibility. Report risks, files changed, and next steps, and disclose any checks not run.

## Windows build prerequisites

- Windows 10/11 on x64 or ARM64.
- [Git](https://git-scm.com).
- [.NET 10 SDK](https://dotnet.microsoft.com/download).
- [NSIS](https://nsis.sourceforge.io) for the Windows installer.
- Claude Code is optional for coding assistance and product AI features.

From a PowerShell terminal opened at the fork root:

```powershell
dotnet build TradingHub.slnx
dotnet test tests/TradingHub.Tests
.\build-windows.ps1
```

The Windows build script should produce architecture-specific installers in `dist`. Confirm actual output names and successful commands. Do not assume the upstream author's installer contains this fork's translations.

## Cross-platform verification

Verify both OS implementations after any change to shared source:

- Build and test the entire solution.
- Exercise simulator connection, chart loading, backtesting and replay.
- Check credentials are saved only in OS secure stores; never include test secrets in commits.
- Verify that manual real-account permissions and confirmations are unchanged.
- Check startup resumption of real-money strategies remains disabled unless explicitly enabled.
- Check detached windows, tray/menu panel positioning, navigation, fonts, dialogs and power management.
- Run isolated UI snapshot scenarios, for example:

```bash
dotnet run --project tools/TradingHub.UiSnapshot -- snapshots 4 trading connections panel footprint mercado
```

Some scenario identifiers (such as `mercado`) are intentionally retained in Spanish because they are test keys, not displayed labels.

## Linux considerations

Linux support would require a separate compatibility review: Secret Service/libsecret for credentials, a suitable sleep inhibitor (such as `systemd-inhibit`), `xdg-open` for file/URL launching, desktop integration, and packaging. Do not claim Linux is supported without build and runtime evidence.

## Project provenance

The original Windows porting work belongs to the upstream project maintained by [@ralphtradesz](https://x.com/ralphtradesz). This English fork remains separate; any future upstream contribution requires its own pull request and maintainers' approval.
