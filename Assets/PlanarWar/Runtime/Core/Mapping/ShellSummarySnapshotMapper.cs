using Newtonsoft.Json.Linq;
using PlanarWar.Client.Core.Contracts;
using PlanarWar.Client.Network;
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
                ResourceTickTiming = MapTimer(summary["resourceTickTiming"] as JObject),
                ActiveResearch = MapResearch(summary["activeResearch"] as JObject),
                ThreatWarnings = (summary["threatWarnings"] as JArray)?.OfType<JObject>().Select(w => new ThreatWarningSnapshot { Headline = w["headline"]?.Read<string>() ?? w["title"]?.Read<string>() ?? w["summary"]?.Read<string>() ?? "Warning" }).ToList() ?? new(),
                OpeningOperations = (city?["settlementOpeningOperations"] as JArray)?.OfType<JObject>().Select(op => new OperationSnapshot { Title = op["title"]?.Read<string>() ?? "Operation", Readiness = op["readiness"]?.Read<string>() ?? "-" }).ToList() ?? new(),
                ActiveMissions = (summary["activeMissions"] as JArray)?.OfType<JObject>().Select(m => new MissionSnapshot
                {
                    Id = m["mission"]?["id"]?.Read<string>() ?? "mission",
                    Title = m["mission"]?["title"]?.Read<string>() ?? m["mission"]?["id"]?.Read<string>() ?? "Mission",
                    FinishesAtUtc = ParseUtc(m["finishesAt"])
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
                    AttachmentKind = j["attachmentKind"]?.Read<string>() ?? "job",
                    Completed = j["completed"]?.Read<bool>() ?? false,
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
                TickMs = obj["tickMs"]?.Read<double?>() ?? obj["tick_ms"]?.Read<double?>(),
                LastTickAtUtc = ParseUtc(obj["lastTickAt"] ?? obj["last_tick_at"]),
                NextTickAtUtc = ParseUtc(obj["nextTickAt"] ?? obj["next_tick_at"])
            };
        }

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
            var s = token?.Read<string>();
            if (string.IsNullOrWhiteSpace(s)) return null;
            if (!DateTimeOffset.TryParse(s, out var dto)) return null;
            return dto.UtcDateTime;
        }
    }
}
