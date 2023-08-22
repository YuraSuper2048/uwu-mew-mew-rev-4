using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SimpleOpenAi;

public static partial class OpenAi
{
    public static class Completions
    {
        public static async Task<Result> CreateAsync(string prompt, string model = "text-davinci-003",
            double temperature = 1, int maxTokens = 16, double topP = 1, int n = 1,
            int? logprobs = null, bool? echo = null, object? stop = null,
            double presencePenalty = 0, double frequencyPenalty = 0,
            Dictionary<string, double>? logitBias = null, string? user = null,
            CancellationToken cancellationToken = default)
        {
            var requestBody = new Dictionary<string, object>
            {
                { "prompt", prompt },
                { "model", model },
                { "temperature", temperature },
                { "top_p", topP },
                { "n", n },
                { "presence_penalty", presencePenalty },
                { "frequency_penalty", frequencyPenalty },
                { "max_tokens", maxTokens }
            };

            if (stop != null) requestBody.Add("stop", stop);
            if (logitBias != null) requestBody.Add("logit_bias", logitBias);
            if (logprobs != null) requestBody.Add("logprobs", logprobs);
            if (echo != null) requestBody.Add("echo", echo);
            if (user != null) requestBody.Add("user", user);

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{Base}/completions");
            request.Content = content;
            request.Headers.Authorization = new("Bearer", Key);

            var response = await HttpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = JObject.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            return new()
            {
                Raw = responseBody,
                Text = responseBody["choices"]![0]!["text"]!.Value<string>(),
                FinishReason = responseBody["choices"]![0]!["finish_reason"]!.Value<string>()
            };
        }

        public static async IAsyncEnumerable<Result> CreateStreaming(string prompt, string model = "text-davinci-003",
            double temperature = 1, int maxTokens = 16, double topP = 1, int n = 1,
            int? logprobs = null, bool? echo = null, object? stop = null,
            double presencePenalty = 0, double frequencyPenalty = 0,
            Dictionary<string, double>? logitBias = null, string? user = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var requestBody = new Dictionary<string, object>
            {
                { "prompt", prompt },
                { "model", model },
                { "temperature", temperature },
                { "top_p", topP },
                { "n", n },
                { "presence_penalty", presencePenalty },
                { "frequency_penalty", frequencyPenalty },
                { "max_tokens", maxTokens },
                { "stream", true }
            };

            if (stop != null) requestBody.Add("stop", stop);
            if (logitBias != null) requestBody.Add("logit_bias", logitBias);
            if (logprobs != null) requestBody.Add("logprobs", logprobs);
            if (echo != null) requestBody.Add("echo", echo);
            if (user != null) requestBody.Add("user", user);

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{Base}/completions");
            request.Content = content;
            request.Headers.Authorization = new("Bearer", Key);

            var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var reader = new StreamReader(stream);

            while (await reader.ReadLineAsync(cancellationToken) is { } line)
            {
                if (!line.ToLower().StartsWith("data:")) continue;

                var dataString = line[5..].Trim();

                if (dataString == "[DONE]") break;

                var data = JObject.Parse(dataString);

                yield return new()
                {
                    Raw = data,
                    Text = data["choices"]?[0]?["text"]?.ToString(),
                    FinishReason = data["choices"]?[0]?["finish_reason"]?.ToString()
                };
            }

            reader.Dispose();
        }

        public struct Result
        {
            public JObject Raw;
            public string? Text;
            public string? FinishReason;
        }
    }
}