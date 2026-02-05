using CmlLib.Core.Auth;
using CmlLib.Core.Auth.Microsoft;
using NovaAPI.Models;
using System;
using System.Threading.Tasks;

namespace NovaAPI.Authentication
{
    public class AuthService
    {
        private readonly JELoginHandler _loginHandler;
        private NovaSession? _currentSession;
        public bool IsDemoMode { get; set; } = false;

        public AuthService()
        {
            // The 3.3.1 version uses default Azure ClientID for Minecraft if not specified
            _loginHandler = new JELoginHandlerBuilder().Build();
        }

        public NovaSession? CurrentSession => _currentSession;

        public async Task<NovaSession> LoginAsync()
        {
            try 
            {
                if (IsDemoMode) return CreateDemoSession();

                // Interactive login via Microsoft
                var session = await _loginHandler.Authenticate();
                
                _currentSession = new NovaSession
                {
                    Username = session.Username,
                    UUID = session.UUID,
                    AccessToken = session.AccessToken,
                    UserType = "msa"
                };

                return _currentSession;
            }
            catch (Exception ex)
            {
                // Detailed debug info for the console
                System.Diagnostics.Debug.WriteLine($"[AUTH ERROR] {ex}");

                // Throw a more user-friendly message but DO NOT fallback to demo
                // This allows the user to see the actual Microsoft error
                throw new Exception($"Microsoft Auth Failed ({ex.Message}). Check your browser or internet connection.", ex);
            }
        }

        private const string DiscordClientId = "1467495679009099802";
        private const string DiscordClientSecret = "GcpA4HvnLKNnZkGKsQLFod9Snww-0YEi";
        private const string DiscordRedirectUri = "https://nebulaclient.netlify.app/";

        public async Task<NovaSession> LoginWithDiscordAsync(string authCode)
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                try
                {
                    // 1. Exchange Code for Access Token
                    var tokenRequest = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { "client_id", DiscordClientId },
                        { "client_secret", DiscordClientSecret },
                        { "grant_type", "authorization_code" },
                        { "code", authCode },
                        { "redirect_uri", DiscordRedirectUri }
                    };

                    var tokenResponse = await client.PostAsync("https://discord.com/api/oauth2/token", new System.Net.Http.FormUrlEncodedContent(tokenRequest));
                    var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
                    
                    if (!tokenResponse.IsSuccessStatusCode)
                        throw new Exception($"Discord Token Error: {tokenJson}");

                    var tokenData = Newtonsoft.Json.Linq.JObject.Parse(tokenJson);
                    string accessToken = tokenData["access_token"]?.ToString() ?? throw new Exception("No access token received.");

                    // 2. Fetch User Profile (@me)
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    var userResponse = await client.GetAsync("https://discord.com/api/users/@me");
                    var userJson = await userResponse.Content.ReadAsStringAsync();

                    if (!userResponse.IsSuccessStatusCode)
                        throw new Exception($"Discord User Error: {userJson}");

                    var userData = Newtonsoft.Json.Linq.JObject.Parse(userJson);
                    string username = userData["username"]?.ToString() ?? "Unknown";
                    string userId = userData["id"]?.ToString() ?? "";
                    string avatarHash = userData["avatar"]?.ToString() ?? "";
                    
                    // Construct Avatar URL
                    string avatarUrl = string.IsNullOrEmpty(avatarHash) 
                        ? "https://cdn.discordapp.com/embed/avatars/0.png"
                        : $"https://cdn.discordapp.com/avatars/{userId}/{avatarHash}.png?size=128";

                    _currentSession = new NovaSession
                    {
                        Username = username,
                        UserType = "discord",
                        DiscordId = userId,
                        AccessToken = accessToken,
                        LoggedInAt = DateTime.Now,
                        AvatarUrl = avatarUrl
                    };

                    return _currentSession;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DISCORD AUTH ERROR] {ex}");
                    throw new Exception($"Discord Authentication failed: {ex.Message}");
                }
            }
        }

        public string GetDiscordAuthUrl()
        {
            return $"https://discord.com/oauth2/authorize?client_id={DiscordClientId}&response_type=code&redirect_uri={System.Net.WebUtility.UrlEncode(DiscordRedirectUri)}&scope=identify+guilds+email";
        }

        private NovaSession CreateDemoSession(string name = "Nebula_Dev")
        {
            _currentSession = new NovaSession
            {
                Username = name,
                UUID = Guid.NewGuid().ToString(),
                AccessToken = "demo_token",
                UserType = "demo"
            };
            return _currentSession;
        }

        public async Task<NovaSession?> SilentLoginAsync()
        {
            try
            {
                if (IsDemoMode) return CreateDemoSession();

                var session = await _loginHandler.AuthenticateSilently();
                _currentSession = new NovaSession
                {
                    Username = session.Username,
                    UUID = session.UUID,
                    AccessToken = session.AccessToken
                };
                return _currentSession;
            }
            catch
            {
                return null;
            }
        }

        public void Logout() => _currentSession = null;
    }
}
