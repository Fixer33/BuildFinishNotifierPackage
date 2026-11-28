using System;
using System.Threading;
using System.Threading.Tasks;
using Fixer33.BuildTools.Editor.Notifier.Abstract;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Fixer33.BuildTools.Editor.Notifier.Notifiers
{
    internal class TelegramFinishNotifier : BuildFinishNotifierBase<TelegramConfig>
    {
        public override async void NotifyAsync(BuildResult summaryResult)
        {
            TelegramBotClient client = new TelegramBotClient(Config.BotToken);

            try
            {
                string formattedText = GetFormattedText(summaryResult is BuildResult.Succeeded or BuildResult.Unknown
                    ? Config.CompleteTextTemplate
                    : Config.FailTextTemplate);
                Message message = await client.SendTextMessageAsync(
                    chatId: Config.ChatId,
                    text: formattedText,
                    parseMode: ParseMode.Html,
                    cancellationToken: CancellationToken.None);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send message to chat {Config.ChatId}. Reason: {Mask(e.Message, Config.BotToken)}");
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
            TelegramConfig draft = Config;
            Button saveButton = null, testButton = null;
            Label resultLabel = null;

            void Refresh()
            {
                bool changed = draft.BotToken != Config.BotToken || draft.ChatId != Config.ChatId ||
                               draft.CompleteTextTemplate != Config.CompleteTextTemplate ||
                               draft.FailTextTemplate != Config.FailTextTemplate;
                saveButton?.SetEnabled(changed);
                testButton?.SetEnabled(!string.IsNullOrWhiteSpace(draft.BotToken) &&
                                       !string.IsNullOrWhiteSpace(draft.ChatId));
            }

            root.Add(new Label("Get a message when a build completes or fails")
                { name = "telegram-element__description" });

            CreateTextField(root,
                "telegram-element__bot-token-label", "Bot Token",
                "telegram-element__bot-token-input", draft.BotToken, v => { draft.BotToken = v; Refresh(); });
            CreateTextField(root,
                "telegram-element__chat-id-label", "Chat Id",
                "telegram-element__chat-id-input", draft.ChatId, v => { draft.ChatId = v; Refresh(); });
            CreateTextField(root,
                "telegram-element__completed-text-label", "Completed template",
                "telegram-element__completed-text-input", draft.CompleteTextTemplate,
                v => { draft.CompleteTextTemplate = v; Refresh(); });
            CreateTextField(root,
                "telegram-element__failed-text-label", "Failed template",
                "telegram-element__failed-text-input", draft.FailTextTemplate,
                v => { draft.FailTextTemplate = v; Refresh(); });

            var actions = new VisualElement { name = "telegram-element__actions" };
            actions.AddToClassList("actions");
            root.Add(actions);

            saveButton = new Button(() =>
            {
                Config = draft;
                SaveConfig();
                Refresh();
            }) { name = "telegram-element__save-button", text = "Save" };
            saveButton.AddToClassList("btn");
            saveButton.AddToClassList("btn--primary");
            actions.Add(saveButton);

            testButton = new Button { name = "telegram-element__test-button", text = "Send test message" };
            testButton.AddToClassList("btn");
            testButton.AddToClassList("btn--ghost");
            testButton.clicked += async () =>
            {
                testButton.SetEnabled(false);
                SetResult(resultLabel, "Sending...", null);
                string error = await SendTestAsync(draft);
                SetResult(resultLabel, error == null ? "Test message sent" : error,
                    error == null ? "result--success" : "result--error");
                Refresh();
            };
            actions.Add(testButton);

            resultLabel = new Label { name = "telegram-element__result" };
            resultLabel.AddToClassList("result");
            resultLabel.style.display = DisplayStyle.None;
            root.Add(resultLabel);

            Refresh();
        }

        private static string Mask(string text, string token)
        {
            text ??= "Unknown error";
            return string.IsNullOrEmpty(token) ? text : text.Replace(token, "***");
        }

        private static void SetResult(Label label, string text, string cls)
        {
            label.RemoveFromClassList("result--success");
            label.RemoveFromClassList("result--error");
            if (cls != null) label.AddToClassList(cls);
            label.text = text;
            label.style.display = DisplayStyle.Flex;
        }

        private async Task<string> SendTestAsync(TelegramConfig cfg)
        {
            try
            {
                var client = new TelegramBotClient(cfg.BotToken);
                await client.SendTextMessageAsync(
                    chatId: cfg.ChatId,
                    text: GetFormattedText(cfg.CompleteTextTemplate ?? ""),
                    parseMode: ParseMode.Html,
                    cancellationToken: CancellationToken.None);
                return null;
            }
            catch (Exception e)
            {
                return Mask(e.Message, cfg.BotToken);
            }
        }

        private void CreateTextField(VisualElement root,
            string headerName, string headerText,
            string textFieldName, string textFieldValue,
            Action<string> onValueChanged)
        {
            var field = new VisualElement();
            field.AddToClassList("field");
            var header = new Label { name = headerName, text = headerText };
            header.AddToClassList("field-label");
            field.Add(header);
            var textField = new TextField { name = textFieldName, value = textFieldValue };
            field.Add(textField);
            root.Add(field);
            textField.RegisterValueChangedCallback(args => onValueChanged?.Invoke(args.newValue));
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
