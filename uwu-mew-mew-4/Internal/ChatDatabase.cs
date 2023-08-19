using System.Data.SQLite;
using Newtonsoft.Json;
using uwu_mew_mew_4.Openai;

namespace uwu_mew_mew_4.Internal;

internal static class ChatDatabase
{
    private const string ConnectionString = "Data Source=uwu_mew_mew.db";

    public static async Task<List<OpenAi.Chat.Message>> GetAsync(ulong userId)
    {
        await using var connection = new SQLiteConnection(ConnectionString);
        await connection.OpenAsync();

        var command = new SQLiteCommand(connection);
        command.CommandText = "SELECT * FROM chats WHERE user_id = @user";
        command.Parameters.AddWithValue("@user", userId);

        await using var reader = command.ExecuteReader();
        await reader.ReadAsync();
        return !reader.HasRows
            ? new()
            : JsonConvert.DeserializeObject<List<OpenAi.Chat.Message>>((string)reader["messages"])!;
    }

    public static async Task SetAsync(ulong userId, List<OpenAi.Chat.Message> messages)
    {
        await using var connection = new SQLiteConnection(ConnectionString);
        await connection.OpenAsync();

        var command = new SQLiteCommand(connection);

        command.CommandText = "INSERT OR REPLACE INTO chats (user_id, messages) VALUES (@user, @messages)";
        command.Parameters.AddWithValue("@user", userId);
        command.Parameters.AddWithValue("@messages", JsonConvert.SerializeObject(messages));
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