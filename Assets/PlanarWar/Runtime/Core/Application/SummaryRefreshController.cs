using PlanarWar.Client.Core.Mapping;
using PlanarWar.Client.Network;
using System.Threading.Tasks;

namespace PlanarWar.Client.Core.Application
{
    public sealed class SummaryRefreshController
    {
        private readonly PlanarWarApiClient apiClient;
        private readonly SummaryState summaryState;

        public SummaryRefreshController(PlanarWarApiClient apiClient, SummaryState summaryState)
        {
            this.apiClient = apiClient;
            this.summaryState = summaryState;
        }

        public async Task RefreshAsync()
        {
            var raw = await apiClient.FetchSummaryAsync();
			var snapshot = ShellSummarySnapshotMapper.Map(raw);
			summaryState.Apply(raw, snapshot);
        }
    }
}
