using Discord;
using Discord.WebSocket;
using uwu_mew_mew_4.Internal;
using uwu_mew_mew_4.Openai;

namespace uwu_mew_mew_4;

public static class Ai
{
    public static async Task HandleMessage(SocketUserMessage message)
    {
        var text = message.Content.TrimStart().RemoveStart("<@1109341287372554250>").Trim();
        var response = await OpenAi.Chat.GetChatCompletionAsync(
            new OpenAi.Chat.Message[]
        {
            new("user", text)
        });

        await message.ReplyAsync(response.Content);
    }
}