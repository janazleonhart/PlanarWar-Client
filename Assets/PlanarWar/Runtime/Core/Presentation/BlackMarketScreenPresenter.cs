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
            var nowUtc = System.DateTime.UtcNow;
            var activeWorkshopJobs = s.WorkshopJobs.Count(j => !IsWorkshopJobCollected(j) && !IsWorkshopJobCollectable(j, nowUtc));
            var readyWorkshopJobs = s.WorkshopJobs.Count(j => !IsWorkshopJobCollected(j) && IsWorkshopJobCollectable(j, nowUtc));
            note.text = $"Workshop jobs: {activeWorkshopJobs} active / {readyWorkshopJobs} ready • Warnings: {s.ThreatWarnings.Count} • Operations: {s.OpeningOperations.Count}";
        }

        private static bool IsWorkshopJobCollected(WorkshopJobSnapshot job)
        {
            return job?.CollectedAtUtc.HasValue == true;
        }

        private static bool IsWorkshopJobCollectable(WorkshopJobSnapshot job, System.DateTime nowUtc)
        {
            if (job == null || IsWorkshopJobCollected(job))
            {
                return false;
            }

            if (job.Completed)
            {
                return true;
            }

            return job.FinishesAtUtc.HasValue && job.FinishesAtUtc.Value <= nowUtc;
        }
    }
}
