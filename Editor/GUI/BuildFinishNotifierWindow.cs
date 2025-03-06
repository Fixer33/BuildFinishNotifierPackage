using System;
using BuildFinishNotifier.Editor.Abstract;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace BuildFinishNotifier.Editor.GUI
{
    internal class BuildFinishNotifierWindow : EditorWindow
    {
        [MenuItem("Tools/Fixer33/Build Finish Notifier")]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildFinishNotifierWindow>();
            window.titleContent = new GUIContent("Build Finish Notifier");
            window.Show();
        }

        private IBuildFinishNotifier[] _notifiers;
        private ScrollView _container;
        
        private void CreateGUI()
        {
            GC.Collect();
            
            string loadPath = "Packages/com.fixer33.build-finish-notifier/Editor/GUI/Styles/BuildFinishNotifierWindow.uss";
#if PACKAGES_DEV
            loadPath = "Assets/" + loadPath;            
#endif
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(loadPath);
            if (style != null)
            {
                rootVisualElement.styleSheets.Add(style);
            }
            
            rootVisualElement.Add(_container = new ScrollView()
            {
                name = "container",
            });

            _notifiers = BuildFinishNotifier.CreateAllPossibleNotifiers();
            foreach (var buildNotifier in _notifiers)
            {
                if (buildNotifier == null)
                    continue;
                
                VisualElement element = new VisualElement()
                {
                    name = $"container-element__{buildNotifier.GetType().Name}"
                };
                element.AddToClassList("notifierElement");
                element.Add(new Label()
                {
                    name = "container-element__name",
                    text = buildNotifier.GetType().Name
                });
                _container.contentContainer.Add(element);
                buildNotifier.CreateWindowElement(element);
            }
        }

        private void OnDestroy()
        {
            foreach (var buildFinishNotifier in _notifiers)
            {
                buildFinishNotifier?.Dispose();
            }
            _notifiers = null;

            GC.Collect();
        }
    }
}