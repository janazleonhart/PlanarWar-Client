using PlanarWar.Client.Core;
using PlanarWar.Client.Core.Application;
using PlanarWar.Client.Network;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlanarWar.Client.UI
{
    public sealed class ClientBootstrap : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PlanarWarWsClient networkClient;
        [SerializeField] private PlanarWarMessageRouter router;
        [SerializeField] private UIDocument uiDocument;

        [Header("HTTP")]
        [SerializeField] private string httpBaseUrl = "";
        [SerializeField] private bool autoFetchSummaryOnStart = true;
        [SerializeField] private bool autoRequestWhereAmIOnConnect = true;
        [SerializeField] private bool autoJoinLobbyOnConnect = true;
        [SerializeField] private bool reconnectWsAfterLogin = true;

        private SessionState sessionState;
        private SummaryState summaryState;
        private PlanarWarApiClient apiClient;

        private AuthSessionController authController;
        private SummaryRefreshController summaryController;
        private WsSessionController wsController;
        private ShellNavigationState navigationState;
        private AppShellController appShellController;
        private float nextClockRenderAt;

        private TextField loginNameField;
        private TextField passwordField;

        private void Awake()
        {
            networkClient ??= FindFirstObjectByType<PlanarWarWsClient>();
            router ??= FindFirstObjectByType<PlanarWarMessageRouter>();
            uiDocument ??= FindFirstObjectByType<UIDocument>();

            EnsureFallbackCamera();

            sessionState = new SessionState();
            summaryState = new SummaryState();
            navigationState = new ShellNavigationState();

            var resolvedHttpBaseUrl = ResolveHttpBaseUrl();
            sessionState.SetUrls(networkClient != null ? networkClient.ServerUrl : "-", resolvedHttpBaseUrl);
            apiClient = new PlanarWarApiClient(() => sessionState.HttpBaseUrl, () => sessionState.BearerToken);

            authController = new AuthSessionController(sessionState, apiClient, networkClient, reconnectWsAfterLogin);
            summaryController = new SummaryRefreshController(apiClient, summaryState);
            wsController = new WsSessionController(networkClient, router, sessionState, autoRequestWhereAmIOnConnect, autoJoinLobbyOnConnect);

            if (uiDocument != null)
            {
                var root = uiDocument.rootVisualElement;
                BindUi(root);
                appShellController = new AppShellController(root, sessionState, summaryState, navigationState);
            }

            sessionState.Changed += Render;
            summaryState.Changed += Render;
            navigationState.Changed += Render;
        }

        private void Start()
        {
            networkClient?.Connect();
            if (autoFetchSummaryOnStart)
            {
                RefreshSummary();
            }

            Render();
        }

        private void OnDestroy()
        {
            wsController?.Dispose();
            sessionState.Changed -= Render;
            summaryState.Changed -= Render;
            navigationState.Changed -= Render;
        }

        private void Update()
        {
            if (Time.unscaledTime < nextClockRenderAt)
            {
                return;
            }

            nextClockRenderAt = Time.unscaledTime + 1f;
            Render();
        }


        private static void EnsureFallbackCamera()
        {
            if (Camera.allCamerasCount > 0)
            {
                return;
            }

            var cameraObject = new GameObject("FallbackUICamera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.03f, 0.05f, 0.1f, 1f);
            camera.orthographic = true;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.depth = -100f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";
            DontDestroyOnLoad(cameraObject);
        }

        private void BindUi(VisualElement root)
        {
            loginNameField = root.Q<TextField>("login-name-field");
            passwordField = root.Q<TextField>("password-field");

            if (passwordField != null)
            {
                passwordField.isPasswordField = true;
            }

            root.Q<Button>("login-button")?.RegisterCallback<ClickEvent>(_ => Login());
            root.Q<Button>("logout-button")?.RegisterCallback<ClickEvent>(_ => Logout());
            root.Q<Button>("refresh-button")?.RegisterCallback<ClickEvent>(_ => RefreshSummary());
            root.Q<Button>("whereami-button")?.RegisterCallback<ClickEvent>(_ => wsController?.RequestWhereAmI());
            root.Q<Button>("ping-button")?.RegisterCallback<ClickEvent>(_ => wsController?.SendPing());

            root.Q<Button>("nav-home-button")?.RegisterCallback<ClickEvent>(_ => navigationState.SetActive(ShellScreen.Summary));
            root.Q<Button>("nav-development-button")?.RegisterCallback<ClickEvent>(_ => navigationState.SetActive(ShellScreen.City));
            root.Q<Button>("nav-warfront-button")?.RegisterCallback<ClickEvent>(_ => navigationState.SetActive(ShellScreen.BlackMarket));
            root.Q<Button>("nav-social-button")?.RegisterCallback<ClickEvent>(_ => navigationState.SetActive(ShellScreen.BlackMarket));
        }

        private async void RefreshSummary()
        {
            try
            {
                await summaryController.RefreshAsync();
            }
            catch (Exception ex)
            {
                summaryState.SetError(ex.Message);
            }
        }

        private async void Login()
        {
            var user = loginNameField?.value?.Trim();
            var pass = passwordField?.value ?? string.Empty;

            try
            {
                sessionState.SetLoginStatus("Signing in...");
                await authController.LoginAsync(user, pass);
                RefreshSummary();
            }
            catch (Exception ex)
            {
                sessionState.SetLoginStatus($"Login failed: {ex.Message}");
            }
        }

        private void Logout() => authController.Logout();

        private void Render() => appShellController?.Render();

        private string ResolveHttpBaseUrl()
        {
            var explicitBase = httpBaseUrl?.Trim();
            if (!string.IsNullOrWhiteSpace(explicitBase))
            {
                return explicitBase;
            }

            var wsUrl = networkClient != null ? networkClient.ServerUrl : string.Empty;
            if (Uri.TryCreate(wsUrl, UriKind.Absolute, out var wsUri))
            {
                var builder = new UriBuilder(wsUri)
                {
                    Scheme = wsUri.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase) ? "https" : "http",
                    Path = string.Empty,
                    Query = string.Empty
                };
                return builder.Uri.GetLeftPart(UriPartial.Authority);
            }

            return "http://127.0.0.1:7777";
        }
    }
}
