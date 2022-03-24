using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace TelegramUI.Startup
{
    public static class Config
    {
        private static IConfigurationRoot Configuration() => new ConfigurationBuilder()
            .AddJsonFile("Startup/appsettings.json", false, false)
            .AddJsonFile("Startup/appsettings.local.json", true, false)
            .Build();

        private static string Token() => Configuration()["Telegram:Token"];
        
        public static string BotUsername() => Configuration()["Telegram:BotUsername"];
        public static string AdminId() => Configuration()["Telegram:AdminId"];
        public static string MainDb() => Configuration()["ConnectionStrings:MainDb"];
        
        public static readonly ITelegramBotClient Bot = new TelegramBotClient(Token());
    }
}
