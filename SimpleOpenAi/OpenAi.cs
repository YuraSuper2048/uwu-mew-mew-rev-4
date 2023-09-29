using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SimpleOpenAi;

public static partial class OpenAi
{
    public static readonly HttpClient HttpClient = new();
    public static string Base { get; set; } = (Environment.GetEnvironmentVariable("OPENAI_API_ENDPOINT") ?? "https://api.openai.com/v1").TrimEnd('/');
    public static string Key { get; set; } = Environment.GetEnvironmentVariable("OPENAI_API_KEY")!;
}