using PlanarWar.Client.Core.Application;
using PlanarWar.Client.Network;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlanarWar.Client.Core
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
        private PlanarWar.Client.Core.Presentation.SummaryScreenPresenter summaryPresenter;
        private PlanarWar.Client.Core.Presentation.CityScreenPresenter cityPresenter;
        private PlanarWar.Client.Core.Presentation.BlackMarketScreenPresenter blackMarketPresenter;

        private VisualElement summaryScreen;
        private VisualElement cityScreen;
        private VisualElement blackMarketScreen;
        private Label connectionValue;
        private Label shardValue;
        private Label roomValue;
        private Label summaryStatusValue;
        private Label authStatusValue;
        private Label accountValue;
        private Label actionHintValue;
        private Label lastUpdatedValue;

        private TextField loginNameField;
        private TextField passwordField;

        private void Awake()
        {
            networkClient ??= FindFirstObjectByType<PlanarWarWsClient>();
            router ??= FindFirstObjectByType<PlanarWarMessageRouter>();
            uiDocument ??= FindFirstObjectByType<UIDocument>();

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
                BindUi(uiDocument.rootVisualElement);
                summaryPresenter = new PlanarWar.Client.Core.Presentation.SummaryScreenPresenter(uiDocument.rootVisualElement);
                cityPresenter = new PlanarWar.Client.Core.Presentation.CityScreenPresenter(uiDocument.rootVisualElement);
                blackMarketPresenter = new PlanarWar.Client.Core.Presentation.BlackMarketScreenPresenter(uiDocument.rootVisualElement);

                summaryScreen = uiDocument.rootVisualElement.Q<VisualElement>("summary-screen");
                cityScreen = uiDocument.rootVisualElement.Q<VisualElement>("development-screen");
                blackMarketScreen = uiDocument.rootVisualElement.Q<VisualElement>("placeholder-screen");
                connectionValue = uiDocument.rootVisualElement.Q<Label>("connection-value");
                shardValue = uiDocument.rootVisualElement.Q<Label>("shard-value");
                roomValue = uiDocument.rootVisualElement.Q<Label>("room-value");
                summaryStatusValue = uiDocument.rootVisualElement.Q<Label>("summary-status-value");
                authStatusValue = uiDocument.rootVisualElement.Q<Label>("auth-status-value");
                accountValue = uiDocument.rootVisualElement.Q<Label>("account-value");
                actionHintValue = uiDocument.rootVisualElement.Q<Label>("action-hint-value");
                lastUpdatedValue = uiDocument.rootVisualElement.Q<Label>("last-updated-value");
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

        private void BindUi(VisualElement root)
        {
            loginNameField = root.Q<TextField>("login-name-field");
            passwordField = root.Q<TextField>("password-field");

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

        private void Render()
        {
            if (connectionValue != null) connectionValue.text = sessionState.IsConnected ? "Connected" : "Disconnected";
            if (shardValue != null) shardValue.text = sessionState.ShardId;
            if (roomValue != null) roomValue.text = sessionState.RoomId;
            if (summaryStatusValue != null) summaryStatusValue.text = summaryState.IsLoaded ? "Summary loaded" : summaryState.LastError;
            if (authStatusValue != null) authStatusValue.text = sessionState.LoginStatus;
            if (accountValue != null) accountValue.text = sessionState.DisplayName;
            if (actionHintValue != null) actionHintValue.text = summaryState.Snapshot.HasCity ? "Use City / Black Market tabs for lane-specific read-only surfaces." : "Founder mode: no city snapshot yet.";
            if (lastUpdatedValue != null) lastUpdatedValue.text = summaryState.IsLoaded ? $"Updated {summaryState.LastUpdatedUtc:HH:mm:ss} UTC" : "No summary fetch yet.";

            summaryPresenter?.Render(summaryState.Snapshot);
            cityPresenter?.Render(summaryState.Snapshot);
            blackMarketPresenter?.Render(summaryState.Snapshot);

            if (summaryScreen != null) summaryScreen.style.display = navigationState.ActiveScreen == ShellScreen.Summary ? DisplayStyle.Flex : DisplayStyle.None;
            if (cityScreen != null) cityScreen.style.display = navigationState.ActiveScreen == ShellScreen.City ? DisplayStyle.Flex : DisplayStyle.None;
            if (blackMarketScreen != null) blackMarketScreen.style.display = navigationState.ActiveScreen == ShellScreen.BlackMarket ? DisplayStyle.Flex : DisplayStyle.None;
        }

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
