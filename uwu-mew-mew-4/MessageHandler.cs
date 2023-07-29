using Discord.WebSocket;
using uwu_mew_mew_4.Handlers;

namespace uwu_mew_mew_4;

public static class MessageHandler
{
    public static async Task MessageReceived(SocketMessage msg)
    {
        if (msg is not SocketUserMessage message)
            return;

        if (message.Author.Id != 687600977830084696)
            return;

        if (message.MentionedUsers.Select(u => u.Id).Contains(Bot.Client.CurrentUser.Id))
            await Ai.HandleMessage(message);
    }
}