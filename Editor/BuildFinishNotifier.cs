using System;
using System.Linq;
using BuildFinishNotifier.Editor.Abstract;
using BuildFinishNotifier.Editor.GUI;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BuildFinishNotifier.Editor
{
    internal class BuildFinishNotifier : IPostprocessBuildWithReport
    {
        private const string FIRST_LAUNCH_KEY = "bfn-first-launch";
        
        [InitializeOnLoadMethod]
        public static void OnLoad()
        {
            if (EditorPrefs.GetInt(FIRST_LAUNCH_KEY, 1) != 1) 
                return;
            
            EditorPrefs.SetInt(FIRST_LAUNCH_KEY, 0);
            BuildFinishNotifierWindow.ShowWindow();
        }
        
        public static IBuildFinishNotifier[] CreateAllPossibleNotifiers()
        {
            var notifierTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => typeof(IBuildFinishNotifier).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();
            
            IBuildFinishNotifier[] result = new IBuildFinishNotifier[notifierTypes.Length];
            
            for (int i = 0; i < notifierTypes.Length; i++)
            {
                result[i] = Activator.CreateInstance(notifierTypes[i]) as IBuildFinishNotifier;
                if (result[i] != null)
                {
                    string saveKey = $"bfn-notifier-{result[i].GetType().Name}-data";
                    result[i].LoadConfig(EditorPrefs.GetString(saveKey, null), newConfigRawData =>
                    {
                        EditorPrefs.SetString(saveKey, newConfigRawData);
                    });
                }
            }

            return result;
        }
        
        #region Trigger

        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            var notifiers = CreateAllPossibleNotifiers();
            NotifyAll(notifiers, report.summary.result);
        }

        private static void NotifyAll(IBuildFinishNotifier[] notifiers, BuildResult summaryResult)
        {
            foreach (var notifier in notifiers)
            {
                if (notifier == null)
                    continue;
                
                try
                {
                    notifier.NotifyAsync(summaryResult);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to use notifier {notifier.GetType().Name}. Reason: {e.Message} \n Stacktrace: {e.StackTrace}");
                }
            }
        }

        #endregion
    }
}
