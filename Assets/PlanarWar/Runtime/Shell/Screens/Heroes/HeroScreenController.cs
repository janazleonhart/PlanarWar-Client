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
        private readonly Action onRefreshDeskRequested;

        private readonly Label headline;
        private readonly Label copy;
        private readonly Label overview;
        private readonly Label recruitmentValue;
        private readonly Label rosterValue;
        private readonly Label availabilityValue;
        private readonly Label noteValue;
        private readonly DropdownField heroSelectField;
        private readonly DropdownField candidateSelectField;
        private readonly Button releaseHeroButton;
        private readonly Button recruitHeroButton;
        private readonly Button acceptCandidateButton;
        private readonly Button dismissCandidatesButton;
        private readonly Button refreshButton;
        private readonly InfoCard[] heroCards;

        private readonly List<string> heroChoiceIds = new();
        private readonly List<string> candidateChoiceIds = new();
        private string selectedHeroId = string.Empty;
        private string selectedCandidateId = string.Empty;
        private string pendingReleaseHeroId = string.Empty;

        public HeroScreenController(
            VisualElement root,
            SummaryState summaryState,
            Func<string, Task> onRecruitHeroRequested,
            Func<string, Task> onAcceptHeroRecruitCandidateRequested,
            Func<Task> onDismissHeroRecruitCandidatesRequested,
            Func<string, Task> onReleaseHeroRequested,
            Action onRefreshDeskRequested)
        {
            this.summaryState = summaryState;
            this.onRecruitHeroRequested = onRecruitHeroRequested;
            this.onAcceptHeroRecruitCandidateRequested = onAcceptHeroRecruitCandidateRequested;
            this.onDismissHeroRecruitCandidatesRequested = onDismissHeroRecruitCandidatesRequested;
            this.onReleaseHeroRequested = onReleaseHeroRequested;
            this.onRefreshDeskRequested = onRefreshDeskRequested;

            headline = root.Q<Label>("heroes-headline-value");
            copy = root.Q<Label>("heroes-copy-value");
            overview = root.Q<Label>("heroes-overview-value");
            recruitmentValue = root.Q<Label>("heroes-recruitment-value");
            rosterValue = root.Q<Label>("heroes-roster-value");
            availabilityValue = root.Q<Label>("heroes-availability-value");
            noteValue = root.Q<Label>("heroes-note-value");
            heroSelectField = root.Q<DropdownField>("heroes-manage-hero-field");
            candidateSelectField = root.Q<DropdownField>("heroes-manage-candidate-field");
            releaseHeroButton = root.Q<Button>("heroes-release-button");
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

            releaseHeroButton?.RegisterCallback<ClickEvent>(_ => TriggerReleaseSelected());
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
            var candidates = (recruitment?.Candidates ?? new List<HeroRecruitCandidateSnapshot>()).Where(candidate => candidate != null).ToList();
            var idleCount = heroes.Count(IsIdleHero);
            var onMissionCount = heroes.Count(hero => string.Equals(hero.Status, "on_mission", StringComparison.OrdinalIgnoreCase));
            var unavailableCount = Math.Max(0, heroes.Count - idleCount - onMissionCount);
            var isBusy = summaryState?.IsActionBusy == true;

            if (headline != null) headline.text = IsBlackMarketLane(s) ? "Specialists desk" : "Hero desk";
            if (copy != null)
            {
                copy.text = IsBlackMarketLane(s)
                    ? "Inspect operators, scout candidates, dismiss candidate slates, and release idle specialists without hiding hero truth inside Development."
                    : "Inspect heroes, scout candidates, dismiss candidate slates, and release idle heroes without hiding roster truth inside Development.";
            }

            if (overview != null)
            {
                overview.text = heroes.Count == 0
                    ? "No hero roster is visible in the summary payload."
                    : $"{heroes.Count} hero{Plural(heroes.Count)} • {idleCount} idle • {onMissionCount} on mission • {unavailableCount} other";
            }

            if (recruitmentValue != null) recruitmentValue.text = DescribeRecruitment(recruitment, candidates);
            if (rosterValue != null) rosterValue.text = heroes.Count == 0 ? "No heroes visible yet." : string.Join(" • ", heroes.Take(4).Select(DescribeHeroShort));
            if (availabilityValue != null) availabilityValue.text = heroes.Count == 0 ? "Availability unknown." : $"Idle {idleCount}/{heroes.Count} • deployed {onMissionCount} • geared {heroes.Count(h => h.AttachmentCount > 0)}";

            SyncHeroDropdown(heroes);
            SyncCandidateDropdown(candidates);

            var selectedHero = heroes.FirstOrDefault(hero => string.Equals(hero.Id, selectedHeroId, StringComparison.OrdinalIgnoreCase));
            if (releaseHeroButton != null)
            {
                var canRelease = selectedHero != null && IsIdleHero(selectedHero) && !isBusy && onReleaseHeroRequested != null;
                var armed = selectedHero != null && string.Equals(pendingReleaseHeroId, selectedHero.Id, StringComparison.OrdinalIgnoreCase);
                releaseHeroButton.text = selectedHero == null
                    ? "Select idle hero"
                    : !IsIdleHero(selectedHero)
                        ? "Hero unavailable"
                        : armed
                            ? $"Confirm release {selectedHero.Name}"
                            : $"Release {selectedHero.Name}";
                releaseHeroButton.SetEnabled(canRelease);
            }

            if (recruitHeroButton != null)
            {
                var role = ResolveRecruitRole(recruitment);
                recruitHeroButton.text = isBusy && !string.IsNullOrWhiteSpace(summaryState?.PendingHeroRecruitRole)
                    ? "Recruiting..."
                    : recruitment?.StartEligible == true
                        ? FirstNonBlank(recruitment.CtaLabel, "Scout hero candidate")
                        : "Recruitment blocked";
                recruitHeroButton.SetEnabled(recruitment?.StartEligible == true && !isBusy && onRecruitHeroRequested != null && !string.IsNullOrWhiteSpace(role));
            }

            if (acceptCandidateButton != null)
            {
                var selectedCandidate = candidates.FirstOrDefault(candidate => string.Equals(candidate.CandidateId, selectedCandidateId, StringComparison.OrdinalIgnoreCase));
                acceptCandidateButton.text = selectedCandidate == null ? "Select candidate" : $"Accept {selectedCandidate.DisplayName}";
                acceptCandidateButton.SetEnabled(selectedCandidate != null && !isBusy && onAcceptHeroRecruitCandidateRequested != null);
            }

            if (dismissCandidatesButton != null)
            {
                dismissCandidatesButton.text = candidates.Count == 0 ? "No candidates to dismiss" : $"Dismiss {candidates.Count} candidate{Plural(candidates.Count)}";
                dismissCandidatesButton.SetEnabled(candidates.Count > 0 && !isBusy && onDismissHeroRecruitCandidatesRequested != null);
            }

            refreshButton?.SetEnabled(!isBusy);

            if (noteValue != null)
            {
                noteValue.text = BuildDeskNote(selectedHero, candidates, recruitment, isBusy);
            }

            var cards = BuildCards(heroes, recruitment, candidates, isBusy).Take(heroCards.Length).ToList();
            for (var i = 0; i < heroCards.Length; i++)
            {
                if (i < cards.Count) heroCards[i].Show(cards[i]);
                else heroCards[i].Hide();
            }
        }

        private List<CardView> BuildCards(List<HeroSnapshot> heroes, HeroRecruitmentSnapshot recruitment, List<HeroRecruitCandidateSnapshot> candidates, bool isBusy)
        {
            var cards = new List<CardView>();

            if (recruitment != null && string.Equals(recruitment.Status, "scouting", StringComparison.OrdinalIgnoreCase))
            {
                cards.Add(new CardView(
                    "Recruitment",
                    "Scouting in progress",
                    FormatRecruitmentTimer(recruitment),
                    FirstNonBlank(recruitment.Shortfall, recruitment.BlockedReason, "Candidate scouting is underway."),
                    "Scouting",
                    false,
                    null));
            }

            foreach (var candidate in candidates)
            {
                var candidateId = candidate.CandidateId;
                cards.Add(new CardView(
                    "Candidate",
                    FirstNonBlank(candidate.DisplayName, candidate.ClassName, candidate.Role, "Recruit candidate"),
                    BuildCandidateLore(candidate),
                    BuildCandidateNote(candidate),
                    summaryState?.IsActionBusy == true && string.Equals(summaryState.PendingHeroRecruitCandidateId, candidateId, StringComparison.OrdinalIgnoreCase) ? "Accepting..." : "Accept candidate",
                    !isBusy && onAcceptHeroRecruitCandidateRequested != null && !string.IsNullOrWhiteSpace(candidateId),
                    () => _ = onAcceptHeroRecruitCandidateRequested.Invoke(candidateId)));
            }

            foreach (var hero in heroes)
            {
                var heroId = hero.Id;
                var canRelease = IsIdleHero(hero) && !isBusy && onReleaseHeroRequested != null && !string.IsNullOrWhiteSpace(heroId);
                var armed = string.Equals(pendingReleaseHeroId, heroId, StringComparison.OrdinalIgnoreCase);
                cards.Add(new CardView(
                    "Roster",
                    hero.Name,
                    BuildHeroLore(hero),
                    canRelease ? "Idle heroes can be released; equipped/attached items should return through backend hero-release truth." : BuildHeroUnavailableNote(hero),
                    canRelease ? (armed ? "Confirm release" : "Release hero") : "Unavailable",
                    canRelease,
                    () => TriggerRelease(heroId)));
            }

            if (cards.Count == 0)
            {
                cards.Add(new CardView("Hero roster", "No heroes visible", "The summary payload did not expose heroes yet.", "Recruitment controls remain visible below when the backend exposes a recruitment surface.", "No action", false, null));
            }

            return cards;
        }

        private void SyncHeroDropdown(List<HeroSnapshot> heroes)
        {
            if (heroSelectField == null) return;
            heroChoiceIds.Clear();
            var choices = new List<string>();
            foreach (var hero in heroes)
            {
                if (string.IsNullOrWhiteSpace(hero.Id)) continue;
                heroChoiceIds.Add(hero.Id);
                choices.Add($"{hero.Name} • {FirstNonBlank(hero.Role, "hero")} • {FirstNonBlank(hero.Status, "unknown")}");
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

        private void SyncCandidateDropdown(List<HeroRecruitCandidateSnapshot> candidates)
        {
            if (candidateSelectField == null) return;
            candidateChoiceIds.Clear();
            var choices = new List<string>();
            foreach (var candidate in candidates)
            {
                if (string.IsNullOrWhiteSpace(candidate.CandidateId)) continue;
                candidateChoiceIds.Add(candidate.CandidateId);
                choices.Add($"{FirstNonBlank(candidate.DisplayName, candidate.ClassName, candidate.Role, "Candidate")} • {FirstNonBlank(candidate.ClassName, candidate.Role, "class unknown")}");
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

        private static string DescribeRecruitment(HeroRecruitmentSnapshot recruitment, List<HeroRecruitCandidateSnapshot> candidates)
        {
            if (recruitment == null) return "No recruitment surface in payload.";
            if (candidates != null && candidates.Count > 0) return $"{candidates.Count} candidate{Plural(candidates.Count)} ready • expires {FormatTime(recruitment.CandidateExpiresAtUtc)}";
            if (string.Equals(recruitment.Status, "scouting", StringComparison.OrdinalIgnoreCase)) return FormatRecruitmentTimer(recruitment);
            if (recruitment.StartEligible) return $"Recruitment ready • {CostText(recruitment.WealthCost, recruitment.UnityCost)}";
            return FirstNonBlank(recruitment.BlockedReason, recruitment.Shortfall, $"Recruitment {FirstNonBlank(recruitment.Status, "blocked")}");
        }

        private string BuildDeskNote(HeroSnapshot selectedHero, List<HeroRecruitCandidateSnapshot> candidates, HeroRecruitmentSnapshot recruitment, bool isBusy)
        {
            if (isBusy && !string.IsNullOrWhiteSpace(summaryState?.ActionStatus)) return summaryState.ActionStatus;
            if (selectedHero != null && string.Equals(pendingReleaseHeroId, selectedHero.Id, StringComparison.OrdinalIgnoreCase)) return $"Confirm release for {selectedHero.Name}. This removes the hero from the roster; backend release should return equipped items when present.";
            if (candidates != null && candidates.Count > 0) return "Candidate slate is ready. Accept one candidate or dismiss the slate from this desk.";
            if (recruitment?.StartEligible == true) return "Recruitment can be opened from this hero desk without returning to Development.";
            if (selectedHero != null) return $"Selected {selectedHero.Name}: {BuildHeroLore(selectedHero)}";
            return FirstNonBlank(recruitment?.BlockedReason, recruitment?.Shortfall, "Hero desk is ready, but no actionable roster/candidate truth is visible yet.");
        }

        private static string BuildHeroLore(HeroSnapshot hero)
        {
            var parts = new List<string>
            {
                FirstNonBlank(hero.Role, "hero"),
                FirstNonBlank(hero.Status, "unknown"),
            };
            if (hero.Level.HasValue) parts.Add($"level {hero.Level:0}");
            if (hero.ResponseRoles != null && hero.ResponseRoles.Count > 0) parts.Add(string.Join(", ", hero.ResponseRoles.Take(3)));
            return string.Join(" • ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static string BuildHeroUnavailableNote(HeroSnapshot hero)
        {
            if (hero == null) return "No hero selected.";
            if (IsIdleHero(hero)) return "Idle hero is available.";
            if (string.Equals(hero.Status, "on_mission", StringComparison.OrdinalIgnoreCase)) return "Hero is currently deployed and cannot be released.";
            return $"Hero status {FirstNonBlank(hero.Status, "unknown")} blocks release.";
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

        private static string FormatRecruitmentTimer(HeroRecruitmentSnapshot recruitment)
        {
            var finish = recruitment?.FinishesAtUtc ?? recruitment?.ReadyAtUtc;
            if (finish.HasValue)
            {
                var remaining = finish.Value - DateTime.UtcNow;
                if (remaining <= TimeSpan.Zero) return "Recruitment scouting is ready now.";
                return $"Recruitment resolves in {FormatDuration(remaining)}.";
            }
            return "Recruitment scouting is active; finish time not surfaced.";
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
