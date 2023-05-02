using System.Text.Json;
using ForkDiscordBot.Base.Exception;
using ForkDiscordBot.Model.Config;

namespace ForkDiscordBot.Base.Logic;

public class AppConfigLogic : AbstractLogic
{
    public async Task<AppConfig> LoadAppConfig(string configPath)
    {
        try
        {
            var configFile = new FileInfo(configPath);
            var config = await File.ReadAllTextAsync(configFile.FullName);
            var result = JsonSerializer.Deserialize<AppConfig>(config);

            if (result == null) throw new AppConfigNotFoundException(configPath);

            return result;
        }
        catch (System.Exception e)
        {
            throw new AppConfigNotFoundException(configPath, e);
        }
    }
}