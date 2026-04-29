using PlanarWar.Client.Core;
using PlanarWar.Client.Core.Application;
using PlanarWar.Client.UI.Screens.BlackMarket;
using PlanarWar.Client.UI.Screens.City;
using PlanarWar.Client.UI.Screens.Heroes;
using PlanarWar.Client.UI.Screens.Summary;
using System;
using System.Linq;
using UnityEngine.UIElements;

namespace PlanarWar.Client.UI
{
    public sealed class AppShellController
    {
        private readonly SessionState sessionState;
        private readonly SummaryState summaryState;
        private readonly ShellNavigationState navigationState;
        private readonly ClientVersionState versionState;

        private readonly SummaryScreenController summaryScreen;
        private readonly CityScreenController cityScreen;
        private readonly BlackMarketScreenController blackMarketScreen;
        private readonly HeroScreenController heroScreen;
        private readonly SocialScreenController socialScreen;

        private readonly VisualElement summaryRoot;
        private readonly VisualElement cityRoot;
        private readonly VisualElement blackMarketRoot;
        private readonly VisualElement heroesRoot;
        private readonly VisualElement socialRoot;
        private readonly VisualElement authRoot;

        private readonly Label connectionValue;
        private readonly Label shardValue;
        private readonly Label roomValue;
        private readonly Label summaryStatusValue;
        private readonly Label authStatusValue;
        private readonly Label accountValue;
        private readonly Label actionHintValue;
        private readonly Label lastUpdatedValue;
        private readonly Label liveClockValue;
        private readonly Label clientVersionValue;
        private readonly Label clientChannelValue;
        private readonly Label clientUpdateValue;
        private readonly Label clientAuthorityValue;

        private readonly VisualElement authLoginFields;
        private readonly VisualElement authRegisterFields;
        private readonly VisualElement loginButtonRow;
        private readonly VisualElement registerButtonRow;
        private readonly VisualElement authActionRow;
        private readonly Button loginButton;
        private readonly Button registerButton;
        private readonly Button logoutButton;

        private readonly Label currentChapterTitle;
        private readonly Label currentChapterKicker;
        private readonly Label currentChapterCopy;
        private readonly Label currentChapterMeta;
        private readonly Button navHomeButton;
        private readonly Button navDevelopmentButton;
        private readonly Button navWarfrontButton;
        private readonly Button navHeroesButton;
        private readonly Label navHeroesKicker;
        private readonly Label navHeroesBadge;
        private readonly Label navHeroesTitle;
        private readonly Label navHeroesCopy;
        private readonly Button navSocialButton;

        private readonly Label commsStatusValue;
        private readonly Label commsHintValue;
        private readonly ScrollView chatLogScroll;
        private readonly Button chatAllButton;
        private readonly Button chatRoomButton;
        private readonly Button chatSystemButton;
        private readonly Button sendChatButton;
        private readonly TextField chatInputField;

