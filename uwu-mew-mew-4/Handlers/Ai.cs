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
            var text = message.Content.TrimStart().RemoveStart("<@1109341287372554250>").Trim();

            var userMessages = await ChatDatabase.Get(message.Author.Id);
            userMessages.Add(new("user", text));

            var messages = new List<OpenAi.Chat.Message>
            {
                new("system", SystemPrompts.UwuMewMew)
            };
            messages.AddRange(userMessages);

            var response = await OpenAi.Chat.GetChatCompletionAsync(messages, temperature: 0.6);

            userMessages.Add(new("assistant", response.Content!));
            await ChatDatabase.Set(message.Author.Id, userMessages);

            await message.ReplyAsync(response.Content,
                components: new ComponentBuilder()
                    .WithButton("Reset", "reset", ButtonStyle.Secondary, Emoji.Parse(":broom:"))
                    .Build());
        }
        finally
        {
            typing.Dispose();
        }
    }

    public static async Task Reset(ulong userId)
    {
        await ChatDatabase.Delete(userId);
    }
}