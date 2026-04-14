using PlanarWar.Client.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace PlanarWar.Client.UI.Screens.BlackMarket
{
    public sealed class BlackMarketScreenController
    {
        private readonly Label headline;
        private readonly Label copy;
        private readonly Label note;
        private readonly Label cardsCopy;
        private readonly Label windowsValue;
        private readonly Label readinessValue;
        private readonly Label signalValue;
        private readonly Label missionValue;
        private readonly Label pressureValue;
        private readonly Label noteValue;
        private readonly InfoCard[] cards;

        public BlackMarketScreenController(VisualElement root)
        {
            headline = root.Q<Label>("placeholder-headline-value");
            copy = root.Q<Label>("placeholder-copy-value");
            note = root.Q<Label>("placeholder-note-value");
            cardsCopy = root.Q<Label>("warfront-cards-copy-value");
            windowsValue = root.Q<Label>("warfront-open-windows-value");
            readinessValue = root.Q<Label>("warfront-readiness-value");
            signalValue = root.Q<Label>("warfront-signal-value");
            missionValue = root.Q<Label>("warfront-mission-value");
            pressureValue = root.Q<Label>("warfront-pressure-value");
            noteValue = root.Q<Label>("warfront-note-value");
            cards = Enumerable.Range(1, 4).Select(i => new InfoCard(root, $"warfront-card-{i}")).ToArray();
        }

        public void Render(ShellSummarySnapshot s)
        {
            var warfrontTimers = s.CityTimers
                .Where(t => t.Category != null && t.Category.IndexOf("warfront", StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
            var warfrontWindows = warfrontTimers
                .Where(t => string.Equals(t.Category, "warfront_window", StringComparison.OrdinalIgnoreCase))
                .OrderBy(t => t.FinishesAtUtc ?? DateTime.MaxValue)
                .ToList();
            var warfrontOps = warfrontTimers
                .Where(t => !string.Equals(t.Category, "warfront_window", StringComparison.OrdinalIgnoreCase))
                .OrderBy(t => t.FinishesAtUtc ?? DateTime.MaxValue)
                .ToList();
            var readyArmies = s.Armies.Where(a => (a.Readiness ?? 0) >= 70).ToList();
            var activeMission = s.ActiveMissions.FirstOrDefault();
            var primaryWarning = s.ThreatWarnings.FirstOrDefault()?.Headline;
            var signalPairs = s.WarfrontSignals.Take(3).ToList();

            headline.text = warfrontWindows.Count > 0
                ? "Warfront desk"
                : s.WarfrontSignals.Count > 0
                    ? "Warfront snapshot"
                    : "Warfront review";

            copy.text = warfrontWindows.Count > 0
                ? $"{warfrontWindows.Count} live front window(s) are open. Review readiness, timers, and support posture before committing later action wiring."
                : s.WarfrontSignals.Count > 0
                    ? "Warfront status is visible even without an open front window. This desk stays read-only and shows the current field posture from the summary payload."
                    : "No active warfront snapshot is visible in the current payload. The desk stays honest instead of inventing fronts.";

            note.text = $"Lane {s.City.SettlementLaneLabel} • windows {warfrontWindows.Count} • field timers {warfrontTimers.Count} • signals {s.WarfrontSignals.Count}";
            cardsCopy.text = warfrontWindows.Count > 0
                ? $"Showing {warfrontWindows.Count} active front window(s), {warfrontOps.Count} support timer(s), and current field posture from /api/me."
                : warfrontOps.Count > 0
                    ? $"No open front windows, but {warfrontOps.Count} field timer(s) are still visible in the summary payload."
                    : "No active front windows are visible right now.";

            windowsValue.text = warfrontWindows.Count > 0
                ? $"{warfrontWindows.Count} open • {string.Join(" • ", warfrontWindows.Take(2).Select(w => HumanizeStatus(w.Status)))}"
                : "No active front window.";
            readinessValue.text = s.Armies.Count > 0
                ? $"{readyArmies.Count}/{s.Armies.Count} formation(s) ready"
                : "No formations visible in payload.";
            signalValue.text = signalPairs.Count > 0
                ? string.Join(" • ", signalPairs.Select(FormatSignal))
                : "No warfront status signals.";
            missionValue.text = activeMission != null
                ? $"{activeMission.Title} • {(activeMission.FinishesAtUtc.HasValue ? FormatRemaining(activeMission.FinishesAtUtc.Value - DateTime.UtcNow) : "anchor missing")}"
                : "No active field support mission.";
            pressureValue.text = !string.IsNullOrWhiteSpace(primaryWarning)
                ? primaryWarning
                : warfrontWindows.Count > 0
                    ? "Field windows are open but no extra warning headline is active."
                    : "No extra frontline warning surfaced.";
            noteValue.text = warfrontWindows.Count > 0
                ? "Read-only desk: fronts, signals, and readiness are live; attack/commit actions are intentionally deferred."
                : "Read-only desk: this surface stays honest when the summary payload does not expose a live front window.";

            RenderCards(cards, BuildCards(warfrontWindows, warfrontOps, activeMission, primaryWarning, signalPairs));
        }

        private static List<CardView> BuildCards(List<CityTimerEntrySnapshot> windows, List<CityTimerEntrySnapshot> ops, MissionSnapshot activeMission, string primaryWarning, List<WarfrontSignalSnapshot> signalPairs)
        {
            var cards = new List<CardView>();

            cards.AddRange(windows.Take(2).Select(timer => new CardView(
                family: "Warfront window",
                title: timer.Label,
                lore: $"{HumanizeStatus(timer.Status)} • {FormatRemaining(timer.FinishesAtUtc.HasValue ? timer.FinishesAtUtc.Value - DateTime.UtcNow : (TimeSpan?)null)}",
                note: FirstNonBlank(timer.Detail, "Live warfront window surfaced from cityTimers."))));

            cards.AddRange(ops.Take(Math.Max(0, 3 - cards.Count)).Select(timer => new CardView(
                family: HumanizeCategory(timer.Category),
                title: timer.Label,
                lore: timer.FinishesAtUtc.HasValue ? $"{HumanizeStatus(timer.Status)} • {FormatRemaining(timer.FinishesAtUtc.Value - DateTime.UtcNow)}" : HumanizeStatus(timer.Status),
                note: FirstNonBlank(timer.Detail, "Field timer surfaced from cityTimers."))));

            if (cards.Count < 4 && activeMission != null)
            {
                cards.Add(new CardView(
                    family: "Support mission",
                    title: activeMission.Title,
                    lore: activeMission.FinishesAtUtc.HasValue ? $"Mission resolves in {FormatRemaining(activeMission.FinishesAtUtc.Value - DateTime.UtcNow)}" : "Mission is live without a finish anchor.",
                    note: FirstNonBlank(primaryWarning, "Active mission pressure remains part of the current warfront posture.")));
            }

            if (cards.Count < 4 && signalPairs.Count > 0)
            {
                cards.Add(new CardView(
                    family: "Signal posture",
                    title: signalPairs[0].Label,
                    lore: signalPairs[0].Value,
                    note: signalPairs.Count > 1 ? string.Join(" • ", signalPairs.Skip(1).Select(FormatSignal)) : "Warfront status comes through the summary signal map."));
            }

            if (cards.Count == 0)
            {
                cards.Add(new CardView(
                    family: "Warfront payload",
                    title: "No warfront entry",
                    lore: "No active front window, field timer, or warfront signal is currently visible in the summary payload.",
                    note: "The desk stays honest instead of inventing a fake frontline."));
            }

            return cards;
        }

        private static void RenderCards(InfoCard[] slots, List<CardView> cards)
        {
            for (var i = 0; i < slots.Length; i++)
            {
                if (i < cards.Count)
                {
                    slots[i].Show(cards[i]);
                }
                else
                {
                    slots[i].Hide();
                }
            }
        }

        private static string HumanizeStatus(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "unknown";
            return value.Replace('_', ' ');
        }

        private static string HumanizeCategory(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return "Field timer";
            return string.Join(" ", value.Split('_').Where(part => !string.IsNullOrWhiteSpace(part)).Select(part => part.Length == 1 ? char.ToUpperInvariant(part[0]).ToString() : char.ToUpperInvariant(part[0]) + part.Substring(1)));
        }

        private static string FormatSignal(WarfrontSignalSnapshot signal) => $"{signal.Label}: {signal.Value}";

        private static string FirstNonBlank(params string[] values) => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? string.Empty;

        private static string FormatRemaining(TimeSpan? span)
        {
            if (!span.HasValue) return "time unknown";
            if (span.Value <= TimeSpan.Zero) return "now";
            return span.Value.ToString(span.Value.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss");
        }

        private sealed class InfoCard
        {
            private readonly VisualElement root;
            private readonly Label family;
            private readonly Label title;
            private readonly Label lore;
            private readonly Label note;

            public InfoCard(VisualElement root, string prefix)
            {
                this.root = root.Q<VisualElement>($"{prefix}-root");
                family = root.Q<Label>($"{prefix}-title");
                title = root.Q<Label>($"{prefix}-title-value");
                lore = root.Q<Label>($"{prefix}-lore-value");
                note = root.Q<Label>($"{prefix}-note-value");
            }

            public void Show(CardView card)
            {
                if (root != null) root.style.display = DisplayStyle.Flex;
                if (family != null) family.text = card.Family;
                if (title != null) title.text = card.Title;
                if (lore != null) lore.text = card.Lore;
                if (note != null) note.text = card.Note;
            }

            public void Hide()
            {
                if (root != null) root.style.display = DisplayStyle.None;
            }
        }

        private sealed class CardView
        {
            public string Family { get; }
            public string Title { get; }
            public string Lore { get; }
            public string Note { get; }

            public CardView(string family, string title, string lore, string note)
            {
                Family = family;
                Title = title;
                Lore = lore;
                Note = note;
            }
        }
    }
}
