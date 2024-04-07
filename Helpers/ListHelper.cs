namespace BattleBitApi.Helpers;

public class ListHelper
{
    public static void ShuffleList<T>(IList<T> list)
    {
        var rng = new Random();
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }
    
    public static T GetRandomItem<T>(IReadOnlyList<T> itemList, Random random)
    {
        if (itemList.Count <= 0) return default!;

        var randomIndex = random.Next(0, itemList.Count);
        return itemList[randomIndex];
    }
}