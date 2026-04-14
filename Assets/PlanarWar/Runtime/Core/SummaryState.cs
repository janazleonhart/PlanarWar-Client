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
    }
}
