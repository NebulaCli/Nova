using NovaAPI.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaAPI.Chat
{
    public class ChatService
    {
        private const string ConnectionString = "Host=harder-trout-21449.j77.aws-eu-central-1.cockroachlabs.cloud;Port=26257;Username=Treaki;Password=8YkNzmiV-3LZMb1CAJCHvw;Database=defaultdb;SSL Mode=VerifyFull";

        public async Task SendMessageAsync(string senderId, string receiverId, string content)
        {
            try
            {
                using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand(@"
                    INSERT INTO messages (sender_id, receiver_id, content) 
                    VALUES (@sender, @receiver, @content)", conn);
                
                cmd.Parameters.AddWithValue("sender", senderId);
                cmd.Parameters.AddWithValue("receiver", receiverId);
                cmd.Parameters.AddWithValue("content", content);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB ERROR] SendMessage: {ex.Message}");
            }
        }

        public async Task<List<ChatMessage>> GetMessagesAsync(string myId, string friendId)
        {
            var messages = new List<ChatMessage>();
            try
            {
                using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand(@"
                    SELECT m.*, p.username as sender_name 
                    FROM messages m
                    LEFT JOIN player_stats p ON m.sender_id = p.discord_id
                    WHERE (sender_id = @me AND receiver_id = @friend)
                       OR (sender_id = @friend AND receiver_id = @me)
                    ORDER BY sent_at ASC", conn);
                
                cmd.Parameters.AddWithValue("me", myId);
                cmd.Parameters.AddWithValue("friend", friendId);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string senderId = reader.GetString(reader.GetOrdinal("sender_id"));
                    messages.Add(new ChatMessage
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        SenderId = senderId,
                        SenderName = reader.GetString(reader.GetOrdinal("sender_name")),
                        Content = reader.GetString(reader.GetOrdinal("content")),
                        SentAt = reader.GetDateTime(reader.GetOrdinal("sent_at")),
                        IsMe = senderId == myId
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB ERROR] GetMessages: {ex.Message}");
            }
            return messages;
        }
    }
}
