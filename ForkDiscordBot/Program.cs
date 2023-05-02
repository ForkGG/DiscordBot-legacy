using System.Reflection;
using ForkDiscordBot.Base;
using log4net;
using log4net.Config;
using Mono.Options;

namespace ForkDiscordBot;

public class Program
{
    private string _configPath = "config.json";
    private bool _showHelp;


    public static Task Main(string[] args)
    {
        return new Program().MainAsync(args);
    }

    private async Task MainAsync(string[] args)
    {
        ConfigureLogger();

        var options = CreateOptions();
        try
        {
            options.Parse(args);
        }
        catch (OptionException e)
        {
            Console.WriteLine("Error with passed parameters:");
            Console.WriteLine(e.Message);
            Console.WriteLine("Try `ForkDiscordBot --help' for more information.");
        }

        if (_showHelp)
        {
            options.WriteOptionDescriptions(Console.Out);
            return;
        }

        Console.WriteLine("Starting Discord bot with config: " + _configPath);

        await new DiscordBot().Execute(_configPath);
    }

    private OptionSet CreateOptions()
    {
        var result = new OptionSet
        {
            {
                "c|config=",
                "The {path} to the config file for the bot. Optional",
                v => _configPath = v
            },
            {
                "h|help",
                "Shows this message and exit",
                v => _showHelp = true
            }
        };

        return result;
    }

    private void ConfigureLogger()
    {
        var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
        XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
    }
}