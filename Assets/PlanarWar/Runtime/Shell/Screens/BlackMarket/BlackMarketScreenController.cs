using PlanarWar.Client.Core;
using PlanarWar.Client.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly SummaryState summaryState;
        private readonly Func<string, Task> onReinforceArmyRequested;

        public BlackMarketScreenController(VisualElement root, SummaryState summaryState, Func<string, Task> onReinforceArmyRequested)
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
            cards = Enumerable.Range(1, 4).Select(i => new InfoCard(root, $"warfront-card-{i}", hasButton: true)).ToArray();
            this.summaryState = summaryState;
            this.onReinforceArmyRequested = onReinforceArmyRequested;
        }

        public void Render(ShellSummarySnapshot s)
        {
            var nowUtc = DateTime.UtcNow;
            var warfrontTimers = s.CityTimers
                .Where(t => t.Category != null && (t.Category.IndexOf("warfront", StringComparison.OrdinalIgnoreCase) >= 0 || string.Equals(t.Category, "army_reinforcement", StringComparison.OrdinalIgnoreCase)))
                .OrderBy(t => t.FinishesAtUtc ?? DateTime.MaxValue)
                .ToList();
            var warfrontWindows = warfrontTimers.Where(t => string.Equals(t.Category, "warfront_window", StringComparison.OrdinalIgnoreCase)).ToList();
            var otherWarfrontTimers = warfrontTimers.Where(t => !string.Equals(t.Category, "warfront_window", StringComparison.OrdinalIgnoreCase)).ToList();
            var reinforceTimer = otherWarfrontTimers.FirstOrDefault(t => string.Equals(t.Category, "army_reinforcement", StringComparison.OrdinalIgnoreCase));
            var reinforceOp = s.OpeningOperations.FirstOrDefault(o => string.Equals(o.Kind, "reinforce_army", StringComparison.OrdinalIgnoreCase) && string.Equals(o.Readiness, "ready_now", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(o.ArmyId));
            var readyArmies = s.Armies.Where(a => (a.Readiness ?? 0) >= 70).ToList();
            var activeMission = s.ActiveMissions.FirstOrDefault();
            var primaryWarning = s.ThreatWarnings.FirstOrDefault()?.Headline;
            var signalPairs = s.WarfrontSignals.Take(2).ToList();

            headline.text = warfrontWindows.Count > 0 ? "Warfront desk" : s.WarfrontSignals.Count > 0 ? "Warfront snapshot" : "Warfront review";
            copy.text = warfrontWindows.Count > 0
                ? $"{warfrontWindows.Count} live front window(s) open. Review readiness, timers, and support posture."
                : s.WarfrontSignals.Count > 0
                    ? "Warfront posture is visible from the current summary payload."
                    : "No active warfront snapshot is visible in the current payload.";
            note.text = $"Lane {s.City.SettlementLaneLabel} • windows {warfrontWindows.Count} • timers {warfrontTimers.Count} • signals {s.WarfrontSignals.Count}";
            cardsCopy.text = reinforceTimer != null
                ? $"Army reinforcement is live alongside {warfrontWindows.Count} front window(s)."
                : reinforceOp != null
                    ? $"Reinforce {ResolveArmyName(s, reinforceOp.ArmyId)} is ready now."
                    : warfrontWindows.Count > 0
                        ? $"Showing {warfrontWindows.Count} front window(s) and {otherWarfrontTimers.Count} field timer(s)."
                        : otherWarfrontTimers.Count > 0
                            ? $"{otherWarfrontTimers.Count} field timer(s) visible without an open front window."
                            : "No active front windows are visible right now.";

            windowsValue.text = warfrontWindows.Count > 0 ? $"{warfrontWindows.Count} open • {string.Join(" • ", warfrontWindows.Take(2).Select(w => HumanizeStatus(w.Status)))}" : "No active front window.";
            readinessValue.text = s.Armies.Count > 0 ? $"{readyArmies.Count}/{s.Armies.Count} formation(s) ready" : "No formations visible in payload.";
            signalValue.text = signalPairs.Count > 0 ? CompactSignalSummary(signalPairs) : "No warfront status signals.";
            missionValue.text = activeMission != null ? $"{activeMission.Title} • {(activeMission.FinishesAtUtc.HasValue ? FormatRemaining(activeMission.FinishesAtUtc.Value - nowUtc) : "anchor missing")}" : "No active field support mission.";
            pressureValue.text = !string.IsNullOrWhiteSpace(primaryWarning) ? Truncate(primaryWarning, 96) : warfrontWindows.Count > 0 ? "Field windows are open; no extra warning headline is active." : "No extra frontline warning surfaced.";
            noteValue.text = reinforceTimer != null
                ? "Army reinforcement timer is live here. Fresh reinforce orders stay parked until the current build clears."
                : reinforceOp != null
                    ? "A real reinforce-army opening is visible from settlementOpeningOperations."
                    : warfrontWindows.Count > 0
                        ? "Warfront truth is live here. Reinforce buttons only appear when the payload exposes a ready-now order."
                        : "No reinforce order is currently exposed in the payload.";

            RenderCards(cards, BuildCards(s, warfrontWindows, otherWarfrontTimers, activeMission, primaryWarning, signalPairs, reinforceTimer, reinforceOp));
        }

        private List<CardView> BuildCards(ShellSummarySnapshot s, List<CityTimerEntrySnapshot> windows, List<CityTimerEntrySnapshot> timers, MissionSnapshot activeMission, string primaryWarning, List<WarfrontSignalSnapshot> signalPairs, CityTimerEntrySnapshot reinforceTimer, OperationSnapshot reinforceOp)
        {
            var cards = new List<CardView>();
            var nowUtc = DateTime.UtcNow;

            if (reinforceTimer != null)
            {
                cards.Add(new CardView(
                    family: "Army reinforcement",
                    title: reinforceTimer.Label,
                    lore: reinforceTimer.FinishesAtUtc.HasValue ? $"active • {FormatRemaining(reinforceTimer.FinishesAtUtc.Value - nowUtc)}" : "active",
                    note: Truncate(FirstNonBlank(reinforceTimer.Detail, "Army reinforcement timer surfaced from /api/me."), 96),
                    buttonText: summaryState.IsActionBusy && !string.IsNullOrWhiteSpace(summaryState.PendingArmyReinforcementId) ? "Reinforcing..." : "Reinforcing",
                    buttonEnabled: false));
            }
            else if (reinforceOp != null)
            {
                cards.Add(new CardView(
                    family: "Army reinforce",
                    title: $"Reinforce {ResolveArmyName(s, reinforceOp.ArmyId)}",
                    lore: "Warfront support order is ready now.",
                    note: Truncate(FirstNonBlank(reinforceOp.Title, "A live reinforce order is visible from settlementOpeningOperations."), 96),
                    buttonText: summaryState.IsActionBusy && string.Equals(summaryState.PendingArmyReinforcementId, reinforceOp.ArmyId, StringComparison.OrdinalIgnoreCase) ? "Reinforcing..." : "Reinforce army",
                    buttonEnabled: !summaryState.IsActionBusy && onReinforceArmyRequested != null,
                    onClick: () => TriggerReinforceArmy(reinforceOp.ArmyId)));
            }

            cards.AddRange(windows.Take(Math.Max(0, 2 - cards.Count)).Select(timer => new CardView(
                family: "Warfront window",
                title: timer.Label,
                lore: $"{HumanizeStatus(timer.Status)} • {FormatRemaining(timer.FinishesAtUtc.HasValue ? timer.FinishesAtUtc.Value - nowUtc : (TimeSpan?)null)}",
                note: Truncate(FirstNonBlank(timer.Detail, "Live warfront window surfaced from cityTimers."), 96))));

            if (cards.Count < 4 && activeMission != null)
            {
                cards.Add(new CardView(
                    family: "Support mission",
                    title: activeMission.Title,
                    lore: activeMission.FinishesAtUtc.HasValue ? $"Mission resolves in {FormatRemaining(activeMission.FinishesAtUtc.Value - nowUtc)}" : "Mission is live without a finish anchor.",
                    note: Truncate(FirstNonBlank(primaryWarning, "Active mission pressure remains part of the current warfront posture."), 96)));
            }

            if (cards.Count < 4 && signalPairs.Count > 0)
            {
                cards.Add(new CardView(
                    family: "Signal posture",
                    title: signalPairs[0].Label,
                    lore: Truncate(signalPairs[0].Value, 72),
                    note: signalPairs.Count > 1 ? Truncate(string.Join(" • ", signalPairs.Skip(1).Select(FormatSignal)), 96) : "Warfront status comes through the summary signal map."));
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

        private void TriggerReinforceArmy(string armyId)
        {
            if (summaryState.IsActionBusy || onReinforceArmyRequested == null || string.IsNullOrWhiteSpace(armyId))
            {
                return;
            }

            _ = onReinforceArmyRequested.Invoke(armyId.Trim());
        }

        private static string ResolveArmyName(ShellSummarySnapshot s, string armyId)
        {
            if (string.IsNullOrWhiteSpace(armyId)) return "army";
            return s.Armies.FirstOrDefault(a => string.Equals(a.Id, armyId, StringComparison.OrdinalIgnoreCase))?.Name ?? armyId;
        }

        private static void RenderCards(InfoCard[] slots, List<CardView> cards)
        {
            for (var i = 0; i < slots.Length; i++)
            {
                if (i < cards.Count) slots[i].Show(cards[i]);
                else slots[i].Hide();
            }
        }

        private static string HumanizeStatus(string value) => string.IsNullOrWhiteSpace(value) ? "unknown" : value.Replace('_', ' ');
        private static string FormatSignal(WarfrontSignalSnapshot signal) => $"{signal.Label}: {signal.Value}";
        private static string CompactSignalSummary(List<WarfrontSignalSnapshot> signals) => signals == null || signals.Count == 0 ? "No warfront status signals." : signals.Count == 1 ? FormatSignal(signals[0]) : $"{FormatSignal(signals[0])} • +{signals.Count - 1} more";
        private static string FirstNonBlank(params string[] values) => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? string.Empty;
        private static string Truncate(string value, int maxLen) => string.IsNullOrWhiteSpace(value) || value.Length <= maxLen ? value ?? string.Empty : value.Substring(0, Math.Max(0, maxLen - 1)).TrimEnd() + "…";
        private static string FormatRemaining(TimeSpan? span) => !span.HasValue ? "time unknown" : span.Value <= TimeSpan.Zero ? "now" : span.Value.ToString(span.Value.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss");

        private sealed class InfoCard
        {
            private readonly VisualElement root;
            private readonly Label family;
            private readonly Label title;
            private readonly Label lore;
            private readonly Label note;
            private readonly Button button;
            private Action clickAction;

            public InfoCard(VisualElement root, string prefix, bool hasButton)
            {
                this.root = root.Q<VisualElement>($"{prefix}-root");
                family = root.Q<Label>($"{prefix}-title");
                title = root.Q<Label>($"{prefix}-title-value");
                lore = root.Q<Label>($"{prefix}-lore-value");
                note = root.Q<Label>($"{prefix}-note-value");
                button = hasButton ? root.Q<Button>($"{prefix}-button") : null;

                this.root?.AddToClassList("warfront-desk-card");
                button?.AddToClassList("warfront-desk-card__button");
                if (button != null) button.clicked += () => clickAction?.Invoke();
            }

            public void Show(CardView card)
            {
                if (root != null) root.style.display = DisplayStyle.Flex;
                if (family != null) family.text = card.Family;
                if (title != null) title.text = card.Title;
                if (lore != null) lore.text = card.Lore;
                if (note != null) note.text = card.Note;
                if (button != null)
                {
                    clickAction = card.OnClick;
                    button.style.display = string.IsNullOrWhiteSpace(card.ButtonText) ? DisplayStyle.None : DisplayStyle.Flex;
                    button.text = card.ButtonText ?? "Read-only";
                    button.SetEnabled(card.ButtonEnabled && clickAction != null);
                }
            }

            public void Hide()
            {
                clickAction = null;
                if (root != null) root.style.display = DisplayStyle.None;
            }
        }

        private sealed class CardView
        {
            public string Family { get; }
            public string Title { get; }
            public string Lore { get; }
            public string Note { get; }
            public string ButtonText { get; }
            public bool ButtonEnabled { get; }
            public Action OnClick { get; }

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
        }
    }
}
