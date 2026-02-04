namespace NovaAPI.Models
{
    public class NovaSession
    {
        public string Username { get; set; } = "";
        public string UUID { get; set; } = "";
        public string AccessToken { get; set; } = "";
        public string UserType { get; set; } = "msa";
        public DateTime LoggedInAt { get; set; } = DateTime.Now;
    }

    public class PlayerStats
    {
        public string Username { get; set; } = "";
        public int Level { get; set; } = 1;
        public int Experience { get; set; } = 0;
        public long TotalPlayTimeMinutes { get; set; } = 0;
        public string Rank { get; set; } = "Novice";
        public DateTime LastSeen { get; set; } = DateTime.Now;
    }

    public class LeaderboardEntry
    {
        public int Rank { get; set; }
        public string Username { get; set; } = "";
        public int Level { get; set; }
        public string PlayTime { get; set; } = "0h";
        public string RankTitle { get; set; } = "Novice";
        public bool IsCurrentUser { get; set; } = false;
        
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
}
