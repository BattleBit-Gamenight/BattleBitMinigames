namespace BattleBitApi.ChatCommands;

public class ChatCommandList
{
    public static List<ChatCommand> Commands { get; } = new()
    {
        new Help()
    };

    public static List<ChatCommand> AdminCommands { get; } = new()
    {
        new PlayerDebug(),
        new TeleportCommand()
    };

    public static List<ChatCommand> ModeratorCommands { get; } = new()
    {
        new SendAnnouncement()
    };
}