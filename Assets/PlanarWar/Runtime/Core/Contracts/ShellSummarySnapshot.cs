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
        public List<TechOptionSnapshot> AvailableTechs { get; set; } = new();
        public List<CityTimerEntrySnapshot> CityTimers { get; set; } = new();
        public List<ThreatWarningSnapshot> ThreatWarnings { get; set; } = new();
        public List<OperationSnapshot> OpeningOperations { get; set; } = new();
        public List<MissionSnapshot> ActiveMissions { get; set; } = new();
        public List<HeroSnapshot> Heroes { get; set; } = new();
        public List<ArmySnapshot> Armies { get; set; } = new();
        public HeroRecruitmentSnapshot HeroRecruitment { get; set; }
        public ArmyReinforcementSnapshot ArmyReinforcement { get; set; }
        public List<WorkshopJobSnapshot> WorkshopJobs { get; set; } = new();
        public List<WarfrontSignalSnapshot> WarfrontSignals { get; set; } = new();
        public PublicBackbonePressureConvergenceSurfaceSnapshot PublicBackbonePressureConvergence { get; set; }
        public BlackMarketRuntimeTruthSurfaceSnapshot BlackMarketRuntimeTruth { get; set; }
        public BlackMarketActiveOperationSurfaceSnapshot BlackMarketActiveOperation { get; set; }
        public BlackMarketBackbonePressureSurfaceSnapshot BlackMarketBackbonePressure { get; set; }
        public BlackMarketPayoffRecoverySurfaceSnapshot BlackMarketPayoffRecovery { get; set; }
    }

    [Serializable] public sealed class CitySummarySnapshot { public string Name { get; set; } = "-"; public string SettlementLane { get; set; } = "-"; public string SettlementLaneLabel { get; set; } = "-"; public int? Tier { get; set; } }
    [Serializable] public sealed class ResourceSnapshot { public double? Food { get; set; } public double? Materials { get; set; } public double? Wealth { get; set; } public double? Mana { get; set; } public double? Knowledge { get; set; } public double? Unity { get; set; } }
    [Serializable] public sealed class TimerSnapshot { public double? TickMs { get; set; } public DateTime? LastTickAtUtc { get; set; } public DateTime? NextTickAtUtc { get; set; } }
    [Serializable] public sealed class ResearchSnapshot { public string Id { get; set; } public string Name { get; set; } public double? Progress { get; set; } public double? Cost { get; set; } public DateTime? StartedAtUtc { get; set; } }
    [Serializable] public sealed class TechOptionSnapshot { public string Id { get; set; } = "tech"; public string Name { get; set; } = "Tech"; public string Description { get; set; } = string.Empty; public string Category { get; set; } = "-"; public double? Cost { get; set; } public string LaneIdentity { get; set; } = "neutral"; public string IdentityFamily { get; set; } = string.Empty; public string IdentitySummary { get; set; } = string.Empty; public string OperatorNote { get; set; } = string.Empty; public List<string> UnlockPreview { get; set; } = new(); }
    [Serializable] public sealed class CityTimerEntrySnapshot { public string Id { get; set; } = "timer"; public string Lane { get; set; } = "city"; public string Category { get; set; } = "-"; public string Label { get; set; } = "Timer"; public string Status { get; set; } = "active"; public DateTime? StartedAtUtc { get; set; } public DateTime? FinishesAtUtc { get; set; } public string Detail { get; set; } = string.Empty; }
    [Serializable] public sealed class ThreatWarningSnapshot { public string Headline { get; set; } = "-"; }
    [Serializable]
    public sealed class OperationSnapshot
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = "Operation";
        public string Summary { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string WhyNow { get; set; } = string.Empty;
        public string Payoff { get; set; } = string.Empty;
        public string Risk { get; set; } = string.Empty;
        public string Lane { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Readiness { get; set; } = "-";
        public string CtaLabel { get; set; } = string.Empty;
        public string FocusLabel { get; set; } = string.Empty;
        public List<string> ImpactPreview { get; set; } = new();
        public string Kind { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string ArmyId { get; set; } = string.Empty;
        public string HeroId { get; set; } = string.Empty;
        public string MissionId { get; set; } = string.Empty;
        public string ActionId { get; set; } = string.Empty;
        public string ResponsePosture { get; set; } = string.Empty;
    }

    [Serializable] public sealed class MissionSnapshot { public string Id { get; set; } = "mission"; public string Title { get; set; } = "Mission"; public string InstanceId { get; set; } = string.Empty; public string RegionId { get; set; } = string.Empty; public string AssignedArmyId { get; set; } = string.Empty; public string AssignedArmyName { get; set; } = string.Empty; public string AssignedHeroId { get; set; } = string.Empty; public string AssignedHeroName { get; set; } = string.Empty; public string ResponsePosture { get; set; } = string.Empty; public DateTime? FinishesAtUtc { get; set; } }
    [Serializable] public sealed class HeroSnapshot { public string Id { get; set; } = string.Empty; public string Name { get; set; } = "Hero"; public string Status { get; set; } = "-"; public string Role { get; set; } = string.Empty; public List<string> ResponseRoles { get; set; } = new(); public double? Level { get; set; } public int AttachmentCount { get; set; } }
    [Serializable] public sealed class ArmySnapshot { public string Id { get; set; } = string.Empty; public string Name { get; set; } = "Army"; public string Type { get; set; } = string.Empty; public string Status { get; set; } = "-"; public double? Readiness { get; set; } public double? Size { get; set; } public double? Power { get; set; } public List<string> Specialties { get; set; } = new(); public string HoldRegionId { get; set; } = string.Empty; public string HoldPosture { get; set; } = string.Empty; }
    [Serializable] public sealed class HeroRecruitmentSnapshot { public string Status { get; set; } = string.Empty; public string Lane { get; set; } = string.Empty; public string Role { get; set; } = string.Empty; public string StartRole { get; set; } = string.Empty; public bool StartEligible { get; set; } public string CtaLabel { get; set; } = string.Empty; public string BlockedReason { get; set; } = string.Empty; public string Shortfall { get; set; } = string.Empty; public DateTime? StartedAtUtc { get; set; } public DateTime? FinishesAtUtc { get; set; } public double? WealthCost { get; set; } public double? UnityCost { get; set; } public DateTime? ReadyAtUtc { get; set; } public DateTime? CandidateExpiresAtUtc { get; set; } public List<HeroRecruitCandidateSnapshot> Candidates { get; set; } = new(); }
    [Serializable] public sealed class HeroRecruitCandidateSnapshot { public string CandidateId { get; set; } = string.Empty; public string Lane { get; set; } = string.Empty; public string Role { get; set; } = string.Empty; public string ClassId { get; set; } = string.Empty; public string ClassName { get; set; } = string.Empty; public string DisplayName { get; set; } = string.Empty; public string Summary { get; set; } = string.Empty; public List<string> Traits { get; set; } = new(); public List<HeroRecruitTraitSnapshot> TraitDetails { get; set; } = new(); public double? WealthCost { get; set; } public double? UnityCost { get; set; } }
    [Serializable] public sealed class HeroRecruitTraitSnapshot { public string Id { get; set; } = string.Empty; public string Name { get; set; } = string.Empty; public string Polarity { get; set; } = string.Empty; public string Summary { get; set; } = string.Empty; }
    [Serializable] public sealed class ArmyReinforcementSnapshot { public string Status { get; set; } = string.Empty; public string ArmyId { get; set; } = string.Empty; public string ArmyName { get; set; } = string.Empty; public string ArmyType { get; set; } = string.Empty; public double? ArmyReadiness { get; set; } public DateTime? StartedAtUtc { get; set; } public DateTime? FinishesAtUtc { get; set; } public double? SizeDelta { get; set; } public double? PowerDelta { get; set; } public double? ReadinessDelta { get; set; } public double? MaterialsCost { get; set; } public double? WealthCost { get; set; } public bool StartEligible { get; set; } public string CtaLabel { get; set; } = string.Empty; public string BlockedReason { get; set; } = string.Empty; public string Shortfall { get; set; } = string.Empty; }
    [Serializable] public sealed class WorkshopJobSnapshot { public string Id { get; set; } = "job"; public string AttachmentKind { get; set; } = "job"; public string RecipeId { get; set; } = string.Empty; public string OutputName { get; set; } = string.Empty; public string OutputItemId { get; set; } = string.Empty; public bool Completed { get; set; } public DateTime? CollectedAtUtc { get; set; } public DateTime? FinishesAtUtc { get; set; } }
    [Serializable] public sealed class WorkshopRecipeSnapshot { public string RecipeId { get; set; } = "recipe"; public string Name { get; set; } = "Recipe"; public string Summary { get; set; } = string.Empty; public string GearFamily { get; set; } = string.Empty; public string OutputItemId { get; set; } = string.Empty; public double? WealthCost { get; set; } public double? ManaCost { get; set; } public double? MaterialsCost { get; set; } public double? CraftMinutes { get; set; } public List<string> ResponseTags { get; set; } = new(); }
    [Serializable] public sealed class WarfrontSignalSnapshot { public string Label { get; set; } = "-"; public string Value { get; set; } = "-"; }

    [Serializable] public sealed class ContractFollowThroughSnapshot { public string Lane { get; set; } = string.Empty; public string State { get; set; } = string.Empty; public string ContractId { get; set; } = string.Empty; public string ContractTitle { get; set; } = string.Empty; public string ContractKind { get; set; } = string.Empty; public string ActiveMissionInstanceId { get; set; } = string.Empty; public DateTime? ActiveMissionFinishesAtUtc { get; set; } public string LatestReceiptId { get; set; } = string.Empty; public DateTime? LatestReceiptAtUtc { get; set; } public string SourceMissionId { get; set; } = string.Empty; public string SourceSurface { get; set; } = string.Empty; public string Note { get; set; } = string.Empty; }
    [Serializable] public sealed class PublicBackboneContractEffectsSnapshot { public string Lane { get; set; } = "city"; public string State { get; set; } = string.Empty; public string ContractId { get; set; } = string.Empty; public string ContractTitle { get; set; } = string.Empty; public string ContractKind { get; set; } = string.Empty; public string QueueEffect { get; set; } = string.Empty; public string TrustEffect { get; set; } = string.Empty; public string ServiceEffect { get; set; } = string.Empty; public string Note { get; set; } = string.Empty; public string SourceSurface { get; set; } = string.Empty; }
    [Serializable] public sealed class ShadowContractEffectsSnapshot { public string Lane { get; set; } = "black_market"; public string State { get; set; } = string.Empty; public string ContractId { get; set; } = string.Empty; public string ContractTitle { get; set; } = string.Empty; public string ContractKind { get; set; } = string.Empty; public string ReceiptChainState { get; set; } = string.Empty; public string CovertCarryState { get; set; } = string.Empty; public string LinkedReceiptId { get; set; } = string.Empty; public string LinkedReceiptTitle { get; set; } = string.Empty; public string Note { get; set; } = string.Empty; public string SourceSurface { get; set; } = string.Empty; }

    [Serializable] public sealed class PublicBackbonePressureConvergenceSurfaceSnapshot { public string Lane { get; set; } = "city"; public string Phase { get; set; } = string.Empty; public string Headline { get; set; } = string.Empty; public string Detail { get; set; } = string.Empty; public string RecommendedAction { get; set; } = string.Empty; public string TradeWindow { get; set; } = string.Empty; public string FocusLane { get; set; } = string.Empty; public int ActiveFrontCount { get; set; } public List<PublicBackbonePressureFrontSnapshot> Fronts { get; set; } = new(); public PublicBackbonePressureReceiptSnapshot LatestSupportReceipt { get; set; } public ContractFollowThroughSnapshot ContractFollowThrough { get; set; } public PublicBackboneContractEffectsSnapshot ContractEffects { get; set; } }
    [Serializable] public sealed class PublicBackbonePressureFrontSnapshot { public string Id { get; set; } = string.Empty; public string Label { get; set; } = string.Empty; public string State { get; set; } = string.Empty; public string Headline { get; set; } = string.Empty; public string Summary { get; set; } = string.Empty; public string RecommendedAction { get; set; } = string.Empty; public string SourceSurface { get; set; } = string.Empty; }
    [Serializable] public sealed class PublicBackbonePressureReceiptSnapshot { public string Id { get; set; } = string.Empty; public DateTime? CreatedAtUtc { get; set; } public string Title { get; set; } = string.Empty; public string Summary { get; set; } = string.Empty; public string SourceSurface { get; set; } = string.Empty; }

    [Serializable] public sealed class BlackMarketRuntimeTruthSurfaceSnapshot { public string Lane { get; set; } = "black_market"; public string RuntimeBand { get; set; } = string.Empty; public string Headline { get; set; } = string.Empty; public string Detail { get; set; } = string.Empty; public string OperatorFrontSummary { get; set; } = string.Empty; public BlackMarketRuntimeLensSnapshot WarningWindow { get; set; } = new(); public BlackMarketRuntimeLensSnapshot ActiveOperation { get; set; } = new(); public BlackMarketRuntimeLensSnapshot PayoffWindow { get; set; } = new(); public BlackMarketPublicPressureSnapshot PublicBackbonePressure { get; set; } = new(); }
    [Serializable] public sealed class BlackMarketRuntimeLensSnapshot { public string State { get; set; } = string.Empty; public string Headline { get; set; } = string.Empty; public string Detail { get; set; } = string.Empty; public string SourceSurface { get; set; } = string.Empty; public List<string> ActionIds { get; set; } = new(); }
    [Serializable] public sealed class BlackMarketPublicPressureSnapshot { public string State { get; set; } = string.Empty; public string Headline { get; set; } = string.Empty; public string Detail { get; set; } = string.Empty; public string RecommendedAction { get; set; } = string.Empty; public string SourceSurface { get; set; } = string.Empty; }

    [Serializable] public sealed class BlackMarketActiveOperationSurfaceSnapshot { public string Lane { get; set; } = "black_market"; public string Headline { get; set; } = string.Empty; public string Detail { get; set; } = string.Empty; public int ActiveCount { get; set; } public int FormingCount { get; set; } public int CoolingCount { get; set; } public List<BlackMarketActiveOperationCardSnapshot> Cards { get; set; } = new(); }
    [Serializable] public sealed class BlackMarketActiveOperationCardSnapshot { public string Id { get; set; } = string.Empty; public string Kind { get; set; } = string.Empty; public string State { get; set; } = string.Empty; public string Headline { get; set; } = string.Empty; public string Summary { get; set; } = string.Empty; public string OperatorNote { get; set; } = string.Empty; public string SourceSurface { get; set; } = string.Empty; public string Risk { get; set; } = string.Empty; public List<string> ActionIds { get; set; } = new(); public List<string> MissionOfferIds { get; set; } = new(); }
    [Serializable] public sealed class BlackMarketBackbonePressureSurfaceSnapshot { public string Lane { get; set; } = "black_market"; public string PressureState { get; set; } = string.Empty; public string LeverageWindow { get; set; } = string.Empty; public string Headline { get; set; } = string.Empty; public string Detail { get; set; } = string.Empty; public string RecommendedAction { get; set; } = string.Empty; public List<string> ActiveActionIds { get; set; } = new(); public ContractFollowThroughSnapshot ContractFollowThrough { get; set; } public ShadowContractEffectsSnapshot ContractEffects { get; set; } }

    [Serializable] public sealed class BlackMarketPayoffRecoverySurfaceSnapshot { public string Lane { get; set; } = "black_market"; public string Phase { get; set; } = string.Empty; public string Severity { get; set; } = string.Empty; public string Headline { get; set; } = string.Empty; public string Detail { get; set; } = string.Empty; public string StateReason { get; set; } = string.Empty; public string RecommendedAction { get; set; } = string.Empty; public List<BlackMarketPayoffRecoveryReceiptSnapshot> RecentReceipts { get; set; } = new(); }
    [Serializable] public sealed class BlackMarketPayoffRecoveryReceiptSnapshot { public string Id { get; set; } = string.Empty; public DateTime? CreatedAtUtc { get; set; } public string Title { get; set; } = string.Empty; public string Summary { get; set; } = string.Empty; public string Detail { get; set; } = string.Empty; public string Severity { get; set; } = string.Empty; public string RuntimeActionId { get; set; } = string.Empty; }
}
