// SPDX-License-Identifier: MPL-2.0

using System.Data.SQLite;
using Telegram.Bot.Types;
using static TelegramUI.Startup.Config;

namespace TelegramUI.Commands
{
    public static class Admin
    {
        internal static void ResetUser(Message message)
        {
            using var con = new SQLiteConnection(MainDb());
            con.Open();

            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.Add(new SQLiteParameter("@user", message.ReplyToMessage.From.Id));
            cmd.Parameters.Add(new SQLiteParameter("@chat", message.ReplyToMessage.Chat.Id));
            
            cmd.CommandText = "UPDATE UsersInChats SET HasRolled = 0 WHERE UserId = @user AND ChatId = @chat";
            cmd.ExecuteNonQuery();
            
            con.Close();
        }
        
        internal static void ResetChat(Message message)
        {
            using var con = new SQLiteConnection(MainDb());
            con.Open();
            
            using var cmd = new SQLiteCommand(con);
            cmd.Parameters.Add(new SQLiteParameter("@chat", message.Chat.Id));
            
            cmd.CommandText = "UPDATE UsersInChats SET HasRolled = 0 WHERE ChatId = @chat";
            cmd.ExecuteNonQuery();
            
            con.Close();
        }
    }
}