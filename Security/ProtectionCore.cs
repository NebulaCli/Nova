using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NovaAPI.Security
{
    public class ProtectionCore
    {
        private static ProtectionCore? _instance;
        public static ProtectionCore Instance => _instance ??= new ProtectionCore();

        private CancellationTokenSource? _protectionCts;
        private string _currentDiscordId = "Unknown";
        private string _currentHwid = "Unknown";

        private ProtectionCore() { }

        public void StartProtection(string discordId, string hwid, Process? targetProcess = null)
        {
            _currentDiscordId = discordId;
            _currentHwid = hwid;
            _protectionCts = new CancellationTokenSource();

            Task.Run(() => ObfuscationLoop(_protectionCts.Token, targetProcess));
            Task.Run(() => AntiInjectionLoop(_protectionCts.Token));
        }

        private async Task ObfuscationLoop(CancellationToken token, Process? targetProcess)
        {
            int cycleCount = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    string randomName = (cycleCount % 60 == 0 && cycleCount > 0) 
                        ? GenerateRandomString(45, false) 
                        : GenerateRandomString(45, true);

                    // Obfuscate Launcher
                    UpdateWindowTitle(Process.GetCurrentProcess(), randomName);

                    // Obfuscate Minecraft (The Vanguard Target)
                    if (targetProcess != null && !targetProcess.HasExited)
                    {
                        UpdateWindowTitle(targetProcess, randomName);
                    }
                    
                    cycleCount++;
                }
                catch { }

                await Task.Delay(3000, token);
            }
        }

        private async Task AntiInjectionLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var modules = Process.GetCurrentProcess().Modules;
                    foreach (ProcessModule module in modules)
                    {
                        string? name = module.ModuleName?.ToLower();
                        if (name == null) continue;

                        // List of common injection DLLs or suspicious patterns
                        if (name.Contains("cheat") || name.Contains("hack") || name.Contains("inject") || 
                            name.Contains("extreme") || name.Contains("processhacker") || name.Contains("vape"))
                        {
                            await HandleDetection($"Suspicious DLL Detection: {name}");
                        }
                    }
                }
                catch { }

                await Task.Delay(5000, token);
            }
        }

        private async Task HandleDetection(string reason)
        {
            await AntiCheatManager.Instance.BanUserAsync(_currentDiscordId, reason, _currentHwid);
            Process.GetCurrentProcess().Kill();
        }

        private void UpdateWindowTitle(Process proc, string title)
        {
            try {
                proc.Refresh();
                IntPtr handle = proc.MainWindowHandle;
                if (handle != IntPtr.Zero)
                {
                    SetWindowText(handle, title);
                }
            } catch { }
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool SetWindowText(IntPtr hwnd, string lpString);

        private string GenerateRandomString(int length, bool withDash)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            
            if (withDash && length > 25)
            {
                var part1 = new string(Enumerable.Repeat(chars, 25).Select(s => s[random.Next(s.Length)]).ToArray());
                var part2 = new string(Enumerable.Repeat(chars, length - 26).Select(s => s[random.Next(s.Length)]).ToArray());
                return $"{part1}-{part2}";
            }
            
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public void StopProtection()
        {
            _protectionCts?.Cancel();
        }
    }
}
