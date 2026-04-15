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
        public string PendingMissionInstanceId { get; private set; } = string.Empty;
        public string PendingHeroRecruitRole { get; private set; } = string.Empty;
        public string PendingHeroRecruitCandidateId { get; private set; } = string.Empty;
        public bool PendingHeroRecruitDismiss { get; private set; }
        public string PendingArmyReinforcementId { get; private set; } = string.Empty;

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
            BeginAction(string.IsNullOrWhiteSpace(techId) ? "Starting research..." : $"Starting research: {techId.Trim()}");
            PendingResearchTechId = techId?.Trim() ?? string.Empty;
        }

        public void BeginWorkshopCraft(string recipeId)
        {
            BeginAction(string.IsNullOrWhiteSpace(recipeId) ? "Starting workshop craft..." : $"Starting workshop craft: {recipeId.Trim()}");
            PendingWorkshopRecipeId = recipeId?.Trim() ?? string.Empty;
        }

        public void BeginWorkshopCollect(string jobId)
        {
            BeginAction(string.IsNullOrWhiteSpace(jobId) ? "Collecting workshop item..." : $"Collecting workshop job: {jobId.Trim()}");
            PendingWorkshopJobId = jobId?.Trim() ?? string.Empty;
        }

        public void BeginMissionComplete(string instanceId)
        {
            BeginAction(string.IsNullOrWhiteSpace(instanceId) ? "Completing mission..." : $"Completing mission: {instanceId.Trim()}");
            PendingMissionInstanceId = instanceId?.Trim() ?? string.Empty;
        }

        public void BeginHeroRecruit(string role)
        {
            BeginAction(string.IsNullOrWhiteSpace(role) ? "Recruiting hero..." : $"Recruiting hero: {role.Trim()}");
            PendingHeroRecruitRole = role?.Trim() ?? string.Empty;
        }

        public void BeginHeroRecruitAccept(string candidateId)
        {
            BeginAction(string.IsNullOrWhiteSpace(candidateId) ? "Accepting hero candidate..." : $"Accepting hero candidate: {candidateId.Trim()}");
            PendingHeroRecruitCandidateId = candidateId?.Trim() ?? string.Empty;
        }

        public void BeginHeroRecruitDismiss()
        {
            BeginAction("Dismissing hero candidates...");
            PendingHeroRecruitDismiss = true;
        }

        public void BeginArmyReinforcement(string armyId)
        {
            BeginAction(string.IsNullOrWhiteSpace(armyId) ? "Reinforcing army..." : $"Reinforcing army: {armyId.Trim()}");
            PendingArmyReinforcementId = armyId?.Trim() ?? string.Empty;
        }

        public void FinishAction(string status, bool failed = false)
        {
            IsActionBusy = false;
            ActionFailed = failed;
            PendingResearchTechId = string.Empty;
            PendingWorkshopJobId = string.Empty;
            PendingWorkshopRecipeId = string.Empty;
            PendingMissionInstanceId = string.Empty;
            PendingHeroRecruitRole = string.Empty;
            PendingHeroRecruitCandidateId = string.Empty;
            PendingHeroRecruitDismiss = false;
            PendingArmyReinforcementId = string.Empty;
            ActionStatus = string.IsNullOrWhiteSpace(status) ? (failed ? "Action failed." : "Action complete.") : status.Trim();
            Changed?.Invoke();
        }

        private void BeginAction(string status)
        {
            IsActionBusy = true;
            ActionFailed = false;
            PendingResearchTechId = string.Empty;
            PendingWorkshopJobId = string.Empty;
            PendingWorkshopRecipeId = string.Empty;
            PendingMissionInstanceId = string.Empty;
            PendingHeroRecruitRole = string.Empty;
            PendingHeroRecruitCandidateId = string.Empty;
            PendingHeroRecruitDismiss = false;
            PendingArmyReinforcementId = string.Empty;
            ActionStatus = status;
            Changed?.Invoke();
        }
    }
}
