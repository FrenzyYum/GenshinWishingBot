// SPDX-License-Identifier: MPL-2.0

using System;
using TelegramUI.Scheduler;
using TelegramUI.Telegram;
using static TelegramUI.Startup.Config;

namespace TelegramUI
{
    public static class Program
    {
        private static void Main()
        {
            TaskScheduler.Instance.ScheduleTask(21, 24);

            Bot.OnMessage += TelegramCommands.BotOnMessage;

            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();
        }
    }
}