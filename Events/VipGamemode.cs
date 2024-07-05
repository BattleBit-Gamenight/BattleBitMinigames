using BattleBitAPI.Common;
using BattleBitAPI.Server;
using BattleBitMinigames.Api;

namespace BattleBitMinigames.Events;

public class VipGamemode : Event
{
    private Random _random = new();
    private BattleBitPlayer? TeamAVip;
    private BattleBitPlayer? TeamBVip;
    
    private bool IsPlayerVip(BattleBitPlayer player)
    {
        return player == TeamAVip || player == TeamBVip;
    }
    
    private void SetVipSettings(BattleBitPlayer player)
    {
        if (player.Team == Team.TeamA)
        {
            TeamAVip = player;
        }
        else if (player.Team == Team.TeamB)
        {
            TeamBVip = player;
        }
        
        player.Modifications.GiveDamageMultiplier = 0.25f;
        player.Modifications.ReceiveDamageMultiplier = 0.25f;
        player.KickFromSquad();
        player.Modifications.IsExposedOnMap = true;
        player.JoinSquad(Squads.King);
    }
    
    private void SetNonVipSettings(BattleBitPlayer player)
    {
        player.Modifications.IsExposedOnMap = false;
        player.Modifications.ReceiveDamageMultiplier = 1f;
        player.Modifications.GiveDamageMultiplier = 1f;
    }
    
    // TODO: Implement function to increment player point contribution using player properties

    public override async Task<OnPlayerSpawnArguments?> OnPlayerSpawning(BattleBitPlayer player,
        OnPlayerSpawnArguments request)
    {
        switch (player.Team)
        {
            case Team.TeamA:
            {
                if (TeamAVip == null)
                {
                    SetVipSettings(player);
                    request.Wearings = PlayerOutfits.BlueTeam;
                }
                else
                {
                    SetNonVipSettings(player);
                }
                
                break;
            }
            case Team.TeamB:
            {
                if (TeamBVip == null)
                {
                    SetVipSettings(player);
                    request.Wearings = PlayerOutfits.RedTeam;
                }
                else
                {
                    SetNonVipSettings(player);
                }
                
                break;
            }
            default:
            case Team.None:
                break;
        }
        
        return request;
    }

    public override Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<BattleBitPlayer> args)
    {
        var killer = args.Killer;
        var victim = args.Victim;
        
        if (victim.Team == Team.TeamA)
        {
            if (IsPlayerVip(victim))
            {
                // Give the killer's team 50 tickets for killing the VIP
                Server.RoundSettings.TeamBTickets += 50;
                Server.SayToAllChat($"{killer.Name} has killed the VIP! RU has gained 50 tickets.");
            }
            else
            {
                // Give the killer's team 1 ticket for killing a non-VIP player
                Server.RoundSettings.TeamBTickets += 1;
            }
        } 
        else if (victim.Team == Team.TeamB)
        {
            if (IsPlayerVip(victim))
            {
                // Give the killer's team 50 tickets for killing the VIP
                Server.RoundSettings.TeamATickets += 50;
                Server.SayToAllChat($"{killer.Name} has killed the VIP! US has gained 50 tickets.");
            }
            else
            {
                // Give the killer's team 1 ticket for killing a non-VIP player
                Server.RoundSettings.TeamATickets += 1;
            }
        }
        
        return Task.CompletedTask;
    }

    public override Task OnPlayerLeftSquad(BattleBitPlayer player, Squad<BattleBitPlayer> squad)
    {
        // If the player is the VIP, don't let them leave the VIP squad
        if (!IsPlayerVip(player)) return Task.CompletedTask;
        
        player.SayToChat("You are the VIP! You can't leave the squad.");
        player.JoinSquad(Squads.King);

        return Task.CompletedTask;
    }

    public override Task OnPlayerJoinedSquad(BattleBitPlayer player, Squad<BattleBitPlayer> squad)
    {
        if (IsPlayerVip(player) && squad.Name.ToString() == Squads.King.ToString())
        {
            player.Squad.SquadPoints = 999999;
            return Task.CompletedTask;
        }
        
        // If the player is not the VIP, don't let them join the VIP squad
        if (squad.Name.ToString() != Squads.King.ToString()) return Task.CompletedTask;
        
        player.SayToChat("You can't join the VIP squad.");
        player.KickFromSquad();

        return Task.CompletedTask;
    }
}