namespace ForkDiscordBot.Base.Exception;

public abstract class ForkDiscordBotException : System.Exception
{
    protected ForkDiscordBotException(string message) : base("DiscordBot: " + message)
    {
    }

    protected ForkDiscordBotException(string message, System.Exception innerException) : base("DiscordBot: " + message,
        innerException)
    {
    }
}