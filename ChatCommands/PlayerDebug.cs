using BattleBitApi.Api;
using BattleBitApi.Enums;
using BattleBitApi.Helpers;

namespace BattleBitApi.ChatCommands;

public class PlayerDebug : ChatCommand
{
    public PlayerDebug() : base(
        name: "debug",
        description: "Adds debug information to the player",
        usage: "debug [state]",
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
            
            bool? state = args[0].ToLower() switch
            {
                "on" => true,
                "off" => false,
                _ => null
            };
            
            if (state == null)
            {
                player.Message($"Invalid arguments. Usage: {Usage}");
                return;
            }
            
            player.Debug = state.Value;
            DebugHelper.StartPlayerDebug(player, Server);
        };
    }
}