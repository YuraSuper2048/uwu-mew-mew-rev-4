using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Discord;
using Discord.WebSocket;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using uwu_mew_mew_4.Internal;
using uwu_mew_mew.Misc;
using Color = SixLabors.ImageSharp.Color;

namespace uwu_mew_mew_4.Handlers;

public class ImageGeneration
{
    public static async Task GenerateImage(SocketSlashCommand arg)
    {
        var args = arg.Data.Options;
        var prompt = (string)args.First(a => a.Name == "prompt").Value;
        var cfgScale = args.Any(a => a.Name == "cfg_scale")
            ? (double)args.First(a => a.Name == "cfg_scale").Value
            : StableDiffusion.DefaultCfg;
        var samplingSteps = args.Any(a => a.Name == "steps")
            ? (int)(long)args.First(a => a.Name == "steps").Value
            : StableDiffusion.DefaultSteps;
        var seed = args.Any(a => a.Name == "seed")
            ? (long)(double)args.First(a => a.Name == "seed").Value
            : -1;
        var aspectRatio = args.Any(a => a.Name == "ratio")
            ? (string)args.First(a => a.Name == "ratio").Value
            : "1:1";
        
        if(arg.Channel is IGuildChannel guildChannel)
            Logger.WriteLine($"{arg.User.Username}@{guildChannel.Guild.Name} -> /image prompt:{prompt} steps:{samplingSteps} ratio:{aspectRatio} ");
        else
            Logger.WriteLine($"{arg.User.Username}@dm -> /image prompt:{prompt} steps:{samplingSteps} ratio:{aspectRatio} ");
        
        var message = await arg.FollowupAsync(embed: generationEmbed.Build(), ephemeral: false);

        var stopwatch = Stopwatch.StartNew();
        
        var (image, newSeed) = await StableDiffusion.GenerateImage(prompt, cfgScale, samplingSteps, seed, aspectRatio);

        var link = StableDiffusion.Upload(image);

        await message.ModifyAsync(m => m.Embed = GetFinalEmbed(link, image, newSeed, stopwatch.Elapsed).Build());
    }

    private static readonly EmbedBuilder generationEmbed = new EmbedBuilder()
        .WithColor(255, 192, 203)
        .WithTitle("Hold on mastew~ uwu")
        .WithDescription("Im wowrking as hawd as i can... :cat: mew")
        .WithCurrentTimestamp();
    
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public static Vector3 RgbaToHsv(Rgba32 rgba)
    {
        float r = rgba.R / 255f, g = rgba.G / 255f, b = rgba.B / 255f;
        float max = Math.Max(Math.Max(r, g), b), min = Math.Min(Math.Min(r, g), b), diff = max - min;
        float h = max == min ? 0 : max == r ? (60 * ((g - b) / diff + 6)) % 360 : max == g ? 60 * ((b - r) / diff + 2) : 60 * ((r - g) / diff + 4);
        return new(h, max == 0 ? 0 : diff / max, max);
    }

    private static EmbedBuilder GetFinalEmbed(string link, byte[] image, long seed, TimeSpan time)
    {
        using var memoryStream = new MemoryStream(image);
        var decodedImage = PngDecoder.Instance.Decode<Rgba32>(new(), memoryStream);
        decodedImage.Mutate(i => i.Quantize(new OctreeQuantizer(new(){MaxColors=256})));
        var heat = new Dictionary<Rgba32, int>();
        decodedImage.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var pixelRow = accessor.GetRowSpan(y);

                // ReSharper disable once ForCanBeConvertedToForeach
                for (var x = 0; x < pixelRow.Length; x++)
                {
                    if(!heat.ContainsKey(pixelRow[x]))
                        heat.Add(pixelRow[x], 1);
                    else
                        heat[pixelRow[x]] += 1;
                }
            }
        });

        var bestColor = new Rgba32();
        var minBrightness = 0.6f;
        var minSaturation = 0.7f;
        var maxBrightness = 0.9f;
        var maxFrequency = 0;

        foreach (var color in heat.Keys)
        {
            var hsv = RgbaToHsv(color);

            if (hsv.Y < minSaturation || hsv.Z < minBrightness) continue;
            if (hsv.Z > maxBrightness) continue;
            if (heat[color] <= maxFrequency) continue;
            
            maxFrequency = heat[color];
            bestColor = color;
        }

        if (maxFrequency == 0)
        {
            foreach (var color in heat.Keys)
            {
                var hsv = RgbaToHsv(color);

                if (hsv.Z < minBrightness) continue;
                if (heat[color] <= maxFrequency) continue;
            
                maxFrequency = heat[color];
                bestColor = color;
            }
        }
        
        return new EmbedBuilder()
            .WithAuthor("uwu mew mew~", "https://storage.googleapis.com/uwu-mew-mew/sbGPT.png")
            .WithTitle($"Done in {time.TotalSeconds:F1}s!")
            .WithDescription($"[Direct link]({link})\n**Seed:** {seed}")
            .WithImageUrl(link)
            .WithColor(bestColor.R, bestColor.G, bestColor.B);
    }
}