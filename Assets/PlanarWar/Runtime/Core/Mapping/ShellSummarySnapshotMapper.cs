using Newtonsoft.Json.Linq;
using PlanarWar.Client.Core.Contracts;
using PlanarWar.Client.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanarWar.Client.Core.Mapping
{
    public static class ShellSummarySnapshotMapper
    {
        public static ShellSummarySnapshot Map(JObject summary)
        {
            if (summary == null) return ShellSummarySnapshot.Empty;

            var city = summary["city"] as JObject;
            var resourceTickTiming = FirstObject(
                summary["resourceTickTiming"],
                summary["resource_tick_timing"],
                summary["resourceTick"],
                summary["resource_tick"],
                city?["resourceTickTiming"],
                city?["resource_tick_timing"],
                city?["resourceTick"],
                city?["resource_tick"]);
            var activeMissions = FirstArray(
                summary["activeMissions"],
                summary["active_missions"],
                summary["missions"]?["activeMissions"],
                summary["missions"]?["active_missions"],
                city?["activeMissions"],
                city?["active_missions"]);
            var availableTechs = FirstArray(
                summary["availableTechs"],
                summary["available_techs"],
                city?["availableTechs"],
                city?["available_techs"]);
            var cityTimers = FirstArray(
                summary["cityTimers"],
                summary["city_timers"],
                summary["timers"],
                city?["cityTimers"],
                city?["city_timers"],
                city?["timers"]);
            var openingOps = FirstArray(
                summary["settlementOpeningOperations"],
                summary["settlement_opening_operations"],
                summary["openingOperations"],
                summary["opening_operations"],
                city?["settlementOpeningOperations"],
                city?["settlement_opening_operations"],
                city?["openingOperations"],
                city?["opening_operations"]);

            var mapped = new ShellSummarySnapshot
            {
                Username = summary["username"]?.Read<string>() ?? "Anon",
                FounderMode = summary["founderMode"]?.Read<bool>() ?? false,
                HasCity = summary["hasCity"]?.Read<bool>() ?? false,
                City = new CitySummarySnapshot
                {
                    Name = city?["name"]?.Read<string>() ?? "-",
                    SettlementLane = city?["settlementLane"]?.Read<string>() ?? city?["settlement_lane"]?.Read<string>() ?? "-",
                    SettlementLaneLabel = city?["settlementLaneProfile"]?["label"]?.Read<string>() ?? city?["settlement_lane_profile"]?["label"]?.Read<string>() ?? city?["settlementLane"]?.Read<string>() ?? city?["settlement_lane"]?.Read<string>() ?? "-",
                    Tier = city?["tier"]?.Read<int?>()
                },
                Resources = MapResource(summary["resources"] as JObject),
                ProductionPerTick = MapResource(city?["production"] as JObject, true),
                ResourceTickTiming = MapTimer(resourceTickTiming),
                ActiveResearch = MapResearch(summary["activeResearch"] as JObject ?? summary["active_research"] as JObject),
                AvailableTechs = availableTechs?.OfType<JObject>().Select(MapTech).Where(t => t != null).ToList() ?? new List<TechOptionSnapshot>(),
                CityTimers = cityTimers?.OfType<JObject>().Select(MapCityTimer).Where(t => t != null).ToList() ?? new List<CityTimerEntrySnapshot>(),
                ThreatWarnings = FirstArray(summary["threatWarnings"], summary["threat_warnings"])?.OfType<JObject>().Select(MapThreatWarning).Where(w => w != null).ToList() ?? new List<ThreatWarningSnapshot>(),
                OpeningOperations = openingOps?.OfType<JObject>().Select(MapOperation).Where(o => o != null).ToList() ?? new List<OperationSnapshot>(),
                ActiveMissions = activeMissions?.OfType<JObject>().Select(MapMission).Where(m => m != null).ToList() ?? new List<MissionSnapshot>(),
                Heroes = FirstArray(summary["heroes"], summary["Heroes"])?.OfType<JObject>().Select(MapHero).Where(h => h != null).ToList() ?? new List<HeroSnapshot>(),
                Armies = FirstArray(summary["armies"], summary["Armies"])?.OfType<JObject>().Select(MapArmy).Where(a => a != null).ToList() ?? new List<ArmySnapshot>(),
                HeroRecruitment = MapHeroRecruitment(summary["heroRecruitment"] as JObject ?? summary["hero_recruitment"] as JObject),
                ArmyReinforcement = MapArmyReinforcement(summary["armyReinforcement"] as JObject ?? summary["army_reinforcement"] as JObject),
                WorkshopJobs = FirstArray(summary["workshopJobs"], summary["workshop_jobs"])?.OfType<JObject>().Select(MapWorkshopJob).Where(j => j != null).ToList() ?? new List<WorkshopJobSnapshot>(),
                WarfrontSignals = (summary["warfrontStatus"] as JObject)?.Properties().Select(p => new WarfrontSignalSnapshot { Label = p.Name, Value = p.Value?.ToString() ?? "-" }).ToList()
                    ?? (summary["warfront"] as JObject)?.Properties().Select(p => new WarfrontSignalSnapshot { Label = p.Name, Value = p.Value?.ToString() ?? "-" }).ToList()
                    ?? new List<WarfrontSignalSnapshot>(),
                PublicBackbonePressureConvergence = MapPublicBackbonePressureConvergence(summary["publicBackbonePressureConvergenceSurface"] as JObject ?? summary["public_backbone_pressure_convergence_surface"] as JObject),
                BlackMarketRuntimeTruth = MapBlackMarketRuntimeTruth(summary["blackMarketRuntimeTruthSurface"] as JObject ?? summary["black_market_runtime_truth_surface"] as JObject),
                BlackMarketActiveOperation = MapBlackMarketActiveOperation(summary["blackMarketActiveOperationSurface"] as JObject ?? summary["black_market_active_operation_surface"] as JObject),
                BlackMarketPayoffRecovery = MapBlackMarketPayoffRecovery(summary["blackMarketPayoffRecoverySurface"] as JObject ?? summary["black_market_payoff_recovery_surface"] as JObject),
            };

            ResolveActiveMissionAssignments(mapped);
            return mapped;
        }

        private static ThreatWarningSnapshot MapThreatWarning(JObject obj)
        {
            if (obj == null) return null;
            return new ThreatWarningSnapshot
            {
                Headline = FirstNonBlank(
                    obj["headline"]?.Read<string>(),
                    obj["title"]?.Read<string>(),
                    obj["summary"]?.Read<string>(),
                    "Warning")
            };
        }

        private static MissionSnapshot MapMission(JObject obj)
        {
            if (obj == null) return null;
            return new MissionSnapshot
            {
                Id = obj["mission"]?["id"]?.Read<string>() ?? obj["id"]?.Read<string>() ?? "mission",
                Title = obj["mission"]?["title"]?.Read<string>() ?? obj["title"]?.Read<string>() ?? obj["mission"]?["id"]?.Read<string>() ?? "Mission",
                InstanceId = obj["instanceId"]?.Read<string>() ?? obj["instance_id"]?.Read<string>() ?? string.Empty,
                RegionId = obj["mission"]?["regionId"]?.Read<string>() ?? obj["mission"]?["region_id"]?.Read<string>() ?? obj["regionId"]?.Read<string>() ?? obj["region_id"]?.Read<string>() ?? string.Empty,
                AssignedArmyId = obj["assignedArmyId"]?.Read<string>() ?? obj["assigned_army_id"]?.Read<string>() ?? string.Empty,
                AssignedHeroId = obj["assignedHeroId"]?.Read<string>() ?? obj["assigned_hero_id"]?.Read<string>() ?? string.Empty,
                ResponsePosture = obj["responsePosture"]?.Read<string>() ?? obj["response_posture"]?.Read<string>() ?? string.Empty,
                FinishesAtUtc = ParseUtc(
                    obj["finishesAt"]
                    ?? obj["finishes_at"]
                    ?? obj["finishAt"]
                    ?? obj["finish_at"]
                    ?? obj["endsAt"]
                    ?? obj["ends_at"]
                    ?? obj["deadlineAt"]
                    ?? obj["deadline_at"]
                    ?? obj["timing"]?["finishesAt"]
                    ?? obj["timing"]?["finishes_at"]
                    ?? obj["timing"]?["finishAt"]
                    ?? obj["timing"]?["finish_at"]
                    ?? obj["timing"]?["endsAt"]
                    ?? obj["timing"]?["ends_at"])
            };
        }

        private static HeroSnapshot MapHero(JObject obj)
        {
            if (obj == null) return null;
            return new HeroSnapshot
            {
                Id = obj["id"]?.Read<string>() ?? string.Empty,
                Name = obj["name"]?.Read<string>() ?? "Hero",
                Status = obj["status"]?.Read<string>() ?? "-",
                Role = obj["role"]?.Read<string>() ?? string.Empty,
                ResponseRoles = (obj["responseRoles"] as JArray)?.Select(r => r?.Read<string>()).Where(r => !string.IsNullOrWhiteSpace(r)).ToList()
                    ?? (obj["response_roles"] as JArray)?.Select(r => r?.Read<string>()).Where(r => !string.IsNullOrWhiteSpace(r)).ToList()
                    ?? new List<string>(),
                Level = obj["level"]?.Read<double?>(),
                AttachmentCount = (obj["attachments"] as JArray)?.Count ?? 0,
            };
        }

        private static ArmySnapshot MapArmy(JObject obj)
        {
            if (obj == null) return null;
            return new ArmySnapshot
            {
                Id = obj["id"]?.Read<string>() ?? string.Empty,
                Name = obj["name"]?.Read<string>() ?? "Army",
                Type = obj["type"]?.Read<string>() ?? string.Empty,
                Status = obj["status"]?.Read<string>() ?? "-",
                Readiness = obj["readiness"]?.Read<double?>(),
                Size = obj["size"]?.Read<double?>(),
                Power = obj["power"]?.Read<double?>(),
                Specialties = (obj["specialties"] as JArray)?.Select(s => s?.Read<string>()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList() ?? new List<string>(),
                HoldRegionId = obj["hold"]?["regionId"]?.Read<string>() ?? obj["hold"]?["region_id"]?.Read<string>() ?? string.Empty,
                HoldPosture = obj["hold"]?["posture"]?.Read<string>() ?? string.Empty,
            };
        }

        private static WorkshopJobSnapshot MapWorkshopJob(JObject obj)
        {
            if (obj == null) return null;
            return new WorkshopJobSnapshot
            {
                Id = obj["id"]?.Read<string>() ?? obj["jobId"]?.Read<string>() ?? obj["job_id"]?.Read<string>() ?? "job",
                AttachmentKind = obj["attachmentKind"]?.Read<string>() ?? obj["attachment_kind"]?.Read<string>() ?? "job",
                RecipeId = obj["recipeId"]?.Read<string>() ?? obj["recipe_id"]?.Read<string>() ?? string.Empty,
                OutputName = obj["outputName"]?.Read<string>() ?? obj["output_name"]?.Read<string>() ?? string.Empty,
                OutputItemId = obj["outputItemId"]?.Read<string>() ?? obj["output_item_id"]?.Read<string>() ?? string.Empty,
                Completed = obj["completed"]?.Read<bool>() ?? false,
                CollectedAtUtc = ParseUtc(obj["collectedAt"] ?? obj["collected_at"]),
                FinishesAtUtc = ParseUtc(obj["finishesAt"] ?? obj["finishes_at"]),
            };
        }

        private static void ResolveActiveMissionAssignments(ShellSummarySnapshot summary)
        {
            if (summary == null || summary.ActiveMissions == null || summary.ActiveMissions.Count == 0)
            {
                return;
            }

            foreach (var mission in summary.ActiveMissions)
            {
                if (mission == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(mission.AssignedArmyId) && string.IsNullOrWhiteSpace(mission.AssignedArmyName))
                {
                    mission.AssignedArmyName = summary.Armies?
                        .FirstOrDefault(a => string.Equals(a?.Id, mission.AssignedArmyId, StringComparison.OrdinalIgnoreCase))
                        ?.Name ?? string.Empty;
                }

                if (!string.IsNullOrWhiteSpace(mission.AssignedHeroId) && string.IsNullOrWhiteSpace(mission.AssignedHeroName))
                {
                    mission.AssignedHeroName = summary.Heroes?
                        .FirstOrDefault(h => string.Equals(h?.Id, mission.AssignedHeroId, StringComparison.OrdinalIgnoreCase))
                        ?.Name ?? string.Empty;
                }
            }
        }

        private static HeroRecruitmentSnapshot MapHeroRecruitment(JObject obj)
        {
            if (obj == null) return null;
            return new HeroRecruitmentSnapshot
            {
                Status = obj["status"]?.Read<string>() ?? string.Empty,
                Lane = obj["lane"]?.Read<string>() ?? string.Empty,
                Role = obj["role"]?.Read<string>() ?? string.Empty,
                StartRole = obj["startRole"]?.Read<string>() ?? obj["start_role"]?.Read<string>() ?? string.Empty,
                StartEligible = obj["startEligible"]?.Read<bool>() ?? obj["start_eligible"]?.Read<bool>() ?? false,
                CtaLabel = obj["ctaLabel"]?.Read<string>() ?? obj["cta_label"]?.Read<string>() ?? string.Empty,
                BlockedReason = obj["blockedReason"]?.Read<string>() ?? obj["blocked_reason"]?.Read<string>() ?? string.Empty,
                Shortfall = obj["shortfall"]?.Read<string>() ?? string.Empty,
                StartedAtUtc = ParseUtc(obj["startedAt"] ?? obj["started_at"]),
                FinishesAtUtc = ParseUtc(obj["finishesAt"] ?? obj["finishes_at"]),
                WealthCost = obj["wealthCost"]?.Read<double?>() ?? obj["wealth_cost"]?.Read<double?>(),
                UnityCost = obj["unityCost"]?.Read<double?>() ?? obj["unity_cost"]?.Read<double?>(),
                ReadyAtUtc = ParseUtc(obj["readyAt"] ?? obj["ready_at"]),
                CandidateExpiresAtUtc = ParseUtc(obj["candidateExpiresAt"] ?? obj["candidate_expires_at"]),
                Candidates = (obj["candidates"] as JArray)?.OfType<JObject>().Select(MapHeroRecruitCandidate).Where(c => c != null).ToList() ?? new List<HeroRecruitCandidateSnapshot>(),
            };
        }

        private static HeroRecruitCandidateSnapshot MapHeroRecruitCandidate(JObject obj)
        {
            if (obj == null) return null;
            return new HeroRecruitCandidateSnapshot
            {
                CandidateId = obj["candidateId"]?.Read<string>() ?? obj["candidate_id"]?.Read<string>() ?? string.Empty,
                Lane = obj["lane"]?.Read<string>() ?? string.Empty,
                Role = obj["role"]?.Read<string>() ?? string.Empty,
                ClassId = obj["classId"]?.Read<string>() ?? obj["class_id"]?.Read<string>() ?? string.Empty,
                ClassName = obj["className"]?.Read<string>() ?? obj["class_name"]?.Read<string>() ?? string.Empty,
                DisplayName = obj["displayName"]?.Read<string>() ?? obj["display_name"]?.Read<string>() ?? string.Empty,
                Summary = obj["summary"]?.Read<string>() ?? string.Empty,
                Traits = (obj["traits"] as JArray)?.Select(t =>
                    t is JObject traitObj
                        ? FirstNonBlank(traitObj["name"]?.Read<string>(), traitObj["id"]?.Read<string>())
                        : t?.Read<string>())
                    .Where(t => !string.IsNullOrWhiteSpace(t)).ToList() ?? new List<string>(),
                TraitDetails = (obj["traits"] as JArray)?.OfType<JObject>().Select(MapHeroRecruitTrait).Where(t => t != null).ToList() ?? new List<HeroRecruitTraitSnapshot>(),
                WealthCost = obj["cost"]?["wealth"]?.Read<double?>() ?? obj["wealthCost"]?.Read<double?>() ?? obj["wealth_cost"]?.Read<double?>(),
                UnityCost = obj["cost"]?["unity"]?.Read<double?>() ?? obj["unityCost"]?.Read<double?>() ?? obj["unity_cost"]?.Read<double?>(),
            };
        }

        private static HeroRecruitTraitSnapshot MapHeroRecruitTrait(JObject obj)
        {
            if (obj == null) return null;
            return new HeroRecruitTraitSnapshot
            {
                Id = obj["id"]?.Read<string>() ?? string.Empty,
                Name = obj["name"]?.Read<string>() ?? string.Empty,
                Polarity = obj["polarity"]?.Read<string>() ?? string.Empty,
                Summary = obj["summary"]?.Read<string>() ?? string.Empty,
            };
        }

        private static ArmyReinforcementSnapshot MapArmyReinforcement(JObject obj)
        {
            if (obj == null) return null;
            return new ArmyReinforcementSnapshot
            {
                Status = obj["status"]?.Read<string>() ?? string.Empty,
                ArmyId = obj["armyId"]?.Read<string>() ?? obj["army_id"]?.Read<string>() ?? string.Empty,
                ArmyName = obj["armyName"]?.Read<string>() ?? obj["army_name"]?.Read<string>() ?? string.Empty,
                ArmyType = obj["armyType"]?.Read<string>() ?? obj["army_type"]?.Read<string>() ?? string.Empty,
                ArmyReadiness = obj["armyReadiness"]?.Read<double?>() ?? obj["army_readiness"]?.Read<double?>(),
                StartedAtUtc = ParseUtc(obj["startedAt"] ?? obj["started_at"]),
                FinishesAtUtc = ParseUtc(obj["finishesAt"] ?? obj["finishes_at"]),
                SizeDelta = obj["sizeDelta"]?.Read<double?>() ?? obj["size_delta"]?.Read<double?>(),
                PowerDelta = obj["powerDelta"]?.Read<double?>() ?? obj["power_delta"]?.Read<double?>(),
                ReadinessDelta = obj["readinessDelta"]?.Read<double?>() ?? obj["readiness_delta"]?.Read<double?>(),
                MaterialsCost = obj["materialsCost"]?.Read<double?>() ?? obj["materials_cost"]?.Read<double?>(),
                WealthCost = obj["wealthCost"]?.Read<double?>() ?? obj["wealth_cost"]?.Read<double?>(),
                StartEligible = obj["startEligible"]?.Read<bool>() ?? obj["start_eligible"]?.Read<bool>() ?? false,
                CtaLabel = obj["ctaLabel"]?.Read<string>() ?? obj["cta_label"]?.Read<string>() ?? string.Empty,
                BlockedReason = obj["blockedReason"]?.Read<string>() ?? obj["blocked_reason"]?.Read<string>() ?? string.Empty,
                Shortfall = obj["shortfall"]?.Read<string>() ?? string.Empty,
            };
        }

        private static OperationSnapshot MapOperation(JObject obj)
        {
            if (obj == null) return null;
            var action = obj["action"] as JObject;
            return new OperationSnapshot
            {
                Id = obj["id"]?.Read<string>() ?? string.Empty,
                Title = obj["title"]?.Read<string>() ?? "Operation",
                Summary = obj["summary"]?.Read<string>() ?? string.Empty,
                Detail = obj["detail"]?.Read<string>() ?? string.Empty,
                WhyNow = obj["whyNow"]?.Read<string>() ?? obj["why_now"]?.Read<string>() ?? string.Empty,
                Payoff = obj["payoff"]?.Read<string>() ?? string.Empty,
                Risk = obj["risk"]?.Read<string>() ?? string.Empty,
                Lane = obj["lane"]?.Read<string>() ?? string.Empty,
                Priority = obj["priority"]?.Read<string>() ?? string.Empty,
                Readiness = obj["readiness"]?.Read<string>() ?? "-",
                CtaLabel = obj["ctaLabel"]?.Read<string>() ?? obj["cta_label"]?.Read<string>() ?? string.Empty,
                FocusLabel = obj["focusLabel"]?.Read<string>() ?? obj["focus_label"]?.Read<string>() ?? string.Empty,
                ImpactPreview = (obj["impactPreview"] as JArray)?.Values<string>().Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                    ?? (obj["impact_preview"] as JArray)?.Values<string>().Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                    ?? new List<string>(),
                Kind = action?["kind"]?.Read<string>() ?? string.Empty,
                Role = action?["role"]?.Read<string>() ?? string.Empty,
                ArmyId = action?["armyId"]?.Read<string>() ?? action?["army_id"]?.Read<string>() ?? string.Empty,
                HeroId = action?["heroId"]?.Read<string>() ?? action?["hero_id"]?.Read<string>() ?? string.Empty,
                MissionId = action?["missionId"]?.Read<string>() ?? action?["mission_id"]?.Read<string>() ?? string.Empty,
                ActionId = action?["actionId"]?.Read<string>() ?? action?["action_id"]?.Read<string>() ?? string.Empty,
                ResponsePosture = action?["responsePosture"]?.Read<string>() ?? action?["response_posture"]?.Read<string>() ?? string.Empty,
            };
        }

        private static ResourceSnapshot MapResource(JObject obj, bool perTick = false)
        {
            if (obj == null) return new ResourceSnapshot();
            return new ResourceSnapshot
            {
                Food = obj[perTick ? "foodPerTick" : "food"]?.Read<double?>(),
                Materials = obj[perTick ? "materialsPerTick" : "materials"]?.Read<double?>(),
                Wealth = obj[perTick ? "wealthPerTick" : "wealth"]?.Read<double?>(),
                Mana = obj[perTick ? "manaPerTick" : "mana"]?.Read<double?>(),
                Knowledge = obj[perTick ? "knowledgePerTick" : "knowledge"]?.Read<double?>(),
                Unity = obj[perTick ? "unityPerTick" : "unity"]?.Read<double?>(),
            };
        }

        private static TimerSnapshot MapTimer(JObject obj)
        {
            if (obj == null) return new TimerSnapshot();
            return new TimerSnapshot
            {
                TickMs = obj["tickMs"]?.Read<double?>()
                         ?? obj["tick_ms"]?.Read<double?>()
                         ?? obj["cadenceMs"]?.Read<double?>()
                         ?? obj["cadence_ms"]?.Read<double?>()
                         ?? obj["intervalMs"]?.Read<double?>()
                         ?? obj["interval_ms"]?.Read<double?>(),
                LastTickAtUtc = ParseUtc(obj["lastTickAt"] ?? obj["last_tick_at"] ?? obj["lastAt"] ?? obj["last_at"] ?? obj["lastTick"] ?? obj["last_tick"]),
                NextTickAtUtc = ParseUtc(obj["nextTickAt"] ?? obj["next_tick_at"] ?? obj["nextAt"] ?? obj["next_at"] ?? obj["nextTick"] ?? obj["next_tick"])
            };
        }

        private static TechOptionSnapshot MapTech(JObject obj)
        {
            if (obj == null) return null;
            return new TechOptionSnapshot
            {
                Id = obj["id"]?.Read<string>() ?? "tech",
                Name = obj["name"]?.Read<string>() ?? obj["id"]?.Read<string>() ?? "Tech",
                Description = obj["description"]?.Read<string>() ?? string.Empty,
                Category = obj["category"]?.Read<string>() ?? "-",
                Cost = obj["cost"]?.Read<double?>(),
                LaneIdentity = obj["laneIdentity"]?.Read<string>() ?? obj["lane_identity"]?.Read<string>() ?? "neutral",
                IdentityFamily = obj["identityFamily"]?.Read<string>() ?? obj["identity_family"]?.Read<string>() ?? string.Empty,
                IdentitySummary = obj["identitySummary"]?.Read<string>() ?? obj["identity_summary"]?.Read<string>() ?? string.Empty,
                OperatorNote = obj["operatorNote"]?.Read<string>() ?? obj["operator_note"]?.Read<string>() ?? string.Empty,
                UnlockPreview = (obj["unlockPreview"] as JArray)?.Values<string>().Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                    ?? (obj["unlock_preview"] as JArray)?.Values<string>().Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                    ?? new List<string>()
            };
        }

        private static CityTimerEntrySnapshot MapCityTimer(JObject obj)
        {
            if (obj == null) return null;
            return new CityTimerEntrySnapshot
            {
                Id = obj["id"]?.Read<string>() ?? "timer",
                Lane = obj["lane"]?.Read<string>() ?? "city",
                Category = obj["category"]?.Read<string>() ?? "-",
                Label = obj["label"]?.Read<string>() ?? obj["id"]?.Read<string>() ?? "Timer",
                Status = obj["status"]?.Read<string>() ?? "active",
                StartedAtUtc = ParseUtc(obj["startedAt"] ?? obj["started_at"]),
                FinishesAtUtc = ParseUtc(obj["finishesAt"] ?? obj["finishes_at"]),
                Detail = obj["detail"]?.Read<string>() ?? string.Empty
            };
        }

        private static PublicBackbonePressureConvergenceSurfaceSnapshot MapPublicBackbonePressureConvergence(JObject obj)
        {
            if (obj == null) return null;
            return new PublicBackbonePressureConvergenceSurfaceSnapshot
            {
                Lane = obj["lane"]?.Read<string>() ?? "city",
                Phase = obj["phase"]?.Read<string>() ?? string.Empty,
                Headline = obj["headline"]?.Read<string>() ?? string.Empty,
                Detail = obj["detail"]?.Read<string>() ?? string.Empty,
                RecommendedAction = obj["recommendedAction"]?.Read<string>() ?? obj["recommended_action"]?.Read<string>() ?? string.Empty,
                TradeWindow = obj["tradeWindow"]?.Read<string>() ?? obj["trade_window"]?.Read<string>() ?? string.Empty,
                FocusLane = obj["focusLane"]?.Read<string>() ?? obj["focus_lane"]?.Read<string>() ?? string.Empty,
                ActiveFrontCount = obj["activeFrontCount"]?.Read<int>() ?? obj["active_front_count"]?.Read<int>() ?? 0,
                Fronts = (obj["fronts"] as JArray)?.OfType<JObject>().Select(MapPublicBackbonePressureFront).Where(f => f != null).ToList() ?? new List<PublicBackbonePressureFrontSnapshot>(),
                LatestSupportReceipt = MapPublicBackbonePressureReceipt(obj["latestSupportReceipt"] as JObject ?? obj["latest_support_receipt"] as JObject),
            };
        }

        private static PublicBackbonePressureFrontSnapshot MapPublicBackbonePressureFront(JObject obj)
        {
            if (obj == null) return null;
            return new PublicBackbonePressureFrontSnapshot
            {
                Id = obj["id"]?.Read<string>() ?? string.Empty,
                Label = obj["label"]?.Read<string>() ?? string.Empty,
                State = obj["state"]?.Read<string>() ?? string.Empty,
                Headline = obj["headline"]?.Read<string>() ?? string.Empty,
                Summary = obj["summary"]?.Read<string>() ?? string.Empty,
                RecommendedAction = obj["recommendedAction"]?.Read<string>() ?? obj["recommended_action"]?.Read<string>() ?? string.Empty,
                SourceSurface = obj["sourceSurface"]?.Read<string>() ?? obj["source_surface"]?.Read<string>() ?? string.Empty,
            };
        }

        private static PublicBackbonePressureReceiptSnapshot MapPublicBackbonePressureReceipt(JObject obj)
        {
            if (obj == null) return null;
            return new PublicBackbonePressureReceiptSnapshot
            {
                Id = obj["id"]?.Read<string>() ?? string.Empty,
                CreatedAtUtc = ParseUtc(obj["createdAt"] ?? obj["created_at"]),
                Title = obj["title"]?.Read<string>() ?? string.Empty,
                Summary = obj["summary"]?.Read<string>() ?? string.Empty,
                SourceSurface = obj["sourceSurface"]?.Read<string>() ?? obj["source_surface"]?.Read<string>() ?? string.Empty,
            };
        }

        private static BlackMarketRuntimeTruthSurfaceSnapshot MapBlackMarketRuntimeTruth(JObject obj)
        {
            if (obj == null) return null;
            return new BlackMarketRuntimeTruthSurfaceSnapshot
            {
                Lane = obj["lane"]?.Read<string>() ?? "black_market",
                RuntimeBand = obj["runtimeBand"]?.Read<string>() ?? obj["runtime_band"]?.Read<string>() ?? string.Empty,
                Headline = obj["headline"]?.Read<string>() ?? string.Empty,
                Detail = obj["detail"]?.Read<string>() ?? string.Empty,
                OperatorFrontSummary = obj["operatorFrontSummary"]?.Read<string>() ?? obj["operator_front_summary"]?.Read<string>() ?? string.Empty,
                WarningWindow = MapBlackMarketRuntimeLens(obj["warningWindow"] as JObject ?? obj["warning_window"] as JObject),
                ActiveOperation = MapBlackMarketRuntimeLens(obj["activeOperation"] as JObject ?? obj["active_operation"] as JObject),
                PayoffWindow = MapBlackMarketRuntimeLens(obj["payoffWindow"] as JObject ?? obj["payoff_window"] as JObject),
                PublicBackbonePressure = MapBlackMarketPublicPressure(obj["publicBackbonePressure"] as JObject ?? obj["public_backbone_pressure"] as JObject),
            };
        }

        private static BlackMarketRuntimeLensSnapshot MapBlackMarketRuntimeLens(JObject obj)
        {
            if (obj == null) return new BlackMarketRuntimeLensSnapshot();
            return new BlackMarketRuntimeLensSnapshot
            {
                State = obj["state"]?.Read<string>() ?? string.Empty,
                Headline = obj["headline"]?.Read<string>() ?? string.Empty,
                Detail = obj["detail"]?.Read<string>() ?? string.Empty,
                SourceSurface = obj["sourceSurface"]?.Read<string>() ?? obj["source_surface"]?.Read<string>() ?? string.Empty,
                ActionIds = (obj["actionIds"] as JArray)?.Values<string>().Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                    ?? (obj["action_ids"] as JArray)?.Values<string>().Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                    ?? new List<string>(),
            };
        }

        private static BlackMarketPublicPressureSnapshot MapBlackMarketPublicPressure(JObject obj)
        {
            if (obj == null) return new BlackMarketPublicPressureSnapshot();
            return new BlackMarketPublicPressureSnapshot
            {
                State = obj["state"]?.Read<string>() ?? string.Empty,
                Headline = obj["headline"]?.Read<string>() ?? string.Empty,
                Detail = obj["detail"]?.Read<string>() ?? string.Empty,
                RecommendedAction = obj["recommendedAction"]?.Read<string>() ?? obj["recommended_action"]?.Read<string>() ?? string.Empty,
                SourceSurface = obj["sourceSurface"]?.Read<string>() ?? obj["source_surface"]?.Read<string>() ?? string.Empty,
            };
        }

        private static BlackMarketActiveOperationSurfaceSnapshot MapBlackMarketActiveOperation(JObject obj)
        {
            if (obj == null) return null;
            return new BlackMarketActiveOperationSurfaceSnapshot
            {
                Lane = obj["lane"]?.Read<string>() ?? "black_market",
                Headline = obj["headline"]?.Read<string>() ?? string.Empty,
                Detail = obj["detail"]?.Read<string>() ?? string.Empty,
                ActiveCount = obj["activeCount"]?.Read<int>() ?? obj["active_count"]?.Read<int>() ?? 0,
                FormingCount = obj["formingCount"]?.Read<int>() ?? obj["forming_count"]?.Read<int>() ?? 0,
                CoolingCount = obj["coolingCount"]?.Read<int>() ?? obj["cooling_count"]?.Read<int>() ?? 0,
                Cards = (obj["cards"] as JArray)?.OfType<JObject>().Select(MapBlackMarketActiveOperationCard).Where(c => c != null).ToList() ?? new List<BlackMarketActiveOperationCardSnapshot>(),
            };
        }

        private static BlackMarketActiveOperationCardSnapshot MapBlackMarketActiveOperationCard(JObject obj)
        {
            if (obj == null) return null;
            return new BlackMarketActiveOperationCardSnapshot
            {
                Id = obj["id"]?.Read<string>() ?? string.Empty,
                Kind = obj["kind"]?.Read<string>() ?? string.Empty,
                State = obj["state"]?.Read<string>() ?? string.Empty,
                Headline = obj["headline"]?.Read<string>() ?? string.Empty,
                Summary = obj["summary"]?.Read<string>() ?? string.Empty,
                OperatorNote = obj["operatorNote"]?.Read<string>() ?? obj["operator_note"]?.Read<string>() ?? string.Empty,
                SourceSurface = obj["sourceSurface"]?.Read<string>() ?? obj["source_surface"]?.Read<string>() ?? string.Empty,
                Risk = obj["risk"]?.Read<string>() ?? string.Empty,
                ActionIds = (obj["actionIds"] as JArray)?.Values<string>().Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                    ?? (obj["action_ids"] as JArray)?.Values<string>().Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                    ?? new List<string>(),
                MissionOfferIds = (obj["missionOfferIds"] as JArray)?.Values<string>().Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                    ?? (obj["mission_offer_ids"] as JArray)?.Values<string>().Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                    ?? new List<string>(),
            };
        }

        private static BlackMarketPayoffRecoverySurfaceSnapshot MapBlackMarketPayoffRecovery(JObject obj)
        {
            if (obj == null) return null;
            return new BlackMarketPayoffRecoverySurfaceSnapshot
            {
                Lane = obj["lane"]?.Read<string>() ?? "black_market",
                Phase = obj["phase"]?.Read<string>() ?? string.Empty,
                Severity = obj["severity"]?.Read<string>() ?? string.Empty,
                Headline = obj["headline"]?.Read<string>() ?? string.Empty,
                Detail = obj["detail"]?.Read<string>() ?? string.Empty,
                StateReason = obj["stateReason"]?.Read<string>() ?? obj["state_reason"]?.Read<string>() ?? string.Empty,
                RecommendedAction = obj["recommendedAction"]?.Read<string>() ?? obj["recommended_action"]?.Read<string>() ?? string.Empty,
                RecentReceipts = (obj["recentReceipts"] as JArray)?.OfType<JObject>().Select(MapBlackMarketPayoffRecoveryReceipt).Where(r => r != null).ToList()
                    ?? (obj["recent_receipts"] as JArray)?.OfType<JObject>().Select(MapBlackMarketPayoffRecoveryReceipt).Where(r => r != null).ToList()
                    ?? new List<BlackMarketPayoffRecoveryReceiptSnapshot>(),
            };
        }

        private static BlackMarketPayoffRecoveryReceiptSnapshot MapBlackMarketPayoffRecoveryReceipt(JObject obj)
        {
            if (obj == null) return null;
            return new BlackMarketPayoffRecoveryReceiptSnapshot
            {
                Id = obj["id"]?.Read<string>() ?? string.Empty,
                CreatedAtUtc = ParseUtc(obj["createdAt"] ?? obj["created_at"]),
                Title = obj["title"]?.Read<string>() ?? string.Empty,
                Summary = obj["summary"]?.Read<string>() ?? string.Empty,
                Detail = obj["detail"]?.Read<string>() ?? string.Empty,
                Severity = obj["severity"]?.Read<string>() ?? string.Empty,
                RuntimeActionId = obj["runtimeActionId"]?.Read<string>() ?? obj["runtime_action_id"]?.Read<string>() ?? string.Empty,
            };
        }

        private static string FirstNonBlank(params string[] values)
        {
            if (values == null) return string.Empty;
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        private static JObject FirstObject(params JToken[] tokens) => tokens?.OfType<JObject>().FirstOrDefault();
        private static JArray FirstArray(params JToken[] tokens) => tokens?.OfType<JArray>().FirstOrDefault();

        private static ResearchSnapshot MapResearch(JObject obj)
        {
            if (obj == null) return null;
            return new ResearchSnapshot
            {
                Id = obj["techId"]?.Read<string>() ?? obj["tech_id"]?.Read<string>(),
                Name = obj["name"]?.Read<string>() ?? obj["techId"]?.Read<string>() ?? obj["tech_id"]?.Read<string>() ?? "Research",
                Progress = obj["progress"]?.Read<double?>(),
                Cost = obj["cost"]?.Read<double?>(),
                StartedAtUtc = ParseUtc(obj["startedAt"] ?? obj["started_at"])
            };
        }

        private static DateTime? ParseUtc(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null || token.Type == JTokenType.Undefined)
            {
                return null;
            }

            if (token.Type == JTokenType.Date && token is JValue dateValue)
            {
                if (dateValue.Value is DateTimeOffset dto)
                {
                    return dto.UtcDateTime;
                }

                if (dateValue.Value is DateTime dt)
                {
                    return dt.Kind == DateTimeKind.Utc ? dt : dt.ToUniversalTime();
                }
            }

            var text = token.Type == JTokenType.String ? token.Value<string>() : token.ToString();
            if (string.IsNullOrWhiteSpace(text)) return null;
            if (!DateTimeOffset.TryParse(text, null, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out var parsed)) return null;
            return parsed.UtcDateTime;
        }
    }
}
