using System;
using System.Threading.Tasks;
using BuildFinishNotifier.Editor.Abstract;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BuildFinishNotifier.Editor.Notifiers
{
    public class SoundNotifier : IBuildNotifier
    {
        public async void NotifyAsync(BuildResult summaryResult)
        {
            var settings = BuildFinishNotifierSettings.Instance;
            
            GameObject go = new GameObject("Source");
            var audioSource = go.AddComponent<AudioSource>();
            audioSource.loop = false;
            audioSource.clip = summaryResult == BuildResult.Succeeded ? settings.Success : settings.Fail;
            audioSource.Play();
            await Task.Delay(Convert.ToInt32(audioSource.clip.length * 1000));
            EditorSceneManager.OpenScene(SceneManager.GetActiveScene().path);
        }
    }
}