using System;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UIElements;

namespace BuildFinishNotifier.Editor.Abstract
{
    public abstract class BuildFinishNotifierBase : IBuildFinishNotifier
    {
        public abstract void NotifyAsync(BuildResult summaryResult);
        public abstract void LoadConfig(string rawData, Action<string> onConfigUpdate);
        public virtual void CreateWindowElement(VisualElement root) { }
    }
    
    public abstract class BuildFinishNotifierBase<T> : BuildFinishNotifierBase where T : new()
    {
        protected ref T Config => ref _config;
        
        private T _config;
        private Action<string> _onConfigUpdate;
        
        public override void LoadConfig(string rawData, Action<string> onConfigUpdate)
        {
            _onConfigUpdate = onConfigUpdate;
            try
            {
                _config = JsonUtility.FromJson<T>(rawData);
            }
            catch
            {
                _config = new T();
                WriteDefaultConfig(ref _config);
            }
        }

        protected abstract void WriteDefaultConfig(ref T config);

        protected void SaveConfig()
        {
            _onConfigUpdate?.Invoke(JsonUtility.ToJson(_config));
        }

        protected string GetFormattedText(string val)
        {
            return val.Replace("{project}", Application.productName);
        }
    }
}