using System;
using System.Linq;
using BuildFinishNotifier.Editor.Abstract;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BuildFinishNotifier.Editor
{
    public class BuildFinishTrigger : IPostprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPostprocessBuild(BuildReport report)
        {
            var notifierTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(t => typeof(IBuildNotifier).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();
            
            var notifiers = CreateAllPossibleNotifiers(notifierTypes);
            NotifyAll(notifiers, report.summary.result);
        }

        private IBuildNotifier[] CreateAllPossibleNotifiers(Type[] types)
        {
            IBuildNotifier[] result = new IBuildNotifier[types.Length];
            
            for (int i = 0; i < types.Length; i++)
            {
                result[i] = Activator.CreateInstance(types[i]) as IBuildNotifier;
            }

            return result;
        }

        private void NotifyAll(IBuildNotifier[] notifiers, BuildResult summaryResult)
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
    }
}
