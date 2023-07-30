using Discord;
using Discord.WebSocket;

namespace uwu_mew_mew_4;

public static class Bot
{
    public static DiscordSocketClient Client { get; private set; }

    public static async Task RunAsync()
    {
        await InitClient();

        await Task.Delay(-1);
    }

    private static async Task Ready()
    {
        Console.WriteLine("Ready.");

        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(30));

            if (Client.ConnectionState == ConnectionState.Connected) continue;
            await Task.Delay(TimeSpan.FromSeconds(10));
            if (Client.ConnectionState == ConnectionState.Connected) continue;

            await InitClient();
            return;
        }
    }

    private static async Task InitClient()
    {
        var config = new DiscordSocketConfig();
        config.GatewayIntents = GatewayIntents.AllUnprivileged
                                | GatewayIntents.MessageContent
                                | GatewayIntents.GuildMembers;
        Client = new(config);

        Client.Ready += Ready;
        Client.MessageReceived += MainHandler.OnMessageReceived;
        Client.ButtonExecuted += MainHandler.OnButtonExecuted;

        await Client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_AUTH_TOKEN"));
        await Client.StartAsync();

        await Client.SetStatusAsync(UserStatus.Online);

        Console.Write("Loading... ");
    }
}