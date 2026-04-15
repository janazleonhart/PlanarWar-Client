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
                OpeningOperations = (city?["settlementOpeningOperations"] as JArray)?.OfType<JObject>().Select(MapOpeningOperation).Where(op => op != null).ToList() ?? new(),
                ActiveMissions = activeMissions?.OfType<JObject>().Select(m => new MissionSnapshot
                {
                    Id = m["mission"]?["id"]?.Read<string>() ?? m["instanceId"]?.Read<string>() ?? m["instance_id"]?.Read<string>() ?? "mission",
                    InstanceId = m["instanceId"]?.Read<string>() ?? m["instance_id"]?.Read<string>() ?? string.Empty,
                    Title = m["mission"]?["title"]?.Read<string>() ?? m["mission"]?["id"]?.Read<string>() ?? "Mission",
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
                    Name = h["name"]?.Read<string>() ?? "Hero",
                    Status = h["status"]?.Read<string>() ?? "-",
                    Level = h["level"]?.Read<double?>(),
                    AttachmentCount = (h["attachments"] as JArray)?.Count ?? 0
                }).ToList() ?? new(),
                Armies = (summary["armies"] as JArray)?.OfType<JObject>().Select(a => new ArmySnapshot
                {
                    Name = a["name"]?.Read<string>() ?? "Army",
                    Status = a["status"]?.Read<string>() ?? "-",
                    Readiness = a["readiness"]?.Read<double?>()
                }).ToList() ?? new(),
                WorkshopJobs = (summary["workshopJobs"] as JArray)?.OfType<JObject>().Select(j => new WorkshopJobSnapshot
                {
                    Id = j["id"]?.Read<string>() ?? j["jobId"]?.Read<string>() ?? "job",
                    RecipeId = j["recipeId"]?.Read<string>() ?? string.Empty,
                    AttachmentKind = j["attachmentKind"]?.Read<string>() ?? string.Empty,
                    OutputItemId = j["outputItemId"]?.Read<string>() ?? string.Empty,
                    OutputName = j["outputName"]?.Read<string>() ?? string.Empty,
                    Completed = j["completed"]?.Read<bool>() ?? false,
                    FinishesAtUtc = ParseUtc(j["finishesAt"]),
                    CollectedAtUtc = ParseUtc(j["collectedAt"])
                }).ToList() ?? new(),
                WarfrontSignals = (summary["warfrontStatus"] as JObject)?.Properties().Select(p => new WarfrontSignalSnapshot { Label = p.Name, Value = p.Value?.ToString() ?? "-" }).ToList()
                    ?? (summary["warfront"] as JObject)?.Properties().Select(p => new WarfrontSignalSnapshot { Label = p.Name, Value = p.Value?.ToString() ?? "-" }).ToList()
                    ?? new()
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


        private static OperationSnapshot MapOpeningOperation(JObject obj)
        {
            if (obj == null) return null;

            var action = obj["action"] as JObject;
            return new OperationSnapshot
            {
                Id = obj["id"]?.Read<string>() ?? "operation",
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
                Action = new OpeningActionSnapshot
                {
                    Kind = action?["kind"]?.Read<string>() ?? string.Empty,
                    BuildingKind = action?["buildingKind"]?.Read<string>() ?? action?["building_kind"]?.Read<string>() ?? string.Empty,
                    BuildingId = action?["buildingId"]?.Read<string>() ?? action?["building_id"]?.Read<string>() ?? string.Empty,
                    TechId = action?["techId"]?.Read<string>() ?? action?["tech_id"]?.Read<string>() ?? string.Empty,
                    MissionId = action?["missionId"]?.Read<string>() ?? action?["mission_id"]?.Read<string>() ?? string.Empty,
                    ActionId = action?["actionId"]?.Read<string>() ?? action?["action_id"]?.Read<string>() ?? string.Empty,
                    Role = action?["role"]?.Read<string>() ?? string.Empty,
                    ArmyId = action?["armyId"]?.Read<string>() ?? action?["army_id"]?.Read<string>() ?? string.Empty,
                    HeroId = action?["heroId"]?.Read<string>() ?? action?["hero_id"]?.Read<string>() ?? string.Empty,
                    ResponsePosture = action?["responsePosture"]?.Read<string>() ?? action?["response_posture"]?.Read<string>() ?? string.Empty,
                }
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
