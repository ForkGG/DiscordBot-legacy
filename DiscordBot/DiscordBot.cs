using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


    public class Fork_Bot
    {
        public static async Task StartAsync()
        {
            await new Fork_Bot().RunAsync();
        }

        private async Task RunAsync()
        {
            var config = BuildConfig();
            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();
                services.GetRequiredService<LogService>();
                await client.LoginAsync(TokenType.Bot, config["token"]);
                await client.StartAsync();
                await services.GetRequiredService<CommandHandler>().InitializeAsync();
                await Task.Delay(Timeout.Infinite);
            }
        }

        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("config.json").Build();
        }

        private ServiceProvider ConfigureServices()
        {
            var collection = new ServiceCollection();
            collection.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig() { MessageCacheSize = 50, AlwaysDownloadUsers = true, ExclusiveBulkDelete = true, LogLevel = LogSeverity.Verbose, GatewayIntents = GatewayIntents.DirectMessageReactions | GatewayIntents.DirectMessages | GatewayIntents.GuildBans | GatewayIntents.GuildInvites | GatewayIntents.GuildMembers | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMessages | GatewayIntents.Guilds | GatewayIntents.GuildIntegrations })); // , .TotalShards = 3}))
            collection.AddSingleton(new CommandService(new CommandServiceConfig() { LogLevel = LogSeverity.Verbose, DefaultRunMode = RunMode.Async }));
            collection.AddSingleton<CommandHandler>();
            collection.AddSingleton<LogService>();
            collection.AddSingleton<InteractiveService>();
            return collection.BuildServiceProvider();
        }
    }
