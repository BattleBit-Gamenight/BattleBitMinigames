namespace BattleBitMinigames.Data;

public class ServerSettings
{
    public static readonly List<string> MapRotation = new()
    {
        "Salhan"
    };
    
    public static readonly List<string> GamemodeRotation = new()
    {
        "CashRun"
    };
    
    public static readonly List<string> ServerSizes = new()
    {
        "tiny",   // 8v8
        "small",  // 16v16
        "medium", // 32v32
        "big",    // 64v64
        "ultra"   // 127v127
    };
}