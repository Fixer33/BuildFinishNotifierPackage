using System.Collections.Generic;
using Fixer33.BuildTools.Editor.Notifier.GUI;
using Fixer33.BuildTools.Editor.Preflight;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fixer33.BuildTools.Editor.Settings
{
    /// <summary>Project Settings &gt; Fixer33 &gt; Build Tools.</summary>
    internal static class BuildToolsSettingsProvider
    {
        private const string Path = "Project/Fixer33/Build Tools";

        [SettingsProvider]
        public static SettingsProvider Create()
        {
            return new SettingsProvider(Path, SettingsScope.Project)
            {
                label = "Build Tools",
                keywords = new HashSet<string> { "build", "notifier", "preflight", "checklist", "telegram" },
                activateHandler = (_, root) => Build(root),
            };
        }

        [MenuItem("Tools/Fixer33/Build Tools/Settings")]
        private static void Open() => SettingsService.OpenProjectSettings(Path);

        private static void Build(VisualElement root)
        {
            var settings = BuildToolsSettings.instance;
            root.style.paddingLeft = 10;
            root.style.paddingTop = 6;

            root.Add(new Label("Build Tools") { style = { fontSize = 19, unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 8 } });

            // Notifier
            root.Add(Section("Build Finish Notifier"));
            var notifier = new Toggle("Enabled") { value = settings.NotifierEnabled,
                tooltip = "Send a notification when a build finishes. The channels (Telegram…) are configured per machine." };
            notifier.RegisterValueChangedCallback(e => settings.NotifierEnabled = e.newValue);
            root.Add(notifier);
            root.Add(new Button(BuildFinishNotifierWindow.ShowWindow) { text = "Configure channels…", style = { alignSelf = Align.FlexStart } });

            // Preflight
            root.Add(Section("Build Preflight"));
            var preflight = new Toggle("Enabled") { value = settings.PreflightEnabled,
                tooltip = "Show the preflight window (automatic checks + manual checklist) before every build." };
            preflight.RegisterValueChangedCallback(e => settings.PreflightEnabled = e.newValue);
            root.Add(preflight);

            var batch = new Toggle("Run checks in batch mode") { value = settings.PreflightInBatchMode,
                tooltip = "In CI / command-line builds, run the automatic checks and stop the build when one fails. The manual checklist is never shown in batch mode." };
            batch.RegisterValueChangedCallback(e => settings.PreflightInBatchMode = e.newValue);
            root.Add(batch);

            var checklistRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 4 } };
            var field = new ObjectField("Checklist asset") { objectType = typeof(BuildChecklistData), value = BuildChecklistData.Find(), style = { flexGrow = 1 } };
            field.SetEnabled(false);
            checklistRow.Add(field);
            var create = new Button(() => { field.value = BuildChecklistData.CreateDefault(); }) { text = "Create" };
            create.style.display = field.value == null ? DisplayStyle.Flex : DisplayStyle.None;
            field.RegisterValueChangedCallback(e => create.style.display = e.newValue == null ? DisplayStyle.Flex : DisplayStyle.None);
            checklistRow.Add(create);
            root.Add(checklistRow);

            var checks = BuildPreflight.CreateChecks();
            root.Add(new Label($"Automatic checks found: {checks.Count}") { style = { marginTop = 4 } });
            foreach (var check in checks)
                root.Add(new Label("  • " + check.Title));
        }

        private static Label Section(string text) =>
            new(text) { style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 10, marginBottom = 2 } };
    }
}
