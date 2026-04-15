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

        public void BeginResearchAction(string techId, string statusMessage = null)
        {
            IsActionBusy = true;
            ActionFailed = false;
            PendingResearchTechId = string.IsNullOrWhiteSpace(techId) ? string.Empty : techId.Trim();
            ActionStatus = string.IsNullOrWhiteSpace(statusMessage) ? "Submitting research order..." : statusMessage.Trim();
            Changed?.Invoke();
        }

        public void CompleteResearchAction(string statusMessage)
        {
            IsActionBusy = false;
            ActionFailed = false;
            PendingResearchTechId = string.Empty;
            ActionStatus = string.IsNullOrWhiteSpace(statusMessage) ? "Research order accepted." : statusMessage.Trim();
            Changed?.Invoke();
        }

        public void FailResearchAction(string statusMessage)
        {
            IsActionBusy = false;
            ActionFailed = true;
            PendingResearchTechId = string.Empty;
            ActionStatus = string.IsNullOrWhiteSpace(statusMessage) ? "Research order failed." : statusMessage.Trim();
            Changed?.Invoke();
        }
    }
}