        public AppShellController(VisualElement root, SessionState sessionState, SummaryState summaryState, ShellNavigationState navigationState, ClientVersionState versionState, Func<string, System.Threading.Tasks.Task> onStartResearchRequested, Func<string, System.Threading.Tasks.Task> onStartWorkshopCraftRequested, Func<string, System.Threading.Tasks.Task> onCollectWorkshopRequested, Func<string, System.Threading.Tasks.Task> onRecruitHeroRequested, Func<string, System.Threading.Tasks.Task> onAcceptHeroRecruitCandidateRequested, Func<System.Threading.Tasks.Task> onDismissHeroRecruitCandidatesRequested, Func<string, System.Threading.Tasks.Task> onConstructBuildingRequested, Func<string, System.Threading.Tasks.Task> onUpgradeBuildingRequested, Func<string, string, System.Threading.Tasks.Task> onSwitchBuildingRoutingRequested, Func<string, System.Threading.Tasks.Task> onDestroyBuildingRequested, Func<string, string, System.Threading.Tasks.Task> onRemodelBuildingRequested, Func<string, System.Threading.Tasks.Task> onCancelActiveBuildRequested, Func<string, System.Threading.Tasks.Task> onReinforceArmyRequested, Func<string, string, System.Threading.Tasks.Task> onRenameArmyRequested, Func<string, int, string, System.Threading.Tasks.Task> onSplitArmyRequested, Func<string, string, System.Threading.Tasks.Task> onMergeArmyRequested, Func<string, System.Threading.Tasks.Task> onDisbandArmyRequested, Func<string, string, string, System.Threading.Tasks.Task> onAssignArmyHoldRequested, Func<string, System.Threading.Tasks.Task> onReleaseArmyHoldRequested, Func<string, string, string, System.Threading.Tasks.Task> onWarfrontAssaultRequested, Func<string, string, string, System.Threading.Tasks.Task> onGarrisonStrikeRequested, Func<string, string, string, string, System.Threading.Tasks.Task> onStartMissionRequested, Func<string, System.Threading.Tasks.Task> onCompleteMissionRequested, Func<string, System.Threading.Tasks.Task> onReleaseHeroRequested, Func<string, int, System.Threading.Tasks.Task> onEquipHeroFromArmoryRequested, Func<string, string, System.Threading.Tasks.Task> onUnequipHeroToArmoryRequested, Func<string, string, System.Threading.Tasks.Task> onBootstrapCityRequested, Action onRefreshDeskRequested, Action onBackHomeRequested)
        {
            this.sessionState = sessionState;
            this.summaryState = summaryState;
            this.navigationState = navigationState;
            this.versionState = versionState;

            summaryRoot = root.Q<VisualElement>("summary-screen");
            cityRoot = root.Q<VisualElement>("development-screen");
            blackMarketRoot = root.Q<VisualElement>("placeholder-screen");
            heroesRoot = root.Q<VisualElement>("heroes-screen");
            socialRoot = root.Q<VisualElement>("social-screen");
            authRoot = root.Q<VisualElement>("auth-screen");

            summaryScreen = new SummaryScreenController(root, onBootstrapCityRequested, screen => navigationState.SetActive(screen));
            cityScreen = new CityScreenController(root, summaryState, onStartResearchRequested, onStartWorkshopCraftRequested, onCollectWorkshopRequested, onRecruitHeroRequested, onAcceptHeroRecruitCandidateRequested, onDismissHeroRecruitCandidatesRequested, onConstructBuildingRequested, onUpgradeBuildingRequested, onSwitchBuildingRoutingRequested, onDestroyBuildingRequested, onRemodelBuildingRequested, onCancelActiveBuildRequested, onRefreshDeskRequested, onBackHomeRequested);
            blackMarketScreen = new BlackMarketScreenController(root, summaryState, onReinforceArmyRequested, onRenameArmyRequested, onSplitArmyRequested, onMergeArmyRequested, onDisbandArmyRequested, onAssignArmyHoldRequested, onReleaseArmyHoldRequested, onWarfrontAssaultRequested, onGarrisonStrikeRequested, onStartMissionRequested, onCompleteMissionRequested, onRefreshDeskRequested);
            heroScreen = new HeroScreenController(root, summaryState, onRecruitHeroRequested, onAcceptHeroRecruitCandidateRequested, onDismissHeroRecruitCandidatesRequested, onReleaseHeroRequested, onEquipHeroFromArmoryRequested, onUnequipHeroToArmoryRequested, onRefreshDeskRequested);
            socialScreen = new SocialScreenController(root);

            connectionValue = root.Q<Label>("connection-value");
            shardValue = root.Q<Label>("shard-value");
            roomValue = root.Q<Label>("room-value");
            summaryStatusValue = root.Q<Label>("summary-status-value");
            authStatusValue = root.Q<Label>("auth-status-value");
            accountValue = root.Q<Label>("account-value");
            actionHintValue = root.Q<Label>("action-hint-value");
            lastUpdatedValue = root.Q<Label>("last-updated-value");
            liveClockValue = root.Q<Label>("live-clock-value");
            clientVersionValue = root.Q<Label>("client-version-value");
            clientChannelValue = root.Q<Label>("client-channel-value");
            clientUpdateValue = root.Q<Label>("client-update-value");
            clientAuthorityValue = root.Q<Label>("client-authority-value");

            authLoginFields = root.Q<VisualElement>("auth-login-fields");
            authRegisterFields = root.Q<VisualElement>("auth-register-fields");
            loginButtonRow = root.Q<VisualElement>("login-button-row");
            registerButtonRow = root.Q<VisualElement>("register-button-row");
            authActionRow = root.Q<VisualElement>("auth-action-row");
            loginButton = root.Q<Button>("login-button");
            registerButton = root.Q<Button>("register-button");
            logoutButton = root.Q<Button>("logout-button");

            currentChapterTitle = root.Q<Label>("current-chapter-title");
            currentChapterKicker = root.Q<Label>("current-chapter-kicker");
            currentChapterCopy = root.Q<Label>("current-chapter-copy");
            currentChapterMeta = root.Q<Label>("current-chapter-meta");
            navHomeButton = root.Q<Button>("nav-home-button");
            navDevelopmentButton = root.Q<Button>("nav-development-button");
            navWarfrontButton = root.Q<Button>("nav-warfront-button");
            navHeroesButton = root.Q<Button>("nav-heroes-button");
            navHeroesKicker = root.Q<Label>("nav-heroes-kicker");
            navHeroesBadge = root.Q<Label>("nav-heroes-badge");
            navHeroesTitle = root.Q<Label>("nav-heroes-title");
            navHeroesCopy = root.Q<Label>("nav-heroes-copy");
            navSocialButton = root.Q<Button>("nav-social-button");

            commsStatusValue = root.Q<Label>("comms-status-value");
            commsHintValue = root.Q<Label>("comms-hint-value");
            chatLogScroll = root.Q<ScrollView>("chat-log-scroll");
            chatAllButton = root.Q<Button>("chat-all-button");
            chatRoomButton = root.Q<Button>("chat-room-button");
            chatSystemButton = root.Q<Button>("chat-system-button");
            sendChatButton = root.Q<Button>("send-chat-button");
            chatInputField = root.Q<TextField>("chat-input-field");
        }

