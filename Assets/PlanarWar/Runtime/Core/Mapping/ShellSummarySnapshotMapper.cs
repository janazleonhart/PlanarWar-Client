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
            var openingOps = FirstArray(city?["settlementOpeningOperations"], city?["settlement_opening_operations"]);

            return new ShellSummarySnapshot
            {
                Username = summary["username"]?.Read<string>() ?? "Anon",
                FounderMode = summary["founderMode"]?.Read<bool>() ?? false,
                HasCity = summary["hasCity"]?.Read<bool>() ?? false,
                City = new CitySummarySnapshot
                {
                    Name = city?["name"]?.Read<string>() ?? "-",
                    SettlementLane = city?["settlementLane"]?.Read<string>() ?? "-",
                    SettlementLaneLabel = city?["settlementLaneProfile"]?["label"]?.Read<string>() ?? city?["settlementLane"]?.Read<string>() ?? "-",
                    Tier = city?["tier"]?.Read<int?>()
                },
                Resources = MapResource(summary["resources"] as JObject),
                ProductionPerTick = MapResource(city?["production"] as JObject, true),
                ResourceTickTiming = MapTimer(resourceTickTiming),
                ActiveResearch = MapResearch(summary["activeResearch"] as JObject),
                AvailableTechs = availableTechs?.OfType<JObject>().Select(MapTech).Where(t => t != null).ToList() ?? new List<TechOptionSnapshot>(),
                CityTimers = cityTimers?.OfType<JObject>().Select(MapCityTimer).Where(t => t != null).ToList() ?? new List<CityTimerEntrySnapshot>(),
                ThreatWarnings = (summary["threatWarnings"] as JArray)?.OfType<JObject>().Select(w => new ThreatWarningSnapshot { Headline = w["headline"]?.Read<string>() ?? w["title"]?.Read<string>() ?? w["summary"]?.Read<string>() ?? "Warning" }).ToList() ?? new(),
                OpeningOperations = openingOps?.OfType<JObject>().Select(MapOperation).Where(o => o != null).ToList() ?? new(),
                ActiveMissions = activeMissions?.OfType<JObject>().Select(m => new MissionSnapshot
                {
                    Id = m["mission"]?["id"]?.Read<string>() ?? "mission",
                    Title = m["mission"]?["title"]?.Read<string>() ?? m["mission"]?["id"]?.Read<string>() ?? "Mission",
                    InstanceId = m["instanceId"]?.Read<string>() ?? m["instance_id"]?.Read<string>() ?? string.Empty,
                    RegionId = m["mission"]?["regionId"]?.Read<string>() ?? m["mission"]?["region_id"]?.Read<string>() ?? string.Empty,
                    AssignedArmyId = m["assignedArmyId"]?.Read<string>() ?? m["assigned_army_id"]?.Read<string>() ?? string.Empty,
                    AssignedHeroId = m["assignedHeroId"]?.Read<string>() ?? m["assigned_hero_id"]?.Read<string>() ?? string.Empty,
                    FinishesAtUtc = ParseUtc(
                        m["finishesAt"]
                        ?? m["finishes_at"]
                        ?? m["finishAt"]
                        ?? m["finish_at"]
                        ?? m["endsAt"]
                        ?? m["ends_at"]
                        ?? m["deadlineAt"]
                        ?? m["deadline_at"]
                        ?? m["timing"]?["finishesAt"]
                        ?? m["timing"]?["finishes_at"]
                        ?? m["timing"]?["finishAt"]
                        ?? m["timing"]?["finish_at"]
                        ?? m["timing"]?["endsAt"]
                        ?? m["timing"]?["ends_at"])
                }).ToList() ?? new(),
                Heroes = (summary["heroes"] as JArray)?.OfType<JObject>().Select(h => new HeroSnapshot
                {
                    Id = h["id"]?.Read<string>() ?? string.Empty,
                    Name = h["name"]?.Read<string>() ?? "Hero",
                    Role = h["role"]?.Read<string>() ?? string.Empty,
                    Status = h["status"]?.Read<string>() ?? "-",
                    Level = h["level"]?.Read<double?>(),
                    AttachmentCount = (h["attachments"] as JArray)?.Count ?? 0,
                    ResponseRoles = (h["responseRoles"] as JArray)?.Select(role => role?.Read<string>()).Where(role => !string.IsNullOrWhiteSpace(role)).ToList() ?? new List<string>()
                }).ToList() ?? new(),
                Armies = (summary["armies"] as JArray)?.OfType<JObject>().Select(a => new ArmySnapshot
                {
                    Id = a["id"]?.Read<string>() ?? string.Empty,
                    Name = a["name"]?.Read<string>() ?? "Army",
                    Type = a["type"]?.Read<string>() ?? string.Empty,
                    Status = a["status"]?.Read<string>() ?? "-",
                    Readiness = a["readiness"]?.Read<double?>(),
                    Power = a["power"]?.Read<double?>(),
                    Size = a["size"]?.Read<double?>(),
                    HoldRegionId = a["hold"]?["regionId"]?.Read<string>() ?? a["hold"]?["region_id"]?.Read<string>() ?? string.Empty,
                    HoldPosture = a["hold"]?["posture"]?.Read<string>() ?? string.Empty,
                    Specialties = (a["specialties"] as JArray)?.Select(specialty => specialty?.Read<string>()).Where(specialty => !string.IsNullOrWhiteSpace(specialty)).ToList() ?? new List<string>()
                }).ToList() ?? new(),
                HeroRecruitment = MapHeroRecruitment(summary["heroRecruitment"] as JObject ?? summary["hero_recruitment"] as JObject),
                ArmyReinforcement = MapArmyReinforcement(summary["armyReinforcement"] as JObject ?? summary["army_reinforcement"] as JObject),
                WorkshopJobs = (summary["workshopJobs"] as JArray)?.OfType<JObject>().Select(j => new WorkshopJobSnapshot
                {
                    Id = j["id"]?.Read<string>() ?? j["jobId"]?.Read<string>() ?? "job",
                    AttachmentKind = j["attachmentKind"]?.Read<string>() ?? "job",
                    RecipeId = j["recipeId"]?.Read<string>() ?? string.Empty,
                    OutputName = j["outputName"]?.Read<string>() ?? string.Empty,
                    OutputItemId = j["outputItemId"]?.Read<string>() ?? string.Empty,
                    Completed = j["completed"]?.Read<bool>() ?? false,
                    CollectedAtUtc = ParseUtc(j["collectedAt"] ?? j["collected_at"]),
                    FinishesAtUtc = ParseUtc(j["finishesAt"] ?? j["finishes_at"])
                }).ToList() ?? new(),
                WarfrontSignals = (summary["warfrontStatus"] as JObject)?.Properties().Select(p => new WarfrontSignalSnapshot { Label = p.Name, Value = p.Value?.ToString() ?? "-" }).ToList()
                    ?? (summary["warfront"] as JObject)?.Properties().Select(p => new WarfrontSignalSnapshot { Label = p.Name, Value = p.Value?.ToString() ?? "-" }).ToList()
                    ?? new()
            };
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
                Title = obj["title"]?.Read<string>() ?? "Operation",
                Readiness = obj["readiness"]?.Read<string>() ?? "-",
                Kind = action?["kind"]?.Read<string>() ?? string.Empty,
                Role = action?["role"]?.Read<string>() ?? string.Empty,
                ArmyId = action?["armyId"]?.Read<string>() ?? string.Empty,
                HeroId = action?["heroId"]?.Read<string>() ?? string.Empty,
                MissionId = action?["missionId"]?.Read<string>() ?? string.Empty,
                ActionId = action?["actionId"]?.Read<string>() ?? string.Empty,
                ResponsePosture = action?["responsePosture"]?.Read<string>() ?? string.Empty,
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
                Id = obj["techId"]?.Read<string>(),
                Name = obj["name"]?.Read<string>() ?? obj["techId"]?.Read<string>() ?? "Research",
                Progress = obj["progress"]?.Read<double?>(),
                Cost = obj["cost"]?.Read<double?>(),
                StartedAtUtc = ParseUtc(obj["startedAt"])
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
