using System.Numerics;
using BattleBitApi.Enums;

namespace BattleBitApi.ChatCommands;

public class TeleportCommand : ChatCommand
{
    public TeleportCommand() : base(
        name: "teleport",
        description: "Teleports the player to the specified player",
        usage: "teleport x,y,z",
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

            string[] position = args[0].Split(",");
            if (position.Length != 3)
            {
                player.Message($"Invalid arguments. Usage: {Usage}");
                return;
            }
            
            if (!float.TryParse(position[0], out float x) || !float.TryParse(position[1], out float y) || !float.TryParse(position[2], out float z))
            {
                player.Message($"Invalid arguments. Usage: {Usage}");
                return;
            }
            
            Vector3 targetPosition = new(x, y, z);
            
            player.Teleport(targetPosition);
            
            player.Message($"Teleported to {targetPosition}.");
        };
    }
}