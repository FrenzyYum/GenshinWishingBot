// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using TelegramUI.Commands;
using static TelegramUI.Startup.Config;
using static TelegramUI.Commands.Language;

namespace TelegramUI.Telegram
{
    public static class TelegramCommands
    {
        internal static async void BotOnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message == null || e.Message.Type != MessageType.Text) return;
            
            var entity = JsonSerializer.Serialize(e.Message, new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            });

            try
            {
                var msg = e.Message.Text;
                if (msg.EndsWith(BotUsername()))
                {
                    msg = msg.Substring(0, msg.Length - BotUsername().Length);
                }

                if (e.Message.Text == "/get" && e.Message.From.Id.ToString() == AdminId())
                {
                    await Bot.SendTextMessageAsync(
                        e.Message.Chat,
                        entity,
                        replyToMessageId: e.Message.MessageId);
                }

                if (e.Message.Chat.Type == ChatType.Private)
                {
                    switch (e.Message.Text)
                    {
                        case "/start":
                            await Bot.SendTextMessageAsync(
                                e.Message.Chat,
                                "Use /help to see what this bot can do.");
                            break;
                        case "/help":
                            await Bot.SendTextMessageAsync(
                                e.Message.Chat,
                                "This bot is a Genshin Impact Wish Simulator that works only in the group chats. Feel free to add it to your chat (privacy mode is on so no spying on whatever conversations you may have).\n\nUse <code>/wish</code> command to make a wish. You can only wish once a day per chat. The timer resets at 9PM UTC.\nUse <code>/inv</code> command to see your inventory. Inventories are bound to the chats.\nUse <code>/lang [locale]</code> to change the language in a chat. Only chat admins can do so (make sure you're not anonymous!). Available locales: en, ru.",
                                ParseMode.Html);
                            break;
                        default:
                            if (e.Message.From.Id.ToString() == AdminId() && e.Message.ReplyToMessage != null)
                            {
                                await Bot.SendTextMessageAsync(
                                    e.Message.ReplyToMessage.ForwardFrom.Id,
                                    e.Message.Text);
                            }
                            break;
                    }
                    return;
                }

                if (e.Message.Chat.Type != ChatType.Group && e.Message.Chat.Type != ChatType.Supergroup) return;

                AddChat(e.Message);

                var texts = typeof(Wish).Assembly.GetManifestResourceStream($"TelegramUI.Strings.General.{GetLanguage(e.Message)}.json");
                var sReader = new StreamReader(texts);
                var textsText = sReader.ReadToEnd();
                sReader.Close();
                var textsList = JsonSerializer.Deserialize<List<string>>(textsText);

                switch (msg)
                {
                    case "/inv":
                        var results = Inventory.InventoryFetch(e.Message);
                        
                        while (true)
                        {
                            try
                            {
                                await Bot.SendTextMessageAsync(
                                    e.Message.Chat,
                                    string.Format(textsList[2], HttpUtility.HtmlEncode(e.Message.Chat.Title), HttpUtility.HtmlEncode(e.Message.From.FirstName), results),
                                    replyToMessageId: e.Message.MessageId,
                                    parseMode: ParseMode.Html);
                                break;
                            }
                            catch (Exception exception)
                            {
                                // ignored
                            }
                        }
                        break;
                    case "/wish":
                        if (Wish.HasRolled(e.Message) == 1)
                        {
                            // These fixed parameters depend on Main's ScheduleTask parameters, edit accordingly
                            var minuteDiff = 60 - DateTime.Now.Minute;
                            var hourDiff = 21 - DateTime.Now.Hour - 1;
                            if (minuteDiff == 60)
                            {
                                hourDiff += 1;
                                minuteDiff = 0;
                            }

                            if (hourDiff < 0)
                            {
                                hourDiff += 24;
                            }

                            if (minuteDiff == 0 && hourDiff == 0)
                            {
                                hourDiff = 24;
                            }

                            while (true)
                            {
                                try
                                {
                                    await Bot.SendTextMessageAsync(
                                        e.Message.Chat.Id,
                                        string.Format(textsList[1], hourDiff, minuteDiff),
                                        replyToMessageId: e.Message.MessageId);
                                    break;
                                }
                                catch (Exception exception)
                                {
                                    // ignored
                                }
                            }
                            
                            return;
                        }
                        
                        var pull = Wish.GetCharacterPull(e.Message);

                        while (true)
                        {
                            try
                            {
                                await Bot.SendTextMessageAsync(
                                    e.Message.Chat.Id,
                                    text: $"{pull[0]}<a href=\"{pull[1]}\">\u200b</a>",
                                    parseMode: ParseMode.Html,
                                    disableWebPagePreview: false,
                                    replyToMessageId: e.Message.MessageId);
                                break;
                            }
                            catch (Exception exception)
                            {
                                // ignored
                            }
                        }
                        break;
                    case "/resetUser":
                        if (e.Message.From.Id.ToString() != AdminId()) return;
                        Admin.ResetUser(e.Message);
                        await Bot.SendTextMessageAsync(
                            e.Message.Chat.Id,
                            $"Daily wish reset for {e.Message.ReplyToMessage.From.FirstName}!",
                            replyToMessageId: e.Message.MessageId);
                        break;
                    case "/resetChat":
                        if (e.Message.From.Id.ToString() != AdminId()) return;
                        Admin.ResetChat(e.Message);
                        await Bot.SendTextMessageAsync(
                            e.Message.Chat.Id,
                            $"Daily wish reset for everyone in {e.Message.Chat.Title}!",
                            replyToMessageId: e.Message.MessageId);
                        break;
                    default:
                        if (e.Message.Text.StartsWith("/lang "))
                        {
                            var admins = await Bot.GetChatAdministratorsAsync(e.Message.Chat.Id);
                            foreach (var cM in admins)
                            {
                                if (cM.User.Id == e.Message.From.Id)
                                {
                                    switch (msg)
                                    {
                                        case "/lang ru":
                                            ChangeLanguage(e.Message, "ru");
                                            await Bot.SendTextMessageAsync(
                                                e.Message.Chat,
                                                "Язык изменён на русский.");
                                            break;
                                        case "/lang en":
                                            ChangeLanguage(e.Message, "en");
                                            await Bot.SendTextMessageAsync(
                                                e.Message.Chat,
                                                "Language changed to English.");
                                            break;
                                    }
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception exception)
            {
                if (exception.Message.Contains("Response status code does not indicate success: 429 (Too Many Requests)"))
                {
                    return;
                }
                
                await Bot.SendTextMessageAsync(
                    AdminId(),
                    $"Error: {exception.Message}\n{exception.StackTrace}\n\nEntity: {entity}");
            }
        }
    }
}