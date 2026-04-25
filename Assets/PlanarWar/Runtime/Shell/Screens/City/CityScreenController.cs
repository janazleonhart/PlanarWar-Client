using PlanarWar.Client.Core;
using PlanarWar.Client.Core.Contracts;
using PlanarWar.Client.Core.Presentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace PlanarWar.Client.UI.Screens.City
{
    public sealed class CityScreenController
    {
        private enum DevelopmentLane
        {
            Research,
            Workshop,
            Growth,
        }

        private readonly Label headline;
        private readonly Label copy;
        private readonly Label laneTitle;
        private readonly Label laneCopy;
        private readonly Label card1Title;
        private readonly Label card1Value;
        private readonly Label card2Title;
        private readonly Label card2Value;
        private readonly Label card3Title;
        private readonly Label card3Value;
        private readonly Label researchFocusValue;
        private readonly Label nextTechValue;
        private readonly Label workshopValue;
        private readonly Label growthValue;
        private readonly Label supportValue;
        private readonly Label noteValue;
        private readonly Label researchCardsCopyValue;
        private readonly Label workshopCardsCopyValue;
        private readonly Label growthCardsCopyValue;

        private readonly Button researchLaneButton;
        private readonly Button workshopLaneButton;
        private readonly Button growthLaneButton;
        private readonly VisualElement researchSection;
        private readonly VisualElement workshopSection;
        private readonly VisualElement growthSection;

        private readonly InfoCard[] researchCards;
        private readonly InfoCard[] workshopCards;
        private readonly InfoCard[] growthCards;
        private readonly SummaryState summaryState;
        private readonly Func<string, Task> onStartResearchRequested;
        private readonly Func<string, Task> onStartWorkshopCraftRequested;
        private readonly Func<string, Task> onCollectWorkshopRequested;
        private readonly Func<string, Task> onRecruitHeroRequested;
        private readonly Func<string, Task> onAcceptHeroRecruitCandidateRequested;
        private readonly Func<Task> onDismissHeroRecruitCandidatesRequested;
        private readonly Action onRefreshDeskRequested;
        private readonly Action onBackHomeRequested;
        private readonly Button refreshDeskButton;
        private readonly Button startSuggestedResearchButton;
        private readonly Button backHomeButton;

        private DevelopmentLane activeLane = DevelopmentLane.Research;

        public CityScreenController(VisualElement root, SummaryState summaryState, Func<string, Task> onStartResearchRequested, Func<string, Task> onStartWorkshopCraftRequested, Func<string, Task> onCollectWorkshopRequested, Func<string, Task> onRecruitHeroRequested, Func<string, Task> onAcceptHeroRecruitCandidateRequested, Func<Task> onDismissHeroRecruitCandidatesRequested, Action onRefreshDeskRequested, Action onBackHomeRequested)
        {
            headline = root.Q<Label>("development-headline-value");
            copy = root.Q<Label>("development-copy-value");
            laneTitle = root.Q<Label>("dev-lane-title-value");
            laneCopy = root.Q<Label>("dev-lane-copy-value");
            card1Title = root.Q<Label>("dev-lane-card-1-title");
            card1Value = root.Q<Label>("dev-lane-card-1-value");
            card2Title = root.Q<Label>("dev-lane-card-2-title");
            card2Value = root.Q<Label>("dev-lane-card-2-value");
            card3Title = root.Q<Label>("dev-lane-card-3-title");
            card3Value = root.Q<Label>("dev-lane-card-3-value");
            researchFocusValue = root.Q<Label>("dev-research-focus-value");
            nextTechValue = root.Q<Label>("dev-next-tech-value");
            workshopValue = root.Q<Label>("dev-workshop-value");
            growthValue = root.Q<Label>("dev-growth-value");
            supportValue = root.Q<Label>("dev-support-value");
            noteValue = root.Q<Label>("dev-note-value");
            researchCardsCopyValue = root.Q<Label>("dev-research-cards-copy-value");
            workshopCardsCopyValue = root.Q<Label>("dev-workshop-cards-copy-value");
            growthCardsCopyValue = root.Q<Label>("dev-growth-cards-copy-value");

            researchLaneButton = root.Q<Button>("dev-research-lane-button");
            workshopLaneButton = root.Q<Button>("dev-workshop-lane-button");
            growthLaneButton = root.Q<Button>("dev-growth-lane-button");
            researchSection = root.Q<VisualElement>("dev-research-cards-section");
            workshopSection = root.Q<VisualElement>("dev-workshop-cards-section");
            growthSection = root.Q<VisualElement>("dev-growth-cards-section");

            this.summaryState = summaryState;
            this.onStartResearchRequested = onStartResearchRequested;
            this.onStartWorkshopCraftRequested = onStartWorkshopCraftRequested;
            this.onCollectWorkshopRequested = onCollectWorkshopRequested;
            this.onRecruitHeroRequested = onRecruitHeroRequested;
            this.onAcceptHeroRecruitCandidateRequested = onAcceptHeroRecruitCandidateRequested;
            this.onDismissHeroRecruitCandidatesRequested = onDismissHeroRecruitCandidatesRequested;
            this.onRefreshDeskRequested = onRefreshDeskRequested;
            this.onBackHomeRequested = onBackHomeRequested;

            researchCards = Enumerable.Range(1, 4).Select(i => new InfoCard(root, $"dev-research-card-{i}", hasButton: true)).ToArray();
            workshopCards = Enumerable.Range(1, 4).Select(i => new InfoCard(root, $"dev-workshop-card-{i}", hasButton: true)).ToArray();
            growthCards = Enumerable.Range(1, 4).Select(i => new InfoCard(root, $"dev-growth-card-{i}", hasButton: true)).ToArray();
            refreshDeskButton = root.Q<Button>("dev-refresh-button");
            startSuggestedResearchButton = root.Q<Button>("dev-start-research-button");
            backHomeButton = root.Q<Button>("dev-back-home-button");

            researchLaneButton?.RegisterCallback<ClickEvent>(_ => SetLane(DevelopmentLane.Research));
            workshopLaneButton?.RegisterCallback<ClickEvent>(_ => SetLane(DevelopmentLane.Workshop));
            growthLaneButton?.RegisterCallback<ClickEvent>(_ => SetLane(DevelopmentLane.Growth));
            refreshDeskButton?.RegisterCallback<ClickEvent>(_ => onRefreshDeskRequested?.Invoke());
            startSuggestedResearchButton?.RegisterCallback<ClickEvent>(_ => TriggerSuggestedResearch());
            backHomeButton?.RegisterCallback<ClickEvent>(_ => onBackHomeRequested?.Invoke());
            ApplyLaneSelection();
        }

        public void Render(ShellSummarySnapshot s, SummaryState summaryState)
        {
            var recipeCount = summaryState?.WorkshopRecipes?.Count ?? 0;
            var isBlackMarket = IsBlackMarketLane(s);
            var nowUtc = DateTime.UtcNow;
            var activeResearches = SelectActiveResearches(s, nowUtc);
            var researchStartBlocked = IsResearchStartBlocked(s, summaryState, nowUtc);
            if (growthLaneButton != null)
            {
                growthLaneButton.text = isBlackMarket ? "Fronts" : "Buildings";
            }

            if (!s.HasCity)
            {
                RenderNoCity();
                return;
            }

            headline.text = isBlackMarket ? ShadowLaneText.DescribeDevelopmentHeadline(s.City) : $"{s.City.Name} • Development";
            copy.text = isBlackMarket
                ? ShadowLaneText.DescribeDevelopmentCopy(s.City)
                : $"Tier {(s.City.Tier ?? 0)} {HumanizeLane(s.City.SettlementLaneLabel)} desk. Review research, queues, buildings, and timing without leaving the city shell.";

            card1Title.text = isBlackMarket ? ShadowLaneText.BuildResearchLaneTitle() : "Research lane";
            card1Value.text = activeResearches.Count > 0
                ? FormatResearchOverview(activeResearches, isBlackMarket, nowUtc)
                : s.AvailableTechs.Count > 0
                    ? isBlackMarket
                        ? ShadowLaneText.DescribeResearchLaneValue(s.AvailableTechs)
                        : $"{s.AvailableTechs.Count} tech option{(s.AvailableTechs.Count == 1 ? string.Empty : "s")} ready"
                    : isBlackMarket
                        ? "No active shadow-book or front option surfaced."
                        : "No active research or tech options surfaced.";
            card2Title.text = isBlackMarket ? ShadowLaneText.BuildWorkshopLaneTitle() : "Workshop lane";
            card2Value.text = DescribeWorkshopLane(s, recipeCount, isBlackMarket);
            card3Title.text = isBlackMarket ? "Front lane" : "Building lane";
            card3Value.text = DescribeBuildingLane(s, isBlackMarket);

            researchFocusValue.text = activeResearches.Count > 0
                ? FormatResearchOverview(activeResearches, isBlackMarket, nowUtc)
                : summaryState?.HasRecentResearchStartGuard(nowUtc) == true
                    ? $"Accepted {HumanizeKey(summaryState.RecentStartedResearchTechId)}; waiting for canonical timer."
                    : summaryState?.HasRecentResearchCompletionNotice(nowUtc) == true
                        ? $"Completed {HumanizeKey(summaryState.RecentCompletedResearchTechId)}; no active timer remains."
                        : (isBlackMarket ? "No active shadow-book focus." : "No active research focus.");
            nextTechValue.text = isBlackMarket
                ? ShadowLaneText.BuildNextTechValue(s.AvailableTechs)
                : s.AvailableTechs.FirstOrDefault()?.Name ?? "No available tech surfaced.";
            workshopValue.text = DescribeWorkshopLane(s, recipeCount, isBlackMarket);
            growthValue.text = DescribeBuildingLane(s, isBlackMarket);
            supportValue.text = DescribeSupport(s, isBlackMarket);
            noteValue.text = BuildDeskNote(s, summaryState, isBlackMarket);
            if (startSuggestedResearchButton != null)
            {
                var suggestedTech = GetSuggestedTech(s);
                startSuggestedResearchButton.text = summaryState.IsActionBusy && !string.IsNullOrWhiteSpace(summaryState.PendingResearchTechId)
                    ? "Starting..."
                    : researchStartBlocked
                        ? "Research active"
                        : "Start suggested research";
                startSuggestedResearchButton.SetEnabled(suggestedTech != null && (summaryState == null || !summaryState.IsActionBusy) && !researchStartBlocked && onStartResearchRequested != null);
            }

            RenderResearchLane(s);
            RenderWorkshopLane(s);
            RenderGrowthLane(s);
            ApplyLaneSelection();
        }

        private void RenderNoCity()
        {
            headline.text = "Development desk unavailable";
            copy.text = "Found a city first. Development desks stay read-only until a settlement snapshot exists.";
            laneTitle.text = "No development lane loaded";
            laneCopy.text = "Research, workshop, and growth truth unlock once the client has a real settlement snapshot.";
            card1Title.text = "Research lane";
            card1Value.text = "Found a city to unlock tech posture.";
            card2Title.text = "Workshop lane";
            card2Value.text = "Found a city to unlock workshop queues.";
            card3Title.text = "Building lane";
            card3Value.text = "Found a city to unlock building and cadence posture.";
            researchFocusValue.text = "No research lane loaded.";
            nextTechValue.text = "No suggested unlock surfaced.";
            workshopValue.text = "No workshop queue visible.";
            growthValue.text = "Building cadence unavailable.";
            supportValue.text = "Support posture unavailable.";
            noteValue.text = "Development Desk v1 stays honest: no city snapshot means no desk truth.";
            researchCardsCopyValue.text = "No research cards without a city payload.";
            workshopCardsCopyValue.text = "No workshop cards without a city payload.";
            growthCardsCopyValue.text = "No building cards without a city payload.";
            foreach (var card in researchCards.Concat(workshopCards).Concat(growthCards))
            {
                card.RenderHidden();
            }
            startSuggestedResearchButton?.SetEnabled(false);
            ApplyLaneSelection();
        }

        private void RenderResearchLane(ShellSummarySnapshot s)
        {
            var isBlackMarket = IsBlackMarketLane(s);
            var nowUtc = DateTime.UtcNow;
            var activeResearches = SelectActiveResearches(s, nowUtc);
            var researchStartBlocked = IsResearchStartBlocked(s, summaryState, nowUtc);
            laneTitle.text = activeLane == DevelopmentLane.Research ? (isBlackMarket ? ShadowLaneText.BuildResearchLaneTitle() : "Research lane") : laneTitle.text;
            laneCopy.text = activeLane == DevelopmentLane.Research
                ? (isBlackMarket ? ShadowLaneText.BuildResearchLaneCopy() : "Research keeps the current tech, next unlocks, and readiness posture visible in one place so you can judge the next order before mutation wiring lands.")
                : laneCopy.text;

            var cards = new List<CardView>();
            foreach (var research in activeResearches.Take(4))
            {
                cards.Add(new CardView(
                    family: isBlackMarket ? "Live shadow-book" : "Active research",
                    title: research.Name,
                    lore: BuildResearchLore(research, isBlackMarket, nowUtc),
                    note: BuildResearchNote(research, isBlackMarket, nowUtc),
                    buttonText: IsResearchReady(research, nowUtc) ? "Ready" : "Live",
                    buttonEnabled: false));
            }

            if (cards.Count == 0 && summaryState?.HasRecentResearchStartGuard(nowUtc) == true)
            {
                cards.Add(new CardView(
                    isBlackMarket ? "Accepted shadow order" : "Accepted research order",
                    HumanizeKey(summaryState.RecentStartedResearchTechId),
                    "The server accepted the start request, but /api/me has not surfaced canonical active research or an ETA timer yet.",
                    summaryState.HasResearchStartCanonicalWaitWarning(nowUtc)
                        ? "Start buttons stay locked because canonical ETA truth is still missing from the summary payload."
                        : "Start buttons stay locked so the desk cannot stack duplicate research by accident.",
                    "Awaiting canonical timer",
                    false));
            }
            else if (cards.Count == 0 && summaryState?.HasRecentResearchCompletionNotice(nowUtc) == true)
            {
                cards.Add(new CardView(
                    isBlackMarket ? "Shadow book completed" : "Research completed",
                    HumanizeKey(summaryState.RecentCompletedResearchTechId),
                    "The accepted research is no longer active because the refreshed summary shows it has already resolved or left the available research list.",
                    "No fake countdown is shown after completion; the desk returns to the next real available unlock.",
                    "Completed",
                    false));
            }

            cards.AddRange(s.AvailableTechs.Take(Math.Max(0, 4 - cards.Count)).Select(tech => new CardView(
                family: isBlackMarket ? ShadowLaneText.BuildTechFamily(tech) : (string.IsNullOrWhiteSpace(tech.IdentityFamily) ? HumanizeCategory(tech.Category) : tech.IdentityFamily),
                title: tech.Name,
                lore: isBlackMarket ? ShadowLaneText.BuildTechLore(tech) : FirstNonBlank(tech.IdentitySummary, tech.Description, tech.UnlockPreview.FirstOrDefault(), "No research summary provided."),
                note: BuildTechNote(tech, isBlackMarket),
                buttonText: BuildResearchStartButtonText(tech, researchStartBlocked),
                buttonEnabled: (summaryState == null || !summaryState.IsActionBusy) && !researchStartBlocked && onStartResearchRequested != null,
                onClick: () => TriggerStartResearch(tech.Id))));

            if (cards.Count == 0)
            {
                cards.Add(new CardView(
                    isBlackMarket ? "Shadow books" : "Research payload",
                    isBlackMarket ? "No shadow book entry" : "No tech entry",
                    isBlackMarket ? "No active book, permit front, or quiet leverage entry is visible in the current payload." : "The current /api/me payload did not surface availableTechs or an activeResearch entry.",
                    isBlackMarket ? "The desk stays honest instead of inventing a fake underworld ledger." : "No mutation wiring is attempted here.",
                    "Read-only",
                    false));
            }

            researchCardsCopyValue.text = BuildResearchCardsCopy(s, isBlackMarket, activeResearches, nowUtc);

            RenderCards(researchCards, cards);
        }

        private void RenderWorkshopLane(ShellSummarySnapshot s)
        {
            var isBlackMarket = IsBlackMarketLane(s);
            laneTitle.text = activeLane == DevelopmentLane.Workshop ? (isBlackMarket ? ShadowLaneText.BuildWorkshopLaneTitle() : "Workshop lane") : laneTitle.text;
            laneCopy.text = activeLane == DevelopmentLane.Workshop
                ? (isBlackMarket ? ShadowLaneText.BuildWorkshopLaneCopy() : "Workshop keeps active jobs, ready pickups, real recipe cards, and queue posture visible in one place.")
                : laneCopy.text;

            var cards = new List<CardView>();
            var nowUtc = DateTime.UtcNow;
            var activeJobs = s.WorkshopJobs.Where(j => !IsWorkshopJobCollected(j) && !IsWorkshopJobCollectable(j, nowUtc)).ToList();
            var readyJobs = s.WorkshopJobs.Where(j => !IsWorkshopJobCollected(j) && IsWorkshopJobCollectable(j, nowUtc)).ToList();
            var readyJobIds = new HashSet<string>(readyJobs.Select(j => j.Id), StringComparer.OrdinalIgnoreCase);
            var workshopTimers = s.CityTimers
                .Where(t => string.Equals(t.Category, "workshop_job", StringComparison.OrdinalIgnoreCase))
                .Where(t => !readyJobIds.Contains(t.Id))
                .ToList();

            cards.AddRange(activeJobs.Take(2).Select(job => new CardView(
                family: isBlackMarket ? ShadowLaneText.BuildWorkshopJobFamily(ready: false) : "Active job",
                title: GetWorkshopJobTitle(job),
                lore: isBlackMarket ? ShadowLaneText.BuildWorkshopJobLore(job, ready: false, nowUtc) : job.FinishesAtUtc.HasValue ? $"Ready in {FormatRemaining(job.FinishesAtUtc.Value - nowUtc)}" : "Job surfaced without a finish anchor.",
                note: isBlackMarket ? ShadowLaneText.BuildWorkshopJobNote(job, ready: false) : "Queued workshop job from summary payload.",
                buttonText: "In flight",
                buttonEnabled: false)));

            cards.AddRange(readyJobs.Take(2).Select(job => new CardView(
                family: isBlackMarket ? ShadowLaneText.BuildWorkshopJobFamily(ready: true) : "Ready pickup",
                title: GetWorkshopJobTitle(job),
                lore: isBlackMarket ? ShadowLaneText.BuildWorkshopJobLore(job, ready: true, nowUtc) : "Crafting time elapsed. Collect to deliver the item into city armory storage.",
                note: isBlackMarket
                    ? ShadowLaneText.BuildWorkshopJobNote(job, ready: true)
                    : job.Id == "job" ? "Ready workshop item surfaced without a stable job id." : $"Ready job {job.Id} can now be collected into storage.",
                buttonText: summaryState.IsActionBusy && string.Equals(summaryState.PendingWorkshopJobId, job.Id, StringComparison.OrdinalIgnoreCase) ? "Collecting..." : "Collect",
                buttonEnabled: !summaryState.IsActionBusy && !string.IsNullOrWhiteSpace(job.Id) && job.Id != "job" && onCollectWorkshopRequested != null,
                onClick: () => TriggerCollectWorkshop(job.Id))));

            cards.AddRange(workshopTimers.Take(4 - cards.Count).Select(timer => new CardView(
                family: isBlackMarket ? ShadowLaneText.BuildWorkshopTimerFamily(timer) : "Workshop timer",
                title: timer.Label,
                lore: isBlackMarket ? ShadowLaneText.BuildWorkshopTimerLore(timer, nowUtc) : timer.FinishesAtUtc.HasValue ? $"{timer.Status} • {FormatRemaining(timer.FinishesAtUtc.Value - DateTime.UtcNow)}" : timer.Status,
                note: isBlackMarket ? ShadowLaneText.BuildWorkshopTimerNote(timer) : FirstNonBlank(timer.Detail, "Timer surfaced from cityTimers."))));

            if (cards.Count < 4)
            {
                var recipeCards = summaryState.WorkshopRecipes
                    .Where(r => !string.IsNullOrWhiteSpace(r.RecipeId))
                    .Take(4 - cards.Count)
                    .Select(recipe => new CardView(
                        family: isBlackMarket ? ShadowLaneText.BuildWorkshopRecipeFamily(recipe) : (string.IsNullOrWhiteSpace(recipe.GearFamily) ? "Workshop recipe" : HumanizeKey(recipe.GearFamily)),
                        title: recipe.Name,
                        lore: isBlackMarket ? ShadowLaneText.BuildWorkshopRecipeLore(recipe) : FirstNonBlank(recipe.Summary, recipe.ResponseTags.FirstOrDefault(), "Craftable recipe from workshop catalog."),
                        note: BuildWorkshopRecipeNote(recipe, isBlackMarket, s.ResourceLabels),
                        buttonText: summaryState.IsActionBusy && string.Equals(summaryState.PendingWorkshopRecipeId, recipe.RecipeId, StringComparison.OrdinalIgnoreCase) ? "Crafting..." : "Craft",
                        buttonEnabled: !summaryState.IsActionBusy && onStartWorkshopCraftRequested != null,
                        onClick: () => TriggerStartWorkshopCraft(recipe.RecipeId)));
                cards.AddRange(recipeCards);
            }

            if (cards.Count == 0)
            {
                cards.Add(new CardView(
                    isBlackMarket ? "Covert supply" : "Workshop payload",
                    isBlackMarket ? "No covert supply front" : "No workshop entry",
                    isBlackMarket ? "No live front, ready drop, timer, or quiet recipe is visible in the current payload." : "No workshop job, timer, or recipe catalog entry is currently visible.",
                    isBlackMarket ? "The desk stays honest instead of inventing a fake backroom queue." : "The desk stays honest instead of inventing a fake queue.",
                    "Read-only",
                    false));
            }

            workshopCardsCopyValue.text = isBlackMarket
                ? ShadowLaneText.DescribeWorkshopCardsCopy(activeJobs.Count, readyJobs.Count, summaryState.WorkshopRecipes.Count, workshopTimers.Count, summaryState.WorkshopRecipes)
                : activeJobs.Count > 0
                    ? $"{activeJobs.Count} active workshop job(s) and {readyJobs.Count} ready pickup(s) are visible."
                    : readyJobs.Count > 0
                        ? $"{readyJobs.Count} ready pickup(s) visible; collect wiring is now live."
                        : summaryState.WorkshopRecipes.Count > 0
                            ? $"Showing {summaryState.WorkshopRecipes.Count} craft recipe(s) ready."
                            : workshopTimers.Count > 0
                                ? $"Workshop timing is coming through cityTimers even though no job body is active."
                                : "No workshop queue, timer, or recipe catalog is visible right now.";

            RenderCards(workshopCards, cards);
        }

        private void RenderGrowthLane(ShellSummarySnapshot s)
        {
            var isBlackMarket = IsBlackMarketLane(s);
            laneTitle.text = activeLane == DevelopmentLane.Growth ? (isBlackMarket ? "Front lane" : "Building lane") : laneTitle.text;
            laneCopy.text = activeLane == DevelopmentLane.Growth
                ? (isBlackMarket ? "Front lane keeps operator-front cards, build clocks, cadence, and support posture visible without inventing fake backroom progress." : "Building lane keeps real building cards, construction clocks, cadence, and support posture visible without inventing a fake queue.")
                : laneCopy.text;

            var nowUtc = DateTime.UtcNow;
            var cards = new List<CardView>();
            cards.Add(new CardView(
                family: isBlackMarket ? ShadowLaneText.BuildProductionFamily() : "Production",
                title: isBlackMarket ? ShadowLaneText.BuildProductionTitle() : "Per-tick output",
                lore: FormatProduction(s.ProductionPerTick, s.ResourceLabels),
                note: isBlackMarket ? ShadowLaneText.BuildProductionNote(s.ResourceTickTiming, nowUtc) : s.ResourceTickTiming.NextTickAtUtc.HasValue ? $"Next resource tick in {FormatRemaining(s.ResourceTickTiming.NextTickAtUtc.Value - nowUtc)}" : "Resource cadence is visible without a live anchor."));

            var buildingCards = BuildBuildingCards(s, isBlackMarket, nowUtc, 2);
            cards.AddRange(buildingCards);

            var heroRecruitment = s.HeroRecruitment;
            var recruitTimer = s.CityTimers.FirstOrDefault(t => string.Equals(t.Category, "operator_recruit", StringComparison.OrdinalIgnoreCase));
            var recruitOp = s.OpeningOperations.FirstOrDefault(o => string.Equals(o.Kind, "recruit_hero", StringComparison.OrdinalIgnoreCase) && string.Equals(o.Readiness, "ready_now", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(o.Role));
            if (heroRecruitment != null && string.Equals(heroRecruitment.Status, "candidates_ready", StringComparison.OrdinalIgnoreCase) && heroRecruitment.Candidates.Count > 0)
            {
                cards.Clear();
                foreach (var candidate in heroRecruitment.Candidates.Take(3))
                {
                    cards.Add(new CardView(
                        family: $"{FirstNonBlank(candidate.ClassName, HumanizeKey(candidate.Role))} candidate",
                        title: FirstNonBlank(candidate.DisplayName, candidate.ClassName, HumanizeKey(candidate.Role), "Candidate"),
                        lore: BuildHeroRecruitCandidateLore(candidate),
                        note: BuildHeroRecruitCandidateNote(candidate),
                        buttonText: summaryState.IsActionBusy && string.Equals(summaryState.PendingHeroRecruitCandidateId, candidate.CandidateId, StringComparison.OrdinalIgnoreCase) ? "Signing..." : "Recruit",
                        buttonEnabled: !summaryState.IsActionBusy && onAcceptHeroRecruitCandidateRequested != null && !string.IsNullOrWhiteSpace(candidate.CandidateId),
                        onClick: () => TriggerAcceptHeroRecruitCandidate(candidate.CandidateId)));
                }

                if (cards.Count < 4)
                {
                    cards.Add(new CardView(
                        family: "Candidate review",
                        title: "Pass on current candidates",
                        lore: heroRecruitment.CandidateExpiresAtUtc.HasValue ? $"Review closes in {FormatRemaining(heroRecruitment.CandidateExpiresAtUtc.Value - nowUtc)}" : "Clear this candidate slate and scout again later.",
                        note: "Dismiss the current pool without signing anyone. The next recruit attempt will open a fresh scouting window.",
                        buttonText: summaryState.IsActionBusy && summaryState.PendingHeroRecruitDismiss ? "Dismissing..." : "Dismiss pool",
                        buttonEnabled: !summaryState.IsActionBusy && onDismissHeroRecruitCandidatesRequested != null,
                        onClick: TriggerDismissHeroRecruitCandidates));
                }
            }
            else if (heroRecruitment != null && string.Equals(heroRecruitment.Status, "scouting", StringComparison.OrdinalIgnoreCase))
            {
                var scoutingExpiredLocally = heroRecruitment.FinishesAtUtc.HasValue && heroRecruitment.FinishesAtUtc.Value <= nowUtc;
                cards.Add(new CardView(
                    family: "Hero recruit",
                    title: $"Scouting {HumanizeKey(heroRecruitment.Role)} candidates",
                    lore: heroRecruitment.FinishesAtUtc.HasValue
                        ? scoutingExpiredLocally
                            ? "ready • refresh now"
                            : $"active • {FormatRemaining(heroRecruitment.FinishesAtUtc.Value - nowUtc)}"
                        : "active",
                    note: scoutingExpiredLocally
                        ? $"{BuildHeroRecruitmentScoutingNote(heroRecruitment)} • Timer elapsed locally. Refresh to load candidate cards."
                        : BuildHeroRecruitmentScoutingNote(heroRecruitment),
                    buttonText: scoutingExpiredLocally ? "Refresh candidates" : "Scouting",
                    buttonEnabled: scoutingExpiredLocally && !summaryState.IsActionBusy && onRefreshDeskRequested != null,
                    onClick: scoutingExpiredLocally ? TriggerRefreshDesk : null));
            }
            else if (recruitTimer != null)
            {
                cards.Add(new CardView(
                    family: "Hero recruit",
                    title: recruitTimer.Label,
                    lore: recruitTimer.FinishesAtUtc.HasValue ? $"active • {FormatRemaining(recruitTimer.FinishesAtUtc.Value - nowUtc)}" : "active",
                    note: FirstNonBlank(recruitTimer.Detail, isBlackMarket ? "Recruitment timer is visible in the carry lane." : "Hero recruitment timer is visible from the latest summary."),
                    buttonText: summaryState.IsActionBusy && !string.IsNullOrWhiteSpace(summaryState.PendingHeroRecruitRole) ? "Recruiting..." : "Scouting",
                    buttonEnabled: false));
            }
            else if (heroRecruitment != null && string.Equals(heroRecruitment.Status, "idle", StringComparison.OrdinalIgnoreCase))
            {
                var startRole = FirstNonBlank(heroRecruitment.StartRole, heroRecruitment.Role);
                var pendingStart = summaryState.IsActionBusy
                    && (string.IsNullOrWhiteSpace(startRole)
                        || string.Equals(summaryState.PendingHeroRecruitRole, startRole, StringComparison.OrdinalIgnoreCase));
                cards.Add(new CardView(
                    family: "Hero recruit",
                    title: FirstNonBlank(heroRecruitment.CtaLabel, !string.IsNullOrWhiteSpace(startRole) ? $"Open {HumanizeKey(startRole)} recruitment" : "Open recruitment"),
                    lore: BuildHeroRecruitmentIdleLore(heroRecruitment),
                    note: BuildHeroRecruitmentIdleNote(heroRecruitment),
                    buttonText: pendingStart ? "Opening..." : FirstNonBlank(heroRecruitment.CtaLabel, "Open recruitment"),
                    buttonEnabled: !summaryState.IsActionBusy && onRecruitHeroRequested != null && heroRecruitment.StartEligible,
                    onClick: () => TriggerRecruitHero(startRole)));
            }
            else
            {
                if (recruitOp != null)
                {
                    cards.Add(new CardView(
                        family: "Hero recruit",
                        title: $"Scout {HumanizeKey(recruitOp.Role)} candidates",
                        lore: "Start a real scouting cooldown that resolves into recruitable hero candidates.",
                        note: FirstNonBlank(recruitOp.Title, "A live recruit opening is visible from settlementOpeningOperations."),
                        buttonText: summaryState.IsActionBusy && string.Equals(summaryState.PendingHeroRecruitRole, recruitOp.Role, StringComparison.OrdinalIgnoreCase) ? "Recruiting..." : "Open recruitment",
                        buttonEnabled: !summaryState.IsActionBusy && onRecruitHeroRequested != null,
                        onClick: () => TriggerRecruitHero(recruitOp.Role)));
                }
            }

            cards.AddRange(s.CityTimers
                .Where(t => !string.Equals(t.Category, "resource_tick", StringComparison.OrdinalIgnoreCase) && !string.Equals(t.Category, "workshop_job", StringComparison.OrdinalIgnoreCase) && !string.Equals(t.Category, "operator_recruit", StringComparison.OrdinalIgnoreCase) && !IsBuildTimer(t))
                .Take(Math.Max(0, 3 - cards.Count))
                .Select(timer => new CardView(
                    family: isBlackMarket ? HumanizeBlackMarketTimerCategory(timer.Category) : HumanizeCategory(timer.Category),
                    title: isBlackMarket ? NormalizeBlackMarketTimerLabel(timer.Label, timer.Category) : timer.Label,
                    lore: timer.FinishesAtUtc.HasValue ? $"{timer.Status} • {FormatRemaining(timer.FinishesAtUtc.Value - nowUtc)}" : timer.Status,
                    note: FirstNonBlank(timer.Detail, isBlackMarket ? "Live operations timer is visible in the carry lane." : "Live city timer is visible from the latest summary."))));

            if (cards.Count < 4)
            {
                cards.Add(new CardView(
                    family: isBlackMarket ? "Heat posture" : "Support posture",
                    title: DescribeSupportTitle(s, isBlackMarket),
                    lore: DescribeSupport(s, isBlackMarket),
                    note: isBlackMarket ? ShadowLaneText.BuildSupportCardNote(s) : s.OpeningOperations.Count > 0 ? $"Opening operations visible: {string.Join(" • ", s.OpeningOperations.Take(2).Select(o => o.Title))}" : "No extra opening operation pressure is surfaced right now."));
            }

            var buildingCardsCopy = BuildBuildingCardsCopy(s, isBlackMarket, nowUtc);
            growthCardsCopyValue.text = buildingCards.Count > 0
                ? buildingCardsCopy
                : isBlackMarket
                ? ShadowLaneText.DescribeGrowthCardsCopy(heroRecruitment, recruitTimer, recruitOp, s.CityTimers)
                : heroRecruitment != null && string.Equals(heroRecruitment.Status, "candidates_ready", StringComparison.OrdinalIgnoreCase)
                    ? $"Candidate review is live with {heroRecruitment.Candidates.Count} hero option(s) ready for selection."
                    : heroRecruitment != null && string.Equals(heroRecruitment.Status, "scouting", StringComparison.OrdinalIgnoreCase)
                        ? heroRecruitment.FinishesAtUtc.HasValue && heroRecruitment.FinishesAtUtc.Value <= nowUtc
                            ? "Scouting timer elapsed. Refresh now to load candidate cards from the latest summary payload."
                            : "Hero recruitment is scouting now and will surface candidate cards when the timer resolves."
                        : heroRecruitment != null && string.Equals(heroRecruitment.Status, "idle", StringComparison.OrdinalIgnoreCase)
                            ? heroRecruitment.StartEligible
                                ? "Hero recruitment can be opened directly from the Growth desk."
                                : FirstNonBlank(heroRecruitment.BlockedReason, "Hero recruitment is idle but blocked until resource shortfalls clear.")
                            : recruitTimer != null
                                ? $"Showing cadence, hero recruit timing, and {s.CityTimers.Count} live timer(s)."
                                : s.OpeningOperations.Any(o => string.Equals(o.Kind, "recruit_hero", StringComparison.OrdinalIgnoreCase) && string.Equals(o.Readiness, "ready_now", StringComparison.OrdinalIgnoreCase))
                                    ? "Hero recruit opening is visible from settlementOpeningOperations."
                                    : $"Showing cadence, {s.CityTimers.Count} live timer(s), and current support posture from the summary payload.";
            RenderCards(growthCards, cards);
        }


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

        private static bool IsResearchStartBlocked(ShellSummarySnapshot s, SummaryState state, DateTime nowUtc)
        {
            return SelectActiveResearches(s, nowUtc).Count > 0
                || (state?.HasRecentResearchStartGuard(nowUtc) == true);
        }

        private static bool IsResearchReady(ResearchSnapshot research, DateTime nowUtc)
        {
            return research?.FinishesAtUtc.HasValue == true && research.FinishesAtUtc.Value <= nowUtc;
        }

        private string BuildResearchStartButtonText(TechOptionSnapshot tech, bool researchStartBlocked)
        {
            if (summaryState != null && summaryState.IsActionBusy && string.Equals(summaryState.PendingResearchTechId, tech?.Id, StringComparison.OrdinalIgnoreCase))
            {
                return "Starting...";
            }

            if (researchStartBlocked)
            {
                return summaryState?.HasRecentResearchStartGuard(DateTime.UtcNow) == true ? "Awaiting canonical timer" : "Research active";
            }

            return "Start research";
        }

        private static string BuildResearchCardsCopy(ShellSummarySnapshot s, bool isBlackMarket, IReadOnlyList<ResearchSnapshot> activeResearches, DateTime nowUtc)
        {
            var activeCount = activeResearches?.Count ?? 0;
            var readyCount = activeResearches?.Count(r => IsResearchReady(r, nowUtc)) ?? 0;
            if (isBlackMarket)
            {
                if (activeCount > 0)
                {
                    return $"Showing {activeCount} active shadow-book/front research item(s), {readyCount} ready, and {s.AvailableTechs.Count} available option(s).";
                }

                return ShadowLaneText.DescribeResearchCardsCopy(s);
            }

            if (activeCount > 0)
            {
                return $"Showing {activeCount} active research item(s), {readyCount} ready, and {s.AvailableTechs.Count} available tech option(s).";
            }

            return s.AvailableTechs.Count > 0
                ? $"Showing {s.AvailableTechs.Count} available tech option(s) ready."
                : "No research cards were surfaced in payload.";
        }

        private static string FormatResearchOverview(IReadOnlyList<ResearchSnapshot> activeResearches, bool isBlackMarket, DateTime nowUtc)
        {
            if (activeResearches == null || activeResearches.Count == 0)
            {
                return isBlackMarket ? "No active shadow-book focus." : "No active research focus.";
            }

            var lead = activeResearches[0];
            var timer = lead.FinishesAtUtc.HasValue ? $" • {FormatRemaining(lead.FinishesAtUtc.Value - nowUtc)}" : string.Empty;
            var more = activeResearches.Count > 1 ? $" • +{activeResearches.Count - 1} more" : string.Empty;
            return $"{lead.Name}{timer}{more}";
        }


        private void TriggerRefreshDesk()
        {
            if (summaryState.IsActionBusy || onRefreshDeskRequested == null)
            {
                return;
            }

            onRefreshDeskRequested.Invoke();
        }

        private void TriggerSuggestedResearch()
        {
            var suggested = GetSuggestedTech(summaryState.Snapshot);
            if (suggested == null || summaryState.IsActionBusy || IsResearchStartBlocked(summaryState.Snapshot, summaryState, DateTime.UtcNow) || onStartResearchRequested == null)
            {
                return;
            }

            _ = onStartResearchRequested.Invoke(suggested.Id);
        }

        private void TriggerStartResearch(string techId)
        {
            if (summaryState.IsActionBusy || IsResearchStartBlocked(summaryState.Snapshot, summaryState, DateTime.UtcNow) || onStartResearchRequested == null || string.IsNullOrWhiteSpace(techId))
            {
                return;
            }

            _ = onStartResearchRequested.Invoke(techId.Trim());
        }

        private void TriggerStartWorkshopCraft(string recipeId)
        {
            if (summaryState.IsActionBusy || onStartWorkshopCraftRequested == null || string.IsNullOrWhiteSpace(recipeId))
            {
                return;
            }

            _ = onStartWorkshopCraftRequested.Invoke(recipeId.Trim());
        }

        private void TriggerCollectWorkshop(string jobId)
        {
            if (summaryState.IsActionBusy || onCollectWorkshopRequested == null || string.IsNullOrWhiteSpace(jobId))
            {
                return;
            }

            _ = onCollectWorkshopRequested.Invoke(jobId.Trim());
        }

        private void TriggerRecruitHero(string role)
        {
            if (summaryState.IsActionBusy || onRecruitHeroRequested == null)
            {
                return;
            }

            _ = onRecruitHeroRequested.Invoke(role?.Trim() ?? string.Empty);
        }

        private void TriggerAcceptHeroRecruitCandidate(string candidateId)
        {
            if (summaryState.IsActionBusy || onAcceptHeroRecruitCandidateRequested == null || string.IsNullOrWhiteSpace(candidateId))
            {
                return;
            }

            _ = onAcceptHeroRecruitCandidateRequested.Invoke(candidateId.Trim());
        }

        private void TriggerDismissHeroRecruitCandidates()
        {
            if (summaryState.IsActionBusy || onDismissHeroRecruitCandidatesRequested == null)
            {
                return;
            }

            _ = onDismissHeroRecruitCandidatesRequested.Invoke();
        }

        private static TechOptionSnapshot GetSuggestedTech(ShellSummarySnapshot s)
        {
            return s?.AvailableTechs?.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t?.Id));
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
                return HumanizeKey(recipeId);
            }

            var attachmentKind = job?.AttachmentKind?.Trim();
            if (!string.IsNullOrWhiteSpace(attachmentKind))
            {
                return HumanizeKey(attachmentKind);
            }

            return "Workshop job";
        }

        private static string BuildHeroRecruitCandidateLore(HeroRecruitCandidateSnapshot candidate)
        {
            var bits = new List<string>();
            if (!string.IsNullOrWhiteSpace(candidate.ClassName)) bits.Add(candidate.ClassName);
            if (!string.IsNullOrWhiteSpace(candidate.Role)) bits.Add(HumanizeKey(candidate.Role));
            if (!string.IsNullOrWhiteSpace(candidate.Lane)) bits.Add(HumanizeKey(candidate.Lane));
            return bits.Count > 0 ? string.Join(" • ", bits) : "Recruit candidate ready for review.";
        }

        private static string BuildHeroRecruitCandidateNote(HeroRecruitCandidateSnapshot candidate)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(candidate.Summary))
            {
                parts.Add(candidate.Summary);
            }
            else if (candidate.TraitDetails.Count > 0 || candidate.Traits.Count > 0)
            {
                parts.Add(BuildHeroRecruitTraitDigest(candidate));
            }

            if (candidate.WealthCost.HasValue || candidate.UnityCost.HasValue)
            {
                var labels = ResourcePresentationText.DefaultForLane(candidate.Lane);
                var costParts = new List<string>();
                if (candidate.WealthCost.HasValue) costParts.Add(ResourcePresentationText.Cost(labels, "wealth", candidate.WealthCost));
                if (candidate.UnityCost.HasValue) costParts.Add(ResourcePresentationText.Cost(labels, "unity", candidate.UnityCost));
                parts.Add(string.Join(" • ", costParts.Where(part => !string.IsNullOrWhiteSpace(part))));
            }

            return parts.Count > 0 ? string.Join(" • ", parts) : "Recruit candidate is ready for selection.";
        }

        private static string BuildHeroRecruitTraitDigest(HeroRecruitCandidateSnapshot candidate)
        {
            var traitLabels = candidate.TraitDetails
                .Take(2)
                .Select(BuildHeroRecruitTraitLabel)
                .Where(label => !string.IsNullOrWhiteSpace(label))
                .ToList();

            if (traitLabels.Count == 0)
            {
                traitLabels = candidate.Traits
                    .Take(2)
                    .Select(HumanizeKey)
                    .Where(label => !string.IsNullOrWhiteSpace(label))
                    .ToList();
            }

            return traitLabels.Count > 0 ? $"Traits: {string.Join(", ", traitLabels)}" : string.Empty;
        }

        private static string BuildHeroRecruitTraitLabel(HeroRecruitTraitSnapshot trait)
        {
            if (trait == null)
            {
                return string.Empty;
            }

            var name = FirstNonBlank(trait.Name, trait.Id);
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            var prefix = string.Equals(trait.Polarity, "pro", StringComparison.OrdinalIgnoreCase)
                ? "+"
                : string.Equals(trait.Polarity, "con", StringComparison.OrdinalIgnoreCase)
                    ? "-"
                    : string.Empty;
            return $"{prefix}{HumanizeKey(name)}";
        }

        private static string BuildHeroRecruitmentScoutingNote(HeroRecruitmentSnapshot recruitment)
        {
            var labels = ResourcePresentationText.DefaultForLane(recruitment?.Lane);
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(recruitment.Role)) parts.Add($"Role {HumanizeKey(recruitment.Role)}");
            if (recruitment.WealthCost.HasValue) parts.Add(ResourcePresentationText.Cost(labels, "wealth", recruitment.WealthCost));
            if (recruitment.UnityCost.HasValue) parts.Add(ResourcePresentationText.Cost(labels, "unity", recruitment.UnityCost));
            return parts.Where(part => !string.IsNullOrWhiteSpace(part)).Any() ? string.Join(" • ", parts.Where(part => !string.IsNullOrWhiteSpace(part))) : "Recruitment scouting is in progress.";
        }

        private static string BuildHeroRecruitmentIdleLore(HeroRecruitmentSnapshot recruitment)
        {
            var labels = ResourcePresentationText.DefaultForLane(recruitment?.Lane);
            var parts = new List<string>();
            var startRole = FirstNonBlank(recruitment?.StartRole, recruitment?.Role);
            if (!string.IsNullOrWhiteSpace(startRole)) parts.Add($"Role {HumanizeKey(startRole)}");
            if (!string.IsNullOrWhiteSpace(recruitment?.Lane)) parts.Add(HumanizeKey(recruitment.Lane));
            if (recruitment?.WealthCost.HasValue == true) parts.Add(ResourcePresentationText.Cost(labels, "wealth", recruitment.WealthCost));
            if (recruitment?.UnityCost.HasValue == true) parts.Add(ResourcePresentationText.Cost(labels, "unity", recruitment.UnityCost));
            return parts.Where(part => !string.IsNullOrWhiteSpace(part)).Any() ? string.Join(" • ", parts.Where(part => !string.IsNullOrWhiteSpace(part))) : "Hero recruitment can open from the Growth desk.";
        }

        private static string BuildHeroRecruitmentIdleNote(HeroRecruitmentSnapshot recruitment)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(recruitment?.BlockedReason)) parts.Add(recruitment.BlockedReason);
            else parts.Add("Starts a scouting cooldown that resolves into MUD-class hero candidates.");
            if (!string.IsNullOrWhiteSpace(recruitment?.Shortfall)) parts.Add($"Shortfall: {recruitment.Shortfall}");
            return string.Join(" • ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string BuildDeskNote(ShellSummarySnapshot s, SummaryState summaryState, bool isBlackMarket)
        {
            if (isBlackMarket)
            {
                return ShadowLaneText.BuildDeskNote(s, summaryState?.ActionStatus);
            }

            if (!string.IsNullOrWhiteSpace(summaryState?.ActionStatus))
            {
                return summaryState.ActionStatus;
            }

            return $"Desk state: {s.AvailableTechs.Count} tech option(s), {s.WorkshopJobs.Count} workshop job(s), and {s.CityTimers.Count} live city timer(s).";
        }

        private void SetLane(DevelopmentLane lane)
        {
            activeLane = lane;
            ApplyLaneSelection();
        }

        private void ApplyLaneSelection()
        {
            ApplyButtonState(researchLaneButton, activeLane == DevelopmentLane.Research);
            ApplyButtonState(workshopLaneButton, activeLane == DevelopmentLane.Workshop);
            ApplyButtonState(growthLaneButton, activeLane == DevelopmentLane.Growth);

            if (researchSection != null) researchSection.style.display = activeLane == DevelopmentLane.Research ? DisplayStyle.Flex : DisplayStyle.None;
            if (workshopSection != null) workshopSection.style.display = activeLane == DevelopmentLane.Workshop ? DisplayStyle.Flex : DisplayStyle.None;
            if (growthSection != null) growthSection.style.display = activeLane == DevelopmentLane.Growth ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static void ApplyButtonState(Button button, bool active)
        {
            if (button == null) return;
            if (active)
            {
                button.AddToClassList("action-button--primary");
            }
            else
            {
                button.RemoveFromClassList("action-button--primary");
            }
        }

        private static void RenderCards(InfoCard[] slots, List<CardView> cards)
        {
            for (var i = 0; i < slots.Length; i++)
            {
                if (i < cards.Count)
                {
                    slots[i].Render(cards[i]);
                }
                else
                {
                    slots[i].RenderHidden();
                }
            }
        }

        private static string DescribeWorkshopLane(ShellSummarySnapshot s, int recipeCount, bool isBlackMarket)
        {
            var nowUtc = DateTime.UtcNow;
            var activeJobs = s.WorkshopJobs.Count(j => !IsWorkshopJobCollected(j) && !IsWorkshopJobCollectable(j, nowUtc));
            var readyJobs = s.WorkshopJobs.Count(j => !IsWorkshopJobCollected(j) && IsWorkshopJobCollectable(j, nowUtc));
            var workshopTimers = s.CityTimers.Count(t => string.Equals(t.Category, "workshop_job", StringComparison.OrdinalIgnoreCase));
            if (isBlackMarket)
            {
                return ShadowLaneText.DescribeWorkshopLane(activeJobs, readyJobs, recipeCount, workshopTimers);
            }

            if (activeJobs > 0)
            {
                return $"{activeJobs} active job(s) • {readyJobs} ready pickup(s)";
            }

            if (readyJobs > 0)
            {
                return $"{readyJobs} ready pickup(s) • no active workshop work.";
            }

            if (recipeCount > 0)
            {
                return $"{recipeCount} craft recipe(s) ready.";
            }

            return workshopTimers > 0 ? $"{workshopTimers} workshop timer(s) visible." : "No workshop queue visible.";
        }

        private static string DescribeBuildingLane(ShellSummarySnapshot s, bool isBlackMarket)
        {
            var nowUtc = DateTime.UtcNow;
            var buildings = SelectLaneBuildings(s, isBlackMarket);
            var buildTimers = SelectBuildTimers(s, isBlackMarket);
            var ready = buildings.Count(b => IsBuildingReady(b, nowUtc)) + buildTimers.Count(t => t.FinishesAtUtc.HasValue && t.FinishesAtUtc.Value <= nowUtc);
            var active = buildings.Count(b => IsActiveStatus(b.Status) && !IsBuildingReady(b, nowUtc)) + buildTimers.Count(t => !t.FinishesAtUtc.HasValue || t.FinishesAtUtc.Value > nowUtc);

            if (buildings.Count > 0 || buildTimers.Count > 0)
            {
                var label = isBlackMarket ? "front" : "building";
                var timerLabel = isBlackMarket ? "front timer" : "build timer";
                return $"{buildings.Count} {label} card(s) • {buildTimers.Count} {timerLabel}(s) • {active} active • {ready} ready";
            }

            var liveTimers = s.CityTimers?.Count ?? 0;
            if (s.ResourceTickTiming.NextTickAtUtc.HasValue)
            {
                return $"Next tick in {FormatRemaining(s.ResourceTickTiming.NextTickAtUtc.Value - nowUtc)} • no {(isBlackMarket ? "front" : "building")} payload yet.";
            }

            return liveTimers > 0 ? $"{liveTimers} live timer(s) visible; no {(isBlackMarket ? "front" : "building")} cards surfaced." : $"No {(isBlackMarket ? "front" : "building")} payload surfaced.";
        }

        private static List<CardView> BuildBuildingCards(ShellSummarySnapshot s, bool isBlackMarket, DateTime nowUtc, int maxCards)
        {
            var cards = new List<CardView>();
            var budget = Math.Max(0, maxCards);
            var buildings = SelectLaneBuildings(s, isBlackMarket);
            var timers = SortBuildTimers(SelectBuildTimers(s, isBlackMarket), nowUtc);
            var reservedTimerSlots = timers.Count > 0 && budget > 1 ? 1 : 0;
            var buildingBudget = Math.Max(0, budget - reservedTimerSlots);

            foreach (var building in buildings.Take(buildingBudget))
            {
                cards.Add(new CardView(
                    family: isBlackMarket ? "Operator front" : "Building",
                    title: FormatBuildingTitle(building, isBlackMarket),
                    lore: BuildBuildingLore(building, nowUtc),
                    note: BuildBuildingNote(building, isBlackMarket, nowUtc),
                    buttonText: IsBuildingReady(building, nowUtc) ? "Ready" : IsActiveStatus(building.Status) ? "Active" : "Visible",
                    buttonEnabled: false));
            }

            foreach (var timer in timers.Take(Math.Max(0, budget - cards.Count)))
            {
                cards.Add(new CardView(
                    family: isBlackMarket ? "Front timer" : "Build timer",
                    title: isBlackMarket ? NormalizeBlackMarketTimerLabel(timer.Label, timer.Category) : FirstNonBlank(timer.Label, HumanizeCategory(timer.Category)),
                    lore: FormatTimerState(timer.Status, timer.FinishesAtUtc, nowUtc),
                    note: FirstNonBlank(timer.Detail, isBlackMarket ? "Operator-front timing is visible from cityTimers." : "Construction timing is visible from cityTimers."),
                    buttonText: timer.FinishesAtUtc.HasValue && timer.FinishesAtUtc.Value <= nowUtc ? "Ready" : "Timed",
                    buttonEnabled: false));
            }

            return cards;
        }

        private static string BuildBuildingCardsCopy(ShellSummarySnapshot s, bool isBlackMarket, DateTime nowUtc)
        {
            var buildings = SelectLaneBuildings(s, isBlackMarket);
            var timers = SelectBuildTimers(s, isBlackMarket);
            var ready = buildings.Count(b => IsBuildingReady(b, nowUtc)) + timers.Count(t => t.FinishesAtUtc.HasValue && t.FinishesAtUtc.Value <= nowUtc);
            var active = buildings.Count(b => IsActiveStatus(b.Status) && !IsBuildingReady(b, nowUtc)) + timers.Count(t => !t.FinishesAtUtc.HasValue || t.FinishesAtUtc.Value > nowUtc);
            var label = isBlackMarket ? "operator-front" : "building";

            var timerLabel = isBlackMarket ? "front timer" : "build timer";
            if (buildings.Count == 0 && timers.Count == 0)
            {
                return $"No {label} card or {timerLabel} is visible in the current summary payload.";
            }

            return $"Showing {buildings.Count} {label} card(s), {timers.Count} {timerLabel}(s), {active} active, and {ready} ready/finished state(s).";
        }

        private static List<BuildingSnapshot> SelectLaneBuildings(ShellSummarySnapshot s, bool isBlackMarket)
        {
            return (s?.Buildings ?? new List<BuildingSnapshot>())
                .Where(b => b != null && BuildingBelongsToLane(b, isBlackMarket))
                .ToList();
        }

        private static List<CityTimerEntrySnapshot> SelectBuildTimers(ShellSummarySnapshot s, bool isBlackMarket)
        {
            return (s?.CityTimers ?? new List<CityTimerEntrySnapshot>())
                .Where(t => t != null && (IsBuildTimer(t) || (isBlackMarket && IsFrontTimer(t))) && (!isBlackMarket || ContainsShadowLane(t.Category) || ContainsShadowLane(t.Label) || ContainsShadowLane(t.Detail) || IsBlackMarketLane(s)))
                .ToList();
        }


        private static bool BuildingBelongsToLane(BuildingSnapshot building, bool isBlackMarket)
        {
            var lane = FirstNonBlank(building.Lane, building.Type, building.BuildingId, building.Name);
            var isShadow = ContainsShadowLane(lane) || ContainsShadowLane(building.Detail) || ContainsShadowLane(building.EffectSummary);
            return isBlackMarket ? isShadow || string.IsNullOrWhiteSpace(building.Lane) : !isShadow;
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

        private static bool IsFrontTimer(CityTimerEntrySnapshot timer)
        {
            var category = timer?.Category ?? string.Empty;
            var label = timer?.Label ?? string.Empty;
            var detail = timer?.Detail ?? string.Empty;
            return ContainsShadowLane(category)
                || ContainsShadowLane(label)
                || ContainsShadowLane(detail)
                || category.IndexOf("route", StringComparison.OrdinalIgnoreCase) >= 0
                || label.IndexOf("route", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static List<CityTimerEntrySnapshot> SortBuildTimers(List<CityTimerEntrySnapshot> timers, DateTime nowUtc)
        {
            return (timers ?? new List<CityTimerEntrySnapshot>())
                .OrderBy(t => t.FinishesAtUtc.HasValue && t.FinishesAtUtc.Value <= nowUtc ? 0 : 1)
                .ThenBy(t => t.FinishesAtUtc ?? DateTime.MaxValue)
                .ThenBy(t => FirstNonBlank(t.Label, t.Category, t.Id))
                .ToList();
        }


        private static bool IsBuildingReady(BuildingSnapshot building, DateTime nowUtc)
        {
            return IsReadyStatus(building?.Status) || (building?.FinishesAtUtc.HasValue == true && building.FinishesAtUtc.Value <= nowUtc);
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

        private static bool ContainsShadowLane(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return false;
            var value = raw.Trim();
            return value.IndexOf("black_market", StringComparison.OrdinalIgnoreCase) >= 0
                || value.IndexOf("black market", StringComparison.OrdinalIgnoreCase) >= 0
                || value.IndexOf("black-market", StringComparison.OrdinalIgnoreCase) >= 0
                || value.IndexOf("shadow", StringComparison.OrdinalIgnoreCase) >= 0
                || value.IndexOf("front", StringComparison.OrdinalIgnoreCase) >= 0
                || value.IndexOf("operator", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string FormatBuildingTitle(BuildingSnapshot building, bool isBlackMarket)
        {
            var name = FirstNonBlank(building?.Name, building?.BuildingId, isBlackMarket ? "Operator front" : "Building");
            var suffix = building?.Level.HasValue == true ? $" Lv {building.Level.Value}" : string.Empty;
            return name + suffix;
        }

        private static string BuildBuildingLore(BuildingSnapshot building, DateTime nowUtc)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(building?.Status)) parts.Add(HumanizeKey(building.Status));
            if (building?.FinishesAtUtc.HasValue == true) parts.Add(FormatRemaining(building.FinishesAtUtc.Value - nowUtc));
            if (building?.ProductionPerTick != null)
            {
                var production = FormatProduction(building.ProductionPerTick, new ResourcePresentationSnapshot());
                if (!production.StartsWith("No production", StringComparison.OrdinalIgnoreCase)) parts.Add(production);
            }
            return parts.Count > 0 ? string.Join(" • ", parts) : "Building card surfaced from summary payload.";
        }

        private static string BuildBuildingNote(BuildingSnapshot building, bool isBlackMarket, DateTime nowUtc)
        {
            if (IsBuildingReady(building, nowUtc)) return isBlackMarket ? "Front timer elapsed; refresh or claim flow can surface next." : "Build timer elapsed; refresh or claim flow can surface next.";
            if (!string.IsNullOrWhiteSpace(building?.EffectSummary)) return building.EffectSummary;
            if (!string.IsNullOrWhiteSpace(building?.Detail)) return building.Detail;
            if (building?.StartedAtUtc.HasValue == true) return isBlackMarket ? $"Front opened {building.StartedAtUtc.Value:HH:mm:ss} UTC." : $"Construction started {building.StartedAtUtc.Value:HH:mm:ss} UTC.";
            return isBlackMarket ? "Operator-front truth is visible without fake covert simulation." : "Building truth is visible without fake construction simulation.";
        }

        private static string BuildResearchLore(ResearchSnapshot research, bool isBlackMarket, DateTime nowUtc)
        {
            var prefix = isBlackMarket ? "Shadow-book front" : "Research";
            var parts = new List<string> { $"{prefix} • {FormatProgress(research?.Progress, research?.Cost)}" };
            if (research?.FinishesAtUtc.HasValue == true) parts.Add(FormatRemaining(research.FinishesAtUtc.Value - nowUtc));
            if (!string.IsNullOrWhiteSpace(research?.Status)) parts.Add(HumanizeKey(research.Status));
            return string.Join(" • ", parts);
        }

        private static string BuildResearchNote(ResearchSnapshot research, bool isBlackMarket, DateTime nowUtc)
        {
            if (research?.FinishesAtUtc.HasValue == true && research.FinishesAtUtc.Value <= nowUtc) return isBlackMarket ? "Research timer elapsed locally; refresh to load the resolved shadow-book state." : "Research timer elapsed locally; refresh to load the resolved tech state.";
            if (research?.FinishesAtUtc.HasValue == true) return isBlackMarket ? $"Front resolves in {FormatRemaining(research.FinishesAtUtc.Value - nowUtc)}." : $"Research completes in {FormatRemaining(research.FinishesAtUtc.Value - nowUtc)}.";
            if (research?.StartedAtUtc.HasValue == true) return isBlackMarket ? $"Front opened {research.StartedAtUtc.Value:HH:mm:ss} UTC." : $"Started {research.StartedAtUtc.Value:HH:mm:ss} UTC.";
            return isBlackMarket ? "Live shadow-book front from summary payload." : "Live research queue entry from summary payload.";
        }

        private static string FormatTimerState(string status, DateTime? finishesAtUtc, DateTime nowUtc)
        {
            var state = FirstNonBlank(status, "active");
            return finishesAtUtc.HasValue ? $"{HumanizeKey(state)} • {FormatRemaining(finishesAtUtc.Value - nowUtc)}" : HumanizeKey(state);
        }

        private static string DescribeSupport(ShellSummarySnapshot s, bool isBlackMarket)
        {
            if (isBlackMarket)
            {
                return ShadowLaneText.DescribeSupport(s);
            }

            if (s.ThreatWarnings.Count > 0)
            {
                return s.ThreatWarnings[0].Headline;
            }

            if (s.OpeningOperations.Count > 0)
            {
                return string.Join(" • ", s.OpeningOperations.Take(2).Select(o => $"{o.Title} ({HumanizeKey(o.Readiness)})"));
            }

            return "No extra support pressure is visible right now.";
        }

        private static string DescribeSupportTitle(ShellSummarySnapshot s, bool isBlackMarket)
        {
            if (isBlackMarket)
            {
                return ShadowLaneText.DescribeSupportTitle(s);
            }

            if (s.ThreatWarnings.Count > 0) return "Warning posture";
            if (s.OpeningOperations.Count > 0) return "Opening posture";
            return "Support posture";
        }


        private static bool IsBlackMarketLane(ShellSummarySnapshot s)
        {
            var lane = (s?.City?.SettlementLane ?? s?.City?.SettlementLaneLabel ?? string.Empty).Trim().ToLowerInvariant();
            return lane == "black_market" || lane == "black market" || lane == "black-market" || lane == "blackmarket" || lane == "shadow";
        }

        private static string FormatProgress(double? progress, double? cost)
        {
            if (cost.GetValueOrDefault() > 0)
            {
                return $"{progress.GetValueOrDefault():0.#}/{cost.Value:0.#}";
            }

            return progress.HasValue ? $"{progress.Value:0.#}" : "progress unknown";
        }

        private static string FormatProduction(ResourceSnapshot resource, ResourcePresentationSnapshot labels)
        {
            var chunks = new List<string>();
            ResourcePresentationText.AppendResource(chunks, labels, "food", resource.Food, "/tick");
            ResourcePresentationText.AppendResource(chunks, labels, "materials", resource.Materials, "/tick");
            ResourcePresentationText.AppendResource(chunks, labels, "wealth", resource.Wealth, "/tick");
            ResourcePresentationText.AppendResource(chunks, labels, "mana", resource.Mana, "/tick");
            ResourcePresentationText.AppendResource(chunks, labels, "knowledge", resource.Knowledge, "/tick");
            ResourcePresentationText.AppendResource(chunks, labels, "unity", resource.Unity, "/tick");
            return chunks.Count == 0 ? "No production snapshot surfaced." : string.Join(" • ", chunks);
        }

        private static string BuildWorkshopRecipeNote(WorkshopRecipeSnapshot recipe, bool isBlackMarket, ResourcePresentationSnapshot labels)
        {
            if (isBlackMarket)
            {
                return ShadowLaneText.BuildWorkshopRecipeNote(recipe, labels);
            }

            var parts = new List<string>();
            if (recipe.WealthCost.HasValue) parts.Add($"Wealth {recipe.WealthCost.Value:0.#}");
            if (recipe.ManaCost.HasValue) parts.Add($"Mana {recipe.ManaCost.Value:0.#}");
            if (recipe.MaterialsCost.HasValue) parts.Add($"Materials {recipe.MaterialsCost.Value:0.#}");
            if (recipe.CraftMinutes.HasValue) parts.Add($"Time {FormatMinutes(recipe.CraftMinutes.Value)}");
            if (recipe.ResponseTags.Count > 0) parts.Add(string.Join("/", recipe.ResponseTags.Take(2).Select(HumanizeKey)));
            return parts.Count > 0 ? string.Join(" • ", parts) : "Workshop recipe is ready to craft.";
        }

        private static string BuildTechNote(TechOptionSnapshot tech, bool isBlackMarket)
        {
            if (isBlackMarket)
            {
                return ShadowLaneText.BuildTechNote(tech);
            }

            var parts = new List<string>();
            if (tech.Cost.HasValue) parts.Add($"Cost {tech.Cost.Value:0.#}");
            if (!string.IsNullOrWhiteSpace(tech.LaneIdentity) && !string.Equals(tech.LaneIdentity, "neutral", StringComparison.OrdinalIgnoreCase)) parts.Add($"Lane {HumanizeKey(tech.LaneIdentity)}");
            if (!string.IsNullOrWhiteSpace(tech.OperatorNote)) parts.Add(tech.OperatorNote);
            return parts.Count > 0 ? string.Join(" • ", parts) : "No extra operator note supplied.";
        }

        private static string FirstNonBlank(params string[] values) => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? string.Empty;

        private static string FormatMinutes(double minutes)
        {
            if (minutes <= 0) return "now";
            var span = TimeSpan.FromMinutes(minutes);
            return span.TotalHours >= 1 ? span.ToString(@"hh\:mm") : span.ToString(@"mm\:ss");
        }

        private static string FormatRemaining(TimeSpan span)
        {
            if (span <= TimeSpan.Zero) return "now";
            return span.TotalHours >= 1 ? span.ToString(@"hh\:mm\:ss") : span.ToString(@"mm\:ss");
        }

        private static string HumanizeCategory(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "Category";
            return HumanizeKey(raw);
        }

        private static string HumanizeBlackMarketTimerCategory(string raw)
        {
            if (string.Equals(raw, "warfront_window", StringComparison.OrdinalIgnoreCase)) return "Operations window";
            if (string.Equals(raw, "army_reinforcement", StringComparison.OrdinalIgnoreCase)) return "Cell reinforcement";
            return HumanizeCategory(raw);
        }

        private static string NormalizeBlackMarketTimerLabel(string label, string category)
        {
            var cleaned = FirstNonBlank(label);
            if (string.IsNullOrWhiteSpace(cleaned))
            {
                return string.Equals(category, "warfront_window", StringComparison.OrdinalIgnoreCase)
                    ? "Operations window"
                    : HumanizeBlackMarketTimerCategory(category);
            }

            cleaned = cleaned.Replace("Warfront window", "Operations window", StringComparison.OrdinalIgnoreCase);
            cleaned = cleaned.Replace("Warfront Window", "Operations window", StringComparison.OrdinalIgnoreCase);
            cleaned = cleaned.Replace("warfront window", "operations window", StringComparison.OrdinalIgnoreCase);
            return cleaned;
        }

        private static string HumanizeLane(string raw) => string.IsNullOrWhiteSpace(raw) ? "development" : raw;

        private static string HumanizeKey(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "-";
            var cleaned = raw.Replace('_', ' ').Trim();
            return string.Join(" ", cleaned.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpperInvariant(part[0]) + (part.Length > 1 ? part.Substring(1) : string.Empty)));
        }

        private readonly struct CardView
        {
            public CardView(string family, string title, string lore, string note, string buttonText = null, bool buttonEnabled = false, Action onClick = null)
            {
                Family = family;
                Title = title;
                Lore = lore;
                Note = note;
                ButtonText = buttonText;
                ButtonEnabled = buttonEnabled;
                OnClick = onClick;
            }

            public string Family { get; }
            public string Title { get; }
            public string Lore { get; }
            public string Note { get; }
            public string ButtonText { get; }
            public bool ButtonEnabled { get; }
            public Action OnClick { get; }
        }

        private sealed class InfoCard
        {
            private readonly VisualElement root;
            private readonly Label family;
            private readonly Label title;
            private readonly Label lore;
            private readonly Label note;
            private readonly Button button;
            private Action clickAction;

            public InfoCard(VisualElement shellRoot, string prefix, bool hasButton = false)
            {
                root = shellRoot.Q<VisualElement>(prefix);
                family = shellRoot.Q<Label>($"{prefix}-family-value");
                title = shellRoot.Q<Label>($"{prefix}-title-value");
                lore = shellRoot.Q<Label>($"{prefix}-lore-value");
                note = shellRoot.Q<Label>($"{prefix}-note-value");
                button = hasButton ? shellRoot.Q<Button>($"{prefix}-button") : null;
                if (button != null)
                {
                    button.clicked += () => clickAction?.Invoke();
                }
            }

            public void Render(CardView view)
            {
                if (root == null) return;
                root.style.display = DisplayStyle.Flex;
                if (family != null) family.text = view.Family;
                if (title != null) title.text = view.Title;
                if (lore != null) lore.text = view.Lore;
                if (note != null) note.text = view.Note;
                if (button != null)
                {
                    clickAction = view.OnClick;
                    button.style.display = string.IsNullOrWhiteSpace(view.ButtonText) ? DisplayStyle.None : DisplayStyle.Flex;
                    button.text = view.ButtonText ?? "Read-only";
                    button.SetEnabled(view.ButtonEnabled && clickAction != null);
                }
            }

            public void RenderHidden()
            {
                clickAction = null;
                if (root != null) root.style.display = DisplayStyle.None;
            }
        }
    }
}
