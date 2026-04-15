using Newtonsoft.Json.Linq;
using PlanarWar.Client.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public List<WorkshopRecipeSnapshot> WorkshopRecipes { get; private set; } = new();

        public bool IsActionBusy { get; private set; }
        public string ActionStatus { get; private set; } = string.Empty;
        public bool ActionFailed { get; private set; }
        public string PendingResearchTechId { get; private set; } = string.Empty;
        public string PendingWorkshopJobId { get; private set; } = string.Empty;
        public string PendingWorkshopRecipeId { get; private set; } = string.Empty;
        public string PendingMissionId { get; private set; } = string.Empty;
        public string PendingMissionInstanceId { get; private set; } = string.Empty;

        public void Apply(JObject summary, ShellSummarySnapshot snapshot, IEnumerable<WorkshopRecipeSnapshot> workshopRecipes = null)
        {
            RawSummary = summary;
            Snapshot = snapshot ?? ShellSummarySnapshot.Empty;
            IsLoaded = summary != null;
            LastError = "-";
            LastUpdatedUtc = DateTime.UtcNow;
            WorkshopRecipes = workshopRecipes?.Where(r => r != null).ToList() ?? new List<WorkshopRecipeSnapshot>();
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
            PendingWorkshopRecipeId = string.Empty;
            PendingMissionId = string.Empty;
            PendingMissionInstanceId = string.Empty;
            ActionStatus = string.IsNullOrWhiteSpace(PendingResearchTechId) ? "Starting research..." : $"Starting research: {PendingResearchTechId}";
            Changed?.Invoke();
        }

        public void BeginWorkshopCraft(string recipeId)
        {
            IsActionBusy = true;
            ActionFailed = false;
            PendingResearchTechId = string.Empty;
            PendingWorkshopJobId = string.Empty;
            PendingWorkshopRecipeId = recipeId?.Trim() ?? string.Empty;
            PendingMissionId = string.Empty;
            PendingMissionInstanceId = string.Empty;
            ActionStatus = string.IsNullOrWhiteSpace(PendingWorkshopRecipeId) ? "Starting workshop craft..." : $"Starting workshop craft: {PendingWorkshopRecipeId}";
            Changed?.Invoke();
        }

        public void BeginWorkshopCollect(string jobId)
        {
            IsActionBusy = true;
            ActionFailed = false;
            PendingResearchTechId = string.Empty;
            PendingWorkshopRecipeId = string.Empty;
            PendingWorkshopJobId = jobId?.Trim() ?? string.Empty;
            PendingMissionId = string.Empty;
            PendingMissionInstanceId = string.Empty;
            ActionStatus = string.IsNullOrWhiteSpace(PendingWorkshopJobId) ? "Collecting workshop item..." : $"Collecting workshop job: {PendingWorkshopJobId}";
            Changed?.Invoke();
        }


        public void BeginMissionStart(string missionId)
        {
            IsActionBusy = true;
            ActionFailed = false;
            PendingResearchTechId = string.Empty;
            PendingWorkshopJobId = string.Empty;
            PendingWorkshopRecipeId = string.Empty;
            PendingMissionId = missionId?.Trim() ?? string.Empty;
            PendingMissionInstanceId = string.Empty;
            ActionStatus = string.IsNullOrWhiteSpace(PendingMissionId) ? "Launching mission..." : $"Launching mission: {PendingMissionId}";
            Changed?.Invoke();
        }

        public void BeginMissionComplete(string instanceId, string missionTitle = null)
        {
            IsActionBusy = true;
            ActionFailed = false;
            PendingResearchTechId = string.Empty;
            PendingWorkshopJobId = string.Empty;
            PendingWorkshopRecipeId = string.Empty;
            PendingMissionId = string.Empty;
            PendingMissionInstanceId = instanceId?.Trim() ?? string.Empty;
            ActionStatus = !string.IsNullOrWhiteSpace(missionTitle)
                ? $"Completing mission: {missionTitle.Trim()}"
                : string.IsNullOrWhiteSpace(PendingMissionInstanceId)
                    ? "Completing mission..."
                    : $"Completing mission: {PendingMissionInstanceId}";
            Changed?.Invoke();
        }


        public void FinishAction(string status, bool failed = false)
        {
            IsActionBusy = false;
            ActionFailed = failed;
            PendingResearchTechId = string.Empty;
            PendingWorkshopJobId = string.Empty;
            PendingWorkshopRecipeId = string.Empty;
            PendingMissionId = string.Empty;
            PendingMissionInstanceId = string.Empty;
            ActionStatus = string.IsNullOrWhiteSpace(status) ? (failed ? "Action failed." : "Action complete.") : status.Trim();
            Changed?.Invoke();
        }
    }
}
