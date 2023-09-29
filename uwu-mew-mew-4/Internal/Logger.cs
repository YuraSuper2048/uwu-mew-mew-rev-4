namespace uwu_mew_mew_4.Internal;

internal static class Logger
{
    public static void WriteLine(string text)
    {
        var log = $"[{DateTimeOffset.Now}] {text}\n";
        Console.Write(log);
        File.AppendAllText("log.txt", log);
    }
}