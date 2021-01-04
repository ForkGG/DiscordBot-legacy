using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;


    public class Bot_Tools : InteractiveBase
    {
        [Command("ping", RunMode = RunMode.Async)]
        [Alias("latency")]
        [Summary("Shows the websocket connection's latency and time it takes to send a message.")]
        public async Task PingAsync()
        {
            try
            {
                await Context.Client.SetGameAsync($"in {Context.Client.Guilds.Count} servers");
                var watch = Stopwatch.StartNew();
                var msg = await ReplyAsync("Pong");
                await msg.ModifyAsync(msgProperty => msgProperty.Content = $"🏓 {watch.ElapsedMilliseconds}ms");
            }
            catch (Exception ex)
            {
          
            }
        }
    }
