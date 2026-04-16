using PlanarWar.Client.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace PlanarWar.Client.Core.Presentation
{
    public sealed class CityScreenPresenter
    {
        private readonly Label headline; private readonly Label copy; private readonly Label laneTitle; private readonly Label laneCopy; private readonly Label card1Title; private readonly Label card1Value; private readonly Label card2Title; private readonly Label card2Value; private readonly Label card3Title; private readonly Label card3Value;
        public CityScreenPresenter(VisualElement root)
        {
            headline = root.Q<Label>("development-headline-value"); copy = root.Q<Label>("development-copy-value"); laneTitle = root.Q<Label>("dev-lane-title-value"); laneCopy = root.Q<Label>("dev-lane-copy-value"); card1Title = root.Q<Label>("dev-lane-card-1-title"); card1Value = root.Q<Label>("dev-lane-card-1-value"); card2Title = root.Q<Label>("dev-lane-card-2-title"); card2Value = root.Q<Label>("dev-lane-card-2-value"); card3Title = root.Q<Label>("dev-lane-card-3-title"); card3Value = root.Q<Label>("dev-lane-card-3-value");
        }

        public void Render(ShellSummarySnapshot s)
        {
            headline.text = s.HasCity ? $"{s.City.Name} city desk" : "City desk unavailable";
            copy.text = s.HasCity ? $"Tier {(s.City.Tier ?? 0)} • Lane {s.City.SettlementLaneLabel}" : "Create a settlement to unlock city systems.";
            laneTitle.text = "City overview"; laneCopy.text = "Read-only strike desk.";
            card1Title.text = "Missions"; card1Value.text = s.ActiveMissions.Count == 0 ? "No active missions." : string.Join(" • ", s.ActiveMissions.Take(2).Select(m => BuildMissionDeskSummary(m, s.Armies, s.Heroes, includeTimer: false)));
            card2Title.text = "Heroes"; card2Value.text = s.Heroes.Count == 0 ? "No heroes surfaced." : $"{s.Heroes.Count} total • {s.Heroes.Count(h => h.Status == "idle")} idle";
            card3Title.text = "Warfront"; card3Value.text = s.WarfrontSignals.Count == 0 ? "No warfront snapshot." : string.Join(" • ", s.WarfrontSignals.Take(2).Select(x => $"{x.Label}: {x.Value}"));
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

    }
}
