using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Fleck;
using Websocket.Client;

public class BotTools : InteractiveBase
{
    /// <summary>
    /// Int 0 is to modify the message, int 1 is to send a new message
    /// </summary>
    public static async Task NotificationControlAsync(ulong messageid, ulong channelid, string msg, int status,
        int num = 0)
    {
        try
        {
            if (num == 0)
            {
                IMessageChannel channel = (IMessageChannel) KKK.Client.GetChannel(channelid);
                IUserMessage themessage = (IUserMessage) await channel.GetMessageAsync(messageid);

                await themessage.ModifyAsync(msgProperty => { msgProperty.Embed = Embed(msg, status); });
            }
            else if (num == 1)
            {
                IMessageChannel channel = (IMessageChannel) KKK.Client.GetChannel(channelid);

                await channel.SendMessageAsync(null, false, Embed(msg, status));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    public static Embed Embed(string msg, int status = 0)
    {
        var ebd = new EmbedBuilder();
        if (status == 0)
        {
            Color Colorr = new Color(21, 22, 34);
            ebd.Color = Colorr;
        }
        else if (status == 20 || status == 21)
        {
            ebd.Color = Color.Green;
        }
        else if (status == 40 || status == 44)
        {
            ebd.Color = Color.Red;
        }

        ebd.WithDescription($"{msg}");

        return ebd.Build();
    }

    private static Server server = new Server();

    /// <summary>
    /// By giving server id, it gets ip and token, and using timout is optional, its on 10 sec default
    /// </summary>
    public static async Task<int> Sendmsg(ulong serverid, string msg, long timeout = 10000)
    {
        string token = server.GetTokenOfServer(serverid);
        string ip = server.GetIPForToken(token, 2);

        if (CheckConnection(ip)) //check if its connected
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                if (sw.ElapsedMilliseconds > timeout)
                {
                    Console.WriteLine($"Connection Timeout for {ip}");
                    return 0;
                }

                try
                {
                    var socket = DiscordBot.allSockets.Find(client => client.ConnectionInfo.ClientIpAddress == ip);
                    await socket.Send(msg);
#if DEBUG
                    Console.WriteLine($"{msg} send to {socket.ConnectionInfo.ClientIpAddress}");
#endif
                    return 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return 0;
                }
            }
        }

        return 0;
    }

    [Command("rec", RunMode = RunMode.Async)]
    [Alias("rec")]
    [Summary("Recreates role and channels. usage: (prefix)rec")]
    public async Task Rec()
    {
        await Task.Run(async () =>
        {
            try
            {
                server.RemoveRole(Context.Guild.Id);
                string warning = null;
                try
                {
                    if (Context.Guild.Roles.Any(x => x.Name.ToLower() == "Fork-Mods".ToLower()))
                    {
                        foreach (var Role in Context.Guild.Roles.Where(x => x.Name.ToLower() == "Fork-Mods".ToLower()))
                        {
                            await Role.DeleteAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    warning +=
                        $"`Fork-Mods` role detected, please move my role to top roles then run `$rec` to clean it." +
                        Environment.NewLine;
                }

                try
                {
                    if (Context.Guild.TextChannels.Any(x => x.Name.ToLower() == "Fork-Bot".ToLower()))
                    {
                        foreach (var Chan in Context.Guild.Channels.Where(x => x.Name.ToLower() == "Fork-Bot".ToLower())
                        )
                        {
                            await Chan.DeleteAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    warning +=
                        $"`Fork-Bot` channel detected, please move my role to top roles then run `$rec` to clean it." +
                        Environment.NewLine;
                }

                if (warning == null)
                {
                    ulong origin = (ulong) GuildPermission.Speak + (ulong) GuildPermission.SendTTSMessages +
                                   (ulong) GuildPermission.SendMessages + (ulong) GuildPermission.ViewChannel +
                                   (ulong) GuildPermission.EmbedLinks + (ulong) GuildPermission.Connect +
                                   (ulong) GuildPermission.AttachFiles + (ulong) GuildPermission.AddReactions;
                    GuildPermissions perms = new GuildPermissions(origin);
                    //Color Colorr = new Color(21, 22, 34);
                    var roleee = await Context.Guild.CreateRoleAsync("Fork-Mods", perms, null, false, false, null);
                    var vChan = await Context.Guild.CreateTextChannelAsync("Fork-Bot");
                    await vChan.AddPermissionOverwriteAsync(roleee, CommandHandler.AdminPermissions());
                    await vChan.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, CommandHandler.NoPermissions());

                    var ebd = new EmbedBuilder();
                    ebd.Color = Color.Green;
                    ebd.WithCurrentTimestamp();
                    ebd.WithAuthor($"Fork Server Management", Context.Guild.CurrentUser.GetAvatarUrl());
                    ebd.WithDescription(
                        "Hello there!, Im Fork if you dont know me, i can help you to handle and recieve notifications about your minecraft server." +
                        Environment.NewLine +
                        "I made a private channel for you, please use `$auth [token]` to link this discord server with your fork mc server" +
                        Environment.NewLine + "You can check for your token in fork app settings.");
                    ebd.WithFooter("Fork is a freemium Minecraft server management.");
                    //var ownerr = KKK.Client.GetGuild(guild.Id).OwnerId;
                    await vChan.SendMessageAsync($"<@{Context.Guild.OwnerId}>", false, ebd.Build());
                    var msgg = await vChan.SendMessageAsync(null, false,
                        BotTools.Embed("Dont remove this message, this message will be updated continuously", 20));
                    server.InsertRole(Context.Guild.Id, roleee.Id, vChan.Id, msgg.Id);
                }
                else
                {
                    var ebd = new EmbedBuilder();
                    ebd.Color = Color.Red;
                    ebd.WithCurrentTimestamp();
                    ebd.WithAuthor($"Error", Context.Guild.CurrentUser.GetAvatarUrl());
                    ebd.WithDescription(warning);
                    ebd.WithFooter("Fork is a freemium Minecraft server management.");
                    //var ownerr = KKK.Client.GetGuild(guild.Id).OwnerId;
                    await Context.Guild.DefaultChannel.SendMessageAsync($"<@{Context.Guild.OwnerId}>", false,
                        ebd.Build());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        });
    }

    [Command("stop", RunMode = RunMode.Async)]
    [Alias("stop")]
    [Summary("Stops your mc server. usage: (prefix)stop [worldname]")]
    public async Task stop(string worldname)
    {
        try
        {
            var msg = await ReplyAsync(Context.Message.Author.Mention, false,
                Embed("Alright give me few seconds please."));
            int result = await Sendmsg(Context.Guild.Id,
                $"stop|{worldname}|{Context.User.Username}#{Context.User.Discriminator}|{Context.Channel.Id}|{msg.Id}");
            switch (result)
            {
                case 1:
                    await msg.ModifyAsync(msgProperty =>
                    {
                        msgProperty.Content = $"{Context.Message.Author.Mention}";
                        msgProperty.Embed = Embed("Command Executed.", 20);
                    });
                    break;
                case 0:
                    await msg.ModifyAsync(msgProperty =>
                    {
                        msgProperty.Content = $"{Context.Message.Author.Mention}";
                        msgProperty.Embed =
                            Embed(
                                "Oops. Looks like your fork app isnt online or connection timed out, please restart it.",
                                40);
                    });
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Command("start", RunMode = RunMode.Async)]
    [Alias("start")]
    [Summary("Starts your mc server. usage: (prefix)start [worldname]")]
    public async Task start(string worldname)
    {
        try
        {
            var msg = await ReplyAsync(Context.Message.Author.Mention, false,
                Embed("Alright give me few seconds please."));
            int result = await Sendmsg(Context.Guild.Id,
                $"start|{worldname}|{Context.User.Username}#{Context.User.Discriminator}|{Context.Channel.Id}|{msg.Id}");
            switch (result)
            {
                case 1:
                    await msg.ModifyAsync(msgProperty =>
                    {
                        msgProperty.Content = $"{Context.Message.Author.Mention}";
                        msgProperty.Embed = Embed("Command Executed.", 20);
                    });
                    break;
                case 0:
                    await msg.ModifyAsync(msgProperty =>
                    {
                        msgProperty.Content = $"{Context.Message.Author.Mention}";
                        msgProperty.Embed =
                            Embed(
                                "Oops. Looks like your fork app isnt online or connection timed out, please restart it.",
                                40);
                    });
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    [Command("notify", RunMode = RunMode.Async)]
    [Alias("notification")]
    [Summary("Subscribe to your mc server notifications, (player join/leave) usage: (prefix)notify [mention channel]")]
    public async Task notify(SocketGuildChannel channel)
    {
        try
        {
            string token = server.GetTokenOfServer(Context.Guild.Id);
            string ip = server.GetIPForToken(token, 2);
            var msg = await ReplyAsync(Context.Message.Author.Mention, false,
                Embed("Alright give me few seconds please."));
            if (server.CheckIfNotifyExist(Context.Guild.Id))
            {
                //await msgg.PinAsync();
                server.UpdateNotify(Context.Guild.Id, channel.Id);
                string warn = null;
                if (CheckConnection(ip))
                {
                    await Sendmsg(Context.Guild.Id, $"subscribe|playerEvent");
                }
                else
                {
                    warn = Environment.NewLine +
                           "Couldnt connect to your fork server but dont worry, ill tell your fork server to send me server list once its connected";
                }

                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed($"Notification channel updated.{warn}", 20);
                });
            }
            else if ((bool) server.CheckIfNotifyExist(Context.Guild.Id) == false)
            {
                //await msgg.PinAsync();
                server.InsertNotify(Context.Guild.Id, channel.Id);
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed("Notification channel submitted.", 20);
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occured: " + ex.Message);
        }
    }

    [Command("dnotify", RunMode = RunMode.Async)]
    [Alias("dnotification")]
    [Summary("Unsubscribes to your mc server notifications. usage: (prefix)dnotify")]
    public async Task dnotify()
    {
        try
        {
            string token = server.GetTokenOfServer(Context.Guild.Id);
            string ip = server.GetIPForToken(token, 2);
            var msg = await ReplyAsync(Context.Message.Author.Mention, false,
                Embed("Alright give me few seconds please."));
            if (server.CheckIfNotifyExist(Context.Guild.Id))
            {
                server.RemoveNotify(Context.Guild.Id);
                if (CheckConnection(ip))
                {
                    await Sendmsg(Context.Guild.Id, $"unsub|playerEvent");
                }

                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed("Unsubscribed to notifications successfully.", 20);
                });
            }
            else if (!server.CheckIfNotifyExist(Context.Guild.Id))
            {
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed("You are not subscribed to notifications.", 40);
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occured: " + ex.Message);
        }
    }

    [Command("sub", RunMode = RunMode.Async)]
    [Alias("subscribe")]
    [Summary("Subscribe to an event. usage: (prefix)sub")]
    public async Task sub(bool check)
    {
        try
        {
            string token = server.GetTokenOfServer(Context.Guild.Id);
            string ip = server.GetIPForToken(token, 2);
            int truefalse = check ? 1 : 0;
            var msg = await ReplyAsync(Context.Message.Author.Mention, false,
                Embed("Alright give me few seconds please."));
            if (server.CheckSevent(Context.Guild.Id))
            {
                if (check)
                {
                    if (!server.CheckSevent(Context.Guild.Id, 1))
                    {
                        IMessageChannel chan =
                            (IMessageChannel) Context.Guild.GetChannel((ulong) server.GetSeventCH(Context.Guild.Id, 0));
                        var msgg = await chan.SendMessageAsync(null, false,
                            Embed("Dont remove this message, this message will be updated continuously", 20));
                        server.UpdateSEvent(Context.Guild.Id, msgg.Id, truefalse);
                        string warn = null;
                        if (CheckConnection(ip))
                        {
                            await Sendmsg(Context.Guild.Id, $"subscribe|playerEvent");
                        }
                        else
                        {
                            warn = Environment.NewLine +
                                   "Couldnt connect to your fork server but dont worry, ill tell your fork server to enable server events once its connected";
                        }

                        await msg.ModifyAsync(msgProperty =>
                        {
                            msgProperty.Content = $"{Context.Message.Author.Mention}";
                            msgProperty.Embed = Embed($"Enabled successfully.{warn}", 20);
                        });
                    }
                    else
                    {
                        await msg.ModifyAsync(msgProperty =>
                        {
                            msgProperty.Content = $"{Context.Message.Author.Mention}";
                            msgProperty.Embed = Embed($"Already enabled.", 40);
                        });
                    }
                }
                else if (!check)
                {
                    if (server.CheckSevent(Context.Guild.Id, 1))
                    {
                        server.UpdateSEvent(Context.Guild.Id, 0, truefalse);
                        if (CheckConnection(ip) == true)
                        {
                            await Sendmsg(Context.Guild.Id, $"unsub|serverListEvent");
                        }

                        await msg.ModifyAsync(msgProperty =>
                        {
                            msgProperty.Content = $"{Context.Message.Author.Mention}";
                            msgProperty.Embed = Embed($"Disabled successfully.", 20);
                        });
                    }
                    else
                    {
                        await msg.ModifyAsync(msgProperty =>
                        {
                            msgProperty.Content = $"{Context.Message.Author.Mention}";
                            msgProperty.Embed = Embed($"Already disabled.", 40);
                        });
                    }
                }
            }
            else
            {
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed($"Please use $rec and retry", 40);
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occured: " + ex.Message);
        }
    }

    [Command("ping", RunMode = RunMode.Async)]
    [Alias("latency")]
    [Summary("Shows the websocket connection's latency and time it takes to send a message. usage: (prefix)ping")]
    public async Task PingAsync()
    {
        try
        {
            var watch = Stopwatch.StartNew();
            var msg = await ReplyAsync("Pong");
            await msg.ModifyAsync(msgProperty => msgProperty.Content = $"🏓 {watch.ElapsedMilliseconds}ms");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occured: "+ex.Message);
        }
    }

    [Command("auth", RunMode = RunMode.Async)]
    [Alias("authorize")]
    [Summary("Authorizes your discord server with fork mc server. usage: (prefix)auth [token]")]
    public async Task auth(string token)
    {
        try
        {
            await Context.Message.DeleteAsync();
            var msg = await ReplyAsync(Context.Message.Author.Mention, false,
                Embed("Alright give me few seconds please."));
            if (!server.CheckAuth(token, Context.Guild.Id) && server.CheckOnhold(token))
            {
                //sorting the token goes here
                //After connection if server replies, then its ok
                string ip = server.GetIPForToken(token, 1);
                if (CheckConnection(ip)) //check if its connected
                {
                    server.InsertAuth(Context.Guild.Id, token, ip);
                    server.RemoveOnhold(token);
                    await Sendmsg(Context.Guild.Id, $"status|Linked|{Context.Guild.Name}");
                    if (!server.CheckRoleAndChannel(Context.Guild.Id))
                    {
                        await Rec();
                    }

                    await Sendmsg(Context.Guild.Id, $"subscribe|serverListEvent");

                    await msg.ModifyAsync(msgProperty =>
                    {
                        msgProperty.Content = $"{Context.Message.Author.Mention}";
                        msgProperty.Embed = Embed("Great, your discord server is now authorized with your fork server.",
                            20);
                    });
                }
                else
                {
                    await msg.ModifyAsync(msgProperty =>
                    {
                        msgProperty.Content = $"{Context.Message.Author.Mention}";
                        msgProperty.Embed = Embed("Couldnt connect to your fork server, make sure its running.", 40);
                    });
                }
            }
            else
            {
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed =
                        Embed("Sorry, but this discord server or the token is already authorized or invalid.", 40);
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occured: "+ex.Message);
        }
    }

    [Command("help", RunMode = RunMode.Async)]
    [Summary("All command details, for specific command details use (prefix)help [commandname]")]
    public async Task help(string commandname = "all")
    {
        try
        {
            EmbedBuilder eb = new EmbedBuilder {Color = Color.Blue};
            EmbedBuilder owner = new EmbedBuilder {Color = Color.Blue};
            List<CommandInfo> commands = KKK.CommandService.Commands.ToList();
            bool kwqw = false;
            bool onemod = false;
            if (commandname == "all")
            {
                foreach (ModuleInfo modulename in KKK.CommandService.Modules)
                {
                    string list = "";
                    foreach (CommandInfo command in modulename.Commands)
                    {
                        kwqw = true;
                        list += $"`{command.Name}` | ";
                    }
                    eb.AddField(modulename.Name, list);
                }
            }
            else if (commandname != "all")
            {
                foreach (CommandInfo command in commands)
                {
                    if (command.Name == commandname)
                    {
                        kwqw = true;
                        onemod = true;
                        eb.AddField(command.Name, command.Summary.Replace("(prefix)", KKK.prefix));
                    }
                }
            }

            if (!kwqw)
            {
                eb.Color = Color.Red;
                eb.Description = $"Sorry but we couldn't find ({commandname}) in commands list";
            }
            else
            {
                eb.WithCurrentTimestamp();

                if (!onemod)
                {
                    eb.WithAuthor("Command List", Context.Client.CurrentUser.GetAvatarUrl());
                    eb.Description = $"Use `{KKK.prefix}help [commandname]` to get more details about specific command";
                }
                else
                {
                    eb.WithAuthor($"Info about {commandname} command", Context.Client.CurrentUser.GetAvatarUrl());
                }

                eb.WithThumbnailUrl(Context.Client.CurrentUser.GetAvatarUrl());
                eb.WithFooter("Requested By: " + Context.User.Username + "#" + Context.User.Discriminator);
                await Context.Message.Author.SendMessageAsync(null, false, eb.Build());
            }

            owner.Description = $"I sent you a DM with your request";
            owner.Color = Color.Green;
            await ReplyAsync(Context.Message.Author.Mention, false, owner.Build());
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occured: "+ex.Message);
        }
    }

    [Command("leave", RunMode = RunMode.Async)]
    [Alias("leave")]
    [Summary("Bot will leaves discord server. *be aware, it deletes all guild records in the db* usage: (prefix)leave")]
    public async Task leave()
    {
        try
        {
            var msg = await ReplyAsync(Context.Message.Author.Mention, false,
                Embed(
                    $"Please type `{Context.Guild.Name}` to confirm.{Environment.NewLine}Be aware this process cant be recovered.{Environment.NewLine}Type anything else to cancel."));
            var msgg = await NextMessageAsync(true, true, TimeSpan.FromMinutes(1));
            await ReplyAsync(Context.Message.Author.Mention, false,
                Embed($"Sad to see you go.., ill leave shortly, good bye!", 20));
            if (msgg.Content == Context.Guild.Name)
            {
                if (server.CheckRoleAndChannel(Context.Guild.Id))
                {
                    try
                    {
                        if (Context.Guild.GetChannel((ulong) server.GetRoleandChannel(Context.Guild.Id, 1)) != null)
                        {
                            var channel =
                                Context.Guild.GetChannel((ulong) server.GetRoleandChannel(Context.Guild.Id, 1));
                            await channel.DeleteAsync();
                        }

                        if (Context.Guild.GetRole((ulong) server.GetRoleandChannel(Context.Guild.Id, 0)) != null)
                        {
                            var Role = Context.Guild.GetRole((ulong) server.GetRoleandChannel(Context.Guild.Id, 0));
                            await Role.DeleteAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Exception occured: "+ex.Message);
                    }
                }

                await Sendmsg(Context.Guild.Id, $"rec");
                await Sendmsg(Context.Guild.Id, $"status|OnHold");
                await Sendmsg(Context.Guild.Id, $"unsub|serverListEvent");
                await Sendmsg(Context.Guild.Id, $"unsub|playerEvent");
                server.LeaveServer(Context.Guild.Id);
                try
                {
                    await Context.Guild.LeaveAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception occured: "+ex.Message);
                }
            }
            else
            {
                await ReplyAsync(Context.Message.Author.Mention, false, Embed($"Canceled.", 20));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private static bool CheckConnection(string ip)
    {
        if (DiscordBot.allSockets.Any(client => client.ConnectionInfo.ClientIpAddress == ip))
        {
            var socket = DiscordBot.allSockets.Find(client => client.ConnectionInfo.ClientIpAddress == ip);
            return socket != null && socket.IsAvailable;
        }
        return false;
    }

    [Command("unauth", RunMode = RunMode.Async)]
    [Alias("unauthorize")]
    [Summary("Unauthorizes your discord server with fork mc server. usage: (prefix)unauth")]
    public async Task unauth()
    {
        try
        {
            var msg = await ReplyAsync(Context.Message.Author.Mention, false,
                Embed("Alright give me few seconds please."));
            string token = server.GetTokenOfServer(Context.Guild.Id);
            if (server.CheckAuth(token, Context.Guild.Id))
            {
                await Sendmsg(Context.Guild.Id, $"rec");
                await Sendmsg(Context.Guild.Id, $"status|OnHold");
                await Sendmsg(Context.Guild.Id, $"unsub|serverListEvent");
                await Sendmsg(Context.Guild.Id, $"unsub|playerEvent");
                server.LeaveServer(Context.Guild.Id);
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed("Your discord server got unlinked successfully.", 20);
                });
            }
            else
            {
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed("Your discord server isnt linked.", 40);
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception occured: "+ex.Message);
        }
    }
}