# Install Trading Hub with Claude Code

Claude Code can build and install Trading Hub on your Mac, verify the required tools, and help you connect its AI features to your own Claude Code session. You do not need to be a programmer.

**This guide builds the English fork** at [dhamnaik-lab/trading-hub](https://github.com/dhamnaik-lab/trading-hub). The original author's installers and source checkout do not automatically include this fork's translations.

## Before you begin

- A Mac with Apple Silicon (M1 or newer) and macOS 12 or later.
- [Claude Code](https://claude.com/claude-code) installed and signed in using a plan that supports it.
- Git installed, or a downloaded ZIP of this fork.

For Windows builds, use the Windows instructions in the [main README](README.md#installation); the macOS commands below do not apply.

## Steps

1. Clone **this fork**:

   ```bash
   git clone https://github.com/dhamnaik-lab/trading-hub.git
   cd trading-hub
   ```

   Alternatively select **Code → Download ZIP** on this fork's repository page and extract it.

2. Start Claude Code in the repository folder:

   ```bash
   claude
   ```

3. Send the following prompt to Claude Code:

   > Install Trading Hub on my Mac using the CLAUDE.md instructions in this folder. First check that this is a compatible Apple Silicon Mac running macOS 12 or later. Install the .NET 10 SDK in ~/.dotnet without sudo if it is missing. Build and install the app with ./build-app.sh --install, then check that the installed app can locate my Claude Code CLI. Only create a link in ~/.local/bin if it is necessary. Do not overwrite another app named Trading Hub without asking. Show any real build errors and confirm success only after verifying that the app is installed. Open it and briefly explain how to get started. Preserve my existing user data and credentials.

## What Claude Code does

- Checks the Mac's OS version and CPU architecture.
- Installs a user-local .NET 10 SDK if needed.
- Builds the application and installs it to `/Applications/Trading Hub.app`.
- Verifies that the app can locate your existing Claude Code session for the Backtesting assistant, local laboratory, and live risk agent.
- Opens Trading Hub, which initially uses a **simulated account**. No live orders are sent unless a supported real account is separately configured and authorized.

The app's integration uses your Claude Code CLI session. It does not require placing a Claude API key in this repository.

## Updating

Pull updates from your intended fork branch (or download the new fork ZIP), then ask Claude Code to rebuild and reinstall Trading Hub. Application data, saved connections, and user strategy files live outside the source checkout. Back up important user data before major upgrades.

## Troubleshooting

- **App cannot locate Claude Code:** Confirm the CLI installation and `command -v claude`. A user-local link in `~/.local/bin/claude` may help.
- **macOS cannot verify the application:** The build script applies an ad-hoc signature. Rebuild locally if you copied the app from another computer.
- **Build fails:** Check that `~/.dotnet/dotnet --list-sdks` or `dotnet --list-sdks` includes a 10.x SDK. Inspect the actual compiler error rather than assuming the build completed.
- **The app still displays Spanish:** Localization is ongoing; ensure you built the English fork's latest source and check the translation status in [README.md](README.md).

See the [installation guide](docs/instalacion.md) for platform and storage details.
