using System.Numerics;
using BattleBitMinigames.Data.Regions;
using BattleBitMinigames.Handlers;

namespace BattleBitMinigames.Data;

public class RegionList
{
    public static List<RegionHelper.Region?> GetMapRegions(string map)
    {
        return map switch
        {
            "Lonovo" => new List<RegionHelper.Region?>
            {
                LonovoRegions.TeamASafeZone,
                LonovoRegions.TeamBSafeZone
            },
            _ => new List<RegionHelper.Region?>()
        };
    }
}