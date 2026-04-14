using PlanarWar.Client.Core;
using PlanarWar.Client.Core.Application;
using PlanarWar.Client.UI.Screens.BlackMarket;
using PlanarWar.Client.UI.Screens.City;
using PlanarWar.Client.UI.Screens.Summary;
using System;
using UnityEngine.UIElements;

namespace PlanarWar.Client.UI
{
    public sealed class AppShellController
    {
        private readonly SessionState sessionState;
        private readonly SummaryState summaryState;
        private readonly ShellNavigationState navigationState;

        private readonly SummaryScreenController summaryScreen;
        private readonly CityScreenController cityScreen;
        private readonly BlackMarketScreenController blackMarketScreen;

        private readonly VisualElement summaryRoot;
        private readonly VisualElement cityRoot;
        private readonly VisualElement blackMarketRoot;

        private readonly Label connectionValue;
        private readonly Label shardValue;
        private readonly Label roomValue;
        private readonly Label summaryStatusValue;
        private readonly Label authStatusValue;
        private readonly Label accountValue;
        private readonly Label actionHintValue;
        private readonly Label lastUpdatedValue;
        private readonly Label liveClockValue;

        private readonly VisualElement authLoginFields;
        private readonly VisualElement loginButtonRow;
        private readonly VisualElement authActionRow;
        private readonly Button loginButton;
        private readonly Button logoutButton;

        public AppShellController(VisualElement root, SessionState sessionState, SummaryState summaryState, ShellNavigationState navigationState)
        {
            this.sessionState = sessionState;
            this.summaryState = summaryState;
            this.navigationState = navigationState;

            summaryRoot = root.Q<VisualElement>("summary-screen");
            cityRoot = root.Q<VisualElement>("development-screen");
            blackMarketRoot = root.Q<VisualElement>("placeholder-screen");

            summaryScreen = new SummaryScreenController(root);
            cityScreen = new CityScreenController(root);
            blackMarketScreen = new BlackMarketScreenController(root);

            connectionValue = root.Q<Label>("connection-value");
            shardValue = root.Q<Label>("shard-value");
            roomValue = root.Q<Label>("room-value");
            summaryStatusValue = root.Q<Label>("summary-status-value");
            authStatusValue = root.Q<Label>("auth-status-value");
            accountValue = root.Q<Label>("account-value");
            actionHintValue = root.Q<Label>("action-hint-value");
            lastUpdatedValue = root.Q<Label>("last-updated-value");
            liveClockValue = root.Q<Label>("live-clock-value");

            authLoginFields = root.Q<VisualElement>("auth-login-fields");
            loginButtonRow = root.Q<VisualElement>("login-button-row");
            authActionRow = root.Q<VisualElement>("auth-action-row");
            loginButton = root.Q<Button>("login-button");
            logoutButton = root.Q<Button>("logout-button");
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

            if (authLoginFields != null) authLoginFields.style.display = isAuthenticated ? DisplayStyle.None : DisplayStyle.Flex;
            if (loginButtonRow != null) loginButtonRow.style.display = isAuthenticated ? DisplayStyle.None : DisplayStyle.Flex;
            if (authActionRow != null) authActionRow.style.display = isAuthenticated ? DisplayStyle.Flex : DisplayStyle.None;
            loginButton?.SetEnabled(!isAuthenticated);
            logoutButton?.SetEnabled(isAuthenticated);

            summaryScreen.Render(summaryState.Snapshot, summaryState.IsLoaded);
            cityScreen.Render(summaryState.Snapshot);
            blackMarketScreen.Render(summaryState.Snapshot);

            summaryRoot.style.display = navigationState.ActiveScreen == ShellScreen.Summary ? DisplayStyle.Flex : DisplayStyle.None;
            cityRoot.style.display = navigationState.ActiveScreen == ShellScreen.City ? DisplayStyle.Flex : DisplayStyle.None;
            blackMarketRoot.style.display = navigationState.ActiveScreen == ShellScreen.BlackMarket ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
