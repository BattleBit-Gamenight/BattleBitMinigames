namespace BattleBitApi.Helpers;

public class FormattingHelper
{
    public static string GetFormattedTime(DateTime time)
    {
        return time.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
    }
}