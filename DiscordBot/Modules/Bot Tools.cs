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
    public Discord.Embed Embed(string msg)
    {
        var ebd = new EmbedBuilder();
        Color Colorr = new Color(21, 22, 34);
        ebd.Color = Colorr;
        ebd.WithDescription($"{msg}");
        return ebd.Build();

    }
    public Server server = new Server();
    public async Task<int> Sendmsg(ulong serverid,string msg)
    {
        string token = (string)server.GetTokenOfServer(serverid);
        string ip = (string)server.GetIPForToken(token, 2);

        if ((bool)CheckConnection(ip) == true)  //check if its connected
        {
            int startTickCount = Environment.TickCount;
            int timeout = (int)TimeSpan.FromSeconds(10).TotalMilliseconds; //timeout
            bool flag = false;
            while (Environment.TickCount > startTickCount + timeout && flag == false)
            {
                if (Environment.TickCount > startTickCount + timeout)
                {
                    flag = true;
                    return 0;
                }
                else
                {
                    try
                    {
                        var socket = Discord_Bot.allSockets.Find(client => client.ConnectionInfo.ClientIpAddress == ip);
                        await socket.Send(msg);
                        return 1;
                    }
                    catch (Exception ex)
                    {
                        return 0;
                    }
                }
            }

            return 0;
        }
        else
        {
            return 0;
        }
    }
    [Command("stop", RunMode = RunMode.Async)]
    [Alias("stop")]
    [Summary("Stops your mc server. usage: (prefix)stop [worldname]")]
    public async Task stop(string worldname)
    {
        try
        {
            var msg = await ReplyAsync(Context.Message.Author.Mention, false, Embed("Alright give me few seconds please."));
            int result = await Sendmsg(Context.Guild.Id, $"stop|{worldname}");
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
