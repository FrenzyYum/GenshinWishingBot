# GenshinWishingBot

A Telegram chat bot that simulates Genshin Impact wishes. Built with C#, .NET 5.0 and SQLite as RDBMS. Live version can be found at [@GenshinGachaBot](https://t.me/GenshinGachaBot).

## Usage

Add this bot to your group chat and start using it right away.

- /wish - make a wish and get a randomized result, can wish once a day per chat, resets at 9PM UTC
- /inv - get your inventory in the chat
- /resetUser (bot owner only) - reply to any user's message with this command to reset his daily wish
- /resetChat (bot owner only) - send this to the chat to reset everyone's daily wishes

## Installation

Requirements: .NET SDK 5.0 and SQLite CLI.

1. Clone the repository.
2. Build the solution to restore dependencies.
3. Change the `appsettings.json` token, bot username and admin id accordingly.
4. __IMPORTANT STEP!__ Edit the  `Image` values in the `Items` table in the `main.sql` script. I am using file_id that are individual to every bot so you'll either have to get yours or use external image links. More on file_id [here](https://core.telegram.org/bots/api#sending-files). Cloud folder to the images I've used is [here](https://mega.nz/folder/310HHCIQ#Ohtq8_xdSEfyDdhj9CrU7g).
5. Generate SQLite database in the same directory the script is in (`sqlite3 main.db`, `.read main.sql`).
6. Run the project.

## License

The source code is licensed under [Mozilla Public License 2.0](https://github.com/FrenzyYum/GenshinWishingBot/blob/master/LICENSE).

The database qua database is licensed under [Open Data Commons Open Database License v1.0](https://opendatacommons.org/licenses/odbl/1-0/).

Genshin Impact content and materials are a copyright of miHoYo Co., Ltd. All rights reserved.