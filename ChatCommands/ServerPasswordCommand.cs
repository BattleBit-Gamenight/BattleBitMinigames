using BattleBitMinigames.Api;
using BattleBitMinigames.Data;
using BattleBitMinigames.Enums;

namespace BattleBitMinigames.ChatCommands;

public class ServerPasswordCommand : ChatCommand
{
    public ServerPasswordCommand() : base(
        name: "setpass",
        description: "Set the password for the server.",
        usage: "setpass <password>",
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
            
            if (args.Length < 1)
            {
                player.Message($"Invalid arguments. Usage: {Usage}");
                return;
            }

            String password = args[0];
            
            Server.ExecuteCommand("setpass " + password);
        };
    }
}