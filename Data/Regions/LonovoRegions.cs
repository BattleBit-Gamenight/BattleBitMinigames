using System.Numerics;

namespace BattleBitMinigames.Data.Regions;

public class LonovoRegions
{
    // US safe zone region
    public static readonly List<Vector2> TeamASafeZone = new()
    {
        new Vector2(-475, -242),
        new Vector2(-130f, -204),
        new Vector2(135, -204),
        new Vector2(475, -272),
        new Vector2(475, -475),
        new Vector2(-475, -475)
    };
    
    // RU safe zone region
    public static readonly List<Vector2> TeamBSafeZone = new()
    {
        new Vector2(-475, -242),
        new Vector2(-130f, -204),
        new Vector2(135, -204),
        new Vector2(475, -272),
        new Vector2(475, -475),
        new Vector2(-475, -475)
    };
}