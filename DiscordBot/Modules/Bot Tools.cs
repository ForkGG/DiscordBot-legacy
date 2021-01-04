using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;


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
    [Command("auth", RunMode = RunMode.Async)]
    [Alias("authorize")]
    [Summary("Authorizes your discord server with fork mc server")]
    public async Task auth(string token)
    {
        try
        {
           await Context.Message.DeleteAsync();
            var msg = await ReplyAsync("Alright give me few seconds please.");
            if (!((bool)server.CheckAuth(Context.Guild.Id, token) == false))
            {
                // TO-DO check if token is correct
                //
                server.InsertAuth(Context.Guild.Id, token);
                await msg.ModifyAsync(msgProperty => msgProperty.Content = "Great, your discord server is now authorized with your fork server.");
            }
            else
            {
                await msg.ModifyAsync(msgProperty => msgProperty.Content = "Sorry, but this discord server or the token is already authorized.");
            }
           
        }
        catch (Exception ex)
        {

        }
    }
}
