# Trading Hub — instructions for coding assistants

This repository is the **English-language fork** at https://github.com/dhamnaik-lab/trading-hub, based on the original project by [@ralphtradesz](https://x.com/ralphtradesz). Trading Hub is a locally running C#/.NET 10 and Avalonia desktop trading platform for macOS (Apple Silicon) and Windows 10/11. The original codebase and much of its documentation were written in Spanish; this fork is being localized to English.

**Respond to the user in the language they use, defaulting to English.** Prefer English for new user-facing text, documentation, error messages, and installer prompts. Preserve existing functionality and licensing. Do not translate C# names, paths, API keys, serialized values, wire-protocol fields, trading identifiers, or any third-party content unless they are verified user-facing strings.

## If the user asks to install Trading Hub

Start by detecting the operating system and architecture. Do not follow macOS installation steps on Windows or imply this fork has published English binaries unless a release has actually been verified.

### Windows (Windows 10/11, x64 or ARM64)

1. Confirm supported Windows version and architecture using OS-native commands.
2. Confirm Git, the **.NET 10 SDK**, and **NSIS** are available. An NSIS install command is `winget install NSIS.NSIS` if the user has winget.
3. Run `.\build-windows.ps1` from this repository's root in PowerShell. The script builds installers in `dist`.
4. Show the actual build result, including errors. Do not report success until the installer exists and the command finishes successfully.
5. Do not claim that an upstream author's binary is translated. To obtain the fork's English changes, build from the fork or use a separately verified fork release.

Windows data and secrets are stored separately from the checkout: application data in `%APPDATA%\Trading Hub`, user strategies/indicators under the user's Documents directory, and secrets using Windows Credential Manager. Do not modify those user locations during a source-code task.

### macOS (Apple Silicon, macOS 12 or later)

1. Confirm `uname -m` returns `arm64` and `sw_vers -productVersion` meets the minimum version. Explain unsupported architectures rather than proceeding blindly.
2. Verify .NET 10 availability using `dotnet --list-sdks` or `~/.dotnet/dotnet --list-sdks`. If needed, install the SDK into the user's home directory without admin rights:
   ```bash
   curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
   bash /tmp/dotnet-install.sh --channel 10.0
   ```
3. Run `./build-app.sh --install` from the fork's repository root. Confirm the result before stating that `/Applications/Trading Hub.app` was installed. Ask before replacing an unrelated app with that name.
4. If the user wants AI features, verify Claude Code with `command -v claude` and `claude --version`. The app looks for the CLI in common user installation directories and in `PATH`. If the user has not signed in, have them run `claude` and use `/login`.
5. Launch the application with `open "/Applications/Trading Hub.app"` only after a successful installation.

On macOS, app data lives under `~/Library/Application Support/Trading Hub`, user strategies and indicators under `~/Documents/Trading Hub`, and connection secrets in macOS Keychain. Never edit these locations while running build or isolated tests.

### After installation

Explain that the initial account is simulated; users must explicitly configure a supported real account before sending live orders. The **Connections** screen configures supported providers. Binance and Bybit public market data may be available without account credentials; other integrations require credentials and may be data-only.

The **Backtesting** screen contains the C# strategy editor and Claude assistant. Consult [the documentation](docs/README.md), noting which guides are not yet translated. If the user asks how to support the original project, refer to the original author's support information rather than attributing it to this fork.

## If the user asks to update

Pull the intended branch and run the platform's build process again. Never overwrite local modifications without consent. Back up user data before major upgrades. Windows data remains in `%APPDATA%\Trading Hub`; macOS data remains in `~/Library/Application Support/Trading Hub`.

Do not switch the user's checkout to the original upstream repository when they explicitly want this fork's changes.

## If the user asks to work on the source code

- First inspect the working tree, relevant documentation, and the specific source files affected. Keep changes small and scoped. Do not rewrite working behavior as part of a localization pass.
- Main solution: `dotnet build TradingHub.slnx`.
- Test suite: `dotnet test tests/TradingHub.Tests`.
- Major projects: Core (data, providers, trading), Sdk (indicator API), Charting (Skia), Scripting (Roslyn), Strategies (backtesting, optimization, walk-forward), Indicators.Native, Indicators.Technical, and App (Avalonia UI).
- The visible product name comes from `TradingHub.Common.Product.Name`.
- UI snapshot tests can be run with `dotnet run --project tools/TradingHub.UiSnapshot -- <temporary-folder> 4 <scene>`. Use **isolated temporary data directories**, mock services, and `TRADINGHUB_DATA_DIR` where appropriate.
- **Never** run build/test tools against real account credentials, real user market-data folders, or a live trading session. Concurrent application processes can corrupt data stores.
- Never commit passwords, tokens, API keys, brokerage secrets, or personal account identifiers. The application uses OS-specific secure credential stores.
- Do not place real orders during translation or testing. Simulated orders and test fixtures are sufficient for automated verification.
- Trading safety is essential: preserve order direction, quantity, account selection, risk limits, confirmations, and routing. Any behavioral change needs its own separate review.
- Do not translate identifiers used by serialization, saved settings, API protocols, broker endpoints, strategy source files, or test fixtures without analyzing compatibility.
- Some SDK strings are generated in `src/TradingHub.Sdk/Localization/Strings.g.cs`. Do not hand-edit generated keys; investigate the source/generator or the overrides in `Strings.cs`.
- When translating Markdown headings, verify relative links and fragment anchors; preserve code blocks, paths, commands, and sample input data.
- Preserve attribution to the original author and comply with the [MIT + Commons Clause license](LICENSE). The Commons Clause restricts commercial sales, so do not describe the repository as OSI-approved open-source.
- See `docs/desarrollo.md` and `PORTAR_A_WINDOWS.md` for source and platform details. Those documents are translated into English, but may retain original source and test identifiers.

## Completion checklist

Report actual files changed and exact checks run. Distinguish verified passes from checks that could not be executed. Include unresolved Spanish UI/docs, compatibility risks, and next steps. Never claim a complete English translation or a working binary without verification.
