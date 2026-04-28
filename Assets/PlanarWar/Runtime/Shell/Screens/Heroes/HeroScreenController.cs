using PlanarWar.Client.Core;
using PlanarWar.Client.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace PlanarWar.Client.UI.Screens.Heroes
{
    public sealed class HeroScreenController
    {
        private readonly SummaryState summaryState;
        private readonly Func<string, Task> onRecruitHeroRequested;
        private readonly Func<string, Task> onAcceptHeroRecruitCandidateRequested;
        private readonly Func<Task> onDismissHeroRecruitCandidatesRequested;
        private readonly Func<string, Task> onReleaseHeroRequested;
        private readonly Func<string, int, Task> onEquipHeroFromArmoryRequested;
        private readonly Func<string, string, Task> onUnequipHeroToArmoryRequested;
        private readonly Action onRefreshDeskRequested;

        private readonly Label headline;
        private readonly Label copy;
        private readonly Label overview;
        private readonly Label recruitmentValue;
        private readonly Label rosterValue;
        private readonly Label availabilityValue;
        private readonly Label armoryValue;
        private readonly Label slotCurrentValue;
        private readonly Label slotCompatibleValue;
        private readonly Label noteValue;
        private readonly DropdownField heroSelectField;
        private readonly DropdownField candidateSelectField;
        private readonly DropdownField gearSlotSelectField;
        private readonly DropdownField armoryItemSelectField;
        private readonly VisualElement heroPicker;
        private readonly VisualElement gearSlotPicker;
        private readonly VisualElement armoryItemPicker;
        private readonly VisualElement candidatePicker;
        private readonly Button releaseHeroButton;
        private readonly Button equipArmoryButton;
        private readonly Button unequipGearButton;
        private readonly Button recruitHeroButton;
        private readonly Button acceptCandidateButton;
        private readonly Button dismissCandidatesButton;
        private readonly Button refreshButton;
        private readonly InfoCard[] heroCards;

        private readonly List<string> heroChoiceIds = new();
        private readonly List<string> candidateChoiceIds = new();
        private readonly List<string> gearSlotChoices = new();
        private readonly List<int> armoryChoiceSlotIndexes = new();
        private string selectedHeroId = string.Empty;
        private string selectedCandidateId = string.Empty;
        private string selectedGearSlot = HeroArmorySlotWorkflow.StandardSlots[0];
        private int selectedArmorySlotIndex = -1;
        private string pendingReleaseHeroId = string.Empty;

        public HeroScreenController(
            VisualElement root,
            SummaryState summaryState,
            Func<string, Task> onRecruitHeroRequested,
            Func<string, Task> onAcceptHeroRecruitCandidateRequested,
            Func<Task> onDismissHeroRecruitCandidatesRequested,
            Func<string, Task> onReleaseHeroRequested,
            Func<string, int, Task> onEquipHeroFromArmoryRequested,
            Func<string, string, Task> onUnequipHeroToArmoryRequested,
            Action onRefreshDeskRequested)
        {
            this.summaryState = summaryState;
            this.onRecruitHeroRequested = onRecruitHeroRequested;
            this.onAcceptHeroRecruitCandidateRequested = onAcceptHeroRecruitCandidateRequested;
            this.onDismissHeroRecruitCandidatesRequested = onDismissHeroRecruitCandidatesRequested;
            this.onReleaseHeroRequested = onReleaseHeroRequested;
            this.onEquipHeroFromArmoryRequested = onEquipHeroFromArmoryRequested;
            this.onUnequipHeroToArmoryRequested = onUnequipHeroToArmoryRequested;
            this.onRefreshDeskRequested = onRefreshDeskRequested;

            headline = root.Q<Label>("heroes-headline-value");
            copy = root.Q<Label>("heroes-copy-value");
            overview = root.Q<Label>("heroes-overview-value");
            recruitmentValue = root.Q<Label>("heroes-recruitment-value");
            rosterValue = root.Q<Label>("heroes-roster-value");
            availabilityValue = root.Q<Label>("heroes-availability-value");
            armoryValue = root.Q<Label>("heroes-armory-value");
            slotCurrentValue = root.Q<Label>("heroes-selected-slot-current-value");
            slotCompatibleValue = root.Q<Label>("heroes-selected-slot-compatible-value");
            noteValue = root.Q<Label>("heroes-note-value");
            heroSelectField = root.Q<DropdownField>("heroes-manage-hero-field");
            candidateSelectField = root.Q<DropdownField>("heroes-manage-candidate-field");
            gearSlotSelectField = root.Q<DropdownField>("heroes-gear-slot-field");
            armoryItemSelectField = root.Q<DropdownField>("heroes-armory-item-field");
            heroPicker = root.Q<VisualElement>("heroes-roster-picker");
            gearSlotPicker = root.Q<VisualElement>("heroes-gear-slot-picker");
            armoryItemPicker = root.Q<VisualElement>("heroes-armory-item-picker");
            candidatePicker = root.Q<VisualElement>("heroes-candidate-picker");
            releaseHeroButton = root.Q<Button>("heroes-release-button");
            equipArmoryButton = root.Q<Button>("heroes-equip-armory-button");
            unequipGearButton = root.Q<Button>("heroes-unequip-gear-button");
            recruitHeroButton = root.Q<Button>("heroes-recruit-button");
            acceptCandidateButton = root.Q<Button>("heroes-candidate-accept-button");
            dismissCandidatesButton = root.Q<Button>("heroes-candidate-dismiss-button");
            refreshButton = root.Q<Button>("heroes-refresh-button");
            heroCards = Enumerable.Range(1, 4).Select(i => new InfoCard(root, $"heroes-card-{i}", true)).ToArray();

            heroSelectField?.RegisterValueChangedCallback(evt =>
            {
                var index = heroSelectField.choices?.IndexOf(evt.newValue) ?? -1;
                if (index >= 0 && index < heroChoiceIds.Count)
                {
                    selectedHeroId = heroChoiceIds[index];
                    pendingReleaseHeroId = string.Empty;
                    Render(summaryState?.Snapshot ?? ShellSummarySnapshot.Empty);
                }
            });

            candidateSelectField?.RegisterValueChangedCallback(evt =>
            {
                var index = candidateSelectField.choices?.IndexOf(evt.newValue) ?? -1;
                if (index >= 0 && index < candidateChoiceIds.Count)
                {
                    selectedCandidateId = candidateChoiceIds[index];
                    Render(summaryState?.Snapshot ?? ShellSummarySnapshot.Empty);
                }
            });

            gearSlotSelectField?.RegisterValueChangedCallback(evt =>
            {
                var index = gearSlotSelectField.choices?.IndexOf(evt.newValue) ?? -1;
                if (index >= 0 && index < gearSlotChoices.Count)
                {
                    selectedGearSlot = gearSlotChoices[index];
                    selectedArmorySlotIndex = -1;
                    Render(summaryState?.Snapshot ?? ShellSummarySnapshot.Empty);
                }
            });

            armoryItemSelectField?.RegisterValueChangedCallback(evt =>
            {
                var index = armoryItemSelectField.choices?.IndexOf(evt.newValue) ?? -1;
                if (index >= 0 && index < armoryChoiceSlotIndexes.Count)
                {
                    selectedArmorySlotIndex = armoryChoiceSlotIndexes[index];
                    Render(summaryState?.Snapshot ?? ShellSummarySnapshot.Empty);
                }
            });

            releaseHeroButton?.RegisterCallback<ClickEvent>(_ => TriggerReleaseSelected());
            equipArmoryButton?.RegisterCallback<ClickEvent>(_ => TriggerEquipSelectedArmoryItem());
            unequipGearButton?.RegisterCallback<ClickEvent>(_ => TriggerUnequipSelectedSlot());
            recruitHeroButton?.RegisterCallback<ClickEvent>(_ => TriggerRecruit());
            acceptCandidateButton?.RegisterCallback<ClickEvent>(_ => TriggerAcceptCandidate());
            dismissCandidatesButton?.RegisterCallback<ClickEvent>(_ => TriggerDismissCandidates());
            refreshButton?.RegisterCallback<ClickEvent>(_ => onRefreshDeskRequested?.Invoke());
        }

        public void Render(ShellSummarySnapshot snapshot)
        {
            var s = snapshot ?? ShellSummarySnapshot.Empty;
            var heroes = (s.Heroes ?? new List<HeroSnapshot>()).Where(hero => hero != null).ToList();
            var recruitment = s.HeroRecruitment;
            var armory = s.HeroArmoryBridge;
            var candidates = (recruitment?.Candidates ?? new List<HeroRecruitCandidateSnapshot>()).Where(candidate => candidate != null).ToList();
            var idleCount = heroes.Count(IsIdleHero);
            var onMissionCount = heroes.Count(hero => string.Equals(hero.Status, "on_mission", StringComparison.OrdinalIgnoreCase));
            var unavailableCount = Math.Max(0, heroes.Count - idleCount - onMissionCount);
            var isBusy = summaryState?.IsActionBusy == true;

            var terms = HeroTerminology.For(s);
            var nowUtc = DateTime.UtcNow;

            if (headline != null) headline.text = terms.DeskTitle;
            if (copy != null) copy.text = terms.DeskCopy;

            if (overview != null)
            {
                overview.text = heroes.Count == 0
                    ? $"No {terms.SingularLower} roster is visible in the summary payload."
                    : $"{heroes.Count} {terms.PluralLower} • {idleCount} idle • {onMissionCount} deployed • {unavailableCount} other";
            }

            if (recruitmentValue != null) recruitmentValue.text = DescribeRecruitment(recruitment, candidates, terms);
            if (rosterValue != null) rosterValue.text = heroes.Count == 0 ? $"No {terms.PluralLower} visible yet." : string.Join(" • ", heroes.Take(4).Select(DescribeHeroShort));
            if (availabilityValue != null) availabilityValue.text = heroes.Count == 0 ? "Availability unknown." : $"Idle {idleCount}/{heroes.Count} • deployed {onMissionCount} • attached {heroes.Count(h => h.AttachmentCount > 0)}";
            if (armoryValue != null) armoryValue.text = DescribeArmory(armory, terms);

            if (heroSelectField != null) heroSelectField.label = terms.SingularTitle;
            if (candidateSelectField != null) candidateSelectField.label = terms.CandidateTitle;
            if (gearSlotSelectField != null) gearSlotSelectField.label = HeroArmorySlotWorkflow.BuildSlotSurfaceTitle(terms.IsOperative);
            if (armoryItemSelectField != null) armoryItemSelectField.label = $"Compatible {HeroArmorySlotWorkflow.EquipmentNounLower(terms.IsOperative)}";

            SyncHeroDropdown(heroes, terms);
            SyncHeroButtons(heroes, terms);
            SyncCandidateDropdown(candidates, terms);
            SyncCandidateButtons(candidates, terms);
            SyncGearSlotDropdown();
            SyncGearSlotButtons();

            var selectedHero = heroes.FirstOrDefault(hero => string.Equals(hero.Id, selectedHeroId, StringComparison.OrdinalIgnoreCase));
            var selectedHeroEquipment = FindHeroEquipment(armory, selectedHero?.Id);
            var selectedEquippedEntry = HeroArmorySlotWorkflow.FindEquippedEntry(selectedHeroEquipment, selectedGearSlot);
            var compatibleArmoryItems = HeroArmorySlotWorkflow.GetCompatibleArmoryItems(armory, selectedGearSlot);
            SyncArmoryDropdown(compatibleArmoryItems, terms);
            SyncArmoryItemButtons(compatibleArmoryItems, terms);

            if (slotCurrentValue != null) slotCurrentValue.text = selectedHero == null
                ? $"Select a {terms.SingularLower} to inspect equipped {HeroArmorySlotWorkflow.EquipmentNounLower(terms.IsOperative)} by slot."
                : HeroArmorySlotWorkflow.BuildSelectedSlotCurrentText(selectedHeroEquipment, selectedGearSlot, terms.IsOperative);
            if (slotCompatibleValue != null) slotCompatibleValue.text = HeroArmorySlotWorkflow.BuildCompatibleItemSummary(compatibleArmoryItems, selectedGearSlot, terms.IsOperative);
            if (releaseHeroButton != null)
            {
                var canRelease = selectedHero != null && IsIdleHero(selectedHero) && !isBusy && onReleaseHeroRequested != null;
                var armed = selectedHero != null && string.Equals(pendingReleaseHeroId, selectedHero.Id, StringComparison.OrdinalIgnoreCase);
                releaseHeroButton.text = selectedHero == null
                    ? $"Select idle {terms.SingularLower}"
                    : !IsIdleHero(selectedHero)
                        ? $"{terms.SingularTitle} unavailable"
                        : armed
                            ? $"Confirm {terms.ReleaseVerbLower} {selectedHero.Name}"
                            : $"{terms.ReleaseVerbTitle} {selectedHero.Name}";
                releaseHeroButton.SetEnabled(canRelease);
            }

            if (equipArmoryButton != null)
            {
                var selectedCompatibleItem = compatibleArmoryItems.FirstOrDefault(item => item?.SlotIndex == selectedArmorySlotIndex);
                var canEquip = selectedHero != null && selectedCompatibleItem != null && !isBusy && onEquipHeroFromArmoryRequested != null;
                equipArmoryButton.text = selectedHero == null
                    ? $"Select {terms.SingularLower} for {HeroArmorySlotWorkflow.EquipmentNounLower(terms.IsOperative)}"
                    : selectedCompatibleItem == null
                        ? $"Select compatible {HeroArmorySlotWorkflow.FormatSlotLabel(selectedGearSlot)} {HeroArmorySlotWorkflow.EquipmentNounLower(terms.IsOperative)}"
                        : $"Equip selected {HeroArmorySlotWorkflow.EquipmentNounLower(terms.IsOperative)} to {selectedHero.Name}";
                equipArmoryButton.SetEnabled(canEquip);
            }

            if (unequipGearButton != null)
            {
                var canUnequip = selectedHero != null && selectedEquippedEntry != null && !isBusy && onUnequipHeroToArmoryRequested != null;
                unequipGearButton.text = selectedHero == null
                    ? $"Select {terms.SingularLower}"
                    : selectedEquippedEntry == null
                        ? $"No {HeroArmorySlotWorkflow.FormatSlotLabel(selectedGearSlot)} {HeroArmorySlotWorkflow.EquipmentNounLower(terms.IsOperative)} to return"
                        : $"Return {HeroArmorySlotWorkflow.FormatSlotLabel(selectedGearSlot)} {HeroArmorySlotWorkflow.EquipmentNounLower(terms.IsOperative)} to armory";
                unequipGearButton.SetEnabled(canUnequip);
            }

            if (recruitHeroButton != null)
            {
                var role = ResolveRecruitRole(recruitment);
                recruitHeroButton.text = isBusy && !string.IsNullOrWhiteSpace(summaryState?.PendingHeroRecruitRole)
                    ? terms.RecruitingText
                    : recruitment?.StartEligible == true
                        ? FirstNonBlank(recruitment.CtaLabel, terms.ScoutCandidateText)
                        : terms.RecruitmentBlockedText;
                recruitHeroButton.SetEnabled(recruitment?.StartEligible == true && !isBusy && onRecruitHeroRequested != null && !string.IsNullOrWhiteSpace(role));
            }

            if (acceptCandidateButton != null)
            {
                var selectedCandidate = candidates.FirstOrDefault(candidate => string.Equals(candidate.CandidateId, selectedCandidateId, StringComparison.OrdinalIgnoreCase));
                acceptCandidateButton.text = selectedCandidate == null
                    ? $"Select {terms.CandidateLower}"
                    : $"{terms.AcceptCandidateText}: {FirstNonBlank(selectedCandidate.DisplayName, selectedCandidate.ClassName, selectedCandidate.Role, terms.CandidateTitle)}";
                acceptCandidateButton.SetEnabled(selectedCandidate != null && !isBusy && onAcceptHeroRecruitCandidateRequested != null);
            }

            if (dismissCandidatesButton != null)
            {
                dismissCandidatesButton.text = candidates.Count == 0 ? $"No {terms.CandidatePluralLower} to dismiss" : $"Dismiss {candidates.Count} {terms.CandidateLower}{Plural(candidates.Count)}";
                dismissCandidatesButton.SetEnabled(candidates.Count > 0 && !isBusy && onDismissHeroRecruitCandidatesRequested != null);
            }

            refreshButton?.SetEnabled(!isBusy);

            if (noteValue != null)
            {
                noteValue.text = BuildDeskNote(selectedHero, candidates, recruitment, isBusy, terms, nowUtc);
            }

            var cards = BuildCards(heroes, recruitment, candidates, armory, selectedHeroEquipment, isBusy, terms, nowUtc).Take(heroCards.Length).ToList();
            for (var i = 0; i < heroCards.Length; i++)
            {
                if (i < cards.Count) heroCards[i].Show(cards[i]);
                else heroCards[i].Hide();
            }
        }

        private List<CardView> BuildCards(List<HeroSnapshot> heroes, HeroRecruitmentSnapshot recruitment, List<HeroRecruitCandidateSnapshot> candidates, HeroArmoryBridgeSnapshot armory, HeroEquipmentSnapshot selectedHeroEquipment, bool isBusy, HeroTerminology terms, DateTime nowUtc)
        {
            var cards = new List<CardView>();

            if (summaryState?.HasRecentHeroReceipt(nowUtc) == true)
            {
                cards.Add(BuildRecentHeroReceiptCard(terms));
            }

            if (armory != null)
            {
                cards.Add(BuildArmorySummaryCard(armory, terms));
            }

            if (selectedHeroEquipment != null)
            {
                cards.Add(BuildHeroGearCard(selectedHeroEquipment, terms));
            }

            if (recruitment != null && string.Equals(recruitment.Status, "scouting", StringComparison.OrdinalIgnoreCase))
            {
                cards.Add(new CardView(
                    terms.RecruitmentFamily,
                    terms.ScoutingInProgressTitle,
                    FormatRecruitmentTimer(recruitment, terms),
                    FirstNonBlank(recruitment.Shortfall, recruitment.BlockedReason, terms.ScoutingUnderwayText),
                    terms.ScoutingButtonText,
                    false,
                    null));
            }

            foreach (var candidate in candidates)
            {
                var candidateId = candidate.CandidateId;
                cards.Add(new CardView(
                    terms.CandidateTitle,
                    FirstNonBlank(candidate.DisplayName, candidate.ClassName, candidate.Role, terms.CandidateTitle),
                    BuildCandidateLore(candidate),
                    BuildCandidateNote(candidate),
                    summaryState?.IsActionBusy == true && string.Equals(summaryState.PendingHeroRecruitCandidateId, candidateId, StringComparison.OrdinalIgnoreCase) ? terms.AcceptingText : terms.AcceptCandidateText,
                    !isBusy && onAcceptHeroRecruitCandidateRequested != null && !string.IsNullOrWhiteSpace(candidateId),
                    () => _ = onAcceptHeroRecruitCandidateRequested.Invoke(candidateId)));
            }

            foreach (var hero in heroes)
            {
                var heroId = hero.Id;
                var canRelease = IsIdleHero(hero) && !isBusy && onReleaseHeroRequested != null && !string.IsNullOrWhiteSpace(heroId);
                var armed = string.Equals(pendingReleaseHeroId, heroId, StringComparison.OrdinalIgnoreCase);
                cards.Add(new CardView(
                    terms.RosterFamily,
                    hero.Name,
                    BuildHeroLore(hero, terms),
                    canRelease ? terms.ReleaseNote : BuildHeroUnavailableNote(hero, terms),
                    canRelease ? (armed ? terms.ConfirmReleaseButton : terms.ReleaseButton) : "Unavailable",
                    canRelease,
                    () => TriggerRelease(heroId)));
            }

            if (cards.Count == 0)
            {
                cards.Add(new CardView(terms.RosterFamily, $"No {terms.PluralLower} visible", $"The summary payload did not expose {terms.PluralLower} yet.", $"{terms.RecruitmentFamily} controls remain visible below when the backend exposes a recruitment surface.", "No action", false, null));
            }

            return cards;
        }

        private static HeroEquipmentSnapshot FindHeroEquipment(HeroArmoryBridgeSnapshot armory, string heroId)
        {
            if (armory?.HeroEquipment == null || string.IsNullOrWhiteSpace(heroId)) return null;
            return armory.HeroEquipment.FirstOrDefault(entry => string.Equals(entry?.HeroId, heroId, StringComparison.OrdinalIgnoreCase));
        }

        private void SyncHeroButtons(List<HeroSnapshot> heroes, HeroTerminology terms)
        {
            if (heroPicker == null) return;

            heroPicker.Clear();
            var visibleHeroes = (heroes ?? new List<HeroSnapshot>())
                .Where(hero => hero != null && !string.IsNullOrWhiteSpace(hero.Id))
                .ToList();

            if (visibleHeroes.Count == 0)
            {
                var empty = new Label(terms.IsOperative
                    ? "No operatives are visible in the current roster payload."
                    : "No heroes are visible in the current roster payload.");
                empty.AddToClassList("heroes-roster-choice-empty");
                heroPicker.Add(empty);
                return;
            }

            foreach (var hero in visibleHeroes)
            {
                var heroId = hero.Id;
                var button = new Button(() => SelectHero(heroId))
                {
                    text = BuildHeroChoiceText(hero, terms)
                };
                button.AddToClassList("heroes-roster-choice");
                if (string.Equals(heroId, selectedHeroId, StringComparison.OrdinalIgnoreCase))
                {
                    button.AddToClassList("heroes-roster-choice--selected");
                }

                heroPicker.Add(button);
            }
        }

        private void SyncCandidateButtons(List<HeroRecruitCandidateSnapshot> candidates, HeroTerminology terms)
        {
            if (candidatePicker == null) return;

            candidatePicker.Clear();
            var visibleCandidates = (candidates ?? new List<HeroRecruitCandidateSnapshot>())
                .Where(candidate => candidate != null && !string.IsNullOrWhiteSpace(candidate.CandidateId))
                .ToList();

            if (visibleCandidates.Count == 0)
            {
                var empty = new Label(terms.IsOperative
                    ? "No operative contacts are available from the current scouting slate."
                    : "No hero candidates are available from the current recruitment slate.");
                empty.AddToClassList("heroes-candidate-choice-empty");
                candidatePicker.Add(empty);
                return;
            }

            foreach (var candidate in visibleCandidates)
            {
                var candidateId = candidate.CandidateId;
                var button = new Button(() => SelectCandidate(candidateId))
                {
                    text = BuildCandidateChoiceText(candidate, terms)
                };
                button.AddToClassList("heroes-candidate-choice");
                if (string.Equals(candidateId, selectedCandidateId, StringComparison.OrdinalIgnoreCase))
                {
                    button.AddToClassList("heroes-candidate-choice--selected");
                }

                candidatePicker.Add(button);
            }
        }

        private void SyncGearSlotDropdown()
        {
            if (gearSlotSelectField == null) return;
            gearSlotChoices.Clear();
            var choices = new List<string>();
            foreach (var slot in HeroArmorySlotWorkflow.StandardSlots)
            {
                gearSlotChoices.Add(slot);
                choices.Add(HeroArmorySlotWorkflow.FormatSlotLabel(slot));
            }

            if (string.IsNullOrWhiteSpace(selectedGearSlot) || !gearSlotChoices.Any(slot => HeroArmorySlotWorkflow.SlotsMatch(slot, selectedGearSlot)))
            {
                selectedGearSlot = gearSlotChoices.Count > 0 ? gearSlotChoices[0] : string.Empty;
            }

            gearSlotSelectField.choices = choices;
            var selectedIndex = gearSlotChoices.FindIndex(slot => HeroArmorySlotWorkflow.SlotsMatch(slot, selectedGearSlot));
            gearSlotSelectField.SetValueWithoutNotify(selectedIndex >= 0 && selectedIndex < choices.Count ? choices[selectedIndex] : string.Empty);
            gearSlotSelectField.SetEnabled(choices.Count > 0);
        }

        private void SyncArmoryDropdown(List<HeroArmoryItemSnapshot> compatibleItems, HeroTerminology terms)
        {
            if (armoryItemSelectField == null) return;
            armoryChoiceSlotIndexes.Clear();
            var choices = new List<string>();
            foreach (var item in (compatibleItems ?? new List<HeroArmoryItemSnapshot>()).Where(item => item != null && item.SlotIndex.HasValue))
            {
                armoryChoiceSlotIndexes.Add(item.SlotIndex.Value);
                choices.Add(HeroArmorySlotWorkflow.BuildArmoryItemChoice(item, terms.IsOperative));
            }

            if (selectedArmorySlotIndex >= 0 && !armoryChoiceSlotIndexes.Contains(selectedArmorySlotIndex))
            {
                selectedArmorySlotIndex = -1;
            }

            if (selectedArmorySlotIndex < 0 && armoryChoiceSlotIndexes.Count > 0)
            {
                selectedArmorySlotIndex = armoryChoiceSlotIndexes[0];
            }

            armoryItemSelectField.choices = choices;
            var selectedIndex = armoryChoiceSlotIndexes.FindIndex(slot => slot == selectedArmorySlotIndex);
            armoryItemSelectField.SetValueWithoutNotify(selectedIndex >= 0 && selectedIndex < choices.Count ? choices[selectedIndex] : string.Empty);
            armoryItemSelectField.SetEnabled(choices.Count > 0);
        }

        private void SyncGearSlotButtons()
        {
            if (gearSlotPicker == null) return;

            gearSlotPicker.Clear();
            var slots = gearSlotChoices.Count > 0 ? gearSlotChoices : HeroArmorySlotWorkflow.StandardSlots.ToList();
            foreach (var slot in slots)
            {
                if (string.IsNullOrWhiteSpace(slot)) continue;

                var slotId = slot;
                var button = new Button(() => SelectGearSlot(slotId))
                {
                    text = HeroArmorySlotWorkflow.FormatSlotLabel(slotId)
                };
                button.AddToClassList("heroes-slot-chip");
                if (HeroArmorySlotWorkflow.SlotsMatch(slotId, selectedGearSlot))
                {
                    button.AddToClassList("heroes-slot-chip--selected");
                }

                gearSlotPicker.Add(button);
            }
        }

        private void SyncArmoryItemButtons(List<HeroArmoryItemSnapshot> compatibleItems, HeroTerminology terms)
        {
            if (armoryItemPicker == null) return;

            armoryItemPicker.Clear();
            var items = (compatibleItems ?? new List<HeroArmoryItemSnapshot>())
                .Where(item => item != null && item.SlotIndex.HasValue)
                .OrderBy(item => item.SlotIndex.Value)
                .ToList();

            if (items.Count == 0)
            {
                var empty = new Label(HeroArmorySlotWorkflow.BuildNoArmoryChoiceText(selectedGearSlot, terms.IsOperative));
                empty.AddToClassList("heroes-armory-choice-empty");
                armoryItemPicker.Add(empty);
                return;
            }

            foreach (var item in items)
            {
                var slotIndex = item.SlotIndex.Value;
                var button = new Button(() => SelectArmorySlotIndex(slotIndex))
                {
                    text = HeroArmorySlotWorkflow.BuildArmoryItemChoice(item, terms.IsOperative)
                };
                button.AddToClassList("heroes-armory-choice");
                if (slotIndex == selectedArmorySlotIndex)
                {
                    button.AddToClassList("heroes-armory-choice--selected");
                }

                armoryItemPicker.Add(button);
            }
        }

        private void SelectHero(string heroId)
        {
            if (string.IsNullOrWhiteSpace(heroId) || string.Equals(heroId, selectedHeroId, StringComparison.OrdinalIgnoreCase)) return;
            selectedHeroId = heroId;
            pendingReleaseHeroId = string.Empty;
            Render(summaryState?.Snapshot ?? ShellSummarySnapshot.Empty);
        }

        private void SelectCandidate(string candidateId)
        {
            if (string.IsNullOrWhiteSpace(candidateId) || string.Equals(candidateId, selectedCandidateId, StringComparison.OrdinalIgnoreCase)) return;
            selectedCandidateId = candidateId;
            Render(summaryState?.Snapshot ?? ShellSummarySnapshot.Empty);
        }

        private void SelectGearSlot(string slot)
        {
            if (string.IsNullOrWhiteSpace(slot) || HeroArmorySlotWorkflow.SlotsMatch(slot, selectedGearSlot)) return;
            selectedGearSlot = slot;
            selectedArmorySlotIndex = -1;
            Render(summaryState?.Snapshot ?? ShellSummarySnapshot.Empty);
        }

        private void SelectArmorySlotIndex(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex == selectedArmorySlotIndex) return;
            selectedArmorySlotIndex = slotIndex;
            Render(summaryState?.Snapshot ?? ShellSummarySnapshot.Empty);
        }

        private static CardView BuildArmorySummaryCard(HeroArmoryBridgeSnapshot armory, HeroTerminology terms)
        {
            var summary = armory?.Summary ?? new HeroArmorySummarySnapshot();
            var occupied = summary.OccupiedSlots ?? armory?.ArmoryItems?.Count ?? 0;
            var slots = summary.SlotCount ?? 0;
            var total = summary.TotalItemCount ?? armory?.ArmoryItems?.Sum(item => item?.Qty ?? 0) ?? 0;
            var distinct = summary.DistinctItemIds ?? armory?.ArmoryItems?.Select(item => item?.ItemId).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.OrdinalIgnoreCase).Count() ?? 0;
            var bestPlans = armory?.HeroEquipment?.Sum(entry => entry?.BestLoadoutPlan?.Count ?? 0) ?? 0;
            var lore = slots > 0
                ? $"Shared armory {occupied}/{slots} slots • {total} item{Plural(total)} • {distinct} template{Plural(distinct)}."
                : $"Shared armory surface visible • {total} item{Plural(total)} • {distinct} template{Plural(distinct)}.";
            var equipmentNoun = HeroArmorySlotWorkflow.EquipmentNounLower(terms.IsOperative);
            var note = bestPlans > 0
                ? $"{bestPlans} best-fit {equipmentNoun} issue{Plural(bestPlans)} waiting. {terms.PluralTitle} use the same shared equipment slots as MUD actors."
                : FirstNonBlank(armory?.Note, $"No best-fit {equipmentNoun} issue is currently surfaced for these {terms.PluralLower}.");
            return new CardView("Shared armory", HeroArmorySlotWorkflow.EquipmentNounTitle(terms.IsOperative) + " truth", lore, note, "Armory visible", false, null);
        }

        private CardView BuildHeroGearCard(HeroEquipmentSnapshot equipment, HeroTerminology terms)
        {
            var hero = (summaryState?.Snapshot?.Heroes ?? new List<HeroSnapshot>()).FirstOrDefault(h => string.Equals(h?.Id, equipment?.HeroId, StringComparison.OrdinalIgnoreCase));
            var heroName = hero?.Name ?? equipment?.HeroId ?? terms.SingularTitle;
            var equipped = equipment?.Equipment?.Count ?? 0;
            var best = equipment?.BestLoadoutPlan?.FirstOrDefault();
            var equipmentNoun = HeroArmorySlotWorkflow.EquipmentNounLower(terms.IsOperative);
            var lore = equipped > 0
                ? string.Join(" • ", equipment.Equipment.Take(4).Select(entry => $"{entry.Slot}: {DescribeItemName(entry.Template, entry.ItemId)}"))
                : FirstNonBlank(equipment?.EmptySlotSummary, $"No shared {equipmentNoun} equipped on {heroName}.");
            var note = best != null
                ? FirstNonBlank(best.Summary, $"Best-fit issue: {DescribeItemName(best.Template, best.ItemId)} to {FirstNonBlank(best.TargetSlot, best.Template?.Slot, "slot")}")
                : FirstNonBlank(equipment?.BestLoadoutSummaryNote, equipment?.LoadoutResetSummaryNote, $"{heroName} has no actionable armory recommendation right now.");
            return new CardView($"{terms.SingularTitle} {equipmentNoun}", heroName, lore, note, best != null ? "Best fit visible" : HeroArmorySlotWorkflow.EquipmentNounTitle(terms.IsOperative) + " visible", false, null);
        }

        private void SyncHeroDropdown(List<HeroSnapshot> heroes, HeroTerminology terms)
        {
            if (heroSelectField == null) return;
            heroChoiceIds.Clear();
            var choices = new List<string>();
            foreach (var hero in heroes)
            {
                if (string.IsNullOrWhiteSpace(hero.Id)) continue;
                heroChoiceIds.Add(hero.Id);
                choices.Add($"{hero.Name} • {FirstNonBlank(hero.Role, terms.SingularLower)} • {FirstNonBlank(hero.Status, "unknown")}");
            }

            if (!string.IsNullOrWhiteSpace(selectedHeroId) && heroes.All(hero => !string.Equals(hero.Id, selectedHeroId, StringComparison.OrdinalIgnoreCase)))
            {
                selectedHeroId = string.Empty;
                pendingReleaseHeroId = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(selectedHeroId) && heroChoiceIds.Count > 0)
            {
                selectedHeroId = heroChoiceIds[0];
            }

            heroSelectField.choices = choices;
            var selectedIndex = heroChoiceIds.FindIndex(id => string.Equals(id, selectedHeroId, StringComparison.OrdinalIgnoreCase));
            heroSelectField.SetValueWithoutNotify(selectedIndex >= 0 && selectedIndex < choices.Count ? choices[selectedIndex] : string.Empty);
            heroSelectField.SetEnabled(choices.Count > 0);
        }

        private void SyncCandidateDropdown(List<HeroRecruitCandidateSnapshot> candidates, HeroTerminology terms)
        {
            if (candidateSelectField == null) return;
            candidateChoiceIds.Clear();
            var choices = new List<string>();
            foreach (var candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(candidate.CandidateId)) continue;
                candidateChoiceIds.Add(candidate.CandidateId);
                choices.Add($"{FirstNonBlank(candidate.DisplayName, candidate.ClassName, candidate.Role, terms.CandidateTitle)} • {FirstNonBlank(candidate.ClassName, candidate.Role, "class unknown")}");
            }

            if (!string.IsNullOrWhiteSpace(selectedCandidateId) && candidates.All(candidate => !string.Equals(candidate.CandidateId, selectedCandidateId, StringComparison.OrdinalIgnoreCase)))
            {
                selectedCandidateId = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(selectedCandidateId) && candidateChoiceIds.Count > 0)
            {
                selectedCandidateId = candidateChoiceIds[0];
            }

            candidateSelectField.choices = choices;
            var selectedIndex = candidateChoiceIds.FindIndex(id => string.Equals(id, selectedCandidateId, StringComparison.OrdinalIgnoreCase));
            candidateSelectField.SetValueWithoutNotify(selectedIndex >= 0 && selectedIndex < choices.Count ? choices[selectedIndex] : string.Empty);
            candidateSelectField.SetEnabled(choices.Count > 0);
        }

        private void TriggerRecruit()
        {
            if (summaryState?.IsActionBusy == true || onRecruitHeroRequested == null) return;
            var role = ResolveRecruitRole(summaryState?.Snapshot?.HeroRecruitment);
            if (string.IsNullOrWhiteSpace(role)) return;
            _ = onRecruitHeroRequested.Invoke(role);
        }

        private void TriggerAcceptCandidate()
        {
            if (summaryState?.IsActionBusy == true || onAcceptHeroRecruitCandidateRequested == null || string.IsNullOrWhiteSpace(selectedCandidateId)) return;
            _ = onAcceptHeroRecruitCandidateRequested.Invoke(selectedCandidateId);
        }

        private void TriggerDismissCandidates()
        {
            if (summaryState?.IsActionBusy == true || onDismissHeroRecruitCandidatesRequested == null) return;
            _ = onDismissHeroRecruitCandidatesRequested.Invoke();
        }

        private void TriggerReleaseSelected()
        {
            if (summaryState?.IsActionBusy == true || string.IsNullOrWhiteSpace(selectedHeroId)) return;
            TriggerRelease(selectedHeroId);
        }

        private void TriggerRelease(string heroId)
        {
            if (summaryState?.IsActionBusy == true || onReleaseHeroRequested == null || string.IsNullOrWhiteSpace(heroId)) return;
            if (!string.Equals(pendingReleaseHeroId, heroId, StringComparison.OrdinalIgnoreCase))
            {
                pendingReleaseHeroId = heroId.Trim();
                Render(summaryState?.Snapshot ?? ShellSummarySnapshot.Empty);
                return;
            }

            pendingReleaseHeroId = string.Empty;
            _ = onReleaseHeroRequested.Invoke(heroId.Trim());
        }

        private void TriggerEquipSelectedArmoryItem()
        {
            if (summaryState?.IsActionBusy == true || onEquipHeroFromArmoryRequested == null || string.IsNullOrWhiteSpace(selectedHeroId) || selectedArmorySlotIndex < 0) return;
            var compatibleItems = HeroArmorySlotWorkflow.GetCompatibleArmoryItems(summaryState?.Snapshot?.HeroArmoryBridge, selectedGearSlot);
            if (!compatibleItems.Any(item => item?.SlotIndex == selectedArmorySlotIndex)) return;
            _ = onEquipHeroFromArmoryRequested.Invoke(selectedHeroId, selectedArmorySlotIndex);
        }

        private void TriggerUnequipSelectedSlot()
        {
            if (summaryState?.IsActionBusy == true || onUnequipHeroToArmoryRequested == null || string.IsNullOrWhiteSpace(selectedHeroId) || string.IsNullOrWhiteSpace(selectedGearSlot)) return;
            var equipment = FindHeroEquipment(summaryState?.Snapshot?.HeroArmoryBridge, selectedHeroId);
            if (!HeroArmorySlotWorkflow.HasEquippedSlot(equipment, selectedGearSlot)) return;
            _ = onUnequipHeroToArmoryRequested.Invoke(selectedHeroId, selectedGearSlot);
        }

        private static string DescribeArmory(HeroArmoryBridgeSnapshot armory, HeroTerminology terms)
        {
            if (armory == null) return $"No shared {terms.SingularLower} armory truth surfaced yet.";
            var summary = armory.Summary ?? new HeroArmorySummarySnapshot();
            var total = summary.TotalItemCount ?? armory.ArmoryItems?.Sum(item => item?.Qty ?? 0) ?? 0;
            var occupied = summary.OccupiedSlots ?? armory.ArmoryItems?.Count ?? 0;
            var slots = summary.SlotCount;
            var equipped = armory.HeroEquipment?.Sum(entry => entry?.Equipment?.Count ?? 0) ?? 0;
            var best = armory.HeroEquipment?.Sum(entry => entry?.BestLoadoutPlan?.Count ?? 0) ?? 0;
            var capacity = slots.HasValue ? $"{occupied}/{slots.Value} armory slots" : $"{occupied} occupied armory slot{Plural(occupied)}";
            return $"{capacity} • {total} item{Plural(total)} stocked • {equipped} equipped • {best} best-fit issue{Plural(best)}";
        }

        private static string DescribeItemName(HeroEquipmentTemplateSnapshot template, string fallbackItemId)
        {
            return FirstNonBlank(template?.Name, template?.Id, fallbackItemId, "item");
        }

        private static string FormatStats(HeroEquipmentTemplateSnapshot template)
        {
            if (template?.Stats == null || template.Stats.Count == 0) return string.Empty;
            return string.Join(", ", template.Stats.Take(4).Select(pair => $"{pair.Key} {pair.Value:0.##}"));
        }

        private static string DescribeRecruitment(HeroRecruitmentSnapshot recruitment, List<HeroRecruitCandidateSnapshot> candidates, HeroTerminology terms)
        {
            if (recruitment == null) return $"No {terms.RecruitmentFamily.ToLowerInvariant()} surface in payload.";
            if (candidates != null && candidates.Count > 0) return $"{candidates.Count} {terms.CandidateLower}{Plural(candidates.Count)} ready • expires {FormatTime(recruitment.CandidateExpiresAtUtc)}";
            if (string.Equals(recruitment.Status, "scouting", StringComparison.OrdinalIgnoreCase)) return FormatRecruitmentTimer(recruitment, terms);
            if (recruitment.StartEligible) return $"{terms.RecruitmentReadyText} • {CostText(recruitment.WealthCost, recruitment.UnityCost)}";
            return FirstNonBlank(recruitment.BlockedReason, recruitment.Shortfall, $"{terms.RecruitmentFamily} {FirstNonBlank(recruitment.Status, "blocked")}");
        }

        private string BuildDeskNote(HeroSnapshot selectedHero, List<HeroRecruitCandidateSnapshot> candidates, HeroRecruitmentSnapshot recruitment, bool isBusy, HeroTerminology terms, DateTime nowUtc)
        {
            if (isBusy && !string.IsNullOrWhiteSpace(summaryState?.ActionStatus)) return summaryState.ActionStatus;
            if (summaryState?.HasRecentHeroReceipt(nowUtc) == true)
            {
                var title = FirstNonBlank(summaryState?.RecentHeroReceiptTitle, summaryState?.RecentHeroReceiptAction, $"{terms.SingularTitle} roster report");
                return $"Latest {terms.SingularLower} report received: {title}. Review the report card before it rotates out.";
            }
            if (selectedHero != null && string.Equals(pendingReleaseHeroId, selectedHero.Id, StringComparison.OrdinalIgnoreCase)) return $"Confirm {terms.ReleaseVerbLower} for {selectedHero.Name}. This removes the {terms.SingularLower} from the roster; backend release should return equipped items when present.";
            if (candidates != null && candidates.Count > 0) return $"{terms.CandidateTitle} slate is ready. Accept one {terms.CandidateLower} or dismiss the slate from this desk.";
            if (recruitment?.StartEligible == true) return $"{terms.RecruitmentFamily} can be opened from this {terms.DeskTitle.ToLowerInvariant()} without returning to Development.";
            if (selectedHero != null) return $"Selected {selectedHero.Name}: {BuildHeroLore(selectedHero, terms)}";
            return FirstNonBlank(recruitment?.BlockedReason, recruitment?.Shortfall, $"{terms.DeskTitle} is ready, but no actionable roster/{terms.CandidateLower} truth is visible yet.");
        }

        private CardView BuildRecentHeroReceiptCard(HeroTerminology terms)
        {
            var title = FirstNonBlank(summaryState?.RecentHeroReceiptTitle, summaryState?.RecentHeroReceiptAction, $"{terms.SingularTitle} roster updated");
            var action = FirstNonBlank(summaryState?.RecentHeroReceiptAction, "Recent roster action");
            return new CardView(
                $"{terms.RosterFamily} report",
                title,
                $"{action} • Report received",
                BuildRecentHeroReceiptReportBody(summaryState?.RecentHeroReceipt, terms),
                "Report received",
                false,
                null);
        }

        private static string BuildRecentHeroReceiptReportBody(string receipt, HeroTerminology terms)
        {
            var noun = terms?.SingularLower ?? "hero";
            if (string.IsNullOrWhiteSpace(receipt))
            {
                return $"Roster action completed. Backend returned no readable {noun} report.";
            }

            var normalized = receipt.Trim().Replace("\r\n", "\n").Replace('\r', '\n');
            var lines = new List<string>();
            foreach (var part in normalized.Split(new[] { " • " }, StringSplitOptions.RemoveEmptyEntries))
            {
                foreach (var line in SplitReceiptReportLine(part))
                {
                    if (!string.IsNullOrWhiteSpace(line) && !lines.Contains(line, StringComparer.OrdinalIgnoreCase))
                    {
                        lines.Add(line);
                    }
                }
            }

            return lines.Count == 0 ? normalized : string.Join("\n", lines);
        }

        private static IEnumerable<string> SplitReceiptReportLine(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) yield break;

            var trimmed = text.Trim();
            foreach (var label in new[]
            {
                "Outcome:",
                "Status:",
                "Hero:",
                "Operative:",
                "Candidate:",
                "Contact:",
                "Role:",
                "Rewards:",
                "Gear:",
                "Effects:",
                "Summary:"
            })
            {
                var marker = ". " + label;
                var index = trimmed.IndexOf(marker, StringComparison.Ordinal);
                if (index > 0)
                {
                    var lead = trimmed.Substring(0, index + 1).Trim();
                    var rest = trimmed.Substring(index + 2).Trim();
                    if (!string.IsNullOrWhiteSpace(lead)) yield return lead;
                    if (!string.IsNullOrWhiteSpace(rest)) yield return rest;
                    yield break;
                }
            }

            foreach (var line in trimmed.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var clean = line.Trim();
                if (!string.IsNullOrWhiteSpace(clean)) yield return clean;
            }
        }

        private static string BuildHeroChoiceText(HeroSnapshot hero, HeroTerminology terms)
        {
            if (hero == null) return terms.SingularTitle;
            var title = FirstNonBlank(hero.Name, hero.Id, terms.SingularTitle);
            var role = FirstNonBlank(hero.Role, terms.SingularLower);
            var status = FirstNonBlank(hero.Status, "unknown");
            var details = new List<string> { role, status };
            if (hero.Level.HasValue) details.Add($"level {hero.Level:0}");
            if (hero.AttachmentCount > 0) details.Add($"{hero.AttachmentCount} attached");
            return $"{title} • {string.Join(" • ", details.Where(part => !string.IsNullOrWhiteSpace(part)))}";
        }

        private static string BuildCandidateChoiceText(HeroRecruitCandidateSnapshot candidate, HeroTerminology terms)
        {
            if (candidate == null) return terms.CandidateTitle;
            var title = FirstNonBlank(candidate.DisplayName, candidate.ClassName, candidate.Role, terms.CandidateTitle);
            var role = FirstNonBlank(candidate.ClassName, candidate.ClassId, candidate.Role, "class unknown");
            var cost = CostText(candidate.WealthCost, candidate.UnityCost);
            var traits = BuildCandidateNote(candidate);
            var summary = FirstNonBlank(candidate.Summary, terms.IsOperative ? "Operative contact from live scouting truth." : "Hero candidate from live recruitment truth.");
            return $"{title} • {role}\n{cost}\n{traits}\n{summary}";
        }

        private static string BuildHeroLore(HeroSnapshot hero, HeroTerminology terms)
        {
            var parts = new List<string>
            {
                FirstNonBlank(hero.Role, terms.SingularLower),
                FirstNonBlank(hero.Status, "unknown"),
            };
            if (hero.Level.HasValue) parts.Add($"level {hero.Level:0}");
            if (hero.ResponseRoles != null && hero.ResponseRoles.Count > 0) parts.Add(string.Join(", ", hero.ResponseRoles.Take(3)));
            return string.Join(" • ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string BuildHeroUnavailableNote(HeroSnapshot hero, HeroTerminology terms)
        {
            if (hero == null) return $"No {terms.SingularLower} selected.";
            if (IsIdleHero(hero)) return $"Idle {terms.SingularLower} is available.";
            if (string.Equals(hero.Status, "on_mission", StringComparison.OrdinalIgnoreCase)) return $"{terms.SingularTitle} is currently deployed and cannot be {terms.ReleaseVerbPastLower}.";
            return $"{terms.SingularTitle} status {FirstNonBlank(hero.Status, "unknown")} blocks {terms.ReleaseVerbLower}.";
        }

        private static string BuildCandidateLore(HeroRecruitCandidateSnapshot candidate)
        {
            var parts = new List<string>
            {
                FirstNonBlank(candidate.ClassName, candidate.ClassId, candidate.Role, "class unknown"),
                CostText(candidate.WealthCost, candidate.UnityCost),
                candidate.Summary,
            };
            return string.Join(" • ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string BuildCandidateNote(HeroRecruitCandidateSnapshot candidate)
        {
            if (candidate.TraitDetails != null && candidate.TraitDetails.Count > 0)
            {
                return string.Join(" • ", candidate.TraitDetails.Take(3).Select(trait => FirstNonBlank(trait.Name, trait.Id, trait.Polarity)));
            }
            if (candidate.Traits != null && candidate.Traits.Count > 0)
            {
                return string.Join(" • ", candidate.Traits.Take(3));
            }
            return "No trait details surfaced for this candidate.";
        }

        private static string DescribeHeroShort(HeroSnapshot hero)
        {
            return $"{hero.Name} ({FirstNonBlank(hero.Status, "unknown")})";
        }

        private static string ResolveRecruitRole(HeroRecruitmentSnapshot recruitment)
        {
            return FirstNonBlank(recruitment?.StartRole, recruitment?.Role, "balanced");
        }

        private static string FormatRecruitmentTimer(HeroRecruitmentSnapshot recruitment, HeroTerminology terms)
        {
            var finish = recruitment?.FinishesAtUtc ?? recruitment?.ReadyAtUtc;
            if (finish.HasValue)
            {
                var remaining = finish.Value - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero) return $"{terms.RecruitmentFamily} scouting is ready now.";
                return $"{terms.RecruitmentFamily} resolves in {FormatDuration(remaining)}.";
            }
            return $"{terms.RecruitmentFamily} scouting is active; finish time not surfaced.";
        }

        private static string FormatDuration(TimeSpan remaining)
        {
            if (remaining < TimeSpan.Zero) remaining = TimeSpan.Zero;
            if (remaining.TotalHours >= 1) return $"{(int)remaining.TotalHours:00}:{remaining.Minutes:00}:{remaining.Seconds:00}";
            return $"{remaining.Minutes:00}:{remaining.Seconds:00}";
        }

        private static string FormatTime(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("HH:mm:ss 'UTC'") : "unknown";
        }

        private static string CostText(double? wealth, double? unity)
        {
            var parts = new List<string>();
            if (wealth.HasValue) parts.Add($"wealth {wealth:0}");
            if (unity.HasValue) parts.Add($"unity {unity:0}");
            return parts.Count == 0 ? string.Empty : $"cost {string.Join(", ", parts)}";
        }

        private static bool IsIdleHero(HeroSnapshot hero)
        {
            return hero != null && string.Equals(hero.Status, "idle", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsBlackMarketLane(ShellSummarySnapshot s)
        {
            return string.Equals(s?.City?.SettlementLane, "black_market", StringComparison.OrdinalIgnoreCase)
                || string.Equals(s?.City?.SettlementLaneLabel, "Black Market", StringComparison.OrdinalIgnoreCase);
        }

        private static string Truncate(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;
            var trimmed = text.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed.Substring(0, Math.Max(0, maxLength - 1)) + "…";
        }

        private static string FirstNonBlank(params string[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
            }
            return string.Empty;
        }

        private static string Plural(int count)
        {
            return count == 1 ? string.Empty : "s";
        }

        private sealed class HeroTerminology
        {
            private HeroTerminology(
                string singularTitle,
                string singularLower,
                string pluralTitle,
                string pluralLower,
                string deskTitle,
                string deskCopy,
                string rosterFamily,
                string recruitmentFamily,
                string candidateTitle,
                string candidateLower,
                string candidatePluralLower,
                string releaseVerbTitle,
                string releaseVerbLower,
                string releaseVerbPastLower,
                string scoutCandidateText)
            {
                SingularTitle = singularTitle;
                SingularLower = singularLower;
                PluralTitle = pluralTitle;
                PluralLower = pluralLower;
                DeskTitle = deskTitle;
                DeskCopy = deskCopy;
                RosterFamily = rosterFamily;
                RecruitmentFamily = recruitmentFamily;
                CandidateTitle = candidateTitle;
                CandidateLower = candidateLower;
                CandidatePluralLower = candidatePluralLower;
                ReleaseVerbTitle = releaseVerbTitle;
                ReleaseVerbLower = releaseVerbLower;
                ReleaseVerbPastLower = releaseVerbPastLower;
                ScoutCandidateText = scoutCandidateText;
            }

            public string SingularTitle { get; }
            public string SingularLower { get; }
            public string PluralTitle { get; }
            public string PluralLower { get; }
            public string DeskTitle { get; }
            public string DeskCopy { get; }
            public string RosterFamily { get; }
            public string RecruitmentFamily { get; }
            public string CandidateTitle { get; }
            public string CandidateLower { get; }
            public string CandidatePluralLower { get; }
            public string ReleaseVerbTitle { get; }
            public string ReleaseVerbLower { get; }
            public string ReleaseVerbPastLower { get; }
            public string ScoutCandidateText { get; }
            public string RecruitingText => IsOperative ? "Scouting..." : "Recruiting...";
            public string RecruitmentBlockedText => IsOperative ? "Scouting blocked" : "Recruitment blocked";
            public string RecruitmentReadyText => IsOperative ? "Scouting ready" : "Recruitment ready";
            public string ScoutingInProgressTitle => IsOperative ? "Operative scouting in progress" : "Scouting in progress";
            public string ScoutingUnderwayText => IsOperative ? "Operative scouting is underway." : "Candidate scouting is underway.";
            public string ScoutingButtonText => "Scouting";
            public string AcceptingText => IsOperative ? "Contacting..." : "Accepting...";
            public string AcceptCandidateText => IsOperative ? "Recruit contact" : "Accept candidate";
            public string ReleaseNote => $"Idle {PluralLower} can be {ReleaseVerbPastLower}; equipped/attached items should return through backend hero-release truth.";
            public string ConfirmReleaseButton => $"Confirm {ReleaseVerbLower}";
            public string ReleaseButton => $"{ReleaseVerbTitle} {SingularLower}";
            public bool IsOperative => string.Equals(SingularLower, "operative", StringComparison.OrdinalIgnoreCase);

            public static HeroTerminology For(ShellSummarySnapshot snapshot)
            {
                if (IsBlackMarketLane(snapshot))
                {
                    return new HeroTerminology(
                        "Operative",
                        "operative",
                        "Operatives",
                        "operatives",
                        "Operatives desk",
                        "Inspect named operatives, scout contacts, dismiss contact slates, and retire idle operatives without making the Black Market pretend they are civic heroes.",
                        "Operative roster",
                        "Scouting",
                        "Contact",
                        "contact",
                        "contacts",
                        "Retire",
                        "retire",
                        "retired",
                        "Scout operative contact");
                }

                return new HeroTerminology(
                    "Hero",
                    "hero",
                    "Heroes",
                    "heroes",
                    "Hero desk",
                    "Inspect heroes, scout candidates, dismiss candidate slates, and release idle heroes without hiding roster truth inside Development.",
                    "Hero roster",
                    "Recruitment",
                    "Candidate",
                    "candidate",
                    "candidates",
                    "Release",
                    "release",
                    "released",
                    "Scout hero candidate");
            }
        }

        private sealed class CardView
        {
            public CardView(string family, string title, string lore, string note, string buttonText, bool buttonEnabled, Action onClick)
            {
                Family = family;
                Title = title;
                Lore = lore;
                Note = note;
                ButtonText = buttonText;
                ButtonEnabled = buttonEnabled;
                OnClick = onClick;
            }

            public string Family { get; }
            public string Title { get; }
            public string Lore { get; }
            public string Note { get; }
            public string ButtonText { get; }
            public bool ButtonEnabled { get; }
            public Action OnClick { get; }
        }

        private sealed class InfoCard
        {
            private readonly VisualElement root;
            private readonly Label family;
            private readonly Label title;
            private readonly Label lore;
            private readonly Label note;
            private readonly Button button;
            private Action onClick;

            public InfoCard(VisualElement rootElement, string prefix, bool hasButton)
            {
                root = rootElement.Q<VisualElement>($"{prefix}-root");
                family = rootElement.Q<Label>($"{prefix}-family-value");
                title = rootElement.Q<Label>($"{prefix}-title-value");
                lore = rootElement.Q<Label>($"{prefix}-lore-value");
                note = rootElement.Q<Label>($"{prefix}-note-value");
                button = hasButton ? rootElement.Q<Button>($"{prefix}-button") : null;
                button?.RegisterCallback<ClickEvent>(_ => onClick?.Invoke());
            }

            public void Show(CardView view)
            {
                if (root == null) return;
                root.style.display = DisplayStyle.Flex;
                if (family != null) family.text = view.Family;
                if (title != null) title.text = view.Title;
                if (lore != null) lore.text = view.Lore;
                if (note != null) note.text = view.Note;
                onClick = view.OnClick;
                if (button != null)
                {
                    button.text = view.ButtonText;
                    button.SetEnabled(view.ButtonEnabled && view.OnClick != null);
                    button.style.display = DisplayStyle.Flex;
                }
            }

            public void Hide()
            {
                if (root == null) return;
                root.style.display = DisplayStyle.None;
                onClick = null;
                button?.SetEnabled(false);
            }
        }
    }
}
