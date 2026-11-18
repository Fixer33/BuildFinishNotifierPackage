# Build Finish Notifier - Package Description

Package `com.fixer33.build-finish-notifier` 1.0.0, display name "Build Finish Notifier", Unity 6000.0+. Git URL: `https://github.com/Fixer33/BuildFinishNotifierPackage.git`.

## 1. Purpose and users

An editor-only tool that sends a notification when a Unity player build finishes. Intended for developers who run long builds and want to be told (for example on a phone) when the build is done or has failed. The only shipped notifier channel is Telegram (a bot sends a text message to a chat). Author: Fixer33. Licence: MIT.

## 2. Architecture and main components

All code lives under `Editor/` in one editor-only assembly `com.fixer33.build-finish-notifier.Editor` (root namespace `BuildFinishNotifier.Editor`).

| Component | File | Role |
|---|---|---|
| `BuildFinishNotifier` (internal class, `IPostprocessBuildWithReport`, `callbackOrder` 0) | `Editor/BuildFinishNotifier.cs` | Trigger. `OnPostprocessBuild` creates all notifiers and calls `NotifyAsync` on each with `report.summary.result`. Exceptions per notifier are caught and logged. |
| `IBuildFinishNotifier` | `Editor/Abstract/IBuildFinishNotifier.cs` | Notifier contract: `NotifyAsync(BuildResult)`, `LoadConfig(string, Action<string>)`, `CreateWindowElement(VisualElement)`, plus `IDisposable` with a default `Dispose` that does nothing (contains a `TODO: Implement dispose pattern`). |
| `BuildFinishNotifierBase` / `BuildFinishNotifierBase<T>` | `Editor/Abstract/BuildFinishNotifierBase.cs` | Convenience base classes. The generic one keeps a config of type `T` (`T : new()`), serialised with `JsonUtility`, and exposes `Config` (ref), `SaveConfig()`, `GetFormattedText(string)` and the abstract `WriteDefaultConfig(ref T)`. |
| `BuildFinishNotifierWindow` (internal `EditorWindow`) | `Editor/GUI/BuildFinishNotifierWindow.cs` | UI Toolkit settings window. Builds one framed block per notifier (title = notifier type name) and lets each notifier fill its own controls. Style sheet: `Editor/GUI/Styles/BuildFinishNotifierWindow.uss`. |
| `TelegramFinishNotifier`, `TelegramConfig` (both internal) | `Editor/Notifiers/TelegramFinishNotifier.cs` | The only notifier. Sends a message through the bundled `Telegram.Bot` library. Setup guide: `Documentation/TelegramNotifier.md`. |
| `Telegram.Bot.dll` (+ `.pdb`, `.xml`) | `Editor/Libraries/Telegram/` | Bundled third-party library, enabled for the Editor platform only (read from the `.meta` importer settings). Version not stated in the files read. |

Flow: build finishes -> `OnPostprocessBuild` -> `CreateAllPossibleNotifiers()` (reflection over all loaded assemblies; every non-abstract type implementing `IBuildFinishNotifier` is instantiated with `Activator.CreateInstance`) -> `LoadConfig` with the stored string -> `NotifyAsync`.

Configuration is stored per notifier in `EditorPrefs` under the key `bfn-notifier-<TypeName>-data` (JSON). It is therefore per machine and per user, not per project, and not in version control.

Telegram behaviour: `BuildResult.Succeeded` and `BuildResult.Unknown` use the "complete" template; any other result (failed, cancelled) uses the "fail" template. The message is sent with `ParseMode.Html`. In the text, the token `{project}` is replaced by `Application.productName`. Defaults: "Build of project {project} was successfully finished" and "Project {project} has failed to build"; bot token and chat id are empty. Send errors are logged with `Debug.LogError`.

## 3. Public API surface and extension points

Public types: `IBuildFinishNotifier`, `BuildFinishNotifierBase`, `BuildFinishNotifierBase<T>` (namespace `BuildFinishNotifier.Editor.Abstract`). Everything else is `internal`.

Extension point: add a new notifier by writing a class in an editor assembly that references this package's assembly and either implements `IBuildFinishNotifier` or derives from `BuildFinishNotifierBase<TConfig>` (implement `NotifyAsync` and `WriteDefaultConfig`, optionally override `CreateWindowElement`). It needs a parameterless constructor (it is created through `Activator.CreateInstance`). It is discovered automatically; there is no registration. The config type must be `JsonUtility`-serialisable.

There is no runtime API.

## 4. Dependencies

- Declared in `package.json`: none (empty `dependencies`).
- Asmdef references: none. (A leftover reference to the `com.fixer33.observable-variables` assembly was removed; the author confirmed it was unused.)
- Define symbols: no `versionDefines`, no `defineConstraints`. The window code checks `PACKAGES_DEV`: when defined, the style sheet is loaded from `Assets/Packages/...` instead of `Packages/...` (for development with the package embedded under Assets).
- Implicit: UnityEditor APIs (`IPostprocessBuildWithReport`, `EditorPrefs`, `EditorWindow`), UI Toolkit, and internet access to `api.telegram.org` for the Telegram notifier.

## 5. Editor tools and menu items

- Menu `Tools/Fixer33/Build Finish Notifier` opens the settings window.
- On the first editor load after install (`[InitializeOnLoadMethod]`, EditorPrefs key `bfn-first-launch`, per machine) the window opens once automatically.
- For Telegram the window shows: Bot Token, Chat Id, Completed text message template, Failed build text message template. Each edit is saved immediately.

## 6. Samples and tests

None: no `Samples~`, no test assemblies. A guide `Documentation/TelegramNotifier.md` explains how to get a bot token and chat id (via @BotFather and the `getUpdates` URL).

## 7. How other projects consume it

Install by Git URL in Package Manager (`https://github.com/Fixer33/BuildFinishNotifierPackage.git`), or through the author's `com.fixer33.package-installer` tool (assumed). Then open `Tools > Fixer33 > Build Finish Notifier`, fill in the bot data and run a build. No code is needed.

## 8. Current state

- Finished and functional for Telegram; version 1.0.0 (CHANGELOG: first version, 2025-03-07).
- Rough edges: `IBuildFinishNotifier.Dispose` is an empty default with a TODO; the bot token is stored in plain text in EditorPrefs and is written into the error log on a failed send (author: no fix planned); `TelegramFinishNotifier.NotifyAsync` is `async void`; notifiers are re-created (config reloaded) at every build; `BuildFinishNotifierWindow.OnDestroy` iterates `_notifiers` without a null check; `LoadConfig` writes defaults only when `JsonUtility.FromJson` throws, while empty text may instead give an all-null config (assumed); typo "Reson" in an error message.
- Unused: nothing known.
