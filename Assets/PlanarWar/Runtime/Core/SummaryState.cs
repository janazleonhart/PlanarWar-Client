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

        public void BeginBuildingConstruct(string kind)
        {
            BeginAction(string.IsNullOrWhiteSpace(kind) ? "Starting construction..." : $"Starting construction: {kind.Trim()}");
            PendingBuildingKind = kind?.Trim() ?? string.Empty;
        }

        public void BeginBuildingUpgrade(string buildingId)
        {
            BeginAction(string.IsNullOrWhiteSpace(buildingId) ? "Starting building upgrade..." : $"Starting building upgrade: {buildingId.Trim()}");
            PendingBuildingId = buildingId?.Trim() ?? string.Empty;
        }

        public void BeginBuildingRouting(string buildingId, string routingPreference)
        {
            BeginAction(string.IsNullOrWhiteSpace(buildingId) ? "Switching building routing..." : $"Switching building routing: {buildingId.Trim()}");
            PendingBuildingId = buildingId?.Trim() ?? string.Empty;
            PendingBuildingRoutingPreference = routingPreference?.Trim() ?? string.Empty;
        }

        public void BeginBuildingDestroy(string buildingId, bool confirming = false)
        {
            BeginAction(string.IsNullOrWhiteSpace(buildingId)
                ? (confirming ? "Confirming demolition..." : "Requesting demolition confirmation...")
                : confirming ? $"Confirming demolition: {buildingId.Trim()}" : $"Requesting demolition confirmation: {buildingId.Trim()}");
            PendingBuildingId = buildingId?.Trim() ?? string.Empty;
        }

        public void BeginBuildingRemodel(string buildingId, string targetKind, bool confirming = false)
        {
            var label = string.IsNullOrWhiteSpace(buildingId)
                ? (confirming ? "Confirming remodel..." : "Requesting remodel confirmation...")
                : confirming ? $"Confirming remodel: {buildingId.Trim()}" : $"Requesting remodel confirmation: {buildingId.Trim()}";
            if (!string.IsNullOrWhiteSpace(targetKind))
            {
                label += $" -> {targetKind.Trim()}";
            }

            BeginAction(label);
            PendingBuildingId = buildingId?.Trim() ?? string.Empty;
            PendingBuildingKind = targetKind?.Trim() ?? string.Empty;
        }

        public void BeginActiveBuildCancel(string activeBuildId, bool confirming = false)
        {
            BeginAction(string.IsNullOrWhiteSpace(activeBuildId)
                ? (confirming ? "Confirming build cancellation..." : "Requesting build cancellation confirmation...")
                : confirming ? $"Confirming build cancellation: {activeBuildId.Trim()}" : $"Requesting build cancellation confirmation: {activeBuildId.Trim()}");
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

        public void BeginMissionStartAction(string missionId)
        {
            BeginAction(string.IsNullOrWhiteSpace(missionId) ? "Starting mission..." : $"Starting mission: {missionId.Trim()}");
            PendingMissionOfferId = missionId?.Trim() ?? string.Empty;
        }

        public void BeginMissionCompleteAction(string instanceId)
        {
            BeginAction(string.IsNullOrWhiteSpace(instanceId) ? "Completing mission..." : $"Completing mission: {instanceId.Trim()}");
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


        public static string FormatMissionCompletionReceipt(string responseJson, string fallbackInstanceId)
        {
            var fallback = string.IsNullOrWhiteSpace(fallbackInstanceId)
                ? "Mission completed."
                : $"Mission completed: {fallbackInstanceId.Trim()}";

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
                var outcome = FirstReceiptText(
                    Child(Child(result, "outcome"), "kind"),
                    Child(receipt, "outcome"),
                    Child(root, "outcome"));
                if (!string.IsNullOrWhiteSpace(outcome))
                {
                    parts.Add($"Outcome: {HumanizeReceiptPhrase(outcome)}");
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