        public void Render()
        {
            var isAuthenticated = sessionState.IsAuthenticated;
            connectionValue.text = sessionState.IsConnected ? "Connected" : "Disconnected";
            shardValue.text = string.IsNullOrWhiteSpace(sessionState.ShardId) || sessionState.ShardId == "-" ? "—" : sessionState.ShardId;
            roomValue.text = string.IsNullOrWhiteSpace(sessionState.RoomId) || sessionState.RoomId == "-" ? "—" : sessionState.RoomId;
            summaryStatusValue.text = summaryState.IsLoaded
                ? "Summary loaded"
                : isAuthenticated
                    ? (string.IsNullOrWhiteSpace(summaryState.LastError) || summaryState.LastError == "-"
                        ? "Summary pending"
                        : summaryState.LastError)
                    : "Sign in or create an account";
            var loginStatus = sessionState.LoginStatus;
            authStatusValue.text = isAuthenticated
                ? loginStatus
                : string.Equals(loginStatus, "Demo mode active.", StringComparison.OrdinalIgnoreCase)
                    ? "Sign in or create an account below."
                    : loginStatus;
            accountValue.text = isAuthenticated ? sessionState.DisplayName : "Guest";
            var actionStatus = summaryState.ActionStatus;
            actionHintValue.text = isAuthenticated
                ? (!string.IsNullOrWhiteSpace(actionStatus)
                    ? actionStatus
                    : (summaryState.Snapshot.HasCity ? BuildPostFounderActionHint(summaryState.Snapshot) : "Founder mode: choose City or Black Market from live setup truth."))
                : "Sign in or register first, then the setup screen opens here.";
            lastUpdatedValue.text = summaryState.IsLoaded ? $"Updated {summaryState.LastUpdatedUtc:HH:mm:ss} UTC" : "No summary fetch yet.";
            liveClockValue.text = $"Now {DateTime.UtcNow:HH:mm:ss} UTC";
            clientVersionValue.text = versionState?.BuildLabel ?? "v0.0.0-local";
            clientChannelValue.text = versionState?.ChannelLabel ?? "unknown channel";
            clientUpdateValue.text = versionState?.UpdateStatus ?? "Update status unavailable.";
            clientAuthorityValue.text = versionState?.AuthorityHint ?? "Patch authority unknown.";

            if (authLoginFields != null) authLoginFields.style.display = isAuthenticated ? DisplayStyle.None : DisplayStyle.Flex;
            if (authRegisterFields != null) authRegisterFields.style.display = isAuthenticated ? DisplayStyle.None : DisplayStyle.Flex;
            if (loginButtonRow != null) loginButtonRow.style.display = isAuthenticated ? DisplayStyle.None : DisplayStyle.Flex;
            if (registerButtonRow != null) registerButtonRow.style.display = isAuthenticated ? DisplayStyle.None : DisplayStyle.Flex;
            if (authActionRow != null) authActionRow.style.display = isAuthenticated ? DisplayStyle.Flex : DisplayStyle.None;
            loginButton?.SetEnabled(!isAuthenticated);
            registerButton?.SetEnabled(!isAuthenticated);
            logoutButton?.SetEnabled(isAuthenticated);
            navHomeButton?.SetEnabled(isAuthenticated);
            navDevelopmentButton?.SetEnabled(isAuthenticated);
            navWarfrontButton?.SetEnabled(isAuthenticated);
            navHeroesButton?.SetEnabled(isAuthenticated);
            navSocialButton?.SetEnabled(isAuthenticated);

            summaryScreen.Render(summaryState.Snapshot, summaryState.IsLoaded, summaryState.IsActionBusy, summaryState.ActionStatus, summaryState.ActionFailed);
            cityScreen.Render(summaryState.Snapshot, summaryState);
            blackMarketScreen.Render(summaryState.Snapshot);
            heroScreen.Render(summaryState.Snapshot);
            socialScreen.Render(sessionState);

            if (authRoot != null) authRoot.style.display = isAuthenticated ? DisplayStyle.None : DisplayStyle.Flex;
            if (summaryRoot != null) summaryRoot.style.display = isAuthenticated && navigationState.ActiveScreen == ShellScreen.Summary ? DisplayStyle.Flex : DisplayStyle.None;
            if (cityRoot != null) cityRoot.style.display = isAuthenticated && navigationState.ActiveScreen == ShellScreen.City ? DisplayStyle.Flex : DisplayStyle.None;
            if (blackMarketRoot != null) blackMarketRoot.style.display = isAuthenticated && navigationState.ActiveScreen == ShellScreen.BlackMarket ? DisplayStyle.Flex : DisplayStyle.None;
            if (heroesRoot != null) heroesRoot.style.display = isAuthenticated && navigationState.ActiveScreen == ShellScreen.Heroes ? DisplayStyle.Flex : DisplayStyle.None;
            if (socialRoot != null) socialRoot.style.display = isAuthenticated && navigationState.ActiveScreen == ShellScreen.Social ? DisplayStyle.Flex : DisplayStyle.None;

            RenderChapterState();
            RenderCommsPanel();
        }

