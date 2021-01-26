# Fork DiscordBot

This Discord Bot is used to connect a Fork instance to a Discord server for remote controlling and event updates.

## How to use
This bot is hosted on a public server and is used by every Fork user, if `Enable Discord Bot` is enabled in the Fork settings.

If you run Fork in `DEBUG` mode it will look for this running locally.

## Building and Running
To build and run this code please follow these steps:
 1. Clone/fork this repository
 2. Add Discord.NET beta to nuget package sources: https://www.myget.org/F/discord-net/api/v3/index.json
 3. Add a copy of the [config.json](https://github.com/ForkGG/DiscordBot/blob/main/config.json) to the `DiscordBot` directory and add the token of a Discord bot created [here](https://discord.com/developers/applications)
 4. The project should now be able to build and run. To test it you should also run the [Fork project](https://github.com/ForkGG/Fork) in `DEBUG` mode on the same machine
 
 ## Credits
 - Thanks to [Alpha-c0d3r | Illusion](https://github.com/Alpha-c0d3r) for coding most of this project
 - Thanks to the [Patrons](https://www.patreon.com/forkgg) helping to cover the hosting costs for this Bot
