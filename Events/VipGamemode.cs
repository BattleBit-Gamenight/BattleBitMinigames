using BattleBitAPI.Common;
using BattleBitAPI.Server;
using BattleBitMinigames.Api;
using BattleBitMinigames.Helpers;

namespace BattleBitMinigames.Events;

public class VipGamemode : Event
{
    private Random _random = new();
    private BattleBitPlayer? _teamAVip;
    private BattleBitPlayer? _teamBVip;
    
    private bool IsPlayerVip(BattleBitPlayer player)
    {
        return player == _teamAVip || player == _teamBVip;
    }
    
    private void SetVipSettings(BattleBitPlayer player)
    {
        switch (player.Team)
        {
            case Team.TeamA:
                _teamAVip = player;
                break;
            case Team.TeamB:
                _teamBVip = player;
                break;
            case Team.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(player));
        }

        player.Message($"{RichTextHelper.FromColorName("Gold")}You are the VIP!{RichTextHelper.NewLine()}{RichTextHelper.FromColorName("Orange")}You have reduced damage and fall damage, and you are exposed on the map. {RichTextHelper.FromColorName("Red")}Don't die{RichTextHelper.Color()}!", 10);
        
        player.Modifications.FallDamageMultiplier = 0.25f;
        player.Modifications.GiveDamageMultiplier = 0.25f;
        player.Modifications.ReceiveDamageMultiplier = 0.25f;
        player.Modifications.IsExposedOnMap = true;
        player.KickFromSquad();
        player.JoinSquad(Squads.King);
    }
    
    private static void SetNonVipSettings(BattleBitPlayer player)
    {
        player.Modifications.IsExposedOnMap = false;
        player.Modifications.FallDamageMultiplier = 1f;
        player.Modifications.ReceiveDamageMultiplier = 1f;
        player.Modifications.GiveDamageMultiplier = 1f;
    }
    
    // TODO: Implement function to increment player point contribution using player properties
    
    public override Task<OnPlayerSpawnArguments?> OnPlayerSpawning(BattleBitPlayer player,
        OnPlayerSpawnArguments request)
    {
        if (request.Loadout.HeavyGadget.Name.ToLower().Contains("rpg") || request.Loadout.LightGadget.Name.ToLower().Contains("c4") || request.Loadout.LightGadget.Name.ToLower().Contains("claymore") || request.Loadout.Throwable.Name.ToLower().Contains("impact") || request.Loadout.Throwable.Name.ToLower().Contains("frag"))
        {
            player.SayToChat($"{RichTextHelper.FromColorName("Orange")}You can't use explosives in this gamemode.");
            player.Message($"{RichTextHelper.FromColorName("Orange")}You can't use explosives in this gamemode.", 10);
            return Task.FromResult<OnPlayerSpawnArguments?>(null);
        }
        
        switch (player.Team)
        {
            case Team.TeamA:
            {
                if (_teamAVip == null)
                {
                    SetVipSettings(player);
                    request.Wearings = PlayerOutfits.BlueTeam;
                    request.Loadout.PrimaryWeapon = new WeaponItem()
                    {
                        Tool = Weapons.M249,
                        Barrel = Attachments.Tactical,
                        MainSight = Attachments.RedDot,
                        UnderRail = Attachments.BCMGunFighter
                    };
                    request.Loadout.Throwable = Gadgets.SmokeGrenadeBlue;
                    request.Loadout.ThrowableExtra = 100;
                    request.Loadout.PrimaryExtraMagazines = 100;
                    request.Loadout.SecondaryExtraMagazines = 100;
                }
                else
                {
                    SetNonVipSettings(player);
                }
                
                break;
            }
            case Team.TeamB:
            {
                if (_teamBVip == null)
                {
                    SetVipSettings(player);
                    request.Wearings = PlayerOutfits.RedTeam;
                    request.Loadout.PrimaryWeapon = new WeaponItem()
                    {
                        Tool = Weapons.M249,
                        Barrel = Attachments.Tactical,
                        MainSight = Attachments.RedDot,
                        UnderRail = Attachments.BCMGunFighter
                    };
                    request.Loadout.Throwable = Gadgets.SmokeGrenadeRed;
                    request.Loadout.ThrowableExtra = 100;
                    request.Loadout.PrimaryExtraMagazines = 100;
                    request.Loadout.SecondaryExtraMagazines = 100;
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
        
        return Task.FromResult<OnPlayerSpawnArguments?>(request);
    }
    
    public override Task OnGameStateChanged(GameState oldState, GameState newState)
    {
        switch (newState)
        {
            case GameState.Playing:
                Server.RoundSettings.SecondsLeft = 100000;
                Server.RoundSettings.TeamATickets = 350;
                Server.RoundSettings.TeamBTickets = 350;
                break;
            case GameState.CountingDown:
                break;
            case GameState.WaitingForPlayers:
                break;
            case GameState.EndingGame:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        return Task.CompletedTask;
    }

    public override Task OnPlayerDied(BattleBitPlayer player)
    {
        if (!IsPlayerVip(player)) return Task.CompletedTask;
        switch (player.Team)
        {
            case Team.TeamA:
                Server.SayToAllChat($"The VIP {RichTextHelper.FromColorName("Gold")}{player.Name}{RichTextHelper.Color()} took the easy way out (suicided)! {RichTextHelper.FromColorName("RoyalBlue")}US{RichTextHelper.Color()} has lost {RichTextHelper.FromColorName("Crimson")}20 tickets{RichTextHelper.Color()}.");
                Server.RoundSettings.TeamATickets -= 20;
                _teamAVip = null;
                break;
            case Team.TeamB:
                Server.SayToAllChat($"The VIP {RichTextHelper.FromColorName("Gold")}{player.Name}{RichTextHelper.Color()} took the easy way out (suicided)! {RichTextHelper.FromColorName("Red")}RU{RichTextHelper.Color()} has lost {RichTextHelper.FromColorName("Crimson")}20 tickets{RichTextHelper.Color()}.");
                Server.RoundSettings.TeamBTickets -= 20;
                _teamBVip = null;
                break;
            case Team.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(player));
        }

        return Task.CompletedTask;
    }

    public override Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<BattleBitPlayer> args)
    {
        var killer = args.Killer;
        var victim = args.Victim;
        
        if (killer == null || victim == null) return Task.CompletedTask;
        if (killer == victim) return Task.CompletedTask;
        
        switch (victim.Team)
        {
            case Team.TeamA when IsPlayerVip(victim):
                // Give the killer's team 50 tickets for killing the VIP
                Server.RoundSettings.TeamBTickets += 45;
                Server.RoundSettings.TeamATickets -= 45;
                Server.SayToAllChat($"{RichTextHelper.FromColorName("MediumPurple")}{killer.Name}{RichTextHelper.Color()} has killed the VIP ({RichTextHelper.FromColorName("Gold")}{victim.Name}{RichTextHelper.Color()})! {RichTextHelper.FromColorName("Red")}RU{RichTextHelper.Color()} has {RichTextHelper.FromColorName("Orange")}stolen 75 tickets{RichTextHelper.Color()} from {RichTextHelper.FromColorName("RoyalBlue")}US{RichTextHelper.Color()}.");
                
                // Remove the VIP from the var
                _teamAVip = null;
                
                // Kill the victim
                victim.Kill();
                break;
            case Team.TeamA:
                // Give the killer's team 1 ticket for killing a non-VIP player
                // Server.RoundSettings.TeamBTickets += 1;
                break;
            case Team.TeamB when IsPlayerVip(victim):
                // Give the killer's team 50 tickets for killing the VIP
                Server.RoundSettings.TeamATickets += 45;
                Server.RoundSettings.TeamBTickets -= 45;
                Server.SayToAllChat($"{RichTextHelper.FromColorName("MediumPurple")}{killer.Name}{RichTextHelper.Color()} has killed the VIP ({RichTextHelper.FromColorName("Gold")}{victim.Name}{RichTextHelper.Color()})! {RichTextHelper.FromColorName("RoyalBlue")}US{RichTextHelper.Color()} has {RichTextHelper.FromColorName("Orange")}stolen 75 tickets{RichTextHelper.Color()} from {RichTextHelper.FromColorName("Red")}RU{RichTextHelper.Color()}.");
                
                // Remove the VIP from the var
                _teamBVip = null;
                
                // Kill the victim
                victim.Kill();
                break;
            case Team.TeamB:
                // Give the killer's team 1 ticket for killing a non-VIP player
                // Server.RoundSettings.TeamATickets += 1;
                break;
            case Team.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(args));
        }
        
        return Task.CompletedTask;
    }

    public override Task OnPlayerLeftSquad(BattleBitPlayer player, Squad<BattleBitPlayer> squad)
    {
        // If the player is the VIP, don't let them leave the VIP squad
        if (!IsPlayerVip(player)) return Task.CompletedTask;
        if (squad.Name == Squads.NoSquad) return Task.CompletedTask;
        
        player.SayToChat($"{RichTextHelper.FromColorName("Orange")}You are the VIP! You can't leave the squad.");
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
        if (IsPlayerVip(player) || squad.Name != Squads.King) return Task.CompletedTask;
        
        player.SayToChat($"{RichTextHelper.FromColorName("Orange")}You can't join the VIP squad.");
        player.KickFromSquad();

        return Task.CompletedTask;
    }

    public override Task OnPlayerDisconnected(BattleBitPlayer player)
    {
        if (!IsPlayerVip(player)) return Task.CompletedTask;
        switch (player.Team)
        {
            case Team.TeamA:
                Server.SayToAllChat($"{RichTextHelper.FromColorName("Gold")}{player.Name}{RichTextHelper.Color()} got scared as VIP and ran away (disconnected)! {RichTextHelper.FromColorName("RoyalBlue")}US{RichTextHelper.Color()} has {RichTextHelper.FromColorName("Crimson")}lost 20 tickets{RichTextHelper.Color()}.");
                Server.RoundSettings.TeamATickets -= 20;
                _teamAVip = null;
                break;
            case Team.TeamB:
                Server.SayToAllChat($"{RichTextHelper.FromColorName("Gold")}{player.Name}{RichTextHelper.Color()} got scared as VIP and ran away (disconnected)! {RichTextHelper.FromColorName("Red")}RU{RichTextHelper.Color()} has {RichTextHelper.FromColorName("Crimson")}lost 20 tickets{RichTextHelper.Color()}.");
                Server.RoundSettings.TeamBTickets -= 20;
                _teamBVip = null;
                break;
            case Team.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(player));
        }
        
        return Task.CompletedTask;
    }
}