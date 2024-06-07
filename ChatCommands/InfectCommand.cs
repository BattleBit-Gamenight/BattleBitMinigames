using BattleBitMinigames.Api;
using BattleBitMinigames.Enums;

namespace BattleBitMinigames.ChatCommands;

public class KillCommand : ChatCommand
{
    public KillCommand() : base(
        name: "kill",
        description: "Kill players that misbehave.",
        usage: "kill <steam_id>",
        minimumRequiredRole: PlayerRoles.Moderator
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

            ulong steamId = ulong.Parse(args[0]);
            BattleBitPlayer? targetPlayer = Server.AllPlayers.FirstOrDefault(player => player.SteamID == steamId);

            if (targetPlayer != null)
            {
                targetPlayer.Kill();
                player.Message($"Successfully killed {targetPlayer.Name}");
            }
            else
            {
                player.Message($"Couldn't find player with SteamID \"{steamId}");
            }
        };
    }
}