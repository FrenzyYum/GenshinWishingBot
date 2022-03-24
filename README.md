# GenshinWishingBot

A Telegram chat bot that simulates Genshin Impact wishes. Built with C#, .NET 5.0 and SQLite as RDBMS.

## Usage

Add this bot to your group chat and start using it right away.

- /wish - make a wish and get a randomized result, can wish once a day per chat, resets at 9PM UTC
- /inv - get your inventory in the chat
- /resetUser (bot owner only) - reply to any user's message with this command to reset his daily wish
- /resetChat (bot owner only) - send this to the chat to reset everyone's daily wishes
- /lang [code] (chat admins only) - change the locale for a specific chat, e.g. `/lang en`

## Installation

Requirements: .NET SDK 5.0

1. Clone the repository.
2. Build the solution to restore dependencies.
3. Change the `appsettings.json` token, bot username and admin id accordingly.
4. Set links to images (see [here](https://github.com/FrenzyYum/GenshinWishingBot/blob/master/src/TelegramUI/Commands/Wish.cs#L114))
5. Run the project.

## License

The source code is licensed under Mozilla Public License 2.0.

Genshin Impact content and materials are a copyright of miHoYo Co., Ltd. No copyright infringement intended.