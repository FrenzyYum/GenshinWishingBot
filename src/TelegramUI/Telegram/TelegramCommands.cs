// SPDX-License-Identifier: MPL-2.0

using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using TelegramUI.Commands;
using static TelegramUI.Startup.Config;

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
                                "This bot is a Genshin Impact Wish Simulator that works only in the group chats. Feel free to add it to your chat (privacy mode is on so no spying on whatever conversations you may have).\n\nUse /wish command to make a wish. You can only wish once a day per chat. The timer resets at 9PM UTC.\nUse /inv command to see your inventory. Inventories are bound to the chats.\n\n/about - source code");
                            break;
                        
                        case "/about":
                            await Bot.SendTextMessageAsync(
                                e.Message.Chat,
                                "The source code is available on <a href=\"https://github.com/FrenzyYum/GenshinWishingBot\">GitHub</a>.",
                                ParseMode.Html);
                            break;
                    }
                    return;
                }

                if (e.Message.Chat.Type != ChatType.Group && e.Message.Chat.Type != ChatType.Supergroup) return;

                switch (msg)
                {
                    case "/inv":
                        var results = Inventory.InventoryFetch(e.Message);
                        await Bot.SendTextMessageAsync(
                            e.Message.Chat,
                            $"Here's your inventory in {e.Message.Chat.Title}, {e.Message.From.FirstName}!\n\n{results}",
                            replyToMessageId: e.Message.MessageId);
                        break;
                    
                    case "/wish":
                        if (Wish.HasRolled(e.Message) == 1)
                        {
                            // These fixed parameters depend on Main's ScheduleTask parameters
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

                            await Bot.SendTextMessageAsync(
                                e.Message.Chat.Id,
                                $"You've already wished today! You can wish again in {hourDiff} hours and {minuteDiff} minutes.",
                                replyToMessageId: e.Message.MessageId);
                            return;
                        }

                        var pull = Wish.GetCharacterPull(e.Message);
                        await Bot.SendPhotoAsync(
                            e.Message.Chat.Id,
                            photo: pull[1],
                            caption: pull[0],
                            replyToMessageId: e.Message.MessageId);
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
                }
            }
            catch (Exception exception)
            {
                await Bot.SendTextMessageAsync(
                    AdminId(),
                    $"Error: {exception.Message}\n\nEntity: {entity}");
            }
        }
    }
}