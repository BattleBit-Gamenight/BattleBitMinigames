﻿using BattleBitMinigames.Enums;
using BattleBitMinigames.Helpers;

namespace BattleBitMinigames.ChatCommands;

public class StopServerAndApi : ChatCommand
{
    public StopServerAndApi() : base(
        name: "stop",
        description: "Stop :)",
        usage: "stop",
        minimumRequiredRole: PlayerRoles.Admin
    )
    {
        Action = (args, player) =>
        {
            if (!CanExecute(player))
            {
                player.Message("You do not have permission to execute this command.");
                return;
            }

            Program.Logger.Info("Closing Server...");
            Server.StopServer();
            Server.AnnounceLong("Stopping...");

            while (Server.IsConnected)
            {
                Thread.Sleep(1000);
            }
            
            Program.Logger.Info("Quitting API...");
            Environment.Exit(-1);
        };
    }
}