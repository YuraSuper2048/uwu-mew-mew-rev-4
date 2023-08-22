﻿using Discord;
using Discord.WebSocket;
using uwu_mew_mew_4.Internal;
using Color = Discord.Color;

#pragma warning disable CS8618

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
        Logger.WriteLine("Ready.");
        var onReady = BotEventHandler.OnReady();

        await KeepAlive();
    }

    private static async Task KeepAlive()
    {
        var reconnects = new List<DateTimeOffset>();
        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(30));
            if (Client.ConnectionState == ConnectionState.Connected) continue;
            
            if (reconnects.Select(d => (DateTimeOffset.UtcNow - d).TotalMinutes < 60).Count() > 45)
                await Task.Delay(TimeSpan.FromMinutes(2));
            
            await Task.Delay(TimeSpan.FromSeconds(10));
            if (Client.ConnectionState == ConnectionState.Connected) continue;

            reconnects.Add(DateTimeOffset.UtcNow);
            await Client.LogoutAsync();
            await Client.DisposeAsync();
            var task = InitClient();
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
        Client.MessageReceived += BotEventHandler.OnMessageReceived;
        Client.ButtonExecuted += BotEventHandler.OnButtonExecuted;
        Client.SelectMenuExecuted += BotEventHandler.OnSelectMenuExecuted;
        Client.SlashCommandExecuted += BotEventHandler.OnSlashCommandExecuted;

        await Client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_AUTH_TOKEN"));
        await Client.StartAsync();

        await Client.SetStatusAsync(UserStatus.Online);

        Logger.WriteLine("Connecting to Discord...");
    }

    public static readonly Color[] ColorPalette = 
    {
        new(255, 182, 193),
        new(255, 218, 185),
        new(152, 251, 152)
    };
}