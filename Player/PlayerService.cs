using NovaAPI.Models;
using Npgsql;
using System;
using System.Threading.Tasks;

namespace NovaAPI.Player
{
    public class PlayerService
    {
        private const string ConnectionString = "Host=harder-trout-21449.j77.aws-eu-central-1.cockroachlabs.cloud;Port=26257;Username=Treaki;Password=8YkNzmiV-3LZMb1CAJCHvw;Database=defaultdb;SSL Mode=VerifyFull";

        public async Task<PlayerStats?> GetPlayerStatsAsync(string discordId, string username, string? avatarUrl)
        {
            try
            {
                using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand("SELECT *, current_activity FROM player_stats WHERE discord_id = @id", conn);
                cmd.Parameters.AddWithValue("id", discordId);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new PlayerStats
                    {
                        Username = reader.GetString(reader.GetOrdinal("username")),
                        Level = reader.GetInt32(reader.GetOrdinal("level")),
                        TotalPlayTimeMinutes = reader.GetInt64(reader.GetOrdinal("playtime_minutes")),
                        FriendCode = reader.GetString(reader.GetOrdinal("friend_code")),
                        Rank = reader.GetString(reader.GetOrdinal("rank")),
                        AvatarUrl = reader.IsDBNull(reader.GetOrdinal("avatar_url")) ? null : reader.GetString(reader.GetOrdinal("avatar_url")),
                        LastSeen = reader.GetDateTime(reader.GetOrdinal("last_seen")),
                        CurrentActivity = reader.IsDBNull(reader.GetOrdinal("current_activity")) ? "Main Menu" : reader.GetString(reader.GetOrdinal("current_activity"))
                    };
                }
                else
                {
                    // Create new profile
                    string friendCode = GenerateFriendCode();
                    await CreatePlayerProfileAsync(discordId, username, friendCode, avatarUrl);
                    return new PlayerStats
                    {
                        Username = username,
                        Level = 1,
                        TotalPlayTimeMinutes = 0,
                        FriendCode = friendCode,
                        Rank = "Novice",
                        AvatarUrl = avatarUrl,
                        LastSeen = DateTime.Now
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB ERROR] GetPlayerStats: {ex.Message}");
                return null;
            }
        }

        public async Task UpdatePlayerStatsAsync(string discordId, int level, long playtimeMinutes, string activity = "Main Menu")
        {
            try
            {
                using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand(@"
                    UPDATE player_stats 
                    SET level = @level, playtime_minutes = @playtime, last_seen = CURRENT_TIMESTAMP, current_activity = @activity
                    WHERE discord_id = @id", conn);
                
                cmd.Parameters.AddWithValue("id", discordId);
                cmd.Parameters.AddWithValue("level", level);
                cmd.Parameters.AddWithValue("playtime", playtimeMinutes);
                cmd.Parameters.AddWithValue("activity", activity);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB ERROR] UpdatePlayerStats: {ex.Message}");
            }
        }

        private async Task CreatePlayerProfileAsync(string discordId, string username, string friendCode, string? avatarUrl)
        {
            using var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();

            using var cmd = new NpgsqlCommand(@"
                INSERT INTO player_stats (discord_id, username, friend_code, avatar_url) 
                VALUES (@id, @username, @code, @avatar)", conn);
            
            cmd.Parameters.AddWithValue("id", discordId);
            cmd.Parameters.AddWithValue("username", username);
            cmd.Parameters.AddWithValue("code", friendCode);
            cmd.Parameters.AddWithValue("avatar", (object?)avatarUrl ?? DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
        }

        private string GenerateFriendCode()
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var result = new char[8];
            for (int i = 0; i < 8; i++) result[i] = chars[random.Next(chars.Length)];
            return new string(result);
        }

        public async Task<string?> GetDiscordIdByFriendCodeAsync(string friendCode)
        {
            try
            {
                using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand("SELECT discord_id FROM player_stats WHERE friend_code = @code", conn);
                cmd.Parameters.AddWithValue("code", friendCode.ToUpper());

                return (string?)await cmd.ExecuteScalarAsync();
            }
            catch
            {
                return null;
            }
        }
    }
}
