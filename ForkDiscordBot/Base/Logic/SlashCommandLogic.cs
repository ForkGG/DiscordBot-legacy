using System.Reflection;
using Discord;
using Discord.WebSocket;
using ForkDiscordBot.Base.Exception;
using ForkDiscordBot.Base.Model.Emotes;
using ForkDiscordBot.Base.Model.SlashCommands;

namespace ForkDiscordBot.Base.Logic;

/// <summary>
///     This class is used instead of the generic annotation bases method from the Discord.NET package, so we can
///     individually add/remove commands from guilds depending on the availability of a command
///     <br />
///     example: A guild that is not connected to a Fork instance can not use Server-Managing commands like "start"
/// </summary>
public class SlashCommandLogic : AbstractLogic
{
    private readonly Dictionary<string, SlashCommand> _globalCommands = new();
    private readonly Dictionary<ulong, Dictionary<string, GuildSlashCommand>> _guildCommands = new();

    public void InitializeGlobalCommands()
    {
        foreach (var slashCommandType in GetGlobalSlashCommandTypes())
        {
            var command = Activator.CreateInstance(slashCommandType, this) as SlashCommand;
            if (command == null)
                throw new SlashCommandException("Failed to create instance of following slash-command: " +
                                                nameof(slashCommandType));
            _globalCommands.Add(command.Name, command);
        }
    }

    public void InitializeGuildCommands(SocketGuild guild)
    {
        Dictionary<string, GuildSlashCommand> commands = new();
        foreach (var slashCommandType in GetGuildSlashCommandTypes())
        {
            var command = Activator.CreateInstance(slashCommandType, this, guild) as GuildSlashCommand;
            if (command == null)
                throw new SlashCommandException("Failed to create instance of following slash-command: " +
                                                nameof(slashCommandType));
            commands.Add(command.Name, command);
        }

        if (_guildCommands.ContainsKey(guild.Id))
            // Overwrite if already present
            _guildCommands.Remove(guild.Id);
        _guildCommands.Add(guild.Id, commands);
    }

    public IEnumerable<SlashCommand> GetAllRegisteredGlobalCommands()
    {
        return _globalCommands.Values;
    }

    public IEnumerable<SlashCommand>? GetAllRegisteredGuildCommands(ulong guildId)
    {
        _guildCommands.TryGetValue(guildId, out var guildCommands);
        return guildCommands?.Values;
    }

    public async Task RegisterAllGlobalCommands(DiscordSocketClient client)
    {
        foreach (var command in _globalCommands.Values)
        {
            var commandBuilder = new SlashCommandBuilder()
                .WithName(command.Name)
                .WithDescription(command.Description)
                .AddOptions(command.Parameters.ToArray());
            // TODO CKE Permissions
            await client.CreateGlobalApplicationCommandAsync(commandBuilder.Build());
        }
    }

    public async Task RegisterAllGuildCommands(SocketGuild guild)
    {
        if (!_guildCommands.ContainsKey(guild.Id)) InitializeGuildCommands(guild);
        foreach (var command in _guildCommands[guild.Id].Values)
        {
            var commandBuilder = new SlashCommandBuilder()
                .WithName(command.Name)
                .WithDescription(command.Description)
                .AddOptions(command.Parameters.ToArray());
            // TODO CKE Permissions
            await guild.CreateApplicationCommandAsync(commandBuilder.Build());
        }
    }

    public async Task HandleCommand(SocketSlashCommand command)
    {
        try
        {
            if (_globalCommands.ContainsKey(command.CommandName))
            {
                // Handle global command
                var slashCommand = _globalCommands[command.CommandName];
                await slashCommand.Execute(command);
            }
            else if (command.GuildId.HasValue && _guildCommands.ContainsKey(command.GuildId.Value) &&
                     _guildCommands[command.GuildId.Value].ContainsKey(command.CommandName))
            {
                // Handle guild command
                var guildSlashCommand = _guildCommands[command.GuildId.Value][command.CommandName];
                await guildSlashCommand.Execute(command);
            }
            else
            {
                await command.RespondAsync("Command is not supported");
            }
        }
        catch (System.Exception e)
        {
            if (!command.HasResponded)
                // If any exception occurs that is not handles send a generic response
                await command.RespondAsync(
                    $"{ForkEmotes.RedCircle} Sorry an error occured while processing the command. Please contact the developers or try again",
                    ephemeral: true);
            Log.Error(e);
        }
    }

    private IEnumerable<Type> GetGlobalSlashCommandTypes()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(SlashCommand)) && !t.IsSubclassOf(typeof(GuildSlashCommand)))
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .ToList();
    }

    private IEnumerable<Type> GetGuildSlashCommandTypes()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(GuildSlashCommand)))
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .ToList();
    }
}