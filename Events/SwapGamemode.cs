﻿using System.Numerics;
using BattleBitAPI.Common;
using BattleBitMinigames.Api;

namespace BattleBitMinigames.Events;

public class SwapGamemode : Event
{
    public override async Task OnAPlayerDownedAnotherPlayer(OnPlayerKillArguments<BattleBitPlayer> args)
    {
        var killer = args.Killer;
        var victim = args.Victim;
        var checks = 0;
        
        if (killer == null || killer == victim || !killer.IsAlive)
            return;

        var newPosition = victim.Position + new Vector3(0,1.5f,0);
        
        killer.Teleport(newPosition);
        await Task.Delay(10);
        SetPlayerLoadout(killer, victim.CurrentLoadout);
        
        while (Vector3.Distance(killer.Position, newPosition) > 3 && checks < 15)
        {
            checks++;
            killer.Teleport(newPosition);
            await Task.Delay(10);
        }
    }

    private static void SetPlayerLoadout(BattleBitPlayer player, PlayerLoadout loadout)
    {
        player.SetFirstAidGadget(loadout.FirstAidName, loadout.FirstAidExtra + 1);
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
                Server.RoundSettings.TeamATickets = 450;
                Server.RoundSettings.TeamBTickets = 450;
                break;
            case GameState.CountingDown:
                Server.RoundSettings.SecondsLeft = 10;
                break;
            case GameState.WaitingForPlayers:
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
}