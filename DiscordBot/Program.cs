using System;
using System.Net;


static class Program
    {
        public static void Main()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Fork Bot is starting\n---------------------------------------------------------------");

            DiscordBot.StartAsync().GetAwaiter().GetResult();
        }
    }
