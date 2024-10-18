using System.Globalization;
using System.Numerics;
using BattleBitAPI.Common;
using BattleBitMinigames.Api;
using BattleBitMinigames.Data;
using BattleBitMinigames.Handlers;
using BattleBitMinigames.Interfaces;

namespace BattleBitMinigames.Events;

public class RegionManager : Event
{
    private CancellationTokenSource? _cancellationTokenSource;

    private void StartRegionManager()
    {
        Program.Logger.Info("Started RegionManager!");
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        Task.Run(async () =>
        {
            while (Server.IsConnected && !cancellationToken.IsCancellationRequested)
            {
                foreach (var player in Server.AllPlayers.Where(player =>
                             player.IsAlive && player.Position != Vector3.Zero))
                {
                    var region = RegionHelper.GetIsPlayerInRegion(RegionList.GetMapRegions(Server.Map), player);
                    if (region != null)
                    {
                        switch (Program.ServerConfiguration.LaunchCustomGamemode)
                        {
                            case "vip":
                            {
                                if (player.GetPlayerProperty(IPlayerProperties.IVipPlayerProperties.IsVip) != "true")
                                    continue;
                                
                                HandlePlayerInRegion(player, region);
                                
                                break;
                            }
                            case "hideandseek":
                            {
                                if (player.GetPlayerProperty(IPlayerProperties.IHideAndSeekPlayerProperties.IsSeeking) == "true")
                                    continue;
                                
                                HandlePlayerInRegion(player, region);
                                
                                break;
                            }
                        }
                    }
                    else
                    {
                        // Reset properties when a player leaves the spawn region
                        player.RemovePlayerProperty(IPlayerProperties.IGeneralPlayerProperties.SpawnedInSpawn);
                        player.RemovePlayerProperty(IPlayerProperties.IGeneralPlayerProperties.EnteredSpawnTime);
                        player.RemovePlayerProperty(IPlayerProperties.IGeneralPlayerProperties.SpawnedInSpawnTime);
                    }
                }

                await Task.Delay(1000, cancellationToken);
            }

            Program.Logger.Info("RegionManager stopped.");
        }, cancellationToken);
    }

    private void StopRegionManager()
    {
        if (_cancellationTokenSource is { IsCancellationRequested: false })
        {
            _cancellationTokenSource.Cancel();
            Program.Logger.Info("Stopping RegionManager...");
        }
    }

    public override Task<bool> OnPlayerTypedMessage(BattleBitPlayer player, ChatChannel channel, string msg)
    {
        if (player.GetHighestRole() == Enums.PlayerRoles.Admin)
        {
            if (msg.Contains("?rgm start"))
            {
                StartRegionManager();
            }
            else if (msg.Contains("?rgm stop"))
            {
                StopRegionManager();
            }
        }

        return base.OnPlayerTypedMessage(player, channel, msg);
    }

    public override Task OnPlayerSpawned(BattleBitPlayer player)
    {
        var region = RegionHelper.GetIsPlayerInRegion(RegionList.GetMapRegions(Server.Map), player);
        if (region != null)
        {
            player.SetPlayerProperty(IPlayerProperties.IGeneralPlayerProperties.SpawnedInSpawn, "true");
            player.SetPlayerProperty(IPlayerProperties.IGeneralPlayerProperties.SpawnedInSpawnTime,
                DateTime.UtcNow.ToString());
        }

        return base.OnPlayerSpawned(player);
    }

    public override Task OnGameStateChanged(GameState oldState, GameState newState)
    {
        if (newState == GameState.Playing)
            StartRegionManager();

        if (newState == GameState.EndingGame)
            StopRegionManager();
        return base.OnGameStateChanged(oldState, newState);
    }

    private void HandlePlayerInRegion(BattleBitPlayer player, RegionHelper.Region region)
    {
        var now = DateTime.UtcNow;
        var spawnedInSpawn = player.GetPlayerProperty(IPlayerProperties.IGeneralPlayerProperties.SpawnedInSpawn);
        var spawnedInSpawnTimeStr = player.GetPlayerProperty(IPlayerProperties.IGeneralPlayerProperties.SpawnedInSpawnTime);
        var enteredSpawnTimeStr = player.GetPlayerProperty(IPlayerProperties.IGeneralPlayerProperties.EnteredSpawnTime);

        if (spawnedInSpawn == "true")
        {
            if (DateTime.TryParse(spawnedInSpawnTimeStr, out var spawnedInSpawnTime))
            {
                if ((now - spawnedInSpawnTime).TotalSeconds >= 90)
                {
                    player.Kill();
                    player.Message($"You spawned in the {region.Name} and stayed for too long!", 15f);
                }
                else if ((now - spawnedInSpawnTime).TotalSeconds >= 10)
                {
                    player.Message($"You spawned in the {region.Name}, please leave within {(int)(90 - (now - spawnedInSpawnTime).TotalSeconds)} seconds!", 1f);
                }
                
                player.SetPlayerProperty(IPlayerProperties.IGeneralPlayerProperties.LastMessageSentTime, now.ToUniversalTime().ToString());
            }
        }
        else
        {
            if (enteredSpawnTimeStr == string.Empty)
            {
                player.SetPlayerProperty(IPlayerProperties.IGeneralPlayerProperties.EnteredSpawnTime, now.ToUniversalTime().ToString());
            }
            else if (DateTime.TryParse(enteredSpawnTimeStr, out var enteredSpawnTime))
            {
                if ((now - enteredSpawnTime).TotalSeconds >= 20)
                {
                    player.Kill();
                    player.Message($"You were in the {region.Name} for too long!", 15f);
                }
                else
                {
                    player.Message($"You entered the {region.Name}, please leave within {(int)(20 - (now - enteredSpawnTime).TotalSeconds)} seconds!", 1f);
                }
                
                player.SetPlayerProperty(IPlayerProperties.IGeneralPlayerProperties.LastMessageSentTime, now.ToUniversalTime().ToString());
            }
        }
    }
}