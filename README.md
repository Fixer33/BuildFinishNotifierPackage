# Build Tools

![Unity](https://img.shields.io/badge/Unity-UPM%20Package-blue)
![GitHub](https://img.shields.io/github/license/Fixer33/BuildToolsPackage)

Editor-only build helpers. Both parts can be switched on or off per project in
**Project Settings > Fixer33 > Build Tools** (also `Tools > Fixer33 > Build Tools > Settings`).

- **Build Finish Notifier** (on by default) - sends a message when a build finishes. Telegram is built in; add your own channels by implementing `IBuildFinishNotifier`.
- **Build Preflight** (off by default) - before a build, shows automatic checks (`IBuildPreflightCheck`, written in the game) and a manual checklist (a `BuildChecklistData` asset). The build starts only when nothing failed and every item is ticked.

Formerly `com.fixer33.build-finish-notifier` (renamed in 2.0.0).

## Authors

- [@Fixer33](https://github.com/Fixer33)

## Installation

Install with UPM:

```bash
  1. Open Unity Package Manager (Window/Package Manager)
  2. Press the plus (+) icon in the top-left corner
  3. Choose "Install package from git URL..."
  4. Paste url: https://github.com/Fixer33/BuildToolsPackage.git
  5. Press "Install"
```

Upgrading from `com.fixer33.build-finish-notifier`: remove it from `Packages/manifest.json` and add `com.fixer33.build-tools`. Notifier settings (EditorPrefs) are kept.

## Documentation

- [Usage Guide](Documentation/Usage%20Guide.md)
- [Package Description](Documentation/Package%20Description.md)
- [Telegram notifier setup](Documentation/TelegramNotifier.md)
- [Open Questions](Documentation/Open%20Questions.md)
