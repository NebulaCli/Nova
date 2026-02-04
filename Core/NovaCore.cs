using NovaAPI.Authentication;
using NovaAPI.Leaderboard;
using NovaAPI.News;
using NovaAPI.Security;
using NovaAPI.Mods;
using NovaAPI.Models;
using System.Threading.Tasks;

namespace NovaAPI.Core
{
    /// <summary>
    /// NovaAPI - The core backend powered by Nebula Client
    /// Handles everything from authentication to global player data
    /// </summary>
    public class NovaCore
    {
        private static NovaCore? _instance;
        public static NovaCore Instance => _instance ??= new NovaCore();

        public AuthService Auth { get; }
        public LeaderboardService Leaderboard { get; }
        public NewsService News { get; }
        public ModService Mods { get; }

        private NovaCore()
        {
            Auth = new AuthService();
            Leaderboard = new LeaderboardService();
            News = new NewsService();
            Mods = new ModService();
        }

        public string GetApiVersion() => "1.0.0-PROXIMA";
        
        public string GetHWID() => HardwareId.Generate();

        public async Task<SystemHealth> GetSystemStatusAsync()
        {
            await Task.Delay(300);
            return new SystemHealth
            {
                Status = "Operational",
                LatencyMs = 12.5,
                OnlinePlayers = 1420,
                Version = GetApiVersion()
            };
        }
    }
}
