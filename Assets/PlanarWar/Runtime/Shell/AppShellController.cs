using PlanarWar.Client.Core;
using PlanarWar.Client.Core.Application;
using PlanarWar.Client.UI.Screens.BlackMarket;
using PlanarWar.Client.UI.Screens.City;
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
        private readonly SocialScreenController socialScreen;

        private readonly VisualElement summaryRoot;
        private readonly VisualElement cityRoot;
        private readonly VisualElement blackMarketRoot;
        private readonly VisualElement socialRoot;

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
        private readonly VisualElement loginButtonRow;
        private readonly VisualElement authActionRow;
        private readonly Button loginButton;
        private readonly Button logoutButton;

        private readonly Label currentChapterTitle;
        private readonly Label currentChapterKicker;
        private readonly Label currentChapterCopy;
        private readonly Label currentChapterMeta;
        private readonly Button navHomeButton;
        private readonly Button navDevelopmentButton;
        private readonly Button navWarfrontButton;
        private readonly Button navSocialButton;

        private readonly Label commsStatusValue;
        private readonly Label commsHintValue;
        private readonly ScrollView chatLogScroll;
        private readonly Button chatAllButton;
        private readonly Button chatRoomButton;
        private readonly Button chatSystemButton;
        private readonly Button sendChatButton;
        private readonly TextField chatInputField;

        public AppShellController(VisualElement root, SessionState sessionState, SummaryState summaryState, ShellNavigationState navigationState, ClientVersionState versionState, Func<string, System.Threading.Tasks.Task> onStartResearchRequested, Func<string, System.Threading.Tasks.Task> onStartWorkshopCraftRequested, Func<string, System.Threading.Tasks.Task> onCollectWorkshopRequested, Func<string, System.Threading.Tasks.Task> onRecruitHeroRequested, Func<string, System.Threading.Tasks.Task> onAcceptHeroRecruitCandidateRequested, Func<System.Threading.Tasks.Task> onDismissHeroRecruitCandidatesRequested, Func<string, System.Threading.Tasks.Task> onConstructBuildingRequested, Func<string, System.Threading.Tasks.Task> onUpgradeBuildingRequested, Func<string, string, System.Threading.Tasks.Task> onSwitchBuildingRoutingRequested, Func<string, System.Threading.Tasks.Task> onDestroyBuildingRequested, Func<string, string, System.Threading.Tasks.Task> onRemodelBuildingRequested, Func<string, System.Threading.Tasks.Task> onCancelActiveBuildRequested, Func<string, System.Threading.Tasks.Task> onReinforceArmyRequested, Func<string, string, System.Threading.Tasks.Task> onRenameArmyRequested, Func<string, int, string, System.Threading.Tasks.Task> onSplitArmyRequested, Func<string, string, System.Threading.Tasks.Task> onMergeArmyRequested, Func<string, System.Threading.Tasks.Task> onDisbandArmyRequested, Func<string, string, string, System.Threading.Tasks.Task> onAssignArmyHoldRequested, Func<string, System.Threading.Tasks.Task> onReleaseArmyHoldRequested, Func<string, string, string, System.Threading.Tasks.Task> onWarfrontAssaultRequested, Func<string, string, string, System.Threading.Tasks.Task> onGarrisonStrikeRequested, Action onRefreshDeskRequested, Action onBackHomeRequested)
        {
            this.sessionState = sessionState;
            this.summaryState = summaryState;
            this.navigationState = navigationState;
            this.versionState = versionState;

            summaryRoot = root.Q<VisualElement>("summary-screen");
            cityRoot = root.Q<VisualElement>("development-screen");
            blackMarketRoot = root.Q<VisualElement>("placeholder-screen");
            socialRoot = root.Q<VisualElement>("social-screen");

            summaryScreen = new SummaryScreenController(root);
            cityScreen = new CityScreenController(root, summaryState, onStartResearchRequested, onStartWorkshopCraftRequested, onCollectWorkshopRequested, onRecruitHeroRequested, onAcceptHeroRecruitCandidateRequested, onDismissHeroRecruitCandidatesRequested, onConstructBuildingRequested, onUpgradeBuildingRequested, onSwitchBuildingRoutingRequested, onDestroyBuildingRequested, onRemodelBuildingRequested, onCancelActiveBuildRequested, onRefreshDeskRequested, onBackHomeRequested);
            blackMarketScreen = new BlackMarketScreenController(root, summaryState, onReinforceArmyRequested, onRenameArmyRequested, onSplitArmyRequested, onMergeArmyRequested, onDisbandArmyRequested, onAssignArmyHoldRequested, onReleaseArmyHoldRequested, onWarfrontAssaultRequested, onGarrisonStrikeRequested, onRefreshDeskRequested);
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
            loginButtonRow = root.Q<VisualElement>("login-button-row");
            authActionRow = root.Q<VisualElement>("auth-action-row");
            loginButton = root.Q<Button>("login-button");
            logoutButton = root.Q<Button>("logout-button");

            currentChapterTitle = root.Q<Label>("current-chapter-title");
            currentChapterKicker = root.Q<Label>("current-chapter-kicker");
            currentChapterCopy = root.Q<Label>("current-chapter-copy");
            currentChapterMeta = root.Q<Label>("current-chapter-meta");
            navHomeButton = root.Q<Button>("nav-home-button");
            navDevelopmentButton = root.Q<Button>("nav-development-button");
            navWarfrontButton = root.Q<Button>("nav-warfront-button");
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
                    : "Sign in to load summary";
            authStatusValue.text = isAuthenticated
                ? sessionState.LoginStatus
                : "Awaiting sign in.";
            accountValue.text = isAuthenticated ? sessionState.DisplayName : "Guest";
            actionHintValue.text = summaryState.Snapshot.HasCity ? "Use City / Black Market tabs for lane-specific read-only surfaces." : "Founder mode: no city snapshot yet.";
            lastUpdatedValue.text = summaryState.IsLoaded ? $"Updated {summaryState.LastUpdatedUtc:HH:mm:ss} UTC" : "No summary fetch yet.";
            liveClockValue.text = $"Now {DateTime.UtcNow:HH:mm:ss} UTC";
            clientVersionValue.text = versionState?.BuildLabel ?? "v0.0.0-local";
            clientChannelValue.text = versionState?.ChannelLabel ?? "unknown channel";
            clientUpdateValue.text = versionState?.UpdateStatus ?? "Update status unavailable.";
            clientAuthorityValue.text = versionState?.AuthorityHint ?? "Patch authority unknown.";

            if (authLoginFields != null) authLoginFields.style.display = isAuthenticated ? DisplayStyle.None : DisplayStyle.Flex;
            if (loginButtonRow != null) loginButtonRow.style.display = isAuthenticated ? DisplayStyle.None : DisplayStyle.Flex;
            if (authActionRow != null) authActionRow.style.display = isAuthenticated ? DisplayStyle.Flex : DisplayStyle.None;
            loginButton?.SetEnabled(!isAuthenticated);
            logoutButton?.SetEnabled(isAuthenticated);

            summaryScreen.Render(summaryState.Snapshot, summaryState.IsLoaded);
            cityScreen.Render(summaryState.Snapshot, summaryState);
            blackMarketScreen.Render(summaryState.Snapshot);
            socialScreen.Render(sessionState);

            summaryRoot.style.display = navigationState.ActiveScreen == ShellScreen.Summary ? DisplayStyle.Flex : DisplayStyle.None;
            cityRoot.style.display = navigationState.ActiveScreen == ShellScreen.City ? DisplayStyle.Flex : DisplayStyle.None;
            blackMarketRoot.style.display = navigationState.ActiveScreen == ShellScreen.BlackMarket ? DisplayStyle.Flex : DisplayStyle.None;
            if (socialRoot != null) socialRoot.style.display = navigationState.ActiveScreen == ShellScreen.Social ? DisplayStyle.Flex : DisplayStyle.None;

            RenderChapterState();
            RenderCommsPanel();
        }

        private void RenderChapterState()
        {
            var (title, kicker, copy) = navigationState.ActiveScreen switch
            {
                ShellScreen.Summary => ("Summary", "Command floor", "This rail stays menu-owned. Use Home to scan the empire, then jump into a desk when something needs action."),
                ShellScreen.City => ("Development", "Growth desk", "Research, workshop, and growth cadence stay grouped here as a read-only planning desk."),
                ShellScreen.BlackMarket => ("Operations", "Operations doctrine", "Routes, readiness, holds, and covert deployment stay visible here before deeper operations wiring lands."),
                ShellScreen.Social => ("Social", "Shared comms", "Room state, recent lines, and channel posture stay honest here without inventing a full social stack."),
                _ => ("Summary", "Command floor", "This rail stays menu-owned.")
            };

            if (currentChapterTitle != null) currentChapterTitle.text = title;
            if (currentChapterKicker != null) currentChapterKicker.text = kicker;
            if (currentChapterCopy != null) currentChapterCopy.text = copy;
            if (currentChapterMeta != null) currentChapterMeta.text = navigationState.ActiveScreen == ShellScreen.Summary ? "Live now" : "Desk open";

            SetNavActive(navHomeButton, navigationState.ActiveScreen == ShellScreen.Summary);
            SetNavActive(navDevelopmentButton, navigationState.ActiveScreen == ShellScreen.City);
            SetNavActive(navWarfrontButton, navigationState.ActiveScreen == ShellScreen.BlackMarket);
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

                headline.text = roomJoined || visibleLines.Count > 0 ? "Social desk" : "Social review";
                copy.text = roomJoined
                    ? $"Room {roomText} is visible. Review recent lines, active filter, and connection posture without inventing a larger social stack."
                    : "No room is attached yet. This read-only desk stays honest about comms posture and recent system chatter.";
                overview.text = $"Shard {sessionState.ShardId} • room {roomText} • filter {sessionState.ActiveChatChannel} • {allLines.Count} stored line(s)";

                roomValue.text = roomJoined ? $"Room {roomText} • chat attached" : "No room attached yet.";
                channelValue.text = $"{sessionState.ActiveChatChannel.ToUpperInvariant()} filter active";
                trafficValue.text = visibleLines.Count > 0 ? $"{visibleLines.Count} visible line(s) • {allLines.Count} stored" : "No visible lines for this filter.";
                connectionValue.text = sessionState.IsConnected ? $"Connected • last op {sessionState.LastInboundOp}" : "Disconnected";
                systemValue.text = systemLine != null ? systemLine.ToDisplayText() : "No system notice yet.";
                noteValue.text = roomJoined ? "Room chat is live here; broader friend roster and cross-channel surfaces remain deferred." : "This desk keeps live room/system posture honest while broader friend and outbound channel surfaces stay deferred until they are real." ;

                var cardViews = new[]
                {
                    new CardView("Room state", roomJoined ? $"Room {roomText}" : "No attached room", roomJoined ? "Chat room is attached through WS session state." : "Where-am-I is live, but no room is attached yet.", $"Shard {sessionState.ShardId} • account {sessionState.DisplayName}"),
                    new CardView("Channel filter", sessionState.ActiveChatChannel.ToUpperInvariant(), visibleLines.Count > 0 ? $"Showing {visibleLines.Count} recent line(s) for this filter." : "This filter has no visible lines yet.", $"Available filters: all, room, system."),
                    BuildLineCard(visibleLines.ElementAtOrDefault(0), "Recent line", "No recent chat line is visible yet."),
                    BuildLineCard(visibleLines.ElementAtOrDefault(1) ?? systemLine, "Secondary line", "No secondary line is visible yet.")
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
                    return new CardView(family, "No entry", emptyLore, "Read-only social desk keeps empty states honest.");
                }

                var speaker = string.IsNullOrWhiteSpace(line.From) ? line.ChannelLabel : line.From;
                return new CardView(family, speaker, line.ToDisplayText(), $"Channel {line.ChannelLabel} • {line.TimestampUtc:HH:mm:ss} UTC");
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
