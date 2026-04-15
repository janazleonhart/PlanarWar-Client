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
        private readonly Label managementCopy;
        private readonly Label managementNote;
        private readonly DropdownField managementArmyField;
        private readonly TextField renameInput;
        private readonly Button renameButton;
        private readonly TextField splitSizeInput;
        private readonly TextField splitNameInput;
        private readonly Button splitButton;
        private readonly DropdownField mergeTargetField;
        private readonly Button mergeButton;
        private readonly Button disbandButton;
        private readonly DropdownField holdRegionField;
        private readonly DropdownField holdPostureField;
        private readonly Button assignHoldButton;
        private readonly Button releaseHoldButton;
        private readonly InfoCard[] cards;
        private readonly SummaryState summaryState;
        private readonly Func<string, Task> onReinforceArmyRequested;
        private readonly Func<string, string, Task> onRenameArmyRequested;
        private readonly Func<string, int, string, Task> onSplitArmyRequested;
        private readonly Func<string, string, Task> onMergeArmyRequested;
        private readonly Func<string, Task> onDisbandArmyRequested;
        private readonly Func<string, string, string, Task> onAssignArmyHoldRequested;
        private readonly Func<string, Task> onReleaseArmyHoldRequested;
        private readonly Action onRefreshDeskRequested;
        private readonly List<string> managementArmyChoiceIds = new();
        private readonly List<string> mergeArmyChoiceIds = new();
        private readonly List<string> holdRegionChoiceIds = new();
        private string selectedArmyId = string.Empty;
        private string draftedArmyId = string.Empty;
        private string renameDraft = string.Empty;
        private string splitSizeDraft = string.Empty;
        private string splitNameDraft = string.Empty;
        private string selectedMergeArmyId = string.Empty;
        private string selectedHoldRegionId = string.Empty;
        private string selectedHoldPosture = "frontier_hold";
        private bool suppressManagementEvents;

        public BlackMarketScreenController(VisualElement root, SummaryState summaryState, Func<string, Task> onReinforceArmyRequested, Func<string, string, Task> onRenameArmyRequested, Func<string, int, string, Task> onSplitArmyRequested, Func<string, string, Task> onMergeArmyRequested, Func<string, Task> onDisbandArmyRequested, Func<string, string, string, Task> onAssignArmyHoldRequested, Func<string, Task> onReleaseArmyHoldRequested, Action onRefreshDeskRequested)
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
            managementCopy = root.Q<Label>("warfront-management-copy-value");
            managementNote = root.Q<Label>("warfront-manage-note-value");
            managementArmyField = root.Q<DropdownField>("warfront-manage-army-field");
            renameInput = root.Q<TextField>("warfront-manage-rename-input");
            renameButton = root.Q<Button>("warfront-manage-rename-button");
            splitSizeInput = root.Q<TextField>("warfront-manage-split-size-input");
            splitNameInput = root.Q<TextField>("warfront-manage-split-name-input");
            splitButton = root.Q<Button>("warfront-manage-split-button");
            mergeTargetField = root.Q<DropdownField>("warfront-manage-merge-target-field");
            mergeButton = root.Q<Button>("warfront-manage-merge-button");
            disbandButton = root.Q<Button>("warfront-manage-disband-button");
            holdRegionField = root.Q<DropdownField>("warfront-manage-hold-region-field");
            holdPostureField = root.Q<DropdownField>("warfront-manage-hold-posture-field");
            assignHoldButton = root.Q<Button>("warfront-manage-hold-assign-button");
            releaseHoldButton = root.Q<Button>("warfront-manage-hold-release-button");
            cards = Enumerable.Range(1, 4).Select(i => new InfoCard(root, $"warfront-card-{i}", hasButton: true)).ToArray();
            this.summaryState = summaryState;
            this.onReinforceArmyRequested = onReinforceArmyRequested;
            this.onRenameArmyRequested = onRenameArmyRequested;
            this.onSplitArmyRequested = onSplitArmyRequested;
            this.onMergeArmyRequested = onMergeArmyRequested;
            this.onDisbandArmyRequested = onDisbandArmyRequested;
            this.onAssignArmyHoldRequested = onAssignArmyHoldRequested;
            this.onReleaseArmyHoldRequested = onReleaseArmyHoldRequested;
            this.onRefreshDeskRequested = onRefreshDeskRequested;

            if (managementArmyField != null)
            {
                managementArmyField.RegisterValueChangedCallback(evt =>
                {
                    if (suppressManagementEvents)
                    {
                        return;
                    }

                    var index = managementArmyField.choices?.IndexOf(evt.newValue) ?? -1;
                    if (index >= 0 && index < managementArmyChoiceIds.Count)
                    {
                        selectedArmyId = managementArmyChoiceIds[index];
                        draftedArmyId = string.Empty;
                    }
                });
            }

            mergeTargetField?.RegisterValueChangedCallback(evt =>
            {
                if (suppressManagementEvents)
                {
                    return;
                }

                var index = mergeTargetField.choices?.IndexOf(evt.newValue) ?? -1;
                if (index >= 0 && index < mergeArmyChoiceIds.Count)
                {
                    selectedMergeArmyId = mergeArmyChoiceIds[index];
                }
            });

            holdRegionField?.RegisterValueChangedCallback(evt =>
            {
                if (suppressManagementEvents)
                {
                    return;
                }

                var index = holdRegionField.choices?.IndexOf(evt.newValue) ?? -1;
                if (index >= 0 && index < holdRegionChoiceIds.Count)
                {
                    selectedHoldRegionId = holdRegionChoiceIds[index];
                }
            });

            holdPostureField?.RegisterValueChangedCallback(evt =>
            {
                if (suppressManagementEvents)
                {
                    return;
                }

                selectedHoldPosture = NormalizeHoldPostureLabel(evt.newValue);
            });

            renameInput?.RegisterValueChangedCallback(evt =>
            {
                if (!suppressManagementEvents)
                {
                    renameDraft = evt.newValue ?? string.Empty;
                }
            });

            splitSizeInput?.RegisterValueChangedCallback(evt =>
            {
                if (!suppressManagementEvents)
                {
                    splitSizeDraft = evt.newValue ?? string.Empty;
                }
            });

            splitNameInput?.RegisterValueChangedCallback(evt =>
            {
                if (!suppressManagementEvents)
                {
                    splitNameDraft = evt.newValue ?? string.Empty;
                }
            });

            renameButton?.RegisterCallback<ClickEvent>(_ => TriggerRenameArmy());
            splitButton?.RegisterCallback<ClickEvent>(_ => TriggerSplitArmy());
            mergeButton?.RegisterCallback<ClickEvent>(_ => TriggerMergeArmy());
            disbandButton?.RegisterCallback<ClickEvent>(_ => TriggerDisbandArmy());
            assignHoldButton?.RegisterCallback<ClickEvent>(_ => TriggerAssignHold());
            releaseHoldButton?.RegisterCallback<ClickEvent>(_ => TriggerReleaseHold());
        }

        public void Render(ShellSummarySnapshot summary)
        {
            var nowUtc = DateTime.UtcNow;
            var warfrontTimers = summary.CityTimers
                .Where(timer => timer.Category != null && (timer.Category.IndexOf("warfront", StringComparison.OrdinalIgnoreCase) >= 0 || string.Equals(timer.Category, "army_reinforcement", StringComparison.OrdinalIgnoreCase)))
                .OrderBy(timer => timer.FinishesAtUtc ?? DateTime.MaxValue)
                .ToList();
            var warfrontWindows = warfrontTimers.Where(timer => string.Equals(timer.Category, "warfront_window", StringComparison.OrdinalIgnoreCase)).ToList();
            var otherWarfrontTimers = warfrontTimers.Where(timer => !string.Equals(timer.Category, "warfront_window", StringComparison.OrdinalIgnoreCase)).ToList();
            var reinforceTimer = otherWarfrontTimers.FirstOrDefault(timer => string.Equals(timer.Category, "army_reinforcement", StringComparison.OrdinalIgnoreCase));
            var reinforceOp = summary.OpeningOperations.FirstOrDefault(operation => string.Equals(operation.Kind, "reinforce_army", StringComparison.OrdinalIgnoreCase) && string.Equals(operation.Readiness, "ready_now", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(operation.ArmyId));
            var reinforceState = summary.ArmyReinforcement;
            var activeMission = summary.ActiveMissions.FirstOrDefault();
            var primaryWarning = summary.ThreatWarnings.FirstOrDefault()?.Headline;
            var signalPairs = summary.WarfrontSignals.Take(2).ToList();
            var targetArmyId = FirstNonBlank(reinforceState?.ArmyId, reinforceOp?.ArmyId);
            var rankedArmies = RankArmies(summary.Armies, targetArmyId);

            headline.text = warfrontWindows.Count > 0 ? "Warfront desk" : summary.WarfrontSignals.Count > 0 ? "Warfront snapshot" : "Warfront review";
            copy.text = warfrontWindows.Count > 0
                ? $"{warfrontWindows.Count} live front window(s) open. Review formations, reinforcement truth, and field posture."
                : summary.WarfrontSignals.Count > 0
                    ? "Warfront posture is visible from the current summary payload."
                    : "No active warfront snapshot is visible in the current payload.";
            note.text = $"Lane {summary.City.SettlementLaneLabel} • windows {warfrontWindows.Count} • timers {warfrontTimers.Count} • formations {summary.Armies.Count}";
            cardsCopy.text = BuildCardsCopy(summary.Armies, reinforceState, reinforceTimer, reinforceOp, warfrontWindows.Count, otherWarfrontTimers.Count);

            windowsValue.text = warfrontWindows.Count > 0 ? $"{warfrontWindows.Count} open • {string.Join(" • ", warfrontWindows.Take(2).Select(window => HumanizeStatus(window.Status)))}" : "No active front window.";
            readinessValue.text = BuildForceReadinessSummary(summary.Armies, reinforceState, reinforceOp);
            signalValue.text = signalPairs.Count > 0 ? CompactSignalSummary(signalPairs) : "No warfront status signals.";
            missionValue.text = activeMission != null ? $"{activeMission.Title} • {(activeMission.FinishesAtUtc.HasValue ? FormatRemaining(activeMission.FinishesAtUtc.Value - nowUtc) : "anchor missing")}" : "No active field support mission.";
            pressureValue.text = !string.IsNullOrWhiteSpace(primaryWarning) ? Truncate(primaryWarning, 96) : warfrontWindows.Count > 0 ? "Field windows are open; no extra warning headline is active." : "No extra frontline warning surfaced.";
            noteValue.text = BuildReinforcementDeskNote(summary.Armies, reinforceState, reinforceTimer, reinforceOp, warfrontWindows.Count > 0);

            RenderCards(cards, BuildCards(summary, rankedArmies, warfrontWindows, activeMission, primaryWarning, signalPairs, reinforceState, reinforceTimer, reinforceOp));
            RenderFormationManagement(summary, rankedArmies, targetArmyId);
        }


        private void RenderFormationManagement(ShellSummarySnapshot summary, List<ArmySnapshot> rankedArmies, string targetArmyId)
        {
            if (managementCopy == null && managementNote == null && managementArmyField == null)
            {
                return;
            }

            var armies = (rankedArmies ?? new List<ArmySnapshot>()).Where(army => army != null).ToList();
            if (armies.Count == 0)
            {
                if (managementCopy != null) managementCopy.text = "No formation payload is visible yet, so formation controls stay parked.";
                if (managementNote != null) managementNote.text = "Warfront will unlock rename, split, merge, disband, and hold controls once at least one army is present in /api/me.";
                managementArmyChoiceIds.Clear();
                mergeArmyChoiceIds.Clear();
                holdRegionChoiceIds.Clear();
                managementArmyField?.SetEnabled(false);
                mergeTargetField?.SetEnabled(false);
                holdRegionField?.SetEnabled(false);
                holdPostureField?.SetEnabled(false);
                renameInput?.SetEnabled(false);
                splitSizeInput?.SetEnabled(false);
                splitNameInput?.SetEnabled(false);
                renameButton?.SetEnabled(false);
                splitButton?.SetEnabled(false);
                mergeButton?.SetEnabled(false);
                disbandButton?.SetEnabled(false);
                assignHoldButton?.SetEnabled(false);
                releaseHoldButton?.SetEnabled(false);
                return;
            }

            if (string.IsNullOrWhiteSpace(selectedArmyId) || armies.All(army => !string.Equals(army.Id, selectedArmyId, StringComparison.OrdinalIgnoreCase)))
            {
                selectedArmyId = FirstNonBlank(targetArmyId, armies[0].Id);
                draftedArmyId = string.Empty;
            }

            managementArmyChoiceIds.Clear();
            var choices = armies.Select(army =>
            {
                managementArmyChoiceIds.Add(army.Id);
                return BuildArmyChoiceLabel(army);
            }).ToList();
            var selectedIndex = Math.Max(0, managementArmyChoiceIds.FindIndex(id => string.Equals(id, selectedArmyId, StringComparison.OrdinalIgnoreCase)));
            if (selectedIndex >= managementArmyChoiceIds.Count)
            {
                selectedIndex = 0;
            }

            selectedArmyId = managementArmyChoiceIds[selectedIndex];
            var selectedArmy = armies[selectedIndex];
            var idle = string.Equals(selectedArmy.Status, "idle", StringComparison.OrdinalIgnoreCase);
            var holding = string.Equals(selectedArmy.Status, "holding", StringComparison.OrdinalIgnoreCase);

            if (!string.Equals(draftedArmyId, selectedArmy.Id, StringComparison.OrdinalIgnoreCase))
            {
                draftedArmyId = selectedArmy.Id;
                renameDraft = selectedArmy.Name;
                splitSizeDraft = BuildSuggestedSplitSize(selectedArmy);
                splitNameDraft = BuildSuggestedSplitName(summary.Armies, selectedArmy);
                selectedHoldRegionId = FirstNonBlank(selectedArmy.HoldRegionId, ResolveDefaultHoldRegionId());
                selectedHoldPosture = string.IsNullOrWhiteSpace(selectedArmy.HoldPosture) ? "frontier_hold" : NormalizeHoldPostureLabel(selectedArmy.HoldPosture);
            }

            if (managementArmyField != null)
            {
                suppressManagementEvents = true;
                managementArmyField.choices = choices;
                managementArmyField.SetValueWithoutNotify(choices[selectedIndex]);
                managementArmyField.SetEnabled(!summaryState.IsActionBusy);
                suppressManagementEvents = false;
            }

            var mergeCandidates = armies.Where(army => !string.Equals(army.Id, selectedArmy.Id, StringComparison.OrdinalIgnoreCase)).ToList();
            if (mergeCandidates.Count == 0)
            {
                selectedMergeArmyId = string.Empty;
            }
            else if (string.IsNullOrWhiteSpace(selectedMergeArmyId) || mergeCandidates.All(army => !string.Equals(army.Id, selectedMergeArmyId, StringComparison.OrdinalIgnoreCase)))
            {
                selectedMergeArmyId = mergeCandidates[0].Id;
            }

            if (mergeTargetField != null)
            {
                suppressManagementEvents = true;
                mergeArmyChoiceIds.Clear();
                var mergeChoices = mergeCandidates.Select(army =>
                {
                    mergeArmyChoiceIds.Add(army.Id);
                    return BuildArmyChoiceLabel(army);
                }).ToList();
                mergeTargetField.choices = mergeChoices;
                var mergeIndex = Math.Max(0, mergeArmyChoiceIds.FindIndex(id => string.Equals(id, selectedMergeArmyId, StringComparison.OrdinalIgnoreCase)));
                if (mergeIndex >= mergeArmyChoiceIds.Count)
                {
                    mergeIndex = mergeArmyChoiceIds.Count > 0 ? 0 : -1;
                }
                mergeTargetField.SetValueWithoutNotify(mergeIndex >= 0 && mergeIndex < mergeChoices.Count ? mergeChoices[mergeIndex] : string.Empty);
                mergeTargetField.SetEnabled(!summaryState.IsActionBusy && idle && mergeChoices.Count > 0);
                suppressManagementEvents = false;
            }

            var holdRegions = BuildHoldRegionOptions();
            if (holdRegions.Count == 0)
            {
                selectedHoldRegionId = string.Empty;
            }
            else if (string.IsNullOrWhiteSpace(selectedHoldRegionId) || holdRegions.All(option => !string.Equals(option.RegionId, selectedHoldRegionId, StringComparison.OrdinalIgnoreCase)))
            {
                selectedHoldRegionId = holdRegions[0].RegionId;
            }

            if (holdRegionField != null)
            {
                suppressManagementEvents = true;
                holdRegionChoiceIds.Clear();
                var regionChoices = holdRegions.Select(option =>
                {
                    holdRegionChoiceIds.Add(option.RegionId);
                    return option.Label;
                }).ToList();
                holdRegionField.choices = regionChoices;
                var regionIndex = Math.Max(0, holdRegionChoiceIds.FindIndex(id => string.Equals(id, selectedHoldRegionId, StringComparison.OrdinalIgnoreCase)));
                if (regionIndex >= holdRegionChoiceIds.Count)
                {
                    regionIndex = holdRegionChoiceIds.Count > 0 ? 0 : -1;
                }
                holdRegionField.SetValueWithoutNotify(regionIndex >= 0 && regionIndex < regionChoices.Count ? regionChoices[regionIndex] : string.Empty);
                holdRegionField.SetEnabled(!summaryState.IsActionBusy && idle && regionChoices.Count > 0);
                suppressManagementEvents = false;
            }

            if (holdPostureField != null)
            {
                suppressManagementEvents = true;
                var postureChoices = BuildHoldPostureChoices();
                holdPostureField.choices = postureChoices;
                var postureLabel = HoldPostureToLabel(selectedHoldPosture);
                holdPostureField.SetValueWithoutNotify(postureChoices.Contains(postureLabel) ? postureLabel : postureChoices.FirstOrDefault() ?? string.Empty);
                holdPostureField.SetEnabled(!summaryState.IsActionBusy && idle);
                suppressManagementEvents = false;
            }

            suppressManagementEvents = true;
            renameInput?.SetValueWithoutNotify(renameDraft);
            splitSizeInput?.SetValueWithoutNotify(splitSizeDraft);
            splitNameInput?.SetValueWithoutNotify(splitNameDraft);
            suppressManagementEvents = false;

            var splitSize = ParsePositiveInt(splitSizeDraft);
            var maxSplit = Math.Max(0, (int)Math.Round(selectedArmy.Size ?? 0) - 40);
            var canRename = !summaryState.IsActionBusy && idle && onRenameArmyRequested != null && !string.IsNullOrWhiteSpace(renameDraft?.Trim()) && !string.Equals(renameDraft.Trim(), selectedArmy.Name, StringComparison.OrdinalIgnoreCase);
            var canSplit = !summaryState.IsActionBusy && idle && onSplitArmyRequested != null && splitSize >= 40 && splitSize <= maxSplit;
            var canMerge = !summaryState.IsActionBusy && idle && onMergeArmyRequested != null && !string.IsNullOrWhiteSpace(selectedMergeArmyId);
            var canDisband = !summaryState.IsActionBusy && idle && onDisbandArmyRequested != null && armies.Count > 1;
            var canAssignHold = !summaryState.IsActionBusy && idle && onAssignArmyHoldRequested != null && !string.IsNullOrWhiteSpace(selectedHoldRegionId);
            var canReleaseHold = !summaryState.IsActionBusy && holding && onReleaseArmyHoldRequested != null;

            renameInput?.SetEnabled(!summaryState.IsActionBusy && idle);
            splitSizeInput?.SetEnabled(!summaryState.IsActionBusy && idle);
            splitNameInput?.SetEnabled(!summaryState.IsActionBusy && idle);
            if (renameButton != null)
            {
                renameButton.text = summaryState.IsActionBusy ? "Working..." : "Rename formation";
                renameButton.SetEnabled(canRename);
            }
            if (splitButton != null)
            {
                splitButton.text = summaryState.IsActionBusy ? "Working..." : "Split formation";
                splitButton.SetEnabled(canSplit);
            }
            if (mergeButton != null)
            {
                mergeButton.text = summaryState.IsActionBusy ? "Working..." : "Merge into target";
                mergeButton.SetEnabled(canMerge);
            }
            if (disbandButton != null)
            {
                disbandButton.text = summaryState.IsActionBusy ? "Working..." : "Disband formation";
                disbandButton.SetEnabled(canDisband);
            }
            if (assignHoldButton != null)
            {
                assignHoldButton.text = summaryState.IsActionBusy ? "Working..." : "Assign hold";
                assignHoldButton.SetEnabled(canAssignHold);
            }
            if (releaseHoldButton != null)
            {
                releaseHoldButton.text = summaryState.IsActionBusy ? "Working..." : "Release hold";
                releaseHoldButton.SetEnabled(canReleaseHold);
            }

            if (managementCopy != null)
            {
                managementCopy.text = $"Commander controls stay bounded here: rename, split, merge, disband, or assign an idle formation to a regional hold. Focus: {selectedArmy.Name} • {BuildFormationLore(selectedArmy)}.";
            }

            if (managementNote != null)
            {
                var mergeTarget = mergeCandidates.FirstOrDefault(army => string.Equals(army.Id, selectedMergeArmyId, StringComparison.OrdinalIgnoreCase));
                var holdLabel = holdRegions.FirstOrDefault(option => string.Equals(option.RegionId, selectedHoldRegionId, StringComparison.OrdinalIgnoreCase)).Label;
                managementNote.text = BuildFormationManagementNote(selectedArmy, splitSize, maxSplit, mergeTarget, armies.Count, selectedHoldRegionId, selectedHoldPosture, holdLabel);
            }
        }

        private void TriggerRenameArmy()
        {
            if (summaryState.IsActionBusy || onRenameArmyRequested == null || string.IsNullOrWhiteSpace(selectedArmyId))
            {
                return;
            }

            _ = onRenameArmyRequested.Invoke(selectedArmyId.Trim(), renameDraft?.Trim() ?? string.Empty);
        }

        private void TriggerSplitArmy()
        {
            if (summaryState.IsActionBusy || onSplitArmyRequested == null || string.IsNullOrWhiteSpace(selectedArmyId))
            {
                return;
            }

            var splitSize = ParsePositiveInt(splitSizeDraft);
            if (splitSize <= 0)
            {
                return;
            }

            _ = onSplitArmyRequested.Invoke(selectedArmyId.Trim(), splitSize, splitNameDraft?.Trim() ?? string.Empty);
        }

        private void TriggerMergeArmy()
        {
            if (summaryState.IsActionBusy || onMergeArmyRequested == null || string.IsNullOrWhiteSpace(selectedArmyId) || string.IsNullOrWhiteSpace(selectedMergeArmyId))
            {
                return;
            }

            _ = onMergeArmyRequested.Invoke(selectedArmyId.Trim(), selectedMergeArmyId.Trim());
        }

        private void TriggerDisbandArmy()
        {
            if (summaryState.IsActionBusy || onDisbandArmyRequested == null || string.IsNullOrWhiteSpace(selectedArmyId))
            {
                return;
            }

            _ = onDisbandArmyRequested.Invoke(selectedArmyId.Trim());
        }

        private void TriggerAssignHold()
        {
            if (summaryState.IsActionBusy || onAssignArmyHoldRequested == null || string.IsNullOrWhiteSpace(selectedArmyId) || string.IsNullOrWhiteSpace(selectedHoldRegionId))
            {
                return;
            }

            _ = onAssignArmyHoldRequested.Invoke(selectedArmyId.Trim(), selectedHoldRegionId.Trim(), selectedHoldPosture?.Trim() ?? string.Empty);
        }

        private void TriggerReleaseHold()
        {
            if (summaryState.IsActionBusy || onReleaseArmyHoldRequested == null || string.IsNullOrWhiteSpace(selectedArmyId))
            {
                return;
            }

            _ = onReleaseArmyHoldRequested.Invoke(selectedArmyId.Trim());
        }

        private List<CardView> BuildCards(ShellSummarySnapshot summary, List<ArmySnapshot> rankedArmies, List<CityTimerEntrySnapshot> windows, MissionSnapshot activeMission, string primaryWarning, List<WarfrontSignalSnapshot> signalPairs, ArmyReinforcementSnapshot reinforceState, CityTimerEntrySnapshot reinforceTimer, OperationSnapshot reinforceOp)
        {
            var cards = new List<CardView>();
            var nowUtc = DateTime.UtcNow;
            var targetArmyId = FirstNonBlank(reinforceState?.ArmyId, reinforceOp?.ArmyId);

            if (reinforceState != null)
            {
                if (string.Equals(reinforceState.Status, "reinforcing", StringComparison.OrdinalIgnoreCase))
                {
                    var reinforcingExpiredLocally = reinforceState.FinishesAtUtc.HasValue && reinforceState.FinishesAtUtc.Value <= nowUtc;
                    cards.Add(new CardView(
                        family: "Army reinforcement",
                        title: FirstNonBlank(reinforceState.ArmyName, ResolveArmyName(summary.Armies, reinforceState.ArmyId), "Army reinforcement"),
                        lore: reinforceState.FinishesAtUtc.HasValue
                            ? reinforcingExpiredLocally
                                ? "ready • refresh now"
                                : $"active • {FormatRemaining(reinforceState.FinishesAtUtc.Value - nowUtc)}"
                            : "active",
                        note: reinforcingExpiredLocally
                            ? $"{BuildArmyReinforcementActiveNote(reinforceState, reinforceTimer)} • Timer elapsed locally. Refresh to load the completed reinforcement state."
                            : BuildArmyReinforcementActiveNote(reinforceState, reinforceTimer),
                        buttonText: reinforcingExpiredLocally ? "Refresh reinforcement" : (summaryState.IsActionBusy ? "Reinforcing..." : "Reinforcing"),
                        buttonEnabled: reinforcingExpiredLocally && !summaryState.IsActionBusy && onRefreshDeskRequested != null,
                        onClick: reinforcingExpiredLocally ? TriggerRefreshDesk : null));
                }
                else if (string.Equals(reinforceState.Status, "idle", StringComparison.OrdinalIgnoreCase))
                {
                    var desiredArmyId = FirstNonBlank(reinforceState.ArmyId, reinforceOp?.ArmyId);
                    var pendingForArmy = summaryState.IsActionBusy &&
                        (string.IsNullOrWhiteSpace(summaryState.PendingArmyReinforcementId)
                            ? string.IsNullOrWhiteSpace(desiredArmyId)
                            : string.Equals(summaryState.PendingArmyReinforcementId, desiredArmyId, StringComparison.OrdinalIgnoreCase));

                    cards.Add(new CardView(
                        family: "Army reinforce",
                        title: FirstNonBlank(reinforceState.ArmyName, ResolveArmyName(summary.Armies, desiredArmyId), "Recommended formation"),
                        lore: BuildArmyReinforcementIdleLore(reinforceState),
                        note: BuildArmyReinforcementIdleNote(reinforceState),
                        buttonText: pendingForArmy ? "Reinforcing..." : FirstNonBlank(reinforceState.CtaLabel, "Reinforce formation"),
                        buttonEnabled: !summaryState.IsActionBusy && onReinforceArmyRequested != null && reinforceState.StartEligible,
                        onClick: () => TriggerReinforceArmy(desiredArmyId)));
                }
            }
            else if (reinforceTimer != null)
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
                    title: $"Reinforce {ResolveArmyName(summary.Armies, reinforceOp.ArmyId)}",
                    lore: "Warfront support order is ready now.",
                    note: Truncate(FirstNonBlank(reinforceOp.Title, "A live reinforce order is visible from settlementOpeningOperations."), 96),
                    buttonText: summaryState.IsActionBusy && string.Equals(summaryState.PendingArmyReinforcementId, reinforceOp.ArmyId, StringComparison.OrdinalIgnoreCase) ? "Reinforcing..." : "Reinforce army",
                    buttonEnabled: !summaryState.IsActionBusy && onReinforceArmyRequested != null,
                    onClick: () => TriggerReinforceArmy(reinforceOp.ArmyId)));
            }

            var formationSlots = cards.Count == 0 ? 2 : Math.Min(2, Math.Max(0, 3 - cards.Count));
            foreach (var army in rankedArmies.Take(formationSlots))
            {
                cards.Add(BuildFormationCard(army, string.Equals(army.Id, targetArmyId, StringComparison.OrdinalIgnoreCase)));
            }

            cards.AddRange(windows.Take(Math.Max(0, 4 - cards.Count)).Select(timer => new CardView(
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

        private CardView BuildFormationCard(ArmySnapshot army, bool isTarget)
        {
            return new CardView(
                family: isTarget ? "Reinforcement target" : "Formation roster",
                title: army.Name,
                lore: BuildFormationLore(army),
                note: BuildFormationNote(army));
        }

        private void TriggerRefreshDesk()
        {
            if (summaryState.IsActionBusy || onRefreshDeskRequested == null)
            {
                return;
            }

            onRefreshDeskRequested.Invoke();
        }

        private void TriggerReinforceArmy(string armyId)
        {
            if (summaryState.IsActionBusy || onReinforceArmyRequested == null)
            {
                return;
            }

            _ = onReinforceArmyRequested.Invoke(armyId?.Trim() ?? string.Empty);
        }

        private static string BuildCardsCopy(IReadOnlyList<ArmySnapshot> armies, ArmyReinforcementSnapshot reinforceState, CityTimerEntrySnapshot reinforceTimer, OperationSnapshot reinforceOp, int windowCount, int otherTimerCount)
        {
            if (reinforceState != null)
            {
                if (string.Equals(reinforceState.Status, "reinforcing", StringComparison.OrdinalIgnoreCase))
                {
                    return reinforceState.FinishesAtUtc.HasValue && reinforceState.FinishesAtUtc.Value <= DateTime.UtcNow
                        ? $"Army reinforcement timer elapsed for {FirstNonBlank(reinforceState.ArmyName, "the recommended formation")}. Refresh now to load the settled state."
                        : AppendOptionalDetail($"Army reinforcement is live for {FirstNonBlank(reinforceState.ArmyName, "the recommended formation")}", BuildReinforcementDeltaText(reinforceState), suffix: ".");
                }

                if (string.Equals(reinforceState.Status, "idle", StringComparison.OrdinalIgnoreCase))
                {
                    return reinforceState.StartEligible
                        ? AppendOptionalDetail($"Reinforcement can open directly for {FirstNonBlank(reinforceState.ArmyName, "the recommended formation")}", BuildReinforcementDeltaText(reinforceState), suffix: ".")
                        : FirstNonBlank(reinforceState.BlockedReason, reinforceState.Shortfall, "Reinforcement is idle but currently blocked by resource shortfalls.");
                }
            }

            if (reinforceTimer != null)
            {
                return $"Army reinforcement is live alongside {windowCount} front window(s).";
            }

            if (reinforceOp != null)
            {
                return !string.IsNullOrWhiteSpace(reinforceOp.ArmyId) ? $"Reinforce order ready for {ResolveArmyName(armies, reinforceOp.ArmyId)}." : "A reinforce order is ready now.";
            }

            if (armies.Count > 0)
            {
                var readyCount = armies.Count(army => (army.Readiness ?? 0) >= 70);
                var totals = BuildArmyTotals(armies);
                return string.IsNullOrWhiteSpace(totals)
                    ? $"Formation watch: {readyCount}/{armies.Count} ready"
                    : $"Formation watch: {readyCount}/{armies.Count} ready • {totals}";
            }

            return windowCount > 0
                ? $"Showing {windowCount} front window(s) and {otherTimerCount} field timer(s)."
                : otherTimerCount > 0
                    ? $"{otherTimerCount} field timer(s) visible without an open front window."
                    : "No active front windows are visible right now.";
        }

        private static string BuildForceReadinessSummary(IReadOnlyList<ArmySnapshot> armies, ArmyReinforcementSnapshot reinforceState, OperationSnapshot reinforceOp)
        {
            if (armies.Count == 0 && reinforceState == null)
            {
                return "No formations visible in payload.";
            }

            var readyCount = armies.Count(army => (army.Readiness ?? 0) >= 70);
            var parts = new List<string>();
            if (armies.Count > 0)
            {
                parts.Add($"{readyCount}/{armies.Count} ready");
                var totals = BuildArmyTotals(armies);
                if (!string.IsNullOrWhiteSpace(totals))
                {
                    parts.Add(totals);
                }
            }

            var targetArmyId = FirstNonBlank(reinforceState?.ArmyId, reinforceOp?.ArmyId);
            var targetArmy = RankArmies(armies, targetArmyId).FirstOrDefault();
            if (targetArmy != null)
            {
                parts.Add($"focus {targetArmy.Name}{FormatArmyReadinessSuffix(targetArmy.Readiness)}");
            }

            if (reinforceState != null)
            {
                var deltaText = BuildReinforcementDeltaText(reinforceState);
                if (string.Equals(reinforceState.Status, "reinforcing", StringComparison.OrdinalIgnoreCase))
                {
                    parts.Add(string.IsNullOrWhiteSpace(deltaText)
                        ? $"reinforcing {FirstNonBlank(reinforceState.ArmyName, targetArmy?.Name)}"
                        : $"reinforcing {FirstNonBlank(reinforceState.ArmyName, targetArmy?.Name)} {deltaText}");
                }
                else if (string.Equals(reinforceState.Status, "idle", StringComparison.OrdinalIgnoreCase) && reinforceState.StartEligible)
                {
                    parts.Add(string.IsNullOrWhiteSpace(deltaText)
                        ? $"next reinforce {FirstNonBlank(reinforceState.ArmyName, targetArmy?.Name)}"
                        : $"next reinforce {FirstNonBlank(reinforceState.ArmyName, targetArmy?.Name)} {deltaText}");
                }
            }

            return parts.Count == 0 ? "No formations visible in payload." : string.Join(" • ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string BuildReinforcementDeskNote(IReadOnlyList<ArmySnapshot> armies, ArmyReinforcementSnapshot reinforceState, CityTimerEntrySnapshot reinforceTimer, OperationSnapshot reinforceOp, bool hasWarfrontWindows)
        {
            if (reinforceState != null)
            {
                if (string.Equals(reinforceState.Status, "reinforcing", StringComparison.OrdinalIgnoreCase))
                {
                    return reinforceState.FinishesAtUtc.HasValue && reinforceState.FinishesAtUtc.Value <= DateTime.UtcNow
                        ? "Army reinforcement timer elapsed locally. Refresh now to load the settled payload state."
                        : AppendOptionalDetail(
                            $"Reinforcing {FirstNonBlank(reinforceState.ArmyName, ResolveArmyName(armies, reinforceState.ArmyId), "the target formation")}",
                            BuildReinforcementDeltaText(reinforceState),
                            suffix: ". Fresh reinforce orders stay parked until the current build clears.");
                }

                if (string.Equals(reinforceState.Status, "idle", StringComparison.OrdinalIgnoreCase))
                {
                    return reinforceState.StartEligible
                        ? AppendOptionalDetail(
                            $"Target formation: {FirstNonBlank(reinforceState.ArmyName, ResolveArmyName(armies, reinforceState.ArmyId), "recommended formation")}",
                            BuildReinforcementDeltaText(reinforceState),
                            suffix: ".")
                        : FirstNonBlank(reinforceState.BlockedReason, reinforceState.Shortfall, "Army reinforcement is idle but blocked until resource shortfalls clear.");
                }
            }

            if (reinforceTimer != null)
            {
                return "Army reinforcement timer is live here. Fresh reinforce orders stay parked until the current build clears.";
            }

            if (reinforceOp != null)
            {
                return $"A live reinforce-army opening is visible for {ResolveArmyName(armies, reinforceOp.ArmyId)}.";
            }

            return hasWarfrontWindows
                ? "Warfront truth is live here. Reinforce buttons only appear when the payload exposes a ready-now order."
                : "No reinforce order is currently exposed in the payload.";
        }

        private static string BuildArmyReinforcementIdleLore(ArmyReinforcementSnapshot reinforceState)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(reinforceState.ArmyType)) parts.Add(HumanizeKey(reinforceState.ArmyType));
            if (reinforceState.ArmyReadiness.HasValue) parts.Add($"Readiness {reinforceState.ArmyReadiness.Value:0.#}");
            if (reinforceState.MaterialsCost.HasValue) parts.Add($"Materials {reinforceState.MaterialsCost.Value:0.#}");
            if (reinforceState.WealthCost.HasValue) parts.Add($"Wealth {reinforceState.WealthCost.Value:0.#}");
            return parts.Count > 0 ? string.Join(" • ", parts) : "Army reinforcement is available from the Warfront desk.";
        }

        private static string BuildArmyReinforcementIdleNote(ArmyReinforcementSnapshot reinforceState)
        {
            var parts = new List<string>();
            if (reinforceState.SizeDelta.HasValue) parts.Add($"+{reinforceState.SizeDelta.Value:0.#} troops");
            if (reinforceState.PowerDelta.HasValue) parts.Add($"+{reinforceState.PowerDelta.Value:0.#} power");
            if (reinforceState.ReadinessDelta.HasValue) parts.Add($"+{reinforceState.ReadinessDelta.Value:0.#} readiness");
            if (!string.IsNullOrWhiteSpace(reinforceState.BlockedReason)) parts.Add(reinforceState.BlockedReason);
            else if (!string.IsNullOrWhiteSpace(reinforceState.Shortfall)) parts.Add($"Shortfall: {reinforceState.Shortfall}");
            return parts.Count > 0 ? string.Join(" • ", parts) : "This reinforcement plan is ready from the payload.";
        }

        private static string BuildArmyReinforcementActiveNote(ArmyReinforcementSnapshot reinforceState, CityTimerEntrySnapshot reinforceTimer)
        {
            var parts = new List<string>();
            if (reinforceState.SizeDelta.HasValue) parts.Add($"+{reinforceState.SizeDelta.Value:0.#} troops");
            if (reinforceState.PowerDelta.HasValue) parts.Add($"+{reinforceState.PowerDelta.Value:0.#} power");
            if (reinforceState.ReadinessDelta.HasValue) parts.Add($"+{reinforceState.ReadinessDelta.Value:0.#} readiness");
            if (reinforceState.MaterialsCost.HasValue) parts.Add($"Materials {reinforceState.MaterialsCost.Value:0.#}");
            if (reinforceState.WealthCost.HasValue) parts.Add($"Wealth {reinforceState.WealthCost.Value:0.#}");
            if (parts.Count == 0 && !string.IsNullOrWhiteSpace(reinforceTimer?.Detail)) parts.Add(reinforceTimer.Detail);
            return parts.Count > 0 ? string.Join(" • ", parts) : "Army reinforcement timer surfaced from /api/me.";
        }

        private static string BuildFormationLore(ArmySnapshot army)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(army.Type))
            {
                parts.Add(HumanizeKey(army.Type));
            }

            if (!string.IsNullOrWhiteSpace(army.Status))
            {
                parts.Add(HumanizeStatus(army.Status));
            }

            if (army.Readiness.HasValue)
            {
                parts.Add($"readiness {army.Readiness.Value:0.#}");
            }

            return parts.Count == 0 ? "Formation surfaced from the summary payload." : string.Join(" • ", parts);
        }

        private static string BuildFormationNote(ArmySnapshot army)
        {
            var parts = new List<string>();
            if (army.Size.HasValue)
            {
                parts.Add($"{army.Size.Value:0.#} troops");
            }

            if (army.Power.HasValue)
            {
                parts.Add($"{army.Power.Value:0.#} power");
            }

            if (army.Specialties != null && army.Specialties.Count > 0)
            {
                parts.Add(string.Join(" / ", army.Specialties.Take(3).Select(HumanizeKey)));
            }

            if (!string.IsNullOrWhiteSpace(army.HoldPosture))
            {
                parts.Add($"hold {HumanizeKey(army.HoldPosture)}");
            }

            return parts.Count == 0 ? "No extra formation metadata is visible." : Truncate(string.Join(" • ", parts), 96);
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

        private static string AppendOptionalDetail(string baseText, string detail, string suffix = "")
        {
            return string.IsNullOrWhiteSpace(detail)
                ? $"{baseText}{suffix}"
                : $"{baseText} {detail}{suffix}";
        }

        private static string BuildArmyTotals(IReadOnlyList<ArmySnapshot> armies)
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

        private static string FormatArmyReadinessSuffix(double? readiness)
        {
            return readiness.HasValue ? $" {readiness.Value:0.#}" : string.Empty;
        }

        private static string HumanizeKey(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var cleaned = value.Replace('_', ' ').Replace('-', ' ').Trim();
            return cleaned.Length == 0 ? string.Empty : char.ToUpperInvariant(cleaned[0]) + cleaned.Substring(1);
        }

        private static string ResolveArmyName(IReadOnlyList<ArmySnapshot> armies, string armyId)
        {
            if (string.IsNullOrWhiteSpace(armyId)) return "army";
            return armies.FirstOrDefault(army => string.Equals(army.Id, armyId, StringComparison.OrdinalIgnoreCase))?.Name ?? armyId;
        }

        private static void RenderCards(InfoCard[] slots, List<CardView> cards)
        {
            for (var i = 0; i < slots.Length; i++)
            {
                if (i < cards.Count) slots[i].Show(cards[i]);
                else slots[i].Hide();
            }
        }

        private static string BuildFormationManagementNote(ArmySnapshot army, int splitSize, int maxSplit, ArmySnapshot mergeTarget, int formationCount, string holdRegionId, string holdPosture, string holdRegionLabel)
        {
            if (string.Equals(army.Status, "holding", StringComparison.OrdinalIgnoreCase))
            {
                var holdLine = string.IsNullOrWhiteSpace(army.HoldRegionId) ? "a regional line" : army.HoldRegionId;
                return $"{army.Name} is currently holding {holdLine} as {HumanizeKey(army.HoldPosture)} duty. Release the hold before reassigning region/posture, renaming, splitting, merging, or disbanding.";
            }

            if (!string.Equals(army.Status, "idle", StringComparison.OrdinalIgnoreCase))
            {
                return $"{army.Name} is {HumanizeStatus(army.Status)}. Rename, split, merge, disband, and hold orders stay locked until the formation returns to idle posture.";
            }

            var parts = new List<string>();
            if ((army.Size ?? 0) < 80)
            {
                parts.Add($"{army.Name} is too small to split safely. Keep at least 40 troops on each side of the split.");
            }
            else
            {
                var range = maxSplit >= 40 ? $"Valid split window: 40-{maxSplit} troops." : "Valid split window is currently unavailable.";
                parts.Add($"{range} Current draft: {BuildSplitDraftSummary(splitSize, army)}.");
            }

            if (mergeTarget != null)
            {
                parts.Add($"Merge target: {mergeTarget.Name} • {BuildFormationLore(mergeTarget)}.");
            }
            else
            {
                parts.Add("No secondary formation is available to merge into yet.");
            }

            parts.Add(string.IsNullOrWhiteSpace(holdRegionId)
                ? "Choose a region and posture before assigning a hold line."
                : $"Hold order: {(string.IsNullOrWhiteSpace(holdRegionLabel) ? holdRegionId : holdRegionLabel)} as {HumanizeKey(holdPosture)} duty.");
            parts.Add(formationCount > 1
                ? "Disband removes the selected idle formation and trims upkeep without refunding field investment directly."
                : "You must keep at least one formation on the roster, so disband is parked right now.");
            return string.Join(" ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string BuildSplitDraftSummary(int splitSize, ArmySnapshot army)
        {
            if (splitSize <= 0)
            {
                return "enter a troop count to peel off a sibling formation";
            }

            var size = (int)Math.Round(army.Size ?? 0);
            var remaining = Math.Max(0, size - splitSize);
            return $"move {splitSize} troops from {army.Name} and leave {remaining}";
        }

        private List<HoldRegionOption> BuildHoldRegionOptions()
        {
            var options = new List<HoldRegionOption>();
            if (summaryState?.RawSummary?["regionWar"] is Newtonsoft.Json.Linq.JArray regionWar)
            {
                foreach (var token in regionWar.OfType<Newtonsoft.Json.Linq.JObject>())
                {
                    var regionId = token["regionId"]?.ToString()?.Trim();
                    if (string.IsNullOrWhiteSpace(regionId) || options.Any(option => string.Equals(option.RegionId, regionId, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    var pieces = new List<string>();
                    if (TryReadDoubleToken(token["control"], out var control))
                    {
                        pieces.Add($"control {control:0}");
                    }
                    if (TryReadDoubleToken(token["threat"], out var threat))
                    {
                        pieces.Add($"threat {threat:0}");
                    }

                    options.Add(new HoldRegionOption(regionId, pieces.Count > 0 ? $"{HumanizeRegionLabel(regionId)} • {string.Join(" • ", pieces)}" : HumanizeRegionLabel(regionId)));
                }
            }

            return options;
        }

        private static bool TryReadDoubleToken(Newtonsoft.Json.Linq.JToken token, out double value)
        {
            value = 0;
            if (token == null)
            {
                return false;
            }

            switch (token.Type)
            {
                case Newtonsoft.Json.Linq.JTokenType.Integer:
                case Newtonsoft.Json.Linq.JTokenType.Float:
                    try
                    {
                        value = token.ToObject<double>();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                default:
                    return double.TryParse(token.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out value)
                        || double.TryParse(token.ToString(), out value);
            }
        }

        private string ResolveDefaultHoldRegionId()
        {
            return BuildHoldRegionOptions().FirstOrDefault().RegionId ?? string.Empty;
        }

        private static List<string> BuildHoldPostureChoices()
        {
            return new List<string>
            {
                HoldPostureToLabel("home_guard"),
                HoldPostureToLabel("frontier_hold"),
                HoldPostureToLabel("occupation"),
            };
        }

        private static string HoldPostureToLabel(string posture)
        {
            return NormalizeHoldPostureLabel(posture) switch
            {
                "home_guard" => "Home guard",
                "occupation" => "Occupation",
                _ => "Frontier hold",
            };
        }

        private static string NormalizeHoldPostureLabel(string posture)
        {
            var normalized = (posture ?? string.Empty).Trim().ToLowerInvariant().Replace("-", "_").Replace(" ", "_");
            return normalized switch
            {
                "home_guard" => "home_guard",
                "occupation" => "occupation",
                _ => "frontier_hold",
            };
        }

        private static string HumanizeRegionLabel(string regionId)
        {
            return HumanizeKey((regionId ?? string.Empty).Replace(".", " "));
        }

        private sealed class HoldRegionOption
        {
            public HoldRegionOption(string regionId, string label)
            {
                RegionId = regionId ?? string.Empty;
                Label = label ?? string.Empty;
            }

            public string RegionId { get; }

            public string Label { get; }
        }

        private static string BuildArmyChoiceLabel(ArmySnapshot army)
        {
            var typeLabel = string.IsNullOrWhiteSpace(army.Type) ? "formation" : HumanizeKey(army.Type);
            var sizeText = army.Size.HasValue ? $"{army.Size.Value:0} troops" : "troops unknown";
            var powerText = army.Power.HasValue ? $"{army.Power.Value:0} power" : "power unknown";
            return $"{army.Name} • {typeLabel} • {sizeText} • {powerText}";
        }

        private static string BuildSuggestedSplitSize(ArmySnapshot army)
        {
            var size = (int)Math.Round(army.Size ?? 0);
            if (size <= 80)
            {
                return "40";
            }

            var half = Math.Max(40, Math.Min(size - 40, (int)Math.Round(size / 2d)));
            return half.ToString();
        }

        private static string BuildSuggestedSplitName(IReadOnlyList<ArmySnapshot> armies, ArmySnapshot army)
        {
            var baseName = string.IsNullOrWhiteSpace(army?.Name) ? "Field Detachment" : $"{army.Name} Reserve";
            var candidate = baseName;
            var index = 2;
            while (armies.Any(existing => existing != null && string.Equals(existing.Name, candidate, StringComparison.OrdinalIgnoreCase)))
            {
                candidate = $"{baseName} {index}";
                index += 1;
            }
            return candidate;
        }

        private static int ParsePositiveInt(string rawValue)
        {
            return int.TryParse(rawValue?.Trim(), out var parsed) ? Math.Max(0, parsed) : 0;
        }

        private static string HumanizeStatus(string value) => string.IsNullOrWhiteSpace(value) ? "unknown" : value.Replace('_', ' ');
        private static string FormatSignal(WarfrontSignalSnapshot signal) => $"{signal.Label}: {signal.Value}";
        private static string CompactSignalSummary(List<WarfrontSignalSnapshot> signals) => signals == null || signals.Count == 0 ? "No warfront status signals." : signals.Count == 1 ? FormatSignal(signals[0]) : $"{FormatSignal(signals[0])} • +{signals.Count - 1} more";
        private static string FirstNonBlank(params string[] values) => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
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
