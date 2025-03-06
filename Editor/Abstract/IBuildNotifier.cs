using UnityEditor.Build.Reporting;

namespace BuildFinishNotifier.Editor.Abstract
{
    public interface IBuildNotifier
    {
        public void NotifyAsync(BuildResult summaryResult);
    }
}