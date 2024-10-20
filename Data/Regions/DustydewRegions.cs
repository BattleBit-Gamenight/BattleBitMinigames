using System.Numerics;
using BattleBitMinigames.Handlers;

namespace BattleBitMinigames.Data.Regions;

public class DustydewRegions
{
    public static readonly RegionHelper.Region TeamASafeZone = new(
        "US Safe Zone",
        "You've entered the US Safe Zone. Please leave immediately.",
        new List<Vector2>
        {
            new(-103, -548),
            new(-267, -547),
            new(-458, -363),
            new(-484, -71),
            new(-362, 180),
            new(-327, 80),
            new(-285, 28),
            new(-283, -15),
            new(-364, -48),
            new(-375, -73),
            new(-345, -104),
            new(-346, -157),
            new(-360, -203),
            new(-348, -252),
            new(-320 - 280),
            new(-293, -353),
            new(-204, -404),
            new(-254, -448),
            new(-143, -485)
        }
    );

    // RU safe zone region
    public static readonly RegionHelper.Region? TeamBSafeZone = new(
        "RU Safe Zone",
        "You've entered the RU Safe Zone. Please leave immediately.",
        new List<Vector2>
        {
            new(-216, 474),
            new(-93, 430),
            new(42, 411),
            new(142, 416),
            new(191, 365),
            new(216, 396),
            new(277, 354),
            new(276, 244),
            new(355, 153),
            new(357, 80),
            new(309, -8),
            new(403, -162),
            new (546, 219), 
            new (258, 565),
            new (-133, 653)
        }
    );
}