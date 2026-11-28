using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fixer33.BuildTools.Editor.Preflight
{
    /// <summary>
    /// Shown before a build while the preflight is enabled. Lists the automatic checks and the
    /// manual checklist; "Continue to Build" unlocks when no check failed and every item is ticked.
    /// </summary>
    public class BuildPreflightWindow : EditorWindow
    {
        private BuildPlayerOptions _pendingOptions;
        private bool _hasPendingBuild;
        private bool _continueConfirmed;
        private Button _continueButton;
        private VisualElement _checksContainer;
        private readonly List<Toggle> _toggles = new(8);
        private bool _anyCheckFailed;

        /// <summary>Opens the window for a pending build. The window starts or cancels that build.</summary>
        public static void Open(BuildPlayerOptions options)
        {
            var wnd = CreateInstance<BuildPreflightWindow>();
            wnd.titleContent = new GUIContent("Build Preflight");
            wnd.minSize = new Vector2(480, 420);
            wnd._pendingOptions = options;
            wnd._hasPendingBuild = true;
            wnd.ShowUtility();
            wnd.Focus();
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.paddingLeft = 10;
            root.style.paddingRight = 10;
            root.style.paddingTop = 10;
            root.style.paddingBottom = 10;

            var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1, marginBottom = 6 } };
            root.Add(scroll);

            // Automatic checks
            scroll.Add(Header("Automatic checks"));
            _checksContainer = new VisualElement();
            scroll.Add(_checksContainer);
            var rerun = new Button(RunChecks) { text = "Re-run checks", style = { alignSelf = Align.FlexStart, marginBottom = 8 } };
            scroll.Add(rerun);

            // Manual checklist
            scroll.Add(Header("Before building, confirm:"));
            _toggles.Clear();
            var data = BuildChecklistData.Find();
            if (data == null)
            {
                scroll.Add(new HelpBox("No Build Checklist asset in the project. Create one in Project Settings > Fixer33 > Build Tools.",
                    HelpBoxMessageType.Info));
            }
            else
            {
                foreach (var item in data.Items)
                {
                    var t = new Toggle(item?.Description ?? "") { style = { marginBottom = 2 } };
                    t.RegisterValueChangedCallback(_ => UpdateContinueInteractable());
                    scroll.Add(t);
                    _toggles.Add(t);
                }

                var selectRow = new VisualElement { style = { flexDirection = FlexDirection.Row, marginTop = 4 } };
                selectRow.Add(new Button(() => SetAll(true)) { text = "Select All" });
                selectRow.Add(new Button(() => SetAll(false)) { text = "Select None" });
                scroll.Add(selectRow);
            }

            var footer = new VisualElement { style = { flexDirection = FlexDirection.Row, justifyContent = Justify.FlexEnd } };
            root.Add(footer);
            footer.Add(new Button(Close) { text = "Cancel" });
            _continueButton = new Button(Proceed) { text = "Continue to Build", style = { marginLeft = 6 } };
            footer.Add(_continueButton);

            RunChecks();
        }

        private static Label Header(string text) =>
            new(text) { style = { unityFontStyleAndWeight = FontStyle.Bold, marginBottom = 4, marginTop = 4 } };

        private void RunChecks()
        {
            _checksContainer.Clear();
            _anyCheckFailed = false;

            var checks = BuildPreflight.CreateChecks();
            if (checks.Count == 0)
                _checksContainer.Add(new Label("None. Add checks by implementing IBuildPreflightCheck in an editor assembly.")
                    { style = { color = Color.gray, marginBottom = 4 } });

            foreach (var check in checks)
            {
                var result = BuildPreflight.RunSafe(check, _pendingOptions);
                var type = result.Status switch
                {
                    PreflightCheckStatus.Passed => HelpBoxMessageType.Info,
                    PreflightCheckStatus.Warning => HelpBoxMessageType.Warning,
                    _ => HelpBoxMessageType.Error,
                };
                string text = string.IsNullOrEmpty(result.Message) ? check.Title : $"{check.Title}: {result.Message}";
                _checksContainer.Add(new HelpBox(text, type) { style = { marginBottom = 2 } });
                _anyCheckFailed |= result.Status == PreflightCheckStatus.Failed;
            }

            UpdateContinueInteractable();
        }

        private void SetAll(bool value)
        {
            foreach (var t in _toggles)
                t.SetValueWithoutNotify(value);
            UpdateContinueInteractable();
        }

        private void UpdateContinueInteractable()
        {
            bool ready = !_anyCheckFailed;
            foreach (var t in _toggles)
                ready &= t.value;
            _continueButton?.SetEnabled(ready && _hasPendingBuild);
        }

        private void Proceed()
        {
            _continueConfirmed = true;
            Close();
            try
            {
                BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(_pendingOptions);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Build preflight: failed to start the build. {ex}");
            }
        }

        private void OnDestroy()
        {
            if (_hasPendingBuild && !_continueConfirmed)
                Debug.Log("Build preflight: build cancelled.");
        }
    }
}
