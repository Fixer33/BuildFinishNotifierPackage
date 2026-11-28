using System;
using UnityEditor.Build.Reporting;
using UnityEngine.UIElements;

namespace Fixer33.BuildTools.Editor.Notifier.Abstract
{
    public interface IBuildFinishNotifier : IDisposable
    {
        public void NotifyAsync(BuildResult summaryResult);

        public void LoadConfig(string rawData, Action<string> onConfigUpdate);

        public void CreateWindowElement(VisualElement root);

        void IDisposable.Dispose()
        {
            // TODO: Implement dispose pattern
        }
    }
}