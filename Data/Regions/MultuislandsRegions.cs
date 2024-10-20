using System.Numerics;
using BattleBitMinigames.Handlers;

namespace BattleBitMinigames.Data.Regions;

public class MultuislandsRegions
{
    // US safe zone
    public static readonly RegionHelper.Region TeamASafeZone = new (
        "US Safe Zone",
        "You've entered the US Safe Zone. Please leave immediately.",
        new List<Vector2>
        {
            new(461, 427),
            new(749, -954),
            new(-954, -954),
            new(-954, -215),
            new(-772, -265),
            new(-735, -280),
            new(-666, -265),
            new(-542, -282),
            new(-491, -330),
            new(58, -400),
            new(277,-365)
        }
    );
    
    // RU safe zone region
    public static readonly RegionHelper.Region? TeamBSafeZone = new (
        "RU Safe Zone",
        "You've entered the RU Safe Zone. Please leave immediately.",
        new List<Vector2>
        {
            new (-476, 954),
            new (-394, 664),
            new (-383, 530),
            new (-259, 371),
            new (36, 415),
            new (206, 488),
            new (388, 522),
            new (443, 511),
            new (487, 516),
            new (533, 470),
            new (578, 376),
            new (690, 285),
            new (725, 285),
            new (954, 334),
            new (954, 954)
        }
    );
}