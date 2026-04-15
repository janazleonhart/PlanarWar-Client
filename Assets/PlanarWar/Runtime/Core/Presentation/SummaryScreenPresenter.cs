using PlanarWar.Client.Core.Contracts;
using System;
using System.Linq;
using UnityEngine.UIElements;

namespace PlanarWar.Client.Core.Presentation
{
    public sealed class SummaryScreenPresenter
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

        public SummaryScreenPresenter(VisualElement root)
        {
            statusHeadline = root.Q<Label>("status-headline-value"); resources = root.Q<Label>("resources-value"); production = root.Q<Label>("production-value"); research = root.Q<Label>("research-value"); warnings = root.Q<Label>("warnings-value"); readyOps = root.Q<Label>("ready-ops-value"); heroes = root.Q<Label>("hero-status-value"); armies = root.Q<Label>("army-status-value"); researchTimer = root.Q<Label>("research-timer-value"); workshopTimer = root.Q<Label>("workshop-timer-value"); missionTimer = root.Q<Label>("mission-timer-value"); resourceTick = root.Q<Label>("resource-tick-value");
        }

        public void Render(ShellSummarySnapshot s)
        {
            statusHeadline.text = s.HasCity ? $"{s.City.Name} • {s.City.SettlementLaneLabel}" : (s.FounderMode ? "Founder mode active." : "No settlement loaded.");
            resources.text = FormatResource(s.Resources, "No resources loaded.");
            production.text = FormatResource(s.ProductionPerTick, s.HasCity ? "No production snapshot." : "Found a city to unlock production.", "/tick");
            research.text = s.ActiveResearch == null ? "No active research." : $"{s.ActiveResearch.Name} • {s.ActiveResearch.Progress.GetValueOrDefault():0.#}/{s.ActiveResearch.Cost.GetValueOrDefault():0.#}";
            warnings.text = s.ThreatWarnings.Count == 0 ? "No active threat warnings." : s.ThreatWarnings[0].Headline;
            readyOps.text = s.OpeningOperations.Count == 0 ? "No opening operations surfaced." : $"{s.OpeningOperations.Count(o => string.Equals(o.Readiness, "ready_now", StringComparison.OrdinalIgnoreCase))} ready now";
            heroes.text = s.Heroes.Count == 0 ? (s.HasCity ? "No officer corps visible." : "Found a city to unlock officers.") : $"{s.Heroes.Count(h => h.Status == "idle")}/{s.Heroes.Count} idle • {s.Heroes.Count(h => h.AttachmentCount > 0)} geared";
            armies.text = s.Armies.Count == 0 ? (s.HasCity ? "No formations visible." : "Found a city to unlock formations.") : $"{s.Armies.Count(a => (a.Readiness ?? 0) >= 70)}/{s.Armies.Count} ready";
            researchTimer.text = s.ActiveResearch?.StartedAtUtc == null ? "No active research timer." : $"Running {(DateTime.UtcNow - s.ActiveResearch.StartedAtUtc.Value):hh\\:mm\\:ss}";
            var nowUtc = DateTime.UtcNow;
            var readyWorkshopJobs = s.WorkshopJobs.Count(j => !IsWorkshopJobCollected(j) && IsWorkshopJobCollectable(j, nowUtc));
            var activeWorkshopJobs = s.WorkshopJobs.Count(j => !IsWorkshopJobCollected(j) && !IsWorkshopJobCollectable(j, nowUtc));
            workshopTimer.text = readyWorkshopJobs > 0
                ? $"{readyWorkshopJobs} ready to collect"
                : activeWorkshopJobs > 0
                    ? $"{activeWorkshopJobs} active"
                    : "No active workshop queue.";
            missionTimer.text = s.ActiveMissions.Count == 0 ? "No active mission clock." : s.ActiveMissions[0].Title;
            resourceTick.text = !s.ResourceTickTiming.TickMs.HasValue ? "Tick timing unavailable." : $"every {TimeSpan.FromMilliseconds(s.ResourceTickTiming.TickMs.Value):mm\\:ss}";
        }

        private static string FormatResource(ResourceSnapshot r, string fallback, string suffix = "")
        {
            var chunks = new[] { Pair("Food", r.Food, suffix), Pair("Materials", r.Materials, suffix), Pair("Wealth", r.Wealth, suffix), Pair("Mana", r.Mana, suffix), Pair("Knowledge", r.Knowledge, suffix), Pair("Unity", r.Unity, suffix) }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            return chunks.Length == 0 ? fallback : string.Join(" • ", chunks);
        }

        private static string Pair(string name, double? value, string suffix) => value.HasValue ? $"{name} {value.Value:0.#}{suffix}" : null;

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
    }
}
