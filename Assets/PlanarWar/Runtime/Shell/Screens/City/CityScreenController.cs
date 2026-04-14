using PlanarWar.Client.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace PlanarWar.Client.UI.Screens.City
{
    public sealed class CityScreenController
    {
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
        private readonly Label researchCardsCopy;
        private readonly Label workshopCardsCopy;
        private readonly Label growthCardsCopy;
        private readonly Label researchFocus;
        private readonly Label nextTech;
        private readonly Label workshopValue;
        private readonly Label growthValue;
        private readonly Label supportValue;
        private readonly Label deskNote;
        private readonly DevCard[] researchCards;
        private readonly DevCard[] workshopCards;
        private readonly DevCard[] growthCards;

        public CityScreenController(VisualElement root)
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
            researchCardsCopy = root.Q<Label>("dev-research-cards-copy-value");
            workshopCardsCopy = root.Q<Label>("dev-workshop-cards-copy-value");
            growthCardsCopy = root.Q<Label>("dev-growth-cards-copy-value");
            researchFocus = root.Q<Label>("dev-research-focus-value");
            nextTech = root.Q<Label>("dev-next-tech-value");
            workshopValue = root.Q<Label>("dev-workshop-value");
            growthValue = root.Q<Label>("dev-growth-value");
            supportValue = root.Q<Label>("dev-support-value");
            deskNote = root.Q<Label>("dev-note-value");

            researchCards = BuildCards(root, "dev-research-card", includeButton: true);
            workshopCards = BuildCards(root, "dev-workshop-card");
            growthCards = BuildCards(root, "dev-growth-card");
        }

        public void Render(ShellSummarySnapshot s)
        {
            headline.text = s.HasCity ? $"{s.City.Name} city desk" : "City desk unavailable";
            copy.text = s.HasCity ? $"Tier {(s.City.Tier ?? 0)} • Lane {s.City.SettlementLaneLabel}" : "Create a settlement to unlock city systems.";
            laneTitle.text = "City overview";
            laneCopy.text = "Read-only strike: this desk shows current city truth without mutation wiring.";
            card1Title.text = "Missions";
            card1Value.text = s.ActiveMissions.Count == 0 ? "No active missions." : string.Join(" • ", s.ActiveMissions.Take(2).Select(m => m.Title));
            card2Title.text = "Heroes";
            card2Value.text = s.Heroes.Count == 0 ? "No heroes surfaced." : $"{s.Heroes.Count} total • {s.Heroes.Count(h => h.Status == "idle")} idle";
            card3Title.text = "Warfront";
            card3Value.text = s.WarfrontSignals.Count == 0 ? "No warfront snapshot." : string.Join(" • ", s.WarfrontSignals.Take(2).Select(x => $"{x.Label}: {x.Value}"));

            researchCardsCopy.text = $"{s.ActiveResearch?.Name ?? "No active research"} • {s.OpeningOperations.Count} opening operations";
            workshopCardsCopy.text = $"{s.WorkshopJobs.Count(j => !j.Completed)} active jobs • {s.WorkshopJobs.Count} total jobs";
            growthCardsCopy.text = $"{s.Resources.Food.GetValueOrDefault():0.#} food • {s.Resources.Materials.GetValueOrDefault():0.#} materials • {s.Resources.Wealth.GetValueOrDefault():0.#} wealth";

            researchFocus.text = s.ActiveResearch == null ? "No active research lane loaded." : $"{s.ActiveResearch.Name} • {FormatProgress(s.ActiveResearch.Progress, s.ActiveResearch.Cost)}";
            nextTech.text = s.OpeningOperations.Count == 0 ? "No suggested unlock surfaced." : $"{s.OpeningOperations[0].Title} ({s.OpeningOperations[0].Readiness})";
            workshopValue.text = FormatWorkshopHeadline(s.WorkshopJobs);
            growthValue.text = FormatTick(s.ResourceTickTiming);
            supportValue.text = s.ThreatWarnings.Count == 0 ? "No active threat warnings." : string.Join(" • ", s.ThreatWarnings.Take(2).Select(w => w.Headline));
            deskNote.text = s.HasCity ? "Read-only desk mirrors summary payload truth." : "No settlement loaded yet; development desk is read-only until city exists.";

            RenderResearchCards(s);
            RenderWorkshopCards(s);
            RenderGrowthCards(s);
        }

        private void RenderResearchCards(ShellSummarySnapshot s)
        {
            var cards = new List<(string family, string title, string detail, string note)>();
            if (s.ActiveResearch != null)
            {
                cards.Add(("Active research", s.ActiveResearch.Name, $"Progress {FormatProgress(s.ActiveResearch.Progress, s.ActiveResearch.Cost)}", s.ActiveResearch.StartedAtUtc.HasValue ? $"Started {s.ActiveResearch.StartedAtUtc.Value:HH:mm:ss} UTC" : "Start time unavailable"));
            }

            cards.AddRange(s.OpeningOperations.Take(3).Select(op => ("Available tech", op.Title, $"Readiness {op.Readiness}", "Mapped from settlement opening operations")));
            FillCards(researchCards, cards, "No research cards in payload.");
        }

        private void RenderWorkshopCards(ShellSummarySnapshot s)
        {
            var cards = s.WorkshopJobs
                .OrderBy(j => j.Completed)
                .ThenBy(j => j.FinishesAtUtc ?? DateTime.MaxValue)
                .Take(4)
                .Select(j => (
                    "Workshop queue",
                    string.IsNullOrWhiteSpace(j.AttachmentKind) ? "Workshop job" : j.AttachmentKind,
                    j.Completed ? "Completed" : $"In progress • {FormatRemaining(j.FinishesAtUtc)}",
                    j.FinishesAtUtc.HasValue ? $"Finishes {j.FinishesAtUtc.Value:HH:mm:ss} UTC" : "Finish time unavailable"))
                .ToList();
            FillCards(workshopCards, cards, "No workshop jobs surfaced.");
        }

        private void RenderGrowthCards(ShellSummarySnapshot s)
        {
            var cards = new List<(string family, string title, string detail, string note)>
            {
                ("Growth lane", "Resources", FormatResources(s.Resources), "Current stockpile"),
                ("Growth lane", "Production / tick", FormatResources(s.ProductionPerTick), "Per-tick generation snapshot"),
                ("Growth lane", "Resource timer", FormatTick(s.ResourceTickTiming), FormatTickRaw(s.ResourceTickTiming)),
                ("Growth lane", "Threat posture", s.ThreatWarnings.Count == 0 ? "No warnings." : s.ThreatWarnings[0].Headline, $"{s.ThreatWarnings.Count} total warning entries")
            };
            FillCards(growthCards, cards, "No growth cards surfaced.");
        }

        private static void FillCards(DevCard[] cards, IReadOnlyList<(string family, string title, string detail, string note)> values, string emptyMessage)
        {
            for (var i = 0; i < cards.Length; i++)
            {
                if (i < values.Count)
                {
                    cards[i].Family.text = values[i].family;
                    cards[i].Title.text = values[i].title;
                    cards[i].Detail.text = values[i].detail;
                    cards[i].Note.text = values[i].note;
                    cards[i].SetVisible(true);
                    continue;
                }

                cards[i].Family.text = "Read-only";
                cards[i].Title.text = "No entry";
                cards[i].Detail.text = emptyMessage;
                cards[i].Note.text = "Payload returned no additional items.";
                cards[i].SetVisible(true);
            }
        }

        private static DevCard[] BuildCards(VisualElement root, string prefix, bool includeButton = false) =>
            Enumerable.Range(1, 4)
                .Select(i => new DevCard(
                    root.Q<VisualElement>($"{prefix}-{i}"),
                    root.Q<Label>($"{prefix}-{i}-family-value"),
                    root.Q<Label>($"{prefix}-{i}-title-value"),
                    root.Q<Label>($"{prefix}-{i}-lore-value"),
                    root.Q<Label>($"{prefix}-{i}-note-value"),
                    includeButton ? root.Q<Button>($"{prefix}-{i}-button") : null))
                .ToArray();

        private static string FormatProgress(double? progress, double? cost) => cost.GetValueOrDefault() > 0 ? $"{progress.GetValueOrDefault():0.#}/{cost.Value:0.#}" : $"{progress.GetValueOrDefault():0.#}";
        private static string FormatWorkshopHeadline(List<WorkshopJobSnapshot> jobs) => jobs.Count == 0 ? "No workshop queue visible." : $"{jobs.Count(j => !j.Completed)} active • {jobs.Count(j => j.Completed)} completed";
        private static string FormatResources(ResourceSnapshot r) => $"Food {r.Food.GetValueOrDefault():0.#} • Materials {r.Materials.GetValueOrDefault():0.#} • Wealth {r.Wealth.GetValueOrDefault():0.#} • Mana {r.Mana.GetValueOrDefault():0.#} • Knowledge {r.Knowledge.GetValueOrDefault():0.#} • Unity {r.Unity.GetValueOrDefault():0.#}";
        private static string FormatTick(TimerSnapshot timing)
        {
            var cadence = GetCadence(timing);
            var nextTickAtUtc = ResolveNextTickAtUtc(timing, cadence);
            if (!nextTickAtUtc.HasValue && !cadence.HasValue) return "Growth cadence unavailable.";
            var remaining = nextTickAtUtc.HasValue ? FormatRemaining(nextTickAtUtc) : "anchor missing";
            var cadenceText = cadence.HasValue ? cadence.Value.ToString(@"mm\:ss") : "cadence unknown";
            return $"{remaining} • every {cadenceText}";
        }

        private static string FormatTickRaw(TimerSnapshot timing)
        {
            var cadence = GetCadence(timing);
            var nextTickAtUtc = ResolveNextTickAtUtc(timing, cadence);
            if (!cadence.HasValue && !nextTickAtUtc.HasValue) return "state=no_timing_data; tickMs=n/a, next=n/a";
            return $"state={(nextTickAtUtc.HasValue ? "countdown_ready" : "cadence_only_anchor_missing")}; tickMs={(cadence.HasValue ? cadence.Value.TotalMilliseconds.ToString("0.#") : "n/a")}, next={(nextTickAtUtc.HasValue ? nextTickAtUtc.Value.ToString("HH:mm:ss") + " UTC" : "n/a")}";
        }

        private static string FormatRemaining(DateTime? utc)
        {
            if (!utc.HasValue) return "time unknown";
            var delta = utc.Value - DateTime.UtcNow;
            if (delta <= TimeSpan.Zero) return "due now";
            return delta.TotalHours >= 1 ? delta.ToString(@"hh\:mm\:ss") : delta.ToString(@"mm\:ss");
        }

        private static TimeSpan? GetCadence(TimerSnapshot timing)
        {
            if (!timing.TickMs.HasValue || timing.TickMs <= 0) return null;
            return TimeSpan.FromMilliseconds(timing.TickMs.Value);
        }

        private static DateTime? ResolveNextTickAtUtc(TimerSnapshot timing, TimeSpan? cadence)
        {
            if (timing.NextTickAtUtc.HasValue) return timing.NextTickAtUtc.Value;
            if (!timing.LastTickAtUtc.HasValue || !cadence.HasValue) return null;
            return timing.LastTickAtUtc.Value + cadence.Value;
        }

        private readonly struct DevCard
        {
            public DevCard(VisualElement root, Label family, Label title, Label detail, Label note, Button actionButton)
            {
                Root = root;
                Family = family;
                Title = title;
                Detail = detail;
                Note = note;
                ActionButton = actionButton;
                if (ActionButton != null)
                {
                    ActionButton.SetEnabled(false);
                    ActionButton.text = "Read-only";
                }
            }

            public VisualElement Root { get; }
            public Label Family { get; }
            public Label Title { get; }
            public Label Detail { get; }
            public Label Note { get; }
            public Button ActionButton { get; }

            public void SetVisible(bool visible) => Root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
