using Newtonsoft.Json;
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
        public List<MissionOfferSnapshot> MissionOffers { get; private set; } = new();

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
        public string PendingMissionOfferId { get; private set; } = string.Empty;
        public string PendingMissionInstanceId { get; private set; } = string.Empty;
        public string RecentMissionReceipt { get; private set; } = string.Empty;
        public string RecentMissionTitle { get; private set; } = string.Empty;
        public string RecentMissionInstanceId { get; private set; } = string.Empty;
        public DateTime? RecentMissionReceiptAtUtc { get; private set; }
        public string RecentHeroReceipt { get; private set; } = string.Empty;
        public string RecentHeroReceiptTitle { get; private set; } = string.Empty;
        public string RecentHeroReceiptAction { get; private set; } = string.Empty;
        public DateTime? RecentHeroReceiptAtUtc { get; private set; }
        public string PendingHeroRecruitRole { get; private set; } = string.Empty;
        public string PendingHeroRecruitCandidateId { get; private set; } = string.Empty;
        public bool PendingHeroRecruitDismiss { get; private set; }
        public string PendingHeroReleaseId { get; private set; } = string.Empty;
        public string PendingArmyReinforcementId { get; private set; } = string.Empty;
        public string PendingBuildingKind { get; private set; } = string.Empty;
        public string PendingBuildingId { get; private set; } = string.Empty;
        public string PendingBuildingRoutingPreference { get; private set; } = string.Empty;
        public string PendingBuildingConfirmAction { get; private set; } = string.Empty;
        public string PendingBuildingConfirmToken { get; private set; } = string.Empty;
        public string PendingBuildingConfirmBuildingId { get; private set; } = string.Empty;
        public string PendingBuildingConfirmTargetKind { get; private set; } = string.Empty;
        public string PendingBuildingConfirmActiveBuildId { get; private set; } = string.Empty;

        public void Apply(JObject summary, ShellSummarySnapshot snapshot, IEnumerable<WorkshopRecipeSnapshot> workshopRecipes = null, IEnumerable<MissionOfferSnapshot> missionOffers = null)
        {
            ApplyInternal(summary, snapshot, summary != null, workshopRecipes, missionOffers);
        }

        public void ApplySnapshot(ShellSummarySnapshot snapshot, IEnumerable<WorkshopRecipeSnapshot> workshopRecipes = null, IEnumerable<MissionOfferSnapshot> missionOffers = null)
        {
            ApplyInternal(null, snapshot, snapshot != null, workshopRecipes, missionOffers);
        }

        private void ApplyInternal(JObject summary, ShellSummarySnapshot snapshot, bool isLoaded, IEnumerable<WorkshopRecipeSnapshot> workshopRecipes, IEnumerable<MissionOfferSnapshot> missionOffers)
        {
            RawSummary = summary;
            Snapshot = snapshot ?? ShellSummarySnapshot.Empty;
            IsLoaded = isLoaded;
            LastError = "-";
            LastUpdatedUtc = DateTime.UtcNow;
            WorkshopRecipes = workshopRecipes?.Where(r => r != null).ToList() ?? new List<WorkshopRecipeSnapshot>();
            MissionOffers = missionOffers?.Where(m => m != null).ToList() ?? new List<MissionOfferSnapshot>();
            if (MissionOffers.Count == 0 && Snapshot.MissionOffers != null && Snapshot.MissionOffers.Count > 0)
            {
                MissionOffers = Snapshot.MissionOffers.Where(m => m != null).ToList();
            }
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
            var label = ResolveResearchReceiptLabel(techId);
            BeginAction(string.IsNullOrWhiteSpace(label) ? "Starting research..." : $"Starting research: {label}");
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

        public void BeginBuildingConstruct(string kind)
        {
            var label = ResolveBuildingKindReceiptLabel(kind);
            BeginAction(string.IsNullOrWhiteSpace(label) ? "Starting construction..." : $"Starting construction: {label}");
            PendingBuildingKind = kind?.Trim() ?? string.Empty;
        }

        public void BeginBuildingUpgrade(string buildingId)
        {
            var label = ResolveBuildingReceiptLabel(buildingId);
            BeginAction(string.IsNullOrWhiteSpace(label) ? "Starting building upgrade..." : $"Starting building upgrade: {label}");
            PendingBuildingId = buildingId?.Trim() ?? string.Empty;
        }

        public void BeginBuildingRouting(string buildingId, string routingPreference)
        {
            var buildingLabel = ResolveBuildingReceiptLabel(buildingId);
            var routingLabel = ResolveBuildingRoutingReceiptLabel(routingPreference);
            var label = string.IsNullOrWhiteSpace(buildingLabel) ? "Switching building routing..." : $"Switching building routing: {buildingLabel}";
            if (!string.IsNullOrWhiteSpace(routingLabel))
            {
                label += $" -> {routingLabel}";
            }

            BeginAction(label);
            PendingBuildingId = buildingId?.Trim() ?? string.Empty;
            PendingBuildingRoutingPreference = routingPreference?.Trim() ?? string.Empty;
        }

        public void BeginBuildingDestroy(string buildingId, bool confirming = false)
        {
            var label = ResolveBuildingReceiptLabel(buildingId);
            BeginAction(string.IsNullOrWhiteSpace(label)
                ? (confirming ? "Confirming demolition..." : "Requesting demolition confirmation...")
                : confirming ? $"Confirming demolition: {label}" : $"Requesting demolition confirmation: {label}");
            PendingBuildingId = buildingId?.Trim() ?? string.Empty;
        }

        public void BeginBuildingRemodel(string buildingId, string targetKind, bool confirming = false)
        {
            var buildingLabel = ResolveBuildingReceiptLabel(buildingId);
            var targetLabel = ResolveBuildingKindReceiptLabel(targetKind);
            var label = string.IsNullOrWhiteSpace(buildingLabel)
                ? (confirming ? "Confirming remodel..." : "Requesting remodel confirmation...")
                : confirming ? $"Confirming remodel: {buildingLabel}" : $"Requesting remodel confirmation: {buildingLabel}";
            if (!string.IsNullOrWhiteSpace(targetLabel))
            {
                label += $" -> {targetLabel}";
            }

            BeginAction(label);
            PendingBuildingId = buildingId?.Trim() ?? string.Empty;
            PendingBuildingKind = targetKind?.Trim() ?? string.Empty;
        }

        public void BeginActiveBuildCancel(string activeBuildId, bool confirming = false)
        {
            var label = ResolveActiveBuildReceiptLabel(activeBuildId);
            BeginAction(string.IsNullOrWhiteSpace(label)
                ? (confirming ? "Confirming build cancellation..." : "Requesting build cancellation confirmation...")
                : confirming ? $"Confirming build cancellation: {label}" : $"Requesting build cancellation confirmation: {label}");
            PendingBuildingId = activeBuildId?.Trim() ?? string.Empty;
        }

        public void MarkBuildingConfirmRequired(string action, string confirmToken, string buildingId = null, string targetKind = null, string activeBuildId = null)
        {
            PendingBuildingConfirmAction = action?.Trim() ?? string.Empty;
            PendingBuildingConfirmToken = confirmToken?.Trim() ?? string.Empty;
            PendingBuildingConfirmBuildingId = buildingId?.Trim() ?? string.Empty;
            PendingBuildingConfirmTargetKind = targetKind?.Trim() ?? string.Empty;
            PendingBuildingConfirmActiveBuildId = activeBuildId?.Trim() ?? string.Empty;
            Changed?.Invoke();
        }

        public void ClearBuildingConfirm()
        {
            PendingBuildingConfirmAction = string.Empty;
            PendingBuildingConfirmToken = string.Empty;
            PendingBuildingConfirmBuildingId = string.Empty;
            PendingBuildingConfirmTargetKind = string.Empty;
            PendingBuildingConfirmActiveBuildId = string.Empty;
            Changed?.Invoke();
        }

        public bool HasPendingBuildingConfirm(string action, string buildingId = null, string targetKind = null, string activeBuildId = null)
        {
            if (string.IsNullOrWhiteSpace(PendingBuildingConfirmToken)
                || !string.Equals(PendingBuildingConfirmAction, action?.Trim() ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(buildingId)
                && !string.Equals(PendingBuildingConfirmBuildingId, buildingId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(targetKind)
                && !string.Equals(PendingBuildingConfirmTargetKind, targetKind.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(activeBuildId)
                && !string.Equals(PendingBuildingConfirmActiveBuildId, activeBuildId.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public string GetPendingBuildingConfirmToken(string action, string buildingId = null, string targetKind = null, string activeBuildId = null)
        {
            return HasPendingBuildingConfirm(action, buildingId, targetKind, activeBuildId)
                ? PendingBuildingConfirmToken
                : string.Empty;
        }

        public string ResolveResearchReceiptLabel(string techId)
        {
            var id = techId?.Trim() ?? string.Empty;
            var available = Snapshot?.AvailableTechs?.FirstOrDefault(tech => SameId(tech?.Id, id));
            var active = Snapshot?.ActiveResearches?.FirstOrDefault(research => SameId(research?.Id, id))
                ?? (SameId(Snapshot?.ActiveResearch?.Id, id) ? Snapshot.ActiveResearch : null);
            var timer = Snapshot?.CityTimers?.FirstOrDefault(candidate => SameId(candidate?.Id, id) || SameId(candidate?.Label, id));
            return BuildReceiptLabel(id, FirstUsableReceiptLabel(id, "research", available?.Name, active?.Name, timer?.Label), "research");
        }

        public string ResolveBuildingKindReceiptLabel(string kind)
        {
            var id = kind?.Trim() ?? string.Empty;
            return BuildReceiptLabel(id, FirstUsableReceiptLabel(id, "building", ResolveKnownBuildingKindLabel(id)), "building");
        }

        public string ResolveBuildingReceiptLabel(string buildingId)
        {
            var id = buildingId?.Trim() ?? string.Empty;
            var building = Snapshot?.Buildings?.FirstOrDefault(candidate => SameId(candidate?.Id, id) || SameId(candidate?.BuildingId, id));
            var timer = Snapshot?.CityTimers?.FirstOrDefault(candidate => SameId(candidate?.Id, id));
            var kindLabel = ResolveKnownBuildingKindLabel(FirstNonBlankText(building?.Type, building?.BuildingId));
            return BuildReceiptLabel(id, FirstUsableReceiptLabel(id, "building", building?.Name, kindLabel, building?.Type, building?.BuildingId, timer?.Label), "building");
        }

        public string ResolveActiveBuildReceiptLabel(string activeBuildId)
        {
            var id = activeBuildId?.Trim() ?? string.Empty;
            var timer = Snapshot?.CityTimers?.FirstOrDefault(candidate => SameId(candidate?.Id, id));
            var building = Snapshot?.Buildings?.FirstOrDefault(candidate => SameId(candidate?.Id, id) || SameId(candidate?.BuildingId, id));
            var kindLabel = ResolveKnownBuildingKindLabel(FirstNonBlankText(building?.Type, building?.BuildingId));
            return BuildReceiptLabel(id, FirstUsableReceiptLabel(id, "building", timer?.Label, building?.Name, kindLabel, building?.Type), "building");
        }

        public static string ResolveBuildingRoutingReceiptLabel(string routingPreference)
        {
            var normalized = NormalizeBuildingRoutingPreference(routingPreference);
            if (normalized.Equals("prefer_local", StringComparison.OrdinalIgnoreCase)) return "Local • nearby demand";
            if (normalized.Equals("prefer_reserve", StringComparison.OrdinalIgnoreCase)) return "Reserve • protected stock";
            if (normalized.Equals("prefer_exchange", StringComparison.OrdinalIgnoreCase)) return "Exchange • trade flow";
            return "Balanced • spread output";
        }

        public string ResolveMissionOfferReceiptLabel(string missionId)
        {
            var id = missionId?.Trim() ?? string.Empty;
            var offer = MissionOffers?.FirstOrDefault(mission => SameId(mission?.Id, id))
                ?? Snapshot?.MissionOffers?.FirstOrDefault(mission => SameId(mission?.Id, id));
            var operation = Snapshot?.OpeningOperations?.FirstOrDefault(operation => SameId(operation?.MissionId, id));
            return BuildReceiptLabel(id, FirstUsableReceiptLabel(id, "mission", offer?.Title, operation?.Title), "mission");
        }

        public string ResolveMissionInstanceReceiptLabel(string instanceId)
        {
            var id = instanceId?.Trim() ?? string.Empty;
            var activeMission = Snapshot?.ActiveMissions?.FirstOrDefault(mission => SameId(mission?.InstanceId, id));
            return BuildReceiptLabel(id, FirstUsableReceiptLabel(id, "mission", activeMission?.Title), "mission");
        }

        public string ResolveArmyReceiptLabel(string armyId)
        {
            var id = armyId?.Trim() ?? string.Empty;
            var army = Snapshot?.Armies?.FirstOrDefault(candidate => SameId(candidate?.Id, id));
            var reinforcement = Snapshot?.ArmyReinforcement;
            var reinforcementName = reinforcement != null && SameId(reinforcement.ArmyId, id)
                ? reinforcement.ArmyName
                : string.Empty;
            var activeMission = Snapshot?.ActiveMissions?.FirstOrDefault(mission => SameId(mission?.AssignedArmyId, id));
            return BuildReceiptLabel(id, FirstUsableReceiptLabel(id, "formation", army?.Name, reinforcementName, activeMission?.AssignedArmyName), "formation");
        }

        public string ResolveHeroReceiptLabel(string heroId)
        {
            var id = heroId?.Trim() ?? string.Empty;
            var hero = Snapshot?.Heroes?.FirstOrDefault(candidate => SameId(candidate?.Id, id));
            var activeMission = Snapshot?.ActiveMissions?.FirstOrDefault(mission => SameId(mission?.AssignedHeroId, id));
            return BuildReceiptLabel(id, FirstUsableReceiptLabel(id, "hero", hero?.Name, activeMission?.AssignedHeroName), "hero");
        }

        public string ResolveRegionReceiptLabel(string regionId)
        {
            return BuildReceiptLabel(regionId?.Trim() ?? string.Empty, string.Empty, "region");
        }

        public static string ResolvePostureReceiptLabel(string posture)
        {
            return BuildReceiptLabel(posture?.Trim() ?? string.Empty, string.Empty, "posture");
        }

        public void BeginMissionStartAction(string missionId)
        {
            var label = ResolveMissionOfferReceiptLabel(missionId);
            BeginAction(string.IsNullOrWhiteSpace(label) ? "Starting mission..." : $"Starting mission: {label}");
            PendingMissionOfferId = missionId?.Trim() ?? string.Empty;
        }

        public void BeginMissionCompleteAction(string instanceId)
        {
            var label = ResolveMissionInstanceReceiptLabel(instanceId);
            BeginAction(string.IsNullOrWhiteSpace(label) ? "Completing mission..." : $"Completing mission: {label}");
            PendingMissionInstanceId = instanceId?.Trim() ?? string.Empty;
        }

        public void FinishMissionCompletion(string instanceId, string receipt, string title = null)
        {
            var trimmedInstanceId = instanceId?.Trim() ?? string.Empty;
            var trimmedReceipt = string.IsNullOrWhiteSpace(receipt)
                ? "Mission completed, but the backend did not return a readable receipt."
                : receipt.Trim();

            RecentMissionInstanceId = trimmedInstanceId;
            RecentMissionTitle = title?.Trim() ?? string.Empty;
            RecentMissionReceipt = trimmedReceipt;
            RecentMissionReceiptAtUtc = DateTime.UtcNow;
            FinishAction(trimmedReceipt);
        }

        public bool HasRecentMissionReceipt(DateTime nowUtc, double noticeSeconds = 120)
        {
            if (!RecentMissionReceiptAtUtc.HasValue || string.IsNullOrWhiteSpace(RecentMissionReceipt))
            {
                return false;
            }

            return nowUtc - RecentMissionReceiptAtUtc.Value < TimeSpan.FromSeconds(Math.Max(1, noticeSeconds));
        }

        public void ClearRecentMissionReceipt()
        {
            RecentMissionReceipt = string.Empty;
            RecentMissionTitle = string.Empty;
            RecentMissionInstanceId = string.Empty;
            RecentMissionReceiptAtUtc = null;
            Changed?.Invoke();
        }

        public void FinishHeroActionReceipt(string action, string receipt, string title = null)
        {
            var trimmedAction = action?.Trim() ?? string.Empty;
            var trimmedReceipt = string.IsNullOrWhiteSpace(receipt)
                ? "Roster action completed, but the backend did not return a readable receipt."
                : receipt.Trim();

            RecentHeroReceiptAction = trimmedAction;
            RecentHeroReceiptTitle = title?.Trim() ?? string.Empty;
            RecentHeroReceipt = trimmedReceipt;
            RecentHeroReceiptAtUtc = DateTime.UtcNow;
            FinishAction(trimmedReceipt);
        }

        public bool HasRecentHeroReceipt(DateTime nowUtc, double noticeSeconds = 120)
        {
            if (!RecentHeroReceiptAtUtc.HasValue || string.IsNullOrWhiteSpace(RecentHeroReceipt))
            {
                return false;
            }

            return nowUtc - RecentHeroReceiptAtUtc.Value < TimeSpan.FromSeconds(Math.Max(1, noticeSeconds));
        }

        public void ClearRecentHeroReceipt()
        {
            RecentHeroReceipt = string.Empty;
            RecentHeroReceiptTitle = string.Empty;
            RecentHeroReceiptAction = string.Empty;
            RecentHeroReceiptAtUtc = null;
            Changed?.Invoke();
        }

        public static string FormatHeroActionReceipt(string responseJson, string fallbackAction, string fallbackSubject, string subjectNoun)
        {
            var action = string.IsNullOrWhiteSpace(fallbackAction) ? "Roster action completed" : fallbackAction.Trim();
            var noun = string.IsNullOrWhiteSpace(subjectNoun) ? "Hero" : subjectNoun.Trim();
            var subject = string.IsNullOrWhiteSpace(fallbackSubject) ? string.Empty : fallbackSubject.Trim();
            var fallback = string.IsNullOrWhiteSpace(subject) ? action : $"{action}: {subject}";

            if (string.IsNullOrWhiteSpace(responseJson))
            {
                return $"{fallback}. No {noun.ToLowerInvariant()} receipt was returned.";
            }

            try
            {
                var root = JToken.Parse(responseJson);
                var result = FirstDirectToken(new[] { root }, "result", "hero", "operative", "recruitment", "release", "data");
                if (result == null || result.Type != JTokenType.Object)
                {
                    result = root;
                }

                var receipt = FirstDirectToken(new[] { result, root }, "receipt", "heroReceipt", "hero_receipt", "operativeReceipt", "operative_receipt", "rosterReceipt", "roster_receipt");
                if (receipt == null || receipt.Type != JTokenType.Object)
                {
                    receipt = null;
                }

                var parts = new List<string>();
                var outcome = FirstReceiptText(
                    Child(Child(result, "outcome"), "kind"),
                    Child(result, "outcome"),
                    Child(receipt, "outcome"),
                    Child(result, "status"),
                    Child(root, "status"));
                if (!string.IsNullOrWhiteSpace(outcome))
                {
                    parts.Add($"Outcome: {HumanizeReceiptPhrase(outcome)}");
                }

                var namedSubject = FirstReceiptText(
                    Child(receipt, "heroName"),
                    Child(receipt, "hero_name"),
                    Child(receipt, "operativeName"),
                    Child(receipt, "operative_name"),
                    Child(receipt, "candidateName"),
                    Child(receipt, "candidate_name"),
                    Child(result, "displayName"),
                    Child(result, "display_name"),
                    Child(result, "name"),
                    Child(root, "displayName"),
                    Child(root, "display_name"),
                    Child(root, "name"));
                if (!string.IsNullOrWhiteSpace(namedSubject))
                {
                    parts.Add($"{noun}: {namedSubject}");
                }

                var roleText = FirstReceiptText(
                    Child(result, "className"),
                    Child(result, "class_name"),
                    Child(result, "role"),
                    Child(receipt, "className"),
                    Child(receipt, "class_name"),
                    Child(receipt, "role"));
                if (!string.IsNullOrWhiteSpace(roleText))
                {
                    parts.Add($"Role: {HumanizeReceiptPhrase(roleText)}");
                }

                var rewardText = FormatRewardBundle(FirstDirectToken(
                    new[] { result, receipt, root },
                    "rewards",
                    "reward",
                    "resourceRewards",
                    "resource_rewards",
                    "gains",
                    "gain",
                    "resourceDelta",
                    "resource_delta"));
                if (!string.IsNullOrWhiteSpace(rewardText))
                {
                    parts.Add($"Rewards: {rewardText}");
                }

                var gearText = JoinDistinctReceiptParts(
                    FormatReceiptItem(FirstDirectToken(new[] { result, receipt, root }, "equippedItem", "equipped_item"), "equipped"),
                    FormatReceiptItem(FirstDirectToken(new[] { result, receipt, root }, "unequippedItem", "unequipped_item"), "returned"),
                    FormatReceiptItem(FirstDirectToken(new[] { result, receipt, root }, "swappedOutItem", "swapped_out_item"), "swapped out"));
                if (!string.IsNullOrWhiteSpace(gearText))
                {
                    parts.Add($"Gear: {gearText}");
                }

                var effectText = FormatEffectBundle(FirstDirectToken(
                    new[] { result, receipt, root },
                    "effects",
                    "effect",
                    "changes",
                    "impact",
                    "returnedItems",
                    "returned_items",
                    "itemsReturned",
                    "items_returned",
                    "equipmentReturned",
                    "equipment_returned",
                    "equippedItem",
                    "equipped_item",
                    "unequippedItem",
                    "unequipped_item",
                    "swappedOutItem",
                    "swapped_out_item",
                    "cityArmorySummary",
                    "city_armory_summary",
                    "rosterChange",
                    "roster_change"));
                if (!string.IsNullOrWhiteSpace(effectText))
                {
                    parts.Add($"Effects: {effectText}");
                }

                var summary = FirstReceiptText(
                    Child(receipt, "summary"),
                    Child(result, "summary"),
                    Child(root, "summary"),
                    Child(root, "message"),
                    Child(result, "message"));
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    parts.Add($"Summary: {summary}");
                }

                var readable = string.Join(" • ", parts.Where(part => !string.IsNullOrWhiteSpace(part)).Distinct());
                return string.IsNullOrWhiteSpace(readable)
                    ? $"{fallback}. Backend returned no readable {noun.ToLowerInvariant()} roster receipt."
                    : $"{action}. {readable}";
            }
            catch
            {
                return $"{fallback}. Raw receipt: {responseJson.Trim()}";
            }
        }

        public static string ExtractHeroActionTitle(string responseJson, string fallbackAction, string subjectNoun)
        {
            if (string.IsNullOrWhiteSpace(responseJson))
            {
                return string.IsNullOrWhiteSpace(fallbackAction) ? string.Empty : fallbackAction.Trim();
            }

            try
            {
                var root = JToken.Parse(responseJson);
                var result = FirstDirectToken(new[] { root }, "result", "hero", "operative", "recruitment", "release", "data");
                if (result == null || result.Type != JTokenType.Object)
                {
                    result = root;
                }

                var receipt = FirstDirectToken(new[] { result, root }, "receipt", "heroReceipt", "hero_receipt", "operativeReceipt", "operative_receipt", "rosterReceipt", "roster_receipt");
                var name = FirstReceiptText(
                    Child(receipt, "heroName"),
                    Child(receipt, "hero_name"),
                    Child(receipt, "operativeName"),
                    Child(receipt, "operative_name"),
                    Child(receipt, "candidateName"),
                    Child(receipt, "candidate_name"),
                    Child(result, "displayName"),
                    Child(result, "display_name"),
                    Child(result, "name"),
                    Child(root, "displayName"),
                    Child(root, "display_name"),
                    Child(root, "name"));

                if (!string.IsNullOrWhiteSpace(name))
                {
                    var noun = string.IsNullOrWhiteSpace(subjectNoun) ? "Hero" : subjectNoun.Trim();
                    return $"{noun}: {name.Trim()}";
                }

                return string.IsNullOrWhiteSpace(fallbackAction) ? string.Empty : fallbackAction.Trim();
            }
            catch
            {
                return string.IsNullOrWhiteSpace(fallbackAction) ? string.Empty : fallbackAction.Trim();
            }
        }


        public static string FormatMissionCompletionReceipt(string responseJson, string fallbackInstanceId)
        {
            var fallbackLabel = fallbackInstanceId?.Trim() ?? string.Empty;
            var fallback = string.IsNullOrWhiteSpace(fallbackLabel)
                ? "Mission completed."
                : $"Mission completed: {fallbackLabel}";

            if (string.IsNullOrWhiteSpace(responseJson))
            {
                return $"{fallback} No completion receipt was returned.";
            }

            try
            {
                var root = JToken.Parse(responseJson);
                var result = FirstDirectToken(new[] { root }, "result", "completion", "data");
                if (result == null || result.Type != JTokenType.Object)
                {
                    result = root;
                }

                var receipt = FirstDirectToken(new[] { result, root }, "receipt", "missionReceipt", "mission_receipt");
                if (receipt == null || receipt.Type != JTokenType.Object)
                {
                    receipt = null;
                }

                var parts = new List<string>();
                var missionTitle = FirstReceiptText(
                    Child(receipt, "missionTitle"),
                    Child(receipt, "mission_title"),
                    Child(result, "missionTitle"),
                    Child(result, "mission_title"),
                    Child(root, "missionTitle"),
                    Child(root, "mission_title"));
                if (string.IsNullOrWhiteSpace(missionTitle))
                {
                    missionTitle = fallbackLabel;
                }

                if (!string.IsNullOrWhiteSpace(missionTitle))
                {
                    parts.Add($"Mission: {missionTitle}");
                }

                var outcome = FirstReceiptText(
                    Child(Child(result, "outcome"), "kind"),
                    Child(receipt, "outcome"),
                    Child(root, "outcome"));
                if (!string.IsNullOrWhiteSpace(outcome))
                {
                    parts.Add($"Outcome: {HumanizeReceiptPhrase(outcome)}");
                }
                else
                {
                    var status = FirstReceiptText(
                        Child(receipt, "status"),
                        Child(result, "status"),
                        Child(root, "status"));
                    var normalizedStatus = NormalizeCompletionStatus(status, Child(root, "ok"));
                    if (!string.IsNullOrWhiteSpace(normalizedStatus))
                    {
                        parts.Add($"Status: {normalizedStatus}");
                    }
                }

                var rewardText = FormatRewardBundle(FirstDirectToken(
                    new[] { result, root },
                    "rewards",
                    "reward",
                    "resourceRewards",
                    "resource_rewards",
                    "gains",
                    "gain",
                    "resourceDelta",
                    "resource_delta",
                    "resourceDeltas",
                    "resource_deltas"));
                parts.Add(string.IsNullOrWhiteSpace(rewardText)
                    ? "Rewards: no direct resource reward returned"
                    : $"Rewards: {rewardText}");

                var effectText = FormatEffectBundle(FirstDirectToken(
                    new[] { result, root },
                    "effects",
                    "effect",
                    "changes",
                    "impact",
                    "impacts",
                    "payoff",
                    "regionImpact",
                    "region_impact"));
                var flatEffectText = FormatFlatEffectDeltas(result, root);
                effectText = JoinDistinctReceiptParts(effectText, flatEffectText);
                if (!string.IsNullOrWhiteSpace(effectText))
                {
                    parts.Add($"Effects: {effectText}");
                }

                var summary = FirstReceiptText(
                    Child(receipt, "summary"),
                    Child(result, "summary"),
                    Child(root, "summary"),
                    Child(root, "message"));
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    parts.Add($"Summary: {summary}");
                }

                var setbackText = FormatSetbacks(FirstDirectToken(new[] { receipt, result, root }, "setbacks", "setback"));
                if (!string.IsNullOrWhiteSpace(setbackText))
                {
                    parts.Add($"Setbacks: {setbackText}");
                }

                var followupText = FormatOfferTitles(FirstDirectToken(new[] { root, result }, "followupOffers", "followup_offers"));
                if (!string.IsNullOrWhiteSpace(followupText))
                {
                    parts.Add($"Follow-up: {followupText}");
                }

                var recoveryText = FormatOfferTitles(FirstDirectToken(new[] { root, result }, "recoveryOffers", "recovery_offers"));
                if (!string.IsNullOrWhiteSpace(recoveryText))
                {
                    parts.Add($"Recovery: {recoveryText}");
                }

                var readable = string.Join(" • ", parts.Where(part => !string.IsNullOrWhiteSpace(part)).Distinct());
                return string.IsNullOrWhiteSpace(readable)
                    ? $"{fallback} Backend returned no readable reward/effect receipt."
                    : $"Mission completed. {readable}";
            }
            catch
            {
                return $"{fallback} Raw receipt: {responseJson.Trim()}";
            }
        }

        public static string ExtractMissionCompletionTitle(string responseJson)
        {
            if (string.IsNullOrWhiteSpace(responseJson))
            {
                return string.Empty;
            }

            try
            {
                var root = JToken.Parse(responseJson);
                var result = FirstDirectToken(new[] { root }, "result", "completion", "data");
                if (result == null || result.Type != JTokenType.Object)
                {
                    result = root;
                }

                var receipt = FirstDirectToken(new[] { result, root }, "receipt", "missionReceipt", "mission_receipt");
                var title = FirstReceiptText(
                    Child(receipt, "missionTitle"),
                    Child(receipt, "mission_title"),
                    Child(result, "missionTitle"),
                    Child(result, "mission_title"),
                    Child(root, "missionTitle"),
                    Child(root, "mission_title"));
                return title?.Trim() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static JToken Child(JToken root, string name)
        {
            if (root == null || root.Type != JTokenType.Object || string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var direct = root[name];
            return IsMeaningfulToken(direct) ? direct : null;
        }

        private static JToken FirstDirectToken(IEnumerable<JToken> roots, params string[] names)
        {
            if (roots == null || names == null)
            {
                return null;
            }

            foreach (var root in roots)
            {
                if (root == null || root.Type != JTokenType.Object)
                {
                    continue;
                }

                foreach (var name in names)
                {
                    var direct = Child(root, name);
                    if (IsMeaningfulToken(direct))
                    {
                        return direct;
                    }
                }
            }

            return null;
        }

        private static string FirstReceiptText(params JToken[] tokens)
        {
            if (tokens == null)
            {
                return string.Empty;
            }

            foreach (var token in tokens)
            {
                if (!IsMeaningfulToken(token))
                {
                    continue;
                }

                if (token.Type == JTokenType.String || token.Type == JTokenType.Boolean || token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
                {
                    return token.ToString().Trim();
                }

                var summary = Child(token, "summary") ?? Child(token, "message") ?? Child(token, "detail");
                if (IsMeaningfulToken(summary))
                {
                    return summary.ToString().Trim();
                }
            }

            return string.Empty;
        }

        private static string FormatRewardBundle(JToken token)
        {
            if (!IsMeaningfulToken(token))
            {
                return string.Empty;
            }

            if (token.Type == JTokenType.Array)
            {
                return string.Join(", ", token.Children().Select(FormatRewardBundle).Where(part => !string.IsNullOrWhiteSpace(part)));
            }

            if (token.Type != JTokenType.Object)
            {
                return FormatReceiptToken(token);
            }

            var pieces = new List<string>();
            foreach (var property in token.Children<JProperty>())
            {
                if (property == null || IsReceiptIdentityKey(property.Name) || !IsMeaningfulToken(property.Value))
                {
                    continue;
                }

                if (property.Value.Type == JTokenType.Integer || property.Value.Type == JTokenType.Float)
                {
                    var amount = property.Value.Value<double>();
                    if (Math.Abs(amount) < 0.0001)
                    {
                        continue;
                    }

                    pieces.Add($"{HumanizeReceiptKey(property.Name)} {FormatSignedNumber(amount)}");
                    continue;
                }

                var nested = FormatRewardBundle(property.Value);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    pieces.Add($"{HumanizeReceiptKey(property.Name)} {nested}");
                }
            }

            return string.Join(", ", pieces);
        }

        private static string FormatReceiptItem(JToken token, string verb)
        {
            if (!IsMeaningfulToken(token)) return string.Empty;
            if (token.Type != JTokenType.Object) return FirstReceiptText(token);
            var name = FirstReceiptText(
                Child(token, "name"),
                Child(token, "itemName"),
                Child(token, "item_name"),
                Child(Child(token, "template"), "name"),
                Child(token, "itemId"),
                Child(token, "item_id"));
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            var slot = FirstReceiptText(Child(token, "slot"), Child(Child(token, "template"), "slot"));
            var qtyToken = Child(token, "qty") ?? Child(token, "quantity");
            double? qty = null;
            if (qtyToken != null && (qtyToken.Type == JTokenType.Integer || qtyToken.Type == JTokenType.Float))
            {
                qty = qtyToken.Value<double>();
            }
            var pieces = new List<string> { $"{HumanizeReceiptPhrase(verb)} {name}" };
            if (!string.IsNullOrWhiteSpace(slot)) pieces.Add(slot);
            if (qty.HasValue && Math.Abs(qty.Value - 1) > 0.0001) pieces.Add($"x{qty.Value:0.##}");
            return string.Join(" ", pieces);
        }

        private static string FormatEffectBundle(JToken token)
        {
            if (!IsMeaningfulToken(token))
            {
                return string.Empty;
            }

            if (token.Type == JTokenType.Array)
            {
                return string.Join(", ", token.Children().Select(FormatEffectBundle).Where(part => !string.IsNullOrWhiteSpace(part)));
            }

            if (token.Type != JTokenType.Object)
            {
                return FormatReceiptToken(token);
            }

            var pieces = new List<string>();
            foreach (var property in token.Children<JProperty>())
            {
                if (property == null || IsReceiptIdentityKey(property.Name) || !IsMeaningfulToken(property.Value))
                {
                    continue;
                }

                if (property.Value.Type == JTokenType.Integer || property.Value.Type == JTokenType.Float)
                {
                    var amount = property.Value.Value<double>();
                    if (Math.Abs(amount) < 0.0001)
                    {
                        continue;
                    }

                    pieces.Add($"{HumanizeEffectKey(property.Name)} {FormatSignedNumber(amount)}");
                    continue;
                }

                var nested = FormatEffectBundle(property.Value);
                if (!string.IsNullOrWhiteSpace(nested))
                {
                    pieces.Add($"{HumanizeEffectKey(property.Name)} {nested}");
                }
            }

            return string.Join(", ", pieces);
        }

        private static string FormatFlatEffectDeltas(params JToken[] roots)
        {
            var pieces = new List<string>();
            foreach (var root in roots ?? Array.Empty<JToken>())
            {
                AddFlatNumericEffect(pieces, root, "controlDelta", "Control");
                AddFlatNumericEffect(pieces, root, "control_delta", "Control");
                AddFlatNumericEffect(pieces, root, "threatDelta", "Threat");
                AddFlatNumericEffect(pieces, root, "threat_delta", "Threat");
                AddFlatNumericEffect(pieces, root, "pressureDelta", "Pressure");
                AddFlatNumericEffect(pieces, root, "pressure_delta", "Pressure");
                AddFlatNumericEffect(pieces, root, "recoveryDelta", "Recovery burden");
                AddFlatNumericEffect(pieces, root, "recovery_delta", "Recovery burden");
            }

            return string.Join(", ", pieces.Distinct());
        }

        private static void AddFlatNumericEffect(List<string> pieces, JToken root, string key, string label)
        {
            var token = Child(root, key);
            if (!IsMeaningfulToken(token) || (token.Type != JTokenType.Integer && token.Type != JTokenType.Float))
            {
                return;
            }

            var amount = token.Value<double>();
            if (Math.Abs(amount) < 0.0001)
            {
                return;
            }

            pieces.Add($"{label} {FormatSignedNumber(amount)}");
        }

        private static string FormatSetbacks(JToken token)
        {
            if (!IsMeaningfulToken(token))
            {
                return string.Empty;
            }

            if (token.Type == JTokenType.Array)
            {
                var summaries = token.Children()
                    .Select(item => FirstReceiptText(Child(item, "summary"), Child(item, "detail"), item))
                    .Where(text => !string.IsNullOrWhiteSpace(text))
                    .Take(3)
                    .ToList();
                return summaries.Count == 0 ? string.Empty : string.Join("; ", summaries);
            }

            return FirstReceiptText(Child(token, "summary"), Child(token, "detail"), token);
        }

        private static string FormatOfferTitles(JToken token)
        {
            if (!IsMeaningfulToken(token))
            {
                return string.Empty;
            }

            if (token.Type == JTokenType.Array)
            {
                var titles = token.Children()
                    .Select(item => FirstReceiptText(Child(item, "title"), Child(item, "name"), Child(item, "summary")))
                    .Where(text => !string.IsNullOrWhiteSpace(text))
                    .Take(3)
                    .ToList();
                return titles.Count == 0 ? string.Empty : string.Join(", ", titles);
            }

            return FirstReceiptText(Child(token, "title"), Child(token, "name"), Child(token, "summary"), token);
        }

        private static string NormalizeCompletionStatus(string status, JToken okToken)
        {
            var trimmed = status?.Trim() ?? string.Empty;
            if (string.Equals(trimmed, "ok", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "success", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "accepted", StringComparison.OrdinalIgnoreCase))
            {
                return "Completion accepted";
            }

            if (string.Equals(trimmed, "complete", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(trimmed, "completed", StringComparison.OrdinalIgnoreCase))
            {
                return "Completed";
            }

            if (!string.IsNullOrWhiteSpace(trimmed))
            {
                return HumanizeReceiptPhrase(trimmed);
            }

            if (okToken != null && okToken.Type == JTokenType.Boolean && okToken.Value<bool>())
            {
                return "Completion accepted";
            }

            return string.Empty;
        }

        private static string ResolveKnownBuildingKindLabel(string kind)
        {
            var normalized = NormalizeReceiptIdentity(kind);
            return normalized switch
            {
                "housing" => "Charter Ward",
                "farmland" => "Granary Fields",
                "mine" => "Works Quarry",
                "arcanespire" => "Beacon Tower",
                "hallofrecords" => "Hall of Records",
                "watchbarracks" => "Watch Barracks",
                "provincialoffice" => "Provincial Office",
                "safehouse" => "Safehouse Ring",
                "quietprovisioning" => "Quiet Provisioning Cell",
                "illicitextraction" => "Illicit Extraction Cell",
                "occultrelay" => "Occult Relay",
                "fronthouse" => "Front House",
                "debthouse" => "Debt House",
                "cutoutbureau" => "Cutout Bureau",
                _ => string.Empty,
            };
        }

        private static string NormalizeBuildingRoutingPreference(string routingPreference)
        {
            var value = routingPreference?.Trim() ?? string.Empty;
            if (value.Equals("prefer_local", StringComparison.OrdinalIgnoreCase) || value.Equals("local", StringComparison.OrdinalIgnoreCase)) return "prefer_local";
            if (value.Equals("prefer_reserve", StringComparison.OrdinalIgnoreCase) || value.Equals("reserve", StringComparison.OrdinalIgnoreCase) || value.Equals("protected_reserve", StringComparison.OrdinalIgnoreCase)) return "prefer_reserve";
            if (value.Equals("prefer_exchange", StringComparison.OrdinalIgnoreCase) || value.Equals("exchange", StringComparison.OrdinalIgnoreCase)) return "prefer_exchange";
            return "balanced";
        }

        private static bool SameId(string left, string right)
        {
            return !string.IsNullOrWhiteSpace(left)
                && !string.IsNullOrWhiteSpace(right)
                && string.Equals(left.Trim(), right.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        private static string FirstNonBlankText(params string[] values)
        {
            return values?.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;
        }

        private static string FirstUsableReceiptLabel(string id, string fallbackNoun, params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            foreach (var value in values)
            {
                var trimmed = value?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(trimmed)
                    && !LooksLikePlaceholderLabel(trimmed, fallbackNoun)
                    && !LooksLikeSameIdentity(trimmed, id))
                {
                    return trimmed;
                }
            }

            return FirstNonBlankText(values);
        }

        private static string BuildReceiptLabel(string id, string preferredLabel, string fallbackNoun)
        {
            var trimmedId = id?.Trim() ?? string.Empty;
            var trimmedLabel = preferredLabel?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(trimmedLabel)
                && !LooksLikePlaceholderLabel(trimmedLabel, fallbackNoun)
                && !LooksLikeSameIdentity(trimmedLabel, trimmedId))
            {
                return trimmedLabel;
            }

            if (!string.IsNullOrWhiteSpace(trimmedId))
            {
                return HumanizeReceiptIdentifier(trimmedId);
            }

            return string.Empty;
        }

        private static bool LooksLikePlaceholderLabel(string label, string fallbackNoun)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return true;
            }

            var normalized = NormalizeReceiptIdentity(label);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return true;
            }

            var noun = NormalizeReceiptIdentity(fallbackNoun);
            return normalized == noun
                || normalized == "army"
                || normalized == "hero"
                || normalized == "mission"
                || normalized == "operation"
                || normalized == "formation";
        }

        private static bool LooksLikeSameIdentity(string label, string id)
        {
            if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            return NormalizeReceiptIdentity(label) == NormalizeReceiptIdentity(id);
        }

        private static string NormalizeReceiptIdentity(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var chars = value.Trim()
                .Where(c => !char.IsWhiteSpace(c) && c != '_' && c != '-' && c != ':' && c != '.' && c != '/')
                .Select(char.ToLowerInvariant)
                .ToArray();
            return new string(chars);
        }

        private static string HumanizeReceiptIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var cleaned = value.Trim().Replace('_', ' ').Replace('-', ' ').Replace(':', ' ').Replace('/', ' ');
            var words = cleaned.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
            {
                return string.Empty;
            }

            return string.Join(" ", words.Select(word =>
            {
                if (word.All(char.IsUpper) || word.All(char.IsDigit))
                {
                    return word;
                }

                var lower = word.ToLowerInvariant();
                return char.ToUpperInvariant(lower[0]) + (lower.Length > 1 ? lower.Substring(1) : string.Empty);
            }));
        }

        private static string JoinDistinctReceiptParts(params string[] parts)
        {
            return string.Join(", ", (parts ?? Array.Empty<string>())
                .Where(part => !string.IsNullOrWhiteSpace(part))
                .Select(part => part.Trim())
                .Distinct());
        }

        private static string FormatSignedNumber(double value)
        {
            return value > 0 ? $"+{value:0.##}" : value.ToString("0.##");
        }

        private static string HumanizeEffectKey(string key)
        {
            var label = HumanizeReceiptKey(key);
            if (string.IsNullOrWhiteSpace(label))
            {
                return string.Empty;
            }

            return label.EndsWith(" delta", StringComparison.OrdinalIgnoreCase)
                ? label.Substring(0, label.Length - " delta".Length)
                : label;
        }

        private static string HumanizeReceiptPhrase(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var phrase = text.Trim().Replace("_", " ").Replace("-", " ");
            return phrase.Length == 0
                ? string.Empty
                : char.ToUpperInvariant(phrase[0]) + (phrase.Length > 1 ? phrase.Substring(1) : string.Empty);
        }

        private static string FormatReceiptToken(JToken token)
        {
            if (!IsMeaningfulToken(token))
            {
                return string.Empty;
            }

            if (token.Type == JTokenType.String || token.Type == JTokenType.Boolean || token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
            {
                return FormatScalarToken(token);
            }

            if (token.Type == JTokenType.Array)
            {
                return string.Join(", ", token.Children().Select(FormatReceiptToken).Where(part => !string.IsNullOrWhiteSpace(part)));
            }

            if (token.Type != JTokenType.Object)
            {
                return token.ToString(Formatting.None);
            }

            var pieces = new List<string>();
            foreach (var property in token.Children<JProperty>())
            {
                if (property == null || IsReceiptIdentityKey(property.Name) || !IsMeaningfulToken(property.Value))
                {
                    continue;
                }

                var valueText = FormatReceiptToken(property.Value);
                if (string.IsNullOrWhiteSpace(valueText))
                {
                    continue;
                }

                pieces.Add($"{HumanizeReceiptKey(property.Name)} {valueText}");
            }

            return string.Join(", ", pieces);
        }

        private static string FormatScalarToken(JToken token)
        {
            if (token == null)
            {
                return string.Empty;
            }

            if (token.Type == JTokenType.Integer || token.Type == JTokenType.Float)
            {
                var value = token.Value<double>();
                return FormatSignedNumber(value);
            }

            return token.ToString().Trim();
        }

        private static bool IsMeaningfulToken(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
            {
                return false;
            }

            if (token.Type == JTokenType.String)
            {
                return !string.IsNullOrWhiteSpace(token.ToString());
            }

            if (token.Type == JTokenType.Array)
            {
                return token.Children().Any(IsMeaningfulToken);
            }

            if (token.Type == JTokenType.Object)
            {
                return token.Children<JProperty>().Any(property => property != null && IsMeaningfulToken(property.Value));
            }

            return true;
        }

        private static bool IsReceiptIdentityKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return true;
            }

            var normalized = key.Trim().Replace("_", "").Replace("-", "").ToLowerInvariant();
            return normalized == "id"
                || normalized == "missionid"
                || normalized == "missiontitle"
                || normalized == "instanceid"
                || normalized == "createdat"
                || normalized == "ok"
                || normalized == "status"
                || normalized == "success";
        }

        private static string HumanizeReceiptKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            var spaced = key.Trim().Replace("_", " ").Replace("-", " ");
            var chars = new System.Text.StringBuilder();
            for (var i = 0; i < spaced.Length; i++)
            {
                var c = spaced[i];
                if (i > 0 && char.IsUpper(c) && !char.IsWhiteSpace(spaced[i - 1]))
                {
                    chars.Append(' ');
                }
                chars.Append(c);
            }

            return chars.ToString().Trim().ToLowerInvariant();
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

        public void BeginHeroRelease(string heroId)
        {
            BeginAction(string.IsNullOrWhiteSpace(heroId) ? "Releasing hero..." : $"Releasing hero: {heroId.Trim()}");
            PendingHeroReleaseId = heroId?.Trim() ?? string.Empty;
        }

        public void BeginHeroEquipFromArmory(string heroId, int armorySlotIndex)
        {
            BeginAction(string.IsNullOrWhiteSpace(heroId)
                ? $"Equipping shared gear from armory slot {armorySlotIndex}..."
                : $"Equipping shared gear: {heroId.Trim()} <- armory slot {armorySlotIndex}");
            PendingHeroReleaseId = heroId?.Trim() ?? string.Empty;
        }

        public void BeginHeroUnequipToArmory(string heroId, string slot)
        {
            BeginAction(string.IsNullOrWhiteSpace(heroId)
                ? $"Returning shared gear from {slot}..."
                : $"Returning shared gear: {heroId.Trim()} {slot}");
            PendingHeroReleaseId = heroId?.Trim() ?? string.Empty;
        }

        public void BeginArmyReinforcement(string armyId)
        {
            var label = ResolveArmyReceiptLabel(armyId);
            BeginAction(string.IsNullOrWhiteSpace(label) ? "Reinforcing army..." : $"Reinforcing army: {label}");
            PendingArmyReinforcementId = armyId?.Trim() ?? string.Empty;
        }

        public void BeginArmyRename(string armyId, string requestedName)
        {
            var armyLabel = ResolveArmyReceiptLabel(armyId);
            var label = string.IsNullOrWhiteSpace(armyLabel) ? "Renaming formation..." : $"Renaming formation: {armyLabel}";
            if (!string.IsNullOrWhiteSpace(requestedName))
            {
                label += $" -> {requestedName.Trim()}";
            }

            BeginAction(label);
        }

        public void BeginArmySplit(string armyId, int requestedSize, string requestedName)
        {
            var armyLabel = ResolveArmyReceiptLabel(armyId);
            var label = string.IsNullOrWhiteSpace(armyLabel)
                ? $"Splitting formation ({requestedSize})..."
                : $"Splitting formation: {armyLabel} ({requestedSize})";
            if (!string.IsNullOrWhiteSpace(requestedName))
            {
                label += $" -> {requestedName.Trim()}";
            }

            BeginAction(label);
        }

        public void BeginArmyMerge(string sourceArmyId, string targetArmyId)
        {
            var sourceLabel = ResolveArmyReceiptLabel(sourceArmyId);
            var targetLabel = ResolveArmyReceiptLabel(targetArmyId);
            var label = string.IsNullOrWhiteSpace(sourceLabel)
                ? "Merging formation..."
                : $"Merging formation: {sourceLabel}";
            if (!string.IsNullOrWhiteSpace(targetLabel))
            {
                label += $" -> {targetLabel}";
            }

            BeginAction(label);
        }

        public void BeginArmyDisband(string armyId)
        {
            var armyLabel = ResolveArmyReceiptLabel(armyId);
            BeginAction(string.IsNullOrWhiteSpace(armyLabel)
                ? "Disbanding formation..."
                : $"Disbanding formation: {armyLabel}");
        }

        public void BeginArmyHoldAssign(string armyId, string regionId, string posture)
        {
            var armyLabel = ResolveArmyReceiptLabel(armyId);
            var regionLabel = ResolveRegionReceiptLabel(regionId);
            var postureLabel = ResolvePostureReceiptLabel(posture);
            var label = string.IsNullOrWhiteSpace(armyLabel)
                ? "Assigning regional hold..."
                : $"Assigning regional hold: {armyLabel}";
            if (!string.IsNullOrWhiteSpace(regionLabel))
            {
                label += $" -> {regionLabel}";
            }

            if (!string.IsNullOrWhiteSpace(postureLabel))
            {
                label += $" ({postureLabel})";
            }

            BeginAction(label);
        }

        public void BeginArmyHoldRelease(string armyId)
        {
            var armyLabel = ResolveArmyReceiptLabel(armyId);
            BeginAction(string.IsNullOrWhiteSpace(armyLabel)
                ? "Releasing regional hold..."
                : $"Releasing regional hold: {armyLabel}");
        }

        public void BeginFrontlineDispatch(string actionLabel, string regionId, string armyId, string heroId = null)
        {
            var regionLabel = ResolveRegionReceiptLabel(regionId);
            var armyLabel = ResolveArmyReceiptLabel(armyId);
            var heroLabel = ResolveHeroReceiptLabel(heroId);
            var label = string.IsNullOrWhiteSpace(actionLabel) ? "Dispatching frontline action..." : $"Dispatching {actionLabel.Trim()}";
            if (!string.IsNullOrWhiteSpace(regionLabel))
            {
                label += $" -> {regionLabel}";
            }
            if (!string.IsNullOrWhiteSpace(armyLabel))
            {
                label += $" with {armyLabel}";
            }
            if (!string.IsNullOrWhiteSpace(heroLabel))
            {
                label += $" under {heroLabel}";
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
            PendingMissionOfferId = string.Empty;
            PendingMissionInstanceId = string.Empty;
            PendingHeroRecruitRole = string.Empty;
            PendingHeroRecruitCandidateId = string.Empty;
            PendingHeroRecruitDismiss = false;
            PendingHeroReleaseId = string.Empty;
            PendingArmyReinforcementId = string.Empty;
            PendingBuildingKind = string.Empty;
            PendingBuildingId = string.Empty;
            PendingBuildingRoutingPreference = string.Empty;
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
            PendingMissionOfferId = string.Empty;
            PendingMissionInstanceId = string.Empty;
            PendingHeroRecruitRole = string.Empty;
            PendingHeroRecruitCandidateId = string.Empty;
            PendingHeroRecruitDismiss = false;
            PendingHeroReleaseId = string.Empty;
            PendingArmyReinforcementId = string.Empty;
            PendingBuildingKind = string.Empty;
            PendingBuildingId = string.Empty;
            PendingBuildingRoutingPreference = string.Empty;
            ActionStatus = status;
            Changed?.Invoke();
        }
    }
}
