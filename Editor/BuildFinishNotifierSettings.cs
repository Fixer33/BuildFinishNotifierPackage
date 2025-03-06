using UnityEditor;
using UnityEngine;

namespace BuildFinishNotifier.Editor
{
    [CreateAssetMenu(fileName = "Settings", menuName = "BFNSettings", order = 0)]
    public class BuildFinishNotifierSettings : ScriptableObject
    {
        public static BuildFinishNotifierSettings Instance
        {
            get
            {
                if (_instance == false)
                {
                    var allAssetPaths = AssetDatabase.GetAllAssetPaths();
                    for (int i = 0; i < allAssetPaths.Length; i++)
                    {
                        _instance = AssetDatabase.LoadAssetAtPath<BuildFinishNotifierSettings>(allAssetPaths[i]);

                        if (_instance)
                            break;
                    }
                }

                return _instance;
            }
        }

        private static BuildFinishNotifierSettings _instance;

        public AudioClip Success;
        public AudioClip Fail;

        [Header("Telegram")]
        public string TelegramBotToken;
        [Tooltip("The id of a chat")] public string TelegramChatToSend;
        public string TelegramBuildCompleteText;
        public string TelegramBuildFailText;
    }
}