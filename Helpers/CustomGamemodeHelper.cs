using BattleBitMinigames.Api;
using BattleBitMinigames.Events;

namespace BattleBitMinigames.Helpers;

public class CustomGamemodeHelper
{
    private static readonly List<string> validCustomGamemodes = new()
    {
        "zombies",
        "hideandseek"
    };

    public static bool isValidCustomGamemode(string gm)
    {
        return validCustomGamemodes.Contains(gm.ToLower());
    }
    
    private static readonly List<Event> ZombieEvents = new ()
    {
        new ZombiesGamemode()
    };

    public static void SetCustomGameMode(string gm, BattleBitServer server)
    {
        if (isValidCustomGamemode(gm))
        {
            server.ResetEvents();
            
            switch (gm.ToLower())
            {
                case "zombies":
                    server.AddEvents(ZombieEvents);
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