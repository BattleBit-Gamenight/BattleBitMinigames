using BattleBitAPI.Common;
using BattleBitMinigames.Api;

namespace BattleBitMinigames.Events;

public class ServerSettings : Event
{
    public override Task OnConnected()
    {
        foreach (var map in Server.MapRotation.GetMapRotation())
        {
            Server.MapRotation.RemoveFromRotation(map);
        }
        
        foreach (var gamemode in Server.GamemodeRotation.GetGamemodeRotation())
        {
            Server.GamemodeRotation.RemoveFromRotation(gamemode);
        }
        
        foreach (var map in Program.MapRotation)
        {
            Server.MapRotation.AddToRotation(map);
        }
        
        foreach (var gamemode in Program.GamemodeRotation)
        {
            Server.GamemodeRotation.AddToRotation(gamemode);
        }
        
        Server.ExecuteCommand("setspeedhackdetection false");
        Server.ExecuteCommand("setmaxping 999");

        var serverRotation = Server.MapRotation.GetMapRotation();
        Program.Logger.Info($"Loaded Map Rotation: {string.Join(", ", serverRotation)}");
        
        var gamemodeRotation = Server.GamemodeRotation.GetGamemodeRotation();
        Program.Logger.Info($"Loaded Gamemode Rotation: {string.Join(", ", gamemodeRotation)}");
        
        return Task.CompletedTask;
    }
}