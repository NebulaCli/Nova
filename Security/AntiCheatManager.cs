using System;
using System.Threading.Tasks;
using Npgsql;

namespace NovaAPI.Security
{
    public class AntiCheatManager
    {
        private static AntiCheatManager? _instance;
        public static AntiCheatManager Instance => _instance ??= new AntiCheatManager();

        private const string ConnectionString = "Host=harder-trout-21449.j77.aws-eu-central-1.cockroachlabs.cloud;Port=26257;Username=Treaki;Password=8YkNzmiV-3LZMb1CAJCHvw;Database=defaultdb;SSL Mode=VerifyFull";

        private AntiCheatManager() { }

        public async Task InitializeDatabaseAsync()
        {
            try
            {
                using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                using var cmd1 = new NpgsqlCommand(@"
                    CREATE TABLE IF NOT EXISTS banned_users (
                        discord_id TEXT PRIMARY KEY,
                        reason TEXT,
                        ban_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        hwid TEXT
                    );", conn);
                await cmd1.ExecuteNonQueryAsync();

                using var cmd2 = new NpgsqlCommand(@"
                    CREATE TABLE IF NOT EXISTS player_stats (
                        discord_id TEXT PRIMARY KEY,
                        username TEXT,
                        level INT DEFAULT 1,
                        playtime_minutes BIGINT DEFAULT 0,
                        friend_code TEXT UNIQUE,
                        rank TEXT DEFAULT 'Novice',
                        avatar_url TEXT,
                        last_seen TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        current_activity TEXT DEFAULT 'Main Menu'
                    );
                    ALTER TABLE player_stats ADD COLUMN IF NOT EXISTS level INT DEFAULT 1;
                    ALTER TABLE player_stats ADD COLUMN IF NOT EXISTS rank TEXT DEFAULT 'Novice';
                    ALTER TABLE player_stats ADD COLUMN IF NOT EXISTS current_activity TEXT DEFAULT 'Main Menu';
                    ALTER TABLE player_stats ADD COLUMN IF NOT EXISTS playtime_minutes BIGINT DEFAULT 0;
                    ", conn);
                await cmd2.ExecuteNonQueryAsync();

                using var cmd3 = new NpgsqlCommand(@"
                    CREATE TABLE IF NOT EXISTS friendships (
                        user_id TEXT,
                        friend_id TEXT,
                        PRIMARY KEY (user_id, friend_id)
                    );", conn);
                await cmd3.ExecuteNonQueryAsync();

                using var cmd4 = new NpgsqlCommand(@"
                    CREATE TABLE IF NOT EXISTS friend_requests (
                        from_id TEXT,
                        to_id TEXT,
                        sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                        PRIMARY KEY (from_id, to_id)
                    );", conn);
                await cmd4.ExecuteNonQueryAsync();

                using var cmd5 = new NpgsqlCommand(@"
                    CREATE TABLE IF NOT EXISTS messages (
                        id SERIAL PRIMARY KEY,
                        sender_id TEXT,
                        receiver_id TEXT,
                        content TEXT,
                        sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                    );", conn);
                await cmd5.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[NV-Guard] DB Init Error: {ex.Message}");
            }
        }

        public async Task BanUserAsync(string discordId, string reason, string hwid)
        {
            try
            {
                using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand(@"
                    INSERT INTO banned_users (discord_id, reason, hwid) 
                    VALUES (@id, @reason, @hwid)
                    ON CONFLICT (discord_id) DO NOTHING;", conn);
                
                cmd.Parameters.AddWithValue("id", discordId);
                cmd.Parameters.AddWithValue("reason", reason);
                cmd.Parameters.AddWithValue("hwid", hwid);

                await cmd.ExecuteNonQueryAsync();
                Console.WriteLine($"[NV-Guard] User {discordId} has been BANNED for {reason}.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AntiCheat] Ban Error: {ex.Message}");
            }
        }

        public async Task<bool> IsUserBannedAsync(string discordId)
        {
            try
            {
                using var conn = new NpgsqlConnection(ConnectionString);
                await conn.OpenAsync();

                using var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM banned_users WHERE discord_id = @id", conn);
                cmd.Parameters.AddWithValue("id", discordId);

                var count = (long)(await cmd.ExecuteScalarAsync() ?? 0L);
                return count > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
