using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using ForkDiscordBot.Base.Logic;
using ForkDiscordBot.Model.Config;

namespace ForkDiscordBot.Base;

public class DiscordBot : AbstractBaseComponent
{
    private AppConfig? _appConfig;
    private DiscordSocketClient? _client;
    private InteractionService _interactionService;

    public async Task Execute(string configPath)
    {
        _client = new DiscordSocketClient();
        _client.Log += Logger;

        _appConfig = await new AppConfigLogic().LoadAppConfig(configPath);

        var slashCommandLogic = new SlashCommandLogic();
        slashCommandLogic.InitializeGlobalCommands();

        await _client.LoginAsync(TokenType.Bot, _appConfig.DiscordConfig.Token);
        await _client.StartAsync();

        _client.Ready += async () =>
        {
            await slashCommandLogic.RegisterAllGlobalCommands(_client);
            foreach (var guild in _client.Guilds)
            {
                slashCommandLogic.InitializeGuildCommands(guild);
                await slashCommandLogic.RegisterAllGuildCommands(guild);
            }

            _client.SlashCommandExecuted += slashCommandLogic.HandleCommand;
        };

        _client.JoinedGuild += async guild =>
        {
            slashCommandLogic.InitializeGuildCommands(guild);
            await slashCommandLogic.RegisterAllGuildCommands(guild);
        };

        _interactionService = new InteractionService(_client.Rest);
        _interactionService.add

        await Task.Delay(Timeout.Infinite);
    }

    private Task Logger(LogMessage logMessage)
    {
        var message = logMessage.Source != null ? logMessage.Source + ": " + logMessage.Message : logMessage.Message;
        switch (logMessage.Severity)
        {
            case LogSeverity.Debug:
                Log.Debug(message, logMessage.Exception);
                break;
            case LogSeverity.Info:
                Log.Info(message, logMessage.Exception);
                break;
            case LogSeverity.Warning:
                Log.Warn(message, logMessage.Exception);
                break;
            case LogSeverity.Error:
                Log.Error(message, logMessage.Exception);
                break;
            case LogSeverity.Critical:
                Log.Fatal(message, logMessage.Exception);
                break;
        }

        return Task.CompletedTask;
    }
}