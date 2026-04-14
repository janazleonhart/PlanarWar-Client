using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace PlanarWar.Client.Network
{
    public sealed class PlanarWarApiClient
    {
        private readonly Func<string> getHttpBaseUrl;
        private readonly Func<string> getBearerToken;

        public PlanarWarApiClient(Func<string> getHttpBaseUrl, Func<string> getBearerToken = null)
        {
            this.getHttpBaseUrl = getHttpBaseUrl ?? throw new ArgumentNullException(nameof(getHttpBaseUrl));
            this.getBearerToken = getBearerToken;
        }

        public Task<JObject> FetchSummaryAsync()
        {
            return GetJsonAsync(BuildUrl("/api/me"));
        }

        public Task<JObject> LoginAsync(string emailOrName, string password)
        {
            if (string.IsNullOrWhiteSpace(emailOrName))
            {
                throw new ArgumentException("emailOrName is required", nameof(emailOrName));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("password is required", nameof(password));
            }

            var body = new JObject
            {
                ["emailOrName"] = emailOrName.Trim(),
                ["password"] = password
            };

            return PostJsonAsync(BuildUrl("/api/auth/login"), body, includeBearerToken: false);
        }

        public Task<JObject> StartResearchAsync(string techId)
        {
            if (string.IsNullOrWhiteSpace(techId))
            {
                throw new ArgumentException("techId is required", nameof(techId));
            }

            var body = new JObject
            {
                ["techId"] = techId,
                ["serviceMode"] = "private_city"
            };

            return PostJsonAsync(BuildUrl("/api/tech/start"), body, includeBearerToken: true);
        }

        private string BuildUrl(string path)
        {
            var baseUrl = getHttpBaseUrl()?.Trim();
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidOperationException("HTTP base URL is not configured.");
            }

            return baseUrl.TrimEnd('/') + path;
        }

        private async Task<JObject> GetJsonAsync(string url)
        {
            using var request = UnityWebRequest.Get(url);
            ApplyHeaders(request, includeBearerToken: true);
            await SendAsync(request);
            return ParseResponse(request);
        }

        private async Task<JObject> PostJsonAsync(string url, JObject body, bool includeBearerToken)
        {
            var bytes = Encoding.UTF8.GetBytes(body.ToString());
            using var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(bytes),
                downloadHandler = new DownloadHandlerBuffer(),
            };
            request.SetRequestHeader("Content-Type", "application/json");
            ApplyHeaders(request, includeBearerToken);
            await SendAsync(request);
            return ParseResponse(request);
        }

        private void ApplyHeaders(UnityWebRequest request, bool includeBearerToken)
        {
            request.SetRequestHeader("Accept", "application/json");
            if (!includeBearerToken)
            {
                return;
            }

            var token = getBearerToken?.Invoke();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.SetRequestHeader("Authorization", $"Bearer {token.Trim()}");
            }
        }

        private static async Task SendAsync(UnityWebRequest request)
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }
        }

        private static JObject ParseResponse(UnityWebRequest request)
        {
            if (request.result != UnityWebRequest.Result.Success)
            {
                throw new InvalidOperationException($"HTTP {request.responseCode}: {request.error}\n{request.downloadHandler?.text}");
            }

            var text = request.downloadHandler?.text;
            if (string.IsNullOrWhiteSpace(text))
            {
                return new JObject();
            }

            return JObject.Parse(text);
        }
    }
}
