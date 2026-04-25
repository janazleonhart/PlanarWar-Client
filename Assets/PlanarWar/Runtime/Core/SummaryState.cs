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
        public string RecentStartedResearchTechId { get; private set; } = string.Empty;
        public DateTime? RecentStartedResearchAtUtc { get; private set; }
        public string RecentCompletedResearchTechId { get; private set; } = string.Empty;
        public DateTime? RecentCompletedResearchAtUtc { get; private set; }
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
            ReconcileRecentResearchStartWithSnapshot(Snapshot, LastUpdatedUtc, notify: false);
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

        public void MarkResearchStartAccepted(string techId)
        {
            RecentStartedResearchTechId = techId?.Trim() ?? string.Empty;
            RecentStartedResearchAtUtc = DateTime.UtcNow;
            Changed?.Invoke();
        }

        public void ClearRecentResearchStart()
        {
            RecentStartedResearchTechId = string.Empty;
            RecentStartedResearchAtUtc = null;
            Changed?.Invoke();
        }

        public bool HasRecentResearchCompletionNotice(DateTime nowUtc, double noticeSeconds = 30)
        {
            if (!RecentCompletedResearchAtUtc.HasValue || string.IsNullOrWhiteSpace(RecentCompletedResearchTechId))
            {
                return false;
            }

            return nowUtc - RecentCompletedResearchAtUtc.Value < TimeSpan.FromSeconds(Math.Max(1, noticeSeconds));
        }

        public bool HasRecentResearchStartGuard(DateTime nowUtc, double guardSeconds = 0)
        {
            if (!RecentStartedResearchAtUtc.HasValue || string.IsNullOrWhiteSpace(RecentStartedResearchTechId))
            {
                return false;
            }

            if (guardSeconds <= 0)
            {
                return true;
            }

            return nowUtc - RecentStartedResearchAtUtc.Value < TimeSpan.FromSeconds(Math.Max(1, guardSeconds));
        }

        public bool HasResearchStartCanonicalWaitWarning(DateTime nowUtc, double warningSeconds = 30)
        {
            return HasRecentResearchStartGuard(nowUtc)
                && RecentStartedResearchAtUtc.HasValue
                && nowUtc - RecentStartedResearchAtUtc.Value >= TimeSpan.FromSeconds(Math.Max(1, warningSeconds));
        }

        public void ReconcileRecentResearchStartWithSnapshot(ShellSummarySnapshot snapshot, DateTime nowUtc, bool notify = true)
        {
            if (!HasRecentResearchStartGuard(nowUtc))
            {
                return;
            }

            var acceptedTechId = RecentStartedResearchTechId;
            var hasCanonicalResearch = SnapshotHasCanonicalResearch(snapshot);
            var acceptedResearchCompleted = SnapshotShowsAcceptedResearchCompleted(snapshot, acceptedTechId);
            if (!hasCanonicalResearch && !acceptedResearchCompleted)
            {
                return;
            }

            RecentStartedResearchTechId = string.Empty;
            RecentStartedResearchAtUtc = null;
            if (acceptedResearchCompleted && !hasCanonicalResearch)
            {
                RecentCompletedResearchTechId = acceptedTechId;
                RecentCompletedResearchAtUtc = nowUtc;
            }
            if (notify)
            {
                Changed?.Invoke();
            }
        }

        private static bool SnapshotHasCanonicalResearch(ShellSummarySnapshot snapshot)
        {
            if (snapshot == null)
            {
                return false;
            }

            if (snapshot.ActiveResearch != null && HasResearchIdentity(snapshot.ActiveResearch))
            {
                return true;
            }

            if (snapshot.ActiveResearches != null && snapshot.ActiveResearches.Any(HasResearchIdentity))
            {
                return true;
            }

            return snapshot.CityTimers != null && snapshot.CityTimers.Any(IsResearchTimer);
        }

        private static bool SnapshotShowsAcceptedResearchCompleted(ShellSummarySnapshot snapshot, string acceptedTechId)
        {
            if (snapshot == null || string.IsNullOrWhiteSpace(acceptedTechId))
            {
                return false;
            }

            if (snapshot.ResearchedTechIds != null
                && snapshot.ResearchedTechIds.Any(id => string.Equals(id, acceptedTechId, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            if (snapshot.AvailableTechs == null || snapshot.AvailableTechs.Count == 0)
            {
                return false;
            }

            return !snapshot.AvailableTechs.Any(tech => string.Equals(tech?.Id, acceptedTechId, StringComparison.OrdinalIgnoreCase));
        }

        private static bool HasResearchIdentity(ResearchSnapshot research)
        {
            return research != null
                && (!string.IsNullOrWhiteSpace(research.Id)
                    || !string.IsNullOrWhiteSpace(research.Name)
                    || research.FinishesAtUtc.HasValue);
        }

        private static bool IsResearchTimer(CityTimerEntrySnapshot timer)
        {
            if (timer == null)
            {
                return false;
            }

            return ContainsResearchWord(timer.Category)
                || ContainsResearchWord(timer.Label)
                || ContainsResearchWord(timer.Detail);
        }

        private static bool ContainsResearchWord(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            return raw.IndexOf("research", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("tech", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("unlock", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("shadow_book", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("shadow-book", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("shadow book", StringComparison.OrdinalIgnoreCase) >= 0;
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

        public void BeginArmyRename(string armyId, string requestedName)
        {
            var label = string.IsNullOrWhiteSpace(armyId) ? "Renaming formation..." : $"Renaming formation: {armyId.Trim()}";
            if (!string.IsNullOrWhiteSpace(requestedName))
            {
                label += $" -> {requestedName.Trim()}";
            }

            BeginAction(label);
        }

        public void BeginArmySplit(string armyId, int requestedSize, string requestedName)
        {
            var label = string.IsNullOrWhiteSpace(armyId)
                ? $"Splitting formation ({requestedSize})..."
                : $"Splitting formation: {armyId.Trim()} ({requestedSize})";
            if (!string.IsNullOrWhiteSpace(requestedName))
            {
                label += $" -> {requestedName.Trim()}";
            }

            BeginAction(label);
        }

        public void BeginArmyMerge(string sourceArmyId, string targetArmyId)
        {
            var label = string.IsNullOrWhiteSpace(sourceArmyId)
                ? "Merging formation..."
                : $"Merging formation: {sourceArmyId.Trim()}";
            if (!string.IsNullOrWhiteSpace(targetArmyId))
            {
                label += $" -> {targetArmyId.Trim()}";
            }

            BeginAction(label);
        }

        public void BeginArmyDisband(string armyId)
        {
            BeginAction(string.IsNullOrWhiteSpace(armyId)
                ? "Disbanding formation..."
                : $"Disbanding formation: {armyId.Trim()}");
        }

        public void BeginArmyHoldAssign(string armyId, string regionId, string posture)
        {
            var label = string.IsNullOrWhiteSpace(armyId)
                ? "Assigning regional hold..."
                : $"Assigning regional hold: {armyId.Trim()}";
            if (!string.IsNullOrWhiteSpace(regionId))
            {
                label += $" -> {regionId.Trim()}";
            }

            if (!string.IsNullOrWhiteSpace(posture))
            {
                label += $" ({posture.Trim()})";
            }

            BeginAction(label);
        }

        public void BeginArmyHoldRelease(string armyId)
        {
            BeginAction(string.IsNullOrWhiteSpace(armyId)
                ? "Releasing regional hold..."
                : $"Releasing regional hold: {armyId.Trim()}");
        }

        public void BeginFrontlineDispatch(string actionLabel, string regionId, string armyId, string heroId = null)
        {
            var label = string.IsNullOrWhiteSpace(actionLabel) ? "Dispatching frontline action..." : $"Dispatching {actionLabel.Trim()}";
            if (!string.IsNullOrWhiteSpace(regionId))
            {
                label += $" -> {regionId.Trim()}";
            }
            if (!string.IsNullOrWhiteSpace(armyId))
            {
                label += $" with {armyId.Trim()}";
            }
            if (!string.IsNullOrWhiteSpace(heroId))
            {
                label += $" under {heroId.Trim()}";
            }

            BeginAction(label);
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
