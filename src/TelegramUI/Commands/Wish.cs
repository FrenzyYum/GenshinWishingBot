// SPDX-License-Identifier: MPL-2.0

using System;
using System.Data.SQLite;
using Telegram.Bot.Types;
using static TelegramUI.Startup.Config;

namespace TelegramUI.Commands
{
    public static class Wish
    {
        private static int Randomizer()
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode()).Next(1, 1000);
            return rnd switch
            {
                < 13 => 5,
                < 151 => 4,
                _ => 3
            };
        }
        
        private static string GetResult(int stars)
        {
            var result = "debateclub"; // fallback value
            
            using var con = new SQLiteConnection(MainDb());
            con.Open();
            
            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.Add(new SQLiteParameter("@stars", stars));
            
            cmd.CommandText = "SELECT Id From Items WHERE Stars = @stars ORDER BY RANDOM() LIMIT 1";
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                result = rdr.GetString(0);
            }
            
            con.Close();
            
            return result;
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
            
            var wish = GetResult(rate);
            
            // Roll result
            using var cmd2 = new SQLiteCommand(con);
            cmd2.Parameters.Add(new SQLiteParameter("@wish", wish));
            cmd2.CommandText = "SELECT * From Items WHERE Id = @wish";
            using var rdr2 = cmd2.ExecuteReader();
            while (rdr2.Read())
            {
                result[0] = $"{rdr2.GetString(4)}\n\n{message.From.FirstName}, you have received {rdr2.GetString(1)}!\n\n{rdr2.GetString(1)} is a {rdr2.GetInt32(3)}-star {rdr2.GetString(2)}.";
                result[1] = rdr2.GetString(5);
            }
            
            using var cmd3 = new SQLiteCommand(con);
            cmd3.Parameters.Add(new SQLiteParameter("@wish", wish));
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
            switch (rate)
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
