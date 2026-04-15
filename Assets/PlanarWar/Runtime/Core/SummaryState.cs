using Newtonsoft.Json.Linq;
using PlanarWar.Client.Core.Contracts;
using System;

namespace PlanarWar.Client.Core
{
    [Serializable]
    public sealed class SummaryState
    {
        public event Action Changed;

        public JObject RawSummary { get; private set; }
        public ShellSummarySnapshot Snapshot { get; private set; } = ShellSummarySnapshot.Empty;
        public bool IsLoaded { get; private set; }
        public string LastError { get; private set; } = "-";
        public DateTime LastUpdatedUtc { get; private set; }

        public bool IsActionBusy { get; private set; }
        public string ActionStatus { get; private set; } = string.Empty;
        public bool ActionFailed { get; private set; }
        public string PendingResearchTechId { get; private set; } = string.Empty;
        public string PendingWorkshopJobId { get; private set; } = string.Empty;

        public void Apply(JObject summary, ShellSummarySnapshot snapshot)
        {
            RawSummary = summary;
            Snapshot = snapshot ?? ShellSummarySnapshot.Empty;
            IsLoaded = summary != null;
            LastError = "-";
            LastUpdatedUtc = DateTime.UtcNow;
            Changed?.Invoke();
        }

        public void SetError(string error)
        {
            LastError = string.IsNullOrWhiteSpace(error) ? "Unknown summary error." : error.Trim();
            Changed?.Invoke();
        }

        public void BeginResearchAction(string techId)
        {
            IsActionBusy = true;
            ActionFailed = false;
            PendingResearchTechId = techId?.Trim() ?? string.Empty;
            PendingWorkshopJobId = string.Empty;
            ActionStatus = string.IsNullOrWhiteSpace(PendingResearchTechId) ? "Starting research..." : $"Starting research: {PendingResearchTechId}";
            Changed?.Invoke();
        }

        public void BeginWorkshopCollect(string jobId)
        {
            IsActionBusy = true;
            ActionFailed = false;
            PendingResearchTechId = string.Empty;
            PendingWorkshopJobId = jobId?.Trim() ?? string.Empty;
            ActionStatus = string.IsNullOrWhiteSpace(PendingWorkshopJobId) ? "Collecting workshop item..." : $"Collecting workshop job: {PendingWorkshopJobId}";
            Changed?.Invoke();
        }

        public void FinishAction(string status, bool failed = false)
        {
            IsActionBusy = false;
            ActionFailed = failed;
            PendingResearchTechId = string.Empty;
            PendingWorkshopJobId = string.Empty;
            ActionStatus = string.IsNullOrWhiteSpace(status) ? (failed ? "Action failed." : "Action complete.") : status.Trim();
            Changed?.Invoke();
        }
    }
}
