using NovaAPI.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaAPI.News
{
    public class NewsService
    {
        public async Task<List<NewsEntry>> GetLatestNewsAsync()
        {
            // Simulate API latency
            await Task.Delay(500);

            return new List<NewsEntry>
            {
                new NewsEntry 
                { 
                    Title = "Nebula Client v1.0.0-PROXIMA Released", 
                    Content = "The wait is over. Experience the next generation of PvP gameplay.", 
                    Category = "MAJOR UPDATE" 
                },
                new NewsEntry 
                { 
                    Title = "Seasonal Rank Reset", 
                    Content = "All ranks have been reset for the new competitive season. Good luck!", 
                    Category = "LEADERBOARD" 
                }
            };
        }
    }
}
