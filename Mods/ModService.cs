using NovaAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaAPI.Mods
{
    public class ModService
    {
        public async Task<List<ModUpdate>> GetAvailableUpdatesAsync()
        {
            await Task.Delay(400);
            return new List<ModUpdate>
            {
                new ModUpdate { Name = "Nebula Core", Version = "1.0.1", IsCritical = true },
                new ModUpdate { Name = "PvP Enhancer", Version = "2.3.0", IsCritical = false }
            };
        }
    }
}
