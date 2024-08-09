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
                        if (Program.ServerConfiguration.LaunchCustomGamemode == "vip")
                        {
                            var isVip = player.GetPlayerProperty(PlayerProperties.IVipPlayerProperties.IsVip);
                            if (isVip != "true") continue;

                            var now = DateTime.UtcNow;
                            var spawnedInSpawn =
                                player.GetPlayerProperty(PlayerProperties.IVipPlayerProperties.SpawnedInSpawn);
                            var spawnedInSpawnTimeStr =
                                player.GetPlayerProperty(PlayerProperties.IVipPlayerProperties.SpawnedInSpawnTime);
                            var enteredSpawnTimeStr =
                                player.GetPlayerProperty(PlayerProperties.IVipPlayerProperties.EnteredSpawnTime);

                            if (spawnedInSpawn == "true")
                            {
                                if (DateTime.TryParse(spawnedInSpawnTimeStr, out var spawnedInSpawnTime))
                                {
                                    if ((now - spawnedInSpawnTime).TotalSeconds >= 90)
                                    {
                                        player.Kill();
                                        player.Message($"You spawned in the {region.Name} and stayed for too long!",
                                            15);
                                    }
                                    else if ((now - spawnedInSpawnTime).TotalSeconds >= 10)
                                    {
                                        player.Message(
                                            $"You spawned in the {region.Name}, please leave within {(int)(90 - (now - spawnedInSpawnTime).TotalSeconds)} seconds!",
                                            1f);
                                    }
                                }
                            }
                            else
                            {
                                if (enteredSpawnTimeStr == string.Empty)
                                {
                                    player.SetPlayerProperty(PlayerProperties.IVipPlayerProperties.EnteredSpawnTime,
                                        now.ToUniversalTime().ToString());
                                }
                                else if (DateTime.TryParse(enteredSpawnTimeStr, out var enteredSpawnTime))
                                {
                                    if ((now - enteredSpawnTime).TotalSeconds >= 20)
                                    {
                                        player.Kill();
                                        player.Message($"You were in the {region.Name} for too long!", 15);
                                    }
                                    else
                                    {
                                        player.Message(
                                            $"You entered the {region.Name}, please leave within {(int)(20 - (now - enteredSpawnTime).TotalSeconds)} seconds!",
                                            1f);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        // Reset properties when a player leaves the spawn region
                        player.RemovePlayerProperty(PlayerProperties.IVipPlayerProperties.SpawnedInSpawn);
                        player.RemovePlayerProperty(PlayerProperties.IVipPlayerProperties.EnteredSpawnTime);
                        player.RemovePlayerProperty(PlayerProperties.IVipPlayerProperties.SpawnedInSpawnTime);
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
            player.SetPlayerProperty(PlayerProperties.IVipPlayerProperties.SpawnedInSpawn, "true");
            player.SetPlayerProperty(PlayerProperties.IVipPlayerProperties.SpawnedInSpawnTime,
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
}