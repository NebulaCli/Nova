using CmlLib.Core.Auth;
using CmlLib.Core.Auth.Microsoft;
using NovaAPI.Models;
using System.Threading.Tasks;

namespace NovaAPI.Authentication
{
    public class AuthService
    {
        private readonly JELoginHandler _loginHandler;
        private NovaSession? _currentSession;

        public AuthService()
        {
            // Initialize with default parameters
            _loginHandler = new JELoginHandlerBuilder().Build();
        }

        public NovaSession? CurrentSession => _currentSession;

        public async Task<NovaSession> LoginAsync()
        {
            // Use the interactive login flow
            var session = await _loginHandler.Authenticate();
            
            _currentSession = new NovaSession
            {
                Username = session.Username,
                UUID = session.UUID,
                AccessToken = session.AccessToken
            };

            return _currentSession;
        }

        public async Task<NovaSession?> SilentLoginAsync()
        {
            try
            {
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

        public void Logout()
        {
            _currentSession = null;
        }
    }
}
