using PlanarWar.Client.Core;
using PlanarWar.Client.Core.Contracts;
using PlanarWar.Client.Core.Presentation;
using PlanarWar.Client.UI.Screens.Heroes;
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
        private readonly DropdownField workshopSlotSelectField;
        private readonly DropdownField workshopRecipeSelectField;
        private readonly Label workshopSelectedRecipeValue;
        private readonly Button workshopCraftSelectedButton;
        private readonly VisualElement workshopRecipePicker;

        private readonly Button researchLaneButton;
        private readonly Button workshopLaneButton;
        private readonly Button growthLaneButton;
        private readonly VisualElement researchSection;
        private readonly VisualElement workshopSection;
        private readonly VisualElement growthSection;

        private const int VisibleDevelopmentCardSlots = 4;
        private const int VisibleWorkshopCardSlots = 10;

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
        private readonly Func<string, Task> onConstructBuildingRequested;
        private readonly Func<string, Task> onUpgradeBuildingRequested;
        private readonly Func<string, string, Task> onSwitchBuildingRoutingRequested;
        private readonly Func<string, Task> onDestroyBuildingRequested;
        private readonly Func<string, string, Task> onRemodelBuildingRequested;
        private readonly Func<string, Task> onCancelActiveBuildRequested;
        private readonly Action onRefreshDeskRequested;
        private readonly Action onBackHomeRequested;
        private readonly Button refreshDeskButton;
        private readonly Button startSuggestedResearchButton;
        private readonly Button backHomeButton;

        private DevelopmentLane activeLane = DevelopmentLane.Research;
        private string selectedCityBuildingId = string.Empty;
        private string selectedBlackMarketBuildingId = string.Empty;
        private string selectedCityBuildOptionKind = string.Empty;
        private string selectedBlackMarketBuildOptionKind = string.Empty;
        private readonly List<string> workshopSlotChoiceKeys = new();
        private readonly List<WorkshopRecipeSnapshot> workshopRecipeChoiceSnapshots = new();
        private string selectedWorkshopSlotKey = "all";
        private string selectedWorkshopRecipeId = string.Empty;
        private ShellSummarySnapshot lastRenderedSnapshot = ShellSummarySnapshot.Empty;

        public CityScreenController(VisualElement root, SummaryState summaryState, Func<string, Task> onStartResearchRequested, Func<string, Task> onStartWorkshopCraftRequested, Func<string, Task> onCollectWorkshopRequested, Func<string, Task> onRecruitHeroRequested, Func<string, Task> onAcceptHeroRecruitCandidateRequested, Func<Task> onDismissHeroRecruitCandidatesRequested, Func<string, Task> onConstructBuildingRequested, Func<string, Task> onUpgradeBuildingRequested, Func<string, string, Task> onSwitchBuildingRoutingRequested, Func<string, Task> onDestroyBuildingRequested, Func<string, string, Task> onRemodelBuildingRequested, Func<string, Task> onCancelActiveBuildRequested, Action onRefreshDeskRequested, Action onBackHomeRequested)
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
            workshopSlotSelectField = root.Q<DropdownField>("dev-workshop-slot-field");
            workshopRecipeSelectField = root.Q<DropdownField>("dev-workshop-recipe-field");
            workshopSelectedRecipeValue = root.Q<Label>("dev-workshop-selected-recipe-value");
            workshopCraftSelectedButton = root.Q<Button>("dev-workshop-craft-selected-button");
            workshopRecipePicker = root.Q<VisualElement>("dev-workshop-recipe-picker");

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
            this.onConstructBuildingRequested = onConstructBuildingRequested;
            this.onUpgradeBuildingRequested = onUpgradeBuildingRequested;
            this.onSwitchBuildingRoutingRequested = onSwitchBuildingRoutingRequested;
            this.onDestroyBuildingRequested = onDestroyBuildingRequested;
            this.onRemodelBuildingRequested = onRemodelBuildingRequested;
            this.onCancelActiveBuildRequested = onCancelActiveBuildRequested;
            this.onRefreshDeskRequested = onRefreshDeskRequested;
            this.onBackHomeRequested = onBackHomeRequested;

            researchCards = Enumerable.Range(1, VisibleDevelopmentCardSlots).Select(i => new InfoCard(root, $"dev-research-card-{i}", hasButton: true)).ToArray();
            workshopCards = Enumerable.Range(1, VisibleWorkshopCardSlots).Select(i => new InfoCard(root, $"dev-workshop-card-{i}", hasButton: true)).ToArray();
            growthCards = Enumerable.Range(1, VisibleDevelopmentCardSlots).Select(i => new InfoCard(root, $"dev-growth-card-{i}", hasButton: true)).ToArray();
            refreshDeskButton = root.Q<Button>("dev-refresh-button");
            startSuggestedResearchButton = root.Q<Button>("dev-start-research-button");
            backHomeButton = root.Q<Button>("dev-back-home-button");

            researchLaneButton?.RegisterCallback<ClickEvent>(_ => SetLane(DevelopmentLane.Research));
            workshopLaneButton?.RegisterCallback<ClickEvent>(_ => SetLane(DevelopmentLane.Workshop));
            growthLaneButton?.RegisterCallback<ClickEvent>(_ => SetLane(DevelopmentLane.Growth));
            workshopSlotSelectField?.RegisterValueChangedCallback(evt => SelectWorkshopSlot(evt.newValue));
            workshopRecipeSelectField?.RegisterValueChangedCallback(evt => SelectWorkshopRecipe(evt.newValue));
            workshopCraftSelectedButton?.RegisterCallback<ClickEvent>(_ => TriggerStartWorkshopCraft(selectedWorkshopRecipeId));
            refreshDeskButton?.RegisterCallback<ClickEvent>(_ => onRefreshDeskRequested?.Invoke());
            startSuggestedResearchButton?.RegisterCallback<ClickEvent>(_ => TriggerSuggestedResearch());
            backHomeButton?.RegisterCallback<ClickEvent>(_ => onBackHomeRequested?.Invoke());
            ApplyLaneSelection();
        }

        public void Render(ShellSummarySnapshot s, SummaryState summaryState)
        {
            lastRenderedSnapshot = s ?? ShellSummarySnapshot.Empty;
            var recipeCount = summaryState?.WorkshopRecipes?.Count ?? 0;
            var isBlackMarket = IsBlackMarketLane(s);
            var nowUtc = DateTime.UtcNow;
            var activeResearches = SelectActiveResearches(s, nowUtc);
            var visibleAvailableTechs = SelectAvailableResearchOptions(s, activeResearches);
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
                : visibleAvailableTechs.Count > 0
                    ? isBlackMarket
                        ? ShadowLaneText.DescribeResearchLaneValue(visibleAvailableTechs)
                        : $"{visibleAvailableTechs.Count} tech option{(visibleAvailableTechs.Count == 1 ? string.Empty : "s")} ready"
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
                ? ShadowLaneText.BuildNextTechValue(visibleAvailableTechs)
                : visibleAvailableTechs.FirstOrDefault()?.Name ?? "No available tech surfaced.";
            workshopValue.text = DescribeWorkshopLane(s, recipeCount, isBlackMarket);
            growthValue.text = DescribeBuildingLane(s, isBlackMarket);
            supportValue.text = DescribeSupport(s, isBlackMarket);
            noteValue.text = BuildDeskNote(s, summaryState, isBlackMarket);
            if (startSuggestedResearchButton != null)
            {
                var suggestedTech = GetSuggestedTech(s, activeResearches);
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
            RenderWorkshopRecipePicker(Array.Empty<WorkshopRecipeSnapshot>(), false, null);
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
            var visibleAvailableTechs = SelectAvailableResearchOptions(s, activeResearches);
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
                    buttonText: IsResearchReady(research, nowUtc) ? "Ready to update" : "Live",
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

            cards.AddRange(visibleAvailableTechs.Take(Math.Max(0, 4 - cards.Count)).Select(tech => new CardView(
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

            researchCardsCopyValue.text = BuildResearchCardsCopy(s, isBlackMarket, activeResearches, visibleAvailableTechs, nowUtc);

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
            var recipeCatalog = (summaryState?.WorkshopRecipes ?? new List<WorkshopRecipeSnapshot>())
                .Where(r => r != null && !string.IsNullOrWhiteSpace(r.RecipeId))
                .ToList();
            var visibleRecipeCatalog = FilterWorkshopRecipesBySelectedSlot(recipeCatalog);
            RenderWorkshopRecipePicker(recipeCatalog, isBlackMarket, s.ResourceLabels);

            cards.AddRange(activeJobs.Take(2).Select(job => new CardView(
                family: isBlackMarket ? ShadowLaneText.BuildWorkshopJobFamily(ready: false) : "Active job",
                title: GetWorkshopJobTitle(job, summaryState.WorkshopRecipes),
                lore: isBlackMarket ? ShadowLaneText.BuildWorkshopJobLore(job, ready: false, nowUtc) : job.FinishesAtUtc.HasValue ? $"Ready in {FormatRemaining(job.FinishesAtUtc.Value - nowUtc)}" : "Job surfaced without a finish anchor.",
                note: isBlackMarket ? ShadowLaneText.BuildWorkshopJobNote(job, ready: false) : "Queued workshop job from summary payload.",
                buttonText: "In flight",
                buttonEnabled: false)));

            cards.AddRange(readyJobs.Take(2).Select(job => new CardView(
                family: isBlackMarket ? ShadowLaneText.BuildWorkshopJobFamily(ready: true) : "Ready to collect",
                title: GetWorkshopJobTitle(job, summaryState.WorkshopRecipes),
                lore: isBlackMarket ? ShadowLaneText.BuildWorkshopJobLore(job, ready: true, nowUtc) : "Crafting time elapsed. Collect to deliver the item into city armory storage.",
                note: isBlackMarket
                    ? ShadowLaneText.BuildWorkshopJobNote(job, ready: true)
                    : BuildWorkshopReadyPickupNote(job, summaryState.WorkshopRecipes),
                buttonText: summaryState.IsActionBusy && string.Equals(summaryState.PendingWorkshopJobId, job.Id, StringComparison.OrdinalIgnoreCase) ? "Collecting..." : "Collect",
                buttonEnabled: !summaryState.IsActionBusy && !string.IsNullOrWhiteSpace(job.Id) && job.Id != "job" && onCollectWorkshopRequested != null,
                onClick: () => TriggerCollectWorkshop(job.Id))));

            cards.AddRange(workshopTimers.Take(Math.Max(0, VisibleWorkshopCardSlots - cards.Count)).Select(timer => new CardView(
                family: isBlackMarket ? ShadowLaneText.BuildWorkshopTimerFamily(timer) : "Workshop timer",
                title: GetWorkshopTimerTitle(timer, summaryState.WorkshopRecipes),
                lore: isBlackMarket ? ShadowLaneText.BuildWorkshopTimerLore(timer, nowUtc) : timer.FinishesAtUtc.HasValue ? $"{timer.Status} • {FormatRemaining(timer.FinishesAtUtc.Value - DateTime.UtcNow)}" : timer.Status,
                note: isBlackMarket ? ShadowLaneText.BuildWorkshopTimerNote(timer) : FirstNonBlank(timer.Detail, "Timer surfaced from cityTimers."))));

            if (cards.Count < VisibleWorkshopCardSlots)
            {
                var recipeCards = visibleRecipeCatalog
                    .Take(Math.Max(0, VisibleWorkshopCardSlots - cards.Count))
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

            workshopCardsCopyValue.text = BuildWorkshopCatalogCopy(isBlackMarket, activeJobs.Count, readyJobs.Count, workshopTimers.Count, recipeCatalog.Count, visibleRecipeCatalog.Count);

            RenderCards(workshopCards, cards);
        }


        private void SelectWorkshopSlot(string label)
        {
            var index = workshopSlotSelectField?.choices?.IndexOf(label) ?? -1;
            if (index < 0 || index >= workshopSlotChoiceKeys.Count)
            {
                return;
            }

            var nextSlot = workshopSlotChoiceKeys[index];
            if (string.Equals(selectedWorkshopSlotKey, nextSlot, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            selectedWorkshopSlotKey = nextSlot;
            selectedWorkshopRecipeId = string.Empty;
            if (lastRenderedSnapshot?.HasCity == true)
            {
                RenderWorkshopLane(lastRenderedSnapshot);
            }
        }

        private void SelectWorkshopRecipe(string label)
        {
            var index = workshopRecipeSelectField?.choices?.IndexOf(label) ?? -1;
            if (index < 0 || index >= workshopRecipeChoiceSnapshots.Count)
            {
                return;
            }

            var recipe = workshopRecipeChoiceSnapshots[index];
            selectedWorkshopRecipeId = recipe?.RecipeId ?? string.Empty;
            if (lastRenderedSnapshot?.HasCity == true)
            {
                RenderWorkshopLane(lastRenderedSnapshot);
            }
        }

        private void RenderWorkshopRecipePicker(IReadOnlyList<WorkshopRecipeSnapshot> recipes, bool isBlackMarket, ResourcePresentationSnapshot labels)
        {
            var catalog = recipes?.Where(r => r != null && !string.IsNullOrWhiteSpace(r.RecipeId)).ToList() ?? new List<WorkshopRecipeSnapshot>();
            if (workshopRecipePicker != null)
            {
                workshopRecipePicker.style.display = catalog.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
            }

            workshopSlotChoiceKeys.Clear();
            var slotLabels = new List<string>();
            workshopSlotChoiceKeys.Add("all");
            slotLabels.Add("All slots");

            foreach (var slot in BuildWorkshopSlotChoices(catalog))
            {
                if (string.IsNullOrWhiteSpace(slot) || workshopSlotChoiceKeys.Any(key => string.Equals(key, slot, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                workshopSlotChoiceKeys.Add(slot);
                slotLabels.Add(BuildWorkshopSlotLabel(slot));
            }

            if (!workshopSlotChoiceKeys.Any(key => string.Equals(key, selectedWorkshopSlotKey, StringComparison.OrdinalIgnoreCase)))
            {
                selectedWorkshopSlotKey = "all";
                selectedWorkshopRecipeId = string.Empty;
            }

            var selectedSlotIndex = Math.Max(0, workshopSlotChoiceKeys.FindIndex(key => string.Equals(key, selectedWorkshopSlotKey, StringComparison.OrdinalIgnoreCase)));
            if (workshopSlotSelectField != null)
            {
                workshopSlotSelectField.choices = slotLabels;
                workshopSlotSelectField.SetValueWithoutNotify(slotLabels.Count > 0 && selectedSlotIndex < slotLabels.Count ? slotLabels[selectedSlotIndex] : string.Empty);
                workshopSlotSelectField.SetEnabled(slotLabels.Count > 1);
            }

            var filtered = FilterWorkshopRecipesBySelectedSlot(catalog);
            workshopRecipeChoiceSnapshots.Clear();
            workshopRecipeChoiceSnapshots.AddRange(filtered);
            var recipeLabels = filtered.Select(BuildWorkshopRecipeChoiceLabel).ToList();

            if (filtered.Count == 0)
            {
                selectedWorkshopRecipeId = string.Empty;
            }
            else if (string.IsNullOrWhiteSpace(selectedWorkshopRecipeId) || !filtered.Any(r => string.Equals(r.RecipeId, selectedWorkshopRecipeId, StringComparison.OrdinalIgnoreCase)))
            {
                selectedWorkshopRecipeId = filtered[0].RecipeId;
            }

            var selectedRecipeIndex = filtered.FindIndex(r => string.Equals(r.RecipeId, selectedWorkshopRecipeId, StringComparison.OrdinalIgnoreCase));
            var selectedRecipe = selectedRecipeIndex >= 0 ? filtered[selectedRecipeIndex] : null;
            if (workshopRecipeSelectField != null)
            {
                workshopRecipeSelectField.choices = recipeLabels;
                workshopRecipeSelectField.SetValueWithoutNotify(selectedRecipeIndex >= 0 && selectedRecipeIndex < recipeLabels.Count ? recipeLabels[selectedRecipeIndex] : string.Empty);
                workshopRecipeSelectField.SetEnabled(recipeLabels.Count > 1);
            }

            if (workshopSelectedRecipeValue != null)
            {
                workshopSelectedRecipeValue.text = selectedRecipe == null
                    ? "No craftable recipe is visible for the selected workshop slot."
                    : $"{selectedRecipe.Name} • {BuildWorkshopSlotLabel(GetWorkshopRecipeSlotKey(selectedRecipe))} • {BuildWorkshopRecipeNote(selectedRecipe, isBlackMarket, labels)}";
            }

            if (workshopCraftSelectedButton != null)
            {
                var busy = summaryState?.IsActionBusy == true && selectedRecipe != null && string.Equals(summaryState.PendingWorkshopRecipeId, selectedRecipe.RecipeId, StringComparison.OrdinalIgnoreCase);
                workshopCraftSelectedButton.text = busy ? "Crafting..." : selectedRecipe == null ? "No recipe selected" : $"Craft {selectedRecipe.Name}";
                workshopCraftSelectedButton.SetEnabled(selectedRecipe != null && summaryState?.IsActionBusy != true && onStartWorkshopCraftRequested != null);
            }
        }

        private List<WorkshopRecipeSnapshot> FilterWorkshopRecipesBySelectedSlot(IReadOnlyList<WorkshopRecipeSnapshot> recipes)
        {
            var catalog = recipes?.Where(r => r != null && !string.IsNullOrWhiteSpace(r.RecipeId)).ToList() ?? new List<WorkshopRecipeSnapshot>();
            if (string.IsNullOrWhiteSpace(selectedWorkshopSlotKey) || string.Equals(selectedWorkshopSlotKey, "all", StringComparison.OrdinalIgnoreCase))
            {
                return catalog;
            }

            return catalog
                .Where(recipe => string.Equals(GetWorkshopRecipeSlotKey(recipe), selectedWorkshopSlotKey, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private string BuildWorkshopCatalogCopy(bool isBlackMarket, int activeJobs, int readyJobs, int workshopTimers, int totalRecipes, int visibleRecipes)
        {
            if (activeJobs > 0)
            {
                return $"{activeJobs} active workshop job(s) and {readyJobs} ready pickup(s) are visible.";
            }

            if (readyJobs > 0)
            {
                return $"{readyJobs} ready pickup(s) visible; collect wiring is now live.";
            }

            if (totalRecipes > 0)
            {
                var slotLabel = BuildWorkshopSlotLabel(selectedWorkshopSlotKey);
                return string.Equals(selectedWorkshopSlotKey, "all", StringComparison.OrdinalIgnoreCase)
                    ? $"Showing {totalRecipes} craft recipe(s). Pick a slot to narrow the catalog."
                    : $"Showing {visibleRecipes}/{totalRecipes} craft recipe(s) for {slotLabel}.";
            }

            return workshopTimers > 0
                ? "Workshop timing is coming through cityTimers even though no job body is active."
                : isBlackMarket
                    ? "No covert workshop queue, timer, or recipe catalog is visible right now."
                    : "No workshop queue, timer, or recipe catalog is visible right now.";
        }

        private static List<string> BuildWorkshopSlotChoices(IReadOnlyList<WorkshopRecipeSnapshot> recipes)
        {
            var preferred = new[] { "head", "chest", "legs", "feet", "hands", "mainhand", "offhand", "ring", "neck", "other" };
            var present = new HashSet<string>((recipes ?? Array.Empty<WorkshopRecipeSnapshot>()).Select(GetWorkshopRecipeSlotKey).Where(slot => !string.IsNullOrWhiteSpace(slot)), StringComparer.OrdinalIgnoreCase);
            return preferred.Where(present.Contains).Concat(present.Where(slot => !preferred.Contains(slot, StringComparer.OrdinalIgnoreCase)).OrderBy(slot => slot, StringComparer.OrdinalIgnoreCase)).ToList();
        }

        private static string BuildWorkshopRecipeChoiceLabel(WorkshopRecipeSnapshot recipe)
        {
            if (recipe == null)
            {
                return "Recipe";
            }

            return $"{recipe.Name} • {BuildWorkshopSlotLabel(GetWorkshopRecipeSlotKey(recipe))}";
        }

        private static string BuildWorkshopSlotLabel(string slot)
        {
            if (string.IsNullOrWhiteSpace(slot) || string.Equals(slot, "all", StringComparison.OrdinalIgnoreCase)) return "All slots";
            if (string.Equals(slot, "ring", StringComparison.OrdinalIgnoreCase)) return "Ring";
            if (string.Equals(slot, "other", StringComparison.OrdinalIgnoreCase)) return "Other";
            return HeroArmorySlotWorkflow.FormatSlotLabel(slot);
        }

        private static string GetWorkshopRecipeSlotKey(WorkshopRecipeSnapshot recipe)
        {
            if (recipe == null)
            {
                return "other";
            }

            var explicitSlot = NormalizeWorkshopSlotKey(recipe.GearSlot);
            if (!string.IsNullOrWhiteSpace(explicitSlot))
            {
                return explicitSlot;
            }

            var text = $"{recipe.OutputItemId} {recipe.RecipeId} {recipe.Name} {recipe.Summary} {recipe.GearFamily} {string.Join(" ", recipe.ResponseTags ?? new List<string>())}".ToLowerInvariant();
            if (text.Contains("mainhand") || text.Contains("main hand") || text.Contains("primary hand") || text.Contains("main-hand") || text.Contains("command standard") || text.Contains("banner")) return "mainhand";
            if (text.Contains("offhand") || text.Contains("off hand") || text.Contains("off-hand") || text.Contains("focus")) return "offhand";
            if (text.Contains("head") || text.Contains("helm") || text.Contains("helmet")) return "head";
            if (text.Contains("chest") || text.Contains("cloak") || text.Contains("robe") || text.Contains("body")) return "chest";
            if (text.Contains("legs") || text.Contains("leg ") || text.Contains("greave") || text.Contains("leggings")) return "legs";
            if (text.Contains("feet") || text.Contains("foot") || text.Contains("boot")) return "feet";
            if (text.Contains("hands") || text.Contains("hand slot") || text.Contains("glove") || text.Contains("satchel")) return "hands";
            if (text.Contains("ring") || text.Contains("signet") || text.Contains("seal")) return "ring";
            if (text.Contains("neck") || text.Contains("charm") || text.Contains("amulet")) return "neck";
            return "other";
        }

        private static string NormalizeWorkshopSlotKey(string slot)
        {
            if (string.IsNullOrWhiteSpace(slot)) return string.Empty;
            var normalized = HeroArmorySlotWorkflow.NormalizeSlot(slot);
            if (string.Equals(normalized, "ring1", StringComparison.OrdinalIgnoreCase) || string.Equals(normalized, "ring2", StringComparison.OrdinalIgnoreCase) || string.Equals(normalized, "ring", StringComparison.OrdinalIgnoreCase)) return "ring";
            if (HeroArmorySlotWorkflow.IsStandardSlot(normalized)) return normalized;
            if (string.Equals(normalized, "main", StringComparison.OrdinalIgnoreCase) || string.Equals(normalized, "mainweapon", StringComparison.OrdinalIgnoreCase)) return "mainhand";
            if (string.Equals(normalized, "off", StringComparison.OrdinalIgnoreCase) || string.Equals(normalized, "offweapon", StringComparison.OrdinalIgnoreCase)) return "offhand";
            return string.Empty;
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
            var buildingCards = BuildBuildingCards(s, isBlackMarket, nowUtc, 4);
            if (buildingCards.Count > 0)
            {
                cards.AddRange(buildingCards);
            }
            else
            {
                cards.Add(new CardView(
                    family: isBlackMarket ? ShadowLaneText.BuildProductionFamily() : "Production",
                    title: isBlackMarket ? ShadowLaneText.BuildProductionTitle() : "Per-tick output",
                    lore: FormatProduction(s.ProductionPerTick, s.ResourceLabels),
                    note: isBlackMarket ? ShadowLaneText.BuildProductionNote(s.ResourceTickTiming, nowUtc) : FormatResourceTickNote(s.ResourceTickTiming, nowUtc)));
            }

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
                .GroupBy(r => NormalizeResearchKey(r.Id, r.Name), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(r => r.FinishesAtUtc.HasValue).First())
                .OrderBy(r => IsResearchReady(r, nowUtc) ? 0 : 1)
                .ThenBy(r => r.FinishesAtUtc ?? DateTime.MaxValue)
                .ThenBy(r => FirstNonBlank(r.Name, r.Id))
                .ToList();
        }

        private static List<TechOptionSnapshot> SelectAvailableResearchOptions(ShellSummarySnapshot s, IReadOnlyList<ResearchSnapshot> activeResearches)
        {
            var activeKeys = new HashSet<string>(
                (activeResearches ?? Array.Empty<ResearchSnapshot>())
                    .Where(r => r != null)
                    .Select(r => NormalizeResearchKey(r.Id, r.Name))
                    .Where(key => !string.IsNullOrWhiteSpace(key)),
                StringComparer.OrdinalIgnoreCase);

            return (s?.AvailableTechs ?? new List<TechOptionSnapshot>())
                .Where(tech => tech != null && !activeKeys.Contains(NormalizeResearchKey(tech.Id, tech.Name)))
                .ToList();
        }

        private static string NormalizeResearchKey(string id, string name)
        {
            var raw = StripResearchPrefix(FirstNonBlank(id, name));
            if (string.IsNullOrWhiteSpace(raw)) raw = StripResearchPrefix(name);
            var chars = raw
                .ToLowerInvariant()
                .Where(char.IsLetterOrDigit)
                .ToArray();
            return chars.Length > 0 ? new string(chars) : string.Empty;
        }

        private static string StripResearchPrefix(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var value = raw.Trim();
            if (value.StartsWith("research:", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring("research:".Length).Trim();
            }
            if (value.StartsWith("Research ", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring("Research ".Length).Trim();
            }
            return value;
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

        private static string BuildResearchCardsCopy(ShellSummarySnapshot s, bool isBlackMarket, IReadOnlyList<ResearchSnapshot> activeResearches, IReadOnlyList<TechOptionSnapshot> visibleAvailableTechs, DateTime nowUtc)
        {
            var activeCount = activeResearches?.Count ?? 0;
            var readyCount = activeResearches?.Count(r => IsResearchReady(r, nowUtc)) ?? 0;
            if (isBlackMarket)
            {
                if (activeCount > 0)
                {
                    return $"Showing {activeCount} active shadow-book/front research item(s), {readyCount} ready, and {visibleAvailableTechs.Count} available option(s).";
                }

                return ShadowLaneText.DescribeResearchCardsCopy(s);
            }

            if (activeCount > 0)
            {
                return $"Showing {activeCount} active research item(s), {readyCount} ready, and {visibleAvailableTechs.Count} available tech option(s).";
            }

            return visibleAvailableTechs.Count > 0
                ? $"Showing {visibleAvailableTechs.Count} available tech option(s) ready."
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
            var suggested = GetSuggestedTech(summaryState.Snapshot, SelectActiveResearches(summaryState.Snapshot, DateTime.UtcNow));
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

        private void TriggerConstructBuilding(string kind)
        {
            if (summaryState.IsActionBusy || onConstructBuildingRequested == null || string.IsNullOrWhiteSpace(kind))
            {
                return;
            }

            _ = onConstructBuildingRequested.Invoke(kind.Trim());
        }

        private void TriggerUpgradeBuilding(string buildingId)
        {
            if (summaryState.IsActionBusy || onUpgradeBuildingRequested == null || string.IsNullOrWhiteSpace(buildingId))
            {
                return;
            }

            _ = onUpgradeBuildingRequested.Invoke(buildingId.Trim());
        }

        private void TriggerSwitchBuildingRouting(string buildingId, string routingPreference)
        {
            if (summaryState.IsActionBusy || onSwitchBuildingRoutingRequested == null || string.IsNullOrWhiteSpace(buildingId) || string.IsNullOrWhiteSpace(routingPreference))
            {
                return;
            }

            _ = onSwitchBuildingRoutingRequested.Invoke(buildingId.Trim(), routingPreference.Trim());
        }

        private void TriggerDestroyBuilding(string buildingId)
        {
            if (summaryState.IsActionBusy || onDestroyBuildingRequested == null || string.IsNullOrWhiteSpace(buildingId))
            {
                return;
            }

            _ = onDestroyBuildingRequested.Invoke(buildingId.Trim());
        }

        private void TriggerRemodelBuilding(string buildingId, string targetKind)
        {
            if (summaryState.IsActionBusy || onRemodelBuildingRequested == null || string.IsNullOrWhiteSpace(buildingId) || string.IsNullOrWhiteSpace(targetKind))
            {
                return;
            }

            _ = onRemodelBuildingRequested.Invoke(buildingId.Trim(), targetKind.Trim());
        }

        private void TriggerCancelActiveBuild(string activeBuildId)
        {
            if (summaryState.IsActionBusy || onCancelActiveBuildRequested == null)
            {
                return;
            }

            _ = onCancelActiveBuildRequested.Invoke(activeBuildId?.Trim() ?? string.Empty);
        }

        private static TechOptionSnapshot GetSuggestedTech(ShellSummarySnapshot s, IReadOnlyList<ResearchSnapshot> activeResearches = null)
        {
            return SelectAvailableResearchOptions(s, activeResearches ?? SelectActiveResearches(s, DateTime.UtcNow))
                .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t?.Id));
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
            return GetWorkshopJobTitle(job, null);
        }

        private static string GetWorkshopJobTitle(WorkshopJobSnapshot job, IReadOnlyList<WorkshopRecipeSnapshot> recipes)
        {
            var outputName = CleanWorkshopDisplayName(job?.OutputName);
            if (!string.IsNullOrWhiteSpace(outputName))
            {
                return outputName;
            }

            var recipe = ResolveWorkshopRecipe(job, recipes);
            var recipeName = CleanWorkshopDisplayName(recipe?.Name);
            if (!string.IsNullOrWhiteSpace(recipeName))
            {
                return recipeName;
            }

            var recipeId = CleanWorkshopDisplayName(job?.RecipeId);
            if (!string.IsNullOrWhiteSpace(recipeId))
            {
                return recipeId;
            }

            var outputItemId = CleanWorkshopDisplayName(job?.OutputItemId);
            if (!string.IsNullOrWhiteSpace(outputItemId))
            {
                return outputItemId;
            }

            var attachmentKind = CleanWorkshopDisplayName(job?.AttachmentKind);
            if (!string.IsNullOrWhiteSpace(attachmentKind))
            {
                return attachmentKind;
            }

            return "Workshop job";
        }

        private static string BuildWorkshopReadyPickupNote(WorkshopJobSnapshot job, IReadOnlyList<WorkshopRecipeSnapshot> recipes)
        {
            if (job?.Id == "job")
            {
                return "Ready workshop item surfaced without a stable job id.";
            }

            return $"Ready to collect: {GetWorkshopJobTitle(job, recipes)} can now be collected into storage.";
        }

        private static string GetWorkshopTimerTitle(CityTimerEntrySnapshot timer, IReadOnlyList<WorkshopRecipeSnapshot> recipes)
        {
            var raw = FirstNonBlank(timer?.Label, timer?.Detail, timer?.Id, "Workshop timer");
            var payloadName = ExtractWorkshopTimerPayloadName(raw);
            var resolvedRecipeName = ResolveWorkshopRecipeDisplayName(payloadName, recipes);
            if (!string.IsNullOrWhiteSpace(resolvedRecipeName))
            {
                return resolvedRecipeName;
            }

            var cleanedPayloadName = CleanWorkshopDisplayName(payloadName);
            if (!string.IsNullOrWhiteSpace(cleanedPayloadName)
                && !cleanedPayloadName.Equals("Workshop", StringComparison.OrdinalIgnoreCase)
                && !cleanedPayloadName.Equals("Timer", StringComparison.OrdinalIgnoreCase))
            {
                return cleanedPayloadName;
            }

            return "Workshop timer";
        }

        private static string ExtractWorkshopTimerPayloadName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            var value = raw.Trim();
            foreach (var prefix in new[]
            {
                "Workshop timer",
                "Workshop job",
                "Workshop",
                "Crafting",
                "Craft",
                "Timer",
            })
            {
                if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    value = value.Substring(prefix.Length).Trim(' ', ':', '-', '—');
                    break;
                }
            }

            return value;
        }

        private static string ResolveWorkshopRecipeDisplayName(string raw, IReadOnlyList<WorkshopRecipeSnapshot> recipes)
        {
            if (string.IsNullOrWhiteSpace(raw) || recipes == null || recipes.Count == 0)
            {
                return string.Empty;
            }

            var normalizedRaw = NormalizeIdentity(raw);
            if (string.IsNullOrWhiteSpace(normalizedRaw))
            {
                return string.Empty;
            }

            var recipe = recipes.FirstOrDefault(candidate => WorkshopRecipeMatchesIdentity(candidate, normalizedRaw));
            return CleanWorkshopDisplayName(recipe?.Name);
        }

        private static bool WorkshopRecipeMatchesIdentity(WorkshopRecipeSnapshot recipe, string normalizedRaw)
        {
            if (recipe == null || string.IsNullOrWhiteSpace(normalizedRaw))
            {
                return false;
            }

            foreach (var candidate in new[] { recipe.RecipeId, recipe.OutputItemId, recipe.Name })
            {
                var normalizedCandidate = NormalizeIdentity(candidate);
                if (string.IsNullOrWhiteSpace(normalizedCandidate))
                {
                    continue;
                }

                if (normalizedCandidate.Equals(normalizedRaw, StringComparison.OrdinalIgnoreCase)
                    || normalizedCandidate.IndexOf(normalizedRaw, StringComparison.OrdinalIgnoreCase) >= 0
                    || normalizedRaw.IndexOf(normalizedCandidate, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static WorkshopRecipeSnapshot ResolveWorkshopRecipe(WorkshopJobSnapshot job, IReadOnlyList<WorkshopRecipeSnapshot> recipes)
        {
            if (job == null || recipes == null || recipes.Count == 0)
            {
                return null;
            }

            var recipeId = job.RecipeId?.Trim() ?? string.Empty;
            var outputItemId = job.OutputItemId?.Trim() ?? string.Empty;
            return recipes.FirstOrDefault(recipe => recipe != null
                    && !string.IsNullOrWhiteSpace(recipeId)
                    && string.Equals(recipe.RecipeId, recipeId, StringComparison.OrdinalIgnoreCase))
                ?? recipes.FirstOrDefault(recipe => recipe != null
                    && !string.IsNullOrWhiteSpace(outputItemId)
                    && string.Equals(recipe.OutputItemId, outputItemId, StringComparison.OrdinalIgnoreCase));
        }

        private static string CleanWorkshopDisplayName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            var value = raw.Trim();
            if (LooksLikeBackendWorkshopId(value))
            {
                return HumanizeKey(value);
            }

            return value;
        }

        private static bool LooksLikeBackendWorkshopId(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return false;
            var value = raw.Trim();
            return value.StartsWith("workshop_", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("recipe_", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("item_", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("gear_", StringComparison.OrdinalIgnoreCase)
                || value.IndexOf('_') >= 0;
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
            var actionStatus = summaryState?.ActionStatus;
            if (!string.IsNullOrWhiteSpace(actionStatus) && !IsResearchStartActionStatus(actionStatus))
            {
                return actionStatus;
            }

            var nowUtc = DateTime.UtcNow;
            var researchNote = BuildResearchDeskNote(s, summaryState, isBlackMarket, nowUtc);
            if (!string.IsNullOrWhiteSpace(researchNote))
            {
                return researchNote;
            }

            if (isBlackMarket)
            {
                return ShadowLaneText.BuildDeskNote(s, string.Empty);
            }

            return $"Desk state: {s.AvailableTechs.Count} tech option(s), {s.WorkshopJobs.Count} workshop job(s), and {s.CityTimers.Count} live city timer(s).";
        }

        private static string BuildResearchDeskNote(ShellSummarySnapshot s, SummaryState summaryState, bool isBlackMarket, DateTime nowUtc)
        {
            var activeResearches = SelectActiveResearches(s, nowUtc);
            if (activeResearches.Count > 0)
            {
                var overview = FormatResearchOverview(activeResearches, isBlackMarket, nowUtc);
                return isBlackMarket
                    ? $"Shadow-book active: {overview}."
                    : $"Research active: {overview}.";
            }

            if (summaryState?.HasRecentResearchStartGuard(nowUtc) == true)
            {
                return isBlackMarket
                    ? $"Shadow-book accepted: {HumanizeKey(summaryState.RecentStartedResearchTechId)}; waiting for canonical ETA."
                    : $"Research accepted: {HumanizeKey(summaryState.RecentStartedResearchTechId)}; waiting for canonical ETA.";
            }

            if (summaryState?.HasRecentResearchCompletionNotice(nowUtc) == true)
            {
                return isBlackMarket
                    ? $"Shadow-book completed: {HumanizeKey(summaryState.RecentCompletedResearchTechId)}; desk returned to the next real unlock."
                    : $"Research completed: {HumanizeKey(summaryState.RecentCompletedResearchTechId)}; desk returned to the next real unlock.";
            }

            return string.Empty;
        }

        private static bool IsResearchStartActionStatus(string actionStatus)
        {
            if (string.IsNullOrWhiteSpace(actionStatus))
            {
                return false;
            }

            var trimmed = actionStatus.Trim();
            return trimmed.StartsWith("Research started:", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("Starting research:", StringComparison.OrdinalIgnoreCase)
                || string.Equals(trimmed, "Starting research...", StringComparison.OrdinalIgnoreCase);
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
            var timed = buildings.Count(HasBuildingTimingAnchor) + buildTimers.Count(t => !t.FinishesAtUtc.HasValue || t.FinishesAtUtc.Value > nowUtc);
            var standing = buildings.Count(b => !IsBuildingReady(b, nowUtc) && !HasBuildingTimingAnchor(b));

            if (buildings.Count > 0 || buildTimers.Count > 0)
            {
                var label = isBlackMarket ? "front" : "building";
                var timerLabel = isBlackMarket ? "front timer" : "build timer";
                var standingLabel = isBlackMarket ? "front live" : "standing";
                return $"{buildings.Count} {label} card(s) • {buildTimers.Count} {timerLabel}(s) • {standing} {standingLabel} • {timed} timed • {ready} ready";
            }

            var liveTimers = s.CityTimers?.Count ?? 0;
            var nextResourceTick = ResolveRollingResourceTickAtUtc(s.ResourceTickTiming, nowUtc);
            if (nextResourceTick.HasValue)
            {
                return $"Next tick in {FormatRemaining(nextResourceTick.Value - nowUtc)} • no {(isBlackMarket ? "front" : "building")} payload yet.";
            }

            return liveTimers > 0 ? $"{liveTimers} live timer(s) visible; no {(isBlackMarket ? "front" : "building")} cards surfaced." : $"No {(isBlackMarket ? "front" : "building")} payload surfaced.";
        }

        private List<CardView> BuildBuildingCards(ShellSummarySnapshot s, bool isBlackMarket, DateTime nowUtc, int maxCards)
        {
            var cards = new List<CardView>();
            var budget = Math.Max(0, maxCards);
            if (budget <= 0)
            {
                return cards;
            }

            var buildings = SelectLaneBuildings(s, isBlackMarket);
            var timers = SortBuildTimers(SelectBuildTimers(s, isBlackMarket), nowUtc);
            var activeBuilding = buildings.FirstOrDefault(b => HasBuildingTimingAnchor(b));
            var activeTimer = timers.FirstOrDefault();
            var hasActiveBuildWork = (activeBuilding != null && !IsBuildingReady(activeBuilding, nowUtc))
                || (activeTimer != null && (!activeTimer.FinishesAtUtc.HasValue || activeTimer.FinishesAtUtc.Value > nowUtc));

            cards.Add(BuildBuildingInventoryCard(s, buildings, timers, isBlackMarket, nowUtc));

            if (activeBuilding != null && cards.Count < budget)
            {
                cards.Add(BuildExistingBuildingCard(s, activeBuilding, isBlackMarket, nowUtc, allowUpgrade: false, allBuildings: buildings));
            }
            else if (activeTimer != null && cards.Count < budget)
            {
                cards.Add(BuildBuildTimerCard(activeTimer, isBlackMarket, nowUtc));
            }

            var manageableBuildings = SelectManageableBuildings(buildings, nowUtc);
            BuildingSnapshot selectedBuilding = null;
            if (manageableBuildings.Count > 0)
            {
                selectedBuilding = ResolveSelectedManagedBuilding(manageableBuildings, isBlackMarket);
                if (cards.Count < budget)
                {
                    cards.Add(BuildBuildingSelectorCard(manageableBuildings, selectedBuilding, isBlackMarket));
                }

                if (selectedBuilding != null && cards.Count < budget)
                {
                    cards.Add(BuildExistingBuildingCard(s, selectedBuilding, isBlackMarket, nowUtc, allowUpgrade: !hasActiveBuildWork, allBuildings: buildings));
                }
            }

            var visibleBuildOptions = SelectCurrentlyBuildableOptions(s, isBlackMarket, buildings);
            if (!hasActiveBuildWork)
            {
                if (visibleBuildOptions.Count > 0 && cards.Count < budget)
                {
                    var selectedBuildOption = ResolveSelectedBuildOption(visibleBuildOptions, isBlackMarket);
                    cards.Add(BuildConstructOptionCard(selectedBuildOption, visibleBuildOptions, isBlackMarket, hasActiveBuildWork));
                }
                else if (visibleBuildOptions.Count == 0 && cards.Count < budget)
                {
                    cards.Add(BuildNoAvailableBuildOptionsCard(s, isBlackMarket, buildings));
                }
            }

            return cards;
        }

        private CardView BuildBuildingInventoryCard(ShellSummarySnapshot s, List<BuildingSnapshot> buildings, List<CityTimerEntrySnapshot> timers, bool isBlackMarket, DateTime nowUtc)
        {
            var label = isBlackMarket ? "front" : "building";
            var counts = BuildBuildingTypeCounts(buildings);
            var capacity = FormatBuildingCapacity(s, isBlackMarket, buildings.Count);
            var typeSummary = counts.Count > 0 ? FormatBuildingTypeCounts(counts) : $"Pick a {label} type below to start the first real project.";

            return new CardView(
                family: isBlackMarket ? "Front inventory" : "Building inventory",
                title: buildings.Count > 0
                    ? $"{buildings.Count} {label}{(buildings.Count == 1 ? string.Empty : "s")} across {Math.Max(1, counts.Count)} type{(counts.Count == 1 ? string.Empty : "s")}"
                    : $"No {label}s built yet",
                lore: string.IsNullOrWhiteSpace(capacity) ? typeSummary : $"{capacity} • {typeSummary}",
                note: BuildBuildingInventoryNote(s, buildings, timers, isBlackMarket, nowUtc),
                buttonText: null,
                buttonEnabled: false,
                onClick: null);
        }

        private CardView BuildBuildingSelectorCard(List<BuildingSnapshot> manageableBuildings, BuildingSnapshot selectedBuilding, bool isBlackMarket)
        {
            var noun = isBlackMarket ? "front" : "building";
            var choices = manageableBuildings.Select(b => FormatBuildingSelectorLabel(b, isBlackMarket)).ToList();
            var selectedIndex = selectedBuilding == null ? 0 : Math.Max(0, manageableBuildings.FindIndex(b => BuildingSelectorKeysMatch(b, selectedBuilding)));
            var title = selectedBuilding == null ? $"Choose {noun}" : FormatBuildingTitle(selectedBuilding, isBlackMarket);
            var selectedId = selectedBuilding == null ? string.Empty : FirstNonBlank(GetBuildingActionId(selectedBuilding), GetBuildingSelectorKey(selectedBuilding));
            var selectedLabel = selectedBuilding == null ? string.Empty : FormatBuildingSelectorLabel(selectedBuilding, isBlackMarket);

            return new CardView(
                family: isBlackMarket ? "Front selector" : "Building selector",
                title: title,
                lore: $"{manageableBuildings.Count} completed {noun}{(manageableBuildings.Count == 1 ? string.Empty : "s")} can be managed from this selector.",
                note: string.IsNullOrWhiteSpace(selectedId)
                    ? $"Pick which {noun} to inspect. This entry has no stable backend id, so mutation buttons stay disabled on its management card."
                    : $"Selected {noun}: {selectedLabel}. Backend id truth stays internal for upgrade, remodel, or destroy actions.",
                selectorLabel: isBlackMarket ? "Manage front" : "Manage building",
                selectorOptions: choices,
                selectorIndex: selectedIndex,
                selectorOnChange: index => SelectManagedBuilding(manageableBuildings, isBlackMarket, index));
        }

        private void SelectManagedBuilding(List<BuildingSnapshot> manageableBuildings, bool isBlackMarket, int index)
        {
            if (manageableBuildings == null || manageableBuildings.Count == 0)
            {
                SetSelectedManagedBuildingId(isBlackMarket, string.Empty);
                return;
            }

            var safeIndex = Math.Max(0, Math.Min(index, manageableBuildings.Count - 1));
            SetSelectedManagedBuildingId(isBlackMarket, GetBuildingSelectorKey(manageableBuildings[safeIndex]));
            if (lastRenderedSnapshot != null)
            {
                Render(lastRenderedSnapshot, summaryState);
            }
        }

        private BuildingSnapshot ResolveSelectedManagedBuilding(List<BuildingSnapshot> manageableBuildings, bool isBlackMarket)
        {
            if (manageableBuildings == null || manageableBuildings.Count == 0)
            {
                SetSelectedManagedBuildingId(isBlackMarket, string.Empty);
                return null;
            }

            var selectedId = GetSelectedManagedBuildingId(isBlackMarket);
            var selected = manageableBuildings.FirstOrDefault(b => string.Equals(GetBuildingSelectorKey(b), selectedId, StringComparison.OrdinalIgnoreCase));
            if (selected == null)
            {
                selected = manageableBuildings[0];
                SetSelectedManagedBuildingId(isBlackMarket, GetBuildingSelectorKey(selected));
            }

            return selected;
        }

        private string GetSelectedManagedBuildingId(bool isBlackMarket) => isBlackMarket ? selectedBlackMarketBuildingId : selectedCityBuildingId;

        private void SetSelectedManagedBuildingId(bool isBlackMarket, string selectedId)
        {
            if (isBlackMarket)
            {
                selectedBlackMarketBuildingId = selectedId ?? string.Empty;
            }
            else
            {
                selectedCityBuildingId = selectedId ?? string.Empty;
            }
        }

        private CardView BuildExistingBuildingCard(ShellSummarySnapshot s, BuildingSnapshot building, bool isBlackMarket, DateTime nowUtc, bool allowUpgrade, List<BuildingSnapshot> allBuildings)
        {
            var buildingId = GetBuildingActionId(building);
            var activeBuildId = FirstNonBlank(building?.Id, buildingId);
            var hasTiming = HasBuildingTimingAnchor(building);
            var readyBuild = IsBuildingReady(building, nowUtc);
            var canRefreshCompletedBuild = readyBuild
                && hasTiming
                && !summaryState.IsActionBusy
                && onRefreshDeskRequested != null;
            var pendingUpgrade = summaryState.IsActionBusy && string.Equals(summaryState.PendingBuildingId, buildingId, StringComparison.OrdinalIgnoreCase);
            var canUpgrade = allowUpgrade
                && !summaryState.IsActionBusy
                && !string.IsNullOrWhiteSpace(buildingId)
                && onUpgradeBuildingRequested != null
                && !readyBuild
                && !hasTiming;

            var remodelTarget = SelectRemodelTarget(s, isBlackMarket, building, allBuildings);
            var canRemodel = !summaryState.IsActionBusy
                && !hasTiming
                && onRemodelBuildingRequested != null
                && !string.IsNullOrWhiteSpace(buildingId)
                && remodelTarget.HasValue;
            var pendingRemodelConfirm = canRemodel
                && summaryState.HasPendingBuildingConfirm("remodel", buildingId, remodelTarget.Value.Kind);
            var canDestroy = !summaryState.IsActionBusy
                && !hasTiming
                && onDestroyBuildingRequested != null
                && !string.IsNullOrWhiteSpace(buildingId);
            var pendingDestroyConfirm = canDestroy
                && summaryState.HasPendingBuildingConfirm("destroy", buildingId);
            var canCancel = !summaryState.IsActionBusy
                && hasTiming
                && !readyBuild
                && onCancelActiveBuildRequested != null;
            var pendingCancelConfirm = canCancel
                && summaryState.HasPendingBuildingConfirm("cancel_build", activeBuildId: activeBuildId);
            var routingValues = BuildBuildingRoutingPreferenceValues();
            var routingLabels = BuildBuildingRoutingPreferenceLabels();
            var currentRouting = NormalizeBuildingRoutingPreference(building?.RoutingPreference);
            var routingIndex = Math.Max(0, routingValues.FindIndex(value => string.Equals(value, currentRouting, StringComparison.OrdinalIgnoreCase)));
            if (routingIndex >= routingValues.Count)
            {
                routingIndex = 0;
            }

            var routingPending = summaryState.IsActionBusy
                && string.Equals(summaryState.PendingBuildingId, buildingId, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(summaryState.PendingBuildingRoutingPreference);
            var canSwitchRouting = !summaryState.IsActionBusy
                && !hasTiming
                && onSwitchBuildingRoutingRequested != null
                && !string.IsNullOrWhiteSpace(buildingId);
            var managementNote = BuildBuildingManagementNote(
                BuildBuildingRoutingManagementNote(BuildBuildingNote(building, isBlackMarket, nowUtc), currentRouting, routingPending ? summaryState.PendingBuildingRoutingPreference : string.Empty, isBlackMarket),
                canDestroy || canRemodel,
                canCancel);

            return new CardView(
                family: isBlackMarket ? "Operator front" : "Building",
                title: FormatBuildingTitle(building, isBlackMarket),
                lore: BuildBuildingLore(building, isBlackMarket, nowUtc),
                note: managementNote,
                buttonText: canRefreshCompletedBuild
                    ? BuildBuildingCompletionRefreshButtonText(isBlackMarket)
                    : canUpgrade || pendingUpgrade
                        ? pendingUpgrade ? "Upgrading..." : "Upgrade"
                        : BuildBuildingStatusButtonText(building, isBlackMarket, nowUtc),
                buttonEnabled: canRefreshCompletedBuild || canUpgrade,
                onClick: canRefreshCompletedBuild ? TriggerRefreshDesk : canUpgrade ? () => TriggerUpgradeBuilding(buildingId) : null,
                secondaryButtonText: pendingRemodelConfirm
                    ? $"Confirm remodel → {remodelTarget.Value.Label}"
                    : canRemodel ? $"Remodel → {remodelTarget.Value.Label}" : canCancel ? (pendingCancelConfirm ? "Confirm cancel" : "Cancel project") : null,
                secondaryButtonEnabled: canRemodel || canCancel,
                secondaryOnClick: canRemodel
                    ? () => TriggerRemodelBuilding(buildingId, remodelTarget.Value.Kind)
                    : canCancel ? () => TriggerCancelActiveBuild(activeBuildId) : null,
                tertiaryButtonText: canDestroy ? (pendingDestroyConfirm ? "Confirm destroy" : "Destroy") : null,
                tertiaryButtonEnabled: canDestroy,
                tertiaryOnClick: canDestroy ? () => TriggerDestroyBuilding(buildingId) : null,
                selectorLabel: BuildBuildingRoutingSelectorLabel(isBlackMarket, routingPending ? summaryState.PendingBuildingRoutingPreference : string.Empty),
                selectorOptions: routingLabels,
                selectorIndex: routingIndex,
                selectorOnChange: canSwitchRouting
                    ? index =>
                    {
                        var safeIndex = Math.Max(0, Math.Min(index, routingValues.Count - 1));
                        var nextRouting = routingValues[safeIndex];
                        if (!string.Equals(nextRouting, currentRouting, StringComparison.OrdinalIgnoreCase))
                        {
                            TriggerSwitchBuildingRouting(buildingId, nextRouting);
                        }
                    }
                    : null);
        }

        private CardView BuildBuildTimerCard(CityTimerEntrySnapshot timer, bool isBlackMarket, DateTime nowUtc)
        {
            var activeBuildId = FirstNonBlank(timer?.Id);
            var readyTimer = IsBuildTimerReady(timer, nowUtc);
            var canRefreshCompletedTimer = readyTimer
                && !summaryState.IsActionBusy
                && onRefreshDeskRequested != null;
            var canCancel = !summaryState.IsActionBusy
                && onCancelActiveBuildRequested != null
                && !string.IsNullOrWhiteSpace(activeBuildId)
                && !readyTimer;
            var pendingCancelConfirm = canCancel
                && summaryState.HasPendingBuildingConfirm("cancel_build", activeBuildId: activeBuildId);

            return new CardView(
                family: isBlackMarket ? "Front timer" : "Build timer",
                title: isBlackMarket ? NormalizeBlackMarketTimerLabel(timer.Label, timer.Category) : FirstNonBlank(timer.Label, HumanizeCategory(timer.Category)),
                lore: readyTimer ? BuildBuildingCompletionStatusText(isBlackMarket) : FormatTimerState(timer.Status, timer.FinishesAtUtc, nowUtc),
                note: readyTimer
                    ? BuildBuildingCompletionRefreshNote(isBlackMarket)
                    : FirstNonBlank(timer.Detail, isBlackMarket ? "Operator-front timing is visible from cityTimers." : "Construction timing is visible from cityTimers."),
                buttonText: readyTimer ? BuildBuildingCompletionRefreshButtonText(isBlackMarket) : "Timed",
                buttonEnabled: canRefreshCompletedTimer,
                onClick: canRefreshCompletedTimer ? TriggerRefreshDesk : null,
                secondaryButtonText: canCancel ? (pendingCancelConfirm ? "Confirm cancel" : "Cancel project") : null,
                secondaryButtonEnabled: canCancel,
                secondaryOnClick: canCancel ? () => TriggerCancelActiveBuild(activeBuildId) : null);
        }

        private CardView BuildConstructOptionCard(BuildingBuildOption option, IReadOnlyList<BuildingBuildOption> availableOptions, bool isBlackMarket, bool hasActiveBuildWork)
        {
            var safeOptions = (availableOptions ?? Array.Empty<BuildingBuildOption>())
                .Where(candidate => !string.IsNullOrWhiteSpace(candidate.Kind))
                .ToList();
            var selectedIndex = Math.Max(0, safeOptions.FindIndex(candidate => string.Equals(candidate.Kind, option.Kind, StringComparison.OrdinalIgnoreCase)));
            if (selectedIndex >= safeOptions.Count)
            {
                selectedIndex = 0;
            }

            var pendingConstruct = summaryState.IsActionBusy && string.Equals(summaryState.PendingBuildingKind, option.Kind, StringComparison.OrdinalIgnoreCase);
            var canConstruct = !summaryState.IsActionBusy && !hasActiveBuildWork && onConstructBuildingRequested != null;
            var noun = isBlackMarket ? "front" : "building";
            var choices = safeOptions.Select(FormatBuildOptionChoiceLabel).ToList();
            var choiceCopy = safeOptions.Count > 1
                ? $"Choose from {safeOptions.Count} unlocked affordable {noun} choices before starting work."
                : $"Only one unlocked affordable {noun} choice is available right now.";
            return new CardView(
                family: isBlackMarket ? "Front build choice" : "Build choice",
                title: option.Label,
                lore: option.Summary,
                note: $"Cost {FormatBuildOptionCost(option)}. {choiceCopy} Raw backend kind ids stay internal.",
                buttonText: pendingConstruct ? "Starting..." : hasActiveBuildWork ? (isBlackMarket ? "Front active" : "Build active") : isBlackMarket ? $"Open {option.Label}" : $"Build {option.Label}",
                buttonEnabled: canConstruct,
                onClick: canConstruct ? () => TriggerConstructBuilding(option.Kind) : null,
                selectorLabel: isBlackMarket ? "Choose front to open" : "Choose building to build",
                selectorOptions: choices,
                selectorIndex: selectedIndex,
                selectorOnChange: safeOptions.Count > 1 ? index => SelectBuildOption(safeOptions, isBlackMarket, index) : null);
        }

        private static CardView BuildNoAvailableBuildOptionsCard(ShellSummarySnapshot s, bool isBlackMarket, List<BuildingSnapshot> buildings)
        {
            var all = SelectBuildOptions(isBlackMarket, buildings);
            var locked = all.Count(option => !IsBuildOptionUnlocked(s, option));
            var unlocked = all.Where(option => IsBuildOptionUnlocked(s, option)).ToList();
            var costBlocked = unlocked.Count(option => !CanAffordBuildOption(s?.Resources, option));
            var noun = isBlackMarket ? "front" : "building";
            var blockerParts = new List<string>();
            if (locked > 0) blockerParts.Add($"{locked} locked by research");
            if (costBlocked > 0) blockerParts.Add($"{costBlocked} blocked by visible resources");
            if (blockerParts.Count == 0) blockerParts.Add("backend validation has no current target exposed");

            return new CardView(
                family: isBlackMarket ? "Front choices" : "Build choices",
                title: isBlackMarket ? "No fronts ready to open" : "No buildings ready to build",
                lore: string.Join(" • ", blockerParts),
                note: $"Unavailable {noun} choices are hidden instead of being offered as fake action buttons. Destroy/remodel uses explicit backend confirmation; no refund is issued in v1.",
                buttonText: "No valid build target",
                buttonEnabled: false);
        }

        private static List<BuildingBuildOption> SelectBuildOptions(bool isBlackMarket, List<BuildingSnapshot> buildings)
        {
            var options = isBlackMarket ? BlackMarketBuildOptions : CityBuildOptions;
            var counts = BuildBuildingTypeCounts(buildings);
            return options
                .OrderBy(option => counts.TryGetValue(NormalizeIdentity(option.Kind), out var count) ? count : 0)
                .ThenBy(option => option.SortOrder)
                .ToList();
        }

        private static BuildingBuildOption? SelectRemodelTarget(ShellSummarySnapshot s, bool isBlackMarket, BuildingSnapshot building, List<BuildingSnapshot> buildings)
        {
            var currentKind = NormalizeIdentity(FirstNonBlank(building?.Type, building?.BuildingId, building?.Name));
            foreach (var option in SelectCurrentlyBuildableOptions(s, isBlackMarket, buildings))
            {
                if (!string.Equals(NormalizeIdentity(option.Kind), currentKind, StringComparison.OrdinalIgnoreCase))
                {
                    return option;
                }
            }

            return null;
        }

        private static List<BuildingBuildOption> SelectCurrentlyBuildableOptions(ShellSummarySnapshot s, bool isBlackMarket, List<BuildingSnapshot> buildings)
        {
            return SelectBuildOptions(isBlackMarket, buildings)
                .Where(option => IsBuildOptionUnlocked(s, option))
                .Where(option => CanAffordBuildOption(s?.Resources, option))
                .ToList();
        }

        private BuildingBuildOption ResolveSelectedBuildOption(List<BuildingBuildOption> availableOptions, bool isBlackMarket)
        {
            if (availableOptions == null || availableOptions.Count == 0)
            {
                SetSelectedBuildOptionKind(isBlackMarket, string.Empty);
                return default;
            }

            var selectedKind = GetSelectedBuildOptionKind(isBlackMarket);
            var selected = availableOptions.FirstOrDefault(option => string.Equals(option.Kind, selectedKind, StringComparison.OrdinalIgnoreCase));
            if (string.IsNullOrWhiteSpace(selected.Kind))
            {
                selected = availableOptions[0];
                SetSelectedBuildOptionKind(isBlackMarket, selected.Kind);
            }

            return selected;
        }

        private void SelectBuildOption(List<BuildingBuildOption> availableOptions, bool isBlackMarket, int index)
        {
            if (availableOptions == null || availableOptions.Count == 0)
            {
                SetSelectedBuildOptionKind(isBlackMarket, string.Empty);
                return;
            }

            var safeIndex = Math.Max(0, Math.Min(index, availableOptions.Count - 1));
            SetSelectedBuildOptionKind(isBlackMarket, availableOptions[safeIndex].Kind);
            if (lastRenderedSnapshot != null)
            {
                Render(lastRenderedSnapshot, summaryState);
            }
        }

        private string GetSelectedBuildOptionKind(bool isBlackMarket) => isBlackMarket ? selectedBlackMarketBuildOptionKind : selectedCityBuildOptionKind;

        private void SetSelectedBuildOptionKind(bool isBlackMarket, string kind)
        {
            if (isBlackMarket)
            {
                selectedBlackMarketBuildOptionKind = kind ?? string.Empty;
            }
            else
            {
                selectedCityBuildOptionKind = kind ?? string.Empty;
            }
        }

        private static string FormatBuildOptionChoiceLabel(BuildingBuildOption option)
        {
            if (string.IsNullOrWhiteSpace(option.Label))
            {
                return FormatBuildOptionCost(option);
            }

            return $"{option.Label} • {FormatBuildOptionCost(option)}";
        }

        private static bool IsBuildOptionUnlocked(ShellSummarySnapshot s, BuildingBuildOption option)
        {
            if (string.IsNullOrWhiteSpace(option.RequiredTechId)) return true;
            return (s?.ResearchedTechIds ?? new List<string>())
                .Any(techId => string.Equals(techId, option.RequiredTechId, StringComparison.OrdinalIgnoreCase));
        }

        private static bool CanAffordBuildOption(ResourceSnapshot resources, BuildingBuildOption option)
        {
            if (resources == null) return false;
            return NumberOrZero(resources.Materials) >= option.MaterialsCost
                && NumberOrZero(resources.Wealth) >= option.WealthCost
                && NumberOrZero(resources.Mana) >= option.ManaCost;
        }

        private static double NumberOrZero(double? value) => value ?? 0;

        private static string FormatBuildOptionCost(BuildingBuildOption option)
        {
            var parts = new List<string>
            {
                $"{option.MaterialsCost} materials",
                $"{option.WealthCost} wealth",
            };
            if (option.ManaCost > 0) parts.Add($"{option.ManaCost} mana");
            return string.Join(", ", parts);
        }

        private static List<BuildingSnapshot> SelectManageableBuildings(List<BuildingSnapshot> buildings, DateTime nowUtc)
        {
            return (buildings ?? new List<BuildingSnapshot>())
                .Where(b => b != null)
                .Where(b => !HasBuildingTimingAnchor(b))
                .Where(b => !IsBuildingReady(b, nowUtc))
                .GroupBy(GetBuildingSelectorKey, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(b => b.Slot ?? int.MaxValue)
                .ThenBy(b => FirstNonBlank(b.Name, b.BuildingId, b.Type))
                .ToList();
        }

        private static string GetBuildingSelectorKey(BuildingSnapshot building)
        {
            if (building == null) return string.Empty;
            var stableId = FirstNonBlank(building.Id, building.BuildingId);
            if (!string.IsNullOrWhiteSpace(stableId)) return stableId.Trim();
            if (building.Slot.HasValue) return $"slot:{building.Slot.Value}";
            return NormalizeIdentity(FirstNonBlank(building.Name, building.Type, "building"));
        }

        private static bool BuildingSelectorKeysMatch(BuildingSnapshot left, BuildingSnapshot right)
        {
            return string.Equals(GetBuildingSelectorKey(left), GetBuildingSelectorKey(right), StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatBuildingSelectorLabel(BuildingSnapshot building, bool isBlackMarket)
        {
            var title = FormatBuildingSelectorDisplayTitle(building, isBlackMarket);
            var status = BuildBuildingLifecycleLabel(building, DateTime.UtcNow);
            var parts = new List<string> { title };
            if (!string.IsNullOrWhiteSpace(status)) parts.Add(status);
            return string.Join(" • ", parts);
        }

        private static string FormatBuildingSelectorDisplayTitle(BuildingSnapshot building, bool isBlackMarket)
        {
            var levelSuffix = building?.Level.HasValue == true ? $" Lv {building.Level.Value}" : string.Empty;
            var name = building?.Name?.Trim() ?? string.Empty;
            if (IsPlayerFacingBuildingName(name))
            {
                return name + levelSuffix;
            }

            var type = building?.Type?.Trim() ?? string.Empty;
            if (IsPlayerFacingBuildingType(type))
            {
                return HumanizeKey(type) + levelSuffix;
            }

            var rawFallback = FirstNonBlank(building?.BuildingId, building?.Id, building?.Slot.HasValue == true ? $"slot {building.Slot.Value}" : string.Empty, isBlackMarket ? "Operator front" : "Building");
            return rawFallback + levelSuffix;
        }

        private static bool IsPlayerFacingBuildingName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return false;
            var value = raw.Trim();
            if (value.Equals("Building", StringComparison.OrdinalIgnoreCase)) return false;
            if (value.Equals("Front", StringComparison.OrdinalIgnoreCase)) return false;
            if (value.Equals("Operator front", StringComparison.OrdinalIgnoreCase)) return false;
            return !LooksLikeBackendBuildingId(value);
        }

        private static bool IsPlayerFacingBuildingType(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return false;
            var value = raw.Trim();
            if (value.Equals("Building", StringComparison.OrdinalIgnoreCase)) return false;
            if (value.Equals("Front", StringComparison.OrdinalIgnoreCase)) return false;
            if (value.Equals("Operator front", StringComparison.OrdinalIgnoreCase)) return false;
            return !LooksLikeBackendBuildingId(value);
        }

        private static bool LooksLikeBackendBuildingId(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return false;
            var value = raw.Trim();
            return value.StartsWith("bid_", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("b_", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("building_", StringComparison.OrdinalIgnoreCase);
        }

        private static string FormatBuildingCapacity(ShellSummarySnapshot s, bool isBlackMarket, int occupied)
        {
            var cap = s?.EffectiveBuildingSlots ?? s?.MaxBuildingSlots;
            var noun = isBlackMarket ? "front" : "building";
            if (!cap.HasValue || cap.Value <= 0)
            {
                return "Slot cap not surfaced";
            }

            var open = Math.Max(0, cap.Value - Math.Max(0, occupied));
            return $"{open} open of {cap.Value} {noun} slot{(cap.Value == 1 ? string.Empty : "s")}";
        }

        private static Dictionary<string, int> BuildBuildingTypeCounts(List<BuildingSnapshot> buildings)
        {
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var building in buildings ?? new List<BuildingSnapshot>())
            {
                var key = NormalizeIdentity(FirstNonBlank(building?.Type, building?.BuildingId, building?.Name));
                if (string.IsNullOrWhiteSpace(key)) key = "unknown";
                counts[key] = counts.TryGetValue(key, out var current) ? current + 1 : 1;
            }
            return counts;
        }

        private static string FormatBuildingTypeCounts(Dictionary<string, int> counts)
        {
            if (counts == null || counts.Count == 0) return "No building types surfaced.";
            return string.Join(" • ", counts
                .OrderByDescending(pair => pair.Value)
                .ThenBy(pair => pair.Key)
                .Select(pair => $"{HumanizeKey(pair.Key)} x{pair.Value}"));
        }

        private static string BuildBuildingInventoryNote(ShellSummarySnapshot s, List<BuildingSnapshot> buildings, List<CityTimerEntrySnapshot> timers, bool isBlackMarket, DateTime nowUtc)
        {
            var ready = (buildings ?? new List<BuildingSnapshot>()).Count(b => IsBuildingReady(b, nowUtc))
                + (timers ?? new List<CityTimerEntrySnapshot>()).Count(t => t.FinishesAtUtc.HasValue && t.FinishesAtUtc.Value <= nowUtc);
            var timed = (buildings ?? new List<BuildingSnapshot>()).Count(HasBuildingTimingAnchor)
                + (timers ?? new List<CityTimerEntrySnapshot>()).Count(t => !t.FinishesAtUtc.HasValue || t.FinishesAtUtc.Value > nowUtc);
            var noun = isBlackMarket ? "front" : "building";
            var manageable = SelectManageableBuildings(buildings, nowUtc).Count;
            var capacity = FormatBuildingCapacity(s, isBlackMarket, buildings?.Count ?? 0);
            return $"{ready} ready to update • {timed} timed • {manageable} manageable {noun}{(manageable == 1 ? string.Empty : "s")} • {capacity}. Only unlocked, affordable {noun} choices are shown below. Destroy/remodel/cancel use backend confirm-token contracts with no refunds in v1.";
        }

        private static string GetBuildingActionId(BuildingSnapshot building)
        {
            return FirstNonBlank(building?.Id, building?.BuildingId);
        }

        private static List<string> BuildBuildingRoutingPreferenceValues()
        {
            return new List<string> { "balanced", "prefer_local", "prefer_reserve", "prefer_exchange" };
        }

        private static List<string> BuildBuildingRoutingPreferenceLabels()
        {
            return new List<string>
            {
                "Balanced • spread output",
                "Local • nearby demand",
                "Reserve • protected stock",
                "Exchange • trade flow",
            };
        }

        private static string NormalizeBuildingRoutingPreference(string current)
        {
            var value = (current ?? string.Empty).Trim();
            if (value.Equals("prefer_local", StringComparison.OrdinalIgnoreCase)) return "prefer_local";
            if (value.Equals("local", StringComparison.OrdinalIgnoreCase)) return "prefer_local";
            if (value.Equals("prefer_reserve", StringComparison.OrdinalIgnoreCase)) return "prefer_reserve";
            if (value.Equals("reserve", StringComparison.OrdinalIgnoreCase)) return "prefer_reserve";
            if (value.Equals("protected_reserve", StringComparison.OrdinalIgnoreCase)) return "prefer_reserve";
            if (value.Equals("prefer_exchange", StringComparison.OrdinalIgnoreCase)) return "prefer_exchange";
            if (value.Equals("exchange", StringComparison.OrdinalIgnoreCase)) return "prefer_exchange";
            return "balanced";
        }

        private static string FormatBuildingRoutingPreferenceLabel(string routingPreference)
        {
            var normalized = NormalizeBuildingRoutingPreference(routingPreference);
            if (normalized.Equals("prefer_local", StringComparison.OrdinalIgnoreCase)) return "Local";
            if (normalized.Equals("prefer_reserve", StringComparison.OrdinalIgnoreCase)) return "Reserve";
            if (normalized.Equals("prefer_exchange", StringComparison.OrdinalIgnoreCase)) return "Exchange";
            return "Balanced";
        }

        private static string BuildBuildingRoutingSelectorLabel(bool isBlackMarket, string pendingRoutingPreference)
        {
            var prefix = isBlackMarket ? "Front output routing" : "Output routing";
            var guide = "Balanced spreads output; Local feeds nearby demand; Reserve protects stock; Exchange pushes trade.";
            return string.IsNullOrWhiteSpace(pendingRoutingPreference)
                ? $"{prefix} — {guide}"
                : $"{prefix} • switching to {FormatBuildingRoutingPreferenceLabel(pendingRoutingPreference)} — {guide}";
        }

        private static string BuildBuildingRoutingManagementNote(string baseNote, string currentRoutingPreference, string pendingRoutingPreference, bool isBlackMarket)
        {
            var noun = isBlackMarket ? "Front output" : "Building output";
            var note = string.IsNullOrWhiteSpace(baseNote) ? "Building/front truth is visible." : baseNote.Trim();
            var currentLabel = FormatBuildingRoutingPreferenceLabel(currentRoutingPreference);
            if (!string.IsNullOrWhiteSpace(pendingRoutingPreference))
            {
                return $"{note} {noun} routing: {currentLabel}. Routing switch pending: {FormatBuildingRoutingPreferenceLabel(pendingRoutingPreference)}.";
            }

            return $"{note} {noun} routing: {currentLabel}.";
        }

        private static string BuildBuildingCardsCopy(ShellSummarySnapshot s, bool isBlackMarket, DateTime nowUtc)
        {
            var buildings = SelectLaneBuildings(s, isBlackMarket);
            var timers = SelectBuildTimers(s, isBlackMarket);
            var ready = buildings.Count(b => IsBuildingReady(b, nowUtc)) + timers.Count(t => t.FinishesAtUtc.HasValue && t.FinishesAtUtc.Value <= nowUtc);
            var timed = buildings.Count(HasBuildingTimingAnchor) + timers.Count(t => !t.FinishesAtUtc.HasValue || t.FinishesAtUtc.Value > nowUtc);
            var standing = buildings.Count(b => !IsBuildingReady(b, nowUtc) && !HasBuildingTimingAnchor(b));
            var label = isBlackMarket ? "operator-front" : "building";

            var timerLabel = isBlackMarket ? "front timer" : "build timer";
            if (buildings.Count == 0 && timers.Count == 0)
            {
                return $"No {label} card or {timerLabel} is visible in the current summary payload.";
            }

            var standingLabel = isBlackMarket ? "front live" : "standing";
            return $"Showing {buildings.Count} {label} card(s), {timers.Count} {timerLabel}(s), {standing} {standingLabel}, {timed} timed, and {ready} ready to update state(s).";
        }

        private static List<BuildingSnapshot> SelectLaneBuildings(ShellSummarySnapshot s, bool isBlackMarket)
        {
            return (s?.Buildings ?? new List<BuildingSnapshot>())
                .Where(b => b != null && BuildingBelongsToLane(b, isBlackMarket))
                .ToList();
        }

        private static List<CityTimerEntrySnapshot> SelectBuildTimers(ShellSummarySnapshot s, bool isBlackMarket)
        {
            var buildings = SelectLaneBuildings(s, isBlackMarket);
            return (s?.CityTimers ?? new List<CityTimerEntrySnapshot>())
                .Where(t => t != null && (IsBuildTimer(t) || (isBlackMarket && IsFrontTimer(t))) && (!isBlackMarket || ContainsShadowLane(t.Category) || ContainsShadowLane(t.Label) || ContainsShadowLane(t.Detail) || IsBlackMarketLane(s)))
                .Where(t => !TimerMatchesActiveBuilding(t, buildings))
                .ToList();
        }

        private static bool TimerMatchesActiveBuilding(CityTimerEntrySnapshot timer, List<BuildingSnapshot> buildings)
        {
            if (timer == null || buildings == null || buildings.Count == 0) return false;
            var timerId = NormalizeIdentity(timer.Id);
            var timerLabel = NormalizeIdentity(timer.Label);
            if (string.IsNullOrWhiteSpace(timerId) && string.IsNullOrWhiteSpace(timerLabel)) return false;

            foreach (var building in buildings)
            {
                if (building == null) continue;
                if (!building.StartedAtUtc.HasValue && !building.FinishesAtUtc.HasValue) continue;

                var ids = new[]
                {
                    NormalizeIdentity(building.Id),
                    NormalizeIdentity(building.BuildingId),
                    NormalizeIdentity(building.Name),
                    NormalizeIdentity($"construct {building.Name}"),
                    NormalizeIdentity($"upgrade {building.Name}"),
                };

                if (ids.Any(id => !string.IsNullOrWhiteSpace(id) && (id == timerId || id == timerLabel)))
                {
                    return true;
                }
            }

            return false;
        }

        private static string NormalizeIdentity(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var value = raw.Trim();
            if (value.StartsWith("research:", StringComparison.OrdinalIgnoreCase)) value = value.Substring("research:".Length);
            if (value.StartsWith("build:", StringComparison.OrdinalIgnoreCase)) value = value.Substring("build:".Length);
            if (value.StartsWith("construction:", StringComparison.OrdinalIgnoreCase)) value = value.Substring("construction:".Length);
            if (value.StartsWith("upgrade:", StringComparison.OrdinalIgnoreCase)) value = value.Substring("upgrade:".Length);
            if (value.StartsWith("remodel:", StringComparison.OrdinalIgnoreCase)) value = value.Substring("remodel:".Length);
            if (value.StartsWith("cancel_build:", StringComparison.OrdinalIgnoreCase)) value = value.Substring("cancel_build:".Length);
            value = value.Replace('_', ' ').Replace('-', ' ');
            while (value.IndexOf("  ", StringComparison.Ordinal) >= 0) value = value.Replace("  ", " ");
            return value.ToLowerInvariant();
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
                || category.IndexOf("remodel", StringComparison.OrdinalIgnoreCase) >= 0
                || label.IndexOf("build", StringComparison.OrdinalIgnoreCase) >= 0
                || label.IndexOf("construction", StringComparison.OrdinalIgnoreCase) >= 0
                || label.IndexOf("remodel", StringComparison.OrdinalIgnoreCase) >= 0
                || detail.IndexOf("construction", StringComparison.OrdinalIgnoreCase) >= 0
                || detail.IndexOf("remodel", StringComparison.OrdinalIgnoreCase) >= 0;
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

        private static bool IsBuildTimerReady(CityTimerEntrySnapshot timer, DateTime nowUtc)
        {
            return IsReadyStatus(timer?.Status) || (timer?.FinishesAtUtc.HasValue == true && timer.FinishesAtUtc.Value <= nowUtc);
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

        private static bool HasBuildingTimingAnchor(BuildingSnapshot building)
        {
            return building?.StartedAtUtc.HasValue == true || building?.FinishesAtUtc.HasValue == true;
        }

        private static bool IsConstructingStatus(string status)
        {
            var value = (status ?? string.Empty).Trim();
            return value.Equals("build", StringComparison.OrdinalIgnoreCase)
                || value.Equals("building", StringComparison.OrdinalIgnoreCase)
                || value.Equals("construct", StringComparison.OrdinalIgnoreCase)
                || value.Equals("constructing", StringComparison.OrdinalIgnoreCase)
                || value.Equals("construction", StringComparison.OrdinalIgnoreCase)
                || value.Equals("in_progress", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsUpgradingStatus(string status)
        {
            var value = (status ?? string.Empty).Trim();
            return value.Equals("upgrade", StringComparison.OrdinalIgnoreCase)
                || value.Equals("upgrading", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsRemodelingStatus(string status)
        {
            var value = (status ?? string.Empty).Trim();
            return value.Equals("remodel", StringComparison.OrdinalIgnoreCase)
                || value.Equals("remodeling", StringComparison.OrdinalIgnoreCase);
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

        private static string BuildBuildingStatusButtonText(BuildingSnapshot building, bool isBlackMarket, DateTime nowUtc)
        {
            if (IsBuildingReady(building, nowUtc)) return "Ready to update";
            if (IsRemodelingStatus(building?.Status)) return "Remodeling";
            if (IsUpgradingStatus(building?.Status)) return "Upgrading";
            if (IsConstructingStatus(building?.Status) && HasBuildingTimingAnchor(building)) return isBlackMarket ? "Opening" : "Building";
            if (HasBuildingTimingAnchor(building)) return isBlackMarket ? "Front timer" : "Build timer";
            if (IsActiveStatus(building?.Status)) return isBlackMarket ? "Front live" : "Built";
            return "Visible";
        }

        private static string BuildBuildingCompletionStatusText(bool isBlackMarket)
        {
            return isBlackMarket ? "Front successfully opened." : "Building successfully completed.";
        }

        private static string BuildBuildingCompletionRefreshButtonText(bool isBlackMarket)
        {
            return isBlackMarket ? "Update front list" : "Update building list";
        }

        private static string BuildBuildingCompletionRefreshNote(bool isBlackMarket)
        {
            return isBlackMarket
                ? "Timer elapsed. Refresh this desk to pull the finished front into the live front list from backend truth."
                : "Timer elapsed. Refresh this desk to pull the completed building into the building list from backend truth.";
        }

        private static string BuildBuildingLore(BuildingSnapshot building, bool isBlackMarket, DateTime nowUtc)
        {
            if (IsBuildingReady(building, nowUtc) && HasBuildingTimingAnchor(building))
            {
                return BuildBuildingCompletionStatusText(isBlackMarket);
            }

            var parts = new List<string>();
            var statusLabel = BuildBuildingLifecycleLabel(building, nowUtc);
            if (!string.IsNullOrWhiteSpace(statusLabel)) parts.Add(statusLabel);
            if (building?.FinishesAtUtc.HasValue == true) parts.Add(FormatRemaining(building.FinishesAtUtc.Value - nowUtc));
            if (building?.ProductionPerTick != null)
            {
                var production = FormatProduction(building.ProductionPerTick, new ResourcePresentationSnapshot());
                if (!production.StartsWith("No production", StringComparison.OrdinalIgnoreCase)) parts.Add(production);
            }
            return parts.Count > 0 ? string.Join(" • ", parts) : "Building card surfaced from summary payload.";
        }

        private static string BuildBuildingLifecycleLabel(BuildingSnapshot building, DateTime nowUtc)
        {
            if (building == null) return string.Empty;
            if (IsBuildingReady(building, nowUtc)) return "Ready to update";
            if (IsRemodelingStatus(building.Status)) return "Remodeling";
            if (IsUpgradingStatus(building.Status)) return "Upgrading";
            if (IsConstructingStatus(building.Status) && HasBuildingTimingAnchor(building)) return "Building";
            if (HasBuildingTimingAnchor(building)) return "Timed";
            if (IsActiveStatus(building.Status)) return "Operational";
            return HumanizeKey(building.Status);
        }

        private static string BuildBuildingNote(BuildingSnapshot building, bool isBlackMarket, DateTime nowUtc)
        {
            if (IsBuildingReady(building, nowUtc)) return BuildBuildingCompletionRefreshNote(isBlackMarket);
            if (!string.IsNullOrWhiteSpace(building?.EffectSummary)) return building.EffectSummary;
            if (!string.IsNullOrWhiteSpace(building?.Detail)) return building.Detail;
            if (building?.StartedAtUtc.HasValue == true) return isBlackMarket ? $"Front opened {building.StartedAtUtc.Value:HH:mm:ss} UTC." : $"Construction started {building.StartedAtUtc.Value:HH:mm:ss} UTC.";
            return isBlackMarket ? "Operator-front truth is visible without fake covert simulation." : "Building truth is visible without fake construction simulation.";
        }

        private static string BuildBuildingManagementNote(string baseNote, bool canManage, bool canCancel)
        {
            var note = string.IsNullOrWhiteSpace(baseNote) ? "Building/front truth is visible." : baseNote.Trim();
            if (canCancel)
            {
                return $"{note} Cancel requires a second backend confirmation and refunds nothing in v1.";
            }

            if (canManage)
            {
                return $"{note} Destroy/remodel require a second backend confirmation and refund nothing in v1.";
            }

            return note;
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

        private static string FormatResourceTickNote(TimerSnapshot timing, DateTime nowUtc)
        {
            var next = ResolveRollingResourceTickAtUtc(timing, nowUtc);
            return next.HasValue
                ? $"Next resource tick in {FormatRemaining(next.Value - nowUtc)}"
                : "Resource cadence is visible without a live anchor.";
        }

        private static DateTime? ResolveRollingResourceTickAtUtc(TimerSnapshot timing, DateTime nowUtc)
        {
            var cadence = timing?.TickMs.HasValue == true && timing.TickMs.Value > 0
                ? TimeSpan.FromMilliseconds(timing.TickMs.Value)
                : (TimeSpan?)null;
            var anchor = timing?.NextTickAtUtc;
            if (!anchor.HasValue && timing?.LastTickAtUtc.HasValue == true && cadence.HasValue)
            {
                anchor = timing.LastTickAtUtc.Value + cadence.Value;
            }

            if (!anchor.HasValue)
            {
                return null;
            }

            if (!cadence.HasValue || cadence.Value <= TimeSpan.Zero || anchor.Value > nowUtc)
            {
                return anchor.Value;
            }

            var elapsed = nowUtc - anchor.Value;
            var skippedTicks = Math.Floor(elapsed.TotalMilliseconds / cadence.Value.TotalMilliseconds) + 1;
            return anchor.Value.AddMilliseconds(skippedTicks * cadence.Value.TotalMilliseconds);
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

        private readonly struct BuildingBuildOption
        {
            public BuildingBuildOption(string kind, string label, string summary, int sortOrder, int materialsCost, int wealthCost, int manaCost = 0, string requiredTechId = null)
            {
                Kind = kind;
                Label = label;
                Summary = summary;
                SortOrder = sortOrder;
                MaterialsCost = materialsCost;
                WealthCost = wealthCost;
                ManaCost = manaCost;
                RequiredTechId = requiredTechId ?? string.Empty;
            }

            public string Kind { get; }
            public string Label { get; }
            public string Summary { get; }
            public int SortOrder { get; }
            public int MaterialsCost { get; }
            public int WealthCost { get; }
            public int ManaCost { get; }
            public string RequiredTechId { get; }
        }

        private static readonly List<BuildingBuildOption> CityBuildOptions = new()
        {
            new BuildingBuildOption("housing", "Charter Ward", "Housing/ward capacity for population and civic stability.", 10, materialsCost: 60, wealthCost: 30),
            new BuildingBuildOption("farmland", "Granary Fields", "Food production and reserve stability for the civic lane.", 20, materialsCost: 50, wealthCost: 20),
            new BuildingBuildOption("mine", "Works Quarry", "Material throughput for building, workshop, and support demands.", 30, materialsCost: 80, wealthCost: 40),
            new BuildingBuildOption("arcane_spire", "Beacon Tower", "Mana and lawful arcana infrastructure for civic development.", 40, materialsCost: 70, wealthCost: 50, manaCost: 30),
            new BuildingBuildOption("hall_of_records", "Hall of Records", "Records depth building unlocked by the civic research runway.", 50, materialsCost: 64, wealthCost: 40, requiredTechId: "urban_planning_1"),
            new BuildingBuildOption("watch_barracks", "Watch Barracks", "Security/watch depth building unlocked by later civic development.", 60, materialsCost: 78, wealthCost: 34, requiredTechId: "urban_planning_2"),
            new BuildingBuildOption("provincial_office", "Provincial Office", "Outer-district support capacity for higher civic tiers.", 70, materialsCost: 92, wealthCost: 44, requiredTechId: "provincial_charters_1"),
        };

        private static readonly List<BuildingBuildOption> BlackMarketBuildOptions = new()
        {
            new BuildingBuildOption("safehouse", "Safehouse Ring", "Deniable cover, storage, and operator-front safety.", 10, materialsCost: 68, wealthCost: 42),
            new BuildingBuildOption("quiet_provisioning", "Quiet Provisioning Cell", "Low-visibility supplies and reserve movement for the shadow lane.", 20, materialsCost: 52, wealthCost: 24),
            new BuildingBuildOption("illicit_extraction", "Illicit Extraction Cell", "Riskier material throughput and dirty leverage.", 30, materialsCost: 86, wealthCost: 46),
            new BuildingBuildOption("occult_relay", "Occult Relay", "Arcana and hidden relay capacity for covert operations.", 40, materialsCost: 74, wealthCost: 54, manaCost: 26),
            new BuildingBuildOption("front_house", "Front House", "Respectable cover once the laundering runway is unlocked.", 50, materialsCost: 62, wealthCost: 48, requiredTechId: "front_businesses_1"),
            new BuildingBuildOption("debt_house", "Debt House", "Ledgered leverage for debt pressure and durable control.", 60, materialsCost: 54, wealthCost: 56, requiredTechId: "debt_ledgers_1"),
            new BuildingBuildOption("cutout_bureau", "Cutout Bureau", "Deniable network reach for late shadow runway work.", 70, materialsCost: 58, wealthCost: 64, requiredTechId: "cutout_syndicates_1"),
        };

        private readonly struct CardView
        {
            public CardView(
                string family,
                string title,
                string lore,
                string note,
                string buttonText = null,
                bool buttonEnabled = false,
                Action onClick = null,
                string secondaryButtonText = null,
                bool secondaryButtonEnabled = false,
                Action secondaryOnClick = null,
                string tertiaryButtonText = null,
                bool tertiaryButtonEnabled = false,
                Action tertiaryOnClick = null,
                string selectorLabel = null,
                IReadOnlyList<string> selectorOptions = null,
                int selectorIndex = -1,
                Action<int> selectorOnChange = null)
            {
                Family = family;
                Title = title;
                Lore = lore;
                Note = note;
                ButtonText = buttonText;
                ButtonEnabled = buttonEnabled;
                OnClick = onClick;
                SecondaryButtonText = secondaryButtonText;
                SecondaryButtonEnabled = secondaryButtonEnabled;
                SecondaryOnClick = secondaryOnClick;
                TertiaryButtonText = tertiaryButtonText;
                TertiaryButtonEnabled = tertiaryButtonEnabled;
                TertiaryOnClick = tertiaryOnClick;
                SelectorLabel = selectorLabel;
                SelectorOptions = selectorOptions ?? Array.Empty<string>();
                SelectorIndex = selectorIndex;
                SelectorOnChange = selectorOnChange;
            }

            public string Family { get; }
            public string Title { get; }
            public string Lore { get; }
            public string Note { get; }
            public string ButtonText { get; }
            public bool ButtonEnabled { get; }
            public Action OnClick { get; }
            public string SecondaryButtonText { get; }
            public bool SecondaryButtonEnabled { get; }
            public Action SecondaryOnClick { get; }
            public string TertiaryButtonText { get; }
            public bool TertiaryButtonEnabled { get; }
            public Action TertiaryOnClick { get; }
            public string SelectorLabel { get; }
            public IReadOnlyList<string> SelectorOptions { get; }
            public int SelectorIndex { get; }
            public Action<int> SelectorOnChange { get; }
        }

        private sealed class InfoCard
        {
            private readonly VisualElement root;
            private readonly Label family;
            private readonly Label title;
            private readonly Label lore;
            private readonly Label note;
            private readonly Button button;
            private readonly VisualElement selectorShell;
            private readonly Label selectorLabel;
            private readonly VisualElement selectorChoicesRoot;
            private VisualElement extraButtonRow;
            private Button secondaryButton;
            private Button tertiaryButton;
            private Action clickAction;
            private Action secondaryClickAction;
            private Action tertiaryClickAction;
            private Action<int> selectorChangeAction;
            private bool clickEnabled;
            private bool secondaryClickEnabled;
            private bool tertiaryClickEnabled;

            public InfoCard(VisualElement shellRoot, string prefix, bool hasButton = false)
            {
                root = shellRoot.Q<VisualElement>(prefix);
                family = shellRoot.Q<Label>($"{prefix}-family-value");
                title = shellRoot.Q<Label>($"{prefix}-title-value");
                lore = shellRoot.Q<Label>($"{prefix}-lore-value");
                note = shellRoot.Q<Label>($"{prefix}-note-value");
                button = hasButton ? shellRoot.Q<Button>($"{prefix}-button") : null;
                if (root != null)
                {
                    selectorShell = new VisualElement();
                    selectorShell.AddToClassList("development-inline-selector");
                    selectorShell.style.display = DisplayStyle.None;

                    selectorLabel = new Label();
                    selectorLabel.AddToClassList("development-inline-selector-label");
                    selectorShell.Add(selectorLabel);

                    selectorChoicesRoot = new VisualElement();
                    selectorChoicesRoot.AddToClassList("development-inline-selector-options");
                    selectorShell.Add(selectorChoicesRoot);

                    root.Add(selectorShell);
                }

                if (root != null && hasButton)
                {
                    extraButtonRow = new VisualElement();
                    extraButtonRow.style.flexDirection = FlexDirection.Row;
                    extraButtonRow.style.flexWrap = Wrap.Wrap;
                    extraButtonRow.style.marginTop = 4;
                    extraButtonRow.style.display = DisplayStyle.None;

                    secondaryButton = new Button(InvokeSecondaryClick);
                    secondaryButton.style.flexGrow = 1;
                    secondaryButton.style.marginRight = 4;

                    tertiaryButton = new Button(InvokeTertiaryClick);
                    tertiaryButton.style.flexGrow = 1;

                    extraButtonRow.Add(secondaryButton);
                    extraButtonRow.Add(tertiaryButton);
                    root.Add(extraButtonRow);
                }

                if (root != null)
                {
                    root.RegisterCallback<ClickEvent>(evt =>
                    {
                        if (!clickEnabled || clickAction == null)
                        {
                            return;
                        }

                        var targetElement = evt.target as VisualElement;
                        if (targetElement != null
                            && ((button != null && (ReferenceEquals(targetElement, button) || button.Contains(targetElement)))
                                || (extraButtonRow != null && (ReferenceEquals(targetElement, extraButtonRow) || extraButtonRow.Contains(targetElement)))
                                || (selectorShell != null && (ReferenceEquals(targetElement, selectorShell) || selectorShell.Contains(targetElement)))))
                        {
                            return;
                        }

                        InvokeClick();
                    });
                }

                if (button != null)
                {
                    button.clicked += InvokeClick;
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
                clickAction = view.OnClick;
                clickEnabled = view.ButtonEnabled && clickAction != null;
                root.EnableInClassList("is-actionable-card", clickEnabled);
                root.tooltip = clickEnabled
                    ? FirstNonBlank(view.ButtonText, view.Title)
                    : string.Empty;

                selectorChangeAction = view.SelectorOnChange;
                RenderInlineSelector(view);
                if (button != null)
                {
                    button.style.display = string.IsNullOrWhiteSpace(view.ButtonText) ? DisplayStyle.None : DisplayStyle.Flex;
                    button.text = view.ButtonText ?? "Read-only";
                    button.SetEnabled(clickEnabled);
                }

                secondaryClickAction = view.SecondaryOnClick;
                secondaryClickEnabled = view.SecondaryButtonEnabled && secondaryClickAction != null;
                tertiaryClickAction = view.TertiaryOnClick;
                tertiaryClickEnabled = view.TertiaryButtonEnabled && tertiaryClickAction != null;

                if (extraButtonRow != null)
                {
                    var showSecondary = !string.IsNullOrWhiteSpace(view.SecondaryButtonText);
                    var showTertiary = !string.IsNullOrWhiteSpace(view.TertiaryButtonText);
                    extraButtonRow.style.display = showSecondary || showTertiary ? DisplayStyle.Flex : DisplayStyle.None;

                    if (secondaryButton != null)
                    {
                        secondaryButton.style.display = showSecondary ? DisplayStyle.Flex : DisplayStyle.None;
                        secondaryButton.text = view.SecondaryButtonText ?? "Secondary";
                        secondaryButton.SetEnabled(secondaryClickEnabled);
                    }

                    if (tertiaryButton != null)
                    {
                        tertiaryButton.style.display = showTertiary ? DisplayStyle.Flex : DisplayStyle.None;
                        tertiaryButton.text = view.TertiaryButtonText ?? "Tertiary";
                        tertiaryButton.SetEnabled(tertiaryClickEnabled);
                    }
                }
            }

            private void RenderInlineSelector(CardView view)
            {
                if (selectorShell == null || selectorChoicesRoot == null)
                {
                    return;
                }

                selectorChoicesRoot.Clear();
                var choices = (view.SelectorOptions ?? Array.Empty<string>())
                    .Where(choice => !string.IsNullOrWhiteSpace(choice))
                    .ToList();

                if (choices.Count == 0)
                {
                    HideInlineSelector();
                    return;
                }

                selectorShell.style.display = DisplayStyle.Flex;
                if (selectorLabel != null)
                {
                    selectorLabel.text = string.IsNullOrWhiteSpace(view.SelectorLabel) ? "Select" : view.SelectorLabel;
                }

                var selectedIndex = Math.Max(0, Math.Min(view.SelectorIndex, choices.Count - 1));
                var canSelect = selectorChangeAction != null && choices.Count > 1;
                for (var i = 0; i < choices.Count; i++)
                {
                    var choiceIndex = i;
                    var choice = new Button(() =>
                    {
                        if (selectorChangeAction != null)
                        {
                            selectorChangeAction.Invoke(choiceIndex);
                        }
                    });
                    choice.text = choices[i];
                    choice.AddToClassList("development-inline-selector-choice");
                    choice.EnableInClassList("development-inline-selector-choice--selected", i == selectedIndex);
                    choice.SetEnabled(canSelect || i == selectedIndex);
                    selectorChoicesRoot.Add(choice);
                }
            }

            private void HideInlineSelector()
            {
                if (selectorShell != null)
                {
                    selectorShell.style.display = DisplayStyle.None;
                }

                selectorChoicesRoot?.Clear();
            }

            private void InvokeClick()
            {
                if (!clickEnabled || clickAction == null)
                {
                    return;
                }

                clickAction.Invoke();
            }

            private void InvokeSecondaryClick()
            {
                if (!secondaryClickEnabled || secondaryClickAction == null)
                {
                    return;
                }

                secondaryClickAction.Invoke();
            }

            private void InvokeTertiaryClick()
            {
                if (!tertiaryClickEnabled || tertiaryClickAction == null)
                {
                    return;
                }

                tertiaryClickAction.Invoke();
            }

            public void RenderHidden()
            {
                clickAction = null;
                secondaryClickAction = null;
                tertiaryClickAction = null;
                selectorChangeAction = null;
                clickEnabled = false;
                secondaryClickEnabled = false;
                tertiaryClickEnabled = false;
                HideInlineSelector();

                if (extraButtonRow != null)
                {
                    extraButtonRow.style.display = DisplayStyle.None;
                }

                if (root != null)
                {
                    root.EnableInClassList("is-actionable-card", false);
                    root.tooltip = string.Empty;
                    root.style.display = DisplayStyle.None;
                }
            }
        }
    }
}
