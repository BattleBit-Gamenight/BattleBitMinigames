using System.Numerics;
using BattleBitMinigames.Api;

namespace BattleBitMinigames.Handlers;

public class RegionHelper
{
    public class Region
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public List<Vector2> Points { get; set; }

        public Region(string name, string message, List<Vector2> points)
        {
            Name = name;
            Message = message;
            Points = new List<Vector2>(points);
        }
    }
    
    public static Region? GetIsPlayerInRegion(List<Region?> regions, BattleBitPlayer player)
    {
        // Check if the player is in the regions
        return regions.FirstOrDefault(region => IsPointInPolygon(new Vector2(player.Position.X, player.Position.Z), region.Points));
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