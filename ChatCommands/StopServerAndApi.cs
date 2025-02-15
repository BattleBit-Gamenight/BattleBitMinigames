using BattleBitMinigames.Enums;
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
            player.SayToChat("Closing Server...");
            Server.StopServer();
        };
    }
}