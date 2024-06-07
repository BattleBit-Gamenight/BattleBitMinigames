namespace BattleBitMinigames.Helpers;

public class GamemodeHelper
{
    private static List<string> GameModes { get; } = new()
    {
        "TDM",
        "AAA",
        "RUSH",
        "CONQ",
        "DOMI",
        "ELIM",
        "INFCONQ",
        "FRONTLINE",
        "GUNGAMEFFA",
        "FFA",
        "GUNGAMETEAM",
        "SUCIDERUSH",
        "CATCH",
        "INFECTED",
        "CASHRUN",
        "VOXELFORTIFY",
        "VOXELTRENCH",
        "CTF",
        "INVASION",
        "ONELIFE"
    };
    
    public static bool IsValidGamemode(string gm)
    {
        return GameModes.Contains(gm.ToUpperInvariant());
    }
}