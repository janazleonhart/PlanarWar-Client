using System;
using System.Collections.Generic;

namespace PlanarWar.Client.Core.Contracts
{
    [Serializable]
    public sealed class ShellSummarySnapshot
    {
        public static ShellSummarySnapshot Empty => new();

        public string Username { get; set; } = "Anon";
        public bool FounderMode { get; set; }
        public bool HasCity { get; set; }
        public CitySummarySnapshot City { get; set; } = new();
        public ResourceSnapshot Resources { get; set; } = new();
        public ResourceSnapshot ProductionPerTick { get; set; } = new();
        public TimerSnapshot ResourceTickTiming { get; set; } = new();
        public ResearchSnapshot ActiveResearch { get; set; }
        public List<ThreatWarningSnapshot> ThreatWarnings { get; set; } = new();
        public List<OperationSnapshot> OpeningOperations { get; set; } = new();
        public List<MissionSnapshot> ActiveMissions { get; set; } = new();
        public List<HeroSnapshot> Heroes { get; set; } = new();
        public List<ArmySnapshot> Armies { get; set; } = new();
        public List<WorkshopJobSnapshot> WorkshopJobs { get; set; } = new();
        public List<WarfrontSignalSnapshot> WarfrontSignals { get; set; } = new();
    }

    [Serializable] public sealed class CitySummarySnapshot { public string Name { get; set; } = "-"; public string SettlementLane { get; set; } = "-"; public string SettlementLaneLabel { get; set; } = "-"; public int? Tier { get; set; } }
    [Serializable] public sealed class ResourceSnapshot { public double? Food { get; set; } public double? Materials { get; set; } public double? Wealth { get; set; } public double? Mana { get; set; } public double? Knowledge { get; set; } public double? Unity { get; set; } }
    [Serializable] public sealed class TimerSnapshot { public double? TickMs { get; set; } public DateTime? LastTickAtUtc { get; set; } public DateTime? NextTickAtUtc { get; set; } }
    [Serializable] public sealed class ResearchSnapshot { public string Id { get; set; } public string Name { get; set; } public double? Progress { get; set; } public double? Cost { get; set; } public DateTime? StartedAtUtc { get; set; } }
    [Serializable] public sealed class ThreatWarningSnapshot { public string Headline { get; set; } = "-"; }
    [Serializable] public sealed class OperationSnapshot { public string Title { get; set; } = "Operation"; public string Readiness { get; set; } = "-"; }
    [Serializable] public sealed class MissionSnapshot { public string Id { get; set; } = "mission"; public string Title { get; set; } = "Mission"; public DateTime? FinishesAtUtc { get; set; } }
    [Serializable] public sealed class HeroSnapshot { public string Name { get; set; } = "Hero"; public string Status { get; set; } = "-"; public double? Level { get; set; } public int AttachmentCount { get; set; } }
    [Serializable] public sealed class ArmySnapshot { public string Name { get; set; } = "Army"; public string Status { get; set; } = "-"; public double? Readiness { get; set; } }
    [Serializable] public sealed class WorkshopJobSnapshot { public string AttachmentKind { get; set; } = "job"; public bool Completed { get; set; } public DateTime? FinishesAtUtc { get; set; } }
    [Serializable] public sealed class WarfrontSignalSnapshot { public string Label { get; set; } = "-"; public string Value { get; set; } = "-"; }
}
