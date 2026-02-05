using NovaAPI.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaAPI.Friends
{
    public class FriendsService
    {
        private const string ConnectionString = "Host=harder-trout-21449.j77.aws-eu-central-1.cockroachlabs.cloud;Port=26257;Username=Treaki;Password=8YkNzmiV-3LZMb1CAJCHvw;Database=defaultdb;SSL Mode=VerifyFull";

        public async Task<List<FriendEntry>> GetFriendsAsync(string discordId)
        {
            var friends = new List<FriendEntry>();
            try
            {
                using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                // Join with player_stats to get names and avatars
                using var cmd = new NpgsqlCommand(@"
                    SELECT p.username, p.discord_id, p.avatar_url, p.last_seen, p.level, p.rank, p.current_activity 
                    FROM friendships f
                    JOIN player_stats p ON (f.friend_id = p.discord_id)
                    WHERE f.user_id = @id", conn);
                
                cmd.Parameters.AddWithValue("id", discordId);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var lastSeen = reader.GetDateTime(reader.GetOrdinal("last_seen"));
                    bool isOnline = (DateTime.Now - lastSeen).TotalMinutes < 2;

                    string activity = reader.IsDBNull(reader.GetOrdinal("current_activity")) ? "Main Menu" : reader.GetString(reader.GetOrdinal("current_activity"));
                    var presence = FriendPresence.Offline;
                    if (isOnline)
                    {
                        presence = activity.Contains("Playing") ? FriendPresence.InGame : FriendPresence.InLauncher;
                    }

                    friends.Add(new FriendEntry
                    {
                        Username = reader.GetString(reader.GetOrdinal("username")),
                        DiscordId = reader.GetString(reader.GetOrdinal("discord_id")),
                        AvatarUrl = reader.IsDBNull(reader.GetOrdinal("avatar_url")) ? null : reader.GetString(reader.GetOrdinal("avatar_url")),
                        IsOnline = isOnline,
                        Presence = presence,
                        CurrentServer = activity,
                        Level = reader.GetInt32(reader.GetOrdinal("level")),
                        Rank = reader.GetString(reader.GetOrdinal("rank"))
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB ERROR] GetFriends: {ex.Message}");
            }
            return friends;
        }

        public async Task<List<FriendRequest>> GetPendingRequestsAsync(string discordId)
        {
            var requests = new List<FriendRequest>();
            try
            {
                using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand(@"
                    SELECT p.username, p.friend_code, p.avatar_url, fr.sent_at 
                    FROM friend_requests fr
                    JOIN player_stats p ON fr.from_id = p.discord_id
                    WHERE fr.to_id = @id", conn);
                
                cmd.Parameters.AddWithValue("id", discordId);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    requests.Add(new FriendRequest
                    {
                        FromUsername = reader.GetString(reader.GetOrdinal("username")),
                        FromFriendCode = reader.GetString(reader.GetOrdinal("friend_code")),
                        AvatarUrl = reader.IsDBNull(reader.GetOrdinal("avatar_url")) ? null : reader.GetString(reader.GetOrdinal("avatar_url")),
                        SentAt = reader.GetDateTime(reader.GetOrdinal("sent_at"))
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB ERROR] GetPendingRequests: {ex.Message}");
            }
            return requests;
        }

        public async Task<bool> SendRequestByCodeAsync(string fromDiscordId, string targetCode)
        {
            try
            {
                using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                // 1. Find target ID by code
                using var findCmd = new NpgsqlCommand("SELECT discord_id FROM player_stats WHERE friend_code = @code", conn);
                findCmd.Parameters.AddWithValue("code", targetCode.ToUpper());
                string? targetId = (string?)await findCmd.ExecuteScalarAsync();

                if (string.IsNullOrEmpty(targetId) || targetId == fromDiscordId) return false;

                // 2. Insert request
                using var insertCmd = new NpgsqlCommand(@"
                    INSERT INTO friend_requests (from_id, to_id) 
                    VALUES (@from, @to) 
                    ON CONFLICT DO NOTHING", conn);
                
                insertCmd.Parameters.AddWithValue("from", fromDiscordId);
                insertCmd.Parameters.AddWithValue("to", targetId);

                await insertCmd.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AcceptRequestAsync(string myId, string fromCode)
        {
            try
            {
                using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                // 1. Get from_id
                using var findCmd = new NpgsqlCommand("SELECT discord_id FROM player_stats WHERE friend_code = @code", conn);
                findCmd.Parameters.AddWithValue("code", fromCode.ToUpper());
                string? fromId = (string?)await findCmd.ExecuteScalarAsync();

                if (string.IsNullOrEmpty(fromId)) return false;

                // 2. Add to friendships (bidirectional)
                using var friendshipCmd = new NpgsqlCommand(@"
                    INSERT INTO friendships (user_id, friend_id) VALUES (@u, @f), (@f, @u) 
                    ON CONFLICT DO NOTHING", conn);
                friendshipCmd.Parameters.AddWithValue("u", myId);
                friendshipCmd.Parameters.AddWithValue("f", fromId);
                await friendshipCmd.ExecuteNonQueryAsync();

                // 3. Delete request
                using var deleteCmd = new NpgsqlCommand("DELETE FROM friend_requests WHERE from_id = @f AND to_id = @u", conn);
                deleteCmd.Parameters.AddWithValue("f", fromId);
                deleteCmd.Parameters.AddWithValue("u", myId);
                await deleteCmd.ExecuteNonQueryAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task DeclineRequestAsync(string myId, string fromCode)
        {
            try
            {
                using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                using var findCmd = new NpgsqlCommand("SELECT discord_id FROM player_stats WHERE friend_code = @code", conn);
                findCmd.Parameters.AddWithValue("code", fromCode.ToUpper());
                string? fromId = (string?)await findCmd.ExecuteScalarAsync();

                if (string.IsNullOrEmpty(fromId)) return;

                using var deleteCmd = new NpgsqlCommand("DELETE FROM friend_requests WHERE from_id = @f AND to_id = @u", conn);
                deleteCmd.Parameters.AddWithValue("f", fromId);
                deleteCmd.Parameters.AddWithValue("u", myId);
                await deleteCmd.ExecuteNonQueryAsync();
            }
            catch { }
        }
    }
}
