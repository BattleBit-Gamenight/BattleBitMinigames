using BattleBitAPI.Common;
using BattleBitMinigames.Api;
using BattleBitMinigames.Helpers;

namespace BattleBitMinigames.Events;

public class VipGamemode : Event
{
    public override Task<OnPlayerSpawnArguments?> OnPlayerSpawning(BattleBitPlayer player, OnPlayerSpawnArguments request)
    {
        if (true)
        {
            request.Wearings = player.Team switch
            {
                Team.TeamA => PlayerOutfits.BlueTeam,
                Team.TeamB => PlayerOutfits.RedTeam,
                _ => request.Wearings
            };
        }
        
        return base.OnPlayerSpawning(player, request);
    }
}