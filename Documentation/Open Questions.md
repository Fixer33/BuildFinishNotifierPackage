# Build Tools - Open Questions

## Dependencies
1. **Telegram.Bot version.** The bundled DLL's version and licence are not stated in the files read. Should they be recorded?

## Behaviour
2. **`Unknown` build result treated as success.** The Telegram notifier sends the "complete" text for `BuildResult.Succeeded` and `BuildResult.Unknown`. Is that intended? `Cancelled` and `Failed` both give the "failed" text, which may not suit cancellations.
3. **Default config on first use.** `LoadConfig` writes defaults only if `JsonUtility.FromJson` throws. With a null or empty stored string it may return an empty config instead (assumed), leaving the templates empty. Not verified in the editor.
4. **Failure notifications.** Only `OnPostprocessBuild` is used. This callback may not run when a build fails before post-processing, so failure notices may be unreliable. Not tested.

## Robustness
5. **Dispose TODO** in `IBuildFinishNotifier`, and `async void NotifyAsync` in the Telegram notifier: are changes planned?
6. **`OnDestroy` null check.** `BuildFinishNotifierWindow.OnDestroy` iterates `_notifiers` without checking for null.

## Scope
7. **Other channels.** The README lists "Implemented notifiers" with only Telegram. Are others (Slack, Discord, email) planned?
8. **Tests and samples.** None exist. Intended?
9. **Per-machine settings.** Config lives in EditorPrefs (not per project, not shared with a team). Intentional?

## Answered
- **Asmdef reference to `com.fixer33.observable-variables`:** a leftover, confirmed by the author. Removed from the Editor asmdef.
- **Token handling (plain-text storage in EditorPrefs, token in the error log):** author confirmed no fix is planned; left as is.
