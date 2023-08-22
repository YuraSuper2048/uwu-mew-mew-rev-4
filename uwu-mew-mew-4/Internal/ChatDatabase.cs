using System.Data.SQLite;
using Newtonsoft.Json;
using SimpleOpenAi;

namespace uwu_mew_mew_4.Internal;

internal static class ChatDatabase
{
    public record struct Chat(List<OpenAi.Chat.Message>? Messages, string Character);
    
    private const string ConnectionString = "Data Source=uwu_mew_mew.db";

    public static async Task<Chat> GetAsync(ulong userId)
    {
        await using var connection = new SQLiteConnection(ConnectionString);
        await connection.OpenAsync();

        var command = new SQLiteCommand(connection);
        command.CommandText = "SELECT * FROM chats WHERE user_id = @user";
        command.Parameters.AddWithValue("@user", userId);

        await using var reader = command.ExecuteReader();
        await reader.ReadAsync();
        
        if (!reader.HasRows) return new();
        
        var messages = JsonConvert.DeserializeObject<List<OpenAi.Chat.Message>>((string)reader["messages"])!;
        var character = (string)reader["character"];
        return new(messages, character);
    }

    public static async Task SetAsync(ulong userId, Chat chat)
    {
        await using var connection = new SQLiteConnection(ConnectionString);
        await connection.OpenAsync();

        var command = new SQLiteCommand(connection);

        command.CommandText = "INSERT OR REPLACE INTO chats (user_id, messages, character) VALUES (@user, @messages, @character)";
        command.Parameters.AddWithValue("@user", userId);
        command.Parameters.AddWithValue("@messages", JsonConvert.SerializeObject(chat.Messages));
        command.Parameters.AddWithValue("@character", chat.Character);
        await command.ExecuteNonQueryAsync();
    }

    public static async Task DeleteAsync(ulong userId)
    {
        await using var connection = new SQLiteConnection(ConnectionString);
        await connection.OpenAsync();

        var command = new SQLiteCommand(connection);

        command.CommandText = "DELETE FROM chats WHERE user_id = @user";
        command.Parameters.AddWithValue("@user", userId);
        await command.ExecuteNonQueryAsync();
    }
}