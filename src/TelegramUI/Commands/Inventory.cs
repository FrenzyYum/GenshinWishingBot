// SPDX-License-Identifier: MPL-2.0

using System.Collections.Generic;
using System.Data.SQLite;
using Telegram.Bot.Types;
using static TelegramUI.Startup.Config;

namespace TelegramUI.Commands
{
    public static class Inventory
    {
        internal static string InventoryFetch(Message message)
        {
            var result = new string[3];
            var resultArray = new string[3];
            var itemStarCount = new int[3];
            
            // 5 Stars
            result[0] = "";
            itemStarCount[0] = 0;
            // 4 Stars
            result[1] = "";
            itemStarCount[1] = 0;
            // 3 Stars
            result[2] = "";
            itemStarCount[2] = 0;

            var resultCharacters = new string[3];
            var resultWeapons = new string[3];

            var itemIds = new List<string>();
            var countIds = new List<int>();

            using var con = new SQLiteConnection(MainDb());
            con.Open();
            
            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.Add(new SQLiteParameter("@userId", message.From.Id));
            cmd.Parameters.Add(new SQLiteParameter("@chatId", message.Chat.Id));
            
            // Getting user's inventory IDs and count
            cmd.CommandText = "SELECT ItemId, Count FROM InventoryItems WHERE UserId = @userId AND ChatId = @chatId";
            using var rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                itemIds.Add(rdr.GetString(0));
                countIds.Add(rdr.GetInt32(1));
            }
            
            using var cmd2 = new SQLiteCommand(con);

            // Linking item IDs to actual data
            for (var i = 0; i <= itemIds.Count - 1; i++)
            {
               cmd2.Parameters.Add(new SQLiteParameter("@itemId", itemIds[i]));
                cmd2.CommandText = "SELECT Name, Stars, Type FROM Items WHERE Id = @itemId";
                using var rdr2 = cmd2.ExecuteReader();
                while (rdr2.Read())
                {
                    switch (rdr2.GetInt32(1))
                    {
                        case 5:
                            switch (rdr2.GetString(2))
                            {
                                case "Character":
                                    resultCharacters[0] += $"{rdr2.GetString(0)} x{countIds[i]}, ";
                                    itemStarCount[0]+=countIds[i];
                                    break;
                                case "Weapon":
                                    resultWeapons[0] += $"{rdr2.GetString(0)} x{countIds[i]}, ";
                                    itemStarCount[0]+=countIds[i];
                                    break;
                            }
                            break;
                        case 4:
                            switch (rdr2.GetString(2))
                            {
                                case "Character":
                                    resultCharacters[1] += $"{rdr2.GetString(0)} x{countIds[i]}, ";
                                    itemStarCount[1] += countIds[i];
                                    break;
                                case "Weapon":
                                    resultWeapons[1] += $"{rdr2.GetString(0)} x{countIds[i]}, ";
                                    itemStarCount[1] += countIds[i];
                                    break;
                            }
                            break;
                        case 3:
                            switch (rdr2.GetString(2))
                            {
                                case "Character":
                                    resultCharacters[2] += $"{rdr2.GetString(0)} x{countIds[i]}, ";
                                    itemStarCount[2] += countIds[i];
                                    break;
                                case "Weapon":
                                    resultWeapons[2] += $"{rdr2.GetString(0)} x{countIds[i]}, ";
                                    itemStarCount[2] += countIds[i];
                                    break;
                            }
                            break;
                    }
                } 
            }
            
            con.Close();

            result[0] = resultCharacters[0] + resultWeapons[0];
            result[1] = resultCharacters[1] + resultWeapons[1];
            result[2] = resultCharacters[2] + resultWeapons[2];

            if (result[0] != "")
            {
                resultArray[0] = $"\U00002b50\U00002b50\U00002b50\U00002b50\U00002b50 ({itemStarCount[0]})\n{result[0]}";
                resultArray[0] = resultArray[0].Substring(0,  resultArray[0].Length - 2) + "\n\n";
            }
            if (result[1] != "")
            {
                resultArray[1] = $"\U00002b50\U00002b50\U00002b50\U00002b50 ({itemStarCount[1]})\n{result[1]}";
                resultArray[1] = resultArray[1].Substring(0,  resultArray[1].Length - 2) + "\n\n";
            }
            if (result[2] != "")
            {
                resultArray[2] = $"\U00002b50\U00002b50\U00002b50 ({itemStarCount[2]})\n{result[2]}";
                resultArray[2] = resultArray[2].Substring(0,  resultArray[2].Length - 2) + "\n\n";
            }

            var results = resultArray[0] + resultArray[1] + resultArray[2];
            
            if (results == "")
            {
                results = "It's empty! There's nothing here!";
            }
            
            return results;
        }
    }
}