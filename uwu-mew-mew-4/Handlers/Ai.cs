using System.Data.SQLite;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using uwu_mew_mew_4.Internal;
using uwu_mew_mew_4.Openai;

namespace uwu_mew_mew_4.Handlers;

public static class Ai
{
    private const string ConnectionString = "Data Source=uwu_mew_mew.db";

    public static async Task HandleMessage(SocketUserMessage message)
    {
        var text = message.Content.TrimStart().RemoveStart("<@1109341287372554250>").Trim();

        var userMessages = GetUserMessages(message.Author.Id);
        userMessages.Add(new("user", text));

        var messages = new List<OpenAi.Chat.Message>
        {
            new("system", SystemPrompts.UwuMewMew)
        };
        messages.AddRange(userMessages);

        var response = await OpenAi.Chat.GetChatCompletionAsync(messages);

        userMessages.Add(new("assistant", response.Content!));
        SetUserMessages(message.Author.Id, userMessages);

        await message.ReplyAsync(response.Content);
    }

    private static List<OpenAi.Chat.Message> GetUserMessages(ulong userId)
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        var command = new SQLiteCommand(connection);
        command.CommandText = "SELECT * FROM chats WHERE user_id = @user";
        command.Parameters.AddWithValue("@user", userId);

        using var reader = command.ExecuteReader();
        reader.Read();
        return !reader.HasRows ? 
            new() 
            : JsonConvert.DeserializeObject<List<OpenAi.Chat.Message>>((string)reader["messages"])!;
    }

    private static void SetUserMessages(ulong userId, List<OpenAi.Chat.Message> messages)
    {
        using var connection = new SQLiteConnection(ConnectionString);
        connection.Open();

        var command = new SQLiteCommand(connection);

        command.CommandText = "INSERT OR REPLACE INTO chats (user_id, messages) VALUES (@user, @messages)";
        command.Parameters.AddWithValue("@user", userId);
        command.Parameters.AddWithValue("@messages", JsonConvert.SerializeObject(messages));
        command.ExecuteNonQuery();
    }
}