using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using Google.Cloud.Storage.V1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace uwu_mew_mew.Misc;

internal class StableDiffusion
{
    private static readonly HttpClient HttpClient = new();
    
    public record struct GenerationResult(byte[] image, long seed);
    
    public const double DefaultCfg = 5;
    public const int DefaultSteps = 60;
    
    public static async Task<GenerationResult> GenerateImage(string prompt, double cfgScale = DefaultCfg, int steps = DefaultSteps, long seed = -1, string aspectRatio = "1:1")
    {
        var aspectRatioSplit = aspectRatio.Split(':');
        var aspectRatioValue = (double)int.Parse(aspectRatioSplit[0]) / int.Parse(aspectRatioSplit[1]);
        var width = aspectRatioValue * 512;
        var payload = new
        {
            prompt,
            negative_prompt = "nsfw, naked, lewd",
            steps,
            cfg_scale = cfgScale,
            sampler_name = "DPM++ 3M SDE Karras",
            seed,
            width,
            height = 512
        };

        var jsonPayload = JsonConvert.SerializeObject(payload);
        var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Post, txt2imgUrl);
        request.Content = httpContent;
        var response = await HttpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode) return new(Array.Empty<byte>(), -1);
    
        var result = JObject.Parse(await response.Content.ReadAsStringAsync());
        var image = Convert.FromBase64String(((JArray)result["images"]!).First().Value<string>()!);

        return new(image, (long)JObject.Parse(result["info"]!.Value<string>()!)["seed"]!);
    }

    public static string Upload(byte[] image)
    {
        using var uploadStream = new MemoryStream(image);

        var storageClient = StorageClient.Create();
        var obj = storageClient.UploadObject("uwu-mew-mew", $"{Guid.NewGuid()}.png", "image/png", uploadStream);
        return obj.MediaLink;
    }
    
    private const string txt2imgUrl = "http://127.0.0.1:7860/sdapi/v1/txt2img";
}