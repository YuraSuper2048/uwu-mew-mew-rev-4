namespace uwu_mew_mew_4.Internal;

internal static class RandomExtensions
{
    public static T RandomItem<T>(this Random random, IList<T> collection)
    {
        return collection[random.Next(0, collection.Count)];
    } 
}