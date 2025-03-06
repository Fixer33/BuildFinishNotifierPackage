using UnityEditor;
using UnityEngine;

namespace BuildFinishNotifier.Editor
{
    public class SettingsWindow : EditorWindow
    {
        [MenuItem("Tools/Build Finish Notifier/Settings")]
        private static void ShowWindow()
        {
            var window = GetWindow<SettingsWindow>();
            window.titleContent = new GUIContent("BFN Settigns");
            window.Show();
        }

        private BuildFinishNotifierSettings _settings;

        private void OnGUI()
        {
            _settings = BuildFinishNotifierSettings.Instance;
            if (_settings == false)
            {
                EditorGUILayout.HelpBox("Settings asset not found!", MessageType.Error);
                return;
            }
            
            _settings.Success = (AudioClip)EditorGUILayout.ObjectField("Success Clip", _settings.Success, typeof(AudioClip), false);
            _settings.Fail = (AudioClip)EditorGUILayout.ObjectField("Fail Clip", _settings.Fail, typeof(AudioClip), false);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Telegram:", EditorStyles.boldLabel);
            _settings.TelegramBotToken = EditorGUILayout.TextField("Bot token", _settings.TelegramBotToken);
            _settings.TelegramChatToSend = EditorGUILayout.TextField("Chat id to send", _settings.TelegramChatToSend);
            _settings.TelegramBuildCompleteText = EditorGUILayout.TextField("Text to send on build complete", _settings.TelegramBuildCompleteText);
            _settings.TelegramBuildFailText = EditorGUILayout.TextField("Text to send on build fail", _settings.TelegramBuildFailText);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_settings);
            }
        }
    }
}