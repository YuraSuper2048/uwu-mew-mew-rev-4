using Discord.WebSocket;
using uwu_mew_mew_4.Handlers;
#pragma warning disable CS1998
#pragma warning disable CS4014

namespace uwu_mew_mew_4;

public static class MainHandler
{
    public static async Task OnMessageReceived(SocketMessage msg)
    {
        HandleMessage(msg);
    }

    private static async Task HandleMessage(SocketMessage msg)
    {
        if (msg is not SocketUserMessage message)
            return;

        if (message.MentionedUsers.Select(u => u.Id).Contains(Bot.Client.CurrentUser.Id)
            && !message.Author.IsBot)
            await Ai.HandleMessage(message);
    }

    public static async Task OnButtonExecuted(SocketMessageComponent component)
    {
        ButtonExecuted(component);
    }

    private static async Task ButtonExecuted(SocketMessageComponent component)
    {
        await component.DeferAsync(true);
        if (component.Data.CustomId == "reset")
        {
            await Ai.Reset(component.User.Id);
            await component.FollowupAsync("mrrp~ i forgor everything :cat: mew~", ephemeral: true);
        }
    }
}