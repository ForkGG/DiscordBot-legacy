using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;


    static class KKK
    {
        public static string prefix = "$";
        public static CommandService CommandService;
    public static DiscordSocketClient Client;
}

    public class CommandHandler
    {
    public Server server = new Server();
   
        private readonly IServiceProvider Services;

        public CommandHandler(DiscordSocketClient client, CommandService commandService, IServiceProvider services)
        {
            KKK.Client = client;
            KKK.CommandService = commandService;
            Services = services;
        }

        public async Task InitializeAsync()
        {
            await KKK.CommandService.AddModulesAsync(Assembly.GetEntryAssembly(), Services);
        KKK.Client.Ready += ready;
        KKK.Client.UserJoined += welcome;
        KKK.Client.MessageReceived += HandleCommandAsync;
        KKK.Client.MessagesBulkDeleted += BulkDeleteAsync;
        KKK.Client.JoinedGuild += Joinedguild;
    }
    public static OverwritePermissions AdminPermissions()
    {
        return new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);
    }
    public static OverwritePermissions None()
    {
        return new OverwritePermissions(PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny,
                                        PermValue.Deny);
    }
    private async Task Joinedguild(SocketGuild guild)
    {
        var Do = Task.Run(async() => { try {

                if ((!(bool)server.CheckAuth("None", guild.Id) == true)  || (!(bool)server.CheckRoleAndChannel(guild.Id) == true))
                {
                    ulong origin = (ulong)GuildPermission.Speak + (ulong)GuildPermission.SendTTSMessages + (ulong)GuildPermission.SendMessages + (ulong)GuildPermission.ViewChannel + (ulong)GuildPermission.EmbedLinks + (ulong)GuildPermission.Connect + (ulong)GuildPermission.AttachFiles + (ulong)GuildPermission.AddReactions;
                    GuildPermissions perms = new GuildPermissions(origin);
                    //Color Colorr = new Color(21, 22, 34);
                    var roleee = await guild.CreateRoleAsync("Fork-Mods", perms, null, false, false, null);
                    var vChan = await guild.CreateTextChannelAsync("Fork-Bot");
                    await vChan.AddPermissionOverwriteAsync(roleee, AdminPermissions());
                    await vChan.AddPermissionOverwriteAsync(guild.EveryoneRole, None());
                    server.InsertRole(guild.Id, roleee.Id,vChan.Id);
                    var ebd = new EmbedBuilder();
                    ebd.Color = Color.Green;
                    ebd.WithCurrentTimestamp();
                    ebd.WithAuthor($"Fork Server Management", guild.CurrentUser.GetAvatarUrl());
                    ebd.WithDescription("Hello there!, Im Fork if you dont know me, i can help you to handle and recieve notifications about your minecraft server." + Environment.NewLine + "I made a private channel for you, please use `$auth [token]` to link this discord server with your fork mc server" + Environment.NewLine + "You can check for your token in fork app settings.");
                    ebd.WithFooter("Fork is a freemium Minecraft server management.");
                    var ownerr = KKK.Client.GetGuild(guild.Id).OwnerId;
                    await vChan.SendMessageAsync($"<@{ownerr}>", false, ebd.Build());
                }
            } catch (Exception ex) { } });
        await Task.CompletedTask;
    }

    private async Task BulkDeleteAsync(IReadOnlyCollection<Cacheable<IMessage, ulong>> messages, ISocketMessageChannel channel)
        {
            await Task.CompletedTask;
        }

        public async Task ready()
        {
            try
            {
                var Do = Task.Run(() => { try { 
                    
                    
                    
                    } catch (Exception ex) { } });
                await KKK.Client.SetGameAsync($"Working On it", null, ActivityType.Listening);
            }
            catch (Exception ex)
            {

            }
        }

        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value is null)
            {
                return true;
            }

            for (int i = 0, loopTo = value.Length - 1; i <= loopTo; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task welcome(SocketGuildUser user)
        {
        }

        private async Task HandleCommandAsync(SocketMessage message)
        {
            SocketUserMessage userMessage = message as SocketUserMessage;
            if (userMessage is null || userMessage.Author.IsBot || userMessage.Author.IsWebhook)
                return;
            int argPos = 0;
            var context = new SocketCommandContext(KKK.Client, userMessage);
            try
            {
           
            if (!(message.Channel.GetType().ToString() == "Discord.WebSocket.SocketDMChannel"))
                {
                    if (!userMessage.HasMentionPrefix(KKK.Client.CurrentUser, ref argPos) && !userMessage.HasStringPrefix(KKK.prefix, ref argPos, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                    else
                    {
                    var Roleid = (long)server.GetRoleandChannel(context.Guild.Id,0);
                    var authorr = context.Guild.GetUser(context.Message.Author.Id);
                   var thisss = context.Message.Author as SocketGuildUser;
                    if (authorr.Roles.Any(r => r.Id == (ulong)Roleid) == true || thisss.GuildPermissions.ManageGuild == true)
                    {
                        if (userMessage.Content.ToLower().StartsWith($"{KKK.prefix}auth") || (bool)server.CheckAuth("null", context.Guild.Id) == true) //if its for authentication let the command to be executed
                        {
                            string command = userMessage.Content.Substring(argPos).Trim();
                            var result = await KKK.CommandService.ExecuteAsync(context, command, Services);

                            if (!result.IsSuccess)
                            {

                                if (!((int?)result.Error == (int?)CommandError.UnknownCommand) == true)
                                {
                                    Console.WriteLine(result.ErrorReason);
                                }
                            }
                        }
                    }
                
                    }
                }
                else
                {
                    await context.Channel.SendMessageAsync($"Im Not Working yet", false, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
