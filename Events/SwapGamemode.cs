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
        
        killer.Teleport(newPosition);
        
        await Task.Delay(2000);
        if (newPosition.Y < checkOutOfBounds.Y)
            killer.Teleport(newPosition);
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