        private static string BuildPostFounderActionHint(PlanarWar.Client.Core.Contracts.ShellSummarySnapshot snapshot)
        {
            var isBlackMarket = string.Equals(snapshot?.City?.SettlementLane, "black_market", StringComparison.OrdinalIgnoreCase)
                || string.Equals(snapshot?.City?.SettlementLaneLabel, "Black Market", StringComparison.OrdinalIgnoreCase);

            return isBlackMarket
                ? "Use Development for fronts and shadow-book research, Operations for routes and missions, and Operatives for contacts and gear."
                : "Use Development for buildings and research, Operations for missions and formations, and Heroes for recruitment and gear.";
        }

        private void RenderChapterState()
        {
            var heroLane = ResolveHeroLaneText();
            var (title, kicker, copy) = navigationState.ActiveScreen switch
            {
                ShellScreen.Summary => ("Summary", "Command floor", "This rail stays menu-owned. Use Home to scan the empire, then jump into a desk when something needs action."),
                ShellScreen.City => ("Development", "Growth desk", "Research, workshop, and growth cadence stay grouped here as a read-only planning desk."),
                ShellScreen.BlackMarket => ("Operations", "Operations doctrine", "Routes, readiness, holds, and covert deployment stay visible here before deeper operations wiring lands."),
                ShellScreen.Heroes => (heroLane.Title, heroLane.Kicker, heroLane.ChapterCopy),
                ShellScreen.Social => ("Social", "Shared comms", "Room state, recent lines, and channel posture stay honest here without inventing a full social stack."),
                _ => ("Summary", "Command floor", "This rail stays menu-owned.")
            };

            if (navHeroesKicker != null) navHeroesKicker.text = heroLane.KickerShort;
            if (navHeroesBadge != null) navHeroesBadge.text = heroLane.Badge;
            if (navHeroesTitle != null) navHeroesTitle.text = heroLane.Title;
            if (navHeroesCopy != null) navHeroesCopy.text = heroLane.RailCopy;

            if (currentChapterTitle != null) currentChapterTitle.text = title;
            if (currentChapterKicker != null) currentChapterKicker.text = kicker;
            if (currentChapterCopy != null) currentChapterCopy.text = copy;
            if (currentChapterMeta != null) currentChapterMeta.text = navigationState.ActiveScreen == ShellScreen.Summary ? "Live now" : "Desk open";

            SetNavActive(navHomeButton, navigationState.ActiveScreen == ShellScreen.Summary);
            SetNavActive(navDevelopmentButton, navigationState.ActiveScreen == ShellScreen.City);
            SetNavActive(navWarfrontButton, navigationState.ActiveScreen == ShellScreen.BlackMarket);
            SetNavActive(navHeroesButton, navigationState.ActiveScreen == ShellScreen.Heroes);
            SetNavActive(navSocialButton, navigationState.ActiveScreen == ShellScreen.Social);
        }

