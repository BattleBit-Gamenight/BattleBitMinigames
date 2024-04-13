using BattleBitApi.Api;

namespace BattleBitApi.Helpers;

public class PlayerHelpers
{
    public static void KillPlayerInVehicle(BattleBitApiPlayer player, string reason)
    {
        if (player.InVehicle == false)
            return;

        player.Kill();
        player.SayToChat(reason);
    }
}