using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SimpleOpenAi;

public static partial class OpenAi
{
    public static class Moderations
    {
        public struct Result
        {
            public JObject Raw;
            public bool Flagged;
            public Dictionary<string, bool> Categories;
            public Dictionary<string, double> CategoryScores;
        }

        public static async Task<Result> CreateAsync(string input, CancellationToken cancellationToken = default)
        {
            var requestBody = new Dictionary<string, object>
            {
                { "input", input }
            };
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{Base}/moderations");

            var requestJson = JsonConvert.SerializeObject(requestBody);
            httpRequest.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
            httpRequest.Headers.Authorization = new("Bearer", Key);

            var modelsResponse = await HttpClient.SendAsync(httpRequest, cancellationToken);
            modelsResponse.EnsureSuccessStatusCode();

            var responseBody = JObject.Parse(await modelsResponse.Content.ReadAsStringAsync(cancellationToken));
            return new()
            {
                Raw = responseBody,
                Flagged = responseBody["results"]![0]!["flagged"]!.ToObject<bool>(),
                Categories = responseBody["results"]![0]!["categories"]!.ToObject<Dictionary<string, bool>>()!,
                CategoryScores = responseBody["results"]![0]!["category_scores"]!.ToObject<Dictionary<string, double>>()!
            };
        }
    }
}