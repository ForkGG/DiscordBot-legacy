using System.Text;
using Discord;
using Discord.WebSocket;
using ForkDiscordBot.Base.Logic;

namespace ForkDiscordBot.Base.Model.SlashCommands;

public class HelpSlashCommand : SlashCommand
{
    public HelpSlashCommand(SlashCommandLogic logic) : base(logic)
    {
    }

    public override string Name => "help";
    public override string Description => "Lists all available commands";

    public override List<SlashCommandOptionBuilder> Parameters => new();


    public override async Task Execute(SocketSlashCommand command)
    {
        await command.RespondAsync("Here is a list of all supported commands", BuildHelpResponse(command.GuildId),
            ephemeral: true);
    }

    private Embed[] BuildHelpResponse(ulong? guildId)
    {
        var sortedCommands = _logic.GetAllRegisteredGlobalCommands().ToList();

        if (guildId != null)
        {
            var guildCommands = _logic.GetAllRegisteredGuildCommands(guildId.Value);
            if (guildCommands != null) sortedCommands.AddRange(guildCommands);
        }

        sortedCommands.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

        var eb = new EmbedBuilder();
        StringBuilder names = new();
        StringBuilder descriptions = new();
        foreach (var command in sortedCommands)
        {
            var cmdName = command.Name;
            if (command.Parameters.Count > 0)
            {
                cmdName += " [";
                for (var i = 0; i < command.Parameters.Count; i++)
                {
                    cmdName += $"{command.Parameters[i].Name} ({command.Parameters[i].Description})";
                    if (i < command.Parameters.Count - 1) cmdName += " ";
                }

                cmdName += "]";
            }

            names.Append($"${cmdName}{Environment.NewLine}");
            descriptions.Append(command.Description + Environment.NewLine);
        }

        eb.AddField("Command", names.ToString(), true);
        eb.AddField("Description", descriptions.ToString(), true);

        return new[] { eb.Build() };
    }
}