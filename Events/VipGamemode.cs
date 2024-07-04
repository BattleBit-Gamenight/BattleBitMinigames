using BattleBitAPI.Common;
using BattleBitMinigames.Api;

namespace BattleBitMinigames.Events;

public class VipGamemode : Event
{
    private Random _random = new();
    private BattleBitPlayer? TeamAVip;
    private BattleBitPlayer? TeamBVip;

    public override Task<OnPlayerSpawnArguments?> OnPlayerSpawning(BattleBitPlayer player, OnPlayerSpawnArguments request)
    {
        switch (player.Team)
        {
            case Team.TeamA:
            {
                if (TeamAVip == null)
                {
                    TeamAVip = player;
                    request.Wearings = PlayerOutfits.BlueTeam;
                }
                
                break;
            }
            case Team.TeamB:
            {
                if (TeamBVip == null)
                {
                    TeamBVip = player;
                    request.Wearings = PlayerOutfits.RedTeam;
                }
                
                break;
            }
            default:
            case Team.None:
                break;
        }
        
        if (player == TeamAVip || player == TeamBVip)
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