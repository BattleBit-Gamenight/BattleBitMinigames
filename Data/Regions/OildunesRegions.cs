using System.Numerics;
using BattleBitMinigames.Handlers;

namespace BattleBitMinigames.Data.Regions;

public class OildunesRegions
{
    // US safe zone
    public static readonly RegionHelper.Region TeamASafeZone = new(
        "US Safe Zone",
        "You've entered the US Safe Zone. Please leave immediately.",
        new List<Vector2>
        {
            new(-225, -215),
            new(-213, -97),
            new(-224, -56),
            new(-208, -20),
            new(-190, 110),
            new(-189, 149),
            new(-51, 209),
            new(-40, 278),
            new(-268, 279),
            new(-370, 75),
            new(-373, -69),
            new(-267, -229)
        }
    );

    // RU safe zone region
    public static readonly RegionHelper.Region? TeamBSafeZone = new(
        "RU Safe Zone",
        "You've entered the RU Safe Zone. Please leave immediately.",
        new List<Vector2>
        {
            new(192, 233),
            new(207, 230),
            new(230, 266),
            new(326, 178),
            new(368, 55),
            new(367, -55),
            new(234, -293),
            new(-30, -299),
            new(9, -220),
            new(73, -200),
            new(141, -152),
            new(177, -62),
            new(168, 62),
            new(175, 181)
        }
    );
}