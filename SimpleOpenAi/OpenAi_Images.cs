using System.Dynamic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SimpleOpenAi;

public static partial class OpenAi
{
    public static class Images
    {
        /// <summary>
        ///     Creates an image given a prompt.
        /// </summary>
        /// <param name="prompt">A text description of the desired image(s). The maximum length is 1000 characters.</param>
        /// <param name="n">The number of images to generate. Must be between 1 and 10.</param>
        /// <param name="size">The size of the generated images. Must be one of 256x256, 512x512, or 1024x1024.</param>
        /// <param name="response_format">The format in which the generated images are returned. Must be one of url or b64_json.</param>
        /// <param name="user">A unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse.</param>
        /// <returns>A task with value of <see cref="Result" /></returns>
        public static async Task<Result> CreateImage(string prompt, int n = 1,
            string size = "1024x1024", string responseFormat = "url",
            string? user = null, CancellationToken cancellationToken = default)
        {
            var requestBody = new Dictionary<string, object>()
            {
                { "prompt", prompt },
                { "n", n },
                { "size", size },
                { "response_format", responseFormat }
            };
            if(user != null) requestBody.Add("user", user);

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{Base}/images/generations");
            request.Content = content;
            request.Headers.Authorization = new("Bearer", Key);

            var response = await HttpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = JObject.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return new()
            {
                Raw = responseBody,
                Url = responseBody["data"]![0]!["url"]!.Value<string>()
            };
        }

        public struct Result
        {
            public JObject Raw;
            public string? Url;
            public string? FinishReason;
        }
    }
}