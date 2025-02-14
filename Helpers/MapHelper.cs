namespace BattleBitMinigames.Helpers;

public class MapHelper
{
    private static List<string> Maps { get; } = new()
    {
        "DEVTEST",
        "AZAGOR",
        "DISTRICT",
        "WAKISTAN",
        "NAMAK",
        "SALHAN",
        "MULTUISLANDS",
        "SANDYSUNSET",
        "EDUARDOVO",
        "DUSTYDEW",
        "POLYGON",
        "WINEPARADISE",
        "LONOVO",
        "CONSTRUCTION",
        "OILDUNES",
        "BASRA",
        "RIVER",
        "VALLEY",
        "FRUGIS",
        "VOXELLAND",
        "ISLE",
        "TENSATOWN",
        "OLD_DISTRICT",
        "OLD_EDUARDOVO",
        "OLD_MULTUISLANDS",
        "OLD_NAMAK",
        "OLD_OILDUNES",
        "OLD_SANDYSUNSET",
        "OLD_WAKISTAN",
        "OLD_WINEPARADISE",
        "ZALFIBAY",
        "KODIAK",
        "EVENTMAP",
        "OUTSKIRTS",
        "COMPETETIVEAREA",
    };
    
    public static bool IsValidMap(string map)
    {
        return Maps.Contains(map.ToUpperInvariant());
    }
    
    public static List<string> GetAllMaps()
    {
        return Maps;
    }
}