namespace BattleBitApi.ConsoleCommands;

public class ConsoleCommandList
{
    public static List<ConsoleCommand> Commands { get; } = new()
    {
        new Help(),
        new Kick(),
        new Ban(),
        new PlayerList(),
        new SendMessage(),
        new SendAnnouncement()
    };
}