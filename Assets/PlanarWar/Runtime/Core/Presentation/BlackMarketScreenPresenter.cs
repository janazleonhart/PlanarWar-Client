using PlanarWar.Client.Core.Contracts;
using System.Linq;
using UnityEngine.UIElements;

namespace PlanarWar.Client.Core.Presentation
{
    public sealed class BlackMarketScreenPresenter
    {
        private readonly Label headline; private readonly Label copy; private readonly Label note;
        public BlackMarketScreenPresenter(VisualElement root)
        {
            headline = root.Q<Label>("placeholder-headline-value"); copy = root.Q<Label>("placeholder-copy-value"); note = root.Q<Label>("placeholder-note-value");
        }
        public void Render(ShellSummarySnapshot s)
        {
            headline.text = "Black Market overview";
            copy.text = s.City.SettlementLane == "black_market" ? "Current lane is Black Market." : $"Current lane is {s.City.SettlementLaneLabel}.";
            note.text = $"Workshop jobs: {s.WorkshopJobs.Count(j => !j.Completed)} • Warnings: {s.ThreatWarnings.Count} • Operations: {s.OpeningOperations.Count}";
        }
    }
}
