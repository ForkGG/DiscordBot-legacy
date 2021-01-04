using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;


    static class Program
    {
        public static void Main()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"
Fork Bot is starting
---------------------------------------------------------------");
        Console.WriteLine();
            Fork_Bot.StartAsync().GetAwaiter().GetResult();

        }
    }
