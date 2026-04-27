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

            var terms = HeroTerminology.For(s);

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

            if (heroSelectField != null) heroSelectField.label = terms.SingularTitle;
            if (candidateSelectField != null) candidateSelectField.label = terms.CandidateTitle;

            SyncHeroDropdown(heroes, terms);
            SyncCandidateDropdown(candidates, terms);

            var selectedHero = heroes.FirstOrDefault(hero => string.Equals(hero.Id, selectedHeroId, StringComparison.OrdinalIgnoreCase));
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
                acceptCandidateButton.text = selectedCandidate == null ? $"Select {terms.CandidateLower}" : $"Accept {selectedCandidate.DisplayName}";
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
                noteValue.text = BuildDeskNote(selectedHero, candidates, recruitment, isBusy, terms);
            }

            var cards = BuildCards(heroes, recruitment, candidates, isBusy, terms).Take(heroCards.Length).ToList();
            for (var i = 0; i < heroCards.Length; i++)
            {
                if (i < cards.Count) heroCards[i].Show(cards[i]);
                else heroCards[i].Hide();
            }
        }

        private List<CardView> BuildCards(List<HeroSnapshot> heroes, HeroRecruitmentSnapshot recruitment, List<HeroRecruitCandidateSnapshot> candidates, bool isBusy, HeroTerminology terms)
        {
            var cards = new List<CardView>();

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

        private static string DescribeRecruitment(HeroRecruitmentSnapshot recruitment, List<HeroRecruitCandidateSnapshot> candidates, HeroTerminology terms)
        {
            if (recruitment == null) return $"No {terms.RecruitmentFamily.ToLowerInvariant()} surface in payload.";
            if (candidates != null && candidates.Count > 0) return $"{candidates.Count} {terms.CandidateLower}{Plural(candidates.Count)} ready • expires {FormatTime(recruitment.CandidateExpiresAtUtc)}";
            if (string.Equals(recruitment.Status, "scouting", StringComparison.OrdinalIgnoreCase)) return FormatRecruitmentTimer(recruitment, terms);
            if (recruitment.StartEligible) return $"{terms.RecruitmentReadyText} • {CostText(recruitment.WealthCost, recruitment.UnityCost)}";
            return FirstNonBlank(recruitment.BlockedReason, recruitment.Shortfall, $"{terms.RecruitmentFamily} {FirstNonBlank(recruitment.Status, "blocked")}");
        }

        private string BuildDeskNote(HeroSnapshot selectedHero, List<HeroRecruitCandidateSnapshot> candidates, HeroRecruitmentSnapshot recruitment, bool isBusy, HeroTerminology terms)
        {
            if (isBusy && !string.IsNullOrWhiteSpace(summaryState?.ActionStatus)) return summaryState.ActionStatus;
            if (selectedHero != null && string.Equals(pendingReleaseHeroId, selectedHero.Id, StringComparison.OrdinalIgnoreCase)) return $"Confirm {terms.ReleaseVerbLower} for {selectedHero.Name}. This removes the {terms.SingularLower} from the roster; backend release should return equipped items when present.";
            if (candidates != null && candidates.Count > 0) return $"{terms.CandidateTitle} slate is ready. Accept one {terms.CandidateLower} or dismiss the slate from this desk.";
            if (recruitment?.StartEligible == true) return $"{terms.RecruitmentFamily} can be opened from this {terms.DeskTitle.ToLowerInvariant()} without returning to Development.";
            if (selectedHero != null) return $"Selected {selectedHero.Name}: {BuildHeroLore(selectedHero, terms)}";
            return FirstNonBlank(recruitment?.BlockedReason, recruitment?.Shortfall, $"{terms.DeskTitle} is ready, but no actionable roster/{terms.CandidateLower} truth is visible yet.");
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
            private bool IsOperative => string.Equals(SingularLower, "operative", StringComparison.OrdinalIgnoreCase);

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
