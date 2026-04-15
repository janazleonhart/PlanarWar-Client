using PlanarWar.Client.Core;
using PlanarWar.Client.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace PlanarWar.Client.UI.Screens.BlackMarket
{
    public sealed class BlackMarketScreenController
    {
        private readonly SummaryState summaryState;
        private readonly Func<OperationSnapshot, System.Threading.Tasks.Task> onStartMissionRequested;
        private readonly Func<MissionSnapshot, System.Threading.Tasks.Task> onCompleteMissionRequested;
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

        public BlackMarketScreenController(VisualElement root, SummaryState summaryState, Func<OperationSnapshot, System.Threading.Tasks.Task> onStartMissionRequested, Func<MissionSnapshot, System.Threading.Tasks.Task> onCompleteMissionRequested)
        {
            this.summaryState = summaryState;
            this.onStartMissionRequested = onStartMissionRequested;
            this.onCompleteMissionRequested = onCompleteMissionRequested;
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
            var nowUtc = DateTime.UtcNow;
            var warfrontTimers = s.CityTimers
                .Where(t => t.Category != null && t.Category.IndexOf("warfront", StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(t => t.FinishesAtUtc ?? DateTime.MaxValue)
                .ToList();
            var warfrontWindows = warfrontTimers
                .Where(t => string.Equals(t.Category, "warfront_window", StringComparison.OrdinalIgnoreCase))
                .ToList();
            var warfrontOps = warfrontTimers
                .Where(t => !string.Equals(t.Category, "warfront_window", StringComparison.OrdinalIgnoreCase))
                .ToList();
            var readyArmies = s.Armies.Where(a => (a.Readiness ?? 0) >= 70).ToList();
            var activeMission = s.ActiveMissions.FirstOrDefault();
            var missionOperations = s.OpeningOperations
                .Where(IsMissionOperation)
                .ToList();
            var launchableMissionOperations = missionOperations
                .Where(op => string.Equals(op.Readiness, "ready_now", StringComparison.OrdinalIgnoreCase))
                .ToList();
            var primaryWarning = s.ThreatWarnings.FirstOrDefault()?.Headline;
            var signalPairs = s.WarfrontSignals.Take(2).ToList();
            var canLaunchFromDesk = activeMission == null && launchableMissionOperations.Count > 0;

            headline.text = activeMission != null
                ? "Warfront desk"
                : canLaunchFromDesk
                    ? "Warfront command desk"
                    : warfrontWindows.Count > 0
                        ? "Warfront desk"
                        : s.WarfrontSignals.Count > 0
                            ? "Warfront snapshot"
                            : "Warfront review";

            copy.text = activeMission != null
                ? "Active field mission, timers, and support posture are live here."
                : canLaunchFromDesk
                    ? $"{launchableMissionOperations.Count} launch-ready mission card(s) surfaced from the summary opening advisor."
                    : warfrontWindows.Count > 0
                        ? $"{warfrontWindows.Count} live front window(s) open. Review readiness, timers, and support posture."
                        : s.WarfrontSignals.Count > 0
                            ? "Field posture is visible from the current summary payload even without a launch-ready mission card."
                            : "No active warfront snapshot is visible in the current payload.";

            note.text = $"Lane {s.City.SettlementLaneLabel} • windows {warfrontWindows.Count} • timers {warfrontTimers.Count} • signals {s.WarfrontSignals.Count}";
            cardsCopy.text = canLaunchFromDesk
                ? $"Showing {launchableMissionOperations.Count} launch-ready mission card(s) plus {warfrontTimers.Count} field timer(s)."
                : warfrontWindows.Count > 0
                    ? $"Showing {warfrontWindows.Count} front window(s) and {warfrontOps.Count} field timer(s)."
                    : warfrontOps.Count > 0
                        ? $"{warfrontOps.Count} field timer(s) visible without an open front window."
                        : missionOperations.Count > 0
                            ? $"{missionOperations.Count} mission lead(s) surfaced from the opening advisor."
                            : "No active front windows are visible right now.";

            windowsValue.text = warfrontWindows.Count > 0
                ? $"{warfrontWindows.Count} open • {string.Join(" • ", warfrontWindows.Take(2).Select(w => HumanizeStatus(w.Status)))}"
                : canLaunchFromDesk
                    ? "No open front window • mission launch still available"
                    : "No active front window.";
            readinessValue.text = s.Armies.Count > 0
                ? $"{readyArmies.Count}/{s.Armies.Count} formation(s) ready"
                : canLaunchFromDesk
                    ? "Mission launch surfaced without visible formations in payload."
                    : "No formations visible in payload.";
            signalValue.text = signalPairs.Count > 0
                ? CompactSignalSummary(signalPairs)
                : "No warfront status signals.";
            missionValue.text = activeMission != null
                ? $"{activeMission.Title} • {(activeMission.FinishesAtUtc.HasValue ? FormatRemaining(activeMission.FinishesAtUtc.Value - nowUtc) : "anchor missing")}"
                : canLaunchFromDesk
                    ? BuildMissionLeadSummary(launchableMissionOperations[0])
                    : missionOperations.Count > 0
                        ? BuildMissionLeadSummary(missionOperations[0])
                        : "No active field support mission.";
            pressureValue.text = !string.IsNullOrWhiteSpace(primaryWarning)
                ? Truncate(primaryWarning, 96)
                : canLaunchFromDesk
                    ? Truncate(FirstNonBlank(launchableMissionOperations[0].WhyNow, launchableMissionOperations[0].Summary, "Mission board pressure is surfaced through the opening advisor."), 96)
                    : warfrontWindows.Count > 0
                        ? "Field windows are open; no extra warning headline is active."
                        : "No extra frontline warning surfaced.";
            noteValue.text = !string.IsNullOrWhiteSpace(summaryState?.ActionStatus)
                ? summaryState.ActionStatus
                : activeMission != null
                    ? "Live mission timing is visible here. Additional launch orders stay parked until the current field run clears."
                    : canLaunchFromDesk
                        ? "Mission start wiring is live for launch-ready opening operations."
                        : missionOperations.Count > 0
                            ? "Mission leads are visible, but this desk only arms launch buttons for ready-now operations."
                            : warfrontWindows.Count > 0
                                ? "Read-only desk: fronts, signals, and readiness are live."
                                : "Read-only desk: this surface stays honest when no live front window is exposed.";

            RenderCards(cards, BuildCards(warfrontWindows, warfrontOps, missionOperations, activeMission, primaryWarning, signalPairs, canLaunchFromDesk));
        }

        private List<CardView> BuildCards(
            List<CityTimerEntrySnapshot> windows,
            List<CityTimerEntrySnapshot> ops,
            List<OperationSnapshot> missionOperations,
            MissionSnapshot activeMission,
            string primaryWarning,
            List<WarfrontSignalSnapshot> signalPairs,
            bool canLaunchFromDesk)
        {
            var cards = new List<CardView>();

            if (activeMission != null)
            {
                cards.Add(BuildActiveMissionCard(activeMission, primaryWarning));
            }
            else if (canLaunchFromDesk)
            {
                cards.AddRange(missionOperations
                    .Where(op => string.Equals(op.Readiness, "ready_now", StringComparison.OrdinalIgnoreCase))
                    .Take(2)
                    .Select(BuildMissionCard));
            }

            cards.AddRange(windows.Take(Math.Max(0, 3 - cards.Count)).Select(timer => new CardView(
                family: "Warfront window",
                title: timer.Label,
                lore: $"{HumanizeStatus(timer.Status)} • {FormatRemaining(timer.FinishesAtUtc.HasValue ? timer.FinishesAtUtc.Value - DateTime.UtcNow : (TimeSpan?)null)}",
                note: Truncate(FirstNonBlank(timer.Detail, "Live warfront window surfaced from cityTimers."), 96))));

            cards.AddRange(ops.Take(Math.Max(0, 3 - cards.Count)).Select(timer => new CardView(
                family: HumanizeCategory(timer.Category),
                title: timer.Label,
                lore: timer.FinishesAtUtc.HasValue ? $"{HumanizeStatus(timer.Status)} • {FormatRemaining(timer.FinishesAtUtc.Value - DateTime.UtcNow)}" : HumanizeStatus(timer.Status),
                note: Truncate(FirstNonBlank(timer.Detail, "Field timer surfaced from cityTimers."), 96))));

            if (cards.Count < 4 && activeMission == null && missionOperations.Count > 0)
            {
                var readyLead = missionOperations.FirstOrDefault(op => string.Equals(op.Readiness, "ready_now", StringComparison.OrdinalIgnoreCase));
                cards.Add(BuildMissionCard(readyLead ?? missionOperations[0]));
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
                    lore: "No active front window, field timer, mission lead, or warfront signal is currently visible in the summary payload.",
                    note: "The desk stays honest instead of inventing a fake frontline."));
            }

            return cards.Take(4).ToList();
        }

        private CardView BuildActiveMissionCard(MissionSnapshot activeMission, string primaryWarning)
        {
            var isReadyToComplete = activeMission?.FinishesAtUtc.HasValue == true && activeMission.FinishesAtUtc.Value <= DateTime.UtcNow;
            var isPendingComplete = summaryState?.IsActionBusy == true && string.Equals(summaryState.PendingMissionInstanceId, activeMission?.InstanceId, StringComparison.OrdinalIgnoreCase);
            var canComplete = isReadyToComplete && !isPendingComplete && !string.IsNullOrWhiteSpace(activeMission?.InstanceId) && onCompleteMissionRequested != null && !(summaryState?.IsActionBusy ?? false);

            return new CardView(
                family: "Active mission",
                title: activeMission?.Title ?? "Field mission",
                lore: activeMission?.FinishesAtUtc.HasValue == true
                    ? (isReadyToComplete
                        ? "Mission resolve timer elapsed. Complete the field run to clear it from the desk."
                        : $"Mission resolves in {FormatRemaining(activeMission.FinishesAtUtc.Value - DateTime.UtcNow)}")
                    : "Mission is live without a finish anchor.",
                note: Truncate(FirstNonBlank(primaryWarning, isReadyToComplete ? "Field run is ready to complete." : "Additional launch orders stay parked until the current field run clears."), 96),
                buttonText: isReadyToComplete ? (isPendingComplete ? "Completing..." : "Complete mission") : null,
                buttonEnabled: canComplete,
                onClick: () => TriggerCompleteMission(activeMission),
                statusText: isReadyToComplete ? null : "Mission live");
        }

        private CardView BuildMissionCard(OperationSnapshot operation)
        {
            var isReady = string.Equals(operation?.Readiness, "ready_now", StringComparison.OrdinalIgnoreCase);
            var isPendingThisMission = summaryState.IsActionBusy && string.Equals(summaryState.PendingMissionId, operation?.Action?.MissionId, StringComparison.OrdinalIgnoreCase);
            var buttonText = isPendingThisMission
                ? "Launching..."
                : isReady
                    ? (!string.IsNullOrWhiteSpace(operation?.CtaLabel) ? operation.CtaLabel : "Launch mission")
                    : null;
            var note = FirstNonBlank(
                operation?.WhyNow,
                operation?.Summary,
                operation?.Payoff,
                operation?.Risk,
                operation?.ImpactPreview?.FirstOrDefault(),
                "Mission lead surfaced from the settlement opening advisor.");

            return new CardView(
                family: !string.IsNullOrWhiteSpace(operation?.FocusLabel) ? operation.FocusLabel : "Mission lead",
                title: operation?.Title ?? "Mission lead",
                lore: FirstNonBlank(operation?.Summary, operation?.Detail, operation?.Payoff, "Mission lead surfaced without extra summary text."),
                note: Truncate(note, 96),
                buttonText: buttonText,
                buttonEnabled: isReady && !summaryState.IsActionBusy && onStartMissionRequested != null && !string.IsNullOrWhiteSpace(operation?.Action?.MissionId),
                onClick: () => TriggerStartMission(operation));
        }

        private void TriggerStartMission(OperationSnapshot operation)
        {
            if (summaryState == null || summaryState.IsActionBusy || onStartMissionRequested == null || !IsMissionOperation(operation) || string.IsNullOrWhiteSpace(operation.Action?.MissionId))
            {
                return;
            }

            _ = onStartMissionRequested.Invoke(operation);
        }

        private void TriggerCompleteMission(MissionSnapshot mission)
        {
            if (summaryState == null || summaryState.IsActionBusy || onCompleteMissionRequested == null || mission == null || string.IsNullOrWhiteSpace(mission.InstanceId))
            {
                return;
            }

            _ = onCompleteMissionRequested.Invoke(mission);
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

        private static bool IsMissionOperation(OperationSnapshot operation)
        {
            return operation != null
                && string.Equals(operation.Action?.Kind, "start_mission", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(operation.Action?.MissionId);
        }

        private static string BuildMissionLeadSummary(OperationSnapshot operation)
        {
            if (operation == null)
            {
                return "No active field support mission.";
            }

            var readiness = HumanizeStatus(operation.Readiness);
            return $"{operation.Title} • {readiness}";
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

        private static string CompactSignalSummary(List<WarfrontSignalSnapshot> signals)
        {
            if (signals == null || signals.Count == 0)
            {
                return "No warfront status signals.";
            }

            var first = signals[0];
            if (signals.Count == 1)
            {
                return FormatSignal(first);
            }

            return $"{FormatSignal(first)} • +{signals.Count - 1} more";
        }

        private static string FirstNonBlank(params string[] values) => values.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v)) ?? string.Empty;

        private static string Truncate(string value, int maxLen)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLen)
            {
                return value ?? string.Empty;
            }

            return value.Substring(0, Math.Max(0, maxLen - 1)).TrimEnd() + "…";
        }

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
            private readonly Button button;
            private readonly Label status;

            public InfoCard(VisualElement root, string prefix)
            {
                this.root = root.Q<VisualElement>($"{prefix}-root");
                family = root.Q<Label>($"{prefix}-title");
                title = root.Q<Label>($"{prefix}-title-value");
                lore = root.Q<Label>($"{prefix}-lore-value");
                note = root.Q<Label>($"{prefix}-note-value");
                button = root.Q<Button>($"{prefix}-button");
                status = root.Q<Label>($"{prefix}-status");
            }

            public void Show(CardView card)
            {
                if (root != null) root.style.display = DisplayStyle.Flex;
                if (family != null) family.text = card.Family;
                if (title != null) title.text = card.Title;
                if (lore != null) lore.text = card.Lore;
                if (note != null) note.text = card.Note;
                if (status != null)
                {
                    if (card.HasStatus)
                    {
                        status.style.display = DisplayStyle.Flex;
                        status.text = card.StatusText;
                    }
                    else
                    {
                        status.style.display = DisplayStyle.None;
                        status.text = string.Empty;
                    }
                }

                if (button != null)
                {
                    if (card.HasButton)
                    {
                        button.style.display = DisplayStyle.Flex;
                        button.text = card.ButtonText;
                        button.SetEnabled(card.ButtonEnabled);
                        button.clickable = new Clickable(card.OnClick ?? (() => { }));
                    }
                    else
                    {
                        button.style.display = DisplayStyle.None;
                        button.text = string.Empty;
                        button.SetEnabled(false);
                        button.clickable = null;
                    }
                }
            }

            public void Hide()
            {
                if (root != null) root.style.display = DisplayStyle.None;
                if (status != null)
                {
                    status.style.display = DisplayStyle.None;
                    status.text = string.Empty;
                }

                if (button != null)
                {
                    button.style.display = DisplayStyle.None;
                    button.SetEnabled(false);
                    button.clickable = null;
                }
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
            public string StatusText { get; }
            public bool HasButton => !string.IsNullOrWhiteSpace(ButtonText);
            public bool HasStatus => !string.IsNullOrWhiteSpace(StatusText);

            public CardView(string family, string title, string lore, string note, string buttonText = null, bool buttonEnabled = false, Action onClick = null, string statusText = null)
            {
                Family = family;
                Title = title;
                Lore = lore;
                Note = note;
                ButtonText = buttonText;
                ButtonEnabled = buttonEnabled;
                OnClick = onClick;
                StatusText = statusText;
            }
        }
    }
}
