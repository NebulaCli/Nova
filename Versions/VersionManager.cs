using System.Collections.Generic;
using System.Threading.Tasks;

namespace NovaAPI.Versions
{
    public class VersionManager
    {
        public async Task<string> GetLatestClientVersionAsync()
        {
            // Usually fetches from GitHub or Nebula CDN
            await Task.Delay(100);
            return "1.0.0-STABLE";
        }

        public async Task<List<string>> GetFeaturedModIdsAsync()
        {
            return new List<string> { "nebulamc", "keystrokes", "togglesprint", "reachdisplay" };
        }

        public bool IsVersionSupported(string minecraftVersion)
        {
            // Nebula logic for supported versions
            return minecraftVersion == "1.8.9" || minecraftVersion == "1.7.10";
        }
    }
}
