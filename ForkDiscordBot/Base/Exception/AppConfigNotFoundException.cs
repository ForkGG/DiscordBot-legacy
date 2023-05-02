namespace ForkDiscordBot.Base.Exception;

public class AppConfigNotFoundException : ForkDiscordBotException
{
    public AppConfigNotFoundException(string path) : base("Could not load configuration file on path: " + path)
    {
    }

    public AppConfigNotFoundException(string path, System.Exception causedBy) : base(
        "Could not load configuration file on path: " + path, causedBy)
    {
    }
}