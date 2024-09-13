using BattleBitAPI.Common;
using BattleBitMinigames.Api;
using BattleBitMinigames.Data;

namespace BattleBitMinigames.Helpers;

public class CustomGamemodeHelper
{
    private static readonly List<string> validCustomGamemodes = new()
    {
        "zombies",
        "vip",
        "hideandseek",
        "gungame",
        "none"
    };

    public static bool IsValidCustomGamemode(string gm)
    {
        return validCustomGamemodes.Contains(gm.ToLower());
    }

    public static void SetCustomGameMode(string gm, BattleBitServer server)
    {
        if (IsValidCustomGamemode(gm))
        {
            server.ResetEvents();
            
            switch (gm.ToLower())
            {
                case "zombies":
                    server.AddEvents(CustomGameModeEvents.ZombieEvents);
                    server.ForceEndGame();
                    break;
                case "vip":
                    server.AddEvents(CustomGameModeEvents.VipEvents);
                    server.SetServerSizeForNextMatch(MapSize._127vs127);
                    server.ForceEndGame();
                    break;
                case "hideandseek":
                    server.AddEvents(CustomGameModeEvents.HideAndSeekEvents);
                    server.SetServerSizeForNextMatch(MapSize._127vs127);
                    server.ForceEndGame();
                    break;
                case "gungame":
                    server.AddEvents(CustomGameModeEvents.GunGameEvents);
                    server.ForceEndGame();
                    break;
                case "none":
                    server.ResetEvents();
                    server.SetServerSizeForNextMatch(MapSize._8v8);
                    break;
                default:
                    return;
            }
        }
        else
        {
            Program.Logger.Info("Custom Gamemode is not valid");
        }
    }
}