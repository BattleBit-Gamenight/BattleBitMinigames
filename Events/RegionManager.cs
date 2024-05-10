using System.Numerics;
using BattleBitMinigames.Api;
using BattleBitMinigames.Data;
using BattleBitMinigames.Handlers;

namespace BattleBitMinigames.Events;

public class RegionManager : Event
{
    bool Enabled = true;
    
    private void StartRegionManager()
    {
        Task.Run(async () =>
        {
            while (Server.IsConnected)
            {
                foreach (var player in Server.AllPlayers.Where(player => player.IsAlive && player.Position != Vector3.Zero))
                {
                    if (RegionHelper.GetIsPlayerInRegion(RegionList.GetMapRegions(Server.Map), player))
                    {
                        // TODO: Remove in prod
                        player.Message("You are in a safe zone!", 999);
                        // TODO: Spawn task to kill player after X amount of time, make message prettier.
                    } 
                    else
                    {
                        player.Message("You are not in a safe zone!", 1);
                    }
                }

                await Task.Delay(1000);
            }
        });
    }
    
    public override Task OnConnected()
    {
        StartRegionManager();
        return Task.CompletedTask;
    }
}