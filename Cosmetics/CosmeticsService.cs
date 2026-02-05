using NovaAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaAPI.Cosmetics
{
    public class CosmeticsService
    {
        public async Task<List<CosmeticItem>> GetAvailableCosmeticsAsync()
        {
            await Task.Delay(100); // Simulate network
            return new List<CosmeticItem>
            {
                new CosmeticItem { Id = "nebula_cape_v1", Name = "Nebula Original Cape", Type = "Cape", TextureUrl = "https://example.com/capes/nebula.png", IsPremium = false },
                new CosmeticItem { Id = "vanguard_cape", Name = "Vanguard Elite", Type = "Cape", TextureUrl = "https://example.com/capes/vanguard.png", IsPremium = true },
                new CosmeticItem { Id = "nebula_wings_white", Name = "Angelic Wings", Type = "Wings", TextureUrl = "https://example.com/wings/white.png", IsPremium = true }
            };
        }

        public async Task<bool> EquippCosmeticAsync(string cosmeticId, string userId)
        {
            await Task.Delay(200);
            return true;
        }
    }
}
