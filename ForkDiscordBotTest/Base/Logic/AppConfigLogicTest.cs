using ForkDiscordBot.Base.Logic;
using ForkDiscordBot.Model.Config;

namespace ForkDiscordBotTest.Base.Logic;

public class AppConfigLogicTest : AbstractTestBase<AppConfigLogic>
{
    protected override AppConfigLogic Tested { get; set; } = new AppConfigLogic();
    
    [Test]
    public async Task TestConfigLoad()
    {
        AppConfig config = await Tested.LoadAppConfig("config.json");
        Assert.NotNull(config);
    }

}