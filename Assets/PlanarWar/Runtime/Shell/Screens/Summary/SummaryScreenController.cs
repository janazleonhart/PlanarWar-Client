using PlanarWar.Client.Core.Contracts;
using PlanarWar.Client.Core.Presentation;
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
        private readonly Label pressureHandoffTitle;
        private readonly Label pressureHandoffValue;
        private readonly Label pressureHandoffNote;
        private readonly Label pressureConsequenceTitle;
        private readonly Label pressureConsequenceValue;
        private readonly Label pressureConsequenceNote;
        private readonly Label pressureOperationsCopy;
        private readonly Label pressureOperationsCountBadge;
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
            pressureHandoffTitle = root.Q<Label>("pressure-handoff-title");
            pressureHandoffValue = root.Q<Label>("pressure-handoff-value");
            pressureHandoffNote = root.Q<Label>("pressure-handoff-note");
            pressureConsequenceTitle = root.Q<Label>("pressure-consequence-title");
            pressureConsequenceValue = root.Q<Label>("pressure-consequence-value");
            pressureConsequenceNote = root.Q<Label>("pressure-consequence-note");
            pressureOperationsCopy = root.Q<Label>("pressure-operations-copy-value");
            pressureOperationsCountBadge = root.Q<Label>("pressure-operations-count-badge-value");
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
                    demandNote: "Production, reserve, and exchange drag need a live settlement payload before the desk can summarize them honestly.",
                    handoffTitle: "Desk / strip handoff",
                    handoffValue: "No live action handoff.",
                    handoffNote: "Once a settlement is real, the desk and top fast option will point at the same lead answer instead of drifting apart.",
                    consequenceTitle: "Consequence hint",
                    consequenceValue: "No live consequence hint yet.",
                    consequenceNote: "Impact preview stays blank until an actual opening operation is surfaced.");
                RenderOperationStrip(summary, lane: null);
                return;
            }

            var lane = NormalizeLane(summary.City.SettlementLane);
            var stripOperations = SelectOperationStrip(summary, lane);
            var primaryOp = stripOperations.FirstOrDefault() ?? SelectPrimaryOperation(summary.OpeningOperations);

            if (lane == "black_market")
            {
                RenderShadowPressureDesk(summary, primaryOp, lane);
                RenderOperationStrip(summary, lane, stripOperations);
                return;
            }

            RenderCivicPressureDesk(summary, primaryOp, lane);
            RenderOperationStrip(summary, lane, stripOperations);
        }

        private void RenderCivicPressureDesk(ShellSummarySnapshot summary, OperationSnapshot primaryOp, string lane)
        {
            var surface = summary.PublicBackbonePressureConvergence;
            var front = SelectPrimaryCivicFront(surface);
            var followThrough = surface?.ContractFollowThrough;
            var effects = surface?.ContractEffects;
            var phase = HumanizeStage(MapCivicStage(surface, front));
            var continuity = ContractTruthText.BuildContractSeamValue(followThrough, front != null ? $"{front.Label} • {front.Id}" : "Public backbone continuity");
            var recommended = FirstNonBlank(primaryOp?.WhyNow, followThrough?.Note, front?.RecommendedAction, surface?.RecommendedAction, "No civic why-now reason surfaced.");
            var deskHeadline = effects != null && !string.IsNullOrWhiteSpace(effects.ContractTitle)
                ? effects.State == "cooling"
                    ? $"{effects.ContractTitle} is cooling civic effects"
                    : $"{effects.ContractTitle} is shaping civic effects"
                : followThrough != null && !string.IsNullOrWhiteSpace(followThrough.ContractTitle)
                    ? $"{followThrough.ContractTitle} • {ContractTruthText.HumanizeLifecycle(followThrough.State)}"
                    : FirstNonBlank(primaryOp?.FocusLabel, front?.Headline, surface?.Headline, primaryOp?.Title, "Civic pressure desk is quiet enough to stay backgrounded.");
            var deskDetail = FirstNonBlank(effects?.Note, followThrough?.Note, primaryOp?.Summary, surface?.Detail, front?.Summary, primaryOp?.Detail, "No civic convergence detail surfaced.");
            var answerValue = ContractTruthText.BuildCivicEffectsValue(effects, BuildCivicAnswerValue(front, primaryOp));
            var answerNote = ContractTruthText.BuildCivicEffectsNote(effects, followThrough, BuildCivicAnswerNote(surface, front, primaryOp));
            var demandValue = BuildCivicDemandValue(surface);
            var demandNote = BuildCivicDemandNote(surface, primaryOp);
            var contractValue = ContractTruthText.BuildContractLifecycleValue(followThrough, FirstNonBlank(primaryOp?.Title, primaryOp?.FocusLabel, "No civic contract is currently leading."));
            var contractNote = FirstNonBlank(followThrough?.Note, primaryOp?.WhyNow, primaryOp?.Payoff, recommended);

            SetPressureDesk(
                badge: $"City seam • {phase}",
                headline: deskHeadline,
                detail: deskDetail,
                seamTitle: followThrough != null ? "Grounded contract" : "Live seam",
                seamValue: continuity,
                seamNote: followThrough != null
                    ? followThrough.Note
                    : front != null
                        ? $"{phase} • {FirstNonBlank(front.Headline, front.Summary)}"
                        : FirstNonBlank(surface?.Headline, "No explicit civic seam card surfaced."),
                contractTitle: followThrough != null ? "Lifecycle" : "Why this board moved",
                contractValue: contractValue,
                contractNote: contractNote,
                answerTitle: effects != null || followThrough != null ? "Bounded civic effects" : "Answer lane",
                answerValue: answerValue,
                answerNote: answerNote,
                demandTitle: "Supply / reserve bend",
                demandValue: demandValue,
                demandNote: demandNote,
                handoffTitle: "Desk / strip handoff",
                handoffValue: BuildLeadOperationHandoffValue(primaryOp),
                handoffNote: BuildLeadOperationHandoffNote(primaryOp, summary, lane),
                consequenceTitle: "Consequence hint",
                consequenceValue: BuildLeadOperationConsequenceValue(primaryOp),
                consequenceNote: BuildLeadOperationConsequenceNote(primaryOp));
        }

        private void RenderShadowPressureDesk(ShellSummarySnapshot summary, OperationSnapshot primaryOp, string lane)
        {
            var runtime = summary.BlackMarketRuntimeTruth;
            var active = summary.BlackMarketActiveOperation;
            var backbone = summary.BlackMarketBackbonePressure;
            var payoff = summary.BlackMarketPayoffRecovery;
            var card = SelectPrimaryShadowCard(active);
            var followThrough = backbone?.ContractFollowThrough;
            var effects = backbone?.ContractEffects;
            var stage = HumanizeStage(MapShadowStage(card, runtime, backbone, payoff));
            var continuity = ContractTruthText.BuildContractSeamValue(followThrough, card != null ? $"{HumanizeShadowKind(card.Kind)} • {card.Id}" : "Shadow runtime continuity");
            var deskHeadline = effects != null && !string.IsNullOrWhiteSpace(effects.ContractTitle)
                ? effects.State == "cooling"
                    ? $"{effects.ContractTitle} is cooling covert carry"
                    : $"{effects.ContractTitle} is carrying shadow effects"
                : followThrough != null && !string.IsNullOrWhiteSpace(followThrough.ContractTitle)
                    ? $"{followThrough.ContractTitle} • {ContractTruthText.HumanizeLifecycle(followThrough.State)}"
                    : FirstNonBlank(primaryOp?.FocusLabel, card?.Headline, backbone?.Headline, runtime?.Headline, payoff?.Headline, primaryOp?.Title, "Shadow pressure desk is quiet enough to stay backgrounded.");
            var deskDetail = FirstNonBlank(effects?.Note, followThrough?.Note, backbone?.Detail, primaryOp?.Summary, runtime?.Detail, card?.Summary, payoff?.Detail, primaryOp?.Detail, "No shadow convergence detail surfaced.");
            var contractValue = ContractTruthText.BuildContractLifecycleValue(followThrough, FirstNonBlank(primaryOp?.Title, primaryOp?.FocusLabel, "No shadow contract is currently leading."));
            var contractNote = FirstNonBlank(followThrough?.Note, primaryOp?.WhyNow, backbone?.RecommendedAction, runtime?.ActiveOperation?.Detail, payoff?.RecommendedAction, card?.OperatorNote, "No shadow why-now reason surfaced.");
            var answerValue = ContractTruthText.BuildShadowEffectsValue(effects, BuildShadowAnswerValue(card, primaryOp));
            var answerNote = ContractTruthText.BuildShadowEffectsNote(effects, followThrough, BuildShadowAnswerNote(runtime, active, card));
            var demandValue = BuildShadowDemandValue(runtime, backbone, payoff);
            var demandNote = BuildShadowDemandNote(runtime, backbone, payoff, primaryOp);

            SetPressureDesk(
                badge: $"Shadow seam • {stage}",
                headline: deskHeadline,
                detail: deskDetail,
                seamTitle: followThrough != null ? "Grounded contract" : "Live seam",
                seamValue: continuity,
                seamNote: followThrough != null
                    ? followThrough.Note
                    : card != null
                        ? $"{stage} • {FirstNonBlank(card.Summary, card.OperatorNote)}"
                        : FirstNonBlank(backbone?.Headline, runtime?.Headline, payoff?.Headline, "No explicit shadow seam card surfaced."),
                contractTitle: followThrough != null ? "Lifecycle" : "Why this board moved",
                contractValue: contractValue,
                contractNote: contractNote,
                answerTitle: effects != null || followThrough != null ? "Bounded shadow effects" : "Answer lane",
                answerValue: answerValue,
                answerNote: answerNote,
                demandTitle: "Pressure / payoff bend",
                demandValue: demandValue,
                demandNote: demandNote,
                handoffTitle: "Desk / strip handoff",
                handoffValue: BuildLeadOperationHandoffValue(primaryOp),
                handoffNote: BuildLeadOperationHandoffNote(primaryOp, summary, lane),
                consequenceTitle: "Consequence hint",
                consequenceValue: BuildLeadOperationConsequenceValue(primaryOp),
                consequenceNote: BuildLeadOperationConsequenceNote(primaryOp));
        }

        private void RenderOperationStrip(ShellSummarySnapshot summary, string lane, List<OperationSnapshot> selectedOperations = null)
        {
            if (pressureOperationsStrip == null)
            {
                return;
            }

            pressureOperationsStrip.Clear();

            var operations = selectedOperations ?? SelectOperationStrip(summary, lane);
            SetPressureOperationsMeta(BuildOperationStripBadge(operations), BuildOperationStripCopy(summary, lane, operations));
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
                    summary.BlackMarketBackbonePressure?.ContractEffects?.Note,
                    summary.BlackMarketBackbonePressure?.ContractFollowThrough?.Note,
                    summary.BlackMarketBackbonePressure?.Detail,
                    summary.BlackMarketRuntimeTruth?.OperatorFrontSummary,
                    summary.BlackMarketRuntimeTruth?.Detail,
                    summary.BlackMarketPayoffRecovery?.Detail,
                    "No shadow operation is leading right now, so the desk stays honest instead of inventing urgency.");
            }

            return FirstNonBlank(
                summary.PublicBackbonePressureConvergence?.ContractEffects?.Note,
                summary.PublicBackbonePressureConvergence?.ContractFollowThrough?.Note,
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

            var demandTitle = new Label("Demand signal");
            demandTitle.AddToClassList("eyebrow");
            card.Add(demandTitle);

            var demandValue = new Label(BuildOperationDemandSignal(operation, summary, lane));
            demandValue.AddToClassList("metric-subvalue");
            demandValue.AddToClassList("metric-subvalue--wrap");
            card.Add(demandValue);

            var whyTitle = new Label("Why now");
            whyTitle.AddToClassList("eyebrow");
            card.Add(whyTitle);

            var whyValue = new Label(FirstNonBlank(operation?.WhyNow, operation?.Payoff, operation?.Detail, operation?.Summary, "No why-now reason surfaced."));
            whyValue.AddToClassList("metric-subvalue");
            whyValue.AddToClassList("metric-subvalue--wrap");
            card.Add(whyValue);

            var handoffTitle = new Label(BuildOperationHandoffTitle(operation));
            handoffTitle.AddToClassList("eyebrow");
            card.Add(handoffTitle);

            var handoffValue = new Label(BuildOperationHandoff(operation, index == 0));
            handoffValue.AddToClassList("metric-subvalue");
            handoffValue.AddToClassList("metric-subvalue--wrap");
            card.Add(handoffValue);

            var consequenceTitle = new Label("Consequence hint");
            consequenceTitle.AddToClassList("eyebrow");
            card.Add(consequenceTitle);

            var consequenceValue = new Label(BuildOperationConsequenceValue(operation));
            consequenceValue.AddToClassList("metric-subvalue");
            consequenceValue.AddToClassList("metric-subvalue--wrap");
            card.Add(consequenceValue);

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

        private static List<OperationSnapshot> SelectOperationStrip(ShellSummarySnapshot summary, string lane)
        {
            var operations = summary?.OpeningOperations ?? new List<OperationSnapshot>();
            var filtered = operations
                .Where(operation => operation != null)
                .Where(operation => OperationMatchesLane(operation, lane))
                .OrderByDescending(operation => OperationDemandScore(operation, summary, lane))
                .ThenBy(operation => OperationPriorityOrder(operation.Priority))
                .ThenBy(operation => OperationReadinessOrder(operation.Readiness))
                .ThenByDescending(operation => !string.IsNullOrWhiteSpace(operation.WhyNow))
                .ThenByDescending(operation => !string.IsNullOrWhiteSpace(operation.CtaLabel))
                .Take(3)
                .ToList();

            if (filtered.Count > 0 || string.IsNullOrWhiteSpace(lane))
            {
                return filtered;
            }

            return operations
                .Where(operation => operation != null)
                .OrderByDescending(operation => OperationDemandScore(operation, summary, lane: null))
                .ThenBy(operation => OperationPriorityOrder(operation.Priority))
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

        private void SetPressureOperationsMeta(string badge, string copy)
        {
            if (pressureOperationsCountBadge != null) pressureOperationsCountBadge.text = badge;
            if (pressureOperationsCopy != null) pressureOperationsCopy.text = copy;
        }


        private static string BuildOperationHandoffTitle(OperationSnapshot operation)
        {
            switch ((operation?.Readiness ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "ready_now":
                    return "Action handoff";
                case "prepare_soon":
                    return "Prep gate";
                case "blocked":
                    return "Blocker";
                default:
                    return "Desk handoff";
            }
        }

        private static string BuildOperationHandoff(OperationSnapshot operation, bool isLead)
        {
            var detail = CompactSingleLine(FirstNonBlank(operation?.Risk, operation?.Detail, operation?.Summary, operation?.WhyNow));
            switch ((operation?.Readiness ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "ready_now":
                    return isLead
                        ? FirstNonBlank(
                            CompactSingleLine($"Lead with {FirstNonBlank(operation?.CtaLabel, DefaultOperationCta(operation)).ToLowerInvariant()} while this is still the cleanest live answer."),
                            "Lead with this now while the seam is still asking for it.")
                        : FirstNonBlank(
                            CompactSingleLine($"Keep this in hand as the next clean answer if the lead option slips."),
                            "Keep this ready as the next clean answer.");
                case "prepare_soon":
                    return !string.IsNullOrWhiteSpace(detail)
                        ? $"Prep this next: {detail}"
                        : "Prep this next so it can take point before the seam hardens.";
                case "blocked":
                    return !string.IsNullOrWhiteSpace(detail)
                        ? $"Blocked: {detail}"
                        : "Blocked right now; review the gate before spending a turn on it.";
                default:
                    return !string.IsNullOrWhiteSpace(detail)
                        ? $"Desk handoff: {detail}"
                        : "Desk handoff is still settling on the cleanest next answer.";
            }
        }

        private static string BuildOperationConsequenceValue(OperationSnapshot operation)
        {
            if (operation?.ImpactPreview != null)
            {
                foreach (var preview in operation.ImpactPreview)
                {
                    var compact = CompactSingleLine(preview);
                    if (!string.IsNullOrWhiteSpace(compact))
                    {
                        return compact;
                    }
                }
            }

            switch ((operation?.Readiness ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "blocked":
                    return FirstNonBlank(CompactSingleLine(operation?.Risk), CompactSingleLine(operation?.Detail), "No unblock consequence surfaced yet.");
                case "prepare_soon":
                    return FirstNonBlank(CompactSingleLine(operation?.Payoff), CompactSingleLine(operation?.Summary), "This line is still forming, so the consequence stays soft until it clears.");
                default:
                    return FirstNonBlank(CompactSingleLine(operation?.Payoff), CompactSingleLine(operation?.Summary), CompactSingleLine(operation?.Risk), "No consequence hint surfaced.");
            }
        }

        private static string BuildLeadOperationHandoffValue(OperationSnapshot operation)
        {
            if (operation == null)
            {
                return "No lead action surfaced.";
            }

            return $"{HumanizeOperationReadiness(operation.Readiness)} • {FirstNonBlank(operation.Title, operation.FocusLabel, "Lead operation")}";
        }

        private static string BuildLeadOperationHandoffNote(OperationSnapshot operation, ShellSummarySnapshot summary, string lane)
        {
            return operation == null
                ? "The desk will bind to the strongest fast option once the strip has a real lead action."
                : BuildOperationHandoff(operation, isLead: true);
        }

        private static string BuildLeadOperationConsequenceValue(OperationSnapshot operation)
        {
            return operation == null
                ? "No lead consequence surfaced."
                : BuildOperationConsequenceValue(operation);
        }

        private static string BuildLeadOperationConsequenceNote(OperationSnapshot operation)
        {
            if (operation?.ImpactPreview != null)
            {
                foreach (var preview in operation.ImpactPreview.Skip(1))
                {
                    var compact = CompactSingleLine(preview);
                    if (!string.IsNullOrWhiteSpace(compact))
                    {
                        return compact;
                    }
                }
            }

            if (operation == null)
            {
                return "Impact preview stays blank until an opening operation exposes a real outcome hint.";
            }

            return FirstNonBlank(
                CompactSingleLine(operation.Risk),
                CompactSingleLine(operation.Payoff),
                CompactSingleLine(operation.Detail),
                "The lead line did not ship a second consequence note, so the desk stays restrained.");
        }

        private static string CompactSingleLine(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return string.Join(" ", value.Split(new[] { '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)).Trim();
        }

        private static string BuildOperationStripBadge(List<OperationSnapshot> operations)
        {
            if (operations == null || operations.Count == 0)
            {
                return "No fast calls";
            }

            var readyNow = operations.Count(operation => string.Equals(operation?.Readiness, "ready_now", StringComparison.OrdinalIgnoreCase));
            var forming = operations.Count(operation => string.Equals(operation?.Readiness, "prepare_soon", StringComparison.OrdinalIgnoreCase));
            var blocked = operations.Count(operation => string.Equals(operation?.Readiness, "blocked", StringComparison.OrdinalIgnoreCase));
            return $"{operations.Count} shaped • {readyNow} ready • {forming} forming • {blocked} blocked";
        }

        private static string BuildOperationStripCopy(ShellSummarySnapshot summary, string lane, List<OperationSnapshot> operations)
        {
            if (summary?.HasCity != true)
            {
                return "The fast strip stays empty until the summary has a real settlement, a live seam, and a demand bend worth surfacing.";
            }

            if (operations == null || operations.Count == 0)
            {
                return BuildOperationEmptyNote(summary, lane);
            }

            if (string.Equals(lane, "black_market", StringComparison.OrdinalIgnoreCase))
            {
                return BuildShadowOperationStripCopy(summary, operations);
            }

            return BuildCivicOperationStripCopy(summary, operations);
        }

        private static string BuildOperationDemandSignal(OperationSnapshot operation, ShellSummarySnapshot summary, string lane)
        {
            if (operation == null)
            {
                return "No demand signal surfaced.";
            }

            if (string.Equals(lane, "black_market", StringComparison.OrdinalIgnoreCase))
            {
                return BuildShadowOperationDemandSignal(operation, summary);
            }

            return BuildCivicOperationDemandSignal(operation, summary);
        }

        private static int OperationDemandScore(OperationSnapshot operation, ShellSummarySnapshot summary, string lane)
        {
            if (operation == null)
            {
                return int.MinValue;
            }

            var score = 0;
            if (!string.IsNullOrWhiteSpace(operation.WhyNow)) score += 2;
            if (!string.IsNullOrWhiteSpace(operation.CtaLabel)) score += 1;
            if (!string.IsNullOrWhiteSpace(operation.FocusLabel)) score += 1;
            if (!string.IsNullOrWhiteSpace(operation.Payoff)) score += 1;

            if (string.Equals(lane, "black_market", StringComparison.OrdinalIgnoreCase))
            {
                score += ScoreShadowDemandShape(operation, summary);
            }
            else
            {
                score += ScoreCivicDemandShape(operation, summary);
            }

            return score;
        }

        private static int ScoreCivicDemandShape(OperationSnapshot operation, ShellSummarySnapshot summary)
        {
            var posture = BuildOperationPosture(operation, summary, lane: "city").ToLowerInvariant();
            var front = SelectPrimaryCivicFront(summary?.PublicBackbonePressureConvergence);
            var context = string.Join(" ", new[]
            {
                summary?.PublicBackbonePressureConvergence?.TradeWindow,
                summary?.PublicBackbonePressureConvergence?.FocusLane,
                summary?.PublicBackbonePressureConvergence?.RecommendedAction,
                summary?.PublicBackbonePressureConvergence?.Detail,
                summary?.PublicBackbonePressureConvergence?.LatestSupportReceipt?.Title,
                summary?.PublicBackbonePressureConvergence?.LatestSupportReceipt?.Summary,
                summary?.PublicBackbonePressureConvergence?.LatestSupportReceipt?.SourceSurface,
                summary?.PublicBackbonePressureConvergence?.ContractFollowThrough?.ContractTitle,
                summary?.PublicBackbonePressureConvergence?.ContractFollowThrough?.State,
                summary?.PublicBackbonePressureConvergence?.ContractFollowThrough?.Note,
                summary?.PublicBackbonePressureConvergence?.ContractEffects?.QueueEffect,
                summary?.PublicBackbonePressureConvergence?.ContractEffects?.TrustEffect,
                summary?.PublicBackbonePressureConvergence?.ContractEffects?.ServiceEffect,
                summary?.PublicBackbonePressureConvergence?.ContractEffects?.Note,
                front?.Id,
                front?.Headline,
                front?.Summary,
                front?.RecommendedAction,
                operation?.Title,
                operation?.Summary,
                operation?.Detail,
                operation?.WhyNow
            }).ToLowerInvariant();

            var score = 0;
            if (posture.Contains("caravan / reserve") && ContainsAny(context, "vendor_trade", "caravan", "reserve", "depot", "supply", "essentials", "hold window", "exchange")) score += 6;
            if (posture.Contains("investigation") && ContainsAny(context, "npc_city_services", "investig", "audit", "trace", "counterfeit", "records", "registry", "service")) score += 6;
            if (posture.Contains("public relief / repair") && ContainsAny(context, "public_backbone", "relief", "repair", "stabil", "backbone", "triage", "support floor")) score += 6;
            if (ContainsAny(context, "latest receipt", "support floor", "reserve", "exchange", "depot")) score += 2;
            return score;
        }

        private static int ScoreShadowDemandShape(OperationSnapshot operation, ShellSummarySnapshot summary)
        {
            var posture = BuildOperationPosture(operation, summary, lane: "black_market").ToLowerInvariant();
            var card = SelectPrimaryShadowCard(summary?.BlackMarketActiveOperation);
            var context = string.Join(" ", new[]
            {
                summary?.BlackMarketRuntimeTruth?.RuntimeBand,
                summary?.BlackMarketRuntimeTruth?.Headline,
                summary?.BlackMarketRuntimeTruth?.Detail,
                summary?.BlackMarketRuntimeTruth?.OperatorFrontSummary,
                summary?.BlackMarketRuntimeTruth?.PublicBackbonePressure?.State,
                summary?.BlackMarketRuntimeTruth?.PublicBackbonePressure?.RecommendedAction,
                summary?.BlackMarketRuntimeTruth?.PublicBackbonePressure?.Detail,
                summary?.BlackMarketBackbonePressure?.PressureState,
                summary?.BlackMarketBackbonePressure?.LeverageWindow,
                summary?.BlackMarketBackbonePressure?.RecommendedAction,
                summary?.BlackMarketBackbonePressure?.ContractFollowThrough?.ContractTitle,
                summary?.BlackMarketBackbonePressure?.ContractFollowThrough?.State,
                summary?.BlackMarketBackbonePressure?.ContractFollowThrough?.Note,
                summary?.BlackMarketBackbonePressure?.ContractEffects?.ReceiptChainState,
                summary?.BlackMarketBackbonePressure?.ContractEffects?.CovertCarryState,
                summary?.BlackMarketBackbonePressure?.ContractEffects?.LinkedReceiptTitle,
                summary?.BlackMarketBackbonePressure?.ContractEffects?.Note,
                summary?.BlackMarketPayoffRecovery?.Phase,
                summary?.BlackMarketPayoffRecovery?.Headline,
                summary?.BlackMarketPayoffRecovery?.Detail,
                summary?.BlackMarketPayoffRecovery?.RecommendedAction,
                summary?.BlackMarketPayoffRecovery?.StateReason,
                summary?.BlackMarketPayoffRecovery?.RecentReceipts?.FirstOrDefault()?.Title,
                summary?.BlackMarketPayoffRecovery?.RecentReceipts?.FirstOrDefault()?.Summary,
                card?.Kind,
                card?.Headline,
                card?.Summary,
                card?.OperatorNote,
                operation?.Title,
                operation?.Summary,
                operation?.Detail,
                operation?.WhyNow
            }).ToLowerInvariant();

            var score = 0;
            if (posture.Contains("counterfeit") && ContainsAny(context, "counterfeit", "permit", "throughput", "script", "window")) score += 6;
            if (posture.Contains("cover repair") && ContainsAny(context, "cover", "cleanup", "contain", "cooling", "backlash", "repair")) score += 6;
            if (posture.Contains("covert pressure") && ContainsAny(context, "pressure", "backbone", "warning", "bribe", "leverage")) score += 6;
            if (posture.Contains("covert exploit") && ContainsAny(context, "exploit", "payoff", "cash", "window", "active")) score += 6;
            if (ContainsAny(context, "receipt", "backlash", "pressure", "payoff")) score += 2;
            return score;
        }

        private static string BuildCivicOperationStripCopy(ShellSummarySnapshot summary, List<OperationSnapshot> operations)
        {
            var surface = summary?.PublicBackbonePressureConvergence;
            var strongest = operations.FirstOrDefault();
            var strongestPosture = BuildOperationPosture(strongest, summary, lane: "city");
            var receipt = surface?.LatestSupportReceipt;
            var followThrough = surface?.ContractFollowThrough;
            var effects = surface?.ContractEffects;
            var bend = BuildCivicDemandValue(surface);
            var receiptLine = receipt != null ? $"Latest receipt: {receipt.Title}." : string.Empty;
            var contractLine = effects != null
                ? $"Contract effects: {ContractTruthText.BuildCivicEffectsValue(effects, string.Empty)}."
                : followThrough != null
                    ? $"Contract lifecycle: {ContractTruthText.BuildContractSeamValue(followThrough, string.Empty)}."
                    : string.Empty;
            return FirstNonBlank(
                $"Board bend: {bend}. {contractLine} Leading posture: {strongestPosture}. {receiptLine}".Trim(),
                effects?.Note,
                followThrough?.Note,
                surface?.RecommendedAction,
                surface?.Detail,
                "The fast strip should rank the most demand-shaped civic answers first.");
        }

        private static string BuildShadowOperationStripCopy(ShellSummarySnapshot summary, List<OperationSnapshot> operations)
        {
            var runtime = summary?.BlackMarketRuntimeTruth;
            var backbone = summary?.BlackMarketBackbonePressure;
            var payoff = summary?.BlackMarketPayoffRecovery;
            var strongest = operations.FirstOrDefault();
            var strongestPosture = BuildOperationPosture(strongest, summary, lane: "black_market");
            var receipt = payoff?.RecentReceipts?.FirstOrDefault();
            var bend = BuildShadowDemandValue(runtime, backbone, payoff);
            var receiptLine = receipt != null ? $"Latest receipt: {receipt.Title}." : string.Empty;
            var contractLine = backbone?.ContractEffects != null
                ? $"Contract effects: {ContractTruthText.BuildShadowEffectsValue(backbone.ContractEffects, string.Empty)}."
                : backbone?.ContractFollowThrough != null
                    ? $"Contract lifecycle: {ContractTruthText.BuildContractSeamValue(backbone.ContractFollowThrough, string.Empty)}."
                    : string.Empty;
            return FirstNonBlank(
                $"Board bend: {bend}. {contractLine} Leading posture: {strongestPosture}. {receiptLine}".Trim(),
                backbone?.ContractEffects?.Note,
                backbone?.ContractFollowThrough?.Note,
                backbone?.RecommendedAction,
                runtime?.PublicBackbonePressure?.RecommendedAction,
                payoff?.RecommendedAction,
                runtime?.Detail,
                "The fast strip should rank the most demand-shaped shadow answers first.");
        }

        private static string BuildCivicOperationDemandSignal(OperationSnapshot operation, ShellSummarySnapshot summary)
        {
            var surface = summary?.PublicBackbonePressureConvergence;
            var posture = BuildOperationPosture(operation, summary, lane: "city");
            var receipt = surface?.LatestSupportReceipt;
            var bend = BuildCivicDemandValue(surface);

            if (receipt != null && PostureMatchesCivicReceipt(posture, receipt.SourceSurface))
            {
                return $"Receipt-led: {receipt.Title} • {HumanizeWords(receipt.SourceSurface, "support surface")}";
            }

            return $"{bend} • {posture}";
        }

        private static string BuildShadowOperationDemandSignal(OperationSnapshot operation, ShellSummarySnapshot summary)
        {
            var runtime = summary?.BlackMarketRuntimeTruth;
            var backbone = summary?.BlackMarketBackbonePressure;
            var payoff = summary?.BlackMarketPayoffRecovery;
            var receipt = payoff?.RecentReceipts?.FirstOrDefault();
            var posture = BuildOperationPosture(operation, summary, lane: "black_market");
            if (receipt != null && PostureMatchesShadowReceipt(posture, receipt))
            {
                return $"Receipt-led: {receipt.Title} • {HumanizeWords(receipt.Severity, "live pressure")}";
            }

            return $"{BuildShadowDemandValue(runtime, backbone, payoff)} • {posture}";
        }

        private static bool PostureMatchesCivicReceipt(string posture, string sourceSurface)
        {
            var postureText = (posture ?? string.Empty).ToLowerInvariant();
            var source = (sourceSurface ?? string.Empty).Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }

            if (source == "vendor_trade") return postureText.Contains("caravan / reserve");
            if (source == "npc_city_services") return postureText.Contains("investigation");
            if (source == "public_backbone") return postureText.Contains("public relief / repair");
            return false;
        }

        private static bool PostureMatchesShadowReceipt(string posture, BlackMarketPayoffRecoveryReceiptSnapshot receipt)
        {
            var postureText = (posture ?? string.Empty).ToLowerInvariant();
            var text = string.Join(" ", new[] { receipt?.Title, receipt?.Summary, receipt?.Detail, receipt?.RuntimeActionId, receipt?.Severity }).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            if (postureText.Contains("counterfeit")) return ContainsAny(text, "counterfeit", "throughput", "script", "permit");
            if (postureText.Contains("cover repair")) return ContainsAny(text, "cover", "cleanup", "contain", "repair", "backlash");
            if (postureText.Contains("covert pressure")) return ContainsAny(text, "pressure", "warning", "bribe", "leverage");
            if (postureText.Contains("covert exploit")) return ContainsAny(text, "exploit", "payoff", "cash");
            return false;
        }

        private static bool ContainsAny(string text, params string[] needles)
        {
            if (string.IsNullOrWhiteSpace(text) || needles == null || needles.Length == 0)
            {
                return false;
            }

            foreach (var needle in needles)
            {
                if (!string.IsNullOrWhiteSpace(needle) && text.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
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
            string demandNote,
            string handoffTitle,
            string handoffValue,
            string handoffNote,
            string consequenceTitle,
            string consequenceValue,
            string consequenceNote)
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
            if (pressureHandoffTitle != null) pressureHandoffTitle.text = handoffTitle;
            if (pressureHandoffValue != null) pressureHandoffValue.text = handoffValue;
            if (pressureHandoffNote != null) pressureHandoffNote.text = handoffNote;
            if (pressureConsequenceTitle != null) pressureConsequenceTitle.text = consequenceTitle;
            if (pressureConsequenceValue != null) pressureConsequenceValue.text = consequenceValue;
            if (pressureConsequenceNote != null) pressureConsequenceNote.text = consequenceNote;
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
            var contractEffectsState = (surface?.ContractEffects?.State ?? string.Empty).Trim().ToLowerInvariant();
            if (contractEffectsState == "cooling") return "cooling";
            if (contractEffectsState == "active") return "engaged";

            var lifecycleState = (surface?.ContractFollowThrough?.State ?? string.Empty).Trim().ToLowerInvariant();
            if (lifecycleState == "committed" || lifecycleState == "answered") return "engaged";
            if (lifecycleState == "available") return "forming";
            if (lifecycleState == "cooling") return "cooling";

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

        private static string MapShadowStage(BlackMarketActiveOperationCardSnapshot card, BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketBackbonePressureSurfaceSnapshot backbone, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var contractEffectsState = (backbone?.ContractEffects?.State ?? string.Empty).Trim().ToLowerInvariant();
            if (contractEffectsState == "cooling") return "cooling";
            if (contractEffectsState == "active") return "live";

            var lifecycleState = (backbone?.ContractFollowThrough?.State ?? string.Empty).Trim().ToLowerInvariant();
            if (lifecycleState == "committed" || lifecycleState == "answered") return "live";
            if (lifecycleState == "available") return "forming";
            if (lifecycleState == "cooling") return "cooling";

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

        private static string BuildShadowDemandValue(BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketBackbonePressureSurfaceSnapshot backbone, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            if (backbone != null)
            {
                var pressureState = HumanizeWords(backbone.PressureState, "quiet pressure");
                var leverageWindow = HumanizeWords(backbone.LeverageWindow, "closed window");
                return $"{pressureState} • {leverageWindow}";
            }

            var runtimePressureState = HumanizeWords(runtime?.PublicBackbonePressure?.State, "quiet public pressure");
            var payoffPhase = HumanizeWords(payoff?.Phase, "quiet payoff");
            return $"{runtimePressureState} • {payoffPhase}";
        }

        private static string BuildShadowDemandNote(BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketBackbonePressureSurfaceSnapshot backbone, BlackMarketPayoffRecoverySurfaceSnapshot payoff, OperationSnapshot primaryOp)
        {
            if (payoff?.RecentReceipts != null && payoff.RecentReceipts.Count > 0)
            {
                var receipt = payoff.RecentReceipts[0];
                return $"Latest receipt: {receipt.Title} • {receipt.Summary}";
            }

            return FirstNonBlank(
                backbone?.ContractEffects?.Note,
                backbone?.ContractFollowThrough?.Note,
                backbone?.RecommendedAction,
                runtime?.PublicBackbonePressure?.RecommendedAction,
                payoff?.RecommendedAction,
                primaryOp?.WhyNow,
                backbone?.Detail,
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
