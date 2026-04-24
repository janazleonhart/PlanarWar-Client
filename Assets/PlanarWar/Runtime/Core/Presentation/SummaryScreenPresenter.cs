using PlanarWar.Client.Core.Contracts;
using System.Collections.Generic;
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
            resources.text = FormatResource(s.Resources, s.ResourceLabels, "No resources loaded.");
            production.text = FormatResource(s.ProductionPerTick, s.ResourceLabels, s.HasCity ? "No production snapshot." : "Found a city to unlock production.", "/tick");
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
            missionTimer.text = BuildMissionDeskSummary(s);
            resourceTick.text = !s.ResourceTickTiming.TickMs.HasValue ? "Tick timing unavailable." : $"every {TimeSpan.FromMilliseconds(s.ResourceTickTiming.TickMs.Value):mm\\:ss}";
        }

        private static string FormatResource(ResourceSnapshot r, ResourcePresentationSnapshot labels, string fallback, string suffix = "")
        {
            var chunks = new[] { Pair(labels, "food", r.Food, suffix), Pair(labels, "materials", r.Materials, suffix), Pair(labels, "wealth", r.Wealth, suffix), Pair(labels, "mana", r.Mana, suffix), Pair(labels, "knowledge", r.Knowledge, suffix), Pair(labels, "unity", r.Unity, suffix) }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            return chunks.Length == 0 ? fallback : string.Join(" • ", chunks);
        }

        private static string Pair(ResourcePresentationSnapshot labels, string key, double? value, string suffix) => value.HasValue ? $"{ResourcePresentationText.Label(labels, key)} {value.Value:0.#}{suffix}" : null;


        private static string BuildMissionDeskSummary(ShellSummarySnapshot summary)
        {
            if (summary == null || summary.ActiveMissions == null || summary.ActiveMissions.Count == 0)
            {
                return "No active mission clock.";
            }

            return BuildMissionDeskSummary(summary.ActiveMissions[0], summary.Armies, summary.Heroes, includeTimer: true);
        }

        private static string BuildMissionDeskSummary(MissionSnapshot mission, IReadOnlyList<ArmySnapshot> armies, IReadOnlyList<HeroSnapshot> heroes, bool includeTimer)
        {
            if (mission == null)
            {
                return "No active mission clock.";
            }

            var parts = new List<string>();
            var title = FirstNonBlank(mission.Title, mission.Id, "Mission");
            parts.Add(title);

            var regionLabel = HumanizeRegionId(mission.RegionId);
            if (!string.IsNullOrWhiteSpace(regionLabel))
            {
                parts.Add(regionLabel);
            }

            var armyName = ResolveMissionArmyName(armies, mission.AssignedArmyId);
            if (!string.IsNullOrWhiteSpace(armyName))
            {
                parts.Add(armyName);
            }

            var heroName = ResolveMissionHeroName(heroes, mission.AssignedHeroId);
            if (!string.IsNullOrWhiteSpace(heroName))
            {
                parts.Add(heroName);
            }

            if (includeTimer)
            {
                var timer = mission.FinishesAtUtc.HasValue ? FormatRemaining(mission.FinishesAtUtc.Value - DateTime.UtcNow) : "anchor missing";
                parts.Add(timer);
            }

            return string.Join(" • ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string BuildMissionCommitmentSummary(MissionSnapshot mission, IReadOnlyList<ArmySnapshot> armies, IReadOnlyList<HeroSnapshot> heroes)
        {
            if (mission == null)
            {
                return string.Empty;
            }

            var details = new List<string>();
            var regionLabel = HumanizeRegionId(mission.RegionId);
            if (!string.IsNullOrWhiteSpace(regionLabel))
            {
                details.Add($"Region: {regionLabel}");
            }

            var armyName = ResolveMissionArmyName(armies, mission.AssignedArmyId);
            if (!string.IsNullOrWhiteSpace(armyName))
            {
                details.Add($"Formation: {armyName}");
            }

            var heroName = ResolveMissionHeroName(heroes, mission.AssignedHeroId);
            if (!string.IsNullOrWhiteSpace(heroName))
            {
                details.Add($"Hero: {heroName}");
            }

            return string.Join(" • ", details.Where(detail => !string.IsNullOrWhiteSpace(detail)));
        }

        private static string ResolveMissionArmyName(IReadOnlyList<ArmySnapshot> armies, string armyId)
        {
            if (string.IsNullOrWhiteSpace(armyId) || armies == null)
            {
                return string.Empty;
            }

            return armies.FirstOrDefault(army => string.Equals(army.Id, armyId, StringComparison.OrdinalIgnoreCase))?.Name
                ?? armyId.Trim();
        }

        private static string ResolveMissionHeroName(IReadOnlyList<HeroSnapshot> heroes, string heroId)
        {
            if (string.IsNullOrWhiteSpace(heroId) || heroes == null)
            {
                return string.Empty;
            }

            return heroes.FirstOrDefault(hero => string.Equals(hero.Id, heroId, StringComparison.OrdinalIgnoreCase))?.Name
                ?? heroId.Trim();
        }

        private static string HumanizeRegionId(string regionId)
        {
            if (string.IsNullOrWhiteSpace(regionId))
            {
                return string.Empty;
            }

            var cleaned = regionId.Replace('_', ' ').Replace('-', ' ').Trim();
            if (cleaned.Length == 0)
            {
                return string.Empty;
            }

            return char.ToUpperInvariant(cleaned[0]) + cleaned.Substring(1);
        }


        private static string FirstNonBlank(params string[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return string.Empty;
        }

        private static string FormatRemaining(TimeSpan remaining)
        {
            if (remaining < TimeSpan.Zero)
            {
                remaining = TimeSpan.Zero;
            }

            if (remaining.TotalHours >= 1)
            {
                return $"{(int)remaining.TotalHours}h {remaining.Minutes:00}m";
            }

            return $"{remaining.Minutes:00}m {remaining.Seconds:00}s";
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
    }
}
