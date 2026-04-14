using PlanarWar.Client.Core.Contracts;
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
            card1Title.text = "Missions"; card1Value.text = s.ActiveMissions.Count == 0 ? "No active missions." : string.Join(" • ", s.ActiveMissions.Take(2).Select(m => m.Title));
            card2Title.text = "Heroes"; card2Value.text = s.Heroes.Count == 0 ? "No heroes surfaced." : $"{s.Heroes.Count} total • {s.Heroes.Count(h => h.Status == "idle")} idle";
            card3Title.text = "Warfront"; card3Value.text = s.WarfrontSignals.Count == 0 ? "No warfront snapshot." : string.Join(" • ", s.WarfrontSignals.Take(2).Select(x => $"{x.Label}: {x.Value}"));
        }
    }
}
