using System;
using UnityEditor;
using UnityEngine;

namespace Fixer33.BuildTools.Editor.Settings
{
    /// <summary>
    /// Per-project switches for the build tools. Stored in
    /// <c>ProjectSettings/Packages/com.fixer33.build-tools/BuildToolsSettings.asset</c>,
    /// so the whole team shares them through version control.
    /// Edit them in Project Settings &gt; Fixer33 &gt; Build Tools.
    /// </summary>
    [FilePath("ProjectSettings/Packages/com.fixer33.build-tools/BuildToolsSettings.asset",
        FilePathAttribute.Location.ProjectFolder)]
    public sealed class BuildToolsSettings : ScriptableSingleton<BuildToolsSettings>
    {
        /// <summary>Raised after any setting changes and is saved.</summary>
        public static event Action Changed;

        [SerializeField] private bool _notifierEnabled = true;
        [SerializeField] private bool _preflightEnabled;
        [SerializeField] private bool _preflightInBatchMode;

        /// <summary>When false, no build-finish notification is sent and the notifier window does not open on first install.</summary>
        public bool NotifierEnabled
        {
            get => _notifierEnabled;
            set => Set(ref _notifierEnabled, value);
        }

        /// <summary>When true, every build started from the Build Profiles window or the Build menu shows the preflight window first.</summary>
        public bool PreflightEnabled
        {
            get => _preflightEnabled;
            set => Set(ref _preflightEnabled, value);
        }

        /// <summary>
        /// When false (default), builds in batch mode (CI, command line) skip the preflight window.
        /// Automatic checks never show UI in batch mode; when this is true they still run and a failed check stops the build.
        /// </summary>
        public bool PreflightInBatchMode
        {
            get => _preflightInBatchMode;
            set => Set(ref _preflightInBatchMode, value);
        }

        private void Set(ref bool field, bool value)
        {
            if (field == value)
                return;
            field = value;
            Save(true);
            Changed?.Invoke();
        }
    }
}
