using PlanarWar.Client.Network;
using System;
using System.Threading.Tasks;

namespace PlanarWar.Client.Core.Application
{
    public sealed class AuthSessionController
    {
        private readonly SessionState sessionState;
        private readonly PlanarWarApiClient apiClient;
        private readonly PlanarWarWsClient wsClient;
        private readonly bool reconnectWsAfterLogin;

        public AuthSessionController(SessionState sessionState, PlanarWarApiClient apiClient, PlanarWarWsClient wsClient, bool reconnectWsAfterLogin)
        {
            this.sessionState = sessionState;
            this.apiClient = apiClient;
            this.wsClient = wsClient;
            this.reconnectWsAfterLogin = reconnectWsAfterLogin;
        }

        public async Task LoginAsync(string emailOrName, string password)
        {
            var result = await apiClient.LoginAsync(emailOrName, password);
            var token = result["token"]?.Value<string>() ?? result["accessToken"]?.Value<string>() ?? string.Empty;
            var displayName = result["displayName"]?.Value<string>() ?? result["username"]?.Value<string>() ?? emailOrName;

            sessionState.ApplyLogin(token, displayName, string.IsNullOrWhiteSpace(token) ? "Login response missing token." : $"Authenticated as {displayName}.");
            wsClient?.SetAuthToken(token);

            if (wsClient != null && reconnectWsAfterLogin)
            {
                wsClient.Reconnect();
            }
        }

        public void Logout()
        {
            sessionState.ClearLogin();
            wsClient?.ClearAuthToken();
        }
    }
}
