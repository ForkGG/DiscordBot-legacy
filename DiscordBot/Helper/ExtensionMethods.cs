using Discord.Commands;

public static class ExtensionMethods
{
    public static int CustomPriority(this CommandInfo commandInfo)
    {
        return commandInfo.Name switch
        {
            "help" => 0,
            "auth" => 1,
            "unauth" => 2,
            "rec" => 3,
            "leave" => 4,
            "notify" => 5,
            "dnotify" => 6,
            "start" => 7,
            "stop" => 8,
            _ => 99
        };
    }
}