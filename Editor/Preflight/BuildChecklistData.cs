using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Fixer33.BuildTools.Editor.Preflight
{
    /// <summary>
    /// The manual part of the build preflight: items the user ticks before a build.
    /// Keep one asset per project (anywhere under Assets, best in an Editor folder).
    /// Create it from Project Settings &gt; Fixer33 &gt; Build Tools or Create &gt; Fixer33 &gt; Build Checklist.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildChecklistData", menuName = "Fixer33/Build Checklist")]
    public class BuildChecklistData : ScriptableObject
    {
        /// <summary>One manual checklist line.</summary>
        [System.Serializable]
        public class ChecklistItem
        {
            [TextArea]
            public string Description;
        }

        /// <summary>Where <see cref="CreateDefault"/> puts a new asset.</summary>
        public const string DefaultAssetPath = "Assets/Editor/BuildPreflight/BuildChecklistData.asset";

        [SerializeField]
        private List<ChecklistItem> _items = new List<ChecklistItem>();

        /// <summary>The checklist lines, in display order.</summary>
        public IReadOnlyList<ChecklistItem> Items => _items;

        /// <summary>Finds the project's checklist asset, or returns null when there is none.</summary>
        public static BuildChecklistData Find()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:" + nameof(BuildChecklistData)))
            {
                var asset = AssetDatabase.LoadAssetAtPath<BuildChecklistData>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null)
                    return asset;
            }
            return null;
        }

        /// <summary>Creates a checklist with a few generic items at <see cref="DefaultAssetPath"/>.</summary>
        public static BuildChecklistData CreateDefault()
        {
            EnsureFolder("Assets/Editor");
            EnsureFolder("Assets/Editor/BuildPreflight");

            var data = CreateInstance<BuildChecklistData>();
            data._items = new List<ChecklistItem>
            {
                new ChecklistItem { Description = "Update the version number (Player Settings)." },
                new ChecklistItem { Description = "Check the build target and platform settings." },
                new ChecklistItem { Description = "Disable debug and development-only features." },
            };

            AssetDatabase.CreateAsset(data, AssetDatabase.GenerateUniqueAssetPath(DefaultAssetPath));
            AssetDatabase.SaveAssets();
            return data;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            int slash = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
        }
    }
}
