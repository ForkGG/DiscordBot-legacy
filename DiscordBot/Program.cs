using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Models;
using Fleck;
using Interactivity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBot
{
     class Program
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public static List<IWebSocketConnection> allSockets { get; set; } = new List<IWebSocketConnection>();
        /// <summary>Connected Tokens
        /// </summary>
        public static List<string> AliveTokens { get; set; } = new List<string>();

        public static void Main()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine(@"
███████╗░█████╗░██████╗░██╗░░██╗  ██████╗░░█████╗░████████╗
██╔════╝██╔══██╗██╔══██╗██║░██╔╝  ██╔══██╗██╔══██╗╚══██╔══╝
█████╗░░██║░░██║██████╔╝█████═╝░  ██████╦╝██║░░██║░░░██║░░░
██╔══╝░░██║░░██║██╔══██╗██╔═██╗░  ██╔══██╗██║░░██║░░░██║░░░
██║░░░░░╚█████╔╝██║░░██║██║░╚██╗  ██████╦╝╚█████╔╝░░░██║░░░
╚═╝░░░░░░╚════╝░╚═╝░░╚═╝╚═╝░░╚═╝  ╚═════╝░░╚════╝░░░░╚═╝░░░
---------------------------------------------------------------");
            Console.WriteLine("- Checking if database exists ..");
            if (File.Exists(@"Fork.db"))
            {
                Console.WriteLine("- Exists.");
            }
            else
            {
                using (var context = new DatabaseContext())
                {
                    context.Database.EnsureCreated();
                }
                Console.WriteLine("- Created.");
            }
            Console.WriteLine("- Migrating Database");
            using (var context = new DatabaseContext())
            {
                context.Database.Migrate();
            }

            try
            {
                if (File.Exists(@"logs.log"))
                {
                    File.Delete(@"Logs.log");
                }
            }
            catch (Exception xxxx)
            {

            }

            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "nlogs.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");        
            config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
            config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logfile);        
            NLog.LogManager.Configuration = config;

            Console.WriteLine("- Done.");
            new Program().MainAsync().GetAwaiter().GetResult();
        }
        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("config.json").Build();
        }
        public async Task MainAsync()
        {
            var config = BuildConfig();
            using (var services = ConfigureServices())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();
                services.GetRequiredService<LogService>();

                await client.LoginAsync(TokenType.Bot, config["token"]);
                await client.StartAsync();
                await services.GetRequiredService<CommandHandler>().InitializeAsync();
                await Task.Run(async () => await GetReadyWS());
                await Task.Delay(Timeout.Infinite);
            }
        }
        private async Task GetReadyWS()
        {
           
           
            if (KKK.IsClientReady != true)
            {
                Console.WriteLine("Waiting for client to be ready..."); //Dont start events until discord api is ready
            }

            do
            {
                // nothing, just pauses the tasks until discord is ready
            } while (KKK.IsClientReady != true);

            Console.WriteLine("It's ready, lets start, shall we?");
            DatabaseContext context = new DatabaseContext();
#if RELEASE
        var server = new WebSocketServer("wss://0.0.0.0:8181");
        var config = BuildConfig();
        server.Certificate = new X509Certificate2(config["certPath"], config["certPassword"]);
#elif DEBUG
            var server = new WebSocketServer("ws://0.0.0.0:8181");
#endif
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    if (allSockets.Any(client =>
                        client.ConnectionInfo.ClientIpAddress == socket.ConnectionInfo.ClientIpAddress))
                    {
                        var socket2 = allSockets.Find(client =>
                            client.ConnectionInfo.ClientIpAddress == socket.ConnectionInfo.ClientIpAddress);
                        try
                        {
                            allSockets.Remove(socket2);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex.ToString(), "An error occured.");
                        } //Little security, dont let same ip to connect twice

                        allSockets.Add(socket);
                    }
                    else
                    {
                        try
                        {
                            allSockets.Remove(socket);
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex.ToString(), "An error occured.");
                        }

                        allSockets.Add(socket);
                    }
                };
                socket.OnClose = () =>
                {
                    Console.WriteLine("Closed connection: " + socket.ConnectionInfo.ClientIpAddress);
                    allSockets.Remove(socket);
                     if (context.Auth.Any(o => o.IP == socket.ConnectionInfo.ClientIpAddress))
                {
                         string token = context.Auth.AsQueryable().Where(a => a.IP == socket.ConnectionInfo.ClientIpAddress).Single().Token;
                        if (AliveTokens.Contains(token))
                        {
                            AliveTokens.Remove(token);
                            logger.Info($"{socket.ConnectionInfo.ClientIpAddress} Removed from alive tokens list");
                        }
                    }
                  
                };
                socket.OnMessage = message =>
                {
                    Task.Run(async () =>
                    {
                        try
                        {
#if DEBUG
                            Console.WriteLine($"Message Received: {message}");
#endif
                            string[] codes = message.Split('|');
                            switch (codes[0])
                            {
                                case "login":
                                    string token = codes[1];
                                     if (context.Auth.Any(o => o.Token == token))
                                    { 

                                        if (!AliveTokens.Contains(context.Auth.AsQueryable().Where(a => a.IP == socket.ConnectionInfo.ClientIpAddress).Single().Token))
                                        {
                                            try
                                            {
                                                AliveTokens.Add(token);
                                                Console.WriteLine(
                                                    $"{socket.ConnectionInfo.ClientIpAddress}:{token} Added to alive tokens list");
#if DEBUG
                                                Console.WriteLine("Here is Serverid: " +
                                                                context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid);
#endif
                                                if (KKK.Client.GetGuild(context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid) != null)
                                                {
                                                    var guild = KKK.Client.GetGuild(
                                                        context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid);
                                                    await socket.Send($"status|Linked|{guild.Name}");
                                                }
                                                else
                                                {
                                                    await socket.Send($"status|Linked|null");
                                                }
                                                if (context.Notify.Any(o => o.Serverid == context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid))
                                                {
                                                    await socket.Send($"subscribe|playerEvent");
                                                }
                                                if (context.OnJoin.Any(o => (o.Serverid == context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid) && o.sevent == 1))
                                                {
                                                    await socket.Send($"subscribe|serverListEvent");
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                logger.Warn(ex.ToString(), "An error occured.");
                                            }
                                        }
                                    }
                                    else if (!(context.Auth.Any(o => o.Token == token))) 
                                    {
                                        if (!(context.OnHold.Any(o => o.IP == socket.ConnectionInfo.ClientIpAddress)))
                                        {
                                               using (var contextt = new DatabaseContext())
                                        {
                                             OnHold newonhold = new OnHold();
                                                newonhold.IP = socket.ConnectionInfo.ClientIpAddress;
                                                newonhold.Token = token;
                                                contextt.Add(newonhold);
                                                contextt.SaveChanges();
                                        }
                                        } else if ((context.OnHold.Any(o => o.IP == socket.ConnectionInfo.ClientIpAddress)))
                                        {
                                             var onhold = context.OnHold.First(a => a.IP == socket.ConnectionInfo.ClientIpAddress);
                                            onhold.Token = token;
                                            context.SaveChanges();
                                        }
                                        Console.WriteLine("Token: " + token +
                                                          $" IP: {socket.ConnectionInfo.ClientIpAddress} Added to onhold list");
                                        await socket.Send("status|OnHold");
                                    }

                                    break;
                                case "notify":
                                    
                                    if (AliveTokens.Contains(context.Auth.AsQueryable().Where(a => a.IP == socket.ConnectionInfo.ClientIpAddress).Single().Token))
                                    {
                                        await Task.Run(async () =>
                                        {
                                            try
                                            {
                                                string servername = codes[1];
                                                string discordname = codes[2];
                                                string channelid = codes[3];
                                                string messageid = codes[4];
                                                string eventt = codes[5];
                                                string result = codes[6];

                                                switch (eventt)
                                                {
                                                    case "stop":

                                                        switch (result)
                                                        {
                                                            //notify|{servername}|{discordname}|{channelid}|{messageid}|{eventt}|400|this is a test
                                                            case "20": //ok
                                                                await BotTools.NotificationControlAsync(
                                                                    ulong.Parse(messageid), ulong.Parse(channelid),
                                                                    $"Your `{servername}` stopped successfully, command was executed by `{discordname}`",
                                                                    int.Parse(result));
                                                                break;

                                                            case "40": //its stopped already 
                                                                await BotTools.NotificationControlAsync(
                                                                    ulong.Parse(messageid), ulong.Parse(channelid),
                                                                    $"Your `{servername}` is stopped already, command was executed by `{discordname}`",
                                                                    int.Parse(result));
                                                                break;

                                                            case "44": //server not found
                                                                await BotTools.NotificationControlAsync(
                                                                    ulong.Parse(messageid), ulong.Parse(channelid),
                                                                    $"I couldnt find your `{servername}` server, please make sure you typed the right name, command was executed by `{discordname}`",
                                                                    int.Parse(result));
                                                                break;
                                                        }

                                                        break;

                                                    case "start":

                                                        switch (result)
                                                        {
                                                            //notify|{servername}|{discordname}|{channelid}|{messageid}|{eventt}|400|this is a test
                                                            case "20": //ok
                                                                await BotTools.NotificationControlAsync(
                                                                    ulong.Parse(messageid), ulong.Parse(channelid),
                                                                    $"Your `{servername}` server is starting.. , command was executed by `{discordname}`",
                                                                    int.Parse(result));
                                                                break;
                                                            case "21": //ok
                                                                await BotTools.NotificationControlAsync(
                                                                    ulong.Parse(messageid), ulong.Parse(channelid),
                                                                    $"Your `{servername}` started successfully, command was executed by `{discordname}`",
                                                                    int.Parse(result));
                                                                break;
                                                            case "40": //its stopped already 
                                                                await BotTools.NotificationControlAsync(
                                                                    ulong.Parse(messageid), ulong.Parse(channelid),
                                                                    $"Your `{servername}` is running already, command was executed by `{discordname}`",
                                                                    int.Parse(result));
                                                                break;

                                                            case "44": //server not found
                                                                await BotTools.NotificationControlAsync(
                                                                    ulong.Parse(messageid), ulong.Parse(channelid),
                                                                    $"I couldnt find your `{servername}` server, please make sure you typed the right name, command was executed by `{discordname}`",
                                                                    int.Parse(result));
                                                                break;
                                                        }

                                                        break;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                logger.Warn($"Couldn't process request: {ex.Message}");
                                            }
                                        });
                                    }

                                    break;
                                case "event":
                                    await Task.Run(async () =>
                                    {
                                        string eventname = codes[1];
                                        string servername = codes[2];
                                        string playername = codes[3];
                                        try
                                        {
                                            string token = context.Auth.AsQueryable().Where(a => a.IP == socket.ConnectionInfo.ClientIpAddress).Single().Token;
                                            if (AliveTokens.Contains(token))
                                            {
                                                if (KKK.Client.GetGuild(context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid) != null)
                                                {
                                                    if (eventname.StartsWith("p")
                                                        && context.Notify.AsQueryable().Any(x => x.Serverid == context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid)
                                                        && KKK.Client.GetGuild(context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid)
                                                            .GetChannel(
                                                                context.Notify.AsQueryable().Where(a => a.Serverid == context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid).Single().Channelid) != null)
                                                    {
                                                        switch (eventname)
                                                        {
                                                            case "pjoin":
                                                                await BotTools.NotificationControlAsync(0,
                                                                   context.Notify.AsQueryable().Where(a => a.Serverid == context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid).Single().Channelid,
                                                                    $"***{playername}*** Just joined ***{servername}***", 0,
                                                                    1);
                                                                break;
                                                            case "pleave":
                                                                await BotTools.NotificationControlAsync(0,
                                                                context.Notify.AsQueryable().Where(a => a.Serverid == context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid).Single().Channelid,
                                                                    $"***{playername}*** Just left ***{servername}***", 0,
                                                                    1);
                                                                break;
                                                        }
                                                    }
                                                    else if (eventname.StartsWith("serverList") &&
                                                            context.OnJoin.Any(o => (o.Serverid == context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid) && o.sevent == 1)
                                                             && KKK.Client
                                                                 .GetGuild(context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid)
                                                                 .GetChannel(
                                                                 context.OnJoin.AsQueryable().Where(a => a.Serverid == context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid).Single().Channelid) !=
                                                             null)
                                                    {
                                                        await Task.Run(async () =>
                                                        {
                                                            try
                                                            {
                                                                if (AliveTokens.Contains(token) &&
                                                                 (context.OnJoin.Any(o => (o.Serverid == context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid) && o.sevent == 1)))
                                                                {
                                                                    ulong guild = context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid;
                                                                    if (KKK.Client.GetGuild(guild)
                                                                        .GetChannel(
                                                                            context.OnJoin.AsQueryable().Where(a => a.Serverid == context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid).Single().Channelid) != null)
                                                                    {
                                                                        if (context.OnJoin.AsQueryable().Where(a => a.Serverid == context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid).Single().Messageid != 0)
                                                                        {
                                                                            IMessageChannel chan =
                                                                                (IMessageChannel)KKK.Client.GetChannel(context.OnJoin.AsQueryable().Where(a => a.Serverid == context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid).Single().Channelid);
                                                                            IUserMessage msg =
                                                                                (IUserMessage)await chan.GetMessageAsync(context.OnJoin.AsQueryable().Where(a => a.Serverid == context.Auth.AsQueryable().Where(a => a.Token == token).Single().Serverid).Single().Messageid);
                                                                            if (msg != null)
                                                                            {
                                                                                await msg.ModifyAsync(msgProperty =>
                                                                                {
                                                                                    msgProperty.Embed =
                                                                                        BuildServerListEmbed(message);
                                                                                });
                                                                            }
                                                                            else
                                                                            {
                                                                                var msgg = await chan.SendMessageAsync(null,
                                                                                    false,
                                                                                    BotTools.Embed(
                                                                                        "Dont remove this message, this message will be updated continuously",
                                                                                        20));
                                                                                 var server = context.OnJoin.First(a => a.Serverid == guild);
                                                                                server.Messageid = msgg.Id;
                                                                                server.sevent = 1;
                        context.SaveChanges();
                                                                                await msgg.ModifyAsync(msgProperty =>
                                                                                {
                                                                                    msgProperty.Embed =
                                                                                        BuildServerListEmbed(message);
                                                                                });
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                Console.WriteLine("Exception occured: " + ex.ToString());
                                                            }
                                                        });
                                                    }
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("Exception occured: " + ex.ToString());
                                        }
                                    });

                                    break;

                                default:
                                    // ban ip in case gets to x requests To-DO
                                    Console.WriteLine($"Someone is trying to troll here, invalid packet: {message}");
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    });


                    //Console.WriteLine(message);

                    //allSockets.ToList().ForEach(s => s.Send(message));
                };
            });
            var input = Console.ReadLine();
            while (input != "exit")
            {
                //foreach (var socket in allSockets.ToList())
                //{
                //    await socket.Send(input);
                //}
                //input = Console.ReadLine();
            }
        }
        private static Embed BuildServerListEmbed(string msg)
        {
            string[] split = msg.Split('|');

            //List<string> serverList = new List<string>();
            string serverrr = null;
            int i = 2;
            while (i < split.Length - 5)
            {
                string serverName = split[i];
                string serverType = split[i + 1];
                string serverVersion = split[i + 2];
                string serverStatus = split[i + 3];
                string playerCount = split[i + 4];
                string maxPlayers = split[i + 5];
                //TODO replace these with custom cool looking emojis
                Emote statusEmote;
                if (serverStatus.ToLower().Equals("running"))
                {
                    if (!Emote.TryParse("<:Online:800460070709362739>", out statusEmote))
                    {
                        statusEmote = Emote.Parse(":green_circle:");
                    }
                }
                else if (serverStatus.ToLower().Equals("stopped"))
                {
                    if (!Emote.TryParse("<:Offline:800460061255663646>", out statusEmote))
                    {
                        statusEmote = Emote.Parse(":red_circle:");
                    }
                }
                else if (serverStatus.ToLower().Equals("starting"))
                {
                    if (!Emote.TryParse("<:Starting:800460572268953670>", out statusEmote))
                    {
                        statusEmote = Emote.Parse(":yellow_circle:");
                    }
                }
                else
                {
                    statusEmote = Emote.Parse(":black_circle:");
                }

                string typeEmote;
                if (serverType.ToLower().Equals("vanilla"))
                {
                    if (Emote.TryParse("<:Vanilla:800457564403400724>", out _))
                    {
                        typeEmote = "<:Vanilla:800457564403400724>";
                    }
                    else
                    {
                        typeEmote = "Vanilla";
                    }
                }
                else if (serverType.ToLower().Equals("paper"))
                {
                    if (Emote.TryParse("<:Paper:800457547907596359>", out _))
                    {
                        typeEmote = "<:Paper:800457547907596359>";
                    }
                    else
                    {
                        typeEmote = "Paper";
                    }
                }
                else if (serverType.ToLower().Equals("spigot"))
                {
                    if (Emote.TryParse("<:Spigot:800458118857228298>", out _))
                    {
                        typeEmote = "<:Spigot:800458118857228298>";
                    }
                    else
                    {
                        typeEmote = "Spigot";
                    }
                }
                else if (serverType.ToLower().Equals("waterfall"))
                {
                    if (Emote.TryParse("<:Waterfall:800457575590002708>", out _))
                    {
                        typeEmote = "<:Waterfall:800457575590002708>";
                    }
                    else
                    {
                        typeEmote = "Waterfall";
                    }
                }
                else
                {
                    typeEmote = "Unknown";
                }

                //Build server string
                string server = $"{statusEmote} {serverName} ({typeEmote} {serverVersion}) {playerCount}/{maxPlayers}" +
                                Environment.NewLine;
                serverrr += server;
                i = i + 6;
            }

            var ebd = new EmbedBuilder();
            Color Colorr = new Color(21, 22, 34);
            ebd.WithTitle("Your current servers");
            ebd.WithDescription($"{serverrr}");
            ebd.WithCurrentTimestamp();
            return ebd.Build();
        }
        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig() { MessageCacheSize = 20, AlwaysDownloadUsers = true, LogLevel = LogSeverity.Verbose,
              GatewayIntents=  GatewayIntents.DirectMessageReactions | GatewayIntents.DirectMessages |
                                 GatewayIntents.GuildBans | GatewayIntents.GuildInvites | GatewayIntents.GuildMembers |
                                 GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMessages |
                                 GatewayIntents.Guilds | GatewayIntents.GuildIntegrations
            })) //, .TotalShards = 3}))
            .AddSingleton(new CommandService(new CommandServiceConfig() { LogLevel = LogSeverity.Verbose, DefaultRunMode = RunMode.Async }))
            .AddSingleton<CommandHandler>()
            .AddSingleton<LogService>()
             .AddSingleton<InteractivityService>()
                .AddSingleton(new InteractivityConfig { DefaultTimeout = TimeSpan.FromSeconds(40) }) // You can optionally add a custom config
            .BuildServiceProvider();
        }
    }
}