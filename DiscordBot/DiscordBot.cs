using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
    public Server serverr = new Server();
  public static List<IWebSocketConnection> allSockets { get; set; } = new List<IWebSocketConnection>();
    /// <summary>Connected Tokens
    /// </summary>
    public static List<string> AliveTokens { get; set; } = new List<string>();
    public System.Timers.Timer Updateserverlist;
    bool UpdateServer = false;
    public static async Task StartAsync()
        {
            await new Discord_Bot().RunAsync();
        }

        private async Task RunAsync()
        {
        if (!(UpdateServer == true))
        {
            Updateserverlist = new System.Timers.Timer(120000);
            Updateserverlist.Elapsed += async (sender, e) =>
            {
                try
                {
                    foreach (IWebSocketConnection s in allSockets)
                    {
                        if ((bool)serverr.CheckIfIPExist(s.ConnectionInfo.ClientIpAddress, 0) == true)
                        {
                            string token = (string)serverr.CheckIfIPExist(s.ConnectionInfo.ClientIpAddress, 1);
                            if ((bool)serverr.CheckAuth2(token, s.ConnectionInfo.ClientIpAddress) == true)
                            {
                                if (AliveTokens.Contains((string)serverr.CheckIfIPExist(s.ConnectionInfo.ClientIpAddress, 1)))
                                {
                                    var guild = KKK.Client.GetGuild(serverr.GetServerForToken(token));
                                    await s.Send($"serverList");

                                }}}}

                           

                } catch (Exception ex)
                {

                }
            
                };
            Updateserverlist.AutoReset = true;
            Updateserverlist.Enabled = true;
            UpdateServer = true;
        }
        //ILog logger = LogManager.GetLogger(typeof(FleckLog));
        FleckLog.Level = LogLevel.Debug;
        //FleckLog.LogAction = (level, message, ex) => {
        //    switch (level)
        //    {
        //        case LogLevel.Debug:
        //            logger.Debug(message, ex);
        //            break;
        //        case LogLevel.Error:
        //            logger.Error(message, ex);
        //            break;
        //        case LogLevel.Warn:
        //            logger.Warn(message, ex);
        //            break;
        //        default:
        //            logger.Info(message, ex);
        //            break;
        //    }
        //};
        Console.WriteLine("Checking if database exists...");
     serverr.CreateIfNot();
        var server = new WebSocketServer("ws://0.0.0.0:8181");
        server.Start(socket =>
        {

            socket.OnOpen = () =>
            {
                if (allSockets.Any(client => client.ConnectionInfo.ClientIpAddress == socket.ConnectionInfo.ClientIpAddress))
                {
                    var socket2 = allSockets.Find(client => client.ConnectionInfo.ClientIpAddress == socket.ConnectionInfo.ClientIpAddress);
                    try { allSockets.Remove(socket2); } catch (Exception ex) { } //Little security, dont let same ip to connect twice
                    allSockets.Add(socket);
                    
                }
                else
                {
                    try { allSockets.Remove(socket); } catch (Exception ex) { }
                    allSockets.Add(socket); 
                }
            };
            socket.OnClose = () =>
            {

                Console.WriteLine("Close!");
                allSockets.Remove(socket);
                if ((bool)serverr.CheckIfIPExist(socket.ConnectionInfo.ClientIpAddress, 0) == true)
                {
                    if (AliveTokens.Contains((string)serverr.CheckIfIPExist(socket.ConnectionInfo.ClientIpAddress, 1)))
                    {
                        AliveTokens.Remove((string)serverr.CheckIfIPExist(socket.ConnectionInfo.ClientIpAddress, 1));
                        Console.WriteLine($"{socket.ConnectionInfo.ClientIpAddress} Removed from alive tokens list");
                    }
                   
                }
            };
            socket.OnMessage = message =>
            {
                var Do = Task.Run(async () =>
                {
                    try
                    {

                        string[] codes = (message).Split('|');
                        switch (codes[0])
                        {
                            case "login":
                                string token = codes[1];

                                if ((!(bool)serverr.CheckAuth(token) == true) && !(bool)serverr.CheckOnhold(token, socket.ConnectionInfo.ClientIpAddress) == true)
                                {
                                    serverr.InsertOnhold(token, socket.ConnectionInfo.ClientIpAddress);
                                    Console.WriteLine("Token: " + token + $" IP: {socket.ConnectionInfo.ClientIpAddress} Added to onhold list");
                                    await socket.Send("status|OnHold");
                                }
                                else if ((bool)serverr.CheckAuth(token) == true && (bool)serverr.CheckAuth2(token, socket.ConnectionInfo.ClientIpAddress) == true)
                                {
                                    if (!AliveTokens.Contains((string)serverr.CheckIfIPExist(socket.ConnectionInfo.ClientIpAddress, 1)))
                                    {
                                        AliveTokens.Add(token);
                                        Console.WriteLine($"{socket.ConnectionInfo.ClientIpAddress} Added to alive tokens list");
                                        var guild = KKK.Client.GetGuild(serverr.GetServerForToken(token));
                                        if (guild != null)
                                        {
                                            await socket.Send($"status|Linked|{guild.Name}");
                                        }
                                        else { await socket.Send($"status|Linked|null"); }
                                        if ((bool)serverr.CheckIfSubscribed(guild.Id,0) == true)
                                        {
                                            await socket.Send($"subscribe|playerEvent");
                                        }
                                        }

                                }
                                break;
                            case "notify":
                                if (AliveTokens.Contains((string)serverr.CheckIfIPExist(socket.ConnectionInfo.ClientIpAddress, 1)) == true)
                                {
                                    var Do = Task.Run(async () =>
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
                                                            await Bot_Tools.NotificationControlAsync(ulong.Parse(messageid), ulong.Parse(channelid), $"Your `{servername}` stopped successfully, command was executed by `{discordname}`", int.Parse(result));
                                                            break;

                                                        case "40": //its stopped already 
                                                            await Bot_Tools.NotificationControlAsync(ulong.Parse(messageid), ulong.Parse(channelid), $"Your `{servername}` is stopped already, command was executed by `{discordname}`", int.Parse(result));
                                                            break;

                                                        case "44": //server not found
                                                            await Bot_Tools.NotificationControlAsync(ulong.Parse(messageid), ulong.Parse(channelid), $"I couldnt find your `{servername}` server, please make sure you typed the right name, command was executed by `{discordname}`", int.Parse(result));
                                                            break;
                                                    }
                                                    break;

                                                case "start":

                                                    switch (result)
                                                    {
                                                        //notify|{servername}|{discordname}|{channelid}|{messageid}|{eventt}|400|this is a test
                                                        case "20": //ok
                                                            await Bot_Tools.NotificationControlAsync(ulong.Parse(messageid), ulong.Parse(channelid), $"Your `{servername}` server is starting.. , command was executed by `{discordname}`", int.Parse(result));
                                                            break;
                                                        case "21": //ok
                                                            await Bot_Tools.NotificationControlAsync(ulong.Parse(messageid), ulong.Parse(channelid), $"Your `{servername}` started successfully, command was executed by `{discordname}`", int.Parse(result));
                                                            break;
                                                        case "40": //its stopped already 
                                                            await Bot_Tools.NotificationControlAsync(ulong.Parse(messageid), ulong.Parse(channelid), $"Your `{servername}` is running already, command was executed by `{discordname}`", int.Parse(result));
                                                            break;

                                                        case "44": //server not found
                                                            await Bot_Tools.NotificationControlAsync(ulong.Parse(messageid), ulong.Parse(channelid), $"I couldnt find your `{servername}` server, please make sure you typed the right name, command was executed by `{discordname}`", int.Parse(result));
                                                            break;
                                                    }
                                                    break;
                                            }
                                        }
                                        catch (Exception ex) { Console.WriteLine("Couldnt process request: invalid"); }
                                    });


                                }
                                break;
                            case "event":
                                var Do2 = Task.Run(async () =>
                                {
                                    string eventname = codes[1];
                                    string servername = codes[2];
                                    string playername = codes[3];
                                    try
                                    {
                                        if (AliveTokens.Contains((string)serverr.CheckIfIPExist(socket.ConnectionInfo.ClientIpAddress, 1)) == true && (bool)serverr.CheckIfSubscribed(serverr.GetServerForToken((string)serverr.CheckIfIPExist(socket.ConnectionInfo.ClientIpAddress, 1))) == true)
                                        {
                                            var guild = serverr.GetServerForToken((string)serverr.CheckIfIPExist(socket.ConnectionInfo.ClientIpAddress, 1));
                                            if (KKK.Client.GetGuild(guild).GetChannel((ulong)serverr.CheckIfSubscribed(serverr.GetServerForToken((string)serverr.CheckIfIPExist(socket.ConnectionInfo.ClientIpAddress, 1)),1)) != null)
                                            {
                                                switch(eventname)
                                                {
                                                    case "pjoin":
                                                      //  (ulong)serverr.CheckIfSubscribed(serverr.GetServerForToken((string)serverr.CheckIfIPExist(socket.ConnectionInfo.ClientIpAddress, 1)), 1)
                                                      //channel.SendMessageAsync
                                                       await Bot_Tools.NotificationControlAsync(0, (ulong)serverr.CheckIfSubscribed(serverr.GetServerForToken((string)serverr.CheckIfIPExist(socket.ConnectionInfo.ClientIpAddress, 1)), 1),$"***{playername}*** Just joined ***{servername}***",0,1);
                                                        break;
                                                }
                                            }
                                        }
                                    } catch (Exception ex)
                                    {

                                    }
                                });
                               
                                break;
                                default:
                                // ban ip in case gets to x requests To-DO
                                Console.WriteLine($"Someone is trying to troll here, invalid packet: {message}");
                                break;


                        }

                    }
                    catch (Exception ex) { }
                });


                //Console.WriteLine(message);

                //allSockets.ToList().ForEach(s => s.Send(message));

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
