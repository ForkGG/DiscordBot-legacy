using System;
using System.Net;


    static class Program
    {
        public static void Main()
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(@"
Fork Bot Starting
---------------------------------------------------------------");
            Console.WriteLine();
            Fork_Bot.StartAsync().GetAwaiter().GetResult();
        }
    }
