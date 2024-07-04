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
        "gungame"
    };

    public static bool isValidCustomGamemode(string gm)
    {
        return validCustomGamemodes.Contains(gm.ToLower());
    }

    public static void SetCustomGameMode(string gm, BattleBitServer server)
    {
        if (isValidCustomGamemode(gm))
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
                    //server.ForceEndGame();
                    break;
                case "hideandseek":
                    server.AddEvents(CustomGameModeEvents.HideAndSeekEvents);
                    server.ForceEndGame();
                    break;
                case "gungame":
                    server.AddEvents(CustomGameModeEvents.GunGameEvents);
                    server.ForceEndGame();
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