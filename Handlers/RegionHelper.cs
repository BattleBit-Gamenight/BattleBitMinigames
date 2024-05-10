using System.Numerics;
using BattleBitMinigames.Api;

namespace BattleBitMinigames.Handlers;

public class RegionHelper
{
    public static bool GetIsPlayerInRegion(List<List<Vector2>> zone, BattleBitPlayer player)
    {
        // Check if the player is in the regions
        return zone.Any(region => IsPointInPolygon(new Vector2(player.Position.X, player.Position.Z), region));
    }
    
    private static bool IsPointInPolygon(Vector2 point, List<Vector2> zone)
    {
        var result = false;
        var j = zone.Count - 1;

        for (var i = 0; i < zone.Count; i++)
        {
            if (zone[i].Y < point.Y && zone[j].Y >= point.Y || zone[j].Y < point.Y && zone[i].Y >= point.Y)
            {
                if (zone[i].X + (point.Y - zone[i].Y) / (zone[j].Y - zone[i].Y) * (zone[j].X - zone[i].X) < point.X)
                {
                    result = !result;
                }
            }

            j = i;
        }

        return result;
    }
}