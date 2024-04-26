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
        
        foreach (var Gamemode in Data.ServerSettings.GamemodeRotation)
        {
            Server.GamemodeRotation.AddToRotation(Gamemode);
        }
        
        return Task.CompletedTask;
    }
}