using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Fleck;
using Websocket.Client;

public class Bot_Tools : InteractiveBase
    {
    public Server server = new Server();

    [Command("ping", RunMode = RunMode.Async)]
        [Alias("latency")]
        [Summary("Shows the websocket connection's latency and time it takes to send a message.")]
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
    public bool check = false;
    [Command("auth", RunMode = RunMode.Async)]
    [Alias("authorize")]
    [Summary("Authorizes your discord server with fork mc server")]
    public async Task auth(string token)
    {
        try
        {
           await Context.Message.DeleteAsync();
            var msg = await ReplyAsync("Alright give me few seconds please.");
            if (!((bool)server.CheckAuth(token, Context.Guild.Id) == true))
            {
                //sorting the token goes here
                //After connection if server replies, then its ok
               // Checkmsg(token); //check if its in connection

                if ((bool)check == true)
                {
                    server.InsertAuth(Context.Guild.Id, token);
                    await msg.ModifyAsync(msgProperty => msgProperty.Content = "Great, your discord server is now authorized with your fork server.");
                }
                else
                {
                    await msg.ModifyAsync(msgProperty => msgProperty.Content = "Couldn't Connect To Your Fork Server, make sure its running.");
                }
            }
            else
            {
                await msg.ModifyAsync(msgProperty => msgProperty.Content = "Sorry, but this discord server or the token is already authorized.");
            }
          
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    private void CheckConnection(string msgg)
    {
 
      //var socket = Discord_Bot.allSockets.Any(client => client.ConnectionInfo.ClientIpAddress == "");

        if (Discord_Bot.allSockets.Any(client => client.ConnectionInfo.ClientIpAddress == ""))
        {
            check = true;
        }
    }
    [Command("unauth", RunMode = RunMode.Async)]
    [Alias("unauthorize")]
    [Summary("Unauthorizes your discord server with fork mc server")]
    public async Task unauth(string token)
    {
        try
        {
           // TODO


        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
