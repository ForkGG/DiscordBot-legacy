using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
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
            if (!((bool)server.CheckAuth(Context.Guild.Id, token) == true))
            {
                //sorting the token goes here
                //After connection if server replies, then its ok
                var url = new Uri("ws://localhost:8181");
                var exitEvent = new ManualResetEvent(false);

                using (var client = new WebsocketClient(url))
                {
                    client.MessageReceived.Subscribe(msgg => Checkmsg(msgg.Text));
                    await client.Start();
                 client.Send("Hi");
                 client.Dispose(); 
                }
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
    private void Checkmsg(string msgg)
    {
        if(msgg.Contains("Alive"))
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
