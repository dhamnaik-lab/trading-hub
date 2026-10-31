[← Back to documentation](README.md)

# Installation

This guide covers building the **English fork** of Trading Hub on macOS and Windows. The upstream author's downloadable installers are separate binaries; they do not automatically contain this fork's translations.

## Requirements

| Platform | Requirements |
|---|---|
| Windows | Windows 10 or 11, x64 or ARM64; Git, .NET 10 SDK, and NSIS to build an installer |
| macOS | Apple Silicon (M1 or later), macOS 12 or later; .NET 10 SDK to build |
| Internet | Needed to download NuGet packages during initial builds |
| Claude Code | Optional; required only for Claude-powered assistant and risk-agent features |

The published application is self-contained, so an end user does not normally need a separate .NET runtime. **Build from this fork** when you want this fork's English changes.

## Get the correct source

```bash
git clone https://github.com/dhamnaik-lab/trading-hub.git
cd trading-hub
```

You can also download a source ZIP from the fork's GitHub page. Do not clone `ralphtradesz/trading-hub` when your goal is to build the English fork.

## Windows installation (x64 and ARM64)

1. Install [Git](https://git-scm.com), the [.NET 10 SDK](https://dotnet.microsoft.com/download), and [NSIS](https://nsis.sourceforge.io). Windows users with winget can install NSIS with `winget install NSIS.NSIS`.
2. Open PowerShell in the repository root.
3. Run:

   ```powershell
   .\build-windows.ps1
   ```

4. Check for a successful build and find the generated installer(s) in `dist`. Install the appropriate architecture build. Do not assume the build passed merely because an output folder exists.

The Windows installer is per-user and does not require administrator rights. It places program files in `%LOCALAPPDATA%\Programs\Trading Hub`. User data and credentials are kept elsewhere and are not intentionally deleted by an application update or normal uninstall.

**Important:** Existing Windows installer packages linked from [the original author's releases](https://github.com/ralphtradesz/trading-hub/releases) are *upstream* builds and may still display Spanish. They are not verified releases of this fork.

## macOS installation (Apple Silicon)

Verify your platform:

```bash
uname -m                 # must show arm64
sw_vers -productVersion  # macOS 12 or later
```

Intel Macs (`x86_64`) are not supported by the existing macOS build script.

### Option 1: Install with Claude Code

If you have Claude Code, open the fork's repository in Terminal, run `claude`, and ask it to install Trading Hub following `CLAUDE.md`. It should verify the Mac, install a user-local .NET 10 SDK if needed, build the application, confirm CLI availability, and launch the app. It must ask before replacing another app with the same name.

See [Install with Claude Code](../INSTALAR_CON_CLAUDE_CODE.md).

### Option 2: Manual installation

1. Check the .NET SDK:

   ```bash
   ~/.dotnet/dotnet --list-sdks
   ```

   If a 10.x SDK is available, continue to step 3.

2. Install the .NET 10 SDK locally if needed:

   ```bash
   curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
   bash /tmp/dotnet-install.sh --channel 10.0
   ~/.dotnet/dotnet --list-sdks
   ```

3. From the repository root, build and install:

   ```bash
   ./build-app.sh --install
   ```

4. After a successful build and installation, open:

   ```bash
   open "/Applications/Trading Hub.app"
   ```

The build script adds the local .NET SDK to its own environment when needed. The installed Mac application is self-contained.

### What the macOS build script does

- Publishes a Release application for `osx-arm64`.
- Creates the app bundle outside Desktop/iCloud-managed paths to avoid signing attributes.
- Signs the app and stages a copy at `dist/Trading Hub.app`.
- With `--install`, replaces an existing application at `/Applications/Trading Hub.app` and verifies its signature.

Without `--install`, the script builds `dist/Trading Hub.app` without replacing the app in Applications.

### Code signing

The build script can use an ad-hoc signature when you do not have a developer certificate. On another Mac, the first launch may require right-clicking the app and selecting **Open**. A locally ignored `build.local.env` can configure a signing certificate:

```bash
SIGN_IDENTITY="your certificate name"
BUNDLE_ID="com.your-domain.tradinghub"
```

Confirm the certificate identity with `security find-identity -v -p codesigning`. If the identity is not found, the script falls back to ad-hoc signing.

## Claude Code (optional)

Claude Code integration uses your existing CLI session, not a Claude API key stored in the repository. It supports strategy assistance from Backtesting and portfolio risk analysis for live strategies. Without Claude Code, other features can still operate.

On macOS the app searches common CLI installation directories such as `~/.local/bin/claude`, `~/.claude/local/claude`, `/opt/homebrew/bin/claude`, `/usr/local/bin/claude`, the directories in `PATH`, and the Claude desktop app's CLI.

If your CLI is installed elsewhere, you can create a user-local link:

```bash
mkdir -p ~/.local/bin && ln -sf "$(command -v claude)" ~/.local/bin/claude
```

You can also point to an explicit executable using `TRADINGHUB_CLAUDE_CLI`. For authentication, run `claude` and sign in using the supported login flow.

## Where data is stored

| Content | macOS | Windows |
|---|---|---|
| Settings, configured connections, charts, ticks, backtests, and live state | `~/Library/Application Support/Trading Hub` | `%APPDATA%\Trading Hub` |
| Custom C# strategies and indicators | `~/Documents/Trading Hub` | `Documents\Trading Hub` |
| Brokerage passwords, tokens, and secrets | macOS Keychain | Windows Credential Manager |

The data directory contains:

| Path | Description |
|---|---|
| `settings.json` | General preferences, hotkeys, DOM, and order templates |
| `connections.json` | Connection configuration without stored secret values |
| `workspace.json` | Open charts, tabs, layouts, and panels |
| `Ticks/` | Saved trade-by-trade data organized by provider, instrument, and date |
| `Klines/` | Downloaded candles for longer historical views |
| `Drawings/` | Instrument-specific drawings |
| `ChartTemplates/`, `IndicatorTemplates/`, `FootprintTemplates/` | Reusable templates |
| `Accounts/` | Account fills and history |
| `Backtests/` and `Live/` | Research runs and live-strategy state |
| `crash.log` | Unexpected application failures |

User strategies and indicators are stored outside the program installation. Do not accidentally overwrite or remove these directories during a source-code update.

To open the folders from the application, use **File → Open data folder**, **Code → Open indicators folder**, **Code → Open strategies folder**, or **Settings → Data**.

## Updating

1. **Stop or safely manage live strategies and open orders first.** Quit Trading Hub completely; closing its window may leave it active in the system tray or macOS menu bar.
2. Back up important app data and custom code.
3. In the fork checkout, run `git pull` only after checking local changes and the selected branch.
4. Rebuild and reinstall using the appropriate platform command (`.\build-windows.ps1` on Windows, `./build-app.sh --install` on macOS).

An installed app update is intended to preserve user data because it lives outside the program folder. Always verify compatibility before a major upgrade.

## Uninstalling

**Windows:** Quit the app fully, then uninstall Trading Hub from Windows **Settings → Apps → Installed apps**. The per-user application installation is removed; verify and back up data separately if you later choose to delete it. Credentials in Windows Credential Manager must be managed separately.

**macOS:** Quit fully using Command-Q or the menu-bar **Quit Trading Hub** command. Remove the app from Applications, then optionally remove credentials in Keychain Access and the user-data directories:

```bash
# Destructive: run only if you have backups and intend to erase all app data.
rm -rf "/Applications/Trading Hub.app"
rm -rf ~/Library/Application\ Support/Trading\ Hub
```

If you want to remove every stored secret, search for Trading Hub entries in **Keychain Access** and delete only the intended entries. Do not delete user strategies or indicators unless you deliberately wish to lose them. Keep `~/Documents/Trading Hub` if you want to preserve your source files.

## Troubleshooting

- **SDK missing:** Verify `dotnet --list-sdks` or `~/.dotnet/dotnet --list-sdks` includes 10.x.
- **Build fails:** Read the first actual compiler/error output. Verify internet and NuGet package availability.
- **Installer unavailable:** Check that NSIS is installed and the Windows build script completed successfully.
- **Claude Code not found:** Check `claude --version`, the CLI location, and `TRADINGHUB_CLAUDE_CLI`.
- **App exits unexpectedly:** Check `crash.log` in the application's data directory.
- **Spanish screens remain:** This fork is still being localized; check the source branch and current [translation status](../README.md).

Do not run tests against live broker accounts or production market-data directories. See [Development](desarrollo.md) for isolated test guidance.
