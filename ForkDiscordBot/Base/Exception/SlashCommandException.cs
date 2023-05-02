namespace ForkDiscordBot.Base.Exception;

public class SlashCommandException : ForkDiscordBotException
{
    public SlashCommandException(string message) : base(message)
    {
    }

    public SlashCommandException(string message, System.Exception innerException) : base(message, innerException)
    {
    }
}