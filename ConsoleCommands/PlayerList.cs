using BattleBitApi.Api;

namespace BattleBitApi.ConsoleCommands;

public class PlayerList : ConsoleCommand
{
    public PlayerList() : base(
        name: "playerlist",
        description:"Lists all players on the server.",
        usage: "playerlist [page]"
        )
    {
        Action = args =>
        {
            if (!Server.AllPlayers.Any())
            {
                Logger.Info("No players online.");
                return;
            }
            
            int page = 1;
            int pages = (int) Math.Ceiling(Server.AllPlayers.Count() / 10f);
            if (args.Length > 0)
            {
                page = int.TryParse(args[0], out int parsedPage) ? parsedPage : 0;
            }
            
            if (page > pages)
            {
                Logger.Error($"Page {page} does not exist.");
                return;
            }
            
            Logger.Info($"Player list (Page {page}/{pages}):");
            foreach (BattleBitApiPlayer player in Server.AllPlayers.Skip((page - 1) * 10).Take(10))
            {
                string position = $"<{player.Position.X.ToString().Replace(",", ".")}, {player.Position.Y.ToString().Replace(",", ".")}, {player.Position.Z.ToString().Replace(",", ".")}>";
                Logger.Info($"[{Server.AllPlayers.ToList().IndexOf(player) + 1}] - {player.Name} ({player.SteamID}) | {player.Role} | {position}");
            }
        };
    }
}