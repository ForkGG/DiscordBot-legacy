using Discord;
using Discord.WebSocket;
using ForkDiscordBot.Base.Logic;

namespace ForkDiscordBot.Base.Model.SlashCommands;

public abstract class SlashCommand
{
    protected SlashCommandLogic _logic;

    public SlashCommand(SlashCommandLogic logic)
    {
        _logic = logic;
    }


    public abstract string Name { get; }
    public abstract string Description { get; }

    public abstract List<SlashCommandOptionBuilder> Parameters { get; }


    public abstract Task Execute(SocketSlashCommand command);
}