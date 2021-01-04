using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Fleck;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


    public class Discord_Bot
    {
    public static List<IWebSocketConnection> allSockets { get; set; } = new List<IWebSocketConnection>();
    public static async Task StartAsync()
        {
            await new Discord_Bot().RunAsync();
        }

        private async Task RunAsync()
        {
        ILog logger = LogManager.GetLogger(typeof(FleckLog));

        FleckLog.LogAction = (level, message, ex) => {
            switch (level)
            {
                case LogLevel.Debug:
                    logger.Debug(message, ex);
                    break;
                case LogLevel.Error:
                    logger.Error(message, ex);
                    break;
                case LogLevel.Warn:
                    logger.Warn(message, ex);
                    break;
                default:
                    logger.Info(message, ex);
                    break;
            }
        };
        var server = new WebSocketServer("ws://0.0.0.0:8181");
        server.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                Console.WriteLine("Open!");
                allSockets.Add(socket);
            };
            socket.OnClose = () =>
            {
                Console.WriteLine("Close!");
                allSockets.Remove(socket);
            };
            socket.OnMessage = message =>
            {
                Console.WriteLine(message);
                allSockets.ToList().ForEach(s => s.Send(message));
             
            };
        });


       
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
             var input = Console.ReadLine();
        while (input != "exit")
        {
            foreach (var socket in allSockets.ToList())
            {
                socket.Send(input);
            }
            input = Console.ReadLine();
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
