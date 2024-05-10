using System.Numerics;
using BattleBitMinigames.Handlers;

namespace BattleBitMinigames.Data.Regions;

public class LonovoRegions
{
    // US safe zone
    public static readonly RegionHelper.Region TeamASafeZone = new (
        "US Safe Zone",
        "You've entered the US Safe Zone. Please leave immediately.",
        new List<Vector2>
        {
            new (-475, -242),
            new (-130f, -204),
            new (135, -204),
            new (475, -272),
            new (475, -475),
            new (-475, -475)
        }
    );
    
    // RU safe zone region
    public static readonly RegionHelper.Region? TeamBSafeZone = new (
        "RU Safe Zone",
        "You've entered the RU Safe Zone. Please leave immediately.",
        new List<Vector2>
        {
            new (-475, -242),
            new (-130f, -204),
            new (135, -204),
            new (475, -272),
            new (475, -475),
            new (-475, -475)
        }
    );
}