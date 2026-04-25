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
        public static ShellSummarySnapshot Map(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return ShellSummarySnapshot.Empty;
            }

            return Map(JObject.Parse(json));
        }

        private static ShellSummarySnapshot Map(JObject summary)
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
            var researchedTechIds = FirstArray(
                summary["researchedTechIds"],
                summary["researched_tech_ids"],
                summary["completedTechIds"],
                summary["completed_tech_ids"],
                city?["researchedTechIds"],
                city?["researched_tech_ids"],
                city?["completedTechIds"],
                city?["completed_tech_ids"]);
            var cityTimers = FirstArray(
                summary["cityTimers"],
                summary["city_timers"],
                summary["timers"],
                city?["cityTimers"],
                city?["city_timers"],
                city?["timers"]);
            var mappedCityTimers = cityTimers?.OfType<JObject>().Select(MapCityTimer).Where(t => t != null).ToList() ?? new List<CityTimerEntrySnapshot>();
            var activeResearchObjects = ObjectsFromArrays(
                summary["activeResearch"],
                summary["active_research"],
                summary["activeResearches"],
                summary["active_researches"],
                summary["research"]?["active"],
                summary["research"]?["activeResearch"],
                summary["research"]?["active_research"],
                summary["research"]?["activeResearches"],
                summary["research"]?["active_researches"],
                city?["activeResearch"],
                city?["active_research"],
                city?["activeResearches"],
                city?["active_researches"],
                city?["research"]?["active"],
                city?["research"]?["activeResearch"],
                city?["research"]?["active_research"],
                city?["research"]?["activeResearches"],
                city?["research"]?["active_researches"]);
            var activeResearches = BuildActiveResearchList(activeResearchObjects, mappedCityTimers);
            var openingOps = FirstArray(
                summary["settlementOpeningOperations"],
                summary["settlement_opening_operations"],
                summary["openingOperations"],
                summary["opening_operations"],
                city?["settlementOpeningOperations"],
                city?["settlement_opening_operations"],
                city?["openingOperations"],
                city?["opening_operations"]);
            var buildingObjects = ObjectsFromArrays(
                summary["buildings"],
                summary["cityBuildings"],
                summary["city_buildings"],
                summary["buildingCards"],
                summary["building_cards"],
                summary["construction"],
                summary["constructions"],
                summary["blackMarketFronts"],
                summary["black_market_fronts"],
                summary["operatorFronts"],
                summary["operator_fronts"],
                city?["buildings"],
                city?["cityBuildings"],
                city?["city_buildings"],
                city?["buildingCards"],
                city?["building_cards"],
                city?["construction"],
                city?["constructions"],
                city?["blackMarketFronts"],
                city?["black_market_fronts"],
                city?["operatorFronts"],
                city?["operator_fronts"]);

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
                Buildings = buildingObjects.Select(MapBuilding).Where(b => b != null).ToList(),
                Resources = MapResource(summary["resources"] as JObject),
                ResourceLabels = MapResourcePresentation(FirstObject(summary["resourceLabels"], summary["resource_labels"]), city?["settlementLane"]?.Read<string>() ?? city?["settlement_lane"]?.Read<string>()),
                ProductionPerTick = MapResource(city?["production"] as JObject, true),
                ResourceTickTiming = MapTimer(resourceTickTiming),
                ActiveResearch = activeResearches.FirstOrDefault(),
                ActiveResearches = activeResearches,
                AvailableTechs = availableTechs?.OfType<JObject>().Select(MapTech).Where(t => t != null).ToList() ?? new List<TechOptionSnapshot>(),
                ResearchedTechIds = MapStringArray(researchedTechIds),
                CityTimers = mappedCityTimers,
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
                BlackMarketBackbonePressure = MapBlackMarketBackbonePressure(summary["blackMarketBackbonePressureSurface"] as JObject ?? summary["black_market_backbone_pressure_surface"] as JObject),
                BlackMarketPayoffRecovery = MapBlackMarketPayoffRecovery(summary["blackMarketPayoffRecoverySurface"] as JObject ?? summary["black_market_payoff_recovery_surface"] as JObject),
            };

            ResolveActiveMissionAssignments(mapped);
            return mapped;
        }


        private static List<ResearchSnapshot> BuildActiveResearchList(List<JObject> activeResearchObjects, IEnumerable<CityTimerEntrySnapshot> cityTimers)
        {
            var research = activeResearchObjects?
                .Select(MapResearch)
                .Where(r => r != null)
                .ToList() ?? new List<ResearchSnapshot>();

            foreach (var timer in cityTimers ?? Enumerable.Empty<CityTimerEntrySnapshot>())
            {
                if (!IsResearchTimer(timer))
                {
                    continue;
                }

                var fromTimer = new ResearchSnapshot
                {
                    Id = NormalizeResearchTimerId(FirstNonBlank(timer.Id, timer.Label, timer.Category)),
                    Name = NormalizeResearchTimerLabel(FirstNonBlank(timer.Label, timer.Id, timer.Category, "Research")),
                    Status = FirstNonBlank(timer.Status, "active"),
                    StartedAtUtc = timer.StartedAtUtc,
                    FinishesAtUtc = timer.FinishesAtUtc,
                };
                ApplyResearchProgressDetail(fromTimer, timer.Detail);
                research.Add(fromTimer);
            }

            return research
                .Where(r => !string.IsNullOrWhiteSpace(FirstNonBlank(r.Id, r.Name)))
                .GroupBy(r => NormalizeResearchKey(r.Id, r.Name), StringComparer.OrdinalIgnoreCase)
                .Select(MergeResearchGroup)
                .Where(r => r != null)
                .OrderBy(r => r.FinishesAtUtc.HasValue && r.FinishesAtUtc.Value <= DateTime.UtcNow ? 0 : 1)
                .ThenBy(r => r.FinishesAtUtc ?? DateTime.MaxValue)
                .ThenBy(r => FirstNonBlank(r.Name, r.Id))
                .ToList();
        }

        private static ResearchSnapshot MergeResearchGroup(IEnumerable<ResearchSnapshot> group)
        {
            var items = group?.Where(r => r != null).ToList() ?? new List<ResearchSnapshot>();
            if (items.Count == 0) return null;

            var preferredIdentity = items.FirstOrDefault(r => !IsResearchTimerId(r.Id)) ?? items[0];
            var preferredName = items.FirstOrDefault(r => !StartsWithResearchPrefix(r.Name)) ?? preferredIdentity;
            var finish = items
                .Where(r => r.FinishesAtUtc.HasValue)
                .OrderBy(r => r.FinishesAtUtc.Value)
                .Select(r => r.FinishesAtUtc)
                .FirstOrDefault();
            var started = items
                .Where(r => r.StartedAtUtc.HasValue)
                .OrderBy(r => r.StartedAtUtc.Value)
                .Select(r => r.StartedAtUtc)
                .FirstOrDefault();

            return new ResearchSnapshot
            {
                Id = NormalizeResearchTimerId(FirstNonBlank(new[] { preferredIdentity.Id }.Concat(items.Select(r => r.Id)).ToArray())),
                Name = NormalizeResearchTimerLabel(FirstNonBlank(new[] { preferredName.Name, preferredIdentity.Name }.Concat(items.Select(r => r.Name)).ToArray())),
                Status = FirstNonBlank(items.Select(r => r.Status).ToArray()),
                Progress = items.Select(r => r.Progress).FirstOrDefault(v => v.HasValue),
                Cost = items.Select(r => r.Cost).FirstOrDefault(v => v.HasValue),
                StartedAtUtc = started,
                FinishesAtUtc = finish,
            };
        }

        private static bool IsResearchTimer(CityTimerEntrySnapshot timer)
        {
            var category = timer?.Category ?? string.Empty;
            var label = timer?.Label ?? string.Empty;
            var detail = timer?.Detail ?? string.Empty;

            return ContainsResearchWord(category)
                || ContainsResearchWord(label)
                || ContainsResearchWord(detail);
        }

        private static bool ContainsResearchWord(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return false;
            return raw.IndexOf("research", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("tech", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("unlock", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("shadow_book", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("shadow-book", StringComparison.OrdinalIgnoreCase) >= 0
                || raw.IndexOf("shadow book", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string NormalizeResearchKey(string id, string name)
        {
            var raw = NormalizeResearchTimerId(FirstNonBlank(id, name));
            if (string.IsNullOrWhiteSpace(raw)) raw = NormalizeResearchTimerLabel(name);
            raw = NormalizeResearchTimerLabel(raw);
            var chars = raw
                .ToLowerInvariant()
                .Where(char.IsLetterOrDigit)
                .ToArray();
            return chars.Length > 0 ? new string(chars) : "research";
        }

        private static bool IsResearchTimerId(string id)
        {
            return !string.IsNullOrWhiteSpace(id) && id.Trim().StartsWith("research:", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeResearchTimerId(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var value = raw.Trim();
            return value.StartsWith("research:", StringComparison.OrdinalIgnoreCase) ? value.Substring("research:".Length).Trim() : value;
        }

        private static bool StartsWithResearchPrefix(string raw)
        {
            return !string.IsNullOrWhiteSpace(raw) && raw.Trim().StartsWith("Research ", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeResearchTimerLabel(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var value = NormalizeResearchTimerId(raw);
            return value.StartsWith("Research ", StringComparison.OrdinalIgnoreCase) ? value.Substring("Research ".Length).Trim() : value;
        }

        private static void ApplyResearchProgressDetail(ResearchSnapshot research, string detail)
        {
            if (research == null || string.IsNullOrWhiteSpace(detail)) return;
            const string marker = "progress:";
            var index = detail.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index < 0) return;
            var segment = detail.Substring(index + marker.Length).Split('•')[0].Trim();
            var parts = segment.Split('/');
            if (parts.Length != 2) return;

            if (double.TryParse(parts[0].Trim(), out var progress))
            {
                research.Progress = progress;
            }

            if (double.TryParse(parts[1].Trim(), out var cost))
            {
                research.Cost = cost;
            }
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

        private static BuildingSnapshot MapBuilding(JObject obj)
        {
            if (obj == null) return null;

            var definition = obj["building"] as JObject ?? obj["definition"] as JObject ?? obj["template"] as JObject ?? obj["front"] as JObject;
            var production = FirstObject(
                obj["production"],
                obj["productionPerTick"],
                obj["production_per_tick"],
                obj["effects"]?["production"],
                definition?["production"],
                definition?["productionPerTick"],
                definition?["production_per_tick"]);
            var id = FirstNonBlank(
                obj["id"]?.Read<string>(),
                obj["instanceId"]?.Read<string>(),
                obj["instance_id"]?.Read<string>(),
                obj["buildingId"]?.Read<string>(),
                obj["building_id"]?.Read<string>(),
                definition?["id"]?.Read<string>());
            var buildingId = FirstNonBlank(
                obj["buildingId"]?.Read<string>(),
                obj["building_id"]?.Read<string>(),
                obj["type"]?.Read<string>(),
                obj["kind"]?.Read<string>(),
                definition?["id"]?.Read<string>(),
                definition?["type"]?.Read<string>(),
                id);
            var name = FirstNonBlank(
                obj["name"]?.Read<string>(),
                obj["label"]?.Read<string>(),
                obj["title"]?.Read<string>(),
                definition?["name"]?.Read<string>(),
                definition?["label"]?.Read<string>(),
                buildingId,
                "Building");

            return new BuildingSnapshot
            {
                Id = id,
                BuildingId = buildingId,
                Type = FirstNonBlank(obj["type"]?.Read<string>(), obj["kind"]?.Read<string>(), definition?["type"]?.Read<string>(), definition?["kind"]?.Read<string>(), buildingId),
                Name = name,
                Lane = FirstNonBlank(obj["lane"]?.Read<string>(), obj["settlementLane"]?.Read<string>(), obj["settlement_lane"]?.Read<string>(), definition?["lane"]?.Read<string>()),
                Status = FirstNonBlank(obj["status"]?.Read<string>(), obj["state"]?.Read<string>(), obj["phase"]?.Read<string>(), "active"),
                Level = obj["level"]?.Read<int?>() ?? obj["rank"]?.Read<int?>() ?? obj["tier"]?.Read<int?>(),
                Slot = obj["slot"]?.Read<int?>() ?? obj["slotIndex"]?.Read<int?>() ?? obj["slot_index"]?.Read<int?>(),
                StartedAtUtc = ParseUtc(obj["startedAt"] ?? obj["started_at"] ?? obj["constructionStartedAt"] ?? obj["construction_started_at"] ?? obj["upgradingStartedAt"] ?? obj["upgrading_started_at"]),
                FinishesAtUtc = ParseUtc(obj["finishesAt"] ?? obj["finishes_at"] ?? obj["readyAt"] ?? obj["ready_at"] ?? obj["completedAt"] ?? obj["completed_at"] ?? obj["constructionFinishesAt"] ?? obj["construction_finishes_at"] ?? obj["upgradeFinishesAt"] ?? obj["upgrade_finishes_at"]),
                Detail = FirstNonBlank(obj["detail"]?.Read<string>(), obj["summary"]?.Read<string>(), obj["description"]?.Read<string>(), definition?["summary"]?.Read<string>(), definition?["description"]?.Read<string>()),
                EffectSummary = FirstNonBlank(obj["effectSummary"]?.Read<string>(), obj["effect_summary"]?.Read<string>(), obj["effect"]?.Read<string>(), definition?["effectSummary"]?.Read<string>(), definition?["effect_summary"]?.Read<string>(), definition?["effect"]?.Read<string>()),
                ProductionPerTick = MapResource(production, true),
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
                Food = ReadResourceAmount(obj, "food", perTick),
                Materials = ReadResourceAmount(obj, "materials", perTick),
                Wealth = ReadResourceAmount(obj, "wealth", perTick),
                Mana = ReadResourceAmount(obj, "mana", perTick),
                Knowledge = ReadResourceAmount(obj, "knowledge", perTick),
                Unity = ReadResourceAmount(obj, "unity", perTick),
            };
        }

        private static double? ReadResourceAmount(JObject obj, string key, bool perTick)
        {
            if (obj == null || string.IsNullOrWhiteSpace(key)) return null;

            var camel = perTick ? key + "PerTick" : key;
            var snake = perTick ? key + "_per_tick" : key;
            return obj[camel]?.Read<double?>()
                ?? obj[snake]?.Read<double?>()
                ?? obj[key]?.Read<double?>();
        }

        private static ResourcePresentationSnapshot MapResourcePresentation(JObject obj, string lane)
        {
            var isBlackMarket = string.Equals((lane ?? string.Empty).Trim(), "black_market", StringComparison.OrdinalIgnoreCase)
                || string.Equals((lane ?? string.Empty).Trim(), "black market", StringComparison.OrdinalIgnoreCase)
                || string.Equals((lane ?? string.Empty).Trim(), "black-market", StringComparison.OrdinalIgnoreCase);

            return new ResourcePresentationSnapshot
            {
                Food = obj?["food"]?.Read<string>() ?? obj?["foodLabel"]?.Read<string>() ?? (isBlackMarket ? "Provisions" : "Food"),
                Materials = obj?["materials"]?.Read<string>() ?? obj?["materialsLabel"]?.Read<string>() ?? (isBlackMarket ? "Supplies" : "Materials"),
                Wealth = obj?["wealth"]?.Read<string>() ?? obj?["wealthLabel"]?.Read<string>() ?? (isBlackMarket ? "Cashflow" : "Wealth"),
                Mana = obj?["mana"]?.Read<string>() ?? obj?["manaLabel"]?.Read<string>() ?? (isBlackMarket ? "Arcana" : "Mana"),
                Knowledge = obj?["knowledge"]?.Read<string>() ?? obj?["knowledgeLabel"]?.Read<string>() ?? (isBlackMarket ? "Intel" : "Knowledge"),
                Unity = obj?["unity"]?.Read<string>() ?? obj?["unityLabel"]?.Read<string>() ?? (isBlackMarket ? "Loyalty" : "Unity"),
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
                ContractFollowThrough = MapContractFollowThrough(obj["contractFollowThrough"] as JObject ?? obj["contract_follow_through"] as JObject),
                ContractEffects = MapPublicBackboneContractEffects(obj["contractEffects"] as JObject ?? obj["contract_effects"] as JObject),
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

        private static ContractFollowThroughSnapshot MapContractFollowThrough(JObject obj)
        {
            if (obj == null) return null;
            return new ContractFollowThroughSnapshot
            {
                Lane = obj["lane"]?.Read<string>() ?? string.Empty,
                State = obj["state"]?.Read<string>() ?? string.Empty,
                ContractId = obj["contractId"]?.Read<string>() ?? obj["contract_id"]?.Read<string>() ?? string.Empty,
                ContractTitle = obj["contractTitle"]?.Read<string>() ?? obj["contract_title"]?.Read<string>() ?? string.Empty,
                ContractKind = obj["contractKind"]?.Read<string>() ?? obj["contract_kind"]?.Read<string>() ?? string.Empty,
                ActiveMissionInstanceId = obj["activeMissionInstanceId"]?.Read<string>() ?? obj["active_mission_instance_id"]?.Read<string>() ?? string.Empty,
                ActiveMissionFinishesAtUtc = ParseUtc(obj["activeMissionFinishesAt"] ?? obj["active_mission_finishes_at"]),
                LatestReceiptId = obj["latestReceiptId"]?.Read<string>() ?? obj["latest_receipt_id"]?.Read<string>() ?? string.Empty,
                LatestReceiptAtUtc = ParseUtc(obj["latestReceiptAt"] ?? obj["latest_receipt_at"]),
                SourceMissionId = obj["sourceMissionId"]?.Read<string>() ?? obj["source_mission_id"]?.Read<string>() ?? string.Empty,
                SourceSurface = obj["sourceSurface"]?.Read<string>() ?? obj["source_surface"]?.Read<string>() ?? string.Empty,
                Note = obj["note"]?.Read<string>() ?? string.Empty,
            };
        }

        private static PublicBackboneContractEffectsSnapshot MapPublicBackboneContractEffects(JObject obj)
        {
            if (obj == null) return null;
            return new PublicBackboneContractEffectsSnapshot
            {
                Lane = obj["lane"]?.Read<string>() ?? "city",
                State = obj["state"]?.Read<string>() ?? string.Empty,
                ContractId = obj["contractId"]?.Read<string>() ?? obj["contract_id"]?.Read<string>() ?? string.Empty,
                ContractTitle = obj["contractTitle"]?.Read<string>() ?? obj["contract_title"]?.Read<string>() ?? string.Empty,
                ContractKind = obj["contractKind"]?.Read<string>() ?? obj["contract_kind"]?.Read<string>() ?? string.Empty,
                QueueEffect = obj["queueEffect"]?.Read<string>() ?? obj["queue_effect"]?.Read<string>() ?? string.Empty,
                TrustEffect = obj["trustEffect"]?.Read<string>() ?? obj["trust_effect"]?.Read<string>() ?? string.Empty,
                ServiceEffect = obj["serviceEffect"]?.Read<string>() ?? obj["service_effect"]?.Read<string>() ?? string.Empty,
                Note = obj["note"]?.Read<string>() ?? string.Empty,
                SourceSurface = obj["sourceSurface"]?.Read<string>() ?? obj["source_surface"]?.Read<string>() ?? string.Empty,
            };
        }

        private static ShadowContractEffectsSnapshot MapShadowContractEffects(JObject obj)
        {
            if (obj == null) return null;
            return new ShadowContractEffectsSnapshot
            {
                Lane = obj["lane"]?.Read<string>() ?? "black_market",
                State = obj["state"]?.Read<string>() ?? string.Empty,
                ContractId = obj["contractId"]?.Read<string>() ?? obj["contract_id"]?.Read<string>() ?? string.Empty,
                ContractTitle = obj["contractTitle"]?.Read<string>() ?? obj["contract_title"]?.Read<string>() ?? string.Empty,
                ContractKind = obj["contractKind"]?.Read<string>() ?? obj["contract_kind"]?.Read<string>() ?? string.Empty,
                ReceiptChainState = obj["receiptChainState"]?.Read<string>() ?? obj["receipt_chain_state"]?.Read<string>() ?? string.Empty,
                CovertCarryState = obj["covertCarryState"]?.Read<string>() ?? obj["covert_carry_state"]?.Read<string>() ?? string.Empty,
                LinkedReceiptId = obj["linkedReceiptId"]?.Read<string>() ?? obj["linked_receipt_id"]?.Read<string>() ?? string.Empty,
                LinkedReceiptTitle = obj["linkedReceiptTitle"]?.Read<string>() ?? obj["linked_receipt_title"]?.Read<string>() ?? string.Empty,
                Note = obj["note"]?.Read<string>() ?? string.Empty,
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

        private static BlackMarketBackbonePressureSurfaceSnapshot MapBlackMarketBackbonePressure(JObject obj)
        {
            if (obj == null) return null;
            return new BlackMarketBackbonePressureSurfaceSnapshot
            {
                Lane = obj["lane"]?.Read<string>() ?? "black_market",
                PressureState = obj["pressureState"]?.Read<string>() ?? obj["pressure_state"]?.Read<string>() ?? string.Empty,
                LeverageWindow = obj["leverageWindow"]?.Read<string>() ?? obj["leverage_window"]?.Read<string>() ?? string.Empty,
                Headline = obj["headline"]?.Read<string>() ?? string.Empty,
                Detail = obj["detail"]?.Read<string>() ?? string.Empty,
                RecommendedAction = obj["recommendedAction"]?.Read<string>() ?? obj["recommended_action"]?.Read<string>() ?? string.Empty,
                ActiveActionIds = (obj["activeActionIds"] as JArray)?.Values<string>().Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                    ?? (obj["active_action_ids"] as JArray)?.Values<string>().Where(v => !string.IsNullOrWhiteSpace(v)).ToList()
                    ?? new List<string>(),
                ContractFollowThrough = MapContractFollowThrough(obj["contractFollowThrough"] as JObject ?? obj["contract_follow_through"] as JObject),
                ContractEffects = MapShadowContractEffects(obj["contractEffects"] as JObject ?? obj["contract_effects"] as JObject),
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

        private static List<string> MapStringArray(JArray array)
        {
            return array?.Values<string>()
                       .Where(value => !string.IsNullOrWhiteSpace(value))
                       .Select(value => value.Trim())
                       .Distinct(StringComparer.OrdinalIgnoreCase)
                       .ToList()
                   ?? new List<string>();
        }

        private static List<JObject> ObjectsFromArrays(params JToken[] tokens)
        {
            var objects = new List<JObject>();
            if (tokens == null) return objects;

            foreach (var token in tokens)
            {
                if (token is JArray array)
                {
                    objects.AddRange(array.OfType<JObject>());
                }
                else if (token is JObject obj)
                {
                    objects.Add(obj);
                }
            }

            return objects;
        }

        private static ResearchSnapshot MapResearch(JObject obj)
        {
            if (obj == null) return null;
            return new ResearchSnapshot
            {
                Id = obj["techId"]?.Read<string>() ?? obj["tech_id"]?.Read<string>() ?? obj["id"]?.Read<string>(),
                Name = obj["name"]?.Read<string>() ?? obj["title"]?.Read<string>() ?? obj["techId"]?.Read<string>() ?? obj["tech_id"]?.Read<string>() ?? obj["id"]?.Read<string>() ?? "Research",
                Status = FirstNonBlank(obj["status"]?.Read<string>(), obj["state"]?.Read<string>(), obj["phase"]?.Read<string>()),
                Progress = obj["progress"]?.Read<double?>(),
                Cost = obj["cost"]?.Read<double?>() ?? obj["required"]?.Read<double?>() ?? obj["requiredProgress"]?.Read<double?>() ?? obj["required_progress"]?.Read<double?>(),
                StartedAtUtc = ParseUtc(obj["startedAt"] ?? obj["started_at"] ?? obj["researchStartedAt"] ?? obj["research_started_at"]),
                FinishesAtUtc = ParseUtc(obj["finishesAt"] ?? obj["finishes_at"] ?? obj["readyAt"] ?? obj["ready_at"] ?? obj["completedAt"] ?? obj["completed_at"] ?? obj["researchFinishesAt"] ?? obj["research_finishes_at"]),
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
