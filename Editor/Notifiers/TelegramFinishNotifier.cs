using System;
using System.Threading;
using BuildFinishNotifier.Editor.Abstract;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UIElements;

namespace BuildFinishNotifier.Editor.Notifiers
{
    internal class TelegramFinishNotifier : BuildFinishNotifierBase<TelegramConfig>
    {
        public override async void NotifyAsync(BuildResult summaryResult)
        {
            TelegramBotClient client = new TelegramBotClient(Config.BotToken);
    
            try
            {
                Message message = await client.SendTextMessageAsync(
                    chatId: Config.ChatId,
                    text: summaryResult is BuildResult.Succeeded or BuildResult.Unknown ? 
                        Config.CompleteTextTemplate : Config.FailTextTemplate,
                    parseMode: ParseMode.Html,
                    cancellationToken: CancellationToken.None);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send message to chat {Config.ChatId} with bot token {Config.BotToken}. Reson: {e.Message}");
            }

            client = null;
        }
        
        protected override void WriteDefaultConfig(ref TelegramConfig config)
        {
            config.BotToken = "";
            config.ChatId = "";
            config.CompleteTextTemplate = "Build of project {project} was successfully finished";
            config.FailTextTemplate = "Project {project} has failed to build";
        }

        public override void CreateWindowElement(VisualElement root)
        { 
            CreateTextField(root, 
                "telegram-element__bot-token-label", "Bot Token",
                "telegram-element__bot-token-input", Config.BotToken, newValue =>
                {
                    Config.BotToken = newValue;
                });
            
            CreateTextField(root, 
                "telegram-element__chat-id-label", "Chat Id",
                "telegram-element__chat-id-input", Config.ChatId, newValue =>
                {
                    Config.ChatId = newValue;
                });
            
            CreateTextField(root, 
                "telegram-element__completed-text-label", "Completed text message template",
                "telegram-element__completed-text-input", Config.CompleteTextTemplate, newValue =>
                {
                    Config.CompleteTextTemplate = newValue;
                });
            
            CreateTextField(root, 
                "telegram-element__failed-text-label", "Failed build text message template",
                "telegram-element__failed-text-input", Config.FailTextTemplate, newValue =>
                {
                    Config.FailTextTemplate = newValue;
                });
        }

        private void CreateTextField(VisualElement root, 
            string headerName, string headerText, 
            string textFieldName, string textFieldValue,
            Action<string> onValueChanged)
        {
            TextField textField;
            root.Add(new Label()
            {
                name = headerName,
                text = headerText
            });
            root.Add(textField = new TextField()
            {
                name = textFieldName,
                value = textFieldValue
            });
            textField.RegisterValueChangedCallback(args =>
            {
                onValueChanged?.Invoke(args.newValue);
                SaveConfig();
            });
        }
    }
        
    internal struct TelegramConfig
    {
        public string BotToken;
        public string ChatId;
        public string CompleteTextTemplate;
        public string FailTextTemplate;
    }
}