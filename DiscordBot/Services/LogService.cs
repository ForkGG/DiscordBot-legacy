using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.VisualBasic;
namespace DiscordBot
{

    public class LogService
    {
        public LogService(DiscordSocketClient client, CommandService commands)
        {
            client.Log += ClientLog;
            commands.CommandExecuted += CommandLog;
        }

        public static Task ClientLog(LogMessage message)
        {

            Console.WriteLine(message.ToString());
            writetolog(message.ToString());
            return Task.CompletedTask;

        }

        public static void writetolog(string text)
        {
            string location = AppDomain.CurrentDomain.BaseDirectory;
            string fullpath = System.IO.Path.Combine(location, "logs.log");
            try
            {
                System.IO.File.AppendAllText(System.IO.Path.Combine(location, "logs.log"), DateTime.Now + " | " + text + Constants.vbNewLine);
            }
            catch (Exception ex)
            {
            }
        }

        private Task CommandLog(Optional<CommandInfo> info, ICommandContext context, IResult result)
        {
            if (result.IsSuccess)
                return Task.CompletedTask;
            switch (result.Error.Value)
            {
                case CommandError.UnknownCommand:
                    {
                        return Task.CompletedTask;
                    }

                case CommandError.ParseFailed:
                    {
                        Console.WriteLine(context.Guild.Name + " | " + result.ToString());
                        break;
                    }

                case CommandError.BadArgCount:
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(context.Guild.Name + " | " + result.ToString());
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    }

                case CommandError.ObjectNotFound:
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(context.Guild.Name + " | " + result.ToString());
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    }

                case CommandError.MultipleMatches:
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(context.Guild.Name + " | " + result.ToString());
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    }

                case CommandError.UnmetPrecondition:
                    {
                        Console.WriteLine(context.Guild.Name + " | " + result.ToString());
                        break;
                    }

                case CommandError.Exception:
                    {
                        Console.WriteLine(context.Guild.Name + " | " + result.ToString());
                        break;
                    }

                case CommandError.Unsuccessful:
                    {
                        Console.WriteLine(context.Guild.Name + " | " + result.ToString());
                        break;
                    }

                default:
                    {
                        break;
                    }
            }

            return Task.CompletedTask;
        }
    }
}