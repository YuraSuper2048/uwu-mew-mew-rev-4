using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using uwu_mew_mew_4.Internal;

namespace uwu_mew_mew_4.Openai;

public static partial class OpenAi
{
    public static class Chat
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public record Message(string role, string content, string name = "", JObject? function_call = null);

        public struct Result
        {
            public JObject Raw;
            public string? Content;
            public string? FinishReason;
            public string? FunctionCall;
        }
        
        /// <summary>
        ///  Get chat completion from the OpenAI API.
        /// </summary>
        /// <param name="messages">A list of messages comprising the conversation so far.</param>
        /// <param name="model">ID of the model to use.</param>
        /// <param name="temperature">What sampling temperature to use, between 0 and 2.</param>
        /// <param name="topP">An alternative to sampling with temperature, where the model considers the results of the tokens with top_p probability mass.</param>
        /// <param name="n">How many chat completion choices to generate for each input message.</param>
        /// <param name="stop">Up to 4 sequences where the API will stop generating further tokens.</param>
        /// <param name="maxTokens">The maximum number of tokens to generate in the chat completion.</param>
        /// <param name="presencePenalty">Number between -2.0 and 2.0. Positive values penalize new tokens based on whether they appear in the text so far,</param>
        /// <param name="frequencyPenalty">Number between -2.0 and 2.0. Positive values penalize new tokens based on their existing frequency in the text so far,</param>
        /// <param name="logitBias">Modify the likelihood of specified tokens appearing in the completion.</param>
        /// <param name="user">A unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse.</param>
        /// <param name="functions">A list of functions the model may generate JSON inputs for.</param>
        /// <returns>A task with value of <see cref="Result"/></returns>
        public static async Task<Result> GetChatCompletionAsync(IEnumerable<Message> messages,
            string model = "gpt-3.5-turbo", double temperature = 1, double topP = 1, int n = 1,
            object stop = null, int? maxTokens = null, double presencePenalty = 0, double frequencyPenalty = 0, 
            Dictionary<string, double>? logitBias = null, string? user = null, IReadOnlyList<JObject>? functions = null)
        {
            var requestBody = new Dictionary<string, object>
            {
                { "model", model },
                { "temperature", temperature },
                { "top_p", topP },
                { "n", n },
                { "presence_penalty", presencePenalty },
                { "frequency_penalty", frequencyPenalty }
            };
            
            if(maxTokens != null) requestBody.Add("max_tokens", maxTokens);
            if(stop != null) requestBody.Add("stop", stop);
            if(logitBias != null) requestBody.Add("logit_bias", logitBias);
            if(user != null) requestBody.Add("user", user);

            var messageList = new List<object>();
            foreach (var chatMessage in messages)
            {
                var message = new Dictionary<string, object>
                {
                    { "role", chatMessage.role },
                    { "content", chatMessage.content }
                };

                if (chatMessage.function_call != null)
                    message.Add("function_call", chatMessage.function_call);

                if (chatMessage.name != null)
                    message.Add("name", chatMessage.name);

                messageList.Add(message);
            }

            requestBody.Add("messages", messageList);

            if (functions != null)
            {
                requestBody.Add("functions", functions);
                requestBody.Add("function_call", "auto");
            }

            string json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{Endpoint}/chat/completions");
            request.Content = content;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Key);

            var response = await HttpClientFactory.Instance.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());
            return new Result
            {
                Raw = responseBody,
                Content = responseBody["choices"]?[0]?["message"]?["content"]?.ToString(),
                FinishReason = responseBody["choices"]?[0]?["finish_reason"]?.ToString(),
                FunctionCall = responseBody["choices"]?[0]?["message"]?["function_call"]?.ToString()
            };
        }

        /// <summary>
        /// Streams chat completion from the OpenAI API token by token.
        /// </summary>
        /// <param name="messages">A list of messages comprising the conversation so far.</param>
        /// <param name="model">ID of the model to use.</param>
        /// <param name="temperature">What sampling temperature to use, between 0 and 2.</param>
        /// <param name="topP">An alternative to sampling with temperature, where the model considers the results of the tokens with top_p probability mass.</param>
        /// <param name="n">How many chat completion choices to generate for each input message.</param>
        /// <param name="stop">Up to 4 sequences where the API will stop generating further tokens.</param>
        /// <param name="maxTokens">The maximum number of tokens to generate in the chat completion.</param>
        /// <param name="presencePenalty">Number between -2.0 and 2.0. Positive values penalize new tokens based on whether they appear in the text so far</param>
        /// <param name="frequencyPenalty">Number between -2.0 and 2.0. Positive values penalize new tokens based on their existing frequency in the text so far</param>
        /// <param name="logitBias">Modify the likelihood of specified tokens appearing in the completion.</param>
        /// <param name="user">A unique identifier representing your end-user, which can help OpenAI to monitor and detect abuse.</param>
        /// <param name="functions">A list of functions the model may generate JSON inputs for.</param>
        /// <returns>An IAsyncEnumerable of <see cref="Result"/>.</returns>
        public static async IAsyncEnumerable<Result> StreamChatCompletion(IEnumerable<Message> messages,
            string model = "gpt-3.5-turbo", double temperature = 1, double topP = 1, int n = 1,
            object stop = null, int? maxTokens = null, double presencePenalty = 0, double frequencyPenalty = 0,
            Dictionary<string, double>? logitBias = null, string? user = null, IReadOnlyList<JObject>? functions = null)
        {
            var requestBody = new Dictionary<string, object>
            {
                { "model", model },
                { "temperature", temperature },
                { "top_p", topP },
                { "n", n },
                { "presence_penalty", presencePenalty },
                { "frequency_penalty", frequencyPenalty },
                { "stream", true }
            };

            if(maxTokens != null) requestBody.Add("max_tokens", maxTokens);
            if(stop != null) requestBody.Add("stop", stop);
            if(logitBias != null) requestBody.Add("logit_bias", logitBias);
            if(user != null) requestBody.Add("user", user);

            var messageList = new List<object>();
            foreach (var chatMessage in messages)
            {
                var message = new Dictionary<string, object>
                {
                    { "role", chatMessage.role },
                    { "content", chatMessage.content }
                };

                if (chatMessage.function_call != null)
                    message.Add("function_call", chatMessage.function_call);

                if (chatMessage.name != null)
                    message.Add("name", chatMessage.name);

                messageList.Add(message);
            }

            requestBody.Add("messages", messageList);

            if (functions != null)
            {
                requestBody.Add("functions", functions);
                requestBody.Add("function_call", "auto");
            }

            var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"{Endpoint}/chat/completions");
            request.Content = content;
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Key);

            var response = await HttpClientFactory.Instance.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            var stream = await response.Content.ReadAsStreamAsync();
            var reader = new StreamReader(stream);

            while (await reader.ReadLineAsync() is { } line)
            {
                if (!line.ToLower().StartsWith("data:")) continue;

                var dataString = line[5..].Trim();

                var data = JObject.Parse(dataString);

                yield return new Result
                {
                    Raw = data,
                    Content = data["choices"]?[0]?["delta"]?["content"]?.ToString(),
                    FinishReason = data["choices"]?[0]?["finish_reason"]?.ToString(),
                    FunctionCall = data["choices"]?[0]?["delta"]?["function_call"]?.ToString()
                };
            }
            
            reader.Dispose();
        }

    }
}