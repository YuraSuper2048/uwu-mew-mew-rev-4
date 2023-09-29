using System.Diagnostics;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;
using uwu_mew_mew_4.Internal;
using Color = Discord.Color;

#pragma warning disable CS8618
#pragma warning disable CS1998

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

        var keepAlive = KeepAlive();

        try
        {
            //await DumpAllAsync();
        }
        catch (Exception e)
        {
            Logger.WriteLine(e.ToString());
        }
    }

    private static async Task DumpAllAsync()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var count = 0;

        var writer = new StreamWriter("general.jsonl", true);

        await ProcessChannel((IRestMessageChannel)(await Client.Rest.GetChannelAsync(1050422061803245600)));

        async Task ProcessChannel(IRestMessageChannel channel)
        {
            try
            {
                await channel.GetMessagesAsync(1).FirstAsync();
            }
            catch
            {
                return;
            }
            
            await Task.Delay(1000);

            async Task ProcessBatch(IReadOnlyCollection<IMessage> messages)
            {
                void ProcessMessage(IMessage currentMessage)
                {
                    try
                    {
                        lock(writer)
                        {
                            writer.WriteLine(JsonConvert.SerializeObject(new {
                                content = currentMessage.Content,
                                id = currentMessage.Id,
                                author_id = currentMessage.Author.Id,
                                author_name = currentMessage.Author.Username,
                                reply_id = currentMessage.Reference?.MessageId.GetValueOrDefault(),
                                timestamp = currentMessage.Timestamp.ToUniversalTime().ToUnixTimeSeconds(),
                            }));
                        }

                        count++;
                        Console.Write($"| {count,7} | {MathF.Round(count / (stopwatch.ElapsedMilliseconds / 1000), 2),5} msg/s | {MathF.Round(stopwatch.ElapsedMilliseconds / 1000),3} s |\r");
                    }
                    catch
                    {
                        //whatever i dont care
                    }
                }

                Parallel.ForEach(messages, ProcessMessage);
            }
            
            var messagesAsync = channel.GetMessagesAsync(int.MaxValue);

            try
            {
                await foreach (var messages in messagesAsync) 
                {
                    var batch = ProcessBatch(messages);
                }
            }
            catch
            {
                //fc it
            }
        }
    }

    private static async Task KeepAlive()
    {
        var reconnects = new List<DateTimeOffset>();
        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(30));
            if (Client.ConnectionState == ConnectionState.Connected) continue;
            
            if (reconnects.Select(d => (DateTimeOffset.UtcNow - d).TotalMinutes < 60).Count() > 45)
                await Task.Delay(TimeSpan.FromMinutes(15));
            
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
