using System.Numerics;
using BattleBitAPI.Common;
using BattleBitMinigames.Api;

namespace BattleBitMinigames.Events;

public class SwapGamemode : Event
{
    public override async Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<BattleBitPlayer> args)
    {
        var killer = args.Killer;
        var victim = args.Victim;
        
        if (killer == null || killer == victim || !killer.IsAlive)
            return;

        var newPosition = victim.Position + new Vector3(0,1.5f,0);
        var checkOutOfBounds = newPosition - new Vector3(0, 20f, 0);

        await Task.Delay(500);
        killer.Teleport(newPosition);
        
        await Task.Delay(3000);
        if (newPosition.Y < checkOutOfBounds.Y)
            killer.Teleport(newPosition);
    }

    public override Task OnPlayerConnected(BattleBitPlayer player)
    {
        player.Modifications.CanSpectate = false;
        player.Modifications.DownTimeGiveUpTime = 0f;
        
        return base.OnPlayerConnected(player);
    }
}