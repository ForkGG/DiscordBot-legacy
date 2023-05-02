using System.Reflection;
using ForkDiscordBot.Base;

namespace ForkDiscordBotTest.Base;

public class DiscordBotTest : AbstractTestBase<DiscordBot>
{
    protected override DiscordBot Tested { get; set; } = new DiscordBot();
    

}