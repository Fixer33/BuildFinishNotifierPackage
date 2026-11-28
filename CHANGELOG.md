## [2.0.0] - 2026-11-28

### Changed
- **Renamed** from `com.fixer33.build-finish-notifier` to `com.fixer33.build-tools` (repository `BuildToolsPackage`). Replace the old entry in `Packages/manifest.json`.
- Assembly renamed to `com.fixer33.build-tools.Editor`; namespaces moved from `BuildFinishNotifier.Editor.*` to `Fixer33.BuildTools.Editor.Notifier.*`.
- Notifier window menu moved to `Tools/Fixer33/Build Tools/Build Finish Notifier`.

### Added
- Project Settings page `Fixer33/Build Tools` with per-project switches (stored in `ProjectSettings/Packages/com.fixer33.build-tools/`).
- Notifier on/off switch (on by default). When off, nothing is sent and the window does not open on first install.
- Build Preflight (off by default): automatic checks found from `IBuildPreflightCheck` implementations, plus a manual checklist asset `BuildChecklistData`. Optional check run in batch mode.
- EditMode tests for the preflight.

## [1.0.1] - 2026-11-20

### Added
- Save and Send test message buttons in the Telegram notifier settings.
- Documentation: Package Description, Usage Guide and Open Questions.

### Changed
- Redesigned the notifier window styling and layout.
- Telegram settings are now edited as a draft and applied only on Save.
- Removed the assembly definition reference.
- README links to the new documentation.

### Fixed
- Bot token is masked in error messages.

## [1.0.0] - 2025-03-07

### Added

- Created first version of package

### Changed

### Fixed