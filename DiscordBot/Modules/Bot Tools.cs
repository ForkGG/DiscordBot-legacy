using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Fleck;
using Websocket.Client;

public class Bot_Tools : InteractiveBase
    {
    public static async Task NotificationControlAsync(ulong messageid, ulong channelid, string msg, int status)
    {
        try 
        {
            IMessageChannel channel = (IMessageChannel)KKK.Client.GetChannel(channelid);
            IUserMessage themessage = (IUserMessage)await channel.GetMessageAsync(messageid);

            await themessage.ModifyAsync(msgProperty =>
            {
                msgProperty.Embed = Bot_Tools.Embed(msg, status);
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }

    }
    public static Discord.Embed Embed(string msg, int status = 0)
    {
        var ebd = new EmbedBuilder();
        if (status == 0) { Color Colorr = new Color(21, 22, 34); ebd.Color = Colorr;} else if (status == 20 || status == 21) { ebd.Color = Color.Green; } else if (status == 40 || status == 44) { ebd.Color = Color.Red; }
        ebd.WithDescription($"{msg}");
        
        return ebd.Build();



    }

    public Server server = new Server();
    /// <summary>By giving server id, it gets ip and token, and using timout is optional, its on 10 sec default
    /// </summary>
    public async Task<int> Sendmsg(ulong serverid,string msg,long timeout = 10000)
    {
        string token = (string)server.GetTokenOfServer(serverid);
        string ip = (string)server.GetIPForToken(token, 2);

        if ((bool)CheckConnection(ip) == true)  //check if its connected
        {

            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool flag = false;
            while (flag == false)
            {
                if (sw.ElapsedMilliseconds > timeout) { return 0; }
                try
                {

                    var socket = Discord_Bot.allSockets.Find(client => client.ConnectionInfo.ClientIpAddress == ip);
                    await socket.Send(msg);
                    return 1;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return 0;
                }
            }
            return 0;
        }
        else
        {
            return 0;
        }
    }

    [Command("rec", RunMode = RunMode.Async)]
    [Alias("rec")]
    [Summary("Recreates role and channels. usage: (prefix)rec")]
    public async Task Rec()
    {
        try
        {

            if (Context.Guild.GetChannel((ulong)server.GetRoleandChannel(Context.Guild.Id, 1)) != null) { await Context.Guild.GetChannel((ulong)server.GetRoleandChannel(Context.Guild.Id, 1)).DeleteAsync(); }
            if (Context.Guild.GetRole((ulong)server.GetRoleandChannel(Context.Guild.Id, 0)) != null) { await Context.Guild.GetRole((ulong)server.GetRoleandChannel(Context.Guild.Id, 0)).DeleteAsync(); }
            server.RemoveRole(Context.Guild.Id);
            ulong origin = (ulong)GuildPermission.Speak + (ulong)GuildPermission.SendTTSMessages + (ulong)GuildPermission.SendMessages + (ulong)GuildPermission.ViewChannel + (ulong)GuildPermission.EmbedLinks + (ulong)GuildPermission.Connect + (ulong)GuildPermission.AttachFiles + (ulong)GuildPermission.AddReactions;
            GuildPermissions perms = new GuildPermissions(origin);
            //Color Colorr = new Color(21, 22, 34);
            var roleee = await Context.Guild.CreateRoleAsync("Fork-Mods", perms, null, false, false, null);
            var vChan = await Context.Guild.CreateTextChannelAsync("Fork-Bot");
            await vChan.AddPermissionOverwriteAsync(roleee, CommandHandler.AdminPermissions());
            await vChan.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, CommandHandler.None());
            server.InsertRole(Context.Guild.Id, roleee.Id, vChan.Id);
            var ebd = new EmbedBuilder();
            ebd.Color = Color.Green;
            ebd.WithDescription("Done.");
            await ReplyAsync($"<@{Context.Message.Author.Mention}>", false, ebd.Build());
        }
        catch (Exception ex) { Console.WriteLine(ex.ToString()); }
    }
    [Command("stop", RunMode = RunMode.Async)]
    [Alias("stop")]
    [Summary("Stops your mc server. usage: (prefix)stop [worldname]")]
    public async Task stop(string worldname)
    {
        try
        {
            var msg = await ReplyAsync(Context.Message.Author.Mention, false, Embed("Alright give me few seconds please."));
            int result = await Sendmsg(Context.Guild.Id, $"stop|{worldname}|{Context.User.Username}#{Context.User.Discriminator}|{Context.Channel.Id}|{msg.Id}");
            if (result == 1)
            {
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed("Command Executed.");
                });
            }
            else if (result == 0)
            {
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed("Oops. Looks like your fork app isnt online or connection timed out, please restart it.");
                });
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
            var msg = await ReplyAsync(Context.Message.Author.Mention, false, Embed("Alright give me few seconds please."));
            int result = await Sendmsg(Context.Guild.Id, $"start|{worldname}|{Context.User.Username}#{Context.User.Discriminator}|{Context.Channel.Id}|{msg.Id}");
            if (result == 1)
            {
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed("Command Executed.");
                });
            }
            else if (result == 0)
            {
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed("Oops. Looks like your fork app isnt online or connection timed out, please restart it.");
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
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
 var msg = await ReplyAsync(Context.Message.Author.Mention,false,Embed("Alright give me few seconds please."));
            if ((!(bool)server.CheckAuth(token, Context.Guild.Id) == true) && (!(bool)server.CheckOnhold(token) == false))
            {
                //sorting the token goes here
                //After connection if server replies, then its ok
                string ip = (string)server.GetIPForToken(token,1);
                if ((bool)CheckConnection(ip) == true)  //check if its connected
                {
                    server.InsertAuth(Context.Guild.Id, token,ip);
                    server.RemoveOnhold(token);
                    await Sendmsg(Context.Guild.Id, "status|Linked");
                    await msg.ModifyAsync(msgProperty =>
                    {
                        msgProperty.Content = $"{Context.Message.Author.Mention}";
                        msgProperty.Embed = Embed("Great, your discord server is now authorized with your fork server.");
                        });
                }
                else
                {
                    await msg.ModifyAsync(msgProperty =>
                    {
                        msgProperty.Content = $"{Context.Message.Author.Mention}";
                        msgProperty.Embed = Embed("Couldnt connect to your fork server, make sure its running.");
                    });
                }
            }
            else
            {
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed("Sorry, but this discord server or the token is already authorized or invalid.");
                });
            }
          
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    private bool CheckConnection(string ip)
    {
       
        if (Discord_Bot.allSockets.Any(client => client.ConnectionInfo.ClientIpAddress == ip))
        {
            var socket = Discord_Bot.allSockets.Find(client => client.ConnectionInfo.ClientIpAddress == ip);
            if (socket.IsAvailable) {
                return true;
            }
            else
            {
                return false;
            }
            
        }
        else
        {
            return false;
        }
    }
    
    [Command("unauth", RunMode = RunMode.Async)]
    [Alias("unauthorize")]
    [Summary("Unauthorizes your discord server with fork mc server. usage: (prefix)unauth")]
    public async Task unauth()
    {
        try
        {
            var msg = await ReplyAsync(Context.Message.Author.Mention, false, Embed("Alright give me few seconds please."));
            if (((bool)server.CheckAuth("", Context.Guild.Id) == true))
            {
                server.RemoveAuth(Context.Guild.Id);
                server.RemoveRole(Context.Guild.Id);
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed("our discord server got unlinked successfully.");
                });
            }
            else
            {
                await msg.ModifyAsync(msgProperty =>
                {
                    msgProperty.Content = $"{Context.Message.Author.Mention}";
                    msgProperty.Embed = Embed("Your discord server isnt linked.");
                });
            }
            }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
