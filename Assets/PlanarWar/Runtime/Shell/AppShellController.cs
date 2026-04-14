using PlanarWar.Client.Core;
using PlanarWar.Client.Core.Application;
using PlanarWar.Client.UI.Screens.BlackMarket;
using PlanarWar.Client.UI.Screens.City;
using PlanarWar.Client.UI.Screens.Summary;
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
        }

        public void Render()
        {
            connectionValue.text = sessionState.IsConnected ? "Connected" : "Disconnected";
            shardValue.text = sessionState.ShardId;
            roomValue.text = sessionState.RoomId;
            summaryStatusValue.text = summaryState.IsLoaded ? "Summary loaded" : summaryState.LastError;
            authStatusValue.text = sessionState.LoginStatus;
            accountValue.text = sessionState.DisplayName;
            actionHintValue.text = summaryState.Snapshot.HasCity ? "Use City / Black Market tabs for lane-specific read-only surfaces." : "Founder mode: no city snapshot yet.";
            lastUpdatedValue.text = summaryState.IsLoaded ? $"Updated {summaryState.LastUpdatedUtc:HH:mm:ss} UTC" : "No summary fetch yet.";

            summaryScreen.Render(summaryState.Snapshot);
            cityScreen.Render(summaryState.Snapshot);
            blackMarketScreen.Render(summaryState.Snapshot);

            summaryRoot.style.display = navigationState.ActiveScreen == ShellScreen.Summary ? DisplayStyle.Flex : DisplayStyle.None;
            cityRoot.style.display = navigationState.ActiveScreen == ShellScreen.City ? DisplayStyle.Flex : DisplayStyle.None;
            blackMarketRoot.style.display = navigationState.ActiveScreen == ShellScreen.BlackMarket ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
