﻿namespace BattleBitMinigames.ChatCommands;

public class ChatCommandList
{
    public static List<ChatCommand> Commands { get; } = new()
    {
        new Help()
    };

    public static List<ChatCommand> AdminCommands { get; } = new()
    {
        new PlayerDebug(),
        new TeleportCommand(),
        new StopServerAndApi(),
        new ServerSizeCommand(),
        new ServerPasswordCommand()
    };

    public static List<ChatCommand> ModeratorCommands { get; } = new()
    {
        new SendAnnouncement(),
        new CustomGamemode(),
        new GamemodeRotation(),
        new MapRotation(),
        new KillCommand()
    };
}