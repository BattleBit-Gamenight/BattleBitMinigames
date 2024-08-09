using System.Numerics;
using BattleBitMinigames.Handlers;

namespace BattleBitMinigames.Data.Regions;

public class TensaTownRegions
{
    // US safe zone
    public static readonly RegionHelper.Region TeamASafeZone = new (
        "US Safe Zone",
        "You've entered the US Safe Zone. Please leave immediately.",
        new List<Vector2>
        {
            new (300, -125),
            new (265, -187),
            new (278, -242),
            new (246, -285),
            new (162, -272),
            new (110, -243),
            new (64, -226),
            new (-39, -193),
            new (-116, -192),
            new (-187, -233),
            new (-188, -244),
            new (-18, -389),
            new (306, -436),
            new (393, -367),
            new (430, -266),
            new (341, -155)
        }
    );

    // RU safe zone region
    public static readonly RegionHelper.Region? TeamBSafeZone = new (
        "RU Safe Zone",
        "You've entered the RU Safe Zone. Please leave immediately.",
        new List<Vector2>
        {
            new (-199, 86),
            new (-141, 111),
            new (-96, 185),
            new (-76, 230),
            new (-49, 312),
            new (-30, 317),
            new (38, 342),
            new (83, 352),
            new (132, 348),
            new (135, 248),
            new (207, 189),
            new (251, 161),
            new (324, 142),
            new (356, 213),
            new (-276, 277),
            new (194, 407),
            new (-178, 377),
            new (-277, 303),
            new (-274, 188)
        }
    );
}