        private void RenderCommsPanel()
        {
            if (commsStatusValue != null)
            {
                commsStatusValue.text = sessionState.ChatLines.Count > 0 ? sessionState.LastChatLine : "No chat yet.";
            }

            var canSendRoomChat = sessionState.IsConnected && sessionState.HasJoinedChatRoom;
            if (commsHintValue != null)
            {
                var roomText = sessionState.HasJoinedChatRoom ? $"room {sessionState.ChatRoomId}" : "no room attached";
                commsHintValue.text = canSendRoomChat
                    ? $"Room comms are live. Filter {sessionState.ActiveChatChannel.ToUpperInvariant()} • {roomText} • send box routes through websocket room chat."
                    : $"Comms band is connected to live traffic, but outbound room chat waits for a room attachment. Filter {sessionState.ActiveChatChannel.ToUpperInvariant()} • {roomText}.";
            }

            sendChatButton?.SetEnabled(canSendRoomChat);
            chatInputField?.SetEnabled(canSendRoomChat);

            SetFilterActive(chatAllButton, string.Equals(sessionState.ActiveChatChannel, "all", StringComparison.OrdinalIgnoreCase));
            SetFilterActive(chatRoomButton, string.Equals(sessionState.ActiveChatChannel, "room", StringComparison.OrdinalIgnoreCase));
            SetFilterActive(chatSystemButton, string.Equals(sessionState.ActiveChatChannel, "system", StringComparison.OrdinalIgnoreCase));

            if (chatLogScroll == null)
            {
                return;
            }

            chatLogScroll.contentContainer.Clear();
            var lines = sessionState.GetVisibleChatLines();
            if (lines.Count == 0)
            {
                chatLogScroll.contentContainer.Add(new Label("No chat lines visible for this filter yet.") { name = "chat-empty-line" });
                return;
            }

            foreach (var line in lines)
            {
                var row = new VisualElement();
                row.AddToClassList("chat-line");
                var channel = new Label(line.ChannelLabel.ToUpperInvariant());
                channel.AddToClassList("chat-line-channel");
                var text = new Label(line.ToDisplayText());
                text.AddToClassList("chat-line-text");
                row.Add(channel);
                row.Add(text);
                chatLogScroll.contentContainer.Add(row);
            }
        }

        private HeroLaneText ResolveHeroLaneText()
        {
            var isBlackMarket = string.Equals(summaryState?.Snapshot?.City?.SettlementLane, "black_market", StringComparison.OrdinalIgnoreCase)
                || string.Equals(summaryState?.Snapshot?.City?.SettlementLaneLabel, "Black Market", StringComparison.OrdinalIgnoreCase);

            return isBlackMarket
                ? new HeroLaneText(
                    "Operatives",
                    "Roster desk",
                    "Operatives",
                    "Roster",
                    "Scout, review, and retire named operatives here.",
                    "Scouting, contact review, retirement safety, and operative availability stay visible here without pretending Black Market assets are civic heroes.")
                : new HeroLaneText(
                    "Heroes",
                    "Roster desk",
                    "Heroes",
                    "Roster",
                    "Recruit, review, and dismiss heroes here.",
                    "Recruitment, candidate review, release safety, and hero availability stay visible here without hiding inside Development.");
        }

        private readonly struct HeroLaneText
        {
            public HeroLaneText(string title, string kicker, string badge, string kickerShort, string railCopy, string chapterCopy)
            {
                Title = title;
                Kicker = kicker;
                Badge = badge;
                KickerShort = kickerShort;
                RailCopy = railCopy;
                ChapterCopy = chapterCopy;
            }

            public string Title { get; }
            public string Kicker { get; }
            public string Badge { get; }
            public string KickerShort { get; }
            public string RailCopy { get; }
            public string ChapterCopy { get; }
        }

        private static void SetNavActive(Button button, bool active)
        {
            if (button == null) return;
            if (active) button.AddToClassList("rail-button--active");
            else button.RemoveFromClassList("rail-button--active");
        }

        private static void SetFilterActive(Button button, bool active)
        {
            if (button == null) return;
            if (active) button.AddToClassList("action-button--primary");
            else button.RemoveFromClassList("action-button--primary");
        }

        private sealed class SocialScreenController
        {
            private readonly Label headline;
            private readonly Label copy;
            private readonly Label overview;
            private readonly Label roomValue;
            private readonly Label channelValue;
            private readonly Label trafficValue;
            private readonly Label connectionValue;
            private readonly Label systemValue;
            private readonly Label noteValue;
            private readonly InfoCard[] cards;

            public SocialScreenController(VisualElement root)
            {
                headline = root.Q<Label>("social-headline-value");
                copy = root.Q<Label>("social-copy-value");
                overview = root.Q<Label>("social-overview-value");
                roomValue = root.Q<Label>("social-room-value");
                channelValue = root.Q<Label>("social-channel-value");
                trafficValue = root.Q<Label>("social-traffic-value");
                connectionValue = root.Q<Label>("social-connection-value");
                systemValue = root.Q<Label>("social-system-value");
                noteValue = root.Q<Label>("social-note-value");
                cards = Enumerable.Range(1, 4).Select(i => new InfoCard(root, $"social-card-{i}")).ToArray();
            }

