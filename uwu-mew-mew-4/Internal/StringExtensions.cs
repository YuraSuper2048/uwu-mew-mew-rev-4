namespace uwu_mew_mew_4.Internal;

internal static class StringExtensions
{
    public static string RemoveStart(this string s, string toRemove)
    {
        if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(toRemove) || !s.StartsWith(toRemove)) return s;
        return s.Substring(toRemove.Length);
    }
}