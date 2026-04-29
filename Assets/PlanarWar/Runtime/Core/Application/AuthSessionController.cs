using Newtonsoft.Json.Linq;
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
            ApplyAuthResult(result, emailOrName, "Login response missing token.");
        }

        public async Task RegisterAsync(string displayName, string email, string password)
        {
            var result = await apiClient.RegisterAsync(displayName, email, password);
            if (ApplyAuthResult(result, displayName, "Account created. Signing in..."))
            {
                return;
            }

            try
            {
                var loginIdentity = string.IsNullOrWhiteSpace(email) ? displayName : email;
                await LoginAsync(loginIdentity, password);
            }
            catch (Exception ex)
            {
                sessionState.SetLoginStatus($"Account created. Sign in to continue. Auto-login failed: {ex.Message}");
            }
        }

        private bool ApplyAuthResult(JObject result, string fallbackDisplayName, string missingTokenStatus)
        {
            var token = ResolveString(result, "token")
                ?? ResolveString(result, "accessToken")
                ?? ResolveString(result?["auth"] as JObject, "token")
                ?? ResolveString(result?["auth"] as JObject, "accessToken")
                ?? string.Empty;

            var account = result?["account"] as JObject ?? result?["user"] as JObject;
            var displayName = ResolveString(result, "displayName")
                ?? ResolveString(result, "username")
                ?? ResolveString(result, "handle")
                ?? ResolveString(account, "displayName")
                ?? ResolveString(account, "username")
                ?? ResolveString(account, "handle")
                ?? fallbackDisplayName;

            if (string.IsNullOrWhiteSpace(token))
            {
                sessionState.SetLoginStatus(missingTokenStatus);
                return false;
            }

            sessionState.ApplyLogin(token, displayName, $"Authenticated as {displayName}.");
            wsClient?.SetAuthToken(token);

            if (wsClient != null && reconnectWsAfterLogin)
            {
                wsClient.Reconnect();
            }

            return true;
        }

        private static string ResolveString(JObject body, string key)
        {
            return body?[key]?.Read<string>();
        }

        public void Logout()
        {
            sessionState.ClearLogin();
            wsClient?.ClearAuthToken();
        }
    }
}
