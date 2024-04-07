using BattleBitApi.Api;

namespace BattleBitApi.ConsoleCommands;
    
public class Kick : ConsoleCommand
{
    public Kick() : base(
        name: "kick",
        description: "Kicks a player from the server.",
        usage: "kick <steamId> [reason]"
        )
    {
        Action = args =>
        {
            if (args.Length < 1)
            {
                Logger.Error("You must provide a player name.");
                return;
            }
            
            ulong playerSteamId;
            try
            {
                playerSteamId = ulong.Parse(args[0]);
            }
            catch (Exception)
            {
                Logger.Error($"Invalid SteamId \"{args[0]}\".");
                return;
            }
            
            BattleBitApiPlayer? player = Server.AllPlayers.FirstOrDefault(p => p.SteamID == playerSteamId);
            
            if (player == null)
            {
                Logger.Error($"Player with SteamId \"{playerSteamId}\" not found.");
                return;
            }
            
            var reason = args.Length > 1 ? string.Join(" ", args.Skip(1)) : "Kicked by admin.";
            
            player.Kick(reason);
        };
    }
}