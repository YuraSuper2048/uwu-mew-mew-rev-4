using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using SimpleOpenAi;
using uwu_mew_mew_4.Internal;
using Nito.AsyncEx;

namespace uwu_mew_mew_4.Handlers;

public static class Ai
{
    private static readonly Dictionary<ulong, CancellationTokenSource> Generations = new();

    public static async Task HandleMessage(SocketUserMessage message)
    {
        if(message.Channel is IGuildChannel guildChannel)
            Logger.WriteLine($"{message.Author.Username}@{guildChannel.Guild.Name} -> \"{message.Content}\" ");
        else
            Logger.WriteLine($"{message.Author.Username}@dm -> \"{message.Content}\" ");
        
        var userId = message.Author.Id;
        
        var cancellationTokenSource = new CancellationTokenSource();
        if(Generations.TryGetValue(userId, out CancellationTokenSource? token))
        {
            token.Cancel();
            Generations.Remove(userId);
        }
        Generations.Add(userId, cancellationTokenSource);
        var cancellationToken = cancellationTokenSource.Token;
        
        var typing = message.Channel.EnterTypingState();
        try
        {
            var newText = message.Content.TrimStart().RemoveStart("<@1109341287372554250>").Trim();

            /*
            var inputModeration = await OpenAi.Moderations.CreateAsync(newText, cancellationToken: cancellationToken);
            if (inputModeration.Flagged)
            {
                var flaggedScores = inputModeration.CategoryScores.Where(kvp => inputModeration.Categories[kvp.Key]);
                await message.ReplyAsync("Flagged by moderation.\n" +
                                         $"{string.Join('\n', flaggedScores.Select(kvp => $"{kvp.Key}: {kvp.Value.ToString(CultureInfo.InvariantCulture)}"))}");
                return;
            }
            */

            var (userMessages, character) = await ChatDatabase.GetAsync(userId);
            userMessages ??= new();
            userMessages.Add(new("user", newText));
            character ??= "uwu mew mew";

            var messages = new List<OpenAi.Chat.Message>
            {
                new("system", character switch
                {
                    "uwu mew mew" => SystemPrompts.UwuMewMew,
                    "lordpandaspace" => SystemPrompts.Lordpandaspace,
                    "chatgpt" => SystemPrompts.ChatGpt,
                    _ => throw new ArgumentOutOfRangeException()
                })
            };
            messages.AddRange(userMessages);

            /*var userHash = Convert.ToBase64String(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(
                        $"uwumewmew{userId}{message.Author.Username}uwumrrp")));*/

            const string model = "gpt-4";
            
            // ReSharper disable once MethodSupportsCancellation
            var stream = OpenAi.Chat.CreateStreaming
                (messages, temperature: 0.6, model: model);
            
            var contentBuilder = new StringBuilder();

            var embed = GetEmbed(model, character, character switch
            {
                "uwu mew mew" => "https://storage.googleapis.com/uwu-mew-mew/sbGPT.png",
                "lordpandaspace" => "https://storage.googleapis.com/uwu-mew-mew/lordpandaspace.png",
                "chatgpt" => "https://storage.googleapis.com/uwu-mew-mew/chatgpt.png",
                _ => throw new ArgumentOutOfRangeException()
            }, messages.Count(m => m.role == "user"));
            
            userMessages.Add(new("assistant", ""));
            var streamMessage = await message.ReplyAsync(
                text: "mrrp meow... uwu...",
                embed: embed.Build(),
                components: new ComponentBuilder()
                    .WithButton(StopButton)
                    .Build());
            typing.Dispose();

            var stopwatch = Stopwatch.StartNew();

            var locked = false;
            
            // ReSharper disable once UseCancellationTokenForIAsyncEnumerable
            await foreach (var result in stream)
            {
                contentBuilder.Append(result.Content);
                userMessages.RemoveAt(userMessages.Count-1);
                userMessages.Add(new("assistant", contentBuilder.ToString()));

                async Task UpdateMessage()
                {
                    if (locked) return;
                    if (stopwatch.ElapsedMilliseconds < 500) return;

                    locked = true;
                    try
                    {
                        var options = new RequestOptions();
                        options.RetryMode = RetryMode.AlwaysFail;
                        await streamMessage.ModifyAsync(m =>
                        {
                            m.Embed = embed.WithDescription(contentBuilder.ToString()).Build();
                        }, options: options);
                    }
                    finally
                    {
                        locked = false;
                        stopwatch.Restart();
                    }

                    await ChatDatabase.SetAsync(userId, new(userMessages, character));
                }

                var task = UpdateMessage();
                
                if(cancellationToken.IsCancellationRequested)
                    break;
            }

            while (locked) await Task.Delay(1);

            await streamMessage.ModifyAsync(m =>
            {
                m.Content = "";
                m.Embed = embed.WithDescription(contentBuilder.ToString()).Build();
                m.Components = new ComponentBuilder()
                    .WithButton(ResetButton)
                    .WithButton(CharacterButton)
                    .Build();
            });
        }
        finally
        {
            Generations.Remove(userId);
            typing.Dispose();
        }
    }

    private static readonly ButtonBuilder ResetButton = 
        new("Reset", "ai-reset", ButtonStyle.Secondary, emote: Emoji.Parse(":broom:"));
    private static readonly ButtonBuilder CharacterButton = 
        new("Character", "ai-character", ButtonStyle.Secondary, emote: Emoji.Parse(":sparkles:"));
    private static readonly ButtonBuilder StopButton = 
        new("Stop", "ai-stop", ButtonStyle.Danger, emote: Emoji.Parse(":x:")); 

    private static EmbedBuilder GetEmbed(string modelName, string characterName, string pfpUrl, int messages) => new EmbedBuilder()
        .WithColor(Random.Shared.NextAsItem(Bot.ColorPalette))
        .WithAuthor("uwu mew mew~", "https://storage.googleapis.com/uwu-mew-mew/sbGPT.png")
        .WithFooter($"{messages} messages")
        .WithCurrentTimestamp()
        .WithTitle($"{modelName}/{characterName}")
        .WithThumbnailUrl(pfpUrl);

    private static readonly SelectMenuBuilder CharacterSelection = new SelectMenuBuilder()
        .WithOptions(new()
        {
            new("uwu mew mew~", "uwu mew mew", "Uwu catgirl"),
            new("lordpandaspace", "lordpandaspace", "Your submissive friend"),
            new("ChatGPT", "chatgpt", "Standard ChatGPT"),
        }).WithCustomId("ai-character-select")
        .WithMinValues(1).WithMaxValues(1);

    public static async Task Reset(ulong userId)
    {
        var chat = await ChatDatabase.GetAsync(userId);
        if (chat.Character == null)
        {
            await ChatDatabase.DeleteAsync(userId);
            return;
        }

        await ChatDatabase.SetAsync(userId, chat with { Messages = new() });
    }
    
    public static async Task Character(SocketMessageComponent component)
    {
        await component.FollowupAsync("meow~", components: new ComponentBuilder()
            .WithSelectMenu(CharacterSelection).Build());
    }

    public static async Task CharacterSelect(ulong userId, string character)
    {
        await ChatDatabase.SetAsync(userId, new(new(), character));
    }
    
    public static Task Stop(ulong userId)
    {
        Generations[userId].Cancel();
        return Task.CompletedTask;
    }
}
