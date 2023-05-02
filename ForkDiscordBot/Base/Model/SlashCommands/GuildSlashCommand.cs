using Discord.WebSocket;
using ForkDiscordBot.Base.Logic;

namespace ForkDiscordBot.Base.Model.SlashCommands;

public abstract class GuildSlashCommand : SlashCommand
{
    protected SocketGuild _guild;

    protected GuildSlashCommand(SlashCommandLogic logic, SocketGuild guild) : base(logic)
    {
        _guild = guild;
    }
}