using Newtonsoft.Json.Linq;

namespace SimpleOpenAi;

public static partial class OpenAi
{
    public static class Models
    {
        public static async Task<IEnumerable<string>> ListAsync(CancellationToken cancellationToken = default)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"{Base}/models");
            httpRequest.Headers.Authorization = new("Bearer", Key);
            var modelsResponse = await HttpClient.SendAsync(httpRequest, cancellationToken);
            modelsResponse.EnsureSuccessStatusCode();
            var data = JObject.Parse(await modelsResponse.Content.ReadAsStringAsync(cancellationToken))["data"]!;
            return data.Select(m => m["id"]!.Value<string>()!);
        }
    }
}