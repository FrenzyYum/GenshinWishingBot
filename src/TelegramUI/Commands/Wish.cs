// SPDX-License-Identifier: MPL-2.0

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Web;
using Telegram.Bot.Types;
using TelegramUI.Strings.Items;
using static TelegramUI.Startup.Config;
using static TelegramUI.Commands.Language;

namespace TelegramUI.Commands
{
    public static class Wish
    {
        private static int Randomizer()
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode()).Next(1, 1000);
            return rnd switch
            {
                < 12 => 5,
                < 141 => 4,
                _ => 3
            };
        }

        internal static string[] GetCharacterPull(Message message)
        {
            var result = new string[2];
            var rate = Randomizer();
            
            using var con = new SQLiteConnection(MainDb());
            con.Open();
            
            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.Add(new SQLiteParameter("@user", message.From.Id));
            cmd.Parameters.Add(new SQLiteParameter("@chat", message.Chat.Id));
            
            // Check if the user hit pity counter
            cmd.CommandText = "SELECT FourPity, FivePity From UsersInChats WHERE UserId = @user AND ChatId = @chat";
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                if (rdr.GetInt32(0) >= 9)
                {
                    rate = 4;
                }
                if (rdr.GetInt32(1) >= 74)
                {
                    rate = 5;
                }
            }
            
            var items = typeof(Wish).Assembly.GetManifestResourceStream($"TelegramUI.Strings.Items.{GetLanguage(message)}.json");
            var sR = new StreamReader(items);
            var itemsText = sR.ReadToEnd();
            sR.Close();
            
            var itemsList = JsonSerializer.Deserialize<List<Items>>(itemsText);
            
            var filteredList = itemsList.Where(x => x.Stars == rate).ToList();
            var rnd = new Random(Guid.NewGuid().GetHashCode()).Next(filteredList.Count);
            var wish = filteredList[rnd];

            using var cmd3 = new SQLiteCommand(con);
            cmd3.Parameters.Add(new SQLiteParameter("@wish", wish.Id));
            cmd3.Parameters.Add(new SQLiteParameter("@user", message.From.Id));
            cmd3.Parameters.Add(new SQLiteParameter("@chat", message.Chat.Id));
            
            // Adding user to a DB if it doesn't exist
            cmd3.CommandText = "INSERT OR IGNORE INTO UsersInChats(UserId, ChatId) VALUES(@user, @chat)";
            cmd3.ExecuteNonQuery();
            
            // Update that user has rolled in the chat today
            cmd3.CommandText = "UPDATE UsersInChats SET HasRolled = 1 WHERE UserId = @user AND ChatId = @chat";
            cmd3.ExecuteNonQuery();
            
            // Adding the item to the user's inventory
            cmd3.CommandText = "INSERT OR IGNORE INTO InventoryItems(UserId, ChatId, ItemId) VALUES(@user, @chat, @wish)";
            cmd3.ExecuteNonQuery();
            
            // Updating item count in user's inventory
            cmd3.CommandText = "UPDATE InventoryItems SET Count = Count + 1 WHERE UserId = @user AND ChatId = @chat AND ItemId = @wish";
            cmd3.ExecuteNonQuery();

            // Increment the pity counter based on wish star result
            switch (wish.Stars)
            {
                case 3:
                    cmd3.CommandText = "UPDATE UsersInChats SET FourPity = FourPity + 1, FivePity = FivePity + 1 WHERE UserId = @user AND ChatId = @chat";
                    cmd3.ExecuteNonQuery();
                    break;
                case 4:
                    cmd3.CommandText = "UPDATE UsersInChats SET FourPity = 0, FivePity = FivePity + 1 WHERE UserId = @user AND ChatId = @chat";
                    cmd3.ExecuteNonQuery();
                    break;
                case 5:
                    cmd3.CommandText = "UPDATE UsersInChats SET FivePity = 0, FourPity = FourPity + 1 WHERE UserId = @user AND ChatId = @chat";
                    cmd3.ExecuteNonQuery();
                    break;
            }

            con.Close();
            
            var texts = typeof(Wish).Assembly.GetManifestResourceStream($"TelegramUI.Strings.General.{GetLanguage(message)}.json");
            var sReader = new StreamReader(texts);
            var textsText = sReader.ReadToEnd();
            sReader.Close();
            var textsList = JsonSerializer.Deserialize<List<string>>(textsText);
            
            result[0] = string.Format(textsList[0], wish.Description, HttpUtility.HtmlEncode(message.From.FirstName), wish.Name, wish.Stars, wish.Type);

            result[1] =
                $"https://i0.wp.com/raw.githubusercontent.com/FrenzyYum/GenshinWishingBot/master/assets/images/{wish.Id}.webp";
            
            if (wish.Id is "barbara" or "jean")
            {
                result[1] = $"https://i0.wp.com/raw.githubusercontent.com/FrenzyYum/GenshinWishingBot/master/assets/images/{wish.Id}-summer.webp";
            }
            
            if (wish.Id is "keqing" or "ningguang")
            {
                result[1] = $"https://i0.wp.com/raw.githubusercontent.com/FrenzyYum/GenshinWishingBot/master/assets/images/{wish.Id}-lanternrite.webp";
            }
            
            return result;
        }
        
        internal static int HasRolled(Message message)
        {
            var result = 0; //fallback value
            
            using var con = new SQLiteConnection(MainDb());
            con.Open();
            
            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.Add(new SQLiteParameter("@user", message.From.Id));
            cmd.Parameters.Add(new SQLiteParameter("@chat", message.Chat.Id));
            
            cmd.CommandText = "SELECT HasRolled FROM UsersInChats WHERE UserId = @user AND ChatId = @chat";
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                result = rdr.GetInt32(0);
            }
            
            con.Close();
            
            return result;
        }
    }
}
