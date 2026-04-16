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
            readyOps.text = s.OpeningOperations.Count == 0 ? "No opening operations surfaced." : $"{s.OpeningOperations.Count(o => string.Equals(o.Readiness, "ready_now", StringComparison.OrdinalIgnoreCase))} ready now";
            heroes.text = s.Heroes.Count == 0 ? (s.HasCity ? "No officer corps visible." : "Found a city to unlock officers.") : $"{s.Heroes.Count(h => h.Status == "idle")}/{s.Heroes.Count} idle • {s.Heroes.Count(h => h.AttachmentCount > 0)} geared";
            armies.text = BuildArmyDeskSummary(s);
            researchTimer.text = s.ActiveResearch?.StartedAtUtc == null ? "No active research timer." : $"Running {FormatRemaining(nowUtc - s.ActiveResearch.StartedAtUtc.Value)}";
            workshopTimer.text = FormatWorkshop(s.WorkshopJobs);
            missionTimer.text = BuildMissionDeskSummary(s);
            resourceTick.text = FormatTick(s.ResourceTickTiming);
            timerDiagNow.text = $"Live UI clock {nowUtc:HH:mm:ss} UTC";
            timerDiagHeartbeat.text = $"Heartbeat #{heartbeat}";
            timerDiagRaw.text = FormatTimerRaw(s.ResourceTickTiming, isSummaryLoaded);
            timerDiagComputed.text = $"diag: {FormatTick(s.ResourceTickTiming)}";
        }

        private static string FormatResource(ResourceSnapshot r, string fallback, string suffix = "")
        {
            var chunks = new[]
            {
                Pair("Food", r.Food, suffix), Pair("Materials", r.Materials, suffix), Pair("Wealth", r.Wealth, suffix), Pair("Mana", r.Mana, suffix), Pair("Knowledge", r.Knowledge, suffix), Pair("Unity", r.Unity, suffix)
            }.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            return chunks.Length == 0 ? fallback : string.Join(" • ", chunks);
        }

        private static string BuildArmyDeskSummary(ShellSummarySnapshot summary)
        {
            if (summary.Armies.Count == 0)
            {
                return summary.HasCity ? "No formations visible." : "Found a city to unlock formations.";
            }

            var reinforceState = summary.ArmyReinforcement;
            var targetArmyId = FirstNonBlank(reinforceState?.ArmyId);
            var rankedArmies = RankArmies(summary.Armies, targetArmyId).Take(2).ToList();
            var readyCount = summary.Armies.Count(army => (army.Readiness ?? 0) >= 70);
            var parts = new List<string> { $"{readyCount}/{summary.Armies.Count} ready" };

            var totals = BuildArmyTotals(summary.Armies);
            if (!string.IsNullOrWhiteSpace(totals))
            {
                parts.Add(totals);
            }

            foreach (var army in rankedArmies)
            {
                parts.Add(FormatArmyHeadline(army, string.Equals(army.Id, targetArmyId, StringComparison.OrdinalIgnoreCase)));
            }

            var reinforcement = BuildReinforcementInline(reinforceState, summary.Armies);
            if (!string.IsNullOrWhiteSpace(reinforcement))
            {
                parts.Add(reinforcement);
            }

            return string.Join(" • ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string BuildArmyTotals(IEnumerable<ArmySnapshot> armies)
        {
            var parts = new List<string>();
            var sizedArmies = armies.Where(army => army.Size.HasValue).ToList();
            if (sizedArmies.Count > 0)
            {
                parts.Add($"{sizedArmies.Sum(army => army.Size.Value):0.#} troops");
            }

            var poweredArmies = armies.Where(army => army.Power.HasValue).ToList();
            if (poweredArmies.Count > 0)
            {
                parts.Add($"{poweredArmies.Sum(army => army.Power.Value):0.#} power");
            }

            return parts.Count == 0 ? string.Empty : string.Join(" • ", parts);
        }

        private static string FormatArmyHeadline(ArmySnapshot army, bool isTarget)
        {
            var label = new List<string>();
            if (isTarget)
            {
                label.Add("target");
            }

            label.Add(army.Name);
            if (!string.Equals(army.Status, "idle", StringComparison.OrdinalIgnoreCase))
            {
                label.Add(HumanizeStatus(army.Status));
            }

            if (army.Readiness.HasValue)
            {
                label.Add($"{army.Readiness.Value:0.#}");
            }

            return string.Join(" ", label.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string BuildReinforcementInline(ArmyReinforcementSnapshot reinforceState, IReadOnlyList<ArmySnapshot> armies)
        {
            if (reinforceState == null)
            {
                return string.Empty;
            }

            var armyName = FirstNonBlank(reinforceState.ArmyName, ResolveArmyName(armies, reinforceState.ArmyId), "formation");
            var deltaText = BuildReinforcementDeltaText(reinforceState);
            if (string.Equals(reinforceState.Status, "reinforcing", StringComparison.OrdinalIgnoreCase))
            {
                return string.IsNullOrWhiteSpace(deltaText)
                    ? $"Reinforcing {armyName}"
                    : $"Reinforcing {armyName} {deltaText}";
            }

            if (string.Equals(reinforceState.Status, "idle", StringComparison.OrdinalIgnoreCase) && reinforceState.StartEligible)
            {
                return string.IsNullOrWhiteSpace(deltaText)
                    ? $"Next reinforce {armyName}"
                    : $"Next reinforce {armyName} {deltaText}";
            }

            if (!string.IsNullOrWhiteSpace(reinforceState.BlockedReason))
            {
                return $"Reinforce blocked: {reinforceState.BlockedReason}";
            }

            if (!string.IsNullOrWhiteSpace(reinforceState.Shortfall))
            {
                return $"Reinforce shortfall: {reinforceState.Shortfall}";
            }

            return string.Empty;
        }

        private static string BuildReinforcementDeltaText(ArmyReinforcementSnapshot reinforceState)
        {
            var parts = new List<string>();
            if (reinforceState.SizeDelta.HasValue)
            {
                parts.Add($"+{reinforceState.SizeDelta.Value:0.#} troops");
            }

            if (reinforceState.PowerDelta.HasValue)
            {
                parts.Add($"+{reinforceState.PowerDelta.Value:0.#} power");
            }

            if (reinforceState.ReadinessDelta.HasValue)
            {
                parts.Add($"+{reinforceState.ReadinessDelta.Value:0.#} readiness");
            }

            return parts.Count == 0 ? string.Empty : $"({string.Join(", ", parts)})";
        }

        private static List<ArmySnapshot> RankArmies(IEnumerable<ArmySnapshot> armies, string targetArmyId)
        {
            return armies
                .OrderByDescending(army => !string.IsNullOrWhiteSpace(targetArmyId) && string.Equals(army.Id, targetArmyId, StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(army => GetArmyStatusPriority(army.Status))
                .ThenByDescending(army => army.Readiness ?? -1)
                .ThenByDescending(army => army.Power ?? -1)
                .ThenBy(army => army.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static int GetArmyStatusPriority(string status)
        {
            if (string.Equals(status, "reinforcing", StringComparison.OrdinalIgnoreCase)) return 3;
            if (string.Equals(status, "holding", StringComparison.OrdinalIgnoreCase)) return 2;
            if (string.Equals(status, "on_mission", StringComparison.OrdinalIgnoreCase)) return 1;
            return 0;
        }

        private static string ResolveArmyName(IReadOnlyList<ArmySnapshot> armies, string armyId)
        {
            if (string.IsNullOrWhiteSpace(armyId))
            {
                return string.Empty;
            }

            return armies.FirstOrDefault(army => string.Equals(army.Id, armyId, StringComparison.OrdinalIgnoreCase))?.Name ?? armyId;
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

        private static string HumanizeStatus(string value) => string.IsNullOrWhiteSpace(value) ? "unknown" : value.Replace('_', ' ');
        private static string FirstNonBlank(params string[] values) => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
    }
}
