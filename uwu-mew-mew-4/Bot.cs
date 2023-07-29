using Discord;
using Discord.WebSocket;

namespace uwu_mew_mew_4;

public static class Bot
{
    public static DiscordSocketClient Client { get; private set; }

    public static async Task RunAsync()
    {
        var config = new DiscordSocketConfig();
        config.GatewayIntents = GatewayIntents.AllUnprivileged
                                | GatewayIntents.MessageContent
                                | GatewayIntents.GuildMembers;
        Client = new DiscordSocketClient(config);

        Client.Ready += Ready;
        Client.MessageReceived += MessageHandler.MessageReceived;

        await Client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_AUTH_TOKEN"));
        await Client.StartAsync();

        await Client.SetStatusAsync(UserStatus.Online);

        Console.Write("Loading... ");

        await Task.Delay(-1);
    }
    
    private static async Task Ready()
    {
        Console.Write("Ready.");
    }
}