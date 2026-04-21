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

            if (!s.HasCity)
            {
                RenderNoCity();
                return;
            }

            headline.text = isBlackMarket ? ShadowLaneText.DescribeDevelopmentHeadline(s.City) : $"{s.City.Name} • Development";
            copy.text = isBlackMarket
                ? ShadowLaneText.DescribeDevelopmentCopy(s.City)
                : $"Tier {(s.City.Tier ?? 0)} {HumanizeLane(s.City.SettlementLaneLabel)} desk. Review research, queues, and growth posture without leaving the city shell.";

            card1Title.text = isBlackMarket ? "Shadow books" : "Research lane";
            card1Value.text = s.ActiveResearch != null
                ? $"{s.ActiveResearch.Name} • {FormatProgress(s.ActiveResearch.Progress, s.ActiveResearch.Cost)}"
                : s.AvailableTechs.Count > 0
                    ? $"{s.AvailableTechs.Count} tech option{(s.AvailableTechs.Count == 1 ? string.Empty : "s")} ready"
                    : "No active research or tech options surfaced.";
            card2Title.text = isBlackMarket ? "Covert supply" : "Workshop lane";
            card2Value.text = DescribeWorkshopLane(s, recipeCount);
            card3Title.text = isBlackMarket ? "Carry lane" : "Growth lane";
            card3Value.text = DescribeGrowthLane(s, isBlackMarket);

            researchFocusValue.text = s.ActiveResearch?.Name ?? (isBlackMarket ? "No active shadow-book focus." : "No active research focus.");
            nextTechValue.text = s.AvailableTechs.FirstOrDefault()?.Name ?? (isBlackMarket ? "No quiet leverage unlock surfaced." : "No available tech surfaced.");
            workshopValue.text = DescribeWorkshopLane(s, recipeCount);
            growthValue.text = DescribeGrowthLane(s, isBlackMarket);
            supportValue.text = DescribeSupport(s, isBlackMarket);
            noteValue.text = BuildDeskNote(s, summaryState, isBlackMarket);
            if (startSuggestedResearchButton != null)
            {
                var suggestedTech = GetSuggestedTech(s);
                startSuggestedResearchButton.text = summaryState.IsActionBusy && !string.IsNullOrWhiteSpace(summaryState.PendingResearchTechId)
                    ? "Starting..."
                    : "Start suggested research";
                startSuggestedResearchButton.SetEnabled(suggestedTech != null && !summaryState.IsActionBusy && onStartResearchRequested != null);
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
            card3Title.text = "Growth lane";
            card3Value.text = "Found a city to unlock cadence and support posture.";
            researchFocusValue.text = "No research lane loaded.";
            nextTechValue.text = "No suggested unlock surfaced.";
            workshopValue.text = "No workshop queue visible.";
            growthValue.text = "Growth cadence unavailable.";
            supportValue.text = "Support posture unavailable.";
            noteValue.text = "Development Desk v1 stays honest: no city snapshot means no desk truth.";
            researchCardsCopyValue.text = "No research cards without a city payload.";
            workshopCardsCopyValue.text = "No workshop cards without a city payload.";
            growthCardsCopyValue.text = "No growth cards without a city payload.";
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
            laneTitle.text = activeLane == DevelopmentLane.Research ? (isBlackMarket ? ShadowLaneText.BuildResearchLaneTitle() : "Research lane") : laneTitle.text;
            laneCopy.text = activeLane == DevelopmentLane.Research
                ? (isBlackMarket ? ShadowLaneText.BuildResearchLaneCopy() : "Research keeps the current tech, next unlocks, and readiness posture visible in one place so you can judge the next order before mutation wiring lands.")
                : laneCopy.text;

            var cards = new List<CardView>();
            if (s.ActiveResearch != null)
            {
                cards.Add(new CardView(
                    family: "Active research",
                    title: s.ActiveResearch.Name,
                    lore: $"Progress {FormatProgress(s.ActiveResearch.Progress, s.ActiveResearch.Cost)}",
                    note: s.ActiveResearch.StartedAtUtc.HasValue ? $"Started {s.ActiveResearch.StartedAtUtc:HH:mm:ss} UTC" : "Live research queue entry from summary payload.",
                    buttonText: "Live",
                    buttonEnabled: false));
            }

            cards.AddRange(s.AvailableTechs.Take(4 - cards.Count).Select(tech => new CardView(
                family: string.IsNullOrWhiteSpace(tech.IdentityFamily) ? HumanizeCategory(tech.Category) : tech.IdentityFamily,
                title: tech.Name,
                lore: FirstNonBlank(tech.IdentitySummary, tech.Description, tech.UnlockPreview.FirstOrDefault(), "No research summary provided."),
                note: BuildTechNote(tech),
                buttonText: summaryState.IsActionBusy && string.Equals(summaryState.PendingResearchTechId, tech.Id, StringComparison.OrdinalIgnoreCase) ? "Starting..." : "Start research",
                buttonEnabled: !summaryState.IsActionBusy && onStartResearchRequested != null,
                onClick: () => TriggerStartResearch(tech.Id))));

            if (cards.Count == 0)
            {
                cards.Add(new CardView("Research payload", "No tech entry", "The current /api/me payload did not surface availableTechs or an activeResearch entry.", "No mutation wiring is attempted here.", "Read-only", false));
            }

            researchCardsCopyValue.text = s.ActiveResearch != null
                ? $"Active research plus {Math.Max(0, s.AvailableTechs.Count)} surfaced tech option(s)."
                : s.AvailableTechs.Count > 0
                    ? $"Showing {s.AvailableTechs.Count} available tech option(s) from /api/me."
                    : "No research cards were surfaced in payload.";

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
                family: "Active job",
                title: GetWorkshopJobTitle(job),
                lore: job.FinishesAtUtc.HasValue ? $"Ready in {FormatRemaining(job.FinishesAtUtc.Value - nowUtc)}" : "Job surfaced without a finish anchor.",
                note: "Queued workshop job from summary payload.",
                buttonText: "In flight",
                buttonEnabled: false)));

            cards.AddRange(readyJobs.Take(2).Select(job => new CardView(
                family: "Ready pickup",
                title: GetWorkshopJobTitle(job),
                lore: "Crafting time elapsed. Collect to deliver the item into city armory storage.",
                note: job.Id == "job" ? "Ready workshop item surfaced without a stable job id." : $"Ready job {job.Id} can now be collected into storage.",
                buttonText: summaryState.IsActionBusy && string.Equals(summaryState.PendingWorkshopJobId, job.Id, StringComparison.OrdinalIgnoreCase) ? "Collecting..." : "Collect",
                buttonEnabled: !summaryState.IsActionBusy && !string.IsNullOrWhiteSpace(job.Id) && job.Id != "job" && onCollectWorkshopRequested != null,
                onClick: () => TriggerCollectWorkshop(job.Id))));

            cards.AddRange(workshopTimers.Take(4 - cards.Count).Select(timer => new CardView(
                family: "Workshop timer",
                title: timer.Label,
                lore: timer.FinishesAtUtc.HasValue ? $"{timer.Status} • {FormatRemaining(timer.FinishesAtUtc.Value - DateTime.UtcNow)}" : timer.Status,
                note: FirstNonBlank(timer.Detail, "Timer surfaced from cityTimers."))));

            if (cards.Count < 4)
            {
                var recipeCards = summaryState.WorkshopRecipes
                    .Where(r => !string.IsNullOrWhiteSpace(r.RecipeId))
                    .Take(4 - cards.Count)
                    .Select(recipe => new CardView(
                        family: string.IsNullOrWhiteSpace(recipe.GearFamily) ? "Workshop recipe" : HumanizeKey(recipe.GearFamily),
                        title: recipe.Name,
                        lore: FirstNonBlank(recipe.Summary, recipe.ResponseTags.FirstOrDefault(), "Craftable recipe from workshop catalog."),
                        note: BuildWorkshopRecipeNote(recipe),
                        buttonText: summaryState.IsActionBusy && string.Equals(summaryState.PendingWorkshopRecipeId, recipe.RecipeId, StringComparison.OrdinalIgnoreCase) ? "Crafting..." : "Craft",
                        buttonEnabled: !summaryState.IsActionBusy && onStartWorkshopCraftRequested != null,
                        onClick: () => TriggerStartWorkshopCraft(recipe.RecipeId)));
                cards.AddRange(recipeCards);
            }

            if (cards.Count == 0)
            {
                cards.Add(new CardView("Workshop payload", "No workshop entry", "No workshop job, timer, or recipe catalog entry is currently visible.", "The desk stays honest instead of inventing a fake queue.", "Read-only", false));
            }

            workshopCardsCopyValue.text = activeJobs.Count > 0
                ? $"{activeJobs.Count} active workshop job(s) and {readyJobs.Count} ready pickup(s) are visible."
                : readyJobs.Count > 0
                    ? $"{readyJobs.Count} ready pickup(s) visible; collect wiring is now live."
                    : summaryState.WorkshopRecipes.Count > 0
                        ? $"Showing {summaryState.WorkshopRecipes.Count} craft recipe(s) from /api/workshop/recipes."
                        : workshopTimers.Count > 0
                            ? $"Workshop timing is coming through cityTimers even though no job body is active."
                            : "No workshop queue, timer, or recipe catalog is visible right now.";

            RenderCards(workshopCards, cards);
        }

        private void RenderGrowthLane(ShellSummarySnapshot s)
        {
            var isBlackMarket = IsBlackMarketLane(s);
            laneTitle.text = activeLane == DevelopmentLane.Growth ? (isBlackMarket ? ShadowLaneText.BuildGrowthLaneTitle() : "Growth lane") : laneTitle.text;
            laneCopy.text = activeLane == DevelopmentLane.Growth
                ? (isBlackMarket ? ShadowLaneText.BuildGrowthLaneCopy() : "Growth keeps cadence, staffing, and support posture visible so you can read the city’s pacing without drilling into mutations.")
                : laneCopy.text;

            var nowUtc = DateTime.UtcNow;
            var cards = new List<CardView>();
            cards.Add(new CardView(
                family: "Production",
                title: "Per-tick output",
                lore: FormatProduction(s.ProductionPerTick),
                note: s.ResourceTickTiming.NextTickAtUtc.HasValue ? $"Next resource tick in {FormatRemaining(s.ResourceTickTiming.NextTickAtUtc.Value - nowUtc)}" : "Resource cadence is visible without a live anchor."));

            var heroRecruitment = s.HeroRecruitment;
            var recruitTimer = s.CityTimers.FirstOrDefault(t => string.Equals(t.Category, "operator_recruit", StringComparison.OrdinalIgnoreCase));
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
                    note: FirstNonBlank(recruitTimer.Detail, "Hero recruitment timer surfaced from /api/me."),
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
                var recruitOp = s.OpeningOperations.FirstOrDefault(o => string.Equals(o.Kind, "recruit_hero", StringComparison.OrdinalIgnoreCase) && string.Equals(o.Readiness, "ready_now", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(o.Role));
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
                .Where(t => !string.Equals(t.Category, "resource_tick", StringComparison.OrdinalIgnoreCase) && !string.Equals(t.Category, "workshop_job", StringComparison.OrdinalIgnoreCase) && !string.Equals(t.Category, "operator_recruit", StringComparison.OrdinalIgnoreCase))
                .Take(Math.Max(0, 3 - cards.Count))
                .Select(timer => new CardView(
                    family: HumanizeCategory(timer.Category),
                    title: timer.Label,
                    lore: timer.FinishesAtUtc.HasValue ? $"{timer.Status} • {FormatRemaining(timer.FinishesAtUtc.Value - nowUtc)}" : timer.Status,
                    note: FirstNonBlank(timer.Detail, "Live city timer surfaced from /api/me."))));

            if (cards.Count < 4)
            {
                cards.Add(new CardView(
                    family: "Support posture",
                    title: DescribeSupportTitle(s, isBlackMarket),
                    lore: DescribeSupport(s, isBlackMarket),
                    note: s.OpeningOperations.Count > 0 ? $"Opening operations visible: {string.Join(" • ", s.OpeningOperations.Take(2).Select(o => o.Title))}" : "No extra opening operation pressure is surfaced right now."));
            }

            growthCardsCopyValue.text = heroRecruitment != null && string.Equals(heroRecruitment.Status, "candidates_ready", StringComparison.OrdinalIgnoreCase)
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
            if (suggested == null || summaryState.IsActionBusy || onStartResearchRequested == null)
            {
                return;
            }

            _ = onStartResearchRequested.Invoke(suggested.Id);
        }

        private void TriggerStartResearch(string techId)
        {
            if (summaryState.IsActionBusy || onStartResearchRequested == null || string.IsNullOrWhiteSpace(techId))
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
                var costParts = new List<string>();
                if (candidate.WealthCost.HasValue) costParts.Add($"Wealth {candidate.WealthCost.Value:0.#}");
                if (candidate.UnityCost.HasValue) costParts.Add($"Unity {candidate.UnityCost.Value:0.#}");
                parts.Add(string.Join(" • ", costParts));
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
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(recruitment.Role)) parts.Add($"Role {HumanizeKey(recruitment.Role)}");
            if (recruitment.WealthCost.HasValue) parts.Add($"Wealth {recruitment.WealthCost.Value:0.#}");
            if (recruitment.UnityCost.HasValue) parts.Add($"Unity {recruitment.UnityCost.Value:0.#}");
            return parts.Count > 0 ? string.Join(" • ", parts) : "Recruitment scouting is in progress.";
        }

        private static string BuildHeroRecruitmentIdleLore(HeroRecruitmentSnapshot recruitment)
        {
            var parts = new List<string>();
            var startRole = FirstNonBlank(recruitment?.StartRole, recruitment?.Role);
            if (!string.IsNullOrWhiteSpace(startRole)) parts.Add($"Role {HumanizeKey(startRole)}");
            if (!string.IsNullOrWhiteSpace(recruitment?.Lane)) parts.Add(HumanizeKey(recruitment.Lane));
            if (recruitment?.WealthCost.HasValue == true) parts.Add($"Wealth {recruitment.WealthCost.Value:0.#}");
            if (recruitment?.UnityCost.HasValue == true) parts.Add($"Unity {recruitment.UnityCost.Value:0.#}");
            return parts.Count > 0 ? string.Join(" • ", parts) : "Hero recruitment can open from the Growth desk.";
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

        private static string DescribeWorkshopLane(ShellSummarySnapshot s, int recipeCount)
        {
            var nowUtc = DateTime.UtcNow;
            var activeJobs = s.WorkshopJobs.Count(j => !IsWorkshopJobCollected(j) && !IsWorkshopJobCollectable(j, nowUtc));
            var readyJobs = s.WorkshopJobs.Count(j => !IsWorkshopJobCollected(j) && IsWorkshopJobCollectable(j, nowUtc));
            var workshopTimers = s.CityTimers.Count(t => string.Equals(t.Category, "workshop_job", StringComparison.OrdinalIgnoreCase));
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

        private static string DescribeGrowthLane(ShellSummarySnapshot s, bool isBlackMarket)
        {
            if (isBlackMarket)
            {
                return ShadowLaneText.DescribeGrowth(s.ResourceTickTiming, s.CityTimers);
            }

            var liveTimers = s.CityTimers.Count;
            if (s.ResourceTickTiming.NextTickAtUtc.HasValue)
            {
                return $"Next tick in {FormatRemaining(s.ResourceTickTiming.NextTickAtUtc.Value - DateTime.UtcNow)} • {liveTimers} live timer(s).";
            }

            return liveTimers > 0 ? $"{liveTimers} live timer(s) visible; cadence is readable." : "Growth cadence unavailable.";
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

        private static string FormatProduction(ResourceSnapshot resource)
        {
            var chunks = new List<string>();
            AppendResource(chunks, "Food", resource.Food);
            AppendResource(chunks, "Materials", resource.Materials);
            AppendResource(chunks, "Wealth", resource.Wealth);
            AppendResource(chunks, "Mana", resource.Mana);
            AppendResource(chunks, "Knowledge", resource.Knowledge);
            AppendResource(chunks, "Unity", resource.Unity);
            return chunks.Count == 0 ? "No production snapshot surfaced." : string.Join(" • ", chunks);
        }

        private static void AppendResource(List<string> chunks, string label, double? value)
        {
            if (value.HasValue)
            {
                chunks.Add($"{label} {value.Value:0.#}/tick");
            }
        }

        private static string BuildWorkshopRecipeNote(WorkshopRecipeSnapshot recipe)
        {
            var parts = new List<string>();
            if (recipe.WealthCost.HasValue) parts.Add($"Wealth {recipe.WealthCost.Value:0.#}");
            if (recipe.ManaCost.HasValue) parts.Add($"Mana {recipe.ManaCost.Value:0.#}");
            if (recipe.MaterialsCost.HasValue) parts.Add($"Materials {recipe.MaterialsCost.Value:0.#}");
            if (recipe.CraftMinutes.HasValue) parts.Add($"Time {FormatMinutes(recipe.CraftMinutes.Value)}");
            if (recipe.ResponseTags.Count > 0) parts.Add(string.Join("/", recipe.ResponseTags.Take(2).Select(HumanizeKey)));
            return parts.Count > 0 ? string.Join(" • ", parts) : "Workshop recipe is ready to craft.";
        }

        private static string BuildTechNote(TechOptionSnapshot tech)
        {
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
