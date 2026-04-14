using Newtonsoft.Json.Linq;
using PlanarWar.Client.Network;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlanarWar.Client.Core
{
    public sealed class ClientBootstrap : MonoBehaviour
    {
        private enum ChapterView
        {
            Summary,
            Development,
            Warfront,
            Social
        }

        private enum DevelopmentLane
        {
            Research,
            Workshop,
            Growth
        }

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

        [Header("Runtime")]
        [SerializeField] private bool createFallbackCamera = true;

        private SessionState sessionState;
        private SummaryState summaryState;
        private PlanarWarApiClient apiClient;

        private ChapterView activeChapter = ChapterView.Summary;
        private DevelopmentLane activeDevelopmentLane = DevelopmentLane.Research;
        private bool showTimerDiagnostics;
        private float nextLiveRefreshAt;
        private int heartbeatCounter;

        private Label connectionValue;
        private Label shardValue;
        private Label roomValue;
        private Label summaryStatusValue;
        private Label statusHeadlineValue;
        private Label resourcesValue;
        private Label productionValue;
        private Label researchValue;
        private Label warningsValue;
        private Label readyOpsValue;
        private Label heroStatusValue;
        private Label armyStatusValue;
        private Label researchTimerValue;
        private Label workshopTimerValue;
        private Label missionTimerValue;
        private Label resourceTickValue;
        private Label lastUpdatedValue;
        private Label liveClockValue;
        private Label actionHintValue;
        private Label authStatusValue;
        private Label accountValue;
        private Label authCardTitle;
        private Label commsStatusValue;
        private Label commsHintValue;
        private Label currentChapterMeta;
        private Label currentChapterTitle;
        private Label currentChapterKicker;
        private Label currentChapterCopy;
        private Label developmentHeadlineValue;
        private Label developmentCopyValue;
        private Label devLaneTitleValue;
        private Label devLaneCopyValue;
        private Label devLaneCard1Title;
        private Label devLaneCard1Value;
        private Label devLaneCard2Title;
        private Label devLaneCard2Value;
        private Label devLaneCard3Title;
        private Label devLaneCard3Value;
        private Label devResearchFocusValue;
        private Label devNextTechValue;
        private Label devWorkshopValue;
        private Label devGrowthValue;
        private Label devSupportValue;
        private Label devNoteValue;
        private Label devActionsTitleValue;
        private Label devActionsCopyValue;
        private Label devResearchCardsCopyValue;
        private Label devResearchCard1FamilyValue;
        private Label devResearchCard1TitleValue;
        private Label devResearchCard1LoreValue;
        private Label devResearchCard1NoteValue;
        private Label devResearchCard2FamilyValue;
        private Label devResearchCard2TitleValue;
        private Label devResearchCard2LoreValue;
        private Label devResearchCard2NoteValue;
        private Label devResearchCard3FamilyValue;
        private Label devResearchCard3TitleValue;
        private Label devResearchCard3LoreValue;
        private Label devResearchCard3NoteValue;
        private Label devResearchCard4FamilyValue;
        private Label devResearchCard4TitleValue;
        private Label devResearchCard4LoreValue;
        private Label devResearchCard4NoteValue;

        private Label devWorkshopCardsCopyValue;
        private Label devWorkshopCard1FamilyValue;
        private Label devWorkshopCard1TitleValue;
        private Label devWorkshopCard1LoreValue;
        private Label devWorkshopCard1NoteValue;
        private Label devWorkshopCard2FamilyValue;
        private Label devWorkshopCard2TitleValue;
        private Label devWorkshopCard2LoreValue;
        private Label devWorkshopCard2NoteValue;
        private Label devWorkshopCard3FamilyValue;
        private Label devWorkshopCard3TitleValue;
        private Label devWorkshopCard3LoreValue;
        private Label devWorkshopCard3NoteValue;
        private Label devWorkshopCard4FamilyValue;
        private Label devWorkshopCard4TitleValue;
        private Label devWorkshopCard4LoreValue;
        private Label devWorkshopCard4NoteValue;
        private Label devGrowthCardsCopyValue;
        private Label devGrowthCard1FamilyValue;
        private Label devGrowthCard1TitleValue;
        private Label devGrowthCard1LoreValue;
        private Label devGrowthCard1NoteValue;
        private Label devGrowthCard2FamilyValue;
        private Label devGrowthCard2TitleValue;
        private Label devGrowthCard2LoreValue;
        private Label devGrowthCard2NoteValue;
        private Label devGrowthCard3FamilyValue;
        private Label devGrowthCard3TitleValue;
        private Label devGrowthCard3LoreValue;
        private Label devGrowthCard3NoteValue;
        private Label devGrowthCard4FamilyValue;
        private Label devGrowthCard4TitleValue;
        private Label devGrowthCard4LoreValue;
        private Label devGrowthCard4NoteValue;
        private Label placeholderHeadlineValue;
        private Label placeholderCopyValue;
        private Label placeholderNoteValue;
        private Label timerDiagNowValue;
        private Label timerDiagHeartbeatValue;
        private Label timerDiagRawValue;
        private Label timerDiagComputedValue;

        private TextField loginNameField;
        private TextField passwordField;
        private TextField chatInputField;
        private Button loginButton;
        private Button logoutButton;
        private Button refreshButton;
        private Button researchButton;
        private Button whereAmIButton;
        private Button pingButton;
        private Button sendChatButton;
        private Button chatAllButton;
        private Button chatRoomButton;
        private Button chatSystemButton;
        private Button navHomeButton;
        private Button navDevelopmentButton;
        private Button navWarfrontButton;
        private Button navSocialButton;
        private Button toggleTimerDiagnosticsButton;
        private Button devResearchLaneButton;
        private Button devWorkshopLaneButton;
        private Button devGrowthLaneButton;
        private Button devRefreshButton;
        private Button devStartResearchButton;
        private Button devLanePrimaryButton;
        private Button devQuickTech1Button;
        private Button devQuickTech2Button;
        private Button devResearchCard1Button;
        private Button devResearchCard2Button;
        private Button devResearchCard3Button;
        private Button devResearchCard4Button;
        private Button devBackHomeButton;

        private ScrollView chatLogScroll;
        private VisualElement authLoginFields;
        private VisualElement authActionRow;
        private VisualElement loginButtonRow;
        private VisualElement summaryScreen;
        private VisualElement developmentScreen;
        private VisualElement placeholderScreen;
        private VisualElement timerDiagnosticCard;
        private VisualElement devResearchCardsSection;
        private VisualElement devResearchCard1;
        private VisualElement devResearchCard2;
        private VisualElement devResearchCard3;
        private VisualElement devResearchCard4;

        private VisualElement devWorkshopCardsSection;
        private VisualElement devWorkshopCard1;
        private VisualElement devWorkshopCard2;
        private VisualElement devWorkshopCard3;
        private VisualElement devWorkshopCard4;
        private VisualElement devGrowthCardsSection;
        private VisualElement devGrowthCard1;
        private VisualElement devGrowthCard2;
        private VisualElement devGrowthCard3;
        private VisualElement devGrowthCard4;

        private void Awake()
        {
            networkClient ??= FindFirstObjectByType<PlanarWarWsClient>();
            router ??= FindFirstObjectByType<PlanarWarMessageRouter>();
            uiDocument ??= FindFirstObjectByType<UIDocument>();

            EnsureFallbackCamera();

            sessionState = new SessionState();
            summaryState = new SummaryState();
            sessionState.Changed += Render;
            summaryState.Changed += Render;

            var resolvedHttpBaseUrl = ResolveHttpBaseUrl();
            sessionState.SetUrls(networkClient != null ? networkClient.ServerUrl : "-", resolvedHttpBaseUrl);
            apiClient = new PlanarWarApiClient(() => sessionState.HttpBaseUrl, () => sessionState.BearerToken);

            if (uiDocument != null)
            {
                BindUi(uiDocument.rootVisualElement);
                ApplyRuntimeStyles(uiDocument.rootVisualElement);
            }
        }

        private void OnEnable()
        {
            if (networkClient != null)
            {
                networkClient.Connected += OnConnected;
                networkClient.Disconnected += OnDisconnected;
                networkClient.MessageReceived += OnAnyMessage;
            }

            if (router != null)
            {
                router.Welcome += OnWelcome;
                router.HelloAck += OnHelloAck;
                router.WhereAmIResult += OnWhereAmIResult;
                router.RoomJoined += OnRoomJoined;
                router.RoomLeft += OnRoomLeft;
                router.Error += OnWsError;
                router.Chat += OnChat;
            }
        }

        private void Start()
        {
            Render();

            if (autoFetchSummaryOnStart)
            {
                RefreshSummary();
            }
        }

        private void OnDisable()
        {
            if (networkClient != null)
            {
                networkClient.Connected -= OnConnected;
                networkClient.Disconnected -= OnDisconnected;
                networkClient.MessageReceived -= OnAnyMessage;
            }

            if (router != null)
            {
                router.Welcome -= OnWelcome;
                router.HelloAck -= OnHelloAck;
                router.WhereAmIResult -= OnWhereAmIResult;
                router.RoomJoined -= OnRoomJoined;
                router.RoomLeft -= OnRoomLeft;
                router.Error -= OnWsError;
                router.Chat -= OnChat;
            }

            if (sessionState != null) sessionState.Changed -= Render;
            if (summaryState != null) summaryState.Changed -= Render;
        }

        private void Update()
        {
            if (Time.unscaledTime < nextLiveRefreshAt)
            {
                return;
            }

            nextLiveRefreshAt = Time.unscaledTime + 1f;
            heartbeatCounter++;
            RenderLiveSurfaces();
        }

        private void BindUi(VisualElement root)
        {
            connectionValue = root.Q<Label>("connection-value");
            shardValue = root.Q<Label>("shard-value");
            roomValue = root.Q<Label>("room-value");
            summaryStatusValue = root.Q<Label>("summary-status-value");
            statusHeadlineValue = root.Q<Label>("status-headline-value");
            resourcesValue = root.Q<Label>("resources-value");
            productionValue = root.Q<Label>("production-value");
            researchValue = root.Q<Label>("research-value");
            warningsValue = root.Q<Label>("warnings-value");
            readyOpsValue = root.Q<Label>("ready-ops-value");
            heroStatusValue = root.Q<Label>("hero-status-value");
            armyStatusValue = root.Q<Label>("army-status-value");
            researchTimerValue = root.Q<Label>("research-timer-value");
            workshopTimerValue = root.Q<Label>("workshop-timer-value");
            missionTimerValue = root.Q<Label>("mission-timer-value");
            resourceTickValue = root.Q<Label>("resource-tick-value");
            lastUpdatedValue = root.Q<Label>("last-updated-value");
            liveClockValue = root.Q<Label>("live-clock-value");
            actionHintValue = root.Q<Label>("action-hint-value");
            authStatusValue = root.Q<Label>("auth-status-value");
            accountValue = root.Q<Label>("account-value");
            authCardTitle = root.Q<Label>("auth-card-title");
            commsStatusValue = root.Q<Label>("comms-status-value");
            commsHintValue = root.Q<Label>("comms-hint-value");
            currentChapterMeta = root.Q<Label>("current-chapter-meta");
            currentChapterTitle = root.Q<Label>("current-chapter-title");
            currentChapterKicker = root.Q<Label>("current-chapter-kicker");
            currentChapterCopy = root.Q<Label>("current-chapter-copy");
            developmentHeadlineValue = root.Q<Label>("development-headline-value");
            developmentCopyValue = root.Q<Label>("development-copy-value");
            devLaneTitleValue = root.Q<Label>("dev-lane-title-value");
            devLaneCopyValue = root.Q<Label>("dev-lane-copy-value");
            devLaneCard1Title = root.Q<Label>("dev-lane-card-1-title");
            devLaneCard1Value = root.Q<Label>("dev-lane-card-1-value");
            devLaneCard2Title = root.Q<Label>("dev-lane-card-2-title");
            devLaneCard2Value = root.Q<Label>("dev-lane-card-2-value");
            devLaneCard3Title = root.Q<Label>("dev-lane-card-3-title");
            devLaneCard3Value = root.Q<Label>("dev-lane-card-3-value");
            devResearchFocusValue = root.Q<Label>("dev-research-focus-value");
            devNextTechValue = root.Q<Label>("dev-next-tech-value");
            devWorkshopValue = root.Q<Label>("dev-workshop-value");
            devGrowthValue = root.Q<Label>("dev-growth-value");
            devSupportValue = root.Q<Label>("dev-support-value");
            devNoteValue = root.Q<Label>("dev-note-value");
            devActionsTitleValue = root.Q<Label>("dev-actions-title-value");
            devActionsCopyValue = root.Q<Label>("dev-actions-copy-value");
            devResearchCardsCopyValue = root.Q<Label>("dev-research-cards-copy-value");
            devResearchCard1FamilyValue = root.Q<Label>("dev-research-card-1-family-value");
            devResearchCard1TitleValue = root.Q<Label>("dev-research-card-1-title-value");
            devResearchCard1LoreValue = root.Q<Label>("dev-research-card-1-lore-value");
            devResearchCard1NoteValue = root.Q<Label>("dev-research-card-1-note-value");
            devResearchCard2FamilyValue = root.Q<Label>("dev-research-card-2-family-value");
            devResearchCard2TitleValue = root.Q<Label>("dev-research-card-2-title-value");
            devResearchCard2LoreValue = root.Q<Label>("dev-research-card-2-lore-value");
            devResearchCard2NoteValue = root.Q<Label>("dev-research-card-2-note-value");
            devResearchCard3FamilyValue = root.Q<Label>("dev-research-card-3-family-value");
            devResearchCard3TitleValue = root.Q<Label>("dev-research-card-3-title-value");
            devResearchCard3LoreValue = root.Q<Label>("dev-research-card-3-lore-value");
            devResearchCard3NoteValue = root.Q<Label>("dev-research-card-3-note-value");
            devResearchCard4FamilyValue = root.Q<Label>("dev-research-card-4-family-value");
            devResearchCard4TitleValue = root.Q<Label>("dev-research-card-4-title-value");
            devResearchCard4LoreValue = root.Q<Label>("dev-research-card-4-lore-value");
            devResearchCard4NoteValue = root.Q<Label>("dev-research-card-4-note-value");

            devWorkshopCardsCopyValue = root.Q<Label>("dev-workshop-cards-copy-value");
            devWorkshopCard1FamilyValue = root.Q<Label>("dev-workshop-card-1-family-value");
            devWorkshopCard1TitleValue = root.Q<Label>("dev-workshop-card-1-title-value");
            devWorkshopCard1LoreValue = root.Q<Label>("dev-workshop-card-1-lore-value");
            devWorkshopCard1NoteValue = root.Q<Label>("dev-workshop-card-1-note-value");
            devWorkshopCard2FamilyValue = root.Q<Label>("dev-workshop-card-2-family-value");
            devWorkshopCard2TitleValue = root.Q<Label>("dev-workshop-card-2-title-value");
            devWorkshopCard2LoreValue = root.Q<Label>("dev-workshop-card-2-lore-value");
            devWorkshopCard2NoteValue = root.Q<Label>("dev-workshop-card-2-note-value");
            devWorkshopCard3FamilyValue = root.Q<Label>("dev-workshop-card-3-family-value");
            devWorkshopCard3TitleValue = root.Q<Label>("dev-workshop-card-3-title-value");
            devWorkshopCard3LoreValue = root.Q<Label>("dev-workshop-card-3-lore-value");
            devWorkshopCard3NoteValue = root.Q<Label>("dev-workshop-card-3-note-value");
            devWorkshopCard4FamilyValue = root.Q<Label>("dev-workshop-card-4-family-value");
            devWorkshopCard4TitleValue = root.Q<Label>("dev-workshop-card-4-title-value");
            devWorkshopCard4LoreValue = root.Q<Label>("dev-workshop-card-4-lore-value");
            devWorkshopCard4NoteValue = root.Q<Label>("dev-workshop-card-4-note-value");
            devGrowthCardsCopyValue = root.Q<Label>("dev-growth-cards-copy-value");
            devGrowthCard1FamilyValue = root.Q<Label>("dev-growth-card-1-family-value");
            devGrowthCard1TitleValue = root.Q<Label>("dev-growth-card-1-title-value");
            devGrowthCard1LoreValue = root.Q<Label>("dev-growth-card-1-lore-value");
            devGrowthCard1NoteValue = root.Q<Label>("dev-growth-card-1-note-value");
            devGrowthCard2FamilyValue = root.Q<Label>("dev-growth-card-2-family-value");
            devGrowthCard2TitleValue = root.Q<Label>("dev-growth-card-2-title-value");
            devGrowthCard2LoreValue = root.Q<Label>("dev-growth-card-2-lore-value");
            devGrowthCard2NoteValue = root.Q<Label>("dev-growth-card-2-note-value");
            devGrowthCard3FamilyValue = root.Q<Label>("dev-growth-card-3-family-value");
            devGrowthCard3TitleValue = root.Q<Label>("dev-growth-card-3-title-value");
            devGrowthCard3LoreValue = root.Q<Label>("dev-growth-card-3-lore-value");
            devGrowthCard3NoteValue = root.Q<Label>("dev-growth-card-3-note-value");
            devGrowthCard4FamilyValue = root.Q<Label>("dev-growth-card-4-family-value");
            devGrowthCard4TitleValue = root.Q<Label>("dev-growth-card-4-title-value");
            devGrowthCard4LoreValue = root.Q<Label>("dev-growth-card-4-lore-value");
            devGrowthCard4NoteValue = root.Q<Label>("dev-growth-card-4-note-value");
            placeholderHeadlineValue = root.Q<Label>("placeholder-headline-value");
            placeholderCopyValue = root.Q<Label>("placeholder-copy-value");
            placeholderNoteValue = root.Q<Label>("placeholder-note-value");
            timerDiagNowValue = root.Q<Label>("timer-diag-now-value");
            timerDiagHeartbeatValue = root.Q<Label>("timer-diag-heartbeat-value");
            timerDiagRawValue = root.Q<Label>("timer-diag-raw-value");
            timerDiagComputedValue = root.Q<Label>("timer-diag-computed-value");

            loginNameField = root.Q<TextField>("login-name-field");
            passwordField = root.Q<TextField>("password-field");
            chatInputField = root.Q<TextField>("chat-input-field");
            loginButton = root.Q<Button>("login-button");
            logoutButton = root.Q<Button>("logout-button");
            refreshButton = root.Q<Button>("refresh-button");
            researchButton = root.Q<Button>("start-research-button");
            whereAmIButton = root.Q<Button>("whereami-button");
            pingButton = root.Q<Button>("ping-button");
            sendChatButton = root.Q<Button>("send-chat-button");
            chatAllButton = root.Q<Button>("chat-all-button");
            chatRoomButton = root.Q<Button>("chat-room-button");
            chatSystemButton = root.Q<Button>("chat-system-button");
            navHomeButton = root.Q<Button>("nav-home-button");
            navDevelopmentButton = root.Q<Button>("nav-development-button");
            navWarfrontButton = root.Q<Button>("nav-warfront-button");
            navSocialButton = root.Q<Button>("nav-social-button");
            toggleTimerDiagnosticsButton = root.Q<Button>("toggle-timer-diagnostics-button");
            devResearchLaneButton = root.Q<Button>("dev-research-lane-button");
            devWorkshopLaneButton = root.Q<Button>("dev-workshop-lane-button");
            devGrowthLaneButton = root.Q<Button>("dev-growth-lane-button");
            devRefreshButton = root.Q<Button>("dev-refresh-button");
            devStartResearchButton = root.Q<Button>("dev-start-research-button");
            devLanePrimaryButton = root.Q<Button>("dev-lane-primary-button");
            devQuickTech1Button = root.Q<Button>("dev-quick-tech-1-button");
            devQuickTech2Button = root.Q<Button>("dev-quick-tech-2-button");
            devResearchCard1Button = root.Q<Button>("dev-research-card-1-button");
            devResearchCard2Button = root.Q<Button>("dev-research-card-2-button");
            devResearchCard3Button = root.Q<Button>("dev-research-card-3-button");
            devResearchCard4Button = root.Q<Button>("dev-research-card-4-button");
            devBackHomeButton = root.Q<Button>("dev-back-home-button");

            chatLogScroll = root.Q<ScrollView>("chat-log-scroll");
            authLoginFields = root.Q<VisualElement>("auth-login-fields");
            authActionRow = root.Q<VisualElement>("auth-action-row");
            loginButtonRow = root.Q<VisualElement>("login-button-row");
            summaryScreen = root.Q<VisualElement>("summary-screen");
            developmentScreen = root.Q<VisualElement>("development-screen");
            placeholderScreen = root.Q<VisualElement>("placeholder-screen");
            timerDiagnosticCard = root.Q<VisualElement>("timer-diagnostic-card");
            devResearchCardsSection = root.Q<VisualElement>("dev-research-cards-section");
            devResearchCard1 = root.Q<VisualElement>("dev-research-card-1");
            devResearchCard2 = root.Q<VisualElement>("dev-research-card-2");
            devResearchCard3 = root.Q<VisualElement>("dev-research-card-3");
            devResearchCard4 = root.Q<VisualElement>("dev-research-card-4");

            devWorkshopCardsSection = root.Q<VisualElement>("dev-workshop-cards-section");
            devWorkshopCard1 = root.Q<VisualElement>("dev-workshop-card-1");
            devWorkshopCard2 = root.Q<VisualElement>("dev-workshop-card-2");
            devWorkshopCard3 = root.Q<VisualElement>("dev-workshop-card-3");
            devWorkshopCard4 = root.Q<VisualElement>("dev-workshop-card-4");
            devGrowthCardsSection = root.Q<VisualElement>("dev-growth-cards-section");
            devGrowthCard1 = root.Q<VisualElement>("dev-growth-card-1");
            devGrowthCard2 = root.Q<VisualElement>("dev-growth-card-2");
            devGrowthCard3 = root.Q<VisualElement>("dev-growth-card-3");
            devGrowthCard4 = root.Q<VisualElement>("dev-growth-card-4");

            if (passwordField != null)
            {
                passwordField.isPasswordField = true;
            }

            loginButton?.RegisterCallback<ClickEvent>(_ => Login());
            logoutButton?.RegisterCallback<ClickEvent>(_ => Logout());
            refreshButton?.RegisterCallback<ClickEvent>(_ => RefreshSummary());
            researchButton?.RegisterCallback<ClickEvent>(_ => StartSuggestedResearch());
            whereAmIButton?.RegisterCallback<ClickEvent>(_ => RequestWhereAmI());
            pingButton?.RegisterCallback<ClickEvent>(_ => SendPing());
            sendChatButton?.RegisterCallback<ClickEvent>(_ => SendChat());
            chatAllButton?.RegisterCallback<ClickEvent>(_ => sessionState.SetActiveChatChannel("all"));
            chatRoomButton?.RegisterCallback<ClickEvent>(_ => sessionState.SetActiveChatChannel("room"));
            chatSystemButton?.RegisterCallback<ClickEvent>(_ => sessionState.SetActiveChatChannel("system"));
            navHomeButton?.RegisterCallback<ClickEvent>(_ => SwitchChapter(ChapterView.Summary));
            navDevelopmentButton?.RegisterCallback<ClickEvent>(_ => SwitchChapter(ChapterView.Development));
            navWarfrontButton?.RegisterCallback<ClickEvent>(_ => SwitchChapter(ChapterView.Warfront));
            navSocialButton?.RegisterCallback<ClickEvent>(_ => SwitchChapter(ChapterView.Social));
            toggleTimerDiagnosticsButton?.RegisterCallback<ClickEvent>(_ => ToggleTimerDiagnostics());
            devResearchLaneButton?.RegisterCallback<ClickEvent>(_ => SwitchDevelopmentLane(DevelopmentLane.Research));
            devWorkshopLaneButton?.RegisterCallback<ClickEvent>(_ => SwitchDevelopmentLane(DevelopmentLane.Workshop));
            devGrowthLaneButton?.RegisterCallback<ClickEvent>(_ => SwitchDevelopmentLane(DevelopmentLane.Growth));
            devRefreshButton?.RegisterCallback<ClickEvent>(_ => RefreshSummary());
            devStartResearchButton?.RegisterCallback<ClickEvent>(_ => StartSuggestedResearch());
            devLanePrimaryButton?.RegisterCallback<ClickEvent>(_ => RunDevelopmentPrimaryAction());
            devQuickTech1Button?.RegisterCallback<ClickEvent>(_ => StartResearchByIndex(0));
            devQuickTech2Button?.RegisterCallback<ClickEvent>(_ => StartResearchByIndex(1));
            devResearchCard1Button?.RegisterCallback<ClickEvent>(_ => StartResearchCardByIndex(0));
            devResearchCard2Button?.RegisterCallback<ClickEvent>(_ => StartResearchCardByIndex(1));
            devResearchCard3Button?.RegisterCallback<ClickEvent>(_ => StartResearchCardByIndex(2));
            devResearchCard4Button?.RegisterCallback<ClickEvent>(_ => StartResearchCardByIndex(3));
            devBackHomeButton?.RegisterCallback<ClickEvent>(_ => SwitchChapter(ChapterView.Summary));
        }

        private void ApplyRuntimeStyles(VisualElement root)
        {
            root.style.flexDirection = FlexDirection.Row;
            root.style.flexGrow = 1;
            root.style.paddingLeft = 14;
            root.style.paddingRight = 14;
            root.style.paddingTop = 14;
            root.style.paddingBottom = 14;
            root.style.backgroundColor = new Color(0.03f, 0.05f, 0.12f, 1f);
        }

        private void EnsureFallbackCamera()
        {
            if (!createFallbackCamera || FindFirstObjectByType<Camera>() != null)
            {
                return;
            }

            var cameraObject = new GameObject("UiFallbackCamera") { hideFlags = HideFlags.DontSave };
            var cameraComponent = cameraObject.AddComponent<Camera>();
            cameraComponent.clearFlags = CameraClearFlags.SolidColor;
            cameraComponent.backgroundColor = new Color(0.03f, 0.05f, 0.12f, 1f);
            cameraComponent.cullingMask = 0;
            cameraComponent.depth = -100f;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        }

        private string ResolveHttpBaseUrl()
        {
            if (!string.IsNullOrWhiteSpace(httpBaseUrl))
            {
                return httpBaseUrl.Trim().TrimEnd('/');
            }

            if (networkClient == null || string.IsNullOrWhiteSpace(networkClient.ServerUrl))
            {
                return "http://127.0.0.1:4000";
            }

            if (!Uri.TryCreate(networkClient.ServerUrl, UriKind.Absolute, out var wsUri))
            {
                return "http://127.0.0.1:4000";
            }

            var builder = new UriBuilder
            {
                Scheme = wsUri.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase) ? "https" : "http",
                Host = wsUri.Host,
                Port = 4000,
                Path = string.Empty,
            };

            return builder.Uri.ToString().TrimEnd('/');
        }

        private async void RefreshSummary()
        {
            try
            {
                sessionState.SetLoginStatus(sessionState.IsAuthenticated ? $"Authenticated as {sessionState.DisplayName}." : "Demo mode active.");
                var summary = await apiClient.FetchSummaryAsync();
                summaryState.Apply(summary);
                sessionState.ApplySystemNotice(sessionState.IsAuthenticated ? "Empire summary refreshed." : "Demo summary refreshed.");
            }
            catch (Exception ex)
            {
                sessionState.SetLastError(ex.Message);
                summaryState.SetError(ex.Message);
                sessionState.ApplySystemNotice($"Summary refresh failed: {ex.Message}");
                Debug.LogError($"[ClientBootstrap] Summary fetch failed: {ex}");
            }
        }

        private async void Login()
        {
            var emailOrName = loginNameField?.value?.Trim();
            var password = passwordField?.value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(emailOrName) || string.IsNullOrWhiteSpace(password))
            {
                sessionState.SetLastError("Login requires name/email and password.");
                sessionState.SetLoginStatus("Enter account name/email and password.");
                sessionState.ApplySystemNotice("Login requires name/email and password.");
                return;
            }

            try
            {
                sessionState.SetLoginStatus($"Logging in as {emailOrName}...");
                var result = await apiClient.LoginAsync(emailOrName, password);
                var token = result["token"]?.Value<string>() ?? string.Empty;
                var displayName = result["account"]?["displayName"]?.Value<string>()
                    ?? result["account"]?["display_name"]?.Value<string>()
                    ?? result["account"]?["email"]?.Value<string>()
                    ?? emailOrName;

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Login succeeded but no bearer token was returned.");
                }

                sessionState.ApplyLogin(token, displayName);
                sessionState.SetLastError("-");
                if (passwordField != null)
                {
                    passwordField.value = string.Empty;
                }

                if (networkClient != null)
                {
                    networkClient.SetAuthToken(token);
                    if (reconnectWsAfterLogin)
                    {
                        networkClient.Reconnect();
                    }
                }

                RefreshSummary();
            }
            catch (Exception ex)
            {
                sessionState.SetLastError(ex.Message);
                sessionState.SetLoginStatus("Login failed.");
                sessionState.ApplySystemNotice($"Login failed: {ex.Message}");
                Debug.LogError($"[ClientBootstrap] Login failed: {ex}");
            }
        }

        private void Logout()
        {
            sessionState.ClearLogin();
            summaryState.Apply(null);
            networkClient?.ClearAuthToken();
            if (reconnectWsAfterLogin)
            {
                networkClient?.Reconnect();
            }
            RefreshSummary();
        }

        private async void StartSuggestedResearch()
        {
            var techId = summaryState.GetSuggestedResearchId();
            if (string.IsNullOrWhiteSpace(techId))
            {
                return;
            }

            try
            {
                var result = await apiClient.StartResearchAsync(techId);
                sessionState.ApplySystemNotice($"Started research: {summaryState.GetSuggestedResearchLabel()}");
                Debug.Log($"[ClientBootstrap] Research start result: {result}");
                RefreshSummary();
            }
            catch (Exception ex)
            {
                sessionState.SetLastError(ex.Message);
                sessionState.ApplySystemNotice($"Start research failed: {ex.Message}");
                Debug.LogError($"[ClientBootstrap] Start research failed: {ex}");
            }
        }

        private async void StartResearchById(string techId, string techLabel)
        {
            if (string.IsNullOrWhiteSpace(techId) || !sessionState.IsAuthenticated)
            {
                return;
            }

            try
            {
                var result = await apiClient.StartResearchAsync(techId);
                sessionState.ApplySystemNotice($"Started research: {techLabel}");
                Debug.Log($"[ClientBootstrap] Research start result: {result}");
                RefreshSummary();
            }
            catch (Exception ex)
            {
                sessionState.SetLastError(ex.Message);
                sessionState.ApplySystemNotice($"Start research failed: {ex.Message}");
                Debug.LogError($"[ClientBootstrap] Start research failed: {ex}");
            }
        }

        private void StartResearchByIndex(int index)
        {
            var techId = summaryState.GetAvailableTechIdAt(index + 1);
            if (string.IsNullOrWhiteSpace(techId))
            {
                return;
            }

            StartResearchById(techId, summaryState.GetAvailableTechLabelAt(index + 1));
        }

        private void StartResearchCardByIndex(int index)
        {
            var techId = summaryState.GetAvailableTechIdAt(index);
            if (string.IsNullOrWhiteSpace(techId))
            {
                return;
            }

            StartResearchById(techId, summaryState.GetAvailableTechLabelAt(index));
        }

        private void ConfigureDevelopmentResearchCard(
            VisualElement card,
            Label familyValue,
            Label titleValue,
            Label loreValue,
            Label noteValue,
            Button button,
            int techIndex)
        {
            if (card == null)
            {
                return;
            }

            var visible = activeChapter == ChapterView.Development
                && activeDevelopmentLane == DevelopmentLane.Research
                && !string.IsNullOrWhiteSpace(summaryState.GetAvailableTechIdAt(techIndex));

            SetElementDisplay(card, visible);
            if (!visible)
            {
                return;
            }

            if (familyValue != null) familyValue.text = summaryState.GetAvailableTechFamilyLabelAt(techIndex);
            if (titleValue != null) titleValue.text = summaryState.GetAvailableTechTitleAt(techIndex);
            if (loreValue != null) loreValue.text = summaryState.GetAvailableTechLoreAt(techIndex);
            if (noteValue != null) noteValue.text = summaryState.GetAvailableTechOperatorNoteAt(techIndex);

            if (button != null)
            {
                button.text = summaryState.GetAvailableTechActionLabelAt(techIndex);
                button.SetEnabled(sessionState.IsAuthenticated && !summaryState.HasActiveResearch());
            }
        }


        private void ConfigureDevelopmentInfoCard(
            VisualElement card,
            Label familyValue,
            Label titleValue,
            Label loreValue,
            Label noteValue,
            bool visible,
            string family,
            string title,
            string lore,
            string note)
        {
            if (card == null)
            {
                return;
            }

            SetElementDisplay(card, visible);
            if (!visible)
            {
                return;
            }

            if (familyValue != null) familyValue.text = family;
            if (titleValue != null) titleValue.text = title;
            if (loreValue != null) loreValue.text = lore;
            if (noteValue != null) noteValue.text = note;
        }

        private void RunDevelopmentPrimaryAction()
        {
            switch (activeDevelopmentLane)
            {
                case DevelopmentLane.Research:
                    StartSuggestedResearch();
                    break;
                case DevelopmentLane.Workshop:
                case DevelopmentLane.Growth:
                    RefreshSummary();
                    break;
            }
        }

        private bool CanRunDevelopmentPrimaryAction()
        {
            return activeDevelopmentLane == DevelopmentLane.Research
                ? !string.IsNullOrWhiteSpace(summaryState.GetSuggestedResearchId()) && sessionState.IsAuthenticated
                : true;
        }

        private void ConfigureDevelopmentQuickPickButton(Button button, int quickPickIndex)
        {
            if (button == null)
            {
                return;
            }

            var visible = activeChapter == ChapterView.Development
                && activeDevelopmentLane == DevelopmentLane.Research
                && sessionState.IsAuthenticated
                && !summaryState.HasActiveResearch()
                && !string.IsNullOrWhiteSpace(summaryState.GetAvailableTechIdAt(quickPickIndex + 1));

            button.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            button.SetEnabled(visible);
            if (visible)
            {
                button.text = summaryState.GetAvailableTechActionLabelAt(quickPickIndex + 1);
            }
        }

        private void RequestWhereAmI()
        {
            networkClient?.SendOp("whereami");
        }

        private void SendPing()
        {
            networkClient?.SendOp("ping", new JObject
            {
                ["sentAt"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });
        }

        private void SendChat()
        {
            var text = chatInputField?.value?.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            if (!sessionState.IsConnected)
            {
                sessionState.ApplySystemNotice("Chat send blocked: socket disconnected.");
                return;
            }

            if (string.Equals(sessionState.ActiveChatChannel, "system", StringComparison.OrdinalIgnoreCase))
            {
                sessionState.ApplySystemNotice("System is view-only. Switch to Room or All to send chat.");
                return;
            }

            if (!sessionState.HasJoinedChatRoom)
            {
                EnsureLobbyChatRoom();
                sessionState.ApplySystemNotice("Joining lobby chat first. Send again in a moment.");
                return;
            }

            networkClient?.SendOp("chat", new JObject { ["text"] = text });
            if (chatInputField != null)
            {
                chatInputField.value = string.Empty;
            }
        }

        private void OnConnected()
        {
            sessionState.SetConnectionState(true);
            sessionState.SetLastError("-");
            sessionState.ApplySystemNotice("Socket connected.");

            if (autoJoinLobbyOnConnect)
            {
                EnsureLobbyChatRoom();
            }

            if (autoRequestWhereAmIOnConnect)
            {
                RequestWhereAmI();
            }
        }

        private void OnDisconnected()
        {
            sessionState.SetConnectionState(false);
            sessionState.SetLastError(networkClient != null ? networkClient.LastError : "Disconnected.");
            sessionState.ApplySystemNotice("Socket disconnected.");
        }

        private void OnAnyMessage(JObject msg)
        {
            sessionState.SetLastInboundOp(msg?["op"]?.Value<string>() ?? "-");
        }

        private void OnWelcome(JObject payload)
        {
            sessionState.ApplyWelcome(
                payload?["sessionId"]?.Value<string>(),
                payload?["displayName"]?.Value<string>(),
                payload?["shardId"]?.Value<string>());
        }

        private void OnHelloAck(JObject payload)
        {
            sessionState.ApplyHelloAck(payload?["shardId"]?.Value<string>());
        }

        private void OnWhereAmIResult(JObject payload)
        {
            sessionState.ApplyWhereAmI(payload?["shardId"]?.Value<string>(), payload?["roomId"]?.Value<string>());
        }

        private void OnRoomJoined(JObject payload)
        {
            sessionState.ApplyRoomJoined(payload?["roomId"]?.Value<string>());
        }

        private void OnRoomLeft(JObject payload)
        {
            sessionState.ApplyRoomLeft();
        }

        private void OnWsError(JObject payload)
        {
            var code = payload?["code"]?.Value<string>();
            sessionState.ApplyWsError(code);
            if (string.Equals(code, "not_in_room", StringComparison.OrdinalIgnoreCase))
            {
                EnsureLobbyChatRoom();
            }
        }

        private void OnChat(JObject payload)
        {
            sessionState.ApplyChat(
                payload?["channelId"]?.Value<string>() ?? payload?["channel"]?.Value<string>(),
                payload?["channelLabel"]?.Value<string>(),
                payload?["from"]?.Value<string>(),
                payload?["text"]?.Value<string>());
        }

        private void EnsureLobbyChatRoom()
        {
            if (networkClient == null || !sessionState.IsConnected || sessionState.HasJoinedChatRoom)
            {
                return;
            }

            networkClient.SendOp("join_room", new JObject { ["roomId"] = "lobby" });
        }

        private void SwitchChapter(ChapterView chapter)
        {
            activeChapter = chapter;
            Render();
        }

        private void SwitchDevelopmentLane(DevelopmentLane lane)
        {
            activeDevelopmentLane = lane;
            Render();
        }

        private void ToggleTimerDiagnostics()
        {
            showTimerDiagnostics = !showTimerDiagnostics;
            Render();
        }

        private void Render()
        {
            if (connectionValue == null)
            {
                return;
            }

            connectionValue.text = sessionState.IsConnected ? "Connected" : "Disconnected";
            shardValue.text = sessionState.ShardId;
            roomValue.text = sessionState.RoomId;
            summaryStatusValue.text = sessionState.IsAuthenticated ? "Authenticated summary" : "Demo summary";
            statusHeadlineValue.text = summaryState.IsLoaded ? summaryState.GetStatusHeadline() : "Summary not loaded yet.";
            resourcesValue.text = summaryState.GetResourcesSummary();
            productionValue.text = summaryState.GetProductionSummary();
            researchValue.text = summaryState.GetResearchSummary();
            warningsValue.text = summaryState.GetWarningsSummary();
            readyOpsValue.text = summaryState.GetReadyOpsSummary();
            heroStatusValue.text = summaryState.GetHeroStatusSummary();
            armyStatusValue.text = summaryState.GetArmyStatusSummary();
            actionHintValue.text = summaryState.GetSuggestedResearchLabel();
            authStatusValue.text = sessionState.LoginStatus;
            accountValue.text = sessionState.IsAuthenticated ? sessionState.DisplayName : "Guest";
            if (authCardTitle != null)
            {
                authCardTitle.text = sessionState.IsAuthenticated ? "Account" : "Client login";
            }

            var lastError = summaryState.LastError != "-" ? summaryState.LastError : sessionState.LastError;
            lastUpdatedValue.text = summaryState.IsLoaded
                ? $"Updated {summaryState.LastUpdatedUtc:HH:mm:ss} UTC"
                : lastError != "-"
                    ? $"Last error: {lastError}"
                    : "No summary fetch yet.";

            if (researchButton != null)
            {
                researchButton.SetEnabled(!string.IsNullOrWhiteSpace(summaryState.GetSuggestedResearchId()) && sessionState.IsAuthenticated);
            }

            if (devStartResearchButton != null)
            {
                devStartResearchButton.SetEnabled(!string.IsNullOrWhiteSpace(summaryState.GetSuggestedResearchId()) && sessionState.IsAuthenticated);
            }

            if (devLanePrimaryButton != null)
            {
                devLanePrimaryButton.SetEnabled(CanRunDevelopmentPrimaryAction());
            }

            ConfigureDevelopmentQuickPickButton(devQuickTech1Button, 0);
            ConfigureDevelopmentQuickPickButton(devQuickTech2Button, 1);
            ConfigureDevelopmentResearchCard(devResearchCard1, devResearchCard1FamilyValue, devResearchCard1TitleValue, devResearchCard1LoreValue, devResearchCard1NoteValue, devResearchCard1Button, 0);
            ConfigureDevelopmentResearchCard(devResearchCard2, devResearchCard2FamilyValue, devResearchCard2TitleValue, devResearchCard2LoreValue, devResearchCard2NoteValue, devResearchCard2Button, 1);
            ConfigureDevelopmentResearchCard(devResearchCard3, devResearchCard3FamilyValue, devResearchCard3TitleValue, devResearchCard3LoreValue, devResearchCard3NoteValue, devResearchCard3Button, 2);
            ConfigureDevelopmentResearchCard(devResearchCard4, devResearchCard4FamilyValue, devResearchCard4TitleValue, devResearchCard4LoreValue, devResearchCard4NoteValue, devResearchCard4Button, 3);

            ConfigureDevelopmentInfoCard(
                devWorkshopCard1,
                devWorkshopCard1FamilyValue,
                devWorkshopCard1TitleValue,
                devWorkshopCard1LoreValue,
                devWorkshopCard1NoteValue,
                activeChapter == ChapterView.Development && activeDevelopmentLane == DevelopmentLane.Workshop && summaryState.GetVisibleWorkshopCardCount() > 0,
                summaryState.GetWorkshopCardFamilyLabelAt(0),
                summaryState.GetWorkshopCardTitleAt(0),
                summaryState.GetWorkshopCardLoreAt(0),
                summaryState.GetWorkshopCardNoteAt(0));
            ConfigureDevelopmentInfoCard(
                devWorkshopCard2,
                devWorkshopCard2FamilyValue,
                devWorkshopCard2TitleValue,
                devWorkshopCard2LoreValue,
                devWorkshopCard2NoteValue,
                activeChapter == ChapterView.Development && activeDevelopmentLane == DevelopmentLane.Workshop && summaryState.GetVisibleWorkshopCardCount() > 1,
                summaryState.GetWorkshopCardFamilyLabelAt(1),
                summaryState.GetWorkshopCardTitleAt(1),
                summaryState.GetWorkshopCardLoreAt(1),
                summaryState.GetWorkshopCardNoteAt(1));
            ConfigureDevelopmentInfoCard(
                devWorkshopCard3,
                devWorkshopCard3FamilyValue,
                devWorkshopCard3TitleValue,
                devWorkshopCard3LoreValue,
                devWorkshopCard3NoteValue,
                activeChapter == ChapterView.Development && activeDevelopmentLane == DevelopmentLane.Workshop && summaryState.GetVisibleWorkshopCardCount() > 2,
                summaryState.GetWorkshopCardFamilyLabelAt(2),
                summaryState.GetWorkshopCardTitleAt(2),
                summaryState.GetWorkshopCardLoreAt(2),
                summaryState.GetWorkshopCardNoteAt(2));
            ConfigureDevelopmentInfoCard(
                devWorkshopCard4,
                devWorkshopCard4FamilyValue,
                devWorkshopCard4TitleValue,
                devWorkshopCard4LoreValue,
                devWorkshopCard4NoteValue,
                activeChapter == ChapterView.Development && activeDevelopmentLane == DevelopmentLane.Workshop && summaryState.GetVisibleWorkshopCardCount() > 3,
                summaryState.GetWorkshopCardFamilyLabelAt(3),
                summaryState.GetWorkshopCardTitleAt(3),
                summaryState.GetWorkshopCardLoreAt(3),
                summaryState.GetWorkshopCardNoteAt(3));
            ConfigureDevelopmentInfoCard(
                devGrowthCard1,
                devGrowthCard1FamilyValue,
                devGrowthCard1TitleValue,
                devGrowthCard1LoreValue,
                devGrowthCard1NoteValue,
                activeChapter == ChapterView.Development && activeDevelopmentLane == DevelopmentLane.Growth && summaryState.GetVisibleGrowthCardCount() > 0,
                summaryState.GetGrowthCardFamilyLabelAt(0),
                summaryState.GetGrowthCardTitleAt(0),
                summaryState.GetGrowthCardLoreAt(0),
                summaryState.GetGrowthCardNoteAt(0));
            ConfigureDevelopmentInfoCard(
                devGrowthCard2,
                devGrowthCard2FamilyValue,
                devGrowthCard2TitleValue,
                devGrowthCard2LoreValue,
                devGrowthCard2NoteValue,
                activeChapter == ChapterView.Development && activeDevelopmentLane == DevelopmentLane.Growth && summaryState.GetVisibleGrowthCardCount() > 1,
                summaryState.GetGrowthCardFamilyLabelAt(1),
                summaryState.GetGrowthCardTitleAt(1),
                summaryState.GetGrowthCardLoreAt(1),
                summaryState.GetGrowthCardNoteAt(1));
            ConfigureDevelopmentInfoCard(
                devGrowthCard3,
                devGrowthCard3FamilyValue,
                devGrowthCard3TitleValue,
                devGrowthCard3LoreValue,
                devGrowthCard3NoteValue,
                activeChapter == ChapterView.Development && activeDevelopmentLane == DevelopmentLane.Growth && summaryState.GetVisibleGrowthCardCount() > 2,
                summaryState.GetGrowthCardFamilyLabelAt(2),
                summaryState.GetGrowthCardTitleAt(2),
                summaryState.GetGrowthCardLoreAt(2),
                summaryState.GetGrowthCardNoteAt(2));
            ConfigureDevelopmentInfoCard(
                devGrowthCard4,
                devGrowthCard4FamilyValue,
                devGrowthCard4TitleValue,
                devGrowthCard4LoreValue,
                devGrowthCard4NoteValue,
                activeChapter == ChapterView.Development && activeDevelopmentLane == DevelopmentLane.Growth && summaryState.GetVisibleGrowthCardCount() > 3,
                summaryState.GetGrowthCardFamilyLabelAt(3),
                summaryState.GetGrowthCardTitleAt(3),
                summaryState.GetGrowthCardLoreAt(3),
                summaryState.GetGrowthCardNoteAt(3));

            if (logoutButton != null)
            {
                logoutButton.SetEnabled(sessionState.IsAuthenticated);
            }

            if (authLoginFields != null)
            {
                authLoginFields.style.display = sessionState.IsAuthenticated ? DisplayStyle.None : DisplayStyle.Flex;
            }

            if (loginButtonRow != null)
            {
                loginButtonRow.style.display = sessionState.IsAuthenticated ? DisplayStyle.None : DisplayStyle.Flex;
            }

            if (authActionRow != null)
            {
                authActionRow.style.display = sessionState.IsAuthenticated ? DisplayStyle.Flex : DisplayStyle.None;
            }

            RenderChapterState();
            RenderDevelopmentDesk();
            RenderPlaceholder();
            RenderChatBand();
            RenderLiveSurfaces();
        }

        private void RenderChapterState()
        {
            SetElementDisplay(summaryScreen, activeChapter == ChapterView.Summary);
            SetElementDisplay(developmentScreen, activeChapter == ChapterView.Development);
            SetElementDisplay(placeholderScreen, activeChapter == ChapterView.Warfront || activeChapter == ChapterView.Social);
            SetElementDisplay(timerDiagnosticCard, showTimerDiagnostics && activeChapter == ChapterView.Summary);

            if (toggleTimerDiagnosticsButton != null)
            {
                toggleTimerDiagnosticsButton.text = showTimerDiagnostics ? "Hide timer debug" : "Show timer debug";
            }

            switch (activeChapter)
            {
                case ChapterView.Summary:
                    currentChapterMeta.text = "Live now";
                    currentChapterTitle.text = "Summary";
                    currentChapterKicker.text = "Command floor";
                    currentChapterCopy.text = "Use Home to scan the empire, watch live timers, and launch the next order before dropping into a deeper desk.";
                    break;
                case ChapterView.Development:
                    currentChapterMeta.text = "Desk";
                    currentChapterTitle.text = "Development";
                    currentChapterKicker.text = "Growth desk";
                    currentChapterCopy.text = "Research focus, workshop queue, and growth cadence stay grouped in one bounded command desk instead of hiding under the home floor.";
                    break;
                case ChapterView.Warfront:
                    currentChapterMeta.text = "Queued";
                    currentChapterTitle.text = "Warfront";
                    currentChapterKicker.text = "Field chapter";
                    currentChapterCopy.text = "Warfront is routed and named now, but still queued for a later slice so the shell can point to it without faking doctrine or TOMS controls.";
                    break;
                default:
                    currentChapterMeta.text = "Queued";
                    currentChapterTitle.text = "Social";
                    currentChapterKicker.text = "Shared comms lane";
                    currentChapterCopy.text = "Chat, room state, and filters remain anchored in the bottom comms band while the full social chapter waits its turn.";
                    break;
            }

            SetChapterButtonState(navHomeButton, activeChapter == ChapterView.Summary);
            SetChapterButtonState(navDevelopmentButton, activeChapter == ChapterView.Development);
            SetChapterButtonState(navWarfrontButton, activeChapter == ChapterView.Warfront);
            SetChapterButtonState(navSocialButton, activeChapter == ChapterView.Social);
        }

        private void RenderDevelopmentDesk()
        {
            if (developmentHeadlineValue != null)
            {
                developmentHeadlineValue.text = summaryState.GetDevelopmentDeskHeadline();
            }

            if (developmentCopyValue != null)
            {
                developmentCopyValue.text = summaryState.GetDevelopmentDeskSummary();
            }

            var researchFocus = summaryState.GetResearchFocusSummary();
            var nextUnlock = summaryState.GetResearchNextUnlockSummary();
            var commitPosture = summaryState.GetResearchCommitPostureSummary();
            var workshopQueue = summaryState.GetWorkshopTimerSummary();
            var activeMission = summaryState.GetMissionTimerSummary();
            var supportPosture = $"{summaryState.GetHeroStatusSummary()}  •  {summaryState.GetArmyStatusSummary()}";
            var growthCadence = $"{summaryState.GetProductionSummary()}  •  {summaryState.GetResourceTickSummary()}";
            var readyNow = summaryState.GetReadyOpsSummary();
            var deskNote = summaryState.GetDevelopmentDeskNote();

            if (devResearchFocusValue != null) devResearchFocusValue.text = researchFocus;
            if (devNextTechValue != null) devNextTechValue.text = nextUnlock;
            if (devWorkshopValue != null) devWorkshopValue.text = workshopQueue;
            if (devGrowthValue != null) devGrowthValue.text = growthCadence;
            if (devSupportValue != null) devSupportValue.text = supportPosture;
            if (devNoteValue != null) devNoteValue.text = deskNote;

            if (devActionsTitleValue != null)
            {
                devActionsTitleValue.text = activeDevelopmentLane == DevelopmentLane.Research ? "Research actions" : activeDevelopmentLane == DevelopmentLane.Workshop ? "Workshop actions" : "Growth actions";
            }

            if (devActionsCopyValue != null)
            {
                devActionsCopyValue.text = activeDevelopmentLane == DevelopmentLane.Research
                    ? "Commit the suggested unlock from the desk or use the visible research cards below to read family, lore, and operator notes before choosing."
                    : activeDevelopmentLane == DevelopmentLane.Workshop
                        ? "No workshop mutation endpoint is live yet. Refresh this lane to recheck queue state and mission pressure."
                        : "Growth controls are still surfaced through the summary/home floor. Use this lane to refresh cadence and tick state before acting elsewhere.";
            }

            if (devResearchCardsCopyValue != null)
            {
                devResearchCardsCopyValue.text = summaryState.GetVisibleResearchCardsSummary();
            }

            if (devWorkshopCardsCopyValue != null)
            {
                devWorkshopCardsCopyValue.text = summaryState.GetVisibleWorkshopCardsSummary();
            }

            if (devGrowthCardsCopyValue != null)
            {
                devGrowthCardsCopyValue.text = summaryState.GetVisibleGrowthCardsSummary();
            }

            SetElementDisplay(devResearchCardsSection, activeChapter == ChapterView.Development && activeDevelopmentLane == DevelopmentLane.Research && summaryState.GetAvailableTechCount() > 0);
            SetElementDisplay(devWorkshopCardsSection, activeChapter == ChapterView.Development && activeDevelopmentLane == DevelopmentLane.Workshop && summaryState.GetVisibleWorkshopCardCount() > 0);
            SetElementDisplay(devGrowthCardsSection, activeChapter == ChapterView.Development && activeDevelopmentLane == DevelopmentLane.Growth && summaryState.GetVisibleGrowthCardCount() > 0);

            if (devLanePrimaryButton != null)
            {
                devLanePrimaryButton.text = activeDevelopmentLane == DevelopmentLane.Research ? "Start suggested research" : activeDevelopmentLane == DevelopmentLane.Workshop ? "Refresh workshop state" : "Refresh growth state";
            }

            switch (activeDevelopmentLane)
            {
                case DevelopmentLane.Research:
                    devLaneTitleValue.text = "Research lane";
                    devLaneCopyValue.text = summaryState.GetDevelopmentResearchLaneCopy();
                    devLaneCard1Title.text = "Research focus";
                    devLaneCard1Value.text = researchFocus;
                    devLaneCard2Title.text = "Visible next path";
                    devLaneCard2Value.text = nextUnlock;
                    devLaneCard3Title.text = "Commit posture";
                    devLaneCard3Value.text = commitPosture;
                    break;
                case DevelopmentLane.Workshop:
                    devLaneTitleValue.text = "Workshop lane";
                    devLaneCopyValue.text = summaryState.GetDevelopmentWorkshopLaneCopy();
                    devLaneCard1Title.text = "Workshop queue";
                    devLaneCard1Value.text = workshopQueue;
                    devLaneCard2Title.text = "Active mission";
                    devLaneCard2Value.text = activeMission;
                    devLaneCard3Title.text = "Support posture";
                    devLaneCard3Value.text = supportPosture;
                    break;
                default:
                    devLaneTitleValue.text = "Growth lane";
                    devLaneCopyValue.text = summaryState.GetDevelopmentGrowthLaneCopy();
                    devLaneCard1Title.text = "Growth cadence";
                    devLaneCard1Value.text = growthCadence;
                    devLaneCard2Title.text = "Next resource tick";
                    devLaneCard2Value.text = summaryState.GetResourceTickSummary();
                    devLaneCard3Title.text = "Ready now";
                    devLaneCard3Value.text = readyNow;
                    break;
            }

            SetLaneButtonState(devResearchLaneButton, activeDevelopmentLane == DevelopmentLane.Research);
            SetLaneButtonState(devWorkshopLaneButton, activeDevelopmentLane == DevelopmentLane.Workshop);
            SetLaneButtonState(devGrowthLaneButton, activeDevelopmentLane == DevelopmentLane.Growth);
        }

        private void RenderPlaceholder()
        {
            if (activeChapter == ChapterView.Warfront)
            {
                placeholderHeadlineValue.text = "Warfront desk queued";
                placeholderCopyValue.text = "Warfront will become a real chapter later. For now the shell can land here without faking doctrine, field review, or TOMS controls.";
                placeholderNoteValue.text = "Keep using Summary and Development while Warfront remains a planned chapter.";
            }
            else if (activeChapter == ChapterView.Social)
            {
                placeholderHeadlineValue.text = "Social chapter queued";
                placeholderCopyValue.text = "The bottom comms band is the real social surface right now. A larger social chapter can come later without flattening the shell again.";
                placeholderNoteValue.text = "Chat, room state, and filters stay live in the comms band below.";
            }
        }

        private void RenderLiveSurfaces()
        {
            if (liveClockValue != null)
            {
                liveClockValue.text = $"Now {DateTime.UtcNow:HH:mm:ss} UTC";
            }

            RenderLiveTimerLabels();
            RenderTimerDiagnostics();
            if (activeChapter == ChapterView.Development && activeDevelopmentLane == DevelopmentLane.Growth && devLaneCard2Value != null)
            {
                devLaneCard2Value.text = summaryState.GetResourceTickSummary();
                if (devGrowthValue != null)
                {
                    devGrowthValue.text = $"{summaryState.GetProductionSummary()}  •  {summaryState.GetResourceTickSummary()}";
                }
            }
        }

        private void RenderLiveTimerLabels()
        {
            if (!summaryState.IsLoaded)
            {
                if (researchTimerValue != null) researchTimerValue.text = "No active research timer.";
                if (workshopTimerValue != null) workshopTimerValue.text = "No active workshop queue.";
                if (missionTimerValue != null) missionTimerValue.text = "No active mission clock.";
                if (resourceTickValue != null) resourceTickValue.text = "Tick timing unavailable.";
                return;
            }

            if (researchTimerValue != null) researchTimerValue.text = summaryState.GetResearchTimerSummary();
            if (workshopTimerValue != null) workshopTimerValue.text = summaryState.GetWorkshopTimerSummary();
            if (missionTimerValue != null) missionTimerValue.text = summaryState.GetMissionTimerSummary();
            if (resourceTickValue != null) resourceTickValue.text = summaryState.GetResourceTickSummary();
        }

        private void RenderTimerDiagnostics()
        {
            if (timerDiagNowValue == null || timerDiagHeartbeatValue == null || timerDiagRawValue == null || timerDiagComputedValue == null)
            {
                return;
            }

            var nowUtc = DateTime.UtcNow;
            timerDiagNowValue.text = $"Live UI clock {nowUtc:HH:mm:ss} UTC";
            timerDiagHeartbeatValue.text = $"Heartbeat #{heartbeatCounter}";
            timerDiagRawValue.text = summaryState.GetResourceTickRawSummary();
            timerDiagComputedValue.text = summaryState.GetResourceTickDiagnosticSummary(nowUtc);
        }

        private void RenderChatBand()
        {
            if (chatLogScroll != null)
            {
                chatLogScroll.Clear();
                var lines = sessionState.GetVisibleChatLines();
                if (lines.Count == 0)
                {
                    chatLogScroll.Add(new Label("No chat yet.") { name = "chat-empty-line" });
                }
                else
                {
                    foreach (var line in lines)
                    {
                        var row = new VisualElement();
                        row.AddToClassList("chat-line");

                        var channel = new Label(line.ChannelLabel.ToUpperInvariant());
                        channel.AddToClassList("chat-line-channel");
                        row.Add(channel);

                        var text = new Label(line.ToDisplayText());
                        text.AddToClassList("chat-line-text");
                        row.Add(text);

                        chatLogScroll.Add(row);
                    }
                }
            }

            if (commsStatusValue != null)
            {
                commsStatusValue.text = sessionState.LastChatLine;
            }

            if (commsHintValue != null)
            {
                commsHintValue.text = string.Equals(sessionState.ActiveChatChannel, "system", StringComparison.OrdinalIgnoreCase)
                    ? "System is view-only. Switch to Room or All to send a chat line."
                    : "Room chat is live. The band now shows recent lines, filters, and a simple send box.";
            }

            if (sendChatButton != null)
            {
                sendChatButton.SetEnabled(sessionState.IsConnected && !string.Equals(sessionState.ActiveChatChannel, "system", StringComparison.OrdinalIgnoreCase));
            }

            SetChipState(chatAllButton, string.Equals(sessionState.ActiveChatChannel, "all", StringComparison.OrdinalIgnoreCase));
            SetChipState(chatRoomButton, string.Equals(sessionState.ActiveChatChannel, "room", StringComparison.OrdinalIgnoreCase));
            SetChipState(chatSystemButton, string.Equals(sessionState.ActiveChatChannel, "system", StringComparison.OrdinalIgnoreCase));
        }

        private static void SetElementDisplay(VisualElement element, bool visible)
        {
            if (element != null)
            {
                element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private static void SetChapterButtonState(Button button, bool active)
        {
            if (button == null) return;
            if (active) button.AddToClassList("rail-button--active"); else button.RemoveFromClassList("rail-button--active");
        }

        private static void SetLaneButtonState(Button button, bool active)
        {
            SetChipState(button, active);
        }

        private static void SetChipState(VisualElement element, bool active)
        {
            if (element == null) return;
            if (active) element.AddToClassList("action-button--primary"); else element.RemoveFromClassList("action-button--primary");
        }
    }
}
