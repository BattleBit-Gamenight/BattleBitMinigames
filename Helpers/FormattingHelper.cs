using System.Numerics;

namespace BattleBitMinigames.Helpers;

public class FormattingHelper
{
    public static string GetFormattedTime(DateTime time)
    {
        return time.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }
    
    // Convert seconds to minutes and seconds and hide minutes if 0
    public static string GetFormattedTimeFromSeconds(int seconds)
    {
        var minutes = seconds / 60;
        var remainingSeconds = seconds % 60;
        return minutes == 0 ? $"{remainingSeconds}s" : $"{minutes}m {remainingSeconds}s";
    }
    
    public static float DistanceTo(Vector3 a, Vector3 b)
    {
        return (float)Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2) + Math.Pow(a.Z - b.Z, 2));
    }
}