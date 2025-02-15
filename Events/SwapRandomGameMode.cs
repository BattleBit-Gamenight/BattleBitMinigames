using System.Collections.Concurrent;
using System.Numerics;
using BattleBitAPI.Common;
using BattleBitMinigames.Api;
using BattleBitMinigames.Data;

namespace BattleBitMinigames.Events;

public class SwapRandomGamemode : Event
{
    Random rnd = new Random();
    private ConcurrentDictionary<ulong, PlayerLoadout> PlayerLoadouts { get; set; } = new();
    public override async Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<BattleBitPlayer> args)
    {
        var killer = args.Killer;
        var victim = args.Victim;

        if (killer == null || killer == victim || !killer.IsAlive)
            return;

        var newPosition = victim.Position + new Vector3(0, 1.5f, 0);

        killer.Teleport(newPosition);
        await Task.Delay(10);
        SetPlayerLoadout(killer, PlayerLoadouts[victim.SteamID]);
        PlayerLoadouts[killer.SteamID] = victim.CurrentLoadout;

        var checks = 0;
        while (Vector3.Distance(killer.Position, newPosition) > 3 && checks < 15)
        {
            checks++;
            killer.Teleport(newPosition);
            await Task.Delay(10);
        }
    }

    public override Task OnPlayerSpawned(BattleBitPlayer player)
    {
        // If the player's loadout is not set, set it
        PlayerLoadouts[player.SteamID] = player.CurrentLoadout;
        
        return Task.CompletedTask;
    }

    private static void SetPlayerLoadout(BattleBitPlayer player, PlayerLoadout loadout)
    {
        player.SetFirstAidGadget(loadout.FirstAidName, loadout.FirstAidExtra + 20);
        player.SetLightGadget(loadout.LightGadgetName, loadout.LightGadgetExtra + 1);
        player.SetHeavyGadget(loadout.HeavyGadgetName, loadout.HeavyGadgetExtra + 1);
        player.SetThrowable(loadout.ThrowableName, loadout.ThrowableExtra + 1);
        player.SetSecondaryWeapon(loadout.SecondaryWeapon, loadout.SecondaryExtraMagazines + 1);
        Task.Delay(100).Wait();
        player.SetPrimaryWeapon(loadout.PrimaryWeapon, loadout.PrimaryExtraMagazines + 1);
    }

    public override Task OnGameStateChanged(GameState oldState, GameState newState)
    {
        switch (newState)
        {
            case GameState.Playing:
                Server.RoundSettings.SecondsLeft = 100000;
                // Server.RoundSettings.TeamATickets = 600;
                // Server.RoundSettings.TeamBTickets = 600;
                break;
            case GameState.CountingDown:
                Server.RoundSettings.SecondsLeft = 10;
                break;
            case GameState.WaitingForPlayers:
                PlayerLoadouts.Clear();
                break;
            case GameState.EndingGame:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        return Task.CompletedTask;
    }

    public override Task OnPlayerConnected(BattleBitPlayer player)
    {
        player.Modifications.CanSpectate = false;
        player.Modifications.DownTimeGiveUpTime = 0f;

        return base.OnPlayerConnected(player);
    }
    public override async Task<OnPlayerSpawnArguments?> OnPlayerSpawning(BattleBitPlayer player, OnPlayerSpawnArguments request)
    {
        try
        {
            request.Loadout.PrimaryWeapon = new WeaponItem()
            {
                Tool = PlayerWeapons.WeaponList.ElementAt(rnd.Next(0, PlayerWeapons.WeaponList.Count())),
                Barrel = PlayerWeapons.BarrelList.ElementAt(rnd.Next(0, PlayerWeapons.BarrelList.Count())),
                MainSight = PlayerWeapons.SightList.ElementAt(rnd.Next(0, PlayerWeapons.SightList.Count())),
                UnderRail = PlayerWeapons.UnderRailList.ElementAt(rnd.Next(0, PlayerWeapons.UnderRailList.Count())),
                BoltAction = Attachments.BoltActionE,
                CamoIndex = (ushort)rnd.Next(1, 1000),
                AttachmentsCamoIndex = (ushort)rnd.Next(1, 1000),
                UVIndex = (byte)rnd.Next(2, 4),
                AttachmentsUVIndex = (byte)rnd.Next(2, 4)
            };

            request.Loadout.SecondaryWeapon = new WeaponItem()
            {
                Tool = PlayerWeapons.WeaponList.FindAll(weapon => weapon != request.Loadout.PrimaryWeapon.Tool).ElementAt(rnd.Next(0, PlayerWeapons.WeaponList.Count() - 1)),
                Barrel = PlayerWeapons.BarrelList.ElementAt(rnd.Next(0, PlayerWeapons.BarrelList.Count())),
                MainSight = PlayerWeapons.SightList.ElementAt(rnd.Next(0, PlayerWeapons.SightList.Count())),
                UnderRail = PlayerWeapons.UnderRailList.ElementAt(rnd.Next(0, PlayerWeapons.UnderRailList.Count())),
                BoltAction = Attachments.BoltActionE,
                CamoIndex = (ushort)rnd.Next(1, 1000),
                AttachmentsCamoIndex = (ushort)rnd.Next(1, 1000),
                UVIndex = (byte)rnd.Next(2, 4),
                AttachmentsUVIndex = (byte)rnd.Next(2, 4)
            };
            

            request.Loadout.LightGadget = PlayerWeapons.GadgetList.ElementAt(rnd.Next(0, PlayerWeapons.GadgetList.Count()));
            if (request.Loadout.LightGadget.Name.Contains("Rpg"))
                request.Loadout.HeavyGadget = PlayerWeapons.GadgetList.FindAll(gadget => !gadget.Name.Contains("Rpg")).ElementAt(rnd.Next(0, PlayerWeapons.GadgetList.Count() - 5));
            else if (request.Loadout.LightGadget.Name.Contains("Sledge") || request.Loadout.LightGadget.Name.Contains("Pickaxe"))
                request.Loadout.HeavyGadget = PlayerWeapons.GadgetList.FindAll(gadget => !(gadget.Name.Contains("Sledge") || gadget.Name.Contains("Pickaxe"))).ElementAt(rnd.Next(0, PlayerWeapons.GadgetList.Count() - 6));
            else
                request.Loadout.HeavyGadget = PlayerWeapons.GadgetList.FindAll(gadget => gadget != request.Loadout.LightGadget).ElementAt(rnd.Next(0, PlayerWeapons.GadgetList.Count() - 1));
            request.Loadout.Throwable = PlayerWeapons.ThrowableList.ElementAt(rnd.Next(0, PlayerWeapons.ThrowableList.Count()));

            // if (player.PingMs > 150)//this didn't work, need to figure out why
            // {
            //     player.Modifications.RunningSpeedMultiplier = (float)rnd.Next(100, 125) / 100;
            //     player.Modifications.JumpHeightMultiplier = (float)rnd.Next(100, 150) / 100;
            //     player.Modifications.ReloadSpeedMultiplier = (float)rnd.Next(100, 300) / 100;
            //     player.Modifications.FallDamageMultiplier = 0;
            // }
            // player.Modifications.RunningSpeedMultiplier = (float)rnd.Next(95, 300) / 100;
            // player.Modifications.JumpHeightMultiplier = (float)rnd.Next(80, 300) / 100;
            // player.Modifications.ReloadSpeedMultiplier = (float)rnd.Next(100, 300) / 100;
            // player.Modifications.FallDamageMultiplier = (float)rnd.Next(0, 10) / 100;
            
            player.Modifications.KillFeed = true;
            player.Modifications.RespawnTime = 0;
            player.Modifications.CanSuicide = false;
            return request;
        }
        catch (Exception e)
        {
            Console.Out.WriteLine($"Error occured on {player.Name} spawning: {e}");
            return null;
        }
    }
}