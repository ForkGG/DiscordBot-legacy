Imports System.Globalization
Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Timers
Imports Discord
Imports Discord.Addons
Imports Discord.Addons.Interactive
Imports Discord.Commands
Imports Discord.WebSocket
Imports Newtonsoft.Json

Public Class Bot_Tools
    Inherits InteractiveBase

    <Command("ping", RunMode:=RunMode.Async)>
    <[Alias]("latency")>
    <Summary("Shows the websocket connection's latency and time it takes to send a message.")>
    Public Async Function PingAsync() As Task
        Try

            Await Context.Client.SetGameAsync($"in {Context.Client.Guilds.Count} servers")
            Dim watch = Stopwatch.StartNew()
            Dim msg = Await ReplyAsync("Pong")
            Await msg.ModifyAsync(Sub(msgProperty) msgProperty.Content = $"🏓 {watch.ElapsedMilliseconds}ms")
        Catch ex As Exception
            LogService.ClientLog(Nothing, ex.ToString)
        End Try
    End Function


End Class