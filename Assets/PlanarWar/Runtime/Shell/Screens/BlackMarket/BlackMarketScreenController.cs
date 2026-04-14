using PlanarWar.Client.Core.Contracts;
using System.Linq;
using UnityEngine.UIElements;

namespace PlanarWar.Client.UI.Screens.BlackMarket
{
    public sealed class BlackMarketScreenController
    {
        private readonly Label headline;
        private readonly Label copy;
        private readonly Label note;

        public BlackMarketScreenController(VisualElement root)
        {
            headline = root.Q<Label>("placeholder-headline-value");
            copy = root.Q<Label>("placeholder-copy-value");
            note = root.Q<Label>("placeholder-note-value");
        }

        public void Render(ShellSummarySnapshot s)
        {
            headline.text = "Black Market overview";
            copy.text = s.City.SettlementLane == "black_market"
                ? "Current lane is Black Market. This read-only desk shows available shell truth."
                : $"Current lane is {s.City.SettlementLaneLabel}. Switch lane server-side to see Black Market-specific truth.";

            var activeWorkshop = s.WorkshopJobs.Count(j => !j.Completed);
            note.text = $"Workshop jobs: {activeWorkshop} • Warnings: {s.ThreatWarnings.Count} • Operations: {s.OpeningOperations.Count}";
        }
    }
}
