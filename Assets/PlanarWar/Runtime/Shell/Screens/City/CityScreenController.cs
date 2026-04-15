using PlanarWar.Client.Core;
using PlanarWar.Client.Core.Contracts;
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
        private readonly Action onRefreshDeskRequested;
        private readonly Action onBackHomeRequested;
        private readonly Button refreshDeskButton;
        private readonly Button startSuggestedResearchButton;
        private readonly Button backHomeButton;

        private DevelopmentLane activeLane = DevelopmentLane.Research;

        public CityScreenController(VisualElement root, SummaryState summaryState, Func<string, Task> onStartResearchRequested, Func<string, Task> onStartWorkshopCraftRequested, Func<string, Task> onCollectWorkshopRequested, Action onRefreshDeskRequested, Action onBackHomeRequested)
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
            this.onRefreshDeskRequested = onRefreshDeskRequested;
            this.onBackHomeRequested = onBackHomeRequested;

            researchCards = Enumerable.Range(1, 4).Select(i => new InfoCard(root, $"dev-research-card-{i}", hasButton: true)).ToArray();
            workshopCards = Enumerable.Range(1, 4).Select(i => new InfoCard(root, $"dev-workshop-card-{i}", hasButton: true)).ToArray();
            growthCards = Enumerable.Range(1, 4).Select(i => new InfoCard(root, $"dev-growth-card-{i}")).ToArray();
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

            if (!s.HasCity)
            {
                RenderNoCity();
                return;
            }

            headline.text = $"{s.City.Name} • Development";
            copy.text = $"Tier {(s.City.Tier ?? 0)} {HumanizeLane(s.City.SettlementLaneLabel)} desk. Review research, queues, and growth posture without leaving the city shell.";

            card1Title.text = "Research lane";
            card1Value.text = s.ActiveResearch != null
                ? $"{s.ActiveResearch.Name} • {FormatProgress(s.ActiveResearch.Progress, s.ActiveResearch.Cost)}"
                : s.AvailableTechs.Count > 0
                    ? $"{s.AvailableTechs.Count} tech option{(s.AvailableTechs.Count == 1 ? string.Empty : "s")} ready"
                    : "No active research or tech options surfaced.";
            card2Title.text = "Workshop lane";
            card2Value.text = DescribeWorkshopLane(s, recipeCount);
            card3Title.text = "Growth lane";
            card3Value.text = DescribeGrowthLane(s);

            researchFocusValue.text = s.ActiveResearch?.Name ?? "No active research focus.";
            nextTechValue.text = s.AvailableTechs.FirstOrDefault()?.Name ?? "No available tech surfaced.";
            workshopValue.text = DescribeWorkshopLane(s, recipeCount);
            growthValue.text = DescribeGrowthLane(s);
            supportValue.text = DescribeSupport(s);
            noteValue.text = BuildDeskNote(s, summaryState);
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
            laneTitle.text = activeLane == DevelopmentLane.Research ? "Research lane" : laneTitle.text;
            laneCopy.text = activeLane == DevelopmentLane.Research
                ? "Research keeps the current tech, next unlocks, and readiness posture visible in one place so you can judge the next order before mutation wiring lands."
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
            laneTitle.text = activeLane == DevelopmentLane.Workshop ? "Workshop lane" : laneTitle.text;
            laneCopy.text = activeLane == DevelopmentLane.Workshop
                ? "Workshop keeps active jobs, ready pickups, real recipe cards, and queue posture visible in one place."
                : laneCopy.text;

            var cards = new List<CardView>();
            var activeJobs = s.WorkshopJobs.Where(j => !j.Completed).ToList();
            var readyJobs = s.WorkshopJobs.Where(j => j.Completed).ToList();
            var workshopTimers = s.CityTimers.Where(t => string.Equals(t.Category, "workshop_job", StringComparison.OrdinalIgnoreCase)).ToList();

            cards.AddRange(activeJobs.Take(2).Select(job => new CardView(
                family: "Active job",
                title: ResolveWorkshopJobTitle(job),
                lore: job.FinishesAtUtc.HasValue ? $"Ready in {FormatRemaining(job.FinishesAtUtc.Value - DateTime.UtcNow)}" : "Job surfaced without a finish anchor.",
                note: job.Completed ? "Marked complete in payload." : "Queued workshop job from summary payload.",
                buttonText: "In flight",
                buttonEnabled: false)));

            cards.AddRange(readyJobs.Take(2).Select(job => new CardView(
                family: "Ready pickup",
                title: ResolveWorkshopJobTitle(job),
                lore: "Complete and waiting in the workshop queue.",
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
            laneTitle.text = activeLane == DevelopmentLane.Growth ? "Growth lane" : laneTitle.text;
            laneCopy.text = activeLane == DevelopmentLane.Growth
                ? "Growth keeps cadence, live timers, and support posture visible so you can read the city’s pacing without drilling into mutations."
                : laneCopy.text;

            var cards = new List<CardView>();
            cards.Add(new CardView(
                family: "Production",
                title: "Per-tick output",
                lore: FormatProduction(s.ProductionPerTick),
                note: s.ResourceTickTiming.NextTickAtUtc.HasValue ? $"Next resource tick in {FormatRemaining(s.ResourceTickTiming.NextTickAtUtc.Value - DateTime.UtcNow)}" : "Resource cadence is visible without a live anchor."));

            cards.AddRange(s.CityTimers
                .Where(t => !string.Equals(t.Category, "resource_tick", StringComparison.OrdinalIgnoreCase) && !string.Equals(t.Category, "workshop_job", StringComparison.OrdinalIgnoreCase))
                .Take(2)
                .Select(timer => new CardView(
                    family: HumanizeCategory(timer.Category),
                    title: timer.Label,
                    lore: timer.FinishesAtUtc.HasValue ? $"{timer.Status} • {FormatRemaining(timer.FinishesAtUtc.Value - DateTime.UtcNow)}" : timer.Status,
                    note: FirstNonBlank(timer.Detail, "Live city timer surfaced from /api/me."))));

            if (cards.Count < 4)
            {
                var mission = s.ActiveMissions.FirstOrDefault();
                if (mission != null)
                {
                    cards.Add(new CardView(
                        family: "Mission pressure",
                        title: mission.Title,
                        lore: mission.FinishesAtUtc.HasValue ? $"Mission resolves in {FormatRemaining(mission.FinishesAtUtc.Value - DateTime.UtcNow)}" : "Mission is live without a finish anchor.",
                        note: FirstNonBlank(s.ThreatWarnings.FirstOrDefault()?.Headline, "Mission pressure is part of the current growth posture.")));
                }
            }

            if (cards.Count < 4)
            {
                cards.Add(new CardView(
                    family: "Support posture",
                    title: DescribeSupportTitle(s),
                    lore: DescribeSupport(s),
                    note: s.OpeningOperations.Count > 0 ? $"Opening operations visible: {string.Join(" • ", s.OpeningOperations.Take(2).Select(o => o.Title))}" : "No extra opening operation pressure is surfaced right now."));
            }

            growthCardsCopyValue.text = $"Showing cadence, {s.CityTimers.Count} live timer(s), and current support posture from the summary payload.";
            RenderCards(growthCards, cards);
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

        private static TechOptionSnapshot GetSuggestedTech(ShellSummarySnapshot s)
        {
            return s?.AvailableTechs?.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t?.Id));
        }

        private static string BuildDeskNote(ShellSummarySnapshot s, SummaryState summaryState)
        {
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
            var activeJobs = s.WorkshopJobs.Count(j => !j.Completed);
            var readyJobs = s.WorkshopJobs.Count(j => j.Completed);
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

        private static string ResolveWorkshopJobTitle(WorkshopJobSnapshot job)
        {
            if (job == null)
            {
                return "Workshop job";
            }

            var title = FirstNonBlank(job.OutputName, job.DisplayName, job.RecipeId, job.AttachmentKind, job.OutputItemId);
            return string.IsNullOrWhiteSpace(title) ? "Workshop job" : HumanizeKey(title);
        }

        private static string DescribeGrowthLane(ShellSummarySnapshot s)
        {
            var liveTimers = s.CityTimers.Count;
            if (s.ResourceTickTiming.NextTickAtUtc.HasValue)
            {
                return $"Next tick in {FormatRemaining(s.ResourceTickTiming.NextTickAtUtc.Value - DateTime.UtcNow)} • {liveTimers} live timer(s).";
            }

            return liveTimers > 0 ? $"{liveTimers} live timer(s) visible; cadence is readable." : "Growth cadence unavailable.";
        }

        private static string DescribeSupport(ShellSummarySnapshot s)
        {
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

        private static string DescribeSupportTitle(ShellSummarySnapshot s)
        {
            if (s.ThreatWarnings.Count > 0) return "Warning posture";
            if (s.OpeningOperations.Count > 0) return "Opening posture";
            return "Support posture";
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