            public void Render(SessionState sessionState)
            {
                var visibleLines = sessionState.GetVisibleChatLines();
                var allLines = sessionState.ChatLines;
                var systemLine = allLines.LastOrDefault(l => string.Equals(l.ChannelId, "system", StringComparison.OrdinalIgnoreCase));
                var roomJoined = sessionState.HasJoinedChatRoom;
                var roomText = roomJoined ? sessionState.ChatRoomId : "(unattached)";

                headline.text = roomJoined || visibleLines.Count > 0 ? "Comms desk" : "Comms review";
                copy.text = roomJoined
                    ? $"Room {roomText} is live. Review filter, traffic, and recent lines here."
                    : "No room is attached yet. System chatter and filter posture stay visible.";
                overview.text = $"Shard {sessionState.ShardId} • room {roomText} • filter {sessionState.ActiveChatChannel.ToUpperInvariant()} • {visibleLines.Count}/{allLines.Count} visible";

                roomValue.text = roomJoined ? $"Room {roomText} • attached" : "No room attached yet.";
                channelValue.text = $"{sessionState.ActiveChatChannel.ToUpperInvariant()} filter";
                trafficValue.text = visibleLines.Count > 0 ? $"{visibleLines.Count}/{allLines.Count} visible line(s)" : "No visible lines for this filter.";
                connectionValue.text = sessionState.IsConnected ? $"Connected • {sessionState.LastInboundOp}" : "Disconnected";
                systemValue.text = systemLine != null ? systemLine.ToDisplayText() : "No system notice yet.";
                noteValue.text = roomJoined ? "Room chat is live; friend roster, DMs, and moderation surfaces remain deferred." : "Live comms posture stays honest while broader social systems remain deferred." ;

                var cardViews = new[]
                {
                    new CardView("Room", roomJoined ? roomText : "Unattached", roomJoined ? "Room chat is attached through WS session state." : "Use Home / where-am-I to attach when available.", $"Shard {sessionState.ShardId} • {sessionState.DisplayName}"),
                    new CardView("Filter", sessionState.ActiveChatChannel.ToUpperInvariant(), visibleLines.Count > 0 ? $"Showing {visibleLines.Count} of {allLines.Count} stored line(s)." : "No visible lines for this filter yet.", "Filters: all, room, system"),
                    BuildLineCard(visibleLines.ElementAtOrDefault(0), "Recent", "No recent chat line is visible yet."),
                    BuildLineCard(visibleLines.ElementAtOrDefault(1) ?? systemLine, "Secondary", "No secondary line is visible yet.")
                };

                for (var i = 0; i < cards.Length; i++)
                {
                    cards[i].Show(cardViews[i]);
                }
            }

            private static CardView BuildLineCard(SessionState.ChatLine line, string family, string emptyLore)
            {
                if (line == null)
                {
                    return new CardView(family, "No entry", emptyLore, "Empty state uses live chat truth only.");
                }

                var speaker = string.IsNullOrWhiteSpace(line.From) ? line.ChannelLabel : line.From;
                return new CardView(family, speaker, line.ToDisplayText(), $"{line.ChannelLabel} • {line.TimestampUtc:HH:mm:ss} UTC");
            }

            private sealed class InfoCard
            {
                private readonly VisualElement root;
                private readonly Label family;
                private readonly Label title;
                private readonly Label lore;
                private readonly Label note;

                public InfoCard(VisualElement root, string prefix)
                {
                    this.root = root.Q<VisualElement>($"{prefix}-root");
                    family = root.Q<Label>($"{prefix}-title");
                    title = root.Q<Label>($"{prefix}-title-value");
                    lore = root.Q<Label>($"{prefix}-lore-value");
                    note = root.Q<Label>($"{prefix}-note-value");
                }

                public void Show(CardView card)
                {
                    if (root != null) root.style.display = DisplayStyle.Flex;
                    if (family != null) family.text = card.Family;
                    if (title != null) title.text = card.Title;
                    if (lore != null) lore.text = card.Lore;
                    if (note != null) note.text = card.Note;
                }
            }

            private sealed class CardView
            {
                public string Family { get; }
                public string Title { get; }
                public string Lore { get; }
                public string Note { get; }

                public CardView(string family, string title, string lore, string note)
                {
                    Family = family;
                    Title = title;
                    Lore = lore;
                    Note = note;
                }
            }
        }
    }
}
