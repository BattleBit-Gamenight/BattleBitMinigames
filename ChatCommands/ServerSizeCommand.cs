using BattleBitMinigames.Api;
using BattleBitMinigames.Data;
using BattleBitMinigames.Enums;

namespace BattleBitMinigames.ChatCommands;

public class ServerSizeCommand : ChatCommand
{
    public ServerSizeCommand() : base(
        name: "setsize",
        description: "Set the size of the server for next round.",
        usage: "setsize <size>",
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

            String size = args[0];
            
            if (!ServerSettings.ServerSizes.Contains(size))
            {
                player.Message($"Invalid size. Usage: {Usage}");
                return;
            }
            
            Server.ExecuteCommand("setsize " + size);
            player.SayToChat($"Server size set to {size}");
        };
    }
}