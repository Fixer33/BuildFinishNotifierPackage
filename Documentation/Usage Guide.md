# Build Finish Notifier - Usage Guide

Package `com.fixer33.build-finish-notifier` 1.0.0 (Unity 6000.0+). Editor-only tool that sends a notification when a Unity player build finishes. The only shipped channel is Telegram. There is no runtime API.

Items marked **(assumed)** were inferred from the code rather than read directly or tested.

## 1. Installation

**Option A - package installer.** Install `com.fixer33.package-installer` (see section 8), open `Tools > Fixer33 > Package Installer`, find "Build Finish Notifier" in its list and install it. Its `packages-list.json` entry is:

- PackageId: `com.fixer33.build-finish-notifier`
- PackageUrl: `https://github.com/Fixer33/BuildFinishNotifierPackage.git`

**Option B - Git URL.** `Window > Package Manager`, press `+`, choose "Install package from git URL...", paste `https://github.com/Fixer33/BuildFinishNotifierPackage.git`, press "Install".

`package.json` declares no dependencies. The Telegram client library (`Telegram.Bot.dll`) is bundled in the package and enabled for the Editor only.

## 2. Minimal setup (Telegram)

1. After the first install, the settings window opens by itself once per machine. Otherwise open it via `Tools > Fixer33 > Build Finish Notifier`.
2. Create a bot with @BotFather in Telegram and copy its token. Do not share it.
3. Message the bot (or add it to a chat and write 1-2 messages), then open `https://api.telegram.org/bot<token>/getUpdates` in a browser and read the `id` of the chat from the JSON response.
4. In the window, under `TelegramFinishNotifier`, fill in "Bot Token" and "Chat Id".
5. Run a build. When it finishes you receive the message. No code is needed.

Step-by-step bot instructions are also in `Documentation/TelegramNotifier.md`.

## 3. Using the editor window and the message templates

The window shows one framed block per discovered notifier, titled with the notifier's class name. For Telegram the fields are:

| Field | Meaning |
|---|---|
| Bot Token | Token from @BotFather |
| Chat Id | Target chat id |
| Completed text message template | Text sent when the build result is `Succeeded` or `Unknown` |
| Failed build text message template | Text sent for any other result (for example `Failed`, `Cancelled`) |

Every edit is saved immediately (there is no Save button). Defaults:

- Completed: `Build of project {project} was successfully finished`
- Failed: `Project {project} has failed to build`

The token `{project}` is replaced with `Application.productName`. Messages are sent with Telegram `ParseMode.Html`, so HTML tags such as `<b>` work in the templates, and a stray `<` or `&` may make Telegram reject the message (assumed).

## 4. Editor windows and menu paths

| Path | What it does |
|---|---|
| `Tools/Fixer33/Build Finish Notifier` | Opens `BuildFinishNotifierWindow` (internal `EditorWindow`, UI Toolkit). |
| automatic | `[InitializeOnLoadMethod]` opens the window once, the first time the editor loads after install, guarded by EditorPrefs key `bfn-first-launch`. |

## 5. Configuration and scripting defines

- **Storage:** each notifier's config is stored as JSON in `EditorPrefs` under the key `bfn-notifier-<ClassName>-data` (for Telegram: `bfn-notifier-TelegramFinishNotifier-data`). EditorPrefs are per machine and per user: they are not part of the project and are not shared through version control. Team members and build machines must each configure their own.
- **Define `PACKAGES_DEV`:** only for developing the package itself. When defined, the window loads its style sheet from `Assets/Packages/com.fixer33.build-finish-notifier/...` instead of `Packages/com.fixer33.build-finish-notifier/...`. Normal users do not need it.
- The assembly `com.fixer33.build-finish-notifier.Editor` has no `versionDefines` or `defineConstraints`, and no assembly references.

## 6. How it works and extension points

Flow: the build finishes, `BuildFinishNotifier.OnPostprocessBuild` (an `IPostprocessBuildWithReport`, `callbackOrder` 0) runs, `CreateAllPossibleNotifiers()` scans all loaded assemblies for non-abstract types implementing `IBuildFinishNotifier`, creates each with `Activator.CreateInstance`, calls `LoadConfig` with the stored string, then `NotifyAsync(report.summary.result)` is called on each. An exception thrown by one notifier is caught and logged with `Debug.LogError`; other notifiers still run. The notifiers are created again for every build, and again each time the window is created.

Public types (namespace `BuildFinishNotifier.Editor.Abstract`):

- `IBuildFinishNotifier : IDisposable` - members `NotifyAsync(BuildResult)`, `LoadConfig(string rawData, Action<string> onConfigUpdate)`, `CreateWindowElement(VisualElement root)`. `Dispose` has an empty default implementation.
- `BuildFinishNotifierBase` - abstract `NotifyAsync` and `LoadConfig`, virtual empty `CreateWindowElement`.
- `BuildFinishNotifierBase<T> where T : new()` - keeps a `JsonUtility` config of type `T`. Protected members: `ref T Config`, `SaveConfig()`, `GetFormattedText(string)` (replaces `{project}`), abstract `WriteDefaultConfig(ref T config)`.

There is no registration step: a new notifier is found automatically.

### Worked example: a notifier that writes to the Console

Put this in an editor assembly (or in an `Editor` folder, which uses `Assembly-CSharp-Editor`). If you use your own asmdef, add `com.fixer33.build-finish-notifier.Editor` to its references and restrict it to the Editor platform.

```csharp
using System;
using BuildFinishNotifier.Editor.Abstract;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class ConsoleNotifierConfig
{
    public string Message;
}

// Needs a public parameterless constructor (created by Activator.CreateInstance).
public class ConsoleFinishNotifier : BuildFinishNotifierBase<ConsoleNotifierConfig>
{
    public override void NotifyAsync(BuildResult summaryResult)
    {
        Debug.Log(GetFormattedText(Config.Message) + " (" + summaryResult + ")");
    }

    protected override void WriteDefaultConfig(ref ConsoleNotifierConfig config)
    {
        config.Message = "Build of {project} finished";
    }

    public override void CreateWindowElement(VisualElement root)
    {
        var field = new TextField("Message") { value = Config.Message };
        field.RegisterValueChangedCallback(e =>
        {
            Config.Message = e.newValue;
            SaveConfig(); // writes JSON to EditorPrefs via the callback given to LoadConfig
        });
        root.Add(field);
    }
}
```

It appears in the window as a block titled `ConsoleFinishNotifier` and is called after every build. Alternatively implement `IBuildFinishNotifier` directly if you want to manage your own storage.

Notes for notifier authors:

- The class must be non-abstract with a parameterless constructor.
- The storage key uses the class name only, so two notifiers with the same class name in different namespaces would share one key (assumed from the key format).
- Whether `Config` is non-null on the very first use with a class-typed `T` is not verified: the base `LoadConfig` writes defaults only if `JsonUtility.FromJson` throws, and a null or empty stored string may instead produce an empty config (assumed; see section 9).

## 7. Common pitfalls

- **Bot token is stored in plain text and logged on error (known issue, no fix planned).** The token is saved unencrypted in `EditorPrefs` (on Windows, the registry), and when sending fails, `TelegramFinishNotifier` writes `Failed to send message to chat <id> with bot token <token>. ...` to the Console and `Editor.log`. Do not share logs or screenshots of the Console, and consider the token compromised if a log leaks (revoke it through @BotFather).
- **Settings are per machine, not per project.** A teammate or a CI machine will not get your bot data (EditorPrefs). Batch-mode build machines need their own configuration (assumed).
- **Template characters.** Templates are sent as HTML; unescaped `<`, `>` or `&` may cause a send failure (assumed).
- **Cancelled builds use the "failed" text; `Unknown` uses the "completed" text.** Only `Succeeded` and `Unknown` select the completed template.
- **Failure notices may not always arrive.** Only `OnPostprocessBuild` is used. Whether Unity calls it when a build fails before post-processing is not tested.
- **Fire and forget.** `TelegramFinishNotifier.NotifyAsync` is `async void`: errors are only visible as Console messages, and an editor closing right after the build may drop the message (assumed). Network access to `api.telegram.org` is required.
- **Empty fields.** With an empty token or chat id the send fails and an error is logged (including the empty token).
- **`Dispose` does nothing** by default, so notifiers that hold resources must release them themselves; there is no guaranteed lifecycle (the default `Dispose` contains a `TODO`).
- **Window close.** `BuildFinishNotifierWindow.OnDestroy` iterates its notifier list without a null check; closing a window whose `CreateGUI` never ran may log an exception (assumed).
- **Assembly scan.** Notifier discovery calls `GetTypes()` on every loaded assembly; an assembly that fails to load its types could break discovery (assumed).
- Error text contains the typo "Reson" (cosmetic).

## 8. Other com.fixer33.* packages

The package has no dependency on any other package and uses none at runtime or in the editor. Related package:

- `com.fixer33.package-installer` - `https://github.com/Fixer33/PackageInstaller.git`. Installs this package through the entry shown in section 1.

Notifiers are discovered across all loaded assemblies, so a notifier implemented in another package or project assembly (any type implementing `IBuildFinishNotifier`) would appear automatically. No other `com.fixer33.*` package currently does this (assumed from the package list).

## 9. Remaining open questions

These are still open and affect usage:

- **Telegram.Bot version and licence** are not recorded (open question 1).
- **`Unknown` result treated as success** and `Cancelled` treated as failed (question 2).
- **Default config on first use:** with a null or empty stored string, templates may come out empty instead of defaulted; not verified in the editor (question 3).
- **Failure notifications:** may be unreliable if post-processing is not reached; not tested (question 4).
- **Dispose TODO and `async void NotifyAsync`:** whether changes are planned (question 5).
- **`OnDestroy` null check** in the window (question 6).
- **Other channels** (Slack, Discord, email): none are planned as far as known; only Telegram exists (question 7).
- **Tests and samples:** none exist; whether that is intended is open (question 8).
- **Per-machine settings (EditorPrefs):** whether this is intentional is open (question 9).

The token handling (plain-text storage and token in the error log) is answered: no fix is planned, and the behaviour described in section 7 is current.
