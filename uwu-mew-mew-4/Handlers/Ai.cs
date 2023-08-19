using System.Security.Cryptography;
using System.Text;
using Discord;
using Discord.WebSocket;
using uwu_mew_mew_4.Internal;
using uwu_mew_mew_4.Openai;

namespace uwu_mew_mew_4.Handlers;

public static class Ai
{
    public static async Task HandleMessage(SocketUserMessage message)
    {
        var typing = message.Channel.EnterTypingState();
        try
        {
            var inputMessage = message.Content.TrimStart().RemoveStart("<@1109341287372554250>").Trim();

            var userMessages = await ChatDatabase.GetAsync(message.Author.Id);
            userMessages.Add(new("user", inputMessage));

            var messages = new List<OpenAi.Chat.Message>
            {
                new("system", SystemPrompts.UwuMewMew)
            };
            messages.AddRange(userMessages);

            var userHash = Convert.ToBase64String(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(
                        message.Author.Id + "uwumewmew")));
            
            var stream = OpenAi.Chat.StreamChatCompletion
                (messages, temperature: 0.6, model: "gpt-3.5-turbo",
                    user: userHash, 
                    logitBias: new()
                    {
                        {"43210", 2},
                        {"23741", 2},
                        {"52", 1}
                    });
            
            var contentBuilder = new StringBuilder();

            var embed = GetEmbed(messages.Count(m => m.role == "user"));
            
            userMessages.Add(new("assistant", ""));
            var streamMessage = await message.ReplyAsync(
                text: "OwO thinking as hawd as i can~",
                embed: embed.Build(),
                components: new ComponentBuilder()
                    .WithButton(ResetButton)
                    .Build());
            
            await foreach (var result in stream)
            {
                contentBuilder.Append(result.Content);
                userMessages.RemoveAt(userMessages.Count-1);
                userMessages.Add(new("assistant", contentBuilder.ToString()));

                await ChatDatabase.SetAsync(message.Author.Id, userMessages);

                await streamMessage.ModifyAsync(m =>
                {
                    m.Embed = embed.WithDescription(contentBuilder.ToString()).Build();
                });
            }
            
            await streamMessage.ModifyAsync(m =>
            {
                m.Content = "";
            });
        }
        finally
        {
            typing.Dispose();
        }
    }

    private static readonly ButtonBuilder ResetButton = 
        new("Reset", "reset", ButtonStyle.Secondary, emote: Emoji.Parse(":broom:")); 

    private static EmbedBuilder GetEmbed(int messages) => new EmbedBuilder()
        .WithColor(Random.Shared.NextAsItem(Bot.ColorPalette))
        .WithAuthor("uwu mew mew~", "https://storage.googleapis.com/uwu-mew-mew/sbGPT.png")
        .WithFooter($"{messages} messages")
        .WithCurrentTimestamp()
        .WithTitle("uwu mew mew~")
        .WithThumbnailUrl("https://storage.googleapis.com/uwu-mew-mew/sbGPT.png");

    public static async Task Reset(ulong userId)
    {
        await ChatDatabase.DeleteAsync(userId);
    }
}