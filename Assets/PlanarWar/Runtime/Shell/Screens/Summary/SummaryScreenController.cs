using PlanarWar.Client.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace PlanarWar.Client.UI.Screens.Summary
{
    public sealed class SummaryScreenController
    {
        private readonly Label statusHeadline;
        private readonly Label resources;
        private readonly Label production;
        private readonly Label research;
        private readonly Label warnings;
        private readonly Label readyOps;
        private readonly Label heroes;
        private readonly Label armies;
        private readonly Label researchTimer;
        private readonly Label workshopTimer;
        private readonly Label missionTimer;
        private readonly Label resourceTick;
        private readonly Label timerDiagNow;
        private readonly Label timerDiagHeartbeat;
        private readonly Label timerDiagRaw;
        private readonly Label timerDiagComputed;
        private readonly Label pressureDeskBadge;
        private readonly Label pressureDeskHeadline;
        private readonly Label pressureDeskDetail;
        private readonly Label pressureSeamTitle;
        private readonly Label pressureSeamValue;
        private readonly Label pressureSeamNote;
        private readonly Label pressureContractTitle;
        private readonly Label pressureContractValue;
        private readonly Label pressureContractNote;
        private readonly Label pressureAnswerTitle;
        private readonly Label pressureAnswerValue;
        private readonly Label pressureAnswerNote;
        private readonly Label pressureDemandTitle;
        private readonly Label pressureDemandValue;
        private readonly Label pressureDemandNote;
        private readonly VisualElement pressureOperationsStrip;
        private int heartbeat;

        public SummaryScreenController(VisualElement root)
        {
            statusHeadline = root.Q<Label>("status-headline-value");
            resources = root.Q<Label>("resources-value");
            production = root.Q<Label>("production-value");
            research = root.Q<Label>("research-value");
            warnings = root.Q<Label>("warnings-value");
            readyOps = root.Q<Label>("ready-ops-value");
            heroes = root.Q<Label>("hero-status-value");
            armies = root.Q<Label>("army-status-value");
            researchTimer = root.Q<Label>("research-timer-value");
            workshopTimer = root.Q<Label>("workshop-timer-value");
            missionTimer = root.Q<Label>("mission-timer-value");
            resourceTick = root.Q<Label>("resource-tick-value");
            timerDiagNow = root.Q<Label>("timer-diag-now-value");
            timerDiagHeartbeat = root.Q<Label>("timer-diag-heartbeat-value");
            timerDiagRaw = root.Q<Label>("timer-diag-raw-value");
            timerDiagComputed = root.Q<Label>("timer-diag-computed-value");
            pressureDeskBadge = root.Q<Label>("pressure-desk-badge-value");
            pressureDeskHeadline = root.Q<Label>("pressure-desk-headline-value");
            pressureDeskDetail = root.Q<Label>("pressure-desk-detail-value");
            pressureSeamTitle = root.Q<Label>("pressure-seam-title");
            pressureSeamValue = root.Q<Label>("pressure-seam-value");
            pressureSeamNote = root.Q<Label>("pressure-seam-note");
            pressureContractTitle = root.Q<Label>("pressure-contract-title");
            pressureContractValue = root.Q<Label>("pressure-contract-value");
            pressureContractNote = root.Q<Label>("pressure-contract-note");
            pressureAnswerTitle = root.Q<Label>("pressure-answer-title");
            pressureAnswerValue = root.Q<Label>("pressure-answer-value");
            pressureAnswerNote = root.Q<Label>("pressure-answer-note");
            pressureDemandTitle = root.Q<Label>("pressure-demand-title");
            pressureDemandValue = root.Q<Label>("pressure-demand-value");
            pressureDemandNote = root.Q<Label>("pressure-demand-note");
            pressureOperationsStrip = root.Q<VisualElement>("pressure-operations-strip");
        }

        public void Render(ShellSummarySnapshot s, bool isSummaryLoaded)
        {
            heartbeat++;
            var nowUtc = DateTime.UtcNow;

            statusHeadline.text = s.HasCity ? $"{s.City.Name} • {s.City.SettlementLaneLabel}" : (s.FounderMode ? "Founder mode active." : "No settlement loaded.");
            resources.text = FormatResource(s.Resources, "No resources loaded.");
            production.text = FormatResource(s.ProductionPerTick, s.HasCity ? "No production snapshot." : "Found a city to unlock production.", "/tick");
            research.text = s.ActiveResearch == null ? "No active research." : $"{s.ActiveResearch.Name} • {FormatProgress(s.ActiveResearch.Progress, s.ActiveResearch.Cost)}";
            warnings.text = s.ThreatWarnings.Count == 0 ? "No active threat warnings." : s.ThreatWarnings[0].Headline;
            readyOps.text = s.OpeningOperations.Count == 0 ? "No opening operations surfaced." : BuildReadyOpsSummary(s.OpeningOperations);
            heroes.text = s.Heroes.Count == 0 ? (s.HasCity ? "No officer corps visible." : "Found a city to unlock officers.") : $"{s.Heroes.Count(h => h.Status == "idle")}/{s.Heroes.Count} idle • {s.Heroes.Count(h => h.AttachmentCount > 0)} geared";
            armies.text = s.Armies.Count == 0 ? (s.HasCity ? "No formations visible." : "Found a city to unlock formations.") : $"{s.Armies.Count(a => (a.Readiness ?? 0) >= 70)}/{s.Armies.Count} ready";
            researchTimer.text = s.ActiveResearch?.StartedAtUtc == null ? "No active research timer." : $"Running {FormatRemaining(nowUtc - s.ActiveResearch.StartedAtUtc.Value)}";
            workshopTimer.text = FormatWorkshop(s.WorkshopJobs);
            missionTimer.text = FormatMission(s.ActiveMissions);
            resourceTick.text = FormatTick(s.ResourceTickTiming);
            timerDiagNow.text = $"Live UI clock {nowUtc:HH:mm:ss} UTC";
            timerDiagHeartbeat.text = $"Heartbeat #{heartbeat}";
            timerDiagRaw.text = FormatTimerRaw(s.ResourceTickTiming, isSummaryLoaded);
            timerDiagComputed.text = $"diag: {FormatTick(s.ResourceTickTiming)}";

            RenderPressureDesk(s);
        }

        private void RenderPressureDesk(ShellSummarySnapshot summary)
        {
            if (!summary.HasCity)
            {
                SetPressureDesk(
                    badge: summary.FounderMode ? "Founder mode" : "No settlement",
                    headline: "Pressure / contract desk is waiting on a real settlement snapshot.",
                    detail: "The command surface cannot tell you what seam is live, why the board is bending, or which answer lane is honest until /api/me has a live city or black-market payload.",
                    seamTitle: "Live seam",
                    seamValue: "No lane loaded.",
                    seamNote: "Found a settlement first so the main desk can bind pressure, contracts, and reply posture to something real.",
                    contractTitle: "Why now",
                    contractValue: "No contract pressure surfaced.",
                    contractNote: "Once a settlement exists, the board reason will stop hiding behind a generic ready-count.",
                    answerTitle: "Answer lane",
                    answerValue: "No lane truth yet.",
                    answerNote: "Civic desks will surface relief / repair / investigation. Shadow desks will surface covert / deniable / counterfeit answers.",
                    demandTitle: "Demand bend",
                    demandValue: "No supply signal yet.",
                    demandNote: "Production, reserve, and exchange drag need a live settlement payload before the desk can summarize them honestly.");
                RenderOperationStrip(summary, lane: null);
                return;
            }

            var lane = NormalizeLane(summary.City.SettlementLane);
            var primaryOp = SelectPrimaryOperation(summary.OpeningOperations);

            if (lane == "black_market")
            {
                RenderShadowPressureDesk(summary, primaryOp);
                RenderOperationStrip(summary, lane);
                return;
            }

            RenderCivicPressureDesk(summary, primaryOp);
            RenderOperationStrip(summary, lane);
        }

        private void RenderCivicPressureDesk(ShellSummarySnapshot summary, OperationSnapshot primaryOp)
        {
            var surface = summary.PublicBackbonePressureConvergence;
            var front = SelectPrimaryCivicFront(surface);
            var phase = HumanizeStage(MapCivicStage(surface, front));
            var continuity = front != null ? $"{front.Label} • {front.Id}" : "Public backbone continuity";
            var recommended = FirstNonBlank(primaryOp?.WhyNow, front?.RecommendedAction, surface?.RecommendedAction, "No civic why-now reason surfaced.");
            var deskHeadline = FirstNonBlank(primaryOp?.FocusLabel, front?.Headline, surface?.Headline, primaryOp?.Title, "Civic pressure desk is quiet enough to stay backgrounded.");
            var deskDetail = FirstNonBlank(primaryOp?.Summary, surface?.Detail, front?.Summary, primaryOp?.Detail, "No civic convergence detail surfaced.");
            var answerValue = BuildCivicAnswerValue(front, primaryOp);
            var answerNote = BuildCivicAnswerNote(surface, front, primaryOp);
            var demandValue = BuildCivicDemandValue(surface);
            var demandNote = BuildCivicDemandNote(surface, primaryOp);
            var contractValue = FirstNonBlank(primaryOp?.Title, primaryOp?.FocusLabel, "No civic contract is currently leading.");
            var contractNote = FirstNonBlank(primaryOp?.WhyNow, primaryOp?.Payoff, recommended);

            SetPressureDesk(
                badge: $"City seam • {phase}",
                headline: deskHeadline,
                detail: deskDetail,
                seamTitle: "Live seam",
                seamValue: continuity,
                seamNote: front != null
                    ? $"{phase} • {FirstNonBlank(front.Headline, front.Summary)}"
                    : FirstNonBlank(surface?.Headline, "No explicit civic seam card surfaced."),
                contractTitle: "Why this board moved",
                contractValue: contractValue,
                contractNote: contractNote,
                answerTitle: "Answer lane",
                answerValue: answerValue,
                answerNote: answerNote,
                demandTitle: "Supply / reserve bend",
                demandValue: demandValue,
                demandNote: demandNote);
        }

        private void RenderShadowPressureDesk(ShellSummarySnapshot summary, OperationSnapshot primaryOp)
        {
            var runtime = summary.BlackMarketRuntimeTruth;
            var active = summary.BlackMarketActiveOperation;
            var payoff = summary.BlackMarketPayoffRecovery;
            var card = SelectPrimaryShadowCard(active);
            var stage = HumanizeStage(MapShadowStage(card, runtime, payoff));
            var continuity = card != null ? $"{HumanizeShadowKind(card.Kind)} • {card.Id}" : "Shadow runtime continuity";
            var deskHeadline = FirstNonBlank(primaryOp?.FocusLabel, card?.Headline, runtime?.Headline, payoff?.Headline, primaryOp?.Title, "Shadow pressure desk is quiet enough to stay backgrounded.");
            var deskDetail = FirstNonBlank(primaryOp?.Summary, runtime?.Detail, card?.Summary, payoff?.Detail, primaryOp?.Detail, "No shadow convergence detail surfaced.");
            var contractValue = FirstNonBlank(primaryOp?.Title, primaryOp?.FocusLabel, "No shadow contract is currently leading.");
            var contractNote = FirstNonBlank(primaryOp?.WhyNow, runtime?.ActiveOperation?.Detail, payoff?.RecommendedAction, card?.OperatorNote, "No shadow why-now reason surfaced.");
            var answerValue = BuildShadowAnswerValue(card, primaryOp);
            var answerNote = BuildShadowAnswerNote(runtime, active, card);
            var demandValue = BuildShadowDemandValue(runtime, payoff);
            var demandNote = BuildShadowDemandNote(runtime, payoff, primaryOp);

            SetPressureDesk(
                badge: $"Shadow seam • {stage}",
                headline: deskHeadline,
                detail: deskDetail,
                seamTitle: "Live seam",
                seamValue: continuity,
                seamNote: card != null
                    ? $"{stage} • {FirstNonBlank(card.Summary, card.OperatorNote)}"
                    : FirstNonBlank(runtime?.Headline, payoff?.Headline, "No explicit shadow seam card surfaced."),
                contractTitle: "Why this board moved",
                contractValue: contractValue,
                contractNote: contractNote,
                answerTitle: "Answer lane",
                answerValue: answerValue,
                answerNote: answerNote,
                demandTitle: "Pressure / payoff bend",
                demandValue: demandValue,
                demandNote: demandNote);
        }

        private void RenderOperationStrip(ShellSummarySnapshot summary, string lane)
        {
            if (pressureOperationsStrip == null)
            {
                return;
            }

            pressureOperationsStrip.Clear();

            var operations = SelectOperationStrip(summary?.OpeningOperations, lane);
            if (operations.Count == 0)
            {
                pressureOperationsStrip.Add(BuildOperationEmptyCard(summary, lane));
                return;
            }

            for (var index = 0; index < operations.Count; index++)
            {
                pressureOperationsStrip.Add(BuildOperationCard(operations[index], index, summary, lane));
            }
        }

        private VisualElement BuildOperationEmptyCard(ShellSummarySnapshot summary, string lane)
        {
            var card = new VisualElement();
            card.AddToClassList("summary-card");
            card.AddToClassList("pressure-op-card");
            card.AddToClassList("pressure-op-card--empty");

            var eyebrow = new Label("Fast options");
            eyebrow.AddToClassList("eyebrow");
            card.Add(eyebrow);

            var title = new Label(string.IsNullOrWhiteSpace(lane) ? "No operation strip yet." : $"No {HumanizeWords(lane, "lane").ToLowerInvariant()} options surfaced.");
            title.AddToClassList("rail-note-title");
            card.Add(title);

            var detail = new Label(BuildOperationEmptyNote(summary, lane));
            detail.AddToClassList("metric-subvalue");
            detail.AddToClassList("metric-subvalue--wrap");
            card.Add(detail);

            var cta = new Label(summary?.HasCity == true ? "Watch the desk" : "Found a settlement first");
            cta.AddToClassList("pressure-op-card__cta");
            card.Add(cta);
            return card;
        }

        private static string BuildOperationEmptyNote(ShellSummarySnapshot summary, string lane)
        {
            if (summary?.HasCity != true)
            {
                return "The quick-decision strip stays empty until /api/me has a real settlement payload.";
            }

            if (string.Equals(lane, "black_market", StringComparison.OrdinalIgnoreCase))
            {
                return FirstNonBlank(
                    summary.BlackMarketRuntimeTruth?.OperatorFrontSummary,
                    summary.BlackMarketRuntimeTruth?.Detail,
                    summary.BlackMarketPayoffRecovery?.Detail,
                    "No shadow operation is leading right now, so the desk stays honest instead of inventing urgency.");
            }

            return FirstNonBlank(
                summary.PublicBackbonePressureConvergence?.Detail,
                summary.PublicBackbonePressureConvergence?.RecommendedAction,
                "No civic operation is leading right now, so the desk stays honest instead of inventing urgency.");
        }

        private VisualElement BuildOperationCard(OperationSnapshot operation, int index, ShellSummarySnapshot summary, string lane)
        {
            var card = new VisualElement();
            card.AddToClassList("summary-card");
            card.AddToClassList("pressure-op-card");

            var top = new VisualElement();
            top.AddToClassList("pressure-op-card__top");

            var eyebrow = new Label($"Fast option {index + 1:00}");
            eyebrow.AddToClassList("eyebrow");
            top.Add(eyebrow);

            var readiness = new Label(HumanizeOperationReadiness(operation?.Readiness));
            readiness.AddToClassList("pressure-op-card__badge");
            readiness.AddToClassList(OperationReadinessBadgeClass(operation?.Readiness));
            top.Add(readiness);
            card.Add(top);

            var title = new Label(FirstNonBlank(operation?.Title, operation?.FocusLabel, "Operation"));
            title.AddToClassList("rail-note-title");
            card.Add(title);

            var postureTitle = new Label("Lane posture");
            postureTitle.AddToClassList("eyebrow");
            card.Add(postureTitle);

            var postureValue = new Label(BuildOperationPosture(operation, summary, lane));
            postureValue.AddToClassList("summary-value");
            postureValue.AddToClassList("summary-value--glance");
            card.Add(postureValue);

            var whyTitle = new Label("Why now");
            whyTitle.AddToClassList("eyebrow");
            card.Add(whyTitle);

            var whyValue = new Label(FirstNonBlank(operation?.WhyNow, operation?.Payoff, operation?.Detail, operation?.Summary, "No why-now reason surfaced."));
            whyValue.AddToClassList("metric-subvalue");
            whyValue.AddToClassList("metric-subvalue--wrap");
            card.Add(whyValue);

            var cta = new Label(FirstNonBlank(operation?.CtaLabel, DefaultOperationCta(operation)));
            cta.AddToClassList("pressure-op-card__cta");
            card.Add(cta);
            return card;
        }

        private static string DefaultOperationCta(OperationSnapshot operation)
        {
            switch ((operation?.Readiness ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "ready_now":
                    return "Act now";
                case "prepare_soon":
                    return "Prep next";
                case "blocked":
                    return "Review blocker";
                default:
                    return "Review at desk";
            }
        }

        private static string HumanizeOperationReadiness(string readiness)
        {
            switch ((readiness ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "ready_now":
                    return "Ready now";
                case "prepare_soon":
                    return "Forming";
                case "blocked":
                    return "Blocked";
                default:
                    return HumanizeWords(readiness, "Queued");
            }
        }

        private static string OperationReadinessBadgeClass(string readiness)
        {
            switch ((readiness ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "ready_now":
                    return "pressure-op-card__badge--ready";
                case "prepare_soon":
                    return "pressure-op-card__badge--forming";
                case "blocked":
                    return "pressure-op-card__badge--blocked";
                default:
                    return "pressure-op-card__badge--queued";
            }
        }

        private static List<OperationSnapshot> SelectOperationStrip(List<OperationSnapshot> operations, string lane)
        {
            var filtered = (operations ?? new List<OperationSnapshot>())
                .Where(operation => operation != null)
                .Where(operation => OperationMatchesLane(operation, lane))
                .OrderBy(operation => OperationPriorityOrder(operation.Priority))
                .ThenBy(operation => OperationReadinessOrder(operation.Readiness))
                .ThenByDescending(operation => !string.IsNullOrWhiteSpace(operation.WhyNow))
                .ThenByDescending(operation => !string.IsNullOrWhiteSpace(operation.CtaLabel))
                .Take(3)
                .ToList();

            if (filtered.Count > 0 || string.IsNullOrWhiteSpace(lane))
            {
                return filtered;
            }

            return (operations ?? new List<OperationSnapshot>())
                .Where(operation => operation != null)
                .OrderBy(operation => OperationPriorityOrder(operation.Priority))
                .ThenBy(operation => OperationReadinessOrder(operation.Readiness))
                .ThenByDescending(operation => !string.IsNullOrWhiteSpace(operation.WhyNow))
                .Take(3)
                .ToList();
        }

        private static bool OperationMatchesLane(OperationSnapshot operation, string lane)
        {
            if (operation == null || string.IsNullOrWhiteSpace(lane))
            {
                return true;
            }

            var normalizedLane = NormalizeLane(operation.Lane);
            if (string.Equals(normalizedLane, lane, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.Equals(lane, "city", StringComparison.OrdinalIgnoreCase))
            {
                return string.IsNullOrWhiteSpace(operation.Lane);
            }

            return false;
        }

        private static string BuildOperationPosture(OperationSnapshot operation, ShellSummarySnapshot summary, string lane)
        {
            var responsePosture = HumanizeOperationResponsePosture(operation?.ResponsePosture);
            if (!string.IsNullOrWhiteSpace(responsePosture))
            {
                return responsePosture;
            }

            if (string.Equals(lane, "black_market", StringComparison.OrdinalIgnoreCase))
            {
                var shadowKind = ResolveShadowOperationKind(operation, summary);
                if (!string.IsNullOrWhiteSpace(shadowKind))
                {
                    return BuildShadowPostureFromKind(shadowKind);
                }

                return BuildShadowPostureFromText(operation);
            }

            return BuildCivicPostureFromText(operation);
        }

        private static string HumanizeOperationResponsePosture(string responsePosture)
        {
            if (string.IsNullOrWhiteSpace(responsePosture))
            {
                return string.Empty;
            }

            switch (responsePosture.Trim().ToLowerInvariant())
            {
                case "stabilize_first":
                case "stabilize":
                    return "Public relief / repair";
                case "repair":
                    return "Public relief / repair";
                case "investigate":
                    return "Investigation / service desk";
                case "exploit":
                    return "Covert exploit / cash-out";
                case "containment":
                case "contain":
                    return "Cover repair / deniable cleanup";
                case "counterfeit":
                    return "Counterfeit / deniable throughput";
                default:
                    return HumanizeWords(responsePosture, string.Empty);
            }
        }

        private static string ResolveShadowOperationKind(OperationSnapshot operation, ShellSummarySnapshot summary)
        {
            if (!string.IsNullOrWhiteSpace(operation?.Kind))
            {
                return operation.Kind;
            }

            if (summary?.BlackMarketActiveOperation?.Cards == null)
            {
                return string.Empty;
            }

            foreach (var card in summary.BlackMarketActiveOperation.Cards)
            {
                if (card == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(operation?.ActionId) && card.ActionIds?.Any(actionId => string.Equals(actionId, operation.ActionId, StringComparison.OrdinalIgnoreCase)) == true)
                {
                    return card.Kind;
                }

                if (!string.IsNullOrWhiteSpace(operation?.MissionId) && card.MissionOfferIds?.Any(missionId => string.Equals(missionId, operation.MissionId, StringComparison.OrdinalIgnoreCase)) == true)
                {
                    return card.Kind;
                }
            }

            return string.Empty;
        }

        private static string BuildShadowPostureFromKind(string kind)
        {
            switch ((kind ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "counterfeit_job":
                    return "Counterfeit / deniable throughput";
                case "exploit":
                    return "Covert exploit / cash-out";
                case "containment":
                case "cover_repair":
                    return "Cover repair / deniable cleanup";
                case "backbone_pressure":
                case "bribery":
                case "warning_window":
                    return "Covert pressure / deniable leverage";
                default:
                    return string.Empty;
            }
        }

        private static string BuildShadowPostureFromText(OperationSnapshot operation)
        {
            var text = string.Join(" ", new[] { operation?.Title, operation?.Summary, operation?.Detail, operation?.WhyNow, operation?.FocusLabel })
                .Trim()
                .ToLowerInvariant();

            if (text.Contains("counterfeit")) return "Counterfeit / deniable throughput";
            if (text.Contains("cover") || text.Contains("cleanup") || text.Contains("contain")) return "Cover repair / deniable cleanup";
            if (text.Contains("bribe") || text.Contains("pressure") || text.Contains("leverage") || text.Contains("warning")) return "Covert pressure / deniable leverage";
            if (text.Contains("covert") || text.Contains("exploit") || text.Contains("cash")) return "Covert exploit / cash-out";
            return "Covert / deniable / counterfeit";
        }

        private static string BuildCivicPostureFromText(OperationSnapshot operation)
        {
            var text = string.Join(" ", new[] { operation?.Title, operation?.Summary, operation?.Detail, operation?.WhyNow, operation?.FocusLabel })
                .Trim()
                .ToLowerInvariant();

            if (text.Contains("investig") || text.Contains("counterfeit") || text.Contains("audit") || text.Contains("trace")) return "Investigation / service desk";
            if (text.Contains("caravan") || text.Contains("supply") || text.Contains("reserve") || text.Contains("depot")) return "Caravan / reserve relief";
            if (text.Contains("repair") || text.Contains("relief") || text.Contains("stabil") || text.Contains("backbone")) return "Public relief / repair";
            return "Public relief / repair / investigation";
        }

        private void SetPressureDesk(
            string badge,
            string headline,
            string detail,
            string seamTitle,
            string seamValue,
            string seamNote,
            string contractTitle,
            string contractValue,
            string contractNote,
            string answerTitle,
            string answerValue,
            string answerNote,
            string demandTitle,
            string demandValue,
            string demandNote)
        {
            if (pressureDeskBadge != null) pressureDeskBadge.text = badge;
            if (pressureDeskHeadline != null) pressureDeskHeadline.text = headline;
            if (pressureDeskDetail != null) pressureDeskDetail.text = detail;
            if (pressureSeamTitle != null) pressureSeamTitle.text = seamTitle;
            if (pressureSeamValue != null) pressureSeamValue.text = seamValue;
            if (pressureSeamNote != null) pressureSeamNote.text = seamNote;
            if (pressureContractTitle != null) pressureContractTitle.text = contractTitle;
            if (pressureContractValue != null) pressureContractValue.text = contractValue;
            if (pressureContractNote != null) pressureContractNote.text = contractNote;
            if (pressureAnswerTitle != null) pressureAnswerTitle.text = answerTitle;
            if (pressureAnswerValue != null) pressureAnswerValue.text = answerValue;
            if (pressureAnswerNote != null) pressureAnswerNote.text = answerNote;
            if (pressureDemandTitle != null) pressureDemandTitle.text = demandTitle;
            if (pressureDemandValue != null) pressureDemandValue.text = demandValue;
            if (pressureDemandNote != null) pressureDemandNote.text = demandNote;
        }

        private static string NormalizeLane(string lane)
        {
            var normalized = (lane ?? string.Empty).Trim().ToLowerInvariant();
            if (normalized == "black_market" || normalized == "black market" || normalized == "black-market" || normalized == "blackmarket" || normalized == "shadow")
            {
                return "black_market";
            }

            return "city";
        }

        private static string BuildReadyOpsSummary(List<OperationSnapshot> operations)
        {
            if (operations == null || operations.Count == 0)
            {
                return "No opening operations surfaced.";
            }

            var readyNow = operations.Count(o => string.Equals(o.Readiness, "ready_now", StringComparison.OrdinalIgnoreCase));
            var preparing = operations.Count(o => string.Equals(o.Readiness, "prepare_soon", StringComparison.OrdinalIgnoreCase));
            var blocked = operations.Count(o => string.Equals(o.Readiness, "blocked", StringComparison.OrdinalIgnoreCase));
            return $"{readyNow} ready • {preparing} forming • {blocked} blocked";
        }

        private static OperationSnapshot SelectPrimaryOperation(List<OperationSnapshot> operations)
        {
            if (operations == null || operations.Count == 0)
            {
                return null;
            }

            return operations
                .Where(o => o != null)
                .OrderBy(o => OperationPriorityOrder(o.Priority))
                .ThenBy(o => OperationReadinessOrder(o.Readiness))
                .ThenByDescending(o => !string.IsNullOrWhiteSpace(o.WhyNow))
                .FirstOrDefault();
        }

        private static int OperationPriorityOrder(string priority)
        {
            switch ((priority ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "opening":
                    return 0;
                case "high":
                    return 1;
                case "watch":
                    return 2;
                default:
                    return 3;
            }
        }

        private static int OperationReadinessOrder(string readiness)
        {
            switch ((readiness ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "ready_now":
                    return 0;
                case "prepare_soon":
                    return 1;
                case "blocked":
                    return 2;
                default:
                    return 3;
            }
        }

        private static PublicBackbonePressureFrontSnapshot SelectPrimaryCivicFront(PublicBackbonePressureConvergenceSurfaceSnapshot surface)
        {
            return surface?.Fronts?
                .Where(front => front != null)
                .OrderBy(front => CivicFrontOrder(front.State))
                .FirstOrDefault();
        }

        private static int CivicFrontOrder(string state)
        {
            switch ((state ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "active":
                    return 0;
                case "watch":
                    return 1;
                case "cooling":
                    return 2;
                case "quiet":
                default:
                    return 3;
            }
        }

        private static BlackMarketActiveOperationCardSnapshot SelectPrimaryShadowCard(BlackMarketActiveOperationSurfaceSnapshot surface)
        {
            return surface?.Cards?
                .Where(card => card != null)
                .OrderBy(card => ShadowCardOrder(card.State))
                .ThenBy(card => ShadowKindOrder(card.Kind))
                .FirstOrDefault();
        }

        private static int ShadowCardOrder(string state)
        {
            switch ((state ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "live":
                    return 0;
                case "forming":
                    return 1;
                case "cooling":
                    return 2;
                default:
                    return 3;
            }
        }

        private static int ShadowKindOrder(string kind)
        {
            switch ((kind ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "exploit":
                    return 0;
                case "counterfeit_job":
                    return 1;
                case "backbone_pressure":
                    return 2;
                case "containment":
                    return 3;
                case "bribery":
                    return 4;
                case "cover_repair":
                    return 5;
                case "warning_window":
                    return 6;
                default:
                    return 7;
            }
        }

        private static string MapCivicStage(PublicBackbonePressureConvergenceSurfaceSnapshot surface, PublicBackbonePressureFrontSnapshot front)
        {
            var state = (front?.State ?? string.Empty).Trim().ToLowerInvariant();
            if (state == "active") return "engaged";
            if (state == "watch") return "forming";
            if (state == "cooling") return "cooling";

            var phase = (surface?.Phase ?? string.Empty).Trim().ToLowerInvariant();
            if (phase == "triage") return "engaged";
            if (phase == "watch") return "forming";
            if (phase == "cooling") return "cooling";
            return "quiet";
        }

        private static string MapShadowStage(BlackMarketActiveOperationCardSnapshot card, BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var state = (card?.State ?? string.Empty).Trim().ToLowerInvariant();
            if (state == "live" || state == "forming" || state == "cooling")
            {
                return state;
            }

            var phase = (payoff?.Phase ?? string.Empty).Trim().ToLowerInvariant();
            if (phase == "backlash_live" || phase == "payoff_live") return "live";
            if (phase == "cooling") return "cooling";

            var band = (runtime?.RuntimeBand ?? string.Empty).Trim().ToLowerInvariant();
            if (band == "hot" || band == "active") return "live";
            if (band == "watch") return "forming";
            return "quiet";
        }

        private static string HumanizeStage(string stage)
        {
            switch ((stage ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "engaged": return "Engaged";
                case "forming": return "Forming";
                case "live": return "Live";
                case "cooling": return "Cooling";
                default: return "Quiet";
            }
        }

        private static string BuildCivicAnswerValue(PublicBackbonePressureFrontSnapshot front, OperationSnapshot primaryOp)
        {
            var source = (front?.Id ?? string.Empty).Trim().ToLowerInvariant();
            if (source == "public_backbone") return "Public relief / repair";
            if (source == "vendor_trade") return "Caravan / reserve relief";
            if (source == "npc_city_services") return "Investigation / service desk";

            var title = (primaryOp?.Title ?? string.Empty).ToLowerInvariant();
            if (title.Contains("repair") || title.Contains("stabilize")) return "Public relief / repair";
            if (title.Contains("caravan") || title.Contains("supply") || title.Contains("reserve")) return "Caravan / reserve relief";
            if (title.Contains("investig") || title.Contains("counterfeit") || title.Contains("audit")) return "Investigation / service desk";
            return "Public relief / repair / investigation";
        }

        private static string BuildCivicAnswerNote(PublicBackbonePressureConvergenceSurfaceSnapshot surface, PublicBackbonePressureFrontSnapshot front, OperationSnapshot primaryOp)
        {
            var frontSummary = front != null
                ? $"{front.Label}: {FirstNonBlank(front.RecommendedAction, front.Summary, front.Headline)}"
                : string.Empty;
            var operationSummary = primaryOp != null
                ? FirstNonBlank(primaryOp.WhyNow, primaryOp.Payoff, primaryOp.Detail)
                : string.Empty;
            return FirstNonBlank(frontSummary, operationSummary, surface?.RecommendedAction, "The civic desk should keep relief, repair, and investigation answers distinct instead of flattening them into generic board chatter.");
        }

        private static string BuildCivicDemandValue(PublicBackbonePressureConvergenceSurfaceSnapshot surface)
        {
            if (surface == null)
            {
                return "No civic demand bend surfaced.";
            }

            var tradeWindow = HumanizeWords(surface.TradeWindow, "trade window unknown");
            var focusLane = HumanizeWords(surface.FocusLane, "mixed focus");
            return $"{tradeWindow} • {focusLane}";
        }

        private static string BuildCivicDemandNote(PublicBackbonePressureConvergenceSurfaceSnapshot surface, OperationSnapshot primaryOp)
        {
            if (surface?.LatestSupportReceipt != null)
            {
                return $"Latest receipt: {surface.LatestSupportReceipt.Title} • {surface.LatestSupportReceipt.Summary}";
            }

            return FirstNonBlank(
                primaryOp?.WhyNow,
                surface?.RecommendedAction,
                surface?.Detail,
                "Supply, reserve, and exchange pressure are not materially bending the board right now.");
        }

        private static string BuildShadowAnswerValue(BlackMarketActiveOperationCardSnapshot card, OperationSnapshot primaryOp)
        {
            var kind = (card?.Kind ?? string.Empty).Trim().ToLowerInvariant();
            if (kind == "counterfeit_job") return "Counterfeit / deniable throughput";
            if (kind == "exploit") return "Covert exploit / cash-out";
            if (kind == "containment" || kind == "cover_repair") return "Cover repair / deniable cleanup";
            if (kind == "bribery" || kind == "backbone_pressure" || kind == "warning_window") return "Covert pressure / deniable leverage";

            var title = (primaryOp?.Title ?? string.Empty).ToLowerInvariant();
            if (title.Contains("counterfeit")) return "Counterfeit / deniable throughput";
            if (title.Contains("covert") || title.Contains("exploit")) return "Covert exploit / cash-out";
            if (title.Contains("cover") || title.Contains("contain")) return "Cover repair / deniable cleanup";
            return "Covert / deniable / counterfeit";
        }

        private static string BuildShadowAnswerNote(BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketActiveOperationSurfaceSnapshot active, BlackMarketActiveOperationCardSnapshot card)
        {
            if (card != null)
            {
                return FirstNonBlank(card.OperatorNote, card.Summary, runtime?.ActiveOperation?.Detail, active?.Detail, "The shadow desk should keep covert, counterfeit, and cleanup answers distinct instead of flattening them into generic warning sludge.");
            }

            return FirstNonBlank(runtime?.ActiveOperation?.Detail, active?.Detail, runtime?.OperatorFrontSummary, "The shadow desk should keep covert, counterfeit, and cleanup answers distinct instead of flattening them into generic warning sludge.");
        }

        private static string BuildShadowDemandValue(BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var pressureState = HumanizeWords(runtime?.PublicBackbonePressure?.State, "quiet public pressure");
            var payoffPhase = HumanizeWords(payoff?.Phase, "quiet payoff");
            return $"{pressureState} • {payoffPhase}";
        }

        private static string BuildShadowDemandNote(BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff, OperationSnapshot primaryOp)
        {
            if (payoff?.RecentReceipts != null && payoff.RecentReceipts.Count > 0)
            {
                var receipt = payoff.RecentReceipts[0];
                return $"Latest receipt: {receipt.Title} • {receipt.Summary}";
            }

            return FirstNonBlank(
                runtime?.PublicBackbonePressure?.RecommendedAction,
                payoff?.RecommendedAction,
                primaryOp?.WhyNow,
                runtime?.PublicBackbonePressure?.Detail,
                "Public desks, vendor lanes, and permit memory are not materially bending the shadow board right now.");
        }

        private static string HumanizeShadowKind(string kind)
        {
            switch ((kind ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "counterfeit_job": return "Counterfeit job";
                case "backbone_pressure": return "Backbone pressure";
                case "cover_repair": return "Cover repair";
                case "warning_window": return "Warning window";
                case "containment": return "Containment";
                case "bribery": return "Bribery";
                case "exploit": return "Exploit";
                default: return HumanizeWords(kind, "Shadow seam");
            }
        }

        private static string HumanizeWords(string raw, string fallback)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return fallback;
            }

            var words = raw.Trim().Replace("_", " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", words.Select(word => char.ToUpperInvariant(word[0]) + word.Substring(1)));
        }

        private static string FirstNonBlank(params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return string.Empty;
        }

        private static string FormatResource(ResourceSnapshot r, string fallback, string suffix = "")
        {
            var chunks = new[]
            {
                Pair("Food", r.Food, suffix), Pair("Materials", r.Materials, suffix), Pair("Wealth", r.Wealth, suffix), Pair("Mana", r.Mana, suffix), Pair("Knowledge", r.Knowledge, suffix), Pair("Unity", r.Unity, suffix)
            }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            return chunks.Length == 0 ? fallback : string.Join(" • ", chunks);
        }

        private static string Pair(string name, double? value, string suffix) => value.HasValue ? $"{name} {value.Value:0.#}{suffix}" : null;
        private static string FormatProgress(double? p, double? c) => c.GetValueOrDefault() > 0 ? $"{p.GetValueOrDefault():0.#}/{c.Value:0.#}" : $"{p.GetValueOrDefault():0.#}";
        private static string FormatRemaining(TimeSpan span) => span <= TimeSpan.Zero ? "now" : span.ToString(span.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss");

        private static string FormatWorkshop(List<WorkshopJobSnapshot> jobs)
        {
            var nowUtc = DateTime.UtcNow;
            var ready = jobs.Where(j => IsWorkshopJobCollectable(j, nowUtc)).ToArray();
            if (ready.Length > 0)
            {
                return $"{GetWorkshopJobTitle(ready[0])} • ready to collect";
            }

            var active = jobs.Where(j => !IsWorkshopJobCollected(j) && !IsWorkshopJobCollectable(j, nowUtc)).ToArray();
            if (active.Length == 0) return "No active workshop queue.";
            var first = active[0];
            var timer = first.FinishesAtUtc.HasValue ? FormatRemaining(first.FinishesAtUtc.Value - nowUtc) : "time unknown";
            return $"{GetWorkshopJobTitle(first)} • {timer}";
        }

        private static bool IsWorkshopJobCollected(WorkshopJobSnapshot job)
        {
            return job?.CollectedAtUtc.HasValue == true;
        }

        private static bool IsWorkshopJobCollectable(WorkshopJobSnapshot job, DateTime nowUtc)
        {
            if (job == null || IsWorkshopJobCollected(job))
            {
                return false;
            }

            if (job.Completed)
            {
                return true;
            }

            return job.FinishesAtUtc.HasValue && job.FinishesAtUtc.Value <= nowUtc;
        }

        private static string GetWorkshopJobTitle(WorkshopJobSnapshot job)
        {
            var outputName = job?.OutputName?.Trim();
            if (!string.IsNullOrWhiteSpace(outputName))
            {
                return outputName;
            }

            var recipeId = job?.RecipeId?.Trim();
            if (!string.IsNullOrWhiteSpace(recipeId))
            {
                return recipeId;
            }

            var attachmentKind = job?.AttachmentKind?.Trim();
            if (!string.IsNullOrWhiteSpace(attachmentKind))
            {
                return attachmentKind;
            }

            return "workshop job";
        }

        private static string FormatMission(List<MissionSnapshot> missions)
        {
            if (missions.Count == 0) return "No active mission clock.";
            var first = missions[0];
            var timer = first.FinishesAtUtc.HasValue ? FormatRemaining(first.FinishesAtUtc.Value - DateTime.UtcNow) : "anchor missing";
            var context = BuildMissionContext(first);
            return string.IsNullOrWhiteSpace(context)
                ? $"{first.Title} • {timer}"
                : $"{first.Title} • {context} • {timer}";
        }

        private static string BuildMissionContext(MissionSnapshot mission)
        {
            if (mission == null)
            {
                return string.Empty;
            }

            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(mission.RegionId))
            {
                parts.Add($"region {mission.RegionId}");
            }

            var armyName = !string.IsNullOrWhiteSpace(mission.AssignedArmyName) ? mission.AssignedArmyName : mission.AssignedArmyId;
            if (!string.IsNullOrWhiteSpace(armyName))
            {
                parts.Add($"formation {armyName}");
            }

            var heroName = !string.IsNullOrWhiteSpace(mission.AssignedHeroName) ? mission.AssignedHeroName : mission.AssignedHeroId;
            if (!string.IsNullOrWhiteSpace(heroName))
            {
                parts.Add($"hero {heroName}");
            }

            return string.Join(" • ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        private static string FormatTick(TimerSnapshot timing)
        {
            var cadence = GetCadence(timing);
            var nextTickAtUtc = ResolveNextTickAtUtc(timing, cadence);

            if (!nextTickAtUtc.HasValue && !cadence.HasValue)
            {
                return "Tick timing unavailable.";
            }

            var remaining = nextTickAtUtc.HasValue
                ? FormatRemaining(nextTickAtUtc.Value - DateTime.UtcNow)
                : "anchor missing";
            var cadenceText = cadence.HasValue ? FormatRemaining(cadence.Value) : "cadence unknown";
            return $"{remaining} • every {cadenceText} • {DescribeTimingState(nextTickAtUtc.HasValue, cadence.HasValue)}";
        }

        private static string FormatTimerRaw(TimerSnapshot timing, bool isSummaryLoaded)
        {
            if (!isSummaryLoaded)
            {
                return "raw: waiting for summary payload";
            }

            var cadence = GetCadence(timing);
            var nextTickAtUtc = ResolveNextTickAtUtc(timing, cadence);
            if (!cadence.HasValue && !nextTickAtUtc.HasValue)
            {
                return "raw: state=no_timing_data; tickMs=n/a, last=n/a, next=n/a";
            }

            var last = timing.LastTickAtUtc.HasValue ? timing.LastTickAtUtc.Value.ToString("HH:mm:ss") + " UTC" : "n/a";
            var next = nextTickAtUtc.HasValue ? nextTickAtUtc.Value.ToString("HH:mm:ss") + " UTC" : "n/a";
            var tickMsText = cadence.HasValue ? $"{cadence.Value.TotalMilliseconds:0.#}" : "n/a";
            return $"raw: state={GetTimingState(nextTickAtUtc.HasValue, cadence.HasValue)}; tickMs={tickMsText}, last={last}, next={next}";
        }

        private static TimeSpan? GetCadence(TimerSnapshot timing)
        {
            if (!timing.TickMs.HasValue || timing.TickMs <= 0) return null;
            return TimeSpan.FromMilliseconds(timing.TickMs.Value);
        }

        private static DateTime? ResolveNextTickAtUtc(TimerSnapshot timing, TimeSpan? cadence)
        {
            if (timing.NextTickAtUtc.HasValue)
            {
                return timing.NextTickAtUtc.Value;
            }

            if (!timing.LastTickAtUtc.HasValue || !cadence.HasValue)
            {
                return null;
            }

            return timing.LastTickAtUtc.Value + cadence.Value;
        }

        private static string DescribeTimingState(bool hasAnchor, bool hasCadence)
        {
            if (hasAnchor) return "countdown ready";
            if (hasCadence) return "cadence-only";
            return "no timing data";
        }

        private static string GetTimingState(bool hasAnchor, bool hasCadence)
        {
            if (hasAnchor) return "countdown_ready";
            if (hasCadence) return "cadence_only_anchor_missing";
            return "no_timing_data";
        }
    }
}
