namespace uwu_mew_mew_4.Openai;

public static partial class OpenAi
{
    private static readonly string Endpoint = Environment.GetEnvironmentVariable("OPENAI_API_ENDPOINT")!.TrimEnd('/');
    private static readonly string Key = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
}