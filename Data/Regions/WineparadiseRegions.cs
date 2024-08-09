using System.Numerics;
using BattleBitMinigames.Handlers;

namespace BattleBitMinigames.Data.Regions;

public class WineParadiseRegions
{
    // US safe zone
    public static readonly RegionHelper.Region TeamASafeZone = new (
        "US Safe Zone",
        "You've entered the US Safe Zone. Please leave immediately.",
        new List<Vector2>
        {
            new (-92, -271),
            new (-197, -142),
            new (-338, -18),
            new (-387, 190),
            new (-605, -27),
            new (-660, -275),
            new (-510, -372),
            new (-297, -352)
        }
    );
    
    // RU safe zone region
    public static readonly RegionHelper.Region? TeamBSafeZone = new (
        "RU Safe Zone",
        "You've entered the RU Safe Zone. Please leave immediately.",
        new List<Vector2>
        {
            new (-121, 397),
            new (0, 346),
            new (140, 182),
            new (313, 90),
            new (353, 124),
            new (380, 289),
            new (332, 455),
            new (160, 526),
            new (-54, 455)
        }
    );
}