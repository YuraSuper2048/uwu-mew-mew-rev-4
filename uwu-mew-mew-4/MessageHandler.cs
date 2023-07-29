using Discord.WebSocket;

namespace uwu_mew_mew_4;

public static class MessageHandler
{
    public static async Task MessageReceived(SocketMessage msg)
    {
        if (msg is not SocketUserMessage message)
            return;

        if(message.MentionedUsers.Contains(Bot.Client.CurrentUser))
            await Ai.HandleMessage(message);
    }
}