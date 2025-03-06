using System;
using System.Threading;
using BuildFinishNotifier.Editor.Abstract;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BuildFinishNotifier.Editor.Notifiers
{
    public class TelegramNotifier : IBuildNotifier
    {
        public async void NotifyAsync(BuildResult summaryResult)
        {
            var settings = BuildFinishNotifierSettings.Instance;

            TelegramBotClient client = new TelegramBotClient(settings.TelegramBotToken);
    
            try
            {
                Message message = await client.SendTextMessageAsync(
                    chatId: settings.TelegramChatToSend,
                    text: summaryResult is BuildResult.Succeeded or BuildResult.Unknown ? 
                        settings.TelegramBuildCompleteText : settings.TelegramBuildFailText,
                    parseMode: ParseMode.Html,
                    cancellationToken: CancellationToken.None);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to send message to chat {settings.TelegramChatToSend} with bot token {settings.TelegramBotToken}. Reson: {e.Message}");
            }

            client = null;
        }
    }
}