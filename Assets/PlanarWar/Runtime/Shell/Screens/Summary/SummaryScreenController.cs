using PlanarWar.Client.Core.Application;
using PlanarWar.Client.Core.Contracts;
using PlanarWar.Client.Core.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly VisualElement timerDiagnosticCard;
        private readonly Button timerDiagnosticsButton;
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
        private readonly VisualElement postFounderHandoffCard;
        private readonly Label postFounderHandoffHeadline;
        private readonly Label postFounderHandoffCopy;
        private readonly Label postFounderHandoffDetail;
        private readonly Button postFounderDevelopmentButton;
        private readonly Button postFounderOperationsButton;
        private readonly Button postFounderRosterButton;
        private readonly VisualElement earlyLanePostureCard;
        private readonly Label earlyLanePostureBadge;
        private readonly Label earlyLanePostureHeadline;
        private readonly Label earlyLanePostureSummary;
        private readonly Label earlyLanePostureRecommended;
        private readonly Label earlyLanePostureReason;
        private readonly Label earlyLanePostureActionPathTitle;
        private readonly Label earlyLanePostureActionPathStep;
        private readonly Label earlyLanePostureActionPathWhy;
        private readonly Label earlyLanePostureActionPathReceipt;
        private readonly Label earlyLanePostureStrengths;
        private readonly Label earlyLanePostureLiabilities;
        private readonly Label earlyLanePostureProof;
        private readonly Button earlyLanePostureActionButton;
        private ShellScreen earlyLanePostureRecommendedScreen = ShellScreen.City;
        private readonly VisualElement motherBrainActionPathCard;
        private readonly Label motherBrainActionPathBadge;
        private readonly Label motherBrainActionPathHeadline;
        private readonly Label motherBrainActionPathDetail;
        private readonly Label motherBrainActionPathRecommended;
        private readonly Label motherBrainActionPathReason;
        private readonly Label motherBrainActionPathBlockers;
        private readonly Label motherBrainActionPathProof;
        private readonly Label motherBrainActionPathReceipt;
        private readonly Button motherBrainActionPathButton;
        private ShellScreen motherBrainActionPathRecommendedScreen = ShellScreen.BlackMarket;
        private readonly VisualElement publicInfrastructureEconomySpineCard;
        private readonly Label publicInfrastructureEconomySpineBadge;
        private readonly Label publicInfrastructureEconomySpineHeadline;
        private readonly Label publicInfrastructureEconomySpineSummary;
        private readonly Label publicInfrastructureEconomySpineRecommended;
        private readonly Label publicInfrastructureEconomySpineReason;
        private readonly Label publicInfrastructureEconomySpinePublicSignals;
        private readonly Label publicInfrastructureEconomySpineCitySignals;
        private readonly Label publicInfrastructureEconomySpineShadowSignals;
        private readonly Label publicInfrastructureEconomySpineReceipt;
        private readonly Button publicInfrastructureEconomySpineButton;
        private ShellScreen publicInfrastructureEconomySpineRecommendedScreen = ShellScreen.City;
        private readonly VisualElement cityMudConsequenceBridgeCard;
        private readonly Label cityMudConsequenceBridgeBadge;
        private readonly Label cityMudConsequenceBridgeHeadline;
        private readonly Label cityMudConsequenceBridgeSummary;
        private readonly Label cityMudConsequenceBridgeRecommended;
        private readonly Label cityMudConsequenceBridgeReason;
        private readonly Label cityMudConsequenceBridgeBridgeSignals;
        private readonly Label cityMudConsequenceBridgeProgressionSignals;
        private readonly Label cityMudConsequenceBridgeRegionalSignals;
        private readonly Label cityMudConsequenceBridgeReceiptSignals;
        private readonly Label cityMudConsequenceBridgeFollowThrough;
        private readonly Label cityMudConsequenceBridgeGuardrails;
        private readonly Button cityMudConsequenceBridgeButton;
        private ShellScreen cityMudConsequenceBridgeRecommendedScreen = ShellScreen.City;
        private readonly VisualElement cityContractRecoveryBoardCard;
        private readonly Label cityContractRecoveryBoardBadge;
        private readonly Label cityContractRecoveryBoardHeadline;
        private readonly Label cityContractRecoveryBoardSummary;
        private readonly Label cityContractRecoveryBoardRecommended;
        private readonly Label cityContractRecoveryBoardReason;
        private readonly Label cityContractRecoveryBoardRegions;
        private readonly Label cityContractRecoveryBoardCandidate;
        private readonly Label cityContractRecoveryBoardResources;
        private readonly Label cityContractRecoveryBoardReceipt;
        private readonly Label cityContractRecoveryBoardGuardrails;
        private readonly Button cityContractRecoveryBoardButton;
        private ShellScreen cityContractRecoveryBoardRecommendedScreen = ShellScreen.City;
        private readonly VisualElement founderSetupCard;
        private readonly TextField founderCityNameField;
        private readonly Label founderSetupHeadline;
        private readonly Label founderSetupCopy;
        private readonly Label founderActionStatus;
        private readonly Label founderCityChoiceValue;
        private readonly Label founderCityChoiceNote;
        private readonly Label founderMarketChoiceValue;
        private readonly Label founderMarketChoiceNote;
        private readonly Button founderCityPrimaryButton;
        private readonly Button founderMarketPrimaryButton;
        private readonly Button founderCityButton;
        private readonly Button founderMarketButton;
        private bool founderNameSeeded;
        private const bool TimerDiagnosticsDevFlagEnabled = false;
        private int heartbeat;

        public SummaryScreenController(VisualElement root, Func<string, string, Task> onBootstrapCityRequested = null, Action<ShellScreen> onNavigateRequested = null)
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
            timerDiagnosticCard = root.Q<VisualElement>("timer-diagnostic-card");
            timerDiagnosticsButton = root.Q<Button>("toggle-timer-diagnostics-button");
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
            postFounderHandoffCard = root.Q<VisualElement>("post-founder-handoff-card");
            postFounderHandoffHeadline = root.Q<Label>("post-founder-handoff-headline-value");
            postFounderHandoffCopy = root.Q<Label>("post-founder-handoff-copy-value");
            postFounderHandoffDetail = root.Q<Label>("post-founder-handoff-detail-value");
            postFounderDevelopmentButton = root.Q<Button>("post-founder-development-button");
            postFounderOperationsButton = root.Q<Button>("post-founder-operations-button");
            postFounderRosterButton = root.Q<Button>("post-founder-roster-button");
            earlyLanePostureCard = root.Q<VisualElement>("early-lane-posture-card");
            earlyLanePostureBadge = root.Q<Label>("early-lane-posture-badge-value");
            earlyLanePostureHeadline = root.Q<Label>("early-lane-posture-headline-value");
            earlyLanePostureSummary = root.Q<Label>("early-lane-posture-summary-value");
            earlyLanePostureRecommended = root.Q<Label>("early-lane-posture-recommended-value");
            earlyLanePostureReason = root.Q<Label>("early-lane-posture-reason-value");
            earlyLanePostureActionPathTitle = root.Q<Label>("early-lane-posture-action-path-title-value");
            earlyLanePostureActionPathStep = root.Q<Label>("early-lane-posture-action-path-step-value");
            earlyLanePostureActionPathWhy = root.Q<Label>("early-lane-posture-action-path-why-value");
            earlyLanePostureActionPathReceipt = root.Q<Label>("early-lane-posture-action-path-receipt-value");
            earlyLanePostureStrengths = root.Q<Label>("early-lane-posture-strengths-value");
            earlyLanePostureLiabilities = root.Q<Label>("early-lane-posture-liabilities-value");
            earlyLanePostureProof = root.Q<Label>("early-lane-posture-proof-value");
            earlyLanePostureActionButton = root.Q<Button>("early-lane-posture-action-button");
            motherBrainActionPathCard = root.Q<VisualElement>("mother-brain-action-path-card");
            motherBrainActionPathBadge = root.Q<Label>("mother-brain-action-path-badge-value");
            motherBrainActionPathHeadline = root.Q<Label>("mother-brain-action-path-headline-value");
            motherBrainActionPathDetail = root.Q<Label>("mother-brain-action-path-detail-value");
            motherBrainActionPathRecommended = root.Q<Label>("mother-brain-action-path-recommended-value");
            motherBrainActionPathReason = root.Q<Label>("mother-brain-action-path-reason-value");
            motherBrainActionPathBlockers = root.Q<Label>("mother-brain-action-path-blockers-value");
            motherBrainActionPathProof = root.Q<Label>("mother-brain-action-path-proof-value");
            motherBrainActionPathReceipt = root.Q<Label>("mother-brain-action-path-receipt-value");
            motherBrainActionPathButton = root.Q<Button>("mother-brain-action-path-button");
            publicInfrastructureEconomySpineCard = root.Q<VisualElement>("public-infrastructure-economy-spine-card");
            publicInfrastructureEconomySpineBadge = root.Q<Label>("public-infrastructure-economy-spine-badge-value");
            publicInfrastructureEconomySpineHeadline = root.Q<Label>("public-infrastructure-economy-spine-headline-value");
            publicInfrastructureEconomySpineSummary = root.Q<Label>("public-infrastructure-economy-spine-summary-value");
            publicInfrastructureEconomySpineRecommended = root.Q<Label>("public-infrastructure-economy-spine-recommended-value");
            publicInfrastructureEconomySpineReason = root.Q<Label>("public-infrastructure-economy-spine-reason-value");
            publicInfrastructureEconomySpinePublicSignals = root.Q<Label>("public-infrastructure-economy-spine-public-signals-value");
            publicInfrastructureEconomySpineCitySignals = root.Q<Label>("public-infrastructure-economy-spine-city-signals-value");
            publicInfrastructureEconomySpineShadowSignals = root.Q<Label>("public-infrastructure-economy-spine-shadow-signals-value");
            publicInfrastructureEconomySpineReceipt = root.Q<Label>("public-infrastructure-economy-spine-receipt-value");
            publicInfrastructureEconomySpineButton = root.Q<Button>("public-infrastructure-economy-spine-button");
            cityMudConsequenceBridgeCard = root.Q<VisualElement>("city-mud-consequence-bridge-card");
            cityMudConsequenceBridgeBadge = root.Q<Label>("city-mud-consequence-bridge-badge-value");
            cityMudConsequenceBridgeHeadline = root.Q<Label>("city-mud-consequence-bridge-headline-value");
            cityMudConsequenceBridgeSummary = root.Q<Label>("city-mud-consequence-bridge-summary-value");
            cityMudConsequenceBridgeRecommended = root.Q<Label>("city-mud-consequence-bridge-recommended-value");
            cityMudConsequenceBridgeReason = root.Q<Label>("city-mud-consequence-bridge-reason-value");
            cityMudConsequenceBridgeBridgeSignals = root.Q<Label>("city-mud-consequence-bridge-bridge-signals-value");
            cityMudConsequenceBridgeProgressionSignals = root.Q<Label>("city-mud-consequence-bridge-progression-signals-value");
            cityMudConsequenceBridgeRegionalSignals = root.Q<Label>("city-mud-consequence-bridge-regional-signals-value");
            cityMudConsequenceBridgeReceiptSignals = root.Q<Label>("city-mud-consequence-bridge-receipt-signals-value");
            cityMudConsequenceBridgeFollowThrough = root.Q<Label>("city-mud-consequence-bridge-follow-through-value");
            cityMudConsequenceBridgeGuardrails = root.Q<Label>("city-mud-consequence-bridge-guardrails-value");
            cityMudConsequenceBridgeButton = root.Q<Button>("city-mud-consequence-bridge-button");
            cityContractRecoveryBoardCard = root.Q<VisualElement>("city-contract-recovery-board-card");
            cityContractRecoveryBoardBadge = root.Q<Label>("city-contract-recovery-board-badge-value");
            cityContractRecoveryBoardHeadline = root.Q<Label>("city-contract-recovery-board-headline-value");
            cityContractRecoveryBoardSummary = root.Q<Label>("city-contract-recovery-board-summary-value");
            cityContractRecoveryBoardRecommended = root.Q<Label>("city-contract-recovery-board-recommended-value");
            cityContractRecoveryBoardReason = root.Q<Label>("city-contract-recovery-board-reason-value");
            cityContractRecoveryBoardRegions = root.Q<Label>("city-contract-recovery-board-regions-value");
            cityContractRecoveryBoardCandidate = root.Q<Label>("city-contract-recovery-board-candidate-value");
            cityContractRecoveryBoardResources = root.Q<Label>("city-contract-recovery-board-resources-value");
            cityContractRecoveryBoardReceipt = root.Q<Label>("city-contract-recovery-board-receipt-value");
            cityContractRecoveryBoardGuardrails = root.Q<Label>("city-contract-recovery-board-guardrails-value");
            cityContractRecoveryBoardButton = root.Q<Button>("city-contract-recovery-board-button");
            founderSetupCard = root.Q<VisualElement>("founder-setup-card");
            founderCityNameField = root.Q<TextField>("founder-city-name-field");
            founderSetupHeadline = root.Q<Label>("founder-setup-headline-value");
            founderSetupCopy = root.Q<Label>("founder-setup-copy-value");
            founderActionStatus = root.Q<Label>("founder-action-status-value");
            founderCityChoiceValue = root.Q<Label>("founder-city-choice-value");
            founderCityChoiceNote = root.Q<Label>("founder-city-choice-note");
            founderMarketChoiceValue = root.Q<Label>("founder-market-choice-value");
            founderMarketChoiceNote = root.Q<Label>("founder-market-choice-note");
            founderCityPrimaryButton = root.Q<Button>("founder-city-primary-button");
            founderMarketPrimaryButton = root.Q<Button>("founder-market-primary-button");
            founderCityButton = root.Q<Button>("founder-city-button");
            founderMarketButton = root.Q<Button>("founder-market-button");

            founderCityPrimaryButton?.RegisterCallback<ClickEvent>(_ => RequestSettlementBootstrap("city", onBootstrapCityRequested));
            founderMarketPrimaryButton?.RegisterCallback<ClickEvent>(_ => RequestSettlementBootstrap("black_market", onBootstrapCityRequested));
            founderCityButton?.RegisterCallback<ClickEvent>(_ => RequestSettlementBootstrap("city", onBootstrapCityRequested));
            founderMarketButton?.RegisterCallback<ClickEvent>(_ => RequestSettlementBootstrap("black_market", onBootstrapCityRequested));

            postFounderDevelopmentButton?.RegisterCallback<ClickEvent>(_ => RequestPostFounderNavigation(ShellScreen.City, onNavigateRequested));
            postFounderOperationsButton?.RegisterCallback<ClickEvent>(_ => RequestPostFounderNavigation(ShellScreen.BlackMarket, onNavigateRequested));
            postFounderRosterButton?.RegisterCallback<ClickEvent>(_ => RequestPostFounderNavigation(ShellScreen.Heroes, onNavigateRequested));
            earlyLanePostureActionButton?.RegisterCallback<ClickEvent>(_ => RequestPostFounderNavigation(earlyLanePostureRecommendedScreen, onNavigateRequested));
            motherBrainActionPathButton?.RegisterCallback<ClickEvent>(_ => RequestPostFounderNavigation(motherBrainActionPathRecommendedScreen, onNavigateRequested));
            publicInfrastructureEconomySpineButton?.RegisterCallback<ClickEvent>(_ => RequestPostFounderNavigation(publicInfrastructureEconomySpineRecommendedScreen, onNavigateRequested));
            cityMudConsequenceBridgeButton?.RegisterCallback<ClickEvent>(_ => RequestPostFounderNavigation(cityMudConsequenceBridgeRecommendedScreen, onNavigateRequested));
            cityContractRecoveryBoardButton?.RegisterCallback<ClickEvent>(_ => RequestPostFounderNavigation(cityContractRecoveryBoardRecommendedScreen, onNavigateRequested));
        }

        public void Render(ShellSummarySnapshot s, bool isSummaryLoaded, bool isActionBusy = false, string actionStatus = null, bool actionFailed = false)
        {
            heartbeat++;
            var nowUtc = DateTime.UtcNow;

            var activeResearches = SelectActiveResearches(s, nowUtc);
            statusHeadline.text = s.HasCity ? $"{s.City.Name} • {s.City.SettlementLaneLabel}" : (s.FounderMode ? "Founder mode active." : "No settlement loaded.");
            resources.text = FormatResource(s.Resources, s.ResourceLabels, "No resources loaded.");
            production.text = FormatResource(s.ProductionPerTick, s.ResourceLabels, s.HasCity ? "No production snapshot." : "Found a city to unlock production.", "/tick");
            research.text = FormatResearchSummary(activeResearches, nowUtc);
            warnings.text = s.ThreatWarnings.Count == 0 ? "No active threat warnings." : s.ThreatWarnings[0].Headline;
            readyOps.text = s.OpeningOperations.Count == 0 ? "No opening operations surfaced." : BuildReadyOpsSummary(s.OpeningOperations);
            heroes.text = s.Heroes.Count == 0 ? (s.HasCity ? "No officer corps visible." : "Found a city to unlock officers.") : $"{s.Heroes.Count(h => h.Status == "idle")}/{s.Heroes.Count} idle • {s.Heroes.Count(h => h.AttachmentCount > 0)} geared";
            armies.text = s.Armies.Count == 0 ? (s.HasCity ? "No formations visible." : "Found a city to unlock formations.") : $"{s.Armies.Count(a => (a.Readiness ?? 0) >= 70)}/{s.Armies.Count} ready";
            researchTimer.text = FormatResearchTimer(activeResearches, nowUtc);
            workshopTimer.text = FormatWorkshopAndBuild(s, nowUtc);
            missionTimer.text = FormatMission(s.ActiveMissions);
            resourceTick.text = FormatTick(s.ResourceTickTiming);
            RenderTimerDiagnostics(s, isSummaryLoaded, nowUtc);
            RenderFounderSetup(s, isSummaryLoaded, isActionBusy, actionStatus, actionFailed);
            RenderPostFounderHandoff(s, isSummaryLoaded);
            RenderEarlyLanePosture(s, isSummaryLoaded);
            RenderMotherBrainActionPath(s, isSummaryLoaded);
            RenderPublicInfrastructureEconomySpine(s, isSummaryLoaded);
            RenderCityMudConsequenceBridge(s, isSummaryLoaded);
            RenderCityContractRecoveryBoard(s, isSummaryLoaded);

            RenderPressureDesk(s);
        }

        private async void RequestSettlementBootstrap(string lane, Func<string, string, Task> onBootstrapCityRequested)
        {
            if (onBootstrapCityRequested == null)
            {
                return;
            }

            var cityName = founderCityNameField?.value?.Trim() ?? string.Empty;
            await onBootstrapCityRequested(cityName, lane);
        }

        private static void RequestPostFounderNavigation(ShellScreen screen, Action<ShellScreen> onNavigateRequested)
        {
            onNavigateRequested?.Invoke(screen);
        }

        private void RenderPostFounderHandoff(ShellSummarySnapshot summary, bool isSummaryLoaded)
        {
            var shouldShow = isSummaryLoaded && summary != null && summary.HasCity;
            if (postFounderHandoffCard != null)
            {
                postFounderHandoffCard.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (!shouldShow)
            {
                return;
            }

            var lane = NormalizeLane(summary.City?.SettlementLane);
            var isBlackMarket = string.Equals(lane, "black_market", StringComparison.OrdinalIgnoreCase);
            var rosterLabel = isBlackMarket ? "Open Operatives" : "Open Heroes";

            if (postFounderHandoffHeadline != null)
            {
                postFounderHandoffHeadline.text = isBlackMarket
                    ? "Black Market is live. Pick the next desk."
                    : "City is live. Pick the next desk.";
            }

            if (postFounderHandoffCopy != null)
            {
                postFounderHandoffCopy.text = isBlackMarket
                    ? "Use Development for fronts, workshop, and shadow-book research. Use Operations for cells, routes, missions, and pressure. Use Operatives for contacts and gear."
                    : "Use Development for buildings, workshop, and research. Use Operations for missions and formations. Use Heroes for recruitment and gear.";
            }

            if (postFounderHandoffDetail != null)
            {
                postFounderHandoffDetail.text = "These buttons only change the client desk; they do not invent setup progress, rewards, timers, inventory, or town layout state.";
            }

            if (postFounderRosterButton != null)
            {
                postFounderRosterButton.text = rosterLabel;
            }

            postFounderDevelopmentButton?.SetEnabled(true);
            postFounderOperationsButton?.SetEnabled(true);
            postFounderRosterButton?.SetEnabled(true);
        }

        private void RenderEarlyLanePosture(ShellSummarySnapshot summary, bool isSummaryLoaded)
        {
            var posture = summary?.EarlyLanePosture;
            var shouldShow = isSummaryLoaded && summary != null && summary.HasCity && posture != null;
            if (earlyLanePostureCard != null)
            {
                earlyLanePostureCard.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (!shouldShow)
            {
                return;
            }

            var label = FirstNonBlank(posture.Label, ResolveLaneLabel(posture.Lane, summary.City?.SettlementLaneLabel));
            var headline = FirstNonBlank(posture.Headline, $"{label} lane posture");
            var summaryText = FirstNonBlank(posture.Summary, "Backend lane posture is live, but no summary copy was provided.");
            var actionPath = posture.ActionPath;
            var recommendedDesk = FirstNonBlank(actionPath?.RecommendedDesk, posture.RecommendedDesk);
            var recommendedAction = FirstNonBlank(actionPath?.RecommendedActionLabel, posture.RecommendedActionLabel, BuildFallbackPostureAction(posture, summary));
            var reason = FirstNonBlank(actionPath?.WhyThisMatters, posture.NextStepReason, "Recommendation comes from live /api/me earlyLanePosture truth.");
            var recommendedScreen = ResolvePostureScreen(recommendedDesk, summary);
            earlyLanePostureRecommendedScreen = recommendedScreen;

            if (earlyLanePostureBadge != null)
            {
                earlyLanePostureBadge.text = $"{label} • backend posture";
            }

            if (earlyLanePostureHeadline != null)
            {
                earlyLanePostureHeadline.text = headline;
            }

            if (earlyLanePostureSummary != null)
            {
                earlyLanePostureSummary.text = summaryText;
            }

            if (earlyLanePostureRecommended != null)
            {
                earlyLanePostureRecommended.text = recommendedAction;
            }

            if (earlyLanePostureReason != null)
            {
                earlyLanePostureReason.text = reason;
            }

            if (earlyLanePostureActionPathTitle != null)
            {
                earlyLanePostureActionPathTitle.text = FirstNonBlank(actionPath?.Title, "First-hour action path pending");
            }

            if (earlyLanePostureActionPathStep != null)
            {
                earlyLanePostureActionPathStep.text = FirstNonBlank(actionPath?.CurrentStep, posture.NextStepReason, "No first-hour action path step surfaced yet.");
            }

            if (earlyLanePostureActionPathWhy != null)
            {
                earlyLanePostureActionPathWhy.text = FirstNonBlank(actionPath?.WhyThisMatters, "This path will appear when the backend exposes live action-path truth.");
            }

            if (earlyLanePostureActionPathReceipt != null)
            {
                earlyLanePostureActionPathReceipt.text = string.IsNullOrWhiteSpace(actionPath?.NextReceiptFamily)
                    ? "Next receipt family: not surfaced yet."
                    : $"Next receipt family: {HumanizeToken(actionPath.NextReceiptFamily)}.";
            }

            if (earlyLanePostureStrengths != null)
            {
                earlyLanePostureStrengths.text = FormatPostureList(posture.Strengths, "No live strength signals surfaced yet.");
            }

            if (earlyLanePostureLiabilities != null)
            {
                earlyLanePostureLiabilities.text = FormatPostureList(posture.Liabilities, "No live liability signals surfaced yet.");
            }

            if (earlyLanePostureProof != null)
            {
                var proofSignals = actionPath?.LiveProofSignals != null && actionPath.LiveProofSignals.Count > 0
                    ? actionPath.LiveProofSignals
                    : posture.ProofSignals;
                earlyLanePostureProof.text = FormatPostureList(proofSignals, "No proof signals surfaced yet.");
            }

            if (earlyLanePostureActionButton != null)
            {
                earlyLanePostureActionButton.text = BuildPostureButtonLabel(recommendedScreen, summary);
                earlyLanePostureActionButton.SetEnabled(true);
            }
        }


        private void RenderPublicInfrastructureEconomySpine(ShellSummarySnapshot summary, bool isSummaryLoaded)
        {
            var infrastructure = summary?.PublicInfrastructureSummary;
            var spine = infrastructure?.EconomySpine;
            var shouldShow = isSummaryLoaded && summary != null && summary.HasCity && spine != null;
            if (publicInfrastructureEconomySpineCard != null)
            {
                publicInfrastructureEconomySpineCard.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (!shouldShow)
            {
                return;
            }

            var recommendedScreen = ResolvePublicInfrastructureScreen(spine);
            publicInfrastructureEconomySpineRecommendedScreen = recommendedScreen;

            if (publicInfrastructureEconomySpineBadge != null)
            {
                var state = FirstNonBlank(spine.State, infrastructure.StrainBand, "watch");
                publicInfrastructureEconomySpineBadge.text = $"Public spine • {HumanizeToken(state)}";
            }

            if (publicInfrastructureEconomySpineHeadline != null)
            {
                publicInfrastructureEconomySpineHeadline.text = FirstNonBlank(spine.Title, "Public infrastructure economy spine");
            }

            if (publicInfrastructureEconomySpineSummary != null)
            {
                publicInfrastructureEconomySpineSummary.text = FirstNonBlank(spine.Summary, "NPC public services remain the baseline economy spine while player-city infrastructure optimizes the lane.");
            }

            if (publicInfrastructureEconomySpineRecommended != null)
            {
                publicInfrastructureEconomySpineRecommended.text = FirstNonBlank(spine.RecommendedActionLabel, $"Open {BuildDeskNoun(recommendedScreen, summary)}");
            }

            if (publicInfrastructureEconomySpineReason != null)
            {
                publicInfrastructureEconomySpineReason.text = FirstNonBlank(spine.WhyThisMatters, "This surface reads public-backbone and city-economy pressure only; it does not apply fake taxes, queues, protection, rewards, or service outcomes.");
            }

            if (publicInfrastructureEconomySpinePublicSignals != null)
            {
                publicInfrastructureEconomySpinePublicSignals.text = FormatPostureList(spine.PublicBackboneSignals, "No public-backbone signals surfaced yet.");
            }

            if (publicInfrastructureEconomySpineCitySignals != null)
            {
                publicInfrastructureEconomySpineCitySignals.text = FormatPostureList(spine.CityEconomySignals, "No city-economy signals surfaced yet.");
            }

            if (publicInfrastructureEconomySpineShadowSignals != null)
            {
                publicInfrastructureEconomySpineShadowSignals.text = FormatPostureList(spine.ShadowRiskSignals, "No shadow-risk signals surfaced yet.");
            }

            if (publicInfrastructureEconomySpineReceipt != null)
            {
                publicInfrastructureEconomySpineReceipt.text = FormatPublicInfrastructureReceipt(spine, infrastructure);
            }

            if (publicInfrastructureEconomySpineButton != null)
            {
                publicInfrastructureEconomySpineButton.text = BuildPostureButtonLabel(recommendedScreen, summary);
                publicInfrastructureEconomySpineButton.SetEnabled(true);
            }
        }

        private static ShellScreen ResolvePublicInfrastructureScreen(PublicInfrastructureEconomySpineSnapshot spine)
        {
            var service = (spine?.RecommendedService ?? string.Empty).Trim().Replace("-", "_").Replace(" ", "_").ToLowerInvariant();
            if (service == "hero_recruit")
            {
                return ShellScreen.Heroes;
            }

            return ShellScreen.City;
        }

        private static string FormatPublicInfrastructureReceipt(PublicInfrastructureEconomySpineSnapshot spine, PublicInfrastructureSummarySnapshot infrastructure)
        {
            var lines = new List<string>();
            var followThrough = spine?.ReceiptFollowThrough;

            if (followThrough != null)
            {
                if (!string.IsNullOrWhiteSpace(followThrough.State))
                {
                    lines.Add($"Follow-through state: {HumanizeToken(followThrough.State)}.");
                }

                if (!string.IsNullOrWhiteSpace(followThrough.Title))
                {
                    lines.Add(followThrough.Title);
                }

                if (!string.IsNullOrWhiteSpace(followThrough.Summary))
                {
                    lines.Add(followThrough.Summary);
                }

                if (!string.IsNullOrWhiteSpace(followThrough.LatestReceiptId) || !string.IsNullOrWhiteSpace(followThrough.LatestReceiptAt))
                {
                    lines.Add($"Latest receipt: {FirstNonBlank(followThrough.LatestReceiptId, "unknown")} at {FirstNonBlank(followThrough.LatestReceiptAt, "unknown time")}.");
                }

                if (!string.IsNullOrWhiteSpace(followThrough.LatestService) || !string.IsNullOrWhiteSpace(followThrough.LatestMode))
                {
                    lines.Add($"Latest service/mode: {HumanizeToken(followThrough.LatestService)} / {HumanizeToken(followThrough.LatestMode)}.");
                }

                if (!string.IsNullOrWhiteSpace(followThrough.LatestPermitTier))
                {
                    lines.Add($"Latest permit tier: {HumanizeToken(followThrough.LatestPermitTier)}.");
                }

                if (followThrough.LatestQueueMinutes.HasValue || followThrough.LatestStrainScore.HasValue)
                {
                    lines.Add($"Latest queue/strain: {followThrough.LatestQueueMinutes ?? 0}m / {followThrough.LatestStrainScore ?? 0}/100.");
                }

                if (!string.IsNullOrWhiteSpace(followThrough.LatestRunwayDoctrine) || !string.IsNullOrWhiteSpace(followThrough.LatestRunwayStatus))
                {
                    lines.Add($"Runway context: {HumanizeToken(followThrough.LatestRunwayDoctrine)} / {HumanizeToken(followThrough.LatestRunwayStatus)}.");
                }

                if (followThrough.ReceiptCount.HasValue)
                {
                    lines.Add($"Receipt count: {followThrough.ReceiptCount.Value}.");
                }

                if (!string.IsNullOrWhiteSpace(followThrough.RecommendedMode) || !string.IsNullOrWhiteSpace(followThrough.RecommendedService))
                {
                    lines.Add($"Recommended receipt path: {HumanizeToken(followThrough.RecommendedMode)} / {HumanizeToken(followThrough.RecommendedService)}.");
                }

                if (!string.IsNullOrWhiteSpace(followThrough.NextReceiptFamily))
                {
                    lines.Add($"Next receipt family: {HumanizeToken(followThrough.NextReceiptFamily)}.");
                }

                if (followThrough.Signals != null && followThrough.Signals.Count > 0)
                {
                    lines.Add(FormatPostureList(followThrough.Signals, string.Empty));
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(infrastructure?.PermitTier))
                {
                    lines.Add($"Permit tier: {HumanizeToken(infrastructure.PermitTier)}.");
                }

                if (!string.IsNullOrWhiteSpace(infrastructure?.RecommendedMode))
                {
                    lines.Add($"Recommended mode: {HumanizeToken(infrastructure.RecommendedMode)}.");
                }

                if (!string.IsNullOrWhiteSpace(spine?.RecommendedService))
                {
                    lines.Add($"Recommended service: {HumanizeToken(spine.RecommendedService)}.");
                }

                if (!string.IsNullOrWhiteSpace(spine?.NextReceiptFamily))
                {
                    lines.Add($"Next receipt family: {HumanizeToken(spine.NextReceiptFamily)}.");
                }

                if (infrastructure?.ServiceHeat.HasValue == true || infrastructure?.QueuePressure.HasValue == true || infrastructure?.PressureScore.HasValue == true)
                {
                    lines.Add($"Heat/queue/pressure: {infrastructure.ServiceHeat ?? 0}/{infrastructure.QueuePressure ?? 0}/{infrastructure.PressureScore ?? 0}.");
                }

                if (!string.IsNullOrWhiteSpace(infrastructure?.CityStressStage) || infrastructure?.CityStressTotal.HasValue == true)
                {
                    lines.Add($"City stress: {HumanizeToken(infrastructure.CityStressStage)} {infrastructure.CityStressTotal ?? 0}.");
                }
            }

            if (lines.Count > 0)
            {
                return string.Join("\n", lines.Where(line => !string.IsNullOrWhiteSpace(line)));
            }

            return "Public infrastructure receipt follow-through is waiting on backend state.";
        }



        private void RenderCityContractRecoveryBoard(ShellSummarySnapshot summary, bool isSummaryLoaded)
        {
            var board = summary?.CityContractRecoveryBoard;
            var shouldShow = isSummaryLoaded && summary != null && summary.HasCity && board != null;
            if (cityContractRecoveryBoardCard != null)
            {
                cityContractRecoveryBoardCard.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (!shouldShow)
            {
                return;
            }

            var recommendedScreen = ResolveCityContractRecoveryBoardScreen(board);
            cityContractRecoveryBoardRecommendedScreen = recommendedScreen;
            var lead = board.Candidates != null && board.Candidates.Count > 0 ? board.Candidates[0] : null;

            if (cityContractRecoveryBoardBadge != null)
            {
                cityContractRecoveryBoardBadge.text = $"Recovery board • {HumanizeToken(FirstNonBlank(board.State, board.RecommendedFocus, "watch"))}";
            }

            if (cityContractRecoveryBoardHeadline != null)
            {
                cityContractRecoveryBoardHeadline.text = FirstNonBlank(board.Title, "City contract recovery board");
            }

            if (cityContractRecoveryBoardSummary != null)
            {
                cityContractRecoveryBoardSummary.text = FirstNonBlank(board.Summary, "Regional recovery candidates from /api/me.cityContractRecoveryBoard will appear here when backend truth supports them.");
            }

            if (cityContractRecoveryBoardRecommended != null)
            {
                cityContractRecoveryBoardRecommended.text = FirstNonBlank(board.RecommendedCityDeskAction, ContractTruthText.BuildCityContractRecoveryBoardValue(board, "Watch regional recovery truth."));
            }

            if (cityContractRecoveryBoardReason != null)
            {
                cityContractRecoveryBoardReason.text = ContractTruthText.BuildCityContractRecoveryBoardNote(board, "This read-only board consumes existing consequence, receipt, and city-support truth only; it does not execute contracts or invent rewards.");
            }

            if (cityContractRecoveryBoardRegions != null)
            {
                cityContractRecoveryBoardRegions.text = FormatCityContractRecoveryRegions(board);
            }

            if (cityContractRecoveryBoardCandidate != null)
            {
                cityContractRecoveryBoardCandidate.text = lead == null
                    ? "No candidate card is strong enough to list yet."
                    : ContractTruthText.BuildCityContractRecoveryCandidateValue(lead, "Regional recovery candidate waiting on backend summary.");
            }

            if (cityContractRecoveryBoardResources != null)
            {
                cityContractRecoveryBoardResources.text = lead == null
                    ? "Runtime resource requirement: none surfaced."
                    : ContractTruthText.BuildCityContractRecoveryResourcesValue(lead.RequiredResources, "Runtime resource requirement: advisory only.");
            }

            if (cityContractRecoveryBoardReceipt != null)
            {
                cityContractRecoveryBoardReceipt.text = FormatCityContractRecoveryReceipt(board, lead);
            }

            if (cityContractRecoveryBoardGuardrails != null)
            {
                cityContractRecoveryBoardGuardrails.text = FormatPostureList(board.Guardrails, "Read-only board: no fake rewards, item grants, levels, taxes, queue timers, protection, exposure, Rogue Director, TOMS, Crucible, or autonomous Mother Brain behavior.");
            }

            if (cityContractRecoveryBoardButton != null)
            {
                cityContractRecoveryBoardButton.text = BuildPostureButtonLabel(recommendedScreen, summary);
                cityContractRecoveryBoardButton.SetEnabled(true);
            }
        }

        private static ShellScreen ResolveCityContractRecoveryBoardScreen(CityContractRecoveryBoardSnapshot board)
        {
            var lane = NormalizeLane(board?.SettlementLane);
            if (lane == "black_market")
            {
                return ShellScreen.BlackMarket;
            }

            var focus = (board?.RecommendedFocus ?? string.Empty).Trim().Replace("-", "_").Replace(" ", "_").ToLowerInvariant();
            if (focus == "regional_recovery" || focus == "public_backbone" || focus == "city_support")
            {
                return ShellScreen.City;
            }

            return ShellScreen.City;
        }

        private static string FormatCityContractRecoveryRegions(CityContractRecoveryBoardSnapshot board)
        {
            if (board?.EligibleRegionIds != null && board.EligibleRegionIds.Count > 0)
            {
                return $"Eligible regions: {string.Join(", ", board.EligibleRegionIds)}.";
            }

            return "Eligible regions: none surfaced yet.";
        }

        private static string FormatCityContractRecoveryReceipt(CityContractRecoveryBoardSnapshot board, CityContractRecoveryCandidateSnapshot lead)
        {
            var lines = new List<string>();
            if (!string.IsNullOrWhiteSpace(lead?.NextReceiptFamily))
            {
                lines.Add($"Next receipt family: {HumanizeToken(lead.NextReceiptFamily)}.");
            }

            var receipt = board?.LatestRelevantReceipt ?? lead?.LatestRelevantSummary ?? board?.LatestRelevantConsequence;
            if (receipt != null)
            {
                lines.Add($"Latest relevant truth: {FirstNonBlank(receipt.Title, receipt.Id, "receipt")} • {HumanizeToken(receipt.Severity)} • {FirstNonBlank(receipt.RegionId, "unknown region")}.");
            }

            if (lead?.RecommendedMoves != null && lead.RecommendedMoves.Count > 0)
            {
                lines.Add(FormatPostureList(lead.RecommendedMoves, string.Empty));
            }

            return lines.Count > 0
                ? string.Join("\n", lines.Where(line => !string.IsNullOrWhiteSpace(line)))
                : "No latest recovery receipt or consequence summary surfaced yet.";
        }

        private void RenderCityMudConsequenceBridge(ShellSummarySnapshot summary, bool isSummaryLoaded)
        {
            var bridge = summary?.CityMudWorldConsequenceBridge;
            var shouldShow = isSummaryLoaded && summary != null && summary.HasCity && bridge != null;
            if (cityMudConsequenceBridgeCard != null)
            {
                cityMudConsequenceBridgeCard.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (!shouldShow)
            {
                return;
            }

            var recommendedScreen = ResolveCityMudConsequenceBridgeScreen(bridge);
            cityMudConsequenceBridgeRecommendedScreen = recommendedScreen;

            if (cityMudConsequenceBridgeBadge != null)
            {
                cityMudConsequenceBridgeBadge.text = $"City ↔ MUD • {HumanizeToken(FirstNonBlank(bridge.State, bridge.RecommendedFocus, "watch"))}";
            }

            if (cityMudConsequenceBridgeHeadline != null)
            {
                cityMudConsequenceBridgeHeadline.text = FirstNonBlank(bridge.Title, "City-to-MUD consequence bridge");
            }

            if (cityMudConsequenceBridgeSummary != null)
            {
                cityMudConsequenceBridgeSummary.text = FirstNonBlank(bridge.Summary, "City support and world-consequence truth will appear here when /api/me exposes the bridge.");
            }

            if (cityMudConsequenceBridgeRecommended != null)
            {
                cityMudConsequenceBridgeRecommended.text = FirstNonBlank(bridge.RecommendedActionLabel, $"Open {BuildDeskNoun(recommendedScreen, summary)}");
            }

            if (cityMudConsequenceBridgeReason != null)
            {
                cityMudConsequenceBridgeReason.text = FirstNonBlank(bridge.WhyThisMatters, "This bridge reads city support, public backbone, regional consequence, and receipt truth only; it does not grant items, fake progression, taxes, queues, or mandatory city gates.");
            }

            if (cityMudConsequenceBridgeBridgeSignals != null)
            {
                cityMudConsequenceBridgeBridgeSignals.text = FormatCityMudBridgeSignals(bridge);
            }

            if (cityMudConsequenceBridgeProgressionSignals != null)
            {
                cityMudConsequenceBridgeProgressionSignals.text = FormatPostureList(bridge.MudProgressionSignals, "No MUD progression signals surfaced yet.");
            }

            if (cityMudConsequenceBridgeRegionalSignals != null)
            {
                cityMudConsequenceBridgeRegionalSignals.text = FormatRegionalLifeSignals(bridge);
            }

            if (cityMudConsequenceBridgeReceiptSignals != null)
            {
                cityMudConsequenceBridgeReceiptSignals.text = FormatCityMudBridgeReceiptSignals(bridge);
            }

            if (cityMudConsequenceBridgeFollowThrough != null)
            {
                cityMudConsequenceBridgeFollowThrough.text = FormatCityMudBridgeFollowThrough(bridge);
            }

            if (cityMudConsequenceBridgeGuardrails != null)
            {
                cityMudConsequenceBridgeGuardrails.text = FormatPostureList(bridge.Guardrails, "Guardrails: no fake MUD progression, rewards, taxes, queue timers, or mandatory player-city gates.");
            }

            if (cityMudConsequenceBridgeButton != null)
            {
                cityMudConsequenceBridgeButton.text = BuildPostureButtonLabel(recommendedScreen, summary);
                cityMudConsequenceBridgeButton.SetEnabled(true);
            }
        }

        private static ShellScreen ResolveCityMudConsequenceBridgeScreen(CityMudWorldConsequenceBridgeSnapshot bridge)
        {
            var focus = (bridge?.RecommendedFocus ?? string.Empty).Trim().Replace("-", "_").Replace(" ", "_").ToLowerInvariant();
            if (focus == "regional_recovery")
            {
                return ShellScreen.BlackMarket;
            }

            if (focus == "city_support" || focus == "public_backbone")
            {
                return ShellScreen.City;
            }

            return ShellScreen.City;
        }

        private static string FormatCityMudBridgeSignals(CityMudWorldConsequenceBridgeSnapshot bridge)
        {
            var lines = new List<string>
            {
                $"Bridge band: {HumanizeToken(bridge?.BridgeBand)}.",
                $"Recommended posture: {HumanizeToken(bridge?.RecommendedPosture)}.",
                $"Support/logistics/frontier/stability: {bridge?.SupportCapacity ?? 0}/{bridge?.LogisticsPressure ?? 0}/{bridge?.FrontierPressure ?? 0}/{bridge?.StabilityPressure ?? 0}.",
            };

            var exportable = FormatResource(bridge?.ExportableResources ?? new ResourceSnapshot(), new ResourcePresentationSnapshot(), "No exportable city support surfaced.");
            if (!string.IsNullOrWhiteSpace(exportable))
            {
                lines.Add($"Exportable support: {exportable}.");
            }

            if (bridge?.CityMudSignals != null && bridge.CityMudSignals.Count > 0)
            {
                lines.Add(FormatPostureList(bridge.CityMudSignals, string.Empty));
            }

            return string.Join("\n", lines.Where(line => !string.IsNullOrWhiteSpace(line)));
        }

        private static string FormatRegionalLifeSignals(CityMudWorldConsequenceBridgeSnapshot bridge)
        {
            var lines = new List<string>
            {
                $"Affected regions: {((bridge?.AffectedRegionIds != null && bridge.AffectedRegionIds.Count > 0) ? string.Join(", ", bridge.AffectedRegionIds) : "none")}.",
                $"World consequences: {bridge?.WorldConsequenceTotal ?? 0}; severe {bridge?.SevereConsequenceCount ?? 0}; destabilization {bridge?.DestabilizationScore ?? 0}.",
            };

            if (bridge?.RegionalLifeSignals != null && bridge.RegionalLifeSignals.Count > 0)
            {
                lines.Add(FormatPostureList(bridge.RegionalLifeSignals, string.Empty));
            }

            return string.Join("\n", lines.Where(line => !string.IsNullOrWhiteSpace(line)));
        }

        private static string FormatCityMudBridgeReceiptSignals(CityMudWorldConsequenceBridgeSnapshot bridge)
        {
            var lines = new List<string>();

            if (!string.IsNullOrWhiteSpace(bridge?.NextReceiptFamily))
            {
                lines.Add($"Next receipt family: {HumanizeToken(bridge.NextReceiptFamily)}.");
            }

            if (bridge?.LatestRuntimeResponse != null)
            {
                lines.Add($"Latest runtime response: {FormatCityMudBridgeReceipt(bridge.LatestRuntimeResponse)}");
            }

            if (bridge?.LatestWorldConsequence != null)
            {
                lines.Add($"Latest world consequence: {FormatCityMudBridgeReceipt(bridge.LatestWorldConsequence)}");
            }

            if (bridge?.ReceiptSignals != null && bridge.ReceiptSignals.Count > 0)
            {
                lines.Add(FormatPostureList(bridge.ReceiptSignals, string.Empty));
            }

            if (lines.Count == 0)
            {
                return "No City ↔ MUD receipt signals surfaced yet.";
            }

            return string.Join("\n", lines.Where(line => !string.IsNullOrWhiteSpace(line)));
        }

        private static string FormatCityMudBridgeFollowThrough(CityMudWorldConsequenceBridgeSnapshot bridge)
        {
            var followThrough = bridge?.FollowThrough;
            if (followThrough == null)
            {
                return "City ↔ MUD bridge follow-through is waiting on backend state.";
            }

            var lines = new List<string>
            {
                $"Follow-through state: {HumanizeToken(followThrough.State)}.",
                FirstNonBlank(followThrough.Title, "City ↔ MUD bridge follow-through"),
                FirstNonBlank(followThrough.Summary, string.Empty),
            };

            if (!string.IsNullOrWhiteSpace(followThrough.RecommendedActionLabel))
            {
                lines.Add($"Recommended follow-through: {followThrough.RecommendedActionLabel}");
            }

            if (!string.IsNullOrWhiteSpace(followThrough.RecommendedFocus))
            {
                lines.Add($"Recommended focus: {HumanizeToken(followThrough.RecommendedFocus)}.");
            }

            if (!string.IsNullOrWhiteSpace(followThrough.NextReceiptFamily))
            {
                lines.Add($"Next receipt family: {HumanizeToken(followThrough.NextReceiptFamily)}.");
            }

            if (!string.IsNullOrWhiteSpace(followThrough.LatestRuntimeResponseTitle))
            {
                lines.Add($"Latest runtime response: {followThrough.LatestRuntimeResponseTitle}{FormatOptionalTokenSuffix(followThrough.LatestRuntimeResponseOutcome, followThrough.LatestRuntimeActionId)}.");
            }

            if (!string.IsNullOrWhiteSpace(followThrough.LatestWorldConsequenceTitle))
            {
                lines.Add($"Latest world consequence: {followThrough.LatestWorldConsequenceTitle}{FormatOptionalTokenSuffix(string.Empty, followThrough.LatestWorldConsequenceAt)}.");
            }

            if (!string.IsNullOrWhiteSpace(followThrough.LatestBridgeReceiptTitle))
            {
                lines.Add($"Latest bridge receipt: {followThrough.LatestBridgeReceiptTitle}{FormatOptionalTokenSuffix(string.Empty, followThrough.LatestBridgeReceiptAt)}.");
            }

            if (followThrough.ClearWhen != null && followThrough.ClearWhen.Count > 0)
            {
                lines.Add("Clear when: " + string.Join(" ", followThrough.ClearWhen));
            }

            if (followThrough.WatchNext != null && followThrough.WatchNext.Count > 0)
            {
                lines.Add("Watch next: " + string.Join(" ", followThrough.WatchNext));
            }

            if (followThrough.Signals != null && followThrough.Signals.Count > 0)
            {
                lines.Add(FormatPostureList(followThrough.Signals, string.Empty));
            }

            return string.Join("", lines.Where(line => !string.IsNullOrWhiteSpace(line)));
        }

        private static string FormatOptionalTokenSuffix(string first, string second)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(first)) parts.Add(HumanizeToken(first));
            if (!string.IsNullOrWhiteSpace(second)) parts.Add(second);
            return parts.Count > 0 ? $" ({string.Join(", ", parts)})" : string.Empty;
        }

        private static string FormatCityMudBridgeReceipt(CityMudWorldConsequenceBridgeReceiptSnapshot receipt)
        {
            if (receipt == null)
            {
                return "none.";
            }

            var title = FirstNonBlank(receipt.Title, receipt.Id, "unknown receipt");
            var stateParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(receipt.Outcome)) stateParts.Add(HumanizeToken(receipt.Outcome));
            if (!string.IsNullOrWhiteSpace(receipt.Severity)) stateParts.Add(HumanizeToken(receipt.Severity));
            if (!string.IsNullOrWhiteSpace(receipt.RegionId)) stateParts.Add($"region {receipt.RegionId}");
            if (!string.IsNullOrWhiteSpace(receipt.RuntimeActionId)) stateParts.Add($"action {receipt.RuntimeActionId}");

            var suffix = stateParts.Count > 0 ? $" ({string.Join(", ", stateParts)})" : string.Empty;
            return $"{title}{suffix}.";
        }

        private static string BuildDeskNoun(ShellScreen screen, ShellSummarySnapshot summary)
        {
            if (screen == ShellScreen.Heroes)
            {
                var lane = NormalizeLane(summary?.City?.SettlementLane);
                return string.Equals(lane, "black_market", StringComparison.OrdinalIgnoreCase) ? "Operatives" : "Heroes";
            }

            if (screen == ShellScreen.BlackMarket)
            {
                return "Operations";
            }

            return "Development";
        }

        private void RenderMotherBrainActionPath(ShellSummarySnapshot summary, bool isSummaryLoaded)
        {
            var pressure = summary?.MotherBrainPressureStatus;
            var actionPath = pressure?.ActionPath;
            var shouldShow = isSummaryLoaded && summary != null && summary.HasCity && pressure != null && actionPath != null;
            if (motherBrainActionPathCard != null)
            {
                motherBrainActionPathCard.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (!shouldShow)
            {
                return;
            }

            var recommendedDesk = FirstNonBlank(actionPath.RecommendedDesk, "operations");
            var recommendedScreen = ResolvePostureScreen(recommendedDesk, summary);
            motherBrainActionPathRecommendedScreen = recommendedScreen;

            if (motherBrainActionPathBadge != null)
            {
                var severity = FirstNonBlank(pressure.Severity, "watch");
                motherBrainActionPathBadge.text = $"Mother Brain • {HumanizeToken(severity)}";
            }

            if (motherBrainActionPathHeadline != null)
            {
                motherBrainActionPathHeadline.text = FirstNonBlank(actionPath.Title, pressure.Headline, "Mother Brain pressure action path");
            }

            if (motherBrainActionPathDetail != null)
            {
                motherBrainActionPathDetail.text = FirstNonBlank(actionPath.CurrentStep, pressure.Detail, "Mother Brain has not surfaced a current step yet.");
            }

            if (motherBrainActionPathRecommended != null)
            {
                motherBrainActionPathRecommended.text = FirstNonBlank(actionPath.RecommendedActionLabel, pressure.RecommendedAction, BuildPostureButtonLabel(recommendedScreen, summary));
            }

            if (motherBrainActionPathReason != null)
            {
                motherBrainActionPathReason.text = FirstNonBlank(actionPath.WhyThisMatters, pressure.RecommendedAction, "This path is derived from live Mother Brain pressure status; it does not spawn events or complete objectives.");
            }

            if (motherBrainActionPathBlockers != null)
            {
                var blockers = actionPath.Blockers != null && actionPath.Blockers.Count > 0
                    ? actionPath.Blockers
                    : pressure.IncidentBlockedBy;
                motherBrainActionPathBlockers.text = FormatPostureList(blockers, pressure.IncidentReady ? "No follow-through blockers surfaced." : "No explicit blockers surfaced yet.");
            }

            if (motherBrainActionPathProof != null)
            {
                motherBrainActionPathProof.text = FormatPostureList(actionPath.LiveProofSignals, "No Mother Brain proof signals surfaced yet.");
            }

            if (motherBrainActionPathReceipt != null)
            {
                motherBrainActionPathReceipt.text = FormatMotherBrainReceiptFollowThrough(actionPath);
            }

            if (motherBrainActionPathButton != null)
            {
                motherBrainActionPathButton.text = BuildPostureButtonLabel(recommendedScreen, summary);
                motherBrainActionPathButton.SetEnabled(true);
            }
        }

        private static string FormatMotherBrainReceiptFollowThrough(MotherBrainPressureActionPathSnapshot actionPath)
        {
            var followThrough = actionPath?.ReceiptFollowThrough;
            var lines = new List<string>();

            if (!string.IsNullOrWhiteSpace(followThrough?.State))
            {
                lines.Add($"Follow-through state: {HumanizeToken(followThrough.State)}.");
            }

            if (!string.IsNullOrWhiteSpace(followThrough?.Title))
            {
                lines.Add(followThrough.Title.Trim());
            }

            if (!string.IsNullOrWhiteSpace(followThrough?.LatestReceiptTitle))
            {
                var receiptBits = new List<string> { followThrough.LatestReceiptTitle.Trim() };
                if (!string.IsNullOrWhiteSpace(followThrough.LatestReceiptOutcome))
                {
                    receiptBits.Add(HumanizeToken(followThrough.LatestReceiptOutcome));
                }
                if (!string.IsNullOrWhiteSpace(followThrough.LatestReceiptState))
                {
                    receiptBits.Add(HumanizeToken(followThrough.LatestReceiptState));
                }
                lines.Add($"Latest receipt: {string.Join(" • ", receiptBits)}.");
            }

            if (!string.IsNullOrWhiteSpace(followThrough?.Summary))
            {
                lines.Add(followThrough.Summary.Trim());
            }

            if (!string.IsNullOrWhiteSpace(followThrough?.LatestRuntimeActionId))
            {
                lines.Add($"Runtime action: {followThrough.LatestRuntimeActionId.Trim()}.");
            }

            if (!string.IsNullOrWhiteSpace(followThrough?.SourceRegionId))
            {
                lines.Add($"Source region: {HumanizeToken(followThrough.SourceRegionId)}.");
            }

            if (followThrough?.ResponseHistory != null && followThrough.ResponseHistory.Count > 0)
            {
                lines.Add("Recent response history:");
                lines.AddRange(followThrough.ResponseHistory
                    .Where(entry => entry != null && !string.IsNullOrWhiteSpace(entry.Title))
                    .Take(3)
                    .Select(FormatMotherBrainResponseHistoryEntry));
            }

            var recovery = followThrough?.BlockerRecovery;
            if (recovery != null)
            {
                if (!string.IsNullOrWhiteSpace(recovery.State))
                {
                    lines.Add($"Blocker recovery: {HumanizeToken(recovery.State)}.");
                }

                if (!string.IsNullOrWhiteSpace(recovery.Title))
                {
                    lines.Add(recovery.Title.Trim());
                }

                if (!string.IsNullOrWhiteSpace(recovery.Summary))
                {
                    lines.Add(recovery.Summary.Trim());
                }

                if (recovery.Blockers != null && recovery.Blockers.Count > 0)
                {
                    lines.Add($"Blockers: {string.Join(", ", recovery.Blockers.Where(blocker => !string.IsNullOrWhiteSpace(blocker)).Select(blocker => HumanizeToken(blocker)).Take(3))}.");
                }

                if (recovery.ClearWhen != null && recovery.ClearWhen.Count > 0)
                {
                    lines.AddRange(recovery.ClearWhen
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => $"Clear when: {line.Trim()}")
                        .Take(2));
                }

                if (!string.IsNullOrWhiteSpace(recovery.RecommendedActionLabel))
                {
                    lines.Add($"Recovery action: {recovery.RecommendedActionLabel.Trim()}.");
                }

                if (recovery.Signals != null && recovery.Signals.Count > 0)
                {
                    lines.AddRange(recovery.Signals
                        .Where(signal => !string.IsNullOrWhiteSpace(signal))
                        .Select(signal => $"• {signal.Trim()}")
                        .Take(2));
                }
            }

            if (followThrough?.Signals != null && followThrough.Signals.Count > 0)
            {
                lines.AddRange(followThrough.Signals
                    .Where(signal => !string.IsNullOrWhiteSpace(signal))
                    .Select(signal => $"• {signal.Trim()}")
                    .Take(3));
            }

            if (lines.Count > 0)
            {
                return string.Join("\n", lines);
            }

            return string.IsNullOrWhiteSpace(actionPath?.NextReceiptFamily)
                ? "Next receipt family: not surfaced yet."
                : $"Next receipt family: {HumanizeToken(actionPath.NextReceiptFamily)}.";
        }

        private static string FormatMotherBrainResponseHistoryEntry(MotherBrainPressureResponseHistoryEntrySnapshot entry)
        {
            var bits = new List<string>();

            if (!string.IsNullOrWhiteSpace(entry.Outcome))
            {
                bits.Add(HumanizeToken(entry.Outcome));
            }

            if (!string.IsNullOrWhiteSpace(entry.ReceiptState))
            {
                bits.Add(HumanizeToken(entry.ReceiptState));
            }

            if (!string.IsNullOrWhiteSpace(entry.SourceRegionId))
            {
                bits.Add(HumanizeToken(entry.SourceRegionId));
            }

            if (!string.IsNullOrWhiteSpace(entry.RuntimeActionId))
            {
                bits.Add(entry.RuntimeActionId.Trim());
            }

            var suffix = bits.Count > 0 ? $" ({string.Join(" • ", bits)})" : string.Empty;
            return $"• {entry.Title.Trim()}{suffix}.";
        }

        private static string HumanizeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Trim().Replace("_", " ").Replace("-", " ");
        }

        private static ShellScreen ResolvePostureScreen(string recommendedDesk, ShellSummarySnapshot summary)
        {
            var normalized = (recommendedDesk ?? string.Empty).Trim().Replace("-", "_").Replace(" ", "_").ToLowerInvariant();
            if (normalized == "operations" || normalized == "operation")
            {
                return ShellScreen.BlackMarket;
            }

            if (normalized == "heroes" || normalized == "hero" || normalized == "operatives" || normalized == "operative" || normalized == "roster")
            {
                return ShellScreen.Heroes;
            }

            return ShellScreen.City;
        }

        private static string BuildPostureButtonLabel(ShellScreen screen, ShellSummarySnapshot summary)
        {
            if (screen == ShellScreen.BlackMarket)
            {
                return "Open Operations";
            }

            if (screen == ShellScreen.Heroes)
            {
                var lane = NormalizeLane(summary?.City?.SettlementLane);
                return string.Equals(lane, "black_market", StringComparison.OrdinalIgnoreCase) ? "Open Operatives" : "Open Heroes";
            }

            return "Open Development";
        }

        private static string BuildFallbackPostureAction(EarlyLanePostureSnapshot posture, ShellSummarySnapshot summary)
        {
            var screen = ResolvePostureScreen(posture?.RecommendedDesk, summary);
            return BuildPostureButtonLabel(screen, summary);
        }

        private static string ResolveLaneLabel(string postureLane, string fallbackLabel)
        {
            var lane = NormalizeLane(postureLane);
            if (string.Equals(lane, "black_market", StringComparison.OrdinalIgnoreCase))
            {
                return "Black Market";
            }

            if (!string.IsNullOrWhiteSpace(fallbackLabel) && fallbackLabel != "-")
            {
                return fallbackLabel.Trim();
            }

            return "City";
        }

        private static string FormatPostureList(IEnumerable<string> entries, string emptyText)
        {
            var clean = entries?
                .Where(entry => !string.IsNullOrWhiteSpace(entry))
                .Select(entry => entry.Trim())
                .Take(3)
                .ToList() ?? new List<string>();

            if (clean.Count == 0)
            {
                return emptyText;
            }

            return string.Join("\n", clean.Select(entry => $"• {entry}"));
        }

        private void RenderFounderSetup(ShellSummarySnapshot summary, bool isSummaryLoaded, bool isActionBusy, string actionStatus, bool actionFailed)
        {
            var shouldShow = summary != null
                && !summary.HasCity
                && (summary.CanCreateCity
                    || !string.IsNullOrWhiteSpace(summary.SuggestedCityName)
                    || (summary.CitySetupChoices != null && summary.CitySetupChoices.Count > 0));

            if (founderSetupCard != null)
            {
                founderSetupCard.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (!shouldShow)
            {
                RenderFounderActionStatus(string.Empty, false);
                return;
            }

            if (founderCityNameField != null && !founderNameSeeded && !string.IsNullOrWhiteSpace(summary.SuggestedCityName))
            {
                founderCityNameField.value = summary.SuggestedCityName.Trim();
                founderNameSeeded = true;
            }

            if (founderSetupHeadline != null)
            {
                founderSetupHeadline.text = summary.CanCreateCity
                    ? "Choose how this account enters the world."
                    : "Settlement setup truth is visible, but creation is not open yet.";
            }

            if (founderSetupCopy != null)
            {
                founderSetupCopy.text = summary.CanCreateCity
                    ? "Pick City for public growth or Black Market for a shadow operation. This calls the live bootstrap route; no settlement is invented locally."
                    : "The client can read setup choices, but the backend has not marked this account as eligible to found one yet.";
            }

            RenderFounderActionStatus(actionStatus, actionFailed);

            var cityChoice = FindSetupChoice(summary, "city");
            var marketChoice = FindSetupChoice(summary, "black_market");
            if (founderCityChoiceValue != null) founderCityChoiceValue.text = FormatSetupChoiceValue(cityChoice, "City", "Public growth, buildings, production, and civic development.");
            if (founderCityChoiceNote != null) founderCityChoiceNote.text = FormatSetupChoiceNote(cityChoice, "Uses the public settlement lane when the backend exposes setup truth.");
            if (founderMarketChoiceValue != null) founderMarketChoiceValue.text = FormatSetupChoiceValue(marketChoice, "Black Market", "Shadow operations, contacts, covert pressure, and deniable routing.");
            if (founderMarketChoiceNote != null) founderMarketChoiceNote.text = FormatSetupChoiceNote(marketChoice, "Uses the black-market settlement lane when the backend exposes setup truth.");

            var canFound = isSummaryLoaded && summary.CanCreateCity && !isActionBusy;
            SetFounderButton(founderCityPrimaryButton, FirstNonBlank(cityChoice?.CtaLabel, "Found City"), canFound);
            SetFounderButton(founderMarketPrimaryButton, FirstNonBlank(marketChoice?.CtaLabel, "Found Black Market"), canFound);
            SetFounderButton(founderCityButton, FirstNonBlank(cityChoice?.CtaLabel, "Found City"), canFound);
            SetFounderButton(founderMarketButton, FirstNonBlank(marketChoice?.CtaLabel, "Found Black Market"), canFound);
        }

        private static void SetFounderButton(Button button, string label, bool enabled)
        {
            if (button == null)
            {
                return;
            }

            button.text = string.IsNullOrWhiteSpace(label) ? "Found settlement" : label.Trim();
            button.SetEnabled(enabled);
        }

        private void RenderFounderActionStatus(string actionStatus, bool actionFailed)
        {
            if (founderActionStatus == null)
            {
                return;
            }

            var trimmedStatus = actionStatus?.Trim() ?? string.Empty;
            var shouldShow = !string.IsNullOrWhiteSpace(trimmedStatus);
            founderActionStatus.style.display = shouldShow ? DisplayStyle.Flex : DisplayStyle.None;
            founderActionStatus.text = shouldShow ? trimmedStatus : string.Empty;

            if (actionFailed)
            {
                founderActionStatus.AddToClassList("founder-action-status--error");
            }
            else
            {
                founderActionStatus.RemoveFromClassList("founder-action-status--error");
            }
        }

        private static SettlementSetupChoiceSnapshot FindSetupChoice(ShellSummarySnapshot summary, string lane)
        {
            if (summary?.CitySetupChoices == null || string.IsNullOrWhiteSpace(lane))
            {
                return null;
            }

            return summary.CitySetupChoices.FirstOrDefault(choice => SameSetupLane(choice?.Lane, lane));
        }

        private static bool SameSetupLane(string left, string right)
        {
            return string.Equals(NormalizeSetupLane(left), NormalizeSetupLane(right), StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeSetupLane(string lane)
        {
            return (lane ?? string.Empty).Trim().Replace("-", string.Empty).Replace("_", string.Empty).Replace(" ", string.Empty).ToLowerInvariant();
        }

        private static string ResolveSetupLaneLabel(string lane, string fallback)
        {
            var normalized = NormalizeSetupLane(lane);
            if (normalized == "blackmarket")
            {
                return "Black Market";
            }

            if (normalized == "city")
            {
                return "City";
            }

            return HumanizeWords(lane, fallback);
        }

        private static string FormatSetupChoiceValue(SettlementSetupChoiceSnapshot choice, string fallbackLabel, string fallbackSummary)
        {
            var label = FirstNonBlank(choice?.Label, ResolveSetupLaneLabel(choice?.Lane, fallbackLabel), fallbackLabel);
            var summary = FirstNonBlank(choice?.Summary, choice?.Detail, fallbackSummary);
            return string.IsNullOrWhiteSpace(summary) ? label : $"{label} • {summary}";
        }

        private static string FormatSetupChoiceNote(SettlementSetupChoiceSnapshot choice, string fallback)
        {
            var checklist = choice?.Checklist == null || choice.Checklist.Count == 0
                ? string.Empty
                : string.Join(" • ", choice.Checklist.Where(item => !string.IsNullOrWhiteSpace(item)).Take(3));

            return FirstNonBlank(
                JoinSetupParts("Strength", choice?.Strength),
                JoinSetupParts("Liability", choice?.Liability),
                checklist,
                choice?.Detail,
                fallback);
        }

        private static string JoinSetupParts(string label, string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : $"{label}: {value.Trim()}";
        }

        private void RenderTimerDiagnostics(ShellSummarySnapshot s, bool isSummaryLoaded, DateTime nowUtc)
        {
            var diagnosticsEnabled = TimerDiagnosticsDevFlagEnabled;

            if (timerDiagnosticCard != null)
            {
                timerDiagnosticCard.style.display = diagnosticsEnabled ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (timerDiagnosticsButton != null)
            {
                timerDiagnosticsButton.style.display = diagnosticsEnabled ? DisplayStyle.Flex : DisplayStyle.None;
                timerDiagnosticsButton.SetEnabled(diagnosticsEnabled);
            }

            if (!diagnosticsEnabled)
            {
                return;
            }

            if (timerDiagNow != null) timerDiagNow.text = $"Live UI clock {nowUtc:HH:mm:ss} UTC";
            if (timerDiagHeartbeat != null) timerDiagHeartbeat.text = $"Heartbeat #{heartbeat}";
            if (timerDiagRaw != null) timerDiagRaw.text = FormatTimerRaw(s.ResourceTickTiming, isSummaryLoaded);
            if (timerDiagComputed != null) timerDiagComputed.text = $"diag: {FormatTick(s.ResourceTickTiming)}";
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
            var payoff = summary.BlackMarketPayoffRecovery;
            var card = SelectPrimaryShadowCard(active);
            var stage = HumanizeStage(MapShadowStage(card, runtime, payoff));
            var deskHeadline = FirstNonBlank(primaryOp?.FocusLabel, card?.Headline, runtime?.Headline, payoff?.Headline, primaryOp?.Title, "Shadow pressure desk is quiet enough to stay backgrounded.");
            var deskDetail = FirstNonBlank(primaryOp?.Summary, runtime?.Detail, card?.Summary, payoff?.Detail, primaryOp?.Detail, "No shadow convergence detail surfaced.");

            SetPressureDesk(
                badge: $"Shadow seam • {stage}",
                headline: deskHeadline,
                detail: deskDetail,
                seamTitle: "Grounded contract",
                seamValue: ShadowLaneText.BuildGroundedContractValue(primaryOp, card, runtime, payoff),
                seamNote: ShadowLaneText.BuildGroundedContractNote(primaryOp, card, runtime, payoff),
                contractTitle: "Lifecycle",
                contractValue: ShadowLaneText.BuildLifecycleValue(primaryOp, card, runtime, payoff),
                contractNote: ShadowLaneText.BuildLifecycleNote(primaryOp, card, runtime, payoff),
                answerTitle: "Bounded shadow effects",
                answerValue: ShadowLaneText.BuildEffectsValue(primaryOp, card, runtime, payoff),
                answerNote: ShadowLaneText.BuildEffectsNote(primaryOp, card, runtime, payoff),
                demandTitle: "Pressure / payoff bend",
                demandValue: ShadowLaneText.BuildPressureBendValue(runtime, payoff),
                demandNote: ShadowLaneText.BuildPressureBendNote(primaryOp, runtime, payoff),
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
            card.AddToClassList("pressure-op-card--compact");

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

            var postureValue = new Label(BuildHomeOperationSummary(operation, summary, lane));
            postureValue.AddToClassList("summary-value");
            postureValue.AddToClassList("summary-value--glance");
            postureValue.AddToClassList("pressure-op-card__summary");
            card.Add(postureValue);

            var demandValue = new Label(BuildHomeOperationSignal(operation, summary, lane, index == 0));
            demandValue.AddToClassList("metric-subvalue");
            demandValue.AddToClassList("metric-subvalue--wrap");
            demandValue.AddToClassList("pressure-op-card__signal");
            card.Add(demandValue);

            var cta = new Label(FirstNonBlank(operation?.CtaLabel, DefaultOperationCta(operation)));
            cta.AddToClassList("pressure-op-card__cta");
            card.Add(cta);
            return card;
        }

        private static string BuildHomeOperationSummary(OperationSnapshot operation, ShellSummarySnapshot summary, string lane)
        {
            return Truncate(FirstNonBlank(
                operation?.WhyNow,
                operation?.Summary,
                operation?.Detail,
                BuildOperationPosture(operation, summary, lane),
                "No why-now reason surfaced."), 112);
        }

        private static string BuildHomeOperationSignal(OperationSnapshot operation, ShellSummarySnapshot summary, string lane, bool isLead)
        {
            return Truncate(FirstNonBlank(
                BuildOperationDemandSignal(operation, summary, lane),
                BuildOperationConsequenceValue(operation),
                BuildOperationHandoff(operation, isLead),
                "No demand or consequence signal surfaced."), 104);
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

        private static string Truncate(string value, int maxLength)
        {
            var compact = CompactSingleLine(value);
            if (string.IsNullOrEmpty(compact) || maxLength <= 0 || compact.Length <= maxLength)
            {
                return compact;
            }

            if (maxLength <= 1)
            {
                return "…";
            }

            return compact.Substring(0, maxLength - 1).TrimEnd() + "…";
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
            if (posture.Contains("receipt-chain routing") && ContainsAny(context, "counterfeit", "permit", "throughput", "script", "window", "receipt", "paper", "ledger")) score += 6;
            if (posture.Contains("deniable cleanup") && ContainsAny(context, "cover", "cleanup", "contain", "cooling", "backlash", "repair", "wash")) score += 6;
            if (posture.Contains("heat management") && ContainsAny(context, "pressure", "backbone", "warning", "bribe", "leverage", "heat", "exposure")) score += 6;
            if (posture.Contains("shadow books") && ContainsAny(context, "exploit", "payoff", "cash", "window", "active", "carry", "route")) score += 6;
            if (ContainsAny(context, "receipt", "backlash", "pressure", "payoff")) score += 2;
            return score;
        }

        private static string BuildCivicOperationStripCopy(ShellSummarySnapshot summary, List<OperationSnapshot> operations)
        {
            var surface = summary?.PublicBackbonePressureConvergence;
            var strongest = operations.FirstOrDefault();
            var strongestPosture = BuildOperationPosture(strongest, summary, lane: "city");
            var receipt = surface?.LatestSupportReceipt;
            var bend = BuildCivicDemandValue(surface);
            var receiptLine = receipt != null ? $"Latest receipt: {receipt.Title}." : string.Empty;
            return FirstNonBlank(
                $"Board bend: {bend}. Leading posture: {strongestPosture}. {receiptLine}".Trim(),
                surface?.RecommendedAction,
                surface?.Detail,
                "The fast strip should rank the most demand-shaped civic answers first.");
        }

        private static string BuildShadowOperationStripCopy(ShellSummarySnapshot summary, List<OperationSnapshot> operations)
        {
            var runtime = summary?.BlackMarketRuntimeTruth;
            var payoff = summary?.BlackMarketPayoffRecovery;
            var strongest = operations.FirstOrDefault();
            var strongestPosture = BuildOperationPosture(strongest, summary, lane: "black_market");
            var receipt = payoff?.RecentReceipts?.FirstOrDefault();
            var bend = BuildShadowDemandValue(runtime, payoff);
            var receiptLine = receipt != null ? $"Latest receipt: {receipt.Title}." : string.Empty;
            return FirstNonBlank(
                $"Shadow books: {bend}. Lead posture: {strongestPosture}. {receiptLine}".Trim(),
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
            var payoff = summary?.BlackMarketPayoffRecovery;
            var receipt = payoff?.RecentReceipts?.FirstOrDefault();
            var posture = BuildOperationPosture(operation, summary, lane: "black_market");
            if (receipt != null && PostureMatchesShadowReceipt(posture, receipt))
            {
                return $"Receipt-led: {receipt.Title} • {HumanizeWords(receipt.Severity, "live pressure")}";
            }

            return $"{BuildShadowDemandValue(runtime, payoff)} • {posture}";
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

            if (postureText.Contains("receipt-chain routing")) return ContainsAny(text, "counterfeit", "throughput", "script", "permit", "receipt", "paper", "ledger");
            if (postureText.Contains("deniable cleanup")) return ContainsAny(text, "cover", "cleanup", "contain", "repair", "backlash", "wash", "cool");
            if (postureText.Contains("heat management")) return ContainsAny(text, "pressure", "warning", "bribe", "leverage", "heat", "exposure");
            if (postureText.Contains("shadow books")) return ContainsAny(text, "exploit", "payoff", "cash", "carry", "route");
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
                    return "Shadow books / covert cash-out";
                case "containment":
                case "contain":
                    return "Deniable cleanup / route cooling";
                case "counterfeit":
                    return "Receipt-chain routing / forged paper";
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
            return ShadowLaneText.BuildOperationPostureFromKind(kind);
        }

        private static string BuildShadowPostureFromText(OperationSnapshot operation)
        {
            return ShadowLaneText.BuildOperationPostureFromText(operation);
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
            return ShadowLaneText.BuildOperationPostureFromKind(card?.Kind) != string.Empty
                ? ShadowLaneText.BuildOperationPostureFromKind(card?.Kind)
                : ShadowLaneText.BuildOperationPostureFromText(primaryOp);
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
            return ShadowLaneText.BuildPressureBendValue(runtime, payoff);
        }

        private static string BuildShadowDemandNote(BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff, OperationSnapshot primaryOp)
        {
            return ShadowLaneText.BuildPressureBendNote(primaryOp, runtime, payoff);
        }

        private static string HumanizeShadowKind(string kind)
        {
            return ShadowLaneText.HumanizeShadowKind(kind);
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

        private static string FormatResource(ResourceSnapshot r, ResourcePresentationSnapshot labels, string fallback, string suffix = "")
        {
            var chunks = new[]
            {
                Pair(labels, "food", r.Food, suffix), Pair(labels, "materials", r.Materials, suffix), Pair(labels, "wealth", r.Wealth, suffix), Pair(labels, "mana", r.Mana, suffix), Pair(labels, "knowledge", r.Knowledge, suffix), Pair(labels, "unity", r.Unity, suffix)
            }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            return chunks.Length == 0 ? fallback : string.Join(" • ", chunks);
        }

        private static string Pair(ResourcePresentationSnapshot labels, string key, double? value, string suffix) => value.HasValue ? $"{ResourcePresentationText.Label(labels, key)} {value.Value:0.#}{suffix}" : null;
        private static string FormatProgress(double? p, double? c) => c.GetValueOrDefault() > 0 ? $"{p.GetValueOrDefault():0.#}/{c.Value:0.#}" : $"{p.GetValueOrDefault():0.#}";
        private static string FormatRemaining(TimeSpan span) => span <= TimeSpan.Zero ? "now" : span.ToString(span.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss");

        private static List<ResearchSnapshot> SelectActiveResearches(ShellSummarySnapshot s, DateTime nowUtc)
        {
            var selected = new List<ResearchSnapshot>();
            if (s?.ActiveResearches != null)
            {
                selected.AddRange(s.ActiveResearches.Where(r => r != null));
            }

            if (s?.ActiveResearch != null)
            {
                selected.Add(s.ActiveResearch);
            }

            return selected
                .Where(r => !string.IsNullOrWhiteSpace(FirstNonBlank(r.Id, r.Name)))
                .GroupBy(r => FirstNonBlank(r.Id, r.Name), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(r => IsResearchReady(r, nowUtc) ? 0 : 1)
                .ThenBy(r => r.FinishesAtUtc ?? DateTime.MaxValue)
                .ThenBy(r => FirstNonBlank(r.Name, r.Id))
                .ToList();
        }

        private static string FormatResearchSummary(IReadOnlyList<ResearchSnapshot> researches, DateTime nowUtc)
        {
            if (researches == null || researches.Count == 0) return "No active research.";
            var lead = researches[0];
            var more = researches.Count > 1 ? $" • +{researches.Count - 1} more" : string.Empty;
            return $"{FormatResearchEntry(lead, nowUtc)}{more}";
        }

        private static string FormatResearchTimer(IReadOnlyList<ResearchSnapshot> researches, DateTime nowUtc)
        {
            if (researches == null || researches.Count == 0) return "No active research timer.";

            var ready = researches.Count(r => IsResearchReady(r, nowUtc));
            var lead = researches[0];
            var prefix = researches.Count > 1 ? $"{researches.Count} research item(s)" : lead.Name;
            if (lead.FinishesAtUtc.HasValue)
            {
                var leadTimer = lead.FinishesAtUtc.Value <= nowUtc
                    ? "ready to update for result"
                    : $"completes in {FormatRemaining(lead.FinishesAtUtc.Value - nowUtc)}";
                var readySuffix = ready > 0 ? $" • {ready} ready" : string.Empty;
                return $"{prefix} • {leadTimer}{readySuffix}";
            }

            return lead.StartedAtUtc.HasValue
                ? $"{prefix} • running {FormatRemaining(nowUtc - lead.StartedAtUtc.Value)}"
                : $"{prefix} • active, ETA unavailable";
        }

        private static string FormatResearchEntry(ResearchSnapshot research, DateTime nowUtc)
        {
            var timer = research.FinishesAtUtc.HasValue ? $" • {FormatRemaining(research.FinishesAtUtc.Value - nowUtc)}" : string.Empty;
            var status = string.IsNullOrWhiteSpace(research.Status) ? string.Empty : $" • {HumanizeWords(research.Status, "Active")}";
            return $"{research.Name} • {FormatProgress(research.Progress, research.Cost)}{timer}{status}";
        }

        private static bool IsResearchReady(ResearchSnapshot research, DateTime nowUtc)
        {
            return research?.FinishesAtUtc.HasValue == true && research.FinishesAtUtc.Value <= nowUtc;
        }

        private static string FormatWorkshopAndBuild(ShellSummarySnapshot s, DateTime nowUtc)
        {
            var buildReady = (s.Buildings ?? new List<BuildingSnapshot>()).FirstOrDefault(b => b != null && (IsReadyStatus(b.Status) || (b.FinishesAtUtc.HasValue && b.FinishesAtUtc.Value <= nowUtc)));
            if (buildReady != null)
            {
                return $"{FirstNonBlank(buildReady.Name, buildReady.BuildingId, "Building")} • ready to update";
            }

            var buildActive = (s.Buildings ?? new List<BuildingSnapshot>()).FirstOrDefault(b => b != null && IsActiveStatus(b.Status) && b.FinishesAtUtc.HasValue);
            if (buildActive != null)
            {
                return $"{FirstNonBlank(buildActive.Name, buildActive.BuildingId, "Building")} • {FormatRemaining(buildActive.FinishesAtUtc.Value - nowUtc)}";
            }

            var buildTimer = (s.CityTimers ?? new List<CityTimerEntrySnapshot>()).FirstOrDefault(IsBuildTimer);
            if (buildTimer != null)
            {
                return buildTimer.FinishesAtUtc.HasValue
                    ? $"{FirstNonBlank(buildTimer.Label, HumanizeWords(buildTimer.Category, "Build timer"))} • {FormatRemaining(buildTimer.FinishesAtUtc.Value - nowUtc)}"
                    : $"{FirstNonBlank(buildTimer.Label, HumanizeWords(buildTimer.Category, "Build timer"))} • {FirstNonBlank(buildTimer.Status, "timed")}";
            }

            return FormatWorkshop(s.WorkshopJobs);
        }

        private static bool IsBuildTimer(CityTimerEntrySnapshot timer)
        {
            var category = timer?.Category ?? string.Empty;
            var label = timer?.Label ?? string.Empty;
            var detail = timer?.Detail ?? string.Empty;
            return category.IndexOf("build", StringComparison.OrdinalIgnoreCase) >= 0
                || category.IndexOf("construction", StringComparison.OrdinalIgnoreCase) >= 0
                || category.IndexOf("upgrade", StringComparison.OrdinalIgnoreCase) >= 0
                || label.IndexOf("build", StringComparison.OrdinalIgnoreCase) >= 0
                || label.IndexOf("construction", StringComparison.OrdinalIgnoreCase) >= 0
                || detail.IndexOf("construction", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsActiveStatus(string status)
        {
            var value = (status ?? string.Empty).Trim();
            return value.Length == 0
                || value.Equals("active", StringComparison.OrdinalIgnoreCase)
                || value.Equals("building", StringComparison.OrdinalIgnoreCase)
                || value.Equals("constructing", StringComparison.OrdinalIgnoreCase)
                || value.Equals("upgrading", StringComparison.OrdinalIgnoreCase)
                || value.Equals("in_progress", StringComparison.OrdinalIgnoreCase)
                || value.Equals("running", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsReadyStatus(string status)
        {
            var value = (status ?? string.Empty).Trim();
            return value.Equals("ready", StringComparison.OrdinalIgnoreCase)
                || value.Equals("complete", StringComparison.OrdinalIgnoreCase)
                || value.Equals("completed", StringComparison.OrdinalIgnoreCase)
                || value.Equals("finished", StringComparison.OrdinalIgnoreCase)
                || value.Equals("done", StringComparison.OrdinalIgnoreCase);
        }

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
                return HumanizeWords(recipeId, "Workshop job");
            }

            var outputItemId = job?.OutputItemId?.Trim();
            if (!string.IsNullOrWhiteSpace(outputItemId))
            {
                return HumanizeWords(outputItemId, "Workshop item");
            }

            var attachmentKind = job?.AttachmentKind?.Trim();
            if (!string.IsNullOrWhiteSpace(attachmentKind))
            {
                return HumanizeWords(attachmentKind, "Workshop job");
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
            var anchor = timing.NextTickAtUtc;
            if (!anchor.HasValue && timing.LastTickAtUtc.HasValue && cadence.HasValue)
            {
                anchor = timing.LastTickAtUtc.Value + cadence.Value;
            }

            if (!anchor.HasValue)
            {
                return null;
            }

            if (!cadence.HasValue || cadence.Value <= TimeSpan.Zero)
            {
                return anchor.Value;
            }

            var nowUtc = DateTime.UtcNow;
            if (anchor.Value > nowUtc)
            {
                return anchor.Value;
            }

            var elapsed = nowUtc - anchor.Value;
            var skippedTicks = Math.Floor(elapsed.TotalMilliseconds / cadence.Value.TotalMilliseconds) + 1;
            return anchor.Value.AddMilliseconds(skippedTicks * cadence.Value.TotalMilliseconds);
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
