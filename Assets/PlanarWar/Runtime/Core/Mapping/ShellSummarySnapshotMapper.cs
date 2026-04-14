using Newtonsoft.Json.Linq;
using PlanarWar.Client.Core.Contracts;
using System;
using System.Linq;

namespace PlanarWar.Client.Core.Mapping
{
    public static class ShellSummarySnapshotMapper
    {
        public static ShellSummarySnapshot Map(JObject summary)
        {
            if (summary == null) return ShellSummarySnapshot.Empty;

            var city = summary["city"] as JObject;
            return new ShellSummarySnapshot
            {
                Username = summary["username"]?.Value<string>() ?? "Anon",
                FounderMode = summary["founderMode"]?.Value<bool>() ?? false,
                HasCity = summary["hasCity"]?.Value<bool>() ?? false,
                City = new CitySummarySnapshot
                {
                    Name = city?["name"]?.Value<string>() ?? "-",
                    SettlementLane = city?["settlementLane"]?.Value<string>() ?? "-",
                    SettlementLaneLabel = city?["settlementLaneProfile"]?["label"]?.Value<string>() ?? city?["settlementLane"]?.Value<string>() ?? "-",
                    Tier = city?["tier"]?.Value<int?>()
                },
                Resources = MapResource(summary["resources"] as JObject),
                ProductionPerTick = MapResource(city?["production"] as JObject, true),
                ResourceTickTiming = MapTimer(summary["resourceTickTiming"] as JObject),
                ActiveResearch = MapResearch(summary["activeResearch"] as JObject),
                ThreatWarnings = (summary["threatWarnings"] as JArray)?.OfType<JObject>().Select(w => new ThreatWarningSnapshot { Headline = w["headline"]?.Value<string>() ?? w["title"]?.Value<string>() ?? w["summary"]?.Value<string>() ?? "Warning" }).ToList() ?? new(),
                OpeningOperations = (city?["settlementOpeningOperations"] as JArray)?.OfType<JObject>().Select(op => new OperationSnapshot { Title = op["title"]?.Value<string>() ?? "Operation", Readiness = op["readiness"]?.Value<string>() ?? "-" }).ToList() ?? new(),
                ActiveMissions = (summary["activeMissions"] as JArray)?.OfType<JObject>().Select(m => new MissionSnapshot
                {
                    Id = m["mission"]?["id"]?.Value<string>() ?? "mission",
                    Title = m["mission"]?["title"]?.Value<string>() ?? m["mission"]?["id"]?.Value<string>() ?? "Mission",
                    FinishesAtUtc = ParseUtc(m["finishesAt"])
                }).ToList() ?? new(),
                Heroes = (summary["heroes"] as JArray)?.OfType<JObject>().Select(h => new HeroSnapshot
                {
                    Name = h["name"]?.Value<string>() ?? "Hero",
                    Status = h["status"]?.Value<string>() ?? "-",
                    Level = h["level"]?.Value<double?>(),
                    AttachmentCount = (h["attachments"] as JArray)?.Count ?? 0
                }).ToList() ?? new(),
                Armies = (summary["armies"] as JArray)?.OfType<JObject>().Select(a => new ArmySnapshot
                {
                    Name = a["name"]?.Value<string>() ?? "Army",
                    Status = a["status"]?.Value<string>() ?? "-",
                    Readiness = a["readiness"]?.Value<double?>()
                }).ToList() ?? new(),
                WorkshopJobs = (summary["workshopJobs"] as JArray)?.OfType<JObject>().Select(j => new WorkshopJobSnapshot
                {
                    AttachmentKind = j["attachmentKind"]?.Value<string>() ?? "job",
                    Completed = j["completed"]?.Value<bool>() ?? false,
                    FinishesAtUtc = ParseUtc(j["finishesAt"])
                }).ToList() ?? new(),
                WarfrontSignals = (summary["warfront"] as JObject)?.Properties().Select(p => new WarfrontSignalSnapshot { Label = p.Name, Value = p.Value?.ToString() ?? "-" }).ToList() ?? new()
            };
        }

        private static ResourceSnapshot MapResource(JObject obj, bool perTick = false)
        {
            if (obj == null) return new ResourceSnapshot();
            return new ResourceSnapshot
            {
                Food = obj[perTick ? "foodPerTick" : "food"]?.Value<double?>(),
                Materials = obj[perTick ? "materialsPerTick" : "materials"]?.Value<double?>(),
                Wealth = obj[perTick ? "wealthPerTick" : "wealth"]?.Value<double?>(),
                Mana = obj[perTick ? "manaPerTick" : "mana"]?.Value<double?>(),
                Knowledge = obj[perTick ? "knowledgePerTick" : "knowledge"]?.Value<double?>(),
                Unity = obj[perTick ? "unityPerTick" : "unity"]?.Value<double?>(),
            };
        }

        private static TimerSnapshot MapTimer(JObject obj)
        {
            if (obj == null) return new TimerSnapshot();
            return new TimerSnapshot
            {
                TickMs = obj["tickMs"]?.Value<double?>(),
                LastTickAtUtc = ParseUtc(obj["lastTickAt"]),
                NextTickAtUtc = ParseUtc(obj["nextTickAt"])
            };
        }

        private static ResearchSnapshot MapResearch(JObject obj)
        {
            if (obj == null) return null;
            return new ResearchSnapshot
            {
                Id = obj["techId"]?.Value<string>(),
                Name = obj["name"]?.Value<string>() ?? obj["techId"]?.Value<string>() ?? "Research",
                Progress = obj["progress"]?.Value<double?>(),
                Cost = obj["cost"]?.Value<double?>(),
                StartedAtUtc = ParseUtc(obj["startedAt"])
            };
        }

        private static DateTime? ParseUtc(JToken token)
        {
            var s = token?.Value<string>();
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (!DateTimeOffset.TryParse(s, out var dto)) return null;
            return dto.UtcDateTime;
        }
    }
}
