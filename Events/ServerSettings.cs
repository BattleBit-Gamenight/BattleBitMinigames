using BattleBitMinigames.Api;

namespace BattleBitMinigames.Events;

public class ServerSettings : Event
{
    public override Task OnConnected()
    {
        foreach (var Map in Data.ServerSettings.MapRotation)
        {
            Server.MapRotation.AddToRotation(Map);
        }
        
        return Task.CompletedTask;
    }
}