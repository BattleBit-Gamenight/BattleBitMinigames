using BattleBitMinigames.Api;

namespace BattleBitMinigames.Helpers;

public class PlayerHelpers
{
    public static void KillPlayerInVehicle(BattleBitApiPlayer player, string messageToPlayer)
    {
        if (!player.InVehicle) return;

        player.Kill();
        player.SayToChat(messageToPlayer);
    }
}