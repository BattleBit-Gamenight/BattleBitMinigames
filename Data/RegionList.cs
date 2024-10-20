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
            "WineParadise" => new List<RegionHelper.Region?>
            {
              WineParadiseRegions.TeamASafeZone,
              WineParadiseRegions.TeamBSafeZone
            },
            "TensaTown" => new List<RegionHelper.Region?>
            {
                TensatownRegions.TeamASafeZone,
                TensatownRegions.TeamBSafeZone
            },
            "Dustydew" => new List<RegionHelper.Region?>
            {
                DustydewRegions.TeamASafeZone,
                DustydewRegions.TeamBSafeZone
            },
            "OilDunes" => new List<RegionHelper.Region?>
            {
                OildunesRegions.TeamASafeZone,
                OildunesRegions.TeamBSafeZone
            },
            _ => new List<RegionHelper.Region?>()
        };
    }
}