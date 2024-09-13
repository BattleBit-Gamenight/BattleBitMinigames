using System.Collections.Concurrent;
using System.Text;
using BattleBitAPI.Common;
using BattleBitAPI.Server;
using BattleBitMinigames.Api;
using BattleBitMinigames.Enums;
using BattleBitMinigames.Helpers;
using BattleBitMinigames.Interfaces;

namespace BattleBitMinigames.Events;

public class GunGameGamemode : Event
{
    // GAMEMODE SETTINGS
    
    // DO NOT CHANGE THESE VALUES
    private MinigameStates State { get; set; } = MinigameStates.WaitingForPlayers;
    
    // Game Logic
    readonly Random _random = new();
    
    private int GetPlayerTier(BattleBitPlayer player)
    {
        return player.GetPlayerProperty(IPlayerProperties.IGunGamePlayerProperties.Tier) == string.Empty ? 0 : int.Parse(player.GetPlayerProperty("tier"));
    }
    
    private void SetPlayerTier(BattleBitPlayer player, int tier)
    {
        player.SetPlayerProperty(IPlayerProperties.IGunGamePlayerProperties.Tier, tier.ToString());
    }
    
    private void IncrementPlayerTier(BattleBitPlayer player)
    {
        var tier = GetPlayerTier(player);
        player.SetPlayerProperty(IPlayerProperties.IGunGamePlayerProperties.Tier, (tier + 1).ToString());
    }

    private void StartGunGame()
    {
        State = MinigameStates.Running;
        
        foreach (var player in Server.AllPlayers)
        {
            SetPlayerTier(player, 0);
        }
        
        Server.SayToAllChat("Gun Game has started! The first player to get 2 kills with every weapon wins!");
    }

    public override Task OnPlayerJoinedSquad(BattleBitPlayer player, Squad<BattleBitPlayer> squad)
    {
        player.KickFromSquad();
        player.SayToChat("You cannot join a squad in Gun Game.");
        return Task.CompletedTask;
    }

    public override Task<bool> OnPlayerRequestingToChangeRole(BattleBitPlayer player, GameRole requestedRole)
    {
        if (requestedRole != GameRole.Assault)
            player.SetNewRole(GameRole.Assault);
        
        return Task.FromResult(true);
    }

    public override async Task<OnPlayerSpawnArguments?> OnPlayerSpawning(BattleBitPlayer player,
        OnPlayerSpawnArguments request)
    {
        player.SetNewRole(GameRole.Assault);
        
        return request;
    }
    
    // Weapon tiers with int as the key and weapon combo as the value
    private readonly Dictionary<int, PlayerLoadout> _tierLoadout = new Dictionary<int, PlayerLoadout>()
    {
        { 
            0, 
            new PlayerLoadout() 
            {
                PrimaryWeapon = new WeaponItem() { Tool = Weapons.M9 }
            }
        },
        {
            1,
            new PlayerLoadout()
            {
                PrimaryWeapon = new WeaponItem() { Tool = Weapons.MP443 }
            }
        },
        {
            2,
            new PlayerLoadout()
            {
                PrimaryWeapon = new WeaponItem() { Tool = Weapons.Unica }
            }
        },
        {
            3,
            new PlayerLoadout()
            {
                PrimaryWeapon = new WeaponItem() { Tool = Weapons.USP }
            }
        },
        {
            4,
            new PlayerLoadout()
            {
                PrimaryWeapon = new WeaponItem() { Tool = Weapons.Glock18 }
            }
        },
        {
            5,
            new PlayerLoadout()
            {
                PrimaryWeapon = new WeaponItem() { Tool = Weapons.DesertEagle }
            }
        },
        {
            6,
            new PlayerLoadout()
            {
                PrimaryWeapon = new WeaponItem() { Tool = Weapons.Rsh12 }
            }
        }
    };
}