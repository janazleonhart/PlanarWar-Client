using PlanarWar.Client.Core;
using PlanarWar.Client.Core.Application;
using PlanarWar.Client.Network;
using System;
using System.Threading.Tasks;
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
        private ClientVersionState versionState;
        private AppShellController appShellController;
        private float nextClockRenderAt;
        private float nextTimedRefreshCheckAt;
        private bool timedRefreshInFlight;

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
            versionState = new ClientVersionState();

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
                appShellController = new AppShellController(
                    root,
                    sessionState,
                    summaryState,
                    navigationState,
                    versionState,
                    HandleStartResearchRequestedAsync,
                    HandleStartWorkshopCraftRequestedAsync,
                    HandleCollectWorkshopRequestedAsync,
                    HandleRecruitHeroRequestedAsync,
                    HandleAcceptHeroRecruitCandidateRequestedAsync,
                    HandleDismissHeroRecruitCandidatesRequestedAsync,
                    HandleReinforceArmyRequestedAsync,
                    RefreshSummary,
                    () => navigationState.SetActive(ShellScreen.Summary));
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
            if (Time.unscaledTime >= nextTimedRefreshCheckAt)
            {
                nextTimedRefreshCheckAt = Time.unscaledTime + 1f;
                MaybeRefreshTimedState();
            }

            if (Time.unscaledTime < nextClockRenderAt)
            {
                return;
            }

            nextClockRenderAt = Time.unscaledTime + 1f;
            Render();
        }

        private void MaybeRefreshTimedState()
        {
            if (timedRefreshInFlight || summaryState == null || !summaryState.IsLoaded || summaryState.IsActionBusy)
            {
                return;
            }

            var snapshot = summaryState.Snapshot;
            var nowUtc = DateTime.UtcNow;

            var heroRecruitment = snapshot.HeroRecruitment;
            var heroScoutingElapsed = heroRecruitment != null
                && string.Equals(heroRecruitment.Status, "scouting", StringComparison.OrdinalIgnoreCase)
                && heroRecruitment.FinishesAtUtc.HasValue
                && heroRecruitment.FinishesAtUtc.Value <= nowUtc;

            var heroCandidateReviewElapsed = heroRecruitment != null
                && string.Equals(heroRecruitment.Status, "candidates_ready", StringComparison.OrdinalIgnoreCase)
                && heroRecruitment.CandidateExpiresAtUtc.HasValue
                && heroRecruitment.CandidateExpiresAtUtc.Value <= nowUtc;

            var armyReinforcement = snapshot.ArmyReinforcement;
            var armyReinforcementElapsed = armyReinforcement != null
                && string.Equals(armyReinforcement.Status, "reinforcing", StringComparison.OrdinalIgnoreCase)
                && armyReinforcement.FinishesAtUtc.HasValue
                && armyReinforcement.FinishesAtUtc.Value <= nowUtc;

            if (!heroScoutingElapsed && !heroCandidateReviewElapsed && !armyReinforcementElapsed)
            {
                return;
            }

            TriggerTimedRefresh();
        }

        private async void TriggerTimedRefresh()
        {
            if (timedRefreshInFlight)
            {
                return;
            }

            timedRefreshInFlight = true;
            try
            {
                await summaryController.RefreshAsync();
            }
            catch (Exception ex)
            {
                summaryState.SetError(ex.Message);
            }
            finally
            {
                timedRefreshInFlight = false;
            }
        }

        private async Task HandleStartResearchRequestedAsync(string techId)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(techId) || summaryState.IsActionBusy)
            {
                return;
            }

            try
            {
                summaryState.BeginResearchAction(techId);
                await apiClient.StartResearchAsync(techId.Trim());
                await summaryController.RefreshAsync();
                summaryState.FinishAction($"Research started: {techId.Trim()}");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Research failed: {ex.Message}", failed: true);
            }
        }


        private async Task HandleStartWorkshopCraftRequestedAsync(string recipeId)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(recipeId) || summaryState.IsActionBusy)
            {
                return;
            }

            try
            {
                summaryState.BeginWorkshopCraft(recipeId);
                await apiClient.StartWorkshopCraftAsync(recipeId.Trim());
                await summaryController.RefreshAsync();
                summaryState.FinishAction($"Workshop craft started: {recipeId.Trim()}");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Workshop craft failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleCollectWorkshopRequestedAsync(string jobId)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(jobId) || summaryState.IsActionBusy)
            {
                return;
            }

            try
            {
                summaryState.BeginWorkshopCollect(jobId);
                await apiClient.CollectWorkshopAsync(jobId.Trim());
                await summaryController.RefreshAsync();
                summaryState.FinishAction($"Workshop collect complete: {jobId.Trim()}");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Workshop collect failed: {ex.Message}", failed: true);
            }
        }



        private async Task HandleRecruitHeroRequestedAsync(string role)
        {
            if (summaryState == null || apiClient == null || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedRole = role?.Trim() ?? string.Empty;

            try
            {
                summaryState.BeginHeroRecruit(trimmedRole);
                await apiClient.RecruitHeroAsync(trimmedRole);
                await summaryController.RefreshAsync();
                summaryState.FinishAction(string.IsNullOrWhiteSpace(trimmedRole)
                    ? "Hero recruitment scouting started."
                    : $"Hero recruit started: {trimmedRole}");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Hero recruit failed: {ex.Message}", failed: true);
            }
        }


        private async Task HandleAcceptHeroRecruitCandidateRequestedAsync(string candidateId)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(candidateId) || summaryState.IsActionBusy)
            {
                return;
            }

            try
            {
                summaryState.BeginHeroRecruitAccept(candidateId);
                await apiClient.AcceptHeroRecruitCandidateAsync(candidateId.Trim());
                await summaryController.RefreshAsync();
                summaryState.FinishAction("Hero candidate accepted.");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Hero candidate accept failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleDismissHeroRecruitCandidatesRequestedAsync()
        {
            if (summaryState == null || apiClient == null || summaryState.IsActionBusy)
            {
                return;
            }

            try
            {
                summaryState.BeginHeroRecruitDismiss();
                await apiClient.DismissHeroRecruitCandidatesAsync();
                await summaryController.RefreshAsync();
                summaryState.FinishAction("Hero candidates dismissed.");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Hero candidate dismiss failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleReinforceArmyRequestedAsync(string armyId)
        {
            if (summaryState == null || apiClient == null || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedArmyId = armyId?.Trim() ?? string.Empty;

            try
            {
                summaryState.BeginArmyReinforcement(trimmedArmyId);
                await apiClient.ReinforceArmyAsync(trimmedArmyId);
                await summaryController.RefreshAsync();
                summaryState.FinishAction(string.IsNullOrWhiteSpace(trimmedArmyId)
                    ? "Army reinforcement started."
                    : $"Army reinforcement started: {trimmedArmyId}");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Army reinforcement failed: {ex.Message}", failed: true);
            }
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
            root.Q<Button>("nav-social-button")?.RegisterCallback<ClickEvent>(_ => navigationState.SetActive(ShellScreen.Social));

            root.Q<Button>("chat-all-button")?.RegisterCallback<ClickEvent>(_ => sessionState.SetActiveChatChannel("all"));
            root.Q<Button>("chat-room-button")?.RegisterCallback<ClickEvent>(_ => sessionState.SetActiveChatChannel("room"));
            root.Q<Button>("chat-system-button")?.RegisterCallback<ClickEvent>(_ => sessionState.SetActiveChatChannel("system"));
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
