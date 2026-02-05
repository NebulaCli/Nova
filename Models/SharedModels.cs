namespace NovaAPI.Models
{
    public class NovaSession
    {
        public string Username { get; set; } = "";
        public string UUID { get; set; } = "";
        public string AccessToken { get; set; } = "";
        public string UserType { get; set; } = "msa"; // "msa" or "discord"
        public string? AvatarUrl { get; set; }
        public string? DiscordId { get; set; }
        public string FriendCode { get; set; } = "";
        public DateTime LoggedInAt { get; set; } = DateTime.Now;
        
        public string SkinUrl => UserType == "discord" && !string.IsNullOrEmpty(AvatarUrl) 
            ? AvatarUrl 
            : (!string.IsNullOrEmpty(UUID) ? $"https://mc-heads.net/avatar/{UUID}/100" : "pack://application:,,,/Resources/logo.png");
    }

    public class PlayerStats
    {
        public string Username { get; set; } = "";
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public long TotalPlayTimeMinutes { get; set; } = 0;
        public string Rank { get; set; } = "Novice";
        public string? AvatarUrl { get; set; }
        public string FriendCode { get; set; } = "";
        public DateTime LastSeen { get; set; } = DateTime.Now;
        public string CurrentActivity { get; set; } = "Main Menu";
    }

    public class FriendRequest
    {
        public string FromUsername { get; set; } = "";
        public string FromFriendCode { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public DateTime SentAt { get; set; } = DateTime.Now;
    }

    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string Username { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public int Level { get; set; }
        public string PlayTime { get; set; } = "0h";
        public string RankTitle { get; set; } = "Novice";
        public bool IsCurrentUser { get; set; } = false;
        public string UserType { get; set; } = "msa";
        
        public string RankColor => Rank switch {
            1 => "#FFD700", // Gold
            2 => "#C0C0C0", // Silver
            3 => "#CD7F32", // Bronze
            _ => "#222222"
        };
    }
    public class NewsEntry
    {
        public string Title { get; set; } = "";
        public string Content { get; set; } = "";
        public string Author { get; set; } = "Nebula Team";
        public DateTime Date { get; set; } = DateTime.Now;
        public string Category { get; set; } = "UPDATE";
    }

    public class SystemHealth
    {
        public string Status { get; set; } = "Operational";
        public double LatencyMs { get; set; }
        public string Version { get; set; } = "1.0.0";
        public int OnlinePlayers { get; set; }
    }

    public class ModUpdate
    {
        public string Name { get; set; } = "";
        public string Version { get; set; } = "";
        public string DownloadUrl { get; set; } = "";
        public bool IsCritical { get; set; } = false;
    }

    public class CosmeticItem
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Type { get; set; } = "Cape"; // Cape, Hat, Wings
        public string TextureUrl { get; set; } = "";
        public bool IsPremium { get; set; } = false;
    }

    public class FriendEntry
    {
        public string Username { get; set; } = "";
        public string DiscordId { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public bool IsOnline { get; set; } = false;
        public FriendPresence Presence { get; set; } = FriendPresence.Offline;
        public string CurrentServer { get; set; } = "Main Menu";
        public int Level { get; set; } = 1;
        public string Rank { get; set; } = "Novice";

        public string StatusText => Presence switch {
            FriendPresence.InGame => $"PLAYING: {CurrentServer}",
            FriendPresence.InLauncher => "IDLE IN LAUNCHER",
            _ => "OFFLINE"
        };

        public string StatusColor => Presence switch {
            FriendPresence.InGame => "#00FF88",
            FriendPresence.InLauncher => "#FFCC00",
            _ => "#444444"
        };
    }

    public enum FriendPresence
    {
        Offline,
        InLauncher,
        InGame
    }

    public class ChatMessage
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = "";
        public string SenderName { get; set; } = "";
        public string Content { get; set; } = "";
        public DateTime SentAt { get; set; }
        public bool IsMe { get; set; }
    }
}
