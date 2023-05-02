using log4net;

namespace ForkDiscordBot.Base;

public abstract class AbstractBaseComponent
{
    protected readonly ILog Log;

    protected AbstractBaseComponent()
    {
        Log = LogManager.GetLogger(GetType().Name);
    }
}