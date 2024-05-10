using System.Numerics;
using BattleBitMinigames.Api;
using BattleBitMinigames.Data;
using BattleBitMinigames.Handlers;
using BattleBitMinigames.Helpers;

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
                    var region = RegionHelper.GetIsPlayerInRegion(RegionList.GetMapRegions(Server.Map), player);
                    if (region != null)
                    {
                        // TODO: Remove in prod
                        player.Message($"You are in {RichTextHelper.Bold(true)}{region.Name}{RichTextHelper.Bold(false)}!", 999);
                        // TODO: Spawn task to kill player after X amount of time, make message prettier.
                    } 
                    else
                    {
                        player.Message("You are not in a region!", 1);
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