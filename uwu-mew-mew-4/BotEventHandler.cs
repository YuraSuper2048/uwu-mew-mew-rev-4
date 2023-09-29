using Discord;
using Discord.WebSocket;
using uwu_mew_mew_4.Handlers;
using uwu_mew_mew_4.Internal;

#pragma warning disable CS1998
#pragma warning disable CS4014

namespace uwu_mew_mew_4;

public static class BotEventHandler
{
    public static async Task OnMessageReceived(SocketMessage msg)
    {
        HandleMessage(msg);
    }

    private static async Task HandleMessage(SocketMessage msg)
    {
        if (msg is not SocketUserMessage message)
            return;
        try
        {
            if (message.Channel is IDMChannel
                && !message.Author.IsBot)
            {
                await Ai.HandleMessage(message);
                return;
            }
            if (message.MentionedUsers.Select(u => u.Id).Contains(Bot.Client.CurrentUser.Id)
                && !message.Author.IsBot)
            {
                await Ai.HandleMessage(message);
                return;
            }
        }
        catch (Exception e)
        {
            Logger.WriteLine($"Error: {e}");
            message.ReplyAsync("owo i had an error!!!!");
            throw;
        }
    }

    public static async Task OnSlashCommandExecuted(SocketSlashCommand command)
    {
        SlashCommandExecuted(command);
    }

    private static async Task SlashCommandExecuted(SocketSlashCommand command)
    {
        command.DeferAsync();
        try
        {
            if (command.Data.Name == "image")
            {
                await ImageGeneration.GenerateImage(command);
            }
        }
        catch (Exception e)
        {
            Logger.WriteLine($"Error: {e}");
            command.FollowupAsync("owo i had an error!!!!");
            throw;
        }
    }

    public static async Task OnButtonExecuted(SocketMessageComponent component)
    {
        ButtonExecuted(component);
    }

    private static async Task ButtonExecuted(SocketMessageComponent component)
    {
        await component.DeferAsync(true);
        try
        {
            if (component.Data.CustomId == "ai-reset")
            {
                await Ai.Reset(component.User.Id);
                await component.FollowupAsync("mrrp~ i forgor everything :cat: mew~", ephemeral: true);
            }
            if (component.Data.CustomId == "ai-stop")
            {
                await Ai.Stop(component.User.Id);
                await component.FollowupAsync("owo stopped thinking ;-;", ephemeral: true);
            }
            if (component.Data.CustomId == "ai-character")
            {
                await Ai.Character(component);
            }
        }
        catch (Exception e)
        {
            Logger.WriteLine($"Error: {e}");
            component.FollowupAsync("owo i had an error!!!!");
            throw;
        }
    }

    public static async Task OnSelectMenuExecuted(SocketMessageComponent component)
    {
        SelectMenuExecuted(component);
    }

    private static async Task SelectMenuExecuted(SocketMessageComponent component)
    {
        component.DeferAsync(ephemeral: true);
        try
        {
            if (component.Data.CustomId == "ai-character-select")
            {
                await Ai.CharacterSelect(component.User.Id, component.Data.Values.First());
            }
        }
        catch (Exception e)
        {
            Logger.WriteLine($"Error: {e}");
            component.FollowupAsync("owo i had an error!!!!");
            throw;
        }
    }

    public static async Task OnReady()
    {
        var imageCommand = new SlashCommandBuilder();
        imageCommand.Name = "image";
        imageCommand.Description = "Generate an image using Stable Diffusion.";
        imageCommand.AddOption("prompt", ApplicationCommandOptionType.String, 
            "Prompt to generate the image from", isRequired: true);
        imageCommand.AddOption("cfg", ApplicationCommandOptionType.Number, 
            "Classifier Free Guidance scale, how much the model should respect your prompt. Default is 5,5.");
        imageCommand.AddOption("steps", ApplicationCommandOptionType.Integer, 
            "Number of sampling steps. The more the better. Default is 80. ");
        imageCommand.AddOption("seed", ApplicationCommandOptionType.Number, 
            "Seed for the sampler. Only useful if you already know a seed");
        imageCommand.AddOption("ratio", ApplicationCommandOptionType.String, 
            "Aspect ratio for image in format \"16:9\".");

        var commands = await Bot.Client.GetGlobalApplicationCommandsAsync();
        if (!commands.Any(c => c.Name == imageCommand.Name))
            await Bot.Client.CreateGlobalApplicationCommandAsync(imageCommand.Build());
    }
}