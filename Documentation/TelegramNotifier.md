# Telegram notifier usage

1. In telegram create your bot through the bot @BotFather

2. Copy the access token, BUT DO NOT SHARE IT WITH ANYONE!

3. Add this bot to a chat, where you want it to send messages to and send 1-2 messages or, 
if you want it to send messages to you, just message the bot directly.

4. Open your browser and navigate to address 
(where <token> needs to be replaced with your bot token from step 2):
```
https://api.telegram.org/bot<token>/getUpdates
```

5. There you will find a json response string.
Look for the record with value "text":"_Text that you sent_".
In this record you will find value "id":_digits_representing_your_chat_id_"

6. Copy the chat id

7. Open config window:
Tools -> Fixer33 -> Build Finish Notifier

8. Into "Bot Token" field insert bot token from step 2.

9. Into "Chat Id" insert value from step 6

10. Two other fields are the template of text that bot will send to you upon build is completed or failed