using System.Collections.Concurrent;
using Discord;
using Discord.WebSocket;
using uwu_mew_mew_4.Internal;
using uwu_mew_mew_4.Openai;

namespace uwu_mew_mew_4.Handlers;

public static class Ai
{
    public static ConcurrentDictionary<ulong, IList<OpenAi.Chat.Message>> Chats = new();

    public static async Task HandleMessage(SocketUserMessage message)
    {
        var text = message.Content.TrimStart().RemoveStart("<@1109341287372554250>").Trim();

        var userMessages = new List<OpenAi.Chat.Message>();
        if (Chats.TryGetValue(message.Author.Id, out var memoryMessages))
            userMessages.AddRange(memoryMessages);
        userMessages.Add(new("user", text));

        var messages = new List<OpenAi.Chat.Message>();
        messages.Add(new("system", SystemPrompts.UwuMewMew));
        messages.AddRange(userMessages);

        var response = await OpenAi.Chat.GetChatCompletionAsync(messages);

        userMessages.Add(new("assistant", response.Content!));
        Chats.AddOrUpdate(message.Author.Id, _ => userMessages, (_, _) => userMessages);

        await message.ReplyAsync(response.Content);
    }
}