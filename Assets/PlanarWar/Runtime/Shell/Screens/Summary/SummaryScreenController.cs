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
            armies.text = s.Armies.Count == 0 ? (s.HasCity ? "No formations visible." : "Found a city to unlock formations.") : $"{s.Armies.Count(a => (a.Readiness ?? 0) >= 70)}/{s.Armies.Count} ready";
            researchTimer.text = s.ActiveResearch?.StartedAtUtc == null ? "No active research timer." : $"Running {FormatRemaining(nowUtc - s.ActiveResearch.StartedAtUtc.Value)}";
            workshopTimer.text = FormatWorkshop(s.WorkshopJobs);
            missionTimer.text = FormatMission(s.ActiveMissions);
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

        private static string Pair(string name, double? value, string suffix) => value.HasValue ? $"{name} {value.Value:0.#}{suffix}" : null;
        private static string FormatProgress(double? p, double? c) => c.GetValueOrDefault() > 0 ? $"{p.GetValueOrDefault():0.#}/{c.Value:0.#}" : $"{p.GetValueOrDefault():0.#}";
        private static string FormatRemaining(TimeSpan span) => span <= TimeSpan.Zero ? "now" : span.ToString(span.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss");

        private static string FormatWorkshop(List<WorkshopJobSnapshot> jobs)
        {
            var active = jobs.Where(j => !j.Completed).ToArray();
            if (active.Length == 0) return "No active workshop queue.";
            var first = active[0];
            var timer = first.FinishesAtUtc.HasValue ? FormatRemaining(first.FinishesAtUtc.Value - DateTime.UtcNow) : "time unknown";
            return $"{first.AttachmentKind} • {timer}";
        }

        private static string FormatMission(List<MissionSnapshot> missions)
        {
            if (missions.Count == 0) return "No active mission clock.";
            var first = missions[0];
            var timer = first.FinishesAtUtc.HasValue ? FormatRemaining(first.FinishesAtUtc.Value - DateTime.UtcNow) : "time unknown";
            return $"{first.Title} • {timer}";
        }

        private static string FormatTick(TimerSnapshot timing)
        {
            if (!timing.TickMs.HasValue || timing.TickMs <= 0) return "Tick timing unavailable.";
            var cadence = TimeSpan.FromMilliseconds(timing.TickMs.Value);
            var remaining = timing.NextTickAtUtc.HasValue ? FormatRemaining(timing.NextTickAtUtc.Value - DateTime.UtcNow) : FormatRemaining(cadence);
            return $"{remaining} • every {FormatRemaining(cadence)}";
        }

        private static string FormatTimerRaw(TimerSnapshot timing, bool isSummaryLoaded)
        {
            if (!isSummaryLoaded)
            {
                return "raw: waiting for summary payload";
            }

            if (!timing.TickMs.HasValue || timing.TickMs <= 0)
            {
                return "raw: summary loaded; no resource tick timing in payload";
            }

            var last = timing.LastTickAtUtc.HasValue ? timing.LastTickAtUtc.Value.ToString("HH:mm:ss") + " UTC" : "n/a";
            var next = timing.NextTickAtUtc.HasValue ? timing.NextTickAtUtc.Value.ToString("HH:mm:ss") + " UTC" : "n/a";
            return $"raw: tickMs={timing.TickMs:0.#}, last={last}, next={next}";
        }
    }
}
