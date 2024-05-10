using System.Numerics;
using BattleBitMinigames.Data.Regions;

namespace BattleBitMinigames.Data;

public class RegionList
{
    public static List<List<Vector2>> GetMapRegions(string mapName)
    {
        return mapName switch
        {
            // Get the regions for the map "Lonovo"
            "Lonovo" => new List<List<Vector2>>
            {
                LonovoRegions.TeamASafeZone,
                LonovoRegions.TeamBSafeZone
            },
        };
    }
}