using PlanarWar.Client.Core;
using PlanarWar.Client.Core.Contracts;
using PlanarWar.Client.Core.Presentation;
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
        private readonly DropdownField dispatchHeroField;
        private readonly Button dispatchAssaultButton;
        private readonly Button dispatchGarrisonButton;
        private readonly InfoCard[] cards;
        private readonly SummaryState summaryState;
        private readonly Func<string, Task> onReinforceArmyRequested;
        private readonly Func<string, string, Task> onRenameArmyRequested;
        private readonly Func<string, int, string, Task> onSplitArmyRequested;
        private readonly Func<string, string, Task> onMergeArmyRequested;
        private readonly Func<string, Task> onDisbandArmyRequested;
        private readonly Func<string, string, string, Task> onAssignArmyHoldRequested;
        private readonly Func<string, Task> onReleaseArmyHoldRequested;
        private readonly Func<string, string, string, Task> onWarfrontAssaultRequested;
        private readonly Func<string, string, string, Task> onGarrisonStrikeRequested;
        private readonly Action onRefreshDeskRequested;
        private readonly List<string> managementArmyChoiceIds = new();
        private readonly List<string> mergeArmyChoiceIds = new();
        private readonly List<string> holdRegionChoiceIds = new();
        private readonly List<string> dispatchHeroChoiceIds = new();
        private string selectedArmyId = string.Empty;
        private string draftedArmyId = string.Empty;
        private string renameDraft = string.Empty;
        private string splitSizeDraft = string.Empty;
        private string splitNameDraft = string.Empty;
        private string selectedMergeArmyId = string.Empty;
        private string selectedHoldRegionId = string.Empty;
        private string selectedHoldPosture = "frontier_hold";
        private string selectedDispatchHeroId = string.Empty;
        private bool suppressManagementEvents;

        public BlackMarketScreenController(VisualElement root, SummaryState summaryState, Func<string, Task> onReinforceArmyRequested, Func<string, string, Task> onRenameArmyRequested, Func<string, int, string, Task> onSplitArmyRequested, Func<string, string, Task> onMergeArmyRequested, Func<string, Task> onDisbandArmyRequested, Func<string, string, string, Task> onAssignArmyHoldRequested, Func<string, Task> onReleaseArmyHoldRequested, Func<string, string, string, Task> onWarfrontAssaultRequested, Func<string, string, string, Task> onGarrisonStrikeRequested, Action onRefreshDeskRequested)
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
            dispatchHeroField = root.Q<DropdownField>("warfront-manage-dispatch-hero-field");
            dispatchAssaultButton = root.Q<Button>("warfront-manage-dispatch-assault-button");
            dispatchGarrisonButton = root.Q<Button>("warfront-manage-dispatch-garrison-button");
            cards = Enumerable.Range(1, 4).Select(i => new InfoCard(root, $"warfront-card-{i}", hasButton: true)).ToArray();
            this.summaryState = summaryState;
            this.onReinforceArmyRequested = onReinforceArmyRequested;
            this.onRenameArmyRequested = onRenameArmyRequested;
            this.onSplitArmyRequested = onSplitArmyRequested;
            this.onMergeArmyRequested = onMergeArmyRequested;
            this.onDisbandArmyRequested = onDisbandArmyRequested;
            this.onAssignArmyHoldRequested = onAssignArmyHoldRequested;
            this.onReleaseArmyHoldRequested = onReleaseArmyHoldRequested;
            this.onWarfrontAssaultRequested = onWarfrontAssaultRequested;
            this.onGarrisonStrikeRequested = onGarrisonStrikeRequested;
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

            dispatchHeroField?.RegisterValueChangedCallback(evt =>
            {
                if (suppressManagementEvents)
                {
                    return;
                }

                var index = dispatchHeroField.choices?.IndexOf(evt.newValue) ?? -1;
                if (index >= 0 && index < dispatchHeroChoiceIds.Count)
                {
                    selectedDispatchHeroId = dispatchHeroChoiceIds[index];
                }
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
            dispatchAssaultButton?.RegisterCallback<ClickEvent>(_ => TriggerWarfrontAssault());
            dispatchGarrisonButton?.RegisterCallback<ClickEvent>(_ => TriggerGarrisonStrike());
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
            var frontBuildings = SelectFrontBuildings(summary);
            var frontTimers = SelectFrontTimers(summary);

            headline.text = warfrontWindows.Count > 0 ? "Operations desk" : summary.WarfrontSignals.Count > 0 ? "Operations snapshot" : "Operations review";
            copy.text = warfrontWindows.Count > 0
                ? $"{warfrontWindows.Count} live operations window(s) open. Review cells, reinforcement truth, route posture, and covert deployment."
                : summary.WarfrontSignals.Count > 0
                    ? "Operations posture is visible from the current summary payload."
                    : "No active operations snapshot is visible in the current payload.";
            note.text = $"Lane {summary.City.SettlementLaneLabel} • windows {warfrontWindows.Count} • timers {warfrontTimers.Count} • fronts {frontBuildings.Count} • front timers {frontTimers.Count} • cells {summary.Armies.Count}";
            cardsCopy.text = BuildCardsCopy(summary.Armies, reinforceState, reinforceTimer, reinforceOp, warfrontWindows.Count, otherWarfrontTimers.Count) + " • " + BuildFrontCardsCopy(frontBuildings, frontTimers, nowUtc);

            windowsValue.text = warfrontWindows.Count > 0 ? $"{warfrontWindows.Count} open • {string.Join(" • ", warfrontWindows.Take(2).Select(window => HumanizeStatus(window.Status)))}" : "No active operations window.";
            readinessValue.text = BuildForceReadinessSummary(summary.Armies, reinforceState, reinforceOp);
            signalValue.text = signalPairs.Count > 0 ? CompactSignalSummary(signalPairs) : frontBuildings.Count > 0 ? BuildFrontSignalSummary(frontBuildings, frontTimers, nowUtc) : "No operations status signals.";
            missionValue.text = activeMission != null ? $"{activeMission.Title} • {(activeMission.FinishesAtUtc.HasValue ? FormatRemaining(activeMission.FinishesAtUtc.Value - nowUtc) : "anchor missing")}" : "No active support operation.";
            pressureValue.text = !string.IsNullOrWhiteSpace(primaryWarning) ? Truncate(primaryWarning, 96) : warfrontWindows.Count > 0 ? "Operations windows are open; no extra warning headline is active." : "No extra route-pressure warning surfaced.";
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
                if (managementCopy != null) managementCopy.text = "No cell payload is visible yet, so operations controls stay parked.";
                if (managementNote != null) managementNote.text = "Operations controls unlock once at least one live cell is present in the summary payload.";
                managementArmyChoiceIds.Clear();
                mergeArmyChoiceIds.Clear();
                holdRegionChoiceIds.Clear();
                dispatchHeroChoiceIds.Clear();
                managementArmyField?.SetEnabled(false);
                mergeTargetField?.SetEnabled(false);
                holdRegionField?.SetEnabled(false);
                holdPostureField?.SetEnabled(false);
                dispatchHeroField?.SetEnabled(false);
                renameInput?.SetEnabled(false);
                splitSizeInput?.SetEnabled(false);
                splitNameInput?.SetEnabled(false);
                renameButton?.SetEnabled(false);
                splitButton?.SetEnabled(false);
                mergeButton?.SetEnabled(false);
                disbandButton?.SetEnabled(false);
                assignHoldButton?.SetEnabled(false);
                releaseHoldButton?.SetEnabled(false);
                dispatchAssaultButton?.SetEnabled(false);
                dispatchGarrisonButton?.SetEnabled(false);
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
                renameDraft = PresentArmyName(selectedArmy.Name);
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

            var idleHeroes = (summary.Heroes ?? new List<HeroSnapshot>()).Where(hero => hero != null && string.Equals(hero.Status, "idle", StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrWhiteSpace(selectedDispatchHeroId) && idleHeroes.All(hero => !string.Equals(hero.Id, selectedDispatchHeroId, StringComparison.OrdinalIgnoreCase)))
            {
                selectedDispatchHeroId = string.Empty;
            }

            if (dispatchHeroField != null)
            {
                suppressManagementEvents = true;
                dispatchHeroChoiceIds.Clear();
                var dispatchChoices = new List<string> { "No explicit hero" };
                dispatchHeroChoiceIds.Add(string.Empty);
                foreach (var hero in idleHeroes)
                {
                    dispatchHeroChoiceIds.Add(hero.Id);
                    dispatchChoices.Add(BuildHeroChoiceLabel(hero));
                }
                dispatchHeroField.choices = dispatchChoices;
                var heroIndex = Math.Max(0, dispatchHeroChoiceIds.FindIndex(id => string.Equals(id, selectedDispatchHeroId, StringComparison.OrdinalIgnoreCase)));
                if (heroIndex >= dispatchHeroChoiceIds.Count)
                {
                    heroIndex = 0;
                }
                dispatchHeroField.SetValueWithoutNotify(dispatchChoices[heroIndex]);
                dispatchHeroField.SetEnabled(!summaryState.IsActionBusy && dispatchChoices.Count > 0);
                suppressManagementEvents = false;
            }

            suppressManagementEvents = true;
            renameInput?.SetValueWithoutNotify(renameDraft);
            splitSizeInput?.SetValueWithoutNotify(splitSizeDraft);
            splitNameInput?.SetValueWithoutNotify(splitNameDraft);
            suppressManagementEvents = false;

            var splitSize = ParsePositiveInt(splitSizeDraft);
            var maxSplit = Math.Max(0, (int)Math.Round(selectedArmy.Size ?? 0) - 40);
            var presentedSelectedArmyName = PresentArmyName(selectedArmy.Name);
            var canRename = !summaryState.IsActionBusy
                && idle
                && onRenameArmyRequested != null
                && !string.IsNullOrWhiteSpace(renameDraft?.Trim())
                && !string.Equals(renameDraft.Trim(), presentedSelectedArmyName, StringComparison.OrdinalIgnoreCase);
            var canSplit = !summaryState.IsActionBusy && idle && onSplitArmyRequested != null && splitSize >= 40 && splitSize <= maxSplit;
            var canMerge = !summaryState.IsActionBusy && idle && onMergeArmyRequested != null && !string.IsNullOrWhiteSpace(selectedMergeArmyId);
            var canDisband = !summaryState.IsActionBusy && idle && onDisbandArmyRequested != null && armies.Count > 1;
            var hasSelectedDispatchHero = !string.IsNullOrWhiteSpace(selectedDispatchHeroId);
            var canAssignHold = !summaryState.IsActionBusy && idle && onAssignArmyHoldRequested != null && !string.IsNullOrWhiteSpace(selectedHoldRegionId);
            var canReleaseHold = !summaryState.IsActionBusy && holding && onReleaseArmyHoldRequested != null;
            var canDispatchAssault = !summaryState.IsActionBusy && idle && onWarfrontAssaultRequested != null && !string.IsNullOrWhiteSpace(selectedHoldRegionId);
            var canDispatchGarrison = !summaryState.IsActionBusy && idle && onGarrisonStrikeRequested != null && !string.IsNullOrWhiteSpace(selectedHoldRegionId) && (hasSelectedDispatchHero || idleHeroes.Count > 0);

            renameInput?.SetEnabled(!summaryState.IsActionBusy && idle);
            splitSizeInput?.SetEnabled(!summaryState.IsActionBusy && idle);
            splitNameInput?.SetEnabled(!summaryState.IsActionBusy && idle);
            if (renameButton != null)
            {
                renameButton.text = summaryState.IsActionBusy ? "Working..." : "Rename cell";
                renameButton.SetEnabled(canRename);
            }
            if (splitButton != null)
            {
                splitButton.text = summaryState.IsActionBusy ? "Working..." : "Split cell";
                splitButton.SetEnabled(canSplit);
            }
            if (mergeButton != null)
            {
                mergeButton.text = summaryState.IsActionBusy ? "Working..." : "Merge into target";
                mergeButton.SetEnabled(canMerge);
            }
            if (disbandButton != null)
            {
                disbandButton.text = summaryState.IsActionBusy ? "Working..." : "Retire cell";
                disbandButton.SetEnabled(canDisband);
            }
            if (assignHoldButton != null)
            {
                assignHoldButton.text = summaryState.IsActionBusy ? "Working..." : "Assign route";
                assignHoldButton.SetEnabled(canAssignHold);
            }
            if (releaseHoldButton != null)
            {
                releaseHoldButton.text = summaryState.IsActionBusy ? "Working..." : "Release route";
                releaseHoldButton.SetEnabled(canReleaseHold);
            }
            if (dispatchAssaultButton != null)
            {
                dispatchAssaultButton.text = summaryState.IsActionBusy ? "Working..." : "Open pressure line";
                dispatchAssaultButton.SetEnabled(canDispatchAssault);
            }
            if (dispatchGarrisonButton != null)
            {
                dispatchGarrisonButton.text = summaryState.IsActionBusy ? "Working..." : "Run disruption action";
                dispatchGarrisonButton.SetEnabled(canDispatchGarrison);
            }

            if (managementCopy != null)
            {
                managementCopy.text = $"Operations controls stay bounded here: rename, split, merge, retire, assign routes, and dispatch the selected idle cell into live pressure actions. Focus: {PresentArmyName(selectedArmy.Name)} • {BuildFormationLore(selectedArmy)}.";
            }

            if (managementNote != null)
            {
                var mergeTarget = mergeCandidates.FirstOrDefault(army => string.Equals(army.Id, selectedMergeArmyId, StringComparison.OrdinalIgnoreCase));
                var holdLabel = holdRegions.FirstOrDefault(option => string.Equals(option.RegionId, selectedHoldRegionId, StringComparison.OrdinalIgnoreCase)).Label;
                var dispatchHero = idleHeroes.FirstOrDefault(hero => string.Equals(hero.Id, selectedDispatchHeroId, StringComparison.OrdinalIgnoreCase));
                managementNote.text = BuildFormationManagementNote(selectedArmy, splitSize, maxSplit, mergeTarget, armies.Count, selectedHoldRegionId, selectedHoldPosture, holdLabel, dispatchHero, idleHeroes.Count);
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

        private void TriggerWarfrontAssault()
        {
            if (summaryState.IsActionBusy || onWarfrontAssaultRequested == null || string.IsNullOrWhiteSpace(selectedArmyId) || string.IsNullOrWhiteSpace(selectedHoldRegionId))
            {
                return;
            }

            _ = onWarfrontAssaultRequested.Invoke(selectedHoldRegionId.Trim(), selectedArmyId.Trim(), selectedDispatchHeroId?.Trim() ?? string.Empty);
        }

        private void TriggerGarrisonStrike()
        {
            if (summaryState.IsActionBusy || onGarrisonStrikeRequested == null || string.IsNullOrWhiteSpace(selectedArmyId) || string.IsNullOrWhiteSpace(selectedHoldRegionId))
            {
                return;
            }

            _ = onGarrisonStrikeRequested.Invoke(selectedHoldRegionId.Trim(), selectedArmyId.Trim(), selectedDispatchHeroId?.Trim() ?? string.Empty);
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
                        family: "Cell reinforcement",
                        title: PresentArmyName(FirstNonBlank(reinforceState.ArmyName, ResolveArmyName(summary.Armies, reinforceState.ArmyId), "Cell reinforcement")),
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
                        family: "Cell reinforce",
                        title: PresentArmyName(FirstNonBlank(reinforceState.ArmyName, ResolveArmyName(summary.Armies, desiredArmyId), "Recommended cell")),
                        lore: BuildArmyReinforcementIdleLore(reinforceState),
                        note: BuildArmyReinforcementIdleNote(reinforceState),
                        buttonText: pendingForArmy
                            ? "Reinforcing..."
                            : $"Reinforce {PresentArmyName(FirstNonBlank(reinforceState.ArmyName, ResolveArmyName(summary.Armies, desiredArmyId), "cell"))}",
                        buttonEnabled: !summaryState.IsActionBusy && onReinforceArmyRequested != null && reinforceState.StartEligible,
                        onClick: () => TriggerReinforceArmy(desiredArmyId)));
                }
            }
            else if (reinforceTimer != null)
            {
                cards.Add(new CardView(
                    family: "Cell reinforcement",
                    title: FirstNonBlank(reinforceTimer.Label, "Reinforcement timer"),
                    lore: reinforceTimer.FinishesAtUtc.HasValue ? $"active • {FormatRemaining(reinforceTimer.FinishesAtUtc.Value - nowUtc)}" : "active",
                    note: Truncate(FirstNonBlank(reinforceTimer.Detail, "Cell reinforcement timer surfaced from the summary payload."), 96),
                    buttonText: summaryState.IsActionBusy && !string.IsNullOrWhiteSpace(summaryState.PendingArmyReinforcementId) ? "Reinforcing..." : "Reinforcing",
                    buttonEnabled: false));
            }
            else if (reinforceOp != null)
            {
                cards.Add(new CardView(
                    family: "Cell reinforce",
                    title: $"Reinforce {PresentArmyName(ResolveArmyName(summary.Armies, reinforceOp.ArmyId))} cell",
                    lore: "Pressure line is ready now.",
                    note: Truncate(FirstNonBlank(reinforceOp.Title, "A live reinforcement order is visible from settlement opening operations."), 96),
                    buttonText: summaryState.IsActionBusy && string.Equals(summaryState.PendingArmyReinforcementId, reinforceOp.ArmyId, StringComparison.OrdinalIgnoreCase) ? "Reinforcing..." : "Reinforce cell",
                    buttonEnabled: !summaryState.IsActionBusy && onReinforceArmyRequested != null,
                    onClick: () => TriggerReinforceArmy(reinforceOp.ArmyId)));
            }

            cards.AddRange(BuildFrontCards(summary, nowUtc).Take(Math.Max(0, 3 - cards.Count)));

            var formationSlots = cards.Count == 0 ? 2 : Math.Min(2, Math.Max(0, 3 - cards.Count));
            foreach (var army in rankedArmies.Take(formationSlots))
            {
                cards.Add(BuildFormationCard(army, string.Equals(army.Id, targetArmyId, StringComparison.OrdinalIgnoreCase)));
            }

            cards.AddRange(windows.Take(Math.Max(0, 4 - cards.Count)).Select(timer => new CardView(
                family: "Operations window",
                title: NormalizeOperationsTimerLabel(timer.Label),
                lore: $"{HumanizeStatus(timer.Status)} • {FormatRemaining(timer.FinishesAtUtc.HasValue ? timer.FinishesAtUtc.Value - nowUtc : (TimeSpan?)null)}",
                note: Truncate(FirstNonBlank(timer.Detail, "Live operations window surfaced from city timers."), 96))));

            if (cards.Count < 4 && activeMission != null)
            {
                cards.Add(new CardView(
                    family: "Support operation",
                    title: activeMission.Title,
                    lore: activeMission.FinishesAtUtc.HasValue ? $"Mission resolves in {FormatRemaining(activeMission.FinishesAtUtc.Value - nowUtc)}" : "Mission is live without a finish anchor.",
                    note: Truncate(BuildActiveMissionCardNote(activeMission, summary, primaryWarning), 96)));
            }

            if (cards.Count < 4 && signalPairs.Count > 0)
            {
                cards.Add(new CardView(
                    family: "Signal posture",
                    title: signalPairs[0].Label,
                    lore: Truncate(signalPairs[0].Value, 72),
                    note: signalPairs.Count > 1 ? Truncate(string.Join(" • ", signalPairs.Skip(1).Select(FormatSignal)), 96) : "Operations status comes through the summary signal map."));
            }

            if (cards.Count == 0)
            {
                cards.Add(new CardView(
                    family: "Operations payload",
                    title: "No operations entry",
                    lore: "No active operations window, route timer, or operations signal is currently visible in the summary payload.",
                    note: "The desk stays honest instead of inventing a fake battlefield."));
            }

            return cards;
        }

        private static List<CardView> BuildFrontCards(ShellSummarySnapshot summary, DateTime nowUtc)
        {
            var cards = new List<CardView>();
            foreach (var front in SelectFrontBuildings(summary).Take(2))
            {
                cards.Add(new CardView(
                    family: "Operator front",
                    title: FirstNonBlank(front.Name, front.BuildingId, "Operator front"),
                    lore: BuildFrontLore(front, nowUtc),
                    note: Truncate(FirstNonBlank(front.EffectSummary, front.Detail, "Front/building truth is visible without fake covert simulation."), 96)));
            }

            if (cards.Count < 2)
            {
                foreach (var timer in SelectFrontTimers(summary).Take(2 - cards.Count))
                {
                    cards.Add(new CardView(
                        family: "Front timer",
                        title: FirstNonBlank(timer.Label, HumanizeStatus(timer.Category), "Front timer"),
                        lore: timer.FinishesAtUtc.HasValue ? $"{HumanizeStatus(timer.Status)} • {FormatRemaining(timer.FinishesAtUtc.Value - nowUtc)}" : HumanizeStatus(timer.Status),
                        note: Truncate(FirstNonBlank(timer.Detail, "Operator-front timing is visible from cityTimers."), 96)));
                }
            }

            return cards;
        }

        private static string BuildFrontCardsCopy(List<BuildingSnapshot> fronts, List<CityTimerEntrySnapshot> timers, DateTime nowUtc)
        {
            var ready = fronts.Count(front => IsFrontReady(front, nowUtc)) + timers.Count(timer => timer.FinishesAtUtc.HasValue && timer.FinishesAtUtc.Value <= nowUtc);
            if (fronts.Count == 0 && timers.Count == 0)
            {
                return "No operator-front building cards or front build timers surfaced.";
            }

            return $"Fronts {fronts.Count} • front timers {timers.Count} • ready {ready}";
        }

        private static string BuildFrontSignalSummary(List<BuildingSnapshot> fronts, List<CityTimerEntrySnapshot> timers, DateTime nowUtc)
        {
            var firstReady = fronts.FirstOrDefault(front => IsFrontReady(front, nowUtc));
            if (firstReady != null)
            {
                return $"{FirstNonBlank(firstReady.Name, firstReady.BuildingId, "Front")} ready/finished";
            }

            var firstTimed = fronts.FirstOrDefault(front => front?.FinishesAtUtc.HasValue == true)
                ?? null;
            if (firstTimed != null)
            {
                return $"{FirstNonBlank(firstTimed.Name, firstTimed.BuildingId, "Front")} • {FormatRemaining(firstTimed.FinishesAtUtc.Value - nowUtc)}";
            }

            var timer = timers.FirstOrDefault();
            if (timer != null)
            {
                return timer.FinishesAtUtc.HasValue ? $"{FirstNonBlank(timer.Label, "Front timer")} • {FormatRemaining(timer.FinishesAtUtc.Value - nowUtc)}" : FirstNonBlank(timer.Label, "Front timer visible");
            }

            return "No operations status signals.";
        }

        private static List<BuildingSnapshot> SelectFrontBuildings(ShellSummarySnapshot summary)
        {
            return (summary?.Buildings ?? new List<BuildingSnapshot>())
                .Where(front => front != null && (ContainsFrontText(front.Lane) || ContainsFrontText(front.Type) || ContainsFrontText(front.BuildingId) || ContainsFrontText(front.Name) || ContainsFrontText(front.Detail) || ContainsFrontText(front.EffectSummary) || IsBlackMarketLane(summary)))
                .ToList();
        }

        private static List<CityTimerEntrySnapshot> SelectFrontTimers(ShellSummarySnapshot summary)
        {
            return (summary?.CityTimers ?? new List<CityTimerEntrySnapshot>())
                .Where(timer => timer != null && IsFrontTimer(timer))
                .ToList();
        }

        private static bool IsFrontTimer(CityTimerEntrySnapshot timer)
        {
            return ContainsFrontText(timer?.Category) || ContainsFrontText(timer?.Label) || ContainsFrontText(timer?.Detail);
        }

        private static bool ContainsFrontText(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return false;
            return raw.IndexOf("front", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("operator", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("black_market", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("black market", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("shadow", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("build", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("construction", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("upgrade", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsFrontReady(BuildingSnapshot front, DateTime nowUtc)
        {
            var status = (front?.Status ?? string.Empty).Trim();
            return status.Equals("ready", StringComparison.OrdinalIgnoreCase)
                || status.Equals("complete", StringComparison.OrdinalIgnoreCase)
                || status.Equals("completed", StringComparison.OrdinalIgnoreCase)
                || status.Equals("finished", StringComparison.OrdinalIgnoreCase)
                || status.Equals("done", StringComparison.OrdinalIgnoreCase)
                || (front?.FinishesAtUtc.HasValue == true && front.FinishesAtUtc.Value <= nowUtc);
        }

        private static string BuildFrontLore(BuildingSnapshot front, DateTime nowUtc)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(front?.Status)) parts.Add(HumanizeStatus(front.Status));
            if (front?.Level.HasValue == true) parts.Add($"level {front.Level.Value}");
            if (front?.FinishesAtUtc.HasValue == true) parts.Add(FormatRemaining(front.FinishesAtUtc.Value - nowUtc));
            return parts.Count > 0 ? string.Join(" • ", parts) : "Front card surfaced from summary payload.";
        }

        private static bool IsBlackMarketLane(ShellSummarySnapshot summary)
        {
            var lane = (summary?.City?.SettlementLane ?? summary?.City?.SettlementLaneLabel ?? string.Empty).Trim();
            return lane.Equals("black_market", StringComparison.OrdinalIgnoreCase)
                || lane.Equals("black market", StringComparison.OrdinalIgnoreCase)
                || lane.Equals("black-market", StringComparison.OrdinalIgnoreCase)
                || lane.Equals("shadow", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeOperationsTimerLabel(string label)
        {
            var cleaned = FirstNonBlank(label);
            if (string.IsNullOrWhiteSpace(cleaned))
            {
                return "Operations window";
            }

            cleaned = cleaned.Replace("Warfront window", "Operations window", StringComparison.OrdinalIgnoreCase);
            cleaned = cleaned.Replace("Warfront Window", "Operations window", StringComparison.OrdinalIgnoreCase);
            cleaned = cleaned.Replace("warfront window", "operations window", StringComparison.OrdinalIgnoreCase);
            return cleaned;
        }

        private CardView BuildFormationCard(ArmySnapshot army, bool isTarget)
        {
            return new CardView(
                family: isTarget ? "Deployment target" : "Cell roster",
                title: PresentArmyName(army.Name),
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


        private static string BuildActiveMissionCardNote(MissionSnapshot activeMission, ShellSummarySnapshot summary, string fallbackWarning)
        {
            var commitment = BuildMissionCommitmentSummary(activeMission, summary?.Armies ?? new List<ArmySnapshot>(), summary?.Heroes ?? new List<HeroSnapshot>());
            if (!string.IsNullOrWhiteSpace(commitment) && !string.IsNullOrWhiteSpace(fallbackWarning))
            {
                return $"{commitment} • {fallbackWarning}";
            }

            return FirstNonBlank(commitment, fallbackWarning, "Active support pressure remains part of the current operations posture.");
        }

        private static string BuildMissionCommitmentSummary(MissionSnapshot mission, IReadOnlyList<ArmySnapshot> armies, IReadOnlyList<HeroSnapshot> heroes)
        {
            if (mission == null)
            {
                return string.Empty;
            }

            var parts = new List<string>();
            var regionLabel = HumanizeRegionId(mission.RegionId);
            if (!string.IsNullOrWhiteSpace(regionLabel))
            {
                parts.Add($"Region: {regionLabel}");
            }

            var armyName = ResolveMissionArmyName(armies, mission.AssignedArmyId);
            if (!string.IsNullOrWhiteSpace(armyName))
            {
                parts.Add($"Cell: {armyName}");
            }

            var heroName = ResolveMissionHeroName(heroes, mission.AssignedHeroId);
            if (!string.IsNullOrWhiteSpace(heroName))
            {
                parts.Add($"Hero: {heroName}");
            }

            return string.Join(" • ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string ResolveMissionArmyName(IReadOnlyList<ArmySnapshot> armies, string armyId)
        {
            if (string.IsNullOrWhiteSpace(armyId) || armies == null)
            {
                return string.Empty;
            }

            return PresentArmyName(armies.FirstOrDefault(army => string.Equals(army.Id, armyId, StringComparison.OrdinalIgnoreCase))?.Name
                ?? armyId.Trim());
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

        private static string BuildCardsCopy(IReadOnlyList<ArmySnapshot> armies, ArmyReinforcementSnapshot reinforceState, CityTimerEntrySnapshot reinforceTimer, OperationSnapshot reinforceOp, int windowCount, int otherTimerCount)
        {
            if (reinforceState != null)
            {
                if (string.Equals(reinforceState.Status, "reinforcing", StringComparison.OrdinalIgnoreCase))
                {
                    return reinforceState.FinishesAtUtc.HasValue && reinforceState.FinishesAtUtc.Value <= DateTime.UtcNow
                        ? $"Cell reinforcement timer elapsed for {PresentArmyName(FirstNonBlank(reinforceState.ArmyName, "the recommended cell"))}. Refresh now to load the settled state."
                        : AppendOptionalDetail($"Cell reinforcement is live for {PresentArmyName(FirstNonBlank(reinforceState.ArmyName, "the recommended cell"))}", BuildReinforcementDeltaText(reinforceState), suffix: ".");
                }

                if (string.Equals(reinforceState.Status, "idle", StringComparison.OrdinalIgnoreCase))
                {
                    return reinforceState.StartEligible
                        ? AppendOptionalDetail($"Reinforcement can open directly for {PresentArmyName(FirstNonBlank(reinforceState.ArmyName, "the recommended cell"))}", BuildReinforcementDeltaText(reinforceState), suffix: ".")
                        : FirstNonBlank(reinforceState.BlockedReason, reinforceState.Shortfall, "Reinforcement is idle but currently blocked by resource shortfalls.");
                }
            }

            if (reinforceTimer != null)
            {
                return $"Cell reinforcement is live alongside {windowCount} operations window(s).";
            }

            if (reinforceOp != null)
            {
                return !string.IsNullOrWhiteSpace(reinforceOp.ArmyId) ? $"Reinforcement order ready for {PresentArmyName(ResolveArmyName(armies, reinforceOp.ArmyId))}." : "A reinforcement order is ready now.";
            }

            if (armies.Count > 0)
            {
                var readyCount = armies.Count(army => (army.Readiness ?? 0) >= 70);
                var totals = BuildArmyTotals(armies);
                return string.IsNullOrWhiteSpace(totals)
                    ? $"Cell watch: {readyCount}/{armies.Count} ready"
                    : $"Cell watch: {readyCount}/{armies.Count} ready • {totals}";
            }

            return windowCount > 0
                ? $"Showing {windowCount} operations window(s) and {otherTimerCount} route timer(s)."
                : otherTimerCount > 0
                    ? $"{otherTimerCount} route timer(s) visible without an open operations window."
                    : "No active operations windows are visible right now.";
        }

        private static string BuildForceReadinessSummary(IReadOnlyList<ArmySnapshot> armies, ArmyReinforcementSnapshot reinforceState, OperationSnapshot reinforceOp)
        {
            if (armies.Count == 0 && reinforceState == null)
            {
                return "No cells visible in payload.";
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
                parts.Add($"focus {PresentArmyName(targetArmy.Name)}{FormatArmyReadinessSuffix(targetArmy.Readiness)}");
            }

            if (reinforceState != null)
            {
                var deltaText = BuildReinforcementDeltaText(reinforceState);
                if (string.Equals(reinforceState.Status, "reinforcing", StringComparison.OrdinalIgnoreCase))
                {
                    parts.Add(string.IsNullOrWhiteSpace(deltaText)
                        ? $"reinforcing {PresentArmyName(FirstNonBlank(reinforceState.ArmyName, targetArmy?.Name))}"
                        : $"reinforcing {PresentArmyName(FirstNonBlank(reinforceState.ArmyName, targetArmy?.Name))} {deltaText}");
                }
                else if (string.Equals(reinforceState.Status, "idle", StringComparison.OrdinalIgnoreCase) && reinforceState.StartEligible)
                {
                    parts.Add(string.IsNullOrWhiteSpace(deltaText)
                        ? $"next reinforce {PresentArmyName(FirstNonBlank(reinforceState.ArmyName, targetArmy?.Name))}"
                        : $"next reinforce {PresentArmyName(FirstNonBlank(reinforceState.ArmyName, targetArmy?.Name))} {deltaText}");
                }
            }

            return parts.Count == 0 ? "No cells visible in payload." : string.Join(" • ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string BuildReinforcementDeskNote(IReadOnlyList<ArmySnapshot> armies, ArmyReinforcementSnapshot reinforceState, CityTimerEntrySnapshot reinforceTimer, OperationSnapshot reinforceOp, bool hasWarfrontWindows)
        {
            if (reinforceState != null)
            {
                if (string.Equals(reinforceState.Status, "reinforcing", StringComparison.OrdinalIgnoreCase))
                {
                    return reinforceState.FinishesAtUtc.HasValue && reinforceState.FinishesAtUtc.Value <= DateTime.UtcNow
                        ? "Cell reinforcement timer elapsed locally. Refresh now to load the settled payload state."
                        : AppendOptionalDetail(
                            $"Reinforcing {PresentArmyName(FirstNonBlank(reinforceState.ArmyName, ResolveArmyName(armies, reinforceState.ArmyId), "the target cell"))}",
                            BuildReinforcementDeltaText(reinforceState),
                            suffix: ". Fresh reinforce orders stay parked until the current build clears.");
                }

                if (string.Equals(reinforceState.Status, "idle", StringComparison.OrdinalIgnoreCase))
                {
                    return reinforceState.StartEligible
                        ? AppendOptionalDetail(
                            $"Target cell: {PresentArmyName(FirstNonBlank(reinforceState.ArmyName, ResolveArmyName(armies, reinforceState.ArmyId), "recommended cell"))}",
                            BuildReinforcementDeltaText(reinforceState),
                            suffix: ".")
                        : FirstNonBlank(reinforceState.BlockedReason, reinforceState.Shortfall, "Cell reinforcement is idle but blocked until resource shortfalls clear.");
                }
            }

            if (reinforceTimer != null)
            {
                return "Cell reinforcement timer is live here. Fresh reinforce orders stay parked until the current build clears.";
            }

            if (reinforceOp != null)
            {
                return $"A live reinforcement opening is visible for {PresentArmyName(ResolveArmyName(armies, reinforceOp.ArmyId))}.";
            }

            return hasWarfrontWindows
                ? "Operations truth is live here. Reinforce buttons only appear when the payload exposes a ready-now order."
                : "No reinforcement order is currently exposed in the payload.";
        }

        private static string BuildArmyReinforcementIdleLore(ArmyReinforcementSnapshot reinforceState)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(reinforceState.ArmyType)) parts.Add(HumanizeArmyTypeLabel(reinforceState.ArmyType));
            if (reinforceState.ArmyReadiness.HasValue) parts.Add($"Readiness {reinforceState.ArmyReadiness.Value:0.#}");
            var labels = ResourcePresentationText.DefaultForLane("black_market");
            if (reinforceState.MaterialsCost.HasValue) parts.Add(ResourcePresentationText.Cost(labels, "materials", reinforceState.MaterialsCost));
            if (reinforceState.WealthCost.HasValue) parts.Add(ResourcePresentationText.Cost(labels, "wealth", reinforceState.WealthCost));
            return parts.Count > 0 ? string.Join(" • ", parts) : "Cell reinforcement is available from the operations desk.";
        }

        private static string BuildArmyReinforcementIdleNote(ArmyReinforcementSnapshot reinforceState)
        {
            var parts = new List<string>();
            if (reinforceState.SizeDelta.HasValue) parts.Add($"+{reinforceState.SizeDelta.Value:0.#} agents");
            if (reinforceState.PowerDelta.HasValue) parts.Add($"+{reinforceState.PowerDelta.Value:0.#} power");
            if (reinforceState.ReadinessDelta.HasValue) parts.Add($"+{reinforceState.ReadinessDelta.Value:0.#} readiness");
            if (!string.IsNullOrWhiteSpace(reinforceState.BlockedReason)) parts.Add(reinforceState.BlockedReason);
            else if (!string.IsNullOrWhiteSpace(reinforceState.Shortfall)) parts.Add($"Shortfall: {reinforceState.Shortfall}");
            return parts.Count > 0 ? string.Join(" • ", parts) : "This reinforcement plan is ready from the payload.";
        }

        private static string BuildArmyReinforcementActiveNote(ArmyReinforcementSnapshot reinforceState, CityTimerEntrySnapshot reinforceTimer)
        {
            var parts = new List<string>();
            if (reinforceState.SizeDelta.HasValue) parts.Add($"+{reinforceState.SizeDelta.Value:0.#} agents");
            if (reinforceState.PowerDelta.HasValue) parts.Add($"+{reinforceState.PowerDelta.Value:0.#} power");
            if (reinforceState.ReadinessDelta.HasValue) parts.Add($"+{reinforceState.ReadinessDelta.Value:0.#} readiness");
            var labels = ResourcePresentationText.DefaultForLane("black_market");
            if (reinforceState.MaterialsCost.HasValue) parts.Add(ResourcePresentationText.Cost(labels, "materials", reinforceState.MaterialsCost));
            if (reinforceState.WealthCost.HasValue) parts.Add(ResourcePresentationText.Cost(labels, "wealth", reinforceState.WealthCost));
            if (parts.Count == 0 && !string.IsNullOrWhiteSpace(reinforceTimer?.Detail)) parts.Add(reinforceTimer.Detail);
            return parts.Count > 0 ? string.Join(" • ", parts) : "Cell reinforcement timer surfaced from the summary payload.";
        }

        private static string BuildFormationLore(ArmySnapshot army)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(army.Type))
            {
                parts.Add(HumanizeArmyTypeLabel(army.Type));
            }

            if (!string.IsNullOrWhiteSpace(army.Status))
            {
                parts.Add(HumanizeStatus(army.Status));
            }

            if (army.Readiness.HasValue)
            {
                parts.Add($"readiness {army.Readiness.Value:0.#}");
            }

            return parts.Count == 0 ? "Cell surfaced from the summary payload." : string.Join(" • ", parts);
        }

        private static string BuildFormationNote(ArmySnapshot army)
        {
            var parts = new List<string>();
            if (army.Size.HasValue)
            {
                parts.Add($"{army.Size.Value:0.#} agents");
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

            return parts.Count == 0 ? "No extra cell metadata is visible." : Truncate(string.Join(" • ", parts), 96);
        }

        private static string BuildReinforcementDeltaText(ArmyReinforcementSnapshot reinforceState)
        {
            var parts = new List<string>();
            if (reinforceState.SizeDelta.HasValue)
            {
                parts.Add($"+{reinforceState.SizeDelta.Value:0.#} agents");
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
                parts.Add($"{sizedArmies.Sum(army => army.Size.Value):0.#} agents");
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


        private static string HumanizeRegionId(string regionId)
        {
            if (string.IsNullOrWhiteSpace(regionId)) return string.Empty;
            var cleaned = regionId.Replace('_', ' ').Replace('-', ' ').Trim();
            return cleaned.Length == 0 ? string.Empty : char.ToUpperInvariant(cleaned[0]) + cleaned.Substring(1);
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

        private static string BuildFormationManagementNote(ArmySnapshot army, int splitSize, int maxSplit, ArmySnapshot mergeTarget, int formationCount, string holdRegionId, string holdPosture, string holdRegionLabel, HeroSnapshot dispatchHero, int idleHeroCount)
        {
            if (string.Equals(army.Status, "holding", StringComparison.OrdinalIgnoreCase))
            {
                var holdLine = string.IsNullOrWhiteSpace(army.HoldRegionId) ? "a regional line" : army.HoldRegionId;
                return $"{PresentArmyName(army.Name)} is currently holding {holdLine} as {HumanizeKey(army.HoldPosture)} duty. Release the route before reassigning region/posture, renaming, splitting, merging, or retiring the cell.";
            }

            if (!string.Equals(army.Status, "idle", StringComparison.OrdinalIgnoreCase))
            {
                return $"{PresentArmyName(army.Name)} is {HumanizeStatus(army.Status)}. Rename, split, merge, retire, and route orders stay locked until the cell returns to idle posture.";
            }

            var parts = new List<string>();
            if ((army.Size ?? 0) < 80)
            {
                parts.Add($"{PresentArmyName(army.Name)} is too small to split safely. Keep at least 40 agents on each side of the split.");
            }
            else
            {
                var range = maxSplit >= 40 ? $"Valid split window: 40-{maxSplit} agents." : "Valid split window is currently unavailable.";
                parts.Add($"{range} Current draft: {BuildSplitDraftSummary(splitSize, army)}.");
            }

            if (mergeTarget != null)
            {
                parts.Add($"Merge target: {PresentArmyName(mergeTarget.Name)} • {BuildFormationLore(mergeTarget)}.");
            }
            else
            {
                parts.Add("No secondary cell is available to merge into yet.");
            }

            parts.Add(string.IsNullOrWhiteSpace(holdRegionId)
                ? "Choose a region and posture before assigning a route or opening a pressure action."
                : $"Region desk: {(string.IsNullOrWhiteSpace(holdRegionLabel) ? holdRegionId : holdRegionLabel)} as {HumanizeKey(holdPosture)} duty, or deploy {PresentArmyName(army.Name)} there directly.");
            if (dispatchHero != null)
            {
                parts.Add($"Dispatch hero: {dispatchHero.Name} • {BuildHeroDispatchLore(dispatchHero)}.");
            }
            else if (idleHeroCount > 0)
            {
                parts.Add($"Dispatch operative is unset. {idleHeroCount} idle hero{(idleHeroCount == 1 ? string.Empty : "es")} can be assigned explicitly before live pressure actions.");
            }
            else
            {
                parts.Add("No idle hero is visible right now, so disruption action stays parked until one stands free.");
            }
            parts.Add(formationCount > 1
                ? "Retiring the selected idle cell trims upkeep without refunding committed field investment directly."
                : "You must keep at least one cell on the roster, so retirement stays parked right now.");
            return string.Join(" ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string BuildHeroDispatchLore(HeroSnapshot hero)
        {
            var roleText = string.IsNullOrWhiteSpace(hero.Role) ? "covert support" : HumanizeKey(hero.Role);
            var responseText = hero.ResponseRoles != null && hero.ResponseRoles.Count > 0
                ? string.Join("/", hero.ResponseRoles.Select(HumanizeKey))
                 : "operations support";
            return $"{roleText} • {responseText}";
        }

        private static string BuildSplitDraftSummary(int splitSize, ArmySnapshot army)
        {
            if (splitSize <= 0)
            {
                return "enter an operator count to peel off a sibling cell";
            }

            var size = (int)Math.Round(army.Size ?? 0);
            var remaining = Math.Max(0, size - splitSize);
            return $"move {splitSize} agents from {PresentArmyName(army.Name)} and leave {remaining}";
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

        private static string PresentArmyName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                return "cell";
            }

            var value = rawName.Trim();
            value = ReplaceWord(value, "Vanguard", "Cell");
            value = ReplaceWord(value, "Guard", "Cell");
            value = ReplaceWord(value, "Militia", "Operatives");
            return value;
        }

        private static string HumanizeArmyTypeLabel(string rawType)
        {
            var normalized = (rawType ?? string.Empty).Trim().ToLowerInvariant().Replace("-", "_").Replace(" ", "_");
            return normalized switch
            {
                "militia" => "Operatives",
                "vanguard" => "Cell lead",
                "guard" => "Cell watch",
                _ => string.IsNullOrWhiteSpace(rawType) ? "cell" : HumanizeKey(rawType),
            };
        }

        private static string ReplaceWord(string input, string source, string target)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(source))
            {
                return input ?? string.Empty;
            }

            var parts = input.Split(' ');
            for (var i = 0; i < parts.Length; i++)
            {
                if (string.Equals(parts[i], source, StringComparison.OrdinalIgnoreCase))
                {
                    parts[i] = target;
                }
            }

            return string.Join(" ", parts);
        }

        private static string BuildArmyChoiceLabel(ArmySnapshot army)
        {
            var typeLabel = HumanizeArmyTypeLabel(army.Type);
            var sizeText = army.Size.HasValue ? $"{army.Size.Value:0} agents" : "agents unknown";
            var powerText = army.Power.HasValue ? $"{army.Power.Value:0} power" : "power unknown";
            return $"{PresentArmyName(army.Name)} • {typeLabel} • {sizeText} • {powerText}";
        }

        private static string BuildHeroChoiceLabel(HeroSnapshot hero)
        {
            var roleText = string.IsNullOrWhiteSpace(hero.Role) ? "hero" : HumanizeKey(hero.Role);
            var levelText = hero.Level.HasValue ? $"L{hero.Level.Value:0}" : "level unknown";
            var responseText = hero.ResponseRoles != null && hero.ResponseRoles.Count > 0
                ? string.Join("/", hero.ResponseRoles.Select(HumanizeKey))
                 : "operations support";
            return $"{hero.Name} • {roleText} • {levelText} • {responseText}";
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
            var baseName = string.IsNullOrWhiteSpace(army?.Name) ? "Shadow Cell" : $"{PresentArmyName(army.Name)} Reserve";
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
        private static string CompactSignalSummary(List<WarfrontSignalSnapshot> signals) => signals == null || signals.Count == 0 ? "No operations status signals." : signals.Count == 1 ? FormatSignal(signals[0]) : $"{FormatSignal(signals[0])} • +{signals.Count - 1} more";
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
