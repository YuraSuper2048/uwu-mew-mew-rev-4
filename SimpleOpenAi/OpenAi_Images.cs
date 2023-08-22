using System.Dynamic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SimpleOpenAi;

public static partial class OpenAi
{
    public static class Images
    {
        public static async Task<string> GenerateImage(string prompt, CancellationToken cancellationToken = default)
        {
            dynamic requestBody = new ExpandoObject();
            requestBody.prompt = prompt;

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{Base}/images/generations");
            request.Content = content;
            request.Headers.Authorization = new("Bearer", Key);

            var response = await HttpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = JObject.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return responseBody["data"]![0]!["url"]!.Value<string>()!;
        }
    }
}