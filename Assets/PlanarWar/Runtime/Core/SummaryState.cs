using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;

namespace PlanarWar.Client.Core
{
    [Serializable]
    public sealed class SummaryState
    {
        public event Action Changed;

        public JObject RawSummary { get; private set; }
        public bool IsLoaded { get; private set; }
        public string LastError { get; private set; } = "-";
        public DateTime LastUpdatedUtc { get; private set; }

        public string Username => RawSummary?["username"]?.Value<string>() ?? "Anon";
        public bool HasCity => RawSummary?["hasCity"]?.Value<bool>() ?? false;
        public bool FounderMode => RawSummary?["founderMode"]?.Value<bool>() ?? false;
        public string CityName => RawSummary?["city"]?["name"]?.Value<string>() ?? (FounderMode ? "No settlement founded" : "No city data");
        public string LaneLabel => RawSummary?["city"]?["settlementLaneProfile"]?["label"]?.Value<string>()
            ?? RawSummary?["city"]?["settlementLane"]?.Value<string>()
            ?? "-";
        public string TierLabel => RawSummary?["city"]?["tier"]?.Type != null
            ? $"Tier {RawSummary["city"]?["tier"]?.Value<int>() ?? 0}"
            : "-";

        public void Apply(JObject summary)
        {
            RawSummary = summary;
            IsLoaded = summary != null;
            LastError = "-";
            LastUpdatedUtc = DateTime.UtcNow;
            NotifyChanged();
        }

        public void SetError(string error)
        {
            LastError = string.IsNullOrWhiteSpace(error) ? "Unknown summary error." : error.Trim();
            NotifyChanged();
        }

        public string GetResourcesSummary()
        {
            var resources = RawSummary?["resources"] as JObject;
            if (resources == null)
            {
                return "No resources loaded yet.";
            }

            var keys = new[] { "food", "materials", "wealth", "mana", "knowledge", "unity" };
            var chunks = keys
                .Select(key =>
                {
                    var value = resources[key]?.Value<double?>();
                    return value.HasValue ? $"{Cap(key)} {FormatNumber(value.Value)}" : null;
                })
                .Where(chunk => !string.IsNullOrWhiteSpace(chunk))
                .ToArray();

            return chunks.Length > 0 ? string.Join("  •  ", chunks) : resources.ToString();
        }

        public string GetProductionSummary()
        {
            var production = RawSummary?["city"]?["production"] as JObject;
            if (production == null)
            {
                return HasCity ? "No production snapshot." : "Found a city to unlock production.";
            }

            var keys = new[] { "foodPerTick", "materialsPerTick", "wealthPerTick", "manaPerTick", "knowledgePerTick", "unityPerTick" };
            var chunks = keys
                .Select(key =>
                {
                    var value = production[key]?.Value<double?>();
                    return value.HasValue ? $"{Cap(key.Replace("PerTick", ""))} +{FormatNumber(value.Value)}/tick" : null;
                })
                .Where(chunk => !string.IsNullOrWhiteSpace(chunk))
                .ToArray();

            return chunks.Length > 0 ? string.Join("  •  ", chunks) : "Production data unavailable.";
        }

        public string GetResearchSummary()
        {
            var activeResearch = RawSummary?["activeResearch"] as JObject;
            if (activeResearch == null)
            {
                return "No active research.";
            }

            var name = activeResearch["name"]?.Value<string>() ?? activeResearch["techId"]?.Value<string>() ?? "Unknown tech";
            var progress = activeResearch["progress"]?.Value<double?>() ?? 0;
            var cost = activeResearch["cost"]?.Value<double?>();
            if (cost.HasValue && cost.Value > 0)
            {
                var pct = Math.Clamp(progress / cost.Value * 100d, 0d, 100d);
                return $"{name}  •  {FormatNumber(progress)}/{FormatNumber(cost.Value)} ({pct:0.#}%)";
            }

            return $"{name}  •  progress {FormatNumber(progress)}";
        }

        public string GetWarningsSummary()
        {
            var warnings = RawSummary?["threatWarnings"] as JArray;
            if (warnings == null || warnings.Count == 0)
            {
                return "No active threat warnings.";
            }

            var first = warnings[0] as JObject;
            var headline = first?["headline"]?.Value<string>()
                ?? first?["title"]?.Value<string>()
                ?? first?["summary"]?.Value<string>()
                ?? warnings[0]?.ToString();

            return warnings.Count > 1
                ? $"{headline}  •  +{warnings.Count - 1} more"
                : headline;
        }

        public string GetReadyOpsSummary()
        {
            var ops = RawSummary?["city"]?["settlementOpeningOperations"] as JArray;
            if (ops == null || ops.Count == 0)
            {
                return "No opening operations surfaced.";
            }

            var readyNow = ops
                .OfType<JObject>()
                .Count(op => string.Equals(op["readiness"]?.Value<string>(), "ready_now", StringComparison.OrdinalIgnoreCase));

            var first = ops.OfType<JObject>().FirstOrDefault();
            var title = first?["title"]?.Value<string>() ?? "Operation";
            return readyNow > 0 ? $"{readyNow} ready now  •  {title}" : title;
        }


        public string GetResourceTickSummary()
        {
            var timing = RawSummary?["resourceTickTiming"] as JObject;
            if (timing == null)
            {
                return "Tick timing unavailable.";
            }

            var tickMs = timing["tickMs"]?.Value<double?>() ?? 0;
            if (!(tickMs > 0))
            {
                return "Tick timing unavailable.";
            }

            var cadenceSpan = TimeSpan.FromMilliseconds(tickMs);
            var nextTickAt = ParseUtcToken(timing["nextTickAt"]);
            var lastTickAt = ParseUtcToken(timing["lastTickAt"]);
            var nowUtc = DateTime.UtcNow;
            var cadence = FormatCadence(cadenceSpan);

            string countdown;
            if (nextTickAt.HasValue)
            {
                var remaining = nextTickAt.Value - nowUtc;
                if (remaining <= TimeSpan.Zero || remaining > cadenceSpan + TimeSpan.FromSeconds(5))
                {
                    countdown = lastTickAt.HasValue
                        ? FormatNormalizedRemaining(lastTickAt.Value, cadenceSpan, nowUtc)
                        : FormatRemaining(cadenceSpan);
                }
                else
                {
                    countdown = FormatRemaining(remaining);
                }
            }
            else if (lastTickAt.HasValue)
            {
                countdown = FormatNormalizedRemaining(lastTickAt.Value, cadenceSpan, nowUtc);
            }
            else
            {
                countdown = FormatRemaining(cadenceSpan);
            }

            return $"{countdown}  •  every {cadence}";
        }

        public string GetResearchTimerSummary()
        {
            var activeResearch = RawSummary?["activeResearch"] as JObject;
            if (activeResearch == null)
            {
                return "No active research timer.";
            }

            var name = activeResearch["name"]?.Value<string>() ?? activeResearch["techId"]?.Value<string>() ?? "Unknown tech";
            var progress = activeResearch["progress"]?.Value<double?>() ?? 0;
            var cost = activeResearch["cost"]?.Value<double?>() ?? 0;
            var startedAt = ParseUtcToken(activeResearch["startedAt"]);
            var elapsed = startedAt.HasValue ? FormatElapsed(DateTime.UtcNow - startedAt.Value) : "start unknown";

            if (cost > 0)
            {
                var pct = Math.Clamp(progress / cost * 100d, 0d, 100d);
                return $"{name}  •  {pct:0.#}%  •  running {elapsed}";
            }

            return $"{name}  •  running {elapsed}";
        }

        public string GetWorkshopTimerSummary()
        {
            var jobs = RawSummary?["workshopJobs"] as JArray;
            if (jobs == null || jobs.Count == 0)
            {
                return "No active workshop queue.";
            }

            var activeJobs = jobs
                .OfType<JObject>()
                .Where(job => !(job["completed"]?.Value<bool>() ?? false))
                .ToArray();

            if (activeJobs.Length == 0)
            {
                return "No active workshop queue.";
            }

            var first = activeJobs[0];
            var label = first["attachmentKind"]?.Value<string>() ?? "Workshop job";
            var finishesAt = ParseUtcToken(first["finishesAt"]);
            var timer = finishesAt.HasValue ? FormatRemaining(finishesAt.Value - DateTime.UtcNow) : "time unknown";
            return activeJobs.Length > 1
                ? $"{Cap(label.Replace("_", " "))}  •  {timer}  •  +{activeJobs.Length - 1} queued"
                : $"{Cap(label.Replace("_", " "))}  •  {timer}";
        }

        public string GetMissionTimerSummary()
        {
            var missions = RawSummary?["activeMissions"] as JArray;
            if (missions == null || missions.Count == 0)
            {
                return "No active mission clock.";
            }

            var active = missions.OfType<JObject>().ToArray();
            if (active.Length == 0)
            {
                return "No active mission clock.";
            }

            var first = active[0];
            var mission = first["mission"] as JObject;
            var title = mission?["title"]?.Value<string>() ?? mission?["id"]?.Value<string>() ?? "Active mission";
            var finishesAt = ParseUtcToken(first["finishesAt"]);
            var timer = finishesAt.HasValue ? FormatRemaining(finishesAt.Value - DateTime.UtcNow) : "time unknown";
            return active.Length > 1
                ? $"{title}  •  {timer}  •  +{active.Length - 1} more"
                : $"{title}  •  {timer}";
        }


        public string GetResourceTickRawSummary()
        {
            var timing = RawSummary?["resourceTickTiming"] as JObject;
            if (timing == null)
            {
                return "raw tick timing unavailable.";
            }

            var tickMs = timing["tickMs"]?.Value<double?>() ?? 0;
            var lastTickAt = ParseUtcToken(timing["lastTickAt"]);
            var nextTickAt = ParseUtcToken(timing["nextTickAt"]);
            return $"raw tickMs={tickMs:0} • last={FormatUtc(lastTickAt)} • next={FormatUtc(nextTickAt)}";
        }

        public string GetResourceTickDiagnosticSummary(DateTime nowUtc)
        {
            var timing = RawSummary?["resourceTickTiming"] as JObject;
            if (timing == null)
            {
                return "diag unavailable.";
            }

            var tickMs = timing["tickMs"]?.Value<double?>() ?? 0;
            if (!(tickMs > 0))
            {
                return "diag unavailable.";
            }

            var cadenceSpan = TimeSpan.FromMilliseconds(tickMs);
            var nextTickAt = ParseUtcToken(timing["nextTickAt"]);
            var lastTickAt = ParseUtcToken(timing["lastTickAt"]);

            string remaining;
            if (nextTickAt.HasValue)
            {
                var delta = nextTickAt.Value - nowUtc;
                if (delta <= TimeSpan.Zero || delta > cadenceSpan + TimeSpan.FromSeconds(5))
                {
                    remaining = lastTickAt.HasValue
                        ? FormatNormalizedRemaining(lastTickAt.Value, cadenceSpan, nowUtc)
                        : FormatRemaining(cadenceSpan);
                }
                else
                {
                    remaining = FormatRemaining(delta);
                }
            }
            else if (lastTickAt.HasValue)
            {
                remaining = FormatNormalizedRemaining(lastTickAt.Value, cadenceSpan, nowUtc);
            }
            else
            {
                remaining = FormatRemaining(cadenceSpan);
            }

            return $"diag now={nowUtc:HH:mm:ss} UTC • visible={remaining} • every {FormatCadence(cadenceSpan)}";
        }

        public string GetHeroStatusSummary()
        {
            var heroes = RawSummary?["heroes"] as JArray;
            if (heroes == null || heroes.Count == 0)
            {
                return HasCity ? "No officer corps visible." : "Found a city to unlock officers.";
            }

            var heroObjects = heroes.OfType<JObject>().ToArray();
            var total = heroObjects.Length;
            var idle = heroObjects.Count(hero => string.Equals(hero["status"]?.Value<string>(), "idle", StringComparison.OrdinalIgnoreCase));
            var averageLevel = heroObjects
                .Select(hero => hero["level"]?.Value<double?>())
                .Where(level => level.HasValue)
                .Select(level => level!.Value)
                .DefaultIfEmpty(0)
                .Average();
            var geared = heroObjects.Count(hero => (hero["attachments"] as JArray)?.Count > 0);

            var levelText = averageLevel > 0 ? $"avg lvl {averageLevel:0.#}" : "starter corps";
            return geared > 0
                ? $"{idle}/{total} idle  •  {levelText}  •  {geared} geared"
                : $"{idle}/{total} idle  •  {levelText}";
        }

        public string GetArmyStatusSummary()
        {
            var armies = RawSummary?["armies"] as JArray;
            if (armies == null || armies.Count == 0)
            {
                return HasCity ? "No formations visible." : "Found a city to unlock formations.";
            }

            var armyObjects = armies.OfType<JObject>().ToArray();
            var total = armyObjects.Length;
            var idle = armyObjects.Count(army => string.Equals(army["status"]?.Value<string>(), "idle", StringComparison.OrdinalIgnoreCase));
            var committed = total - idle;
            var ready = armyObjects.Count(army => (army["readiness"]?.Value<double?>() ?? 0) >= 70);
            var averageReadiness = armyObjects
                .Select(army => army["readiness"]?.Value<double?>())
                .Where(readiness => readiness.HasValue)
                .Select(readiness => readiness!.Value)
                .DefaultIfEmpty(0)
                .Average();

            return committed > 0
                ? $"{ready}/{total} ready  •  avg readiness {averageReadiness:0.#}  •  {committed} committed"
                : $"{ready}/{total} ready  •  avg readiness {averageReadiness:0.#}";
        }

        public string GetStatusHeadline()
        {
            if (!HasCity)
            {
                return FounderMode ? "Founding mode active." : "No settlement data returned.";
            }

            return $"{CityName}  •  {LaneLabel}  •  {TierLabel}";
        }

        private JObject GetSettlementDeskSurface()
        {
            return RawSummary?["city"]?["settlementDeskSurface"] as JObject;
        }

        private JObject GetAvailableTechAt(int index)
        {
            if (index < 0)
            {
                return null;
            }

            return (RawSummary?["availableTechs"] as JArray)?.OfType<JObject>().Skip(index).FirstOrDefault();
        }

        private string NormalizeTechFamily(JObject tech)
        {
            var family = tech?["identityFamily"]?.Value<string>()?.Trim();
            if (!string.IsNullOrWhiteSpace(family) && !string.Equals(family, "neutral", StringComparison.OrdinalIgnoreCase))
            {
                return family;
            }

            var laneIdentity = tech?["laneIdentity"]?.Value<string>()?.Trim();
            if (!string.IsNullOrWhiteSpace(laneIdentity) && !string.Equals(laneIdentity, "neutral", StringComparison.OrdinalIgnoreCase))
            {
                return laneIdentity;
            }

            return string.Equals(RawSummary?["city"]?["settlementLane"]?.Value<string>(), "black_market", StringComparison.OrdinalIgnoreCase)
                ? "Shadow research"
                : "Civic Works";
        }

        private string GetAvailableTechPrimaryDetail(JObject tech)
        {
            return tech?["description"]?.Value<string>()?.Trim()
                ?? tech?["identitySummary"]?.Value<string>()?.Trim()
                ?? "No research lore surfaced.";
        }

        private string GetAvailableTechSecondaryDetail(JObject tech)
        {
            return tech?["operatorNote"]?.Value<string>()?.Trim()
                ?? tech?["identitySummary"]?.Value<string>()?.Trim()
                ?? "No operator note surfaced.";
        }

        public int GetAvailableTechCount()
        {
            return (RawSummary?["availableTechs"] as JArray)?.Count ?? 0;
        }

        public string GetDevelopmentDeskHeadline()
        {
            var title = GetSettlementDeskSurface()?["developmentDeskTitle"]?.Value<string>();
            if (string.IsNullOrWhiteSpace(title))
            {
                return $"{CityName} development desk";
            }

            return HasCity ? $"{CityName} — {title}" : title;
        }

        public string GetDevelopmentDeskSummary()
        {
            var summary = GetSettlementDeskSurface()?["developmentDeskSummary"]?.Value<string>();
            if (!string.IsNullOrWhiteSpace(summary))
            {
                return summary.Trim();
            }

            var count = GetAvailableTechCount();
            return $"{count} research option{(count == 1 ? " is" : "s are")} surfaced right now. Use this desk to judge research, workshop, and growth posture before committing the next order.";
        }

        public string GetDevelopmentDeskNote()
        {
            var agendaSummary = GetSettlementDeskSurface()?["developmentAgendaSummary"]?.Value<string>();
            var count = GetAvailableTechCount();
            var suffix = count > 0
                ? $" {count} visible research path{(count == 1 ? " is" : "s are")} riding with this snapshot."
                : " No visible research paths are riding with this snapshot right now.";

            if (!string.IsNullOrWhiteSpace(agendaSummary))
            {
                return agendaSummary.Trim() + suffix;
            }

            return $"Development Desk v1 is a command overview.{suffix} The final tree editor and build queue screens come later.";
        }

        public string GetDevelopmentResearchLaneCopy()
        {
            return GetSettlementDeskSurface()?["developmentFocusSummary"]?.Value<string>()
                ?? "Research keeps focus, next unlock, and commit posture together so the player can judge the next order without dropping back to the summary floor.";
        }

        public string GetDevelopmentWorkshopLaneCopy()
        {
            return GetSettlementDeskSurface()?["developmentQueueSummary"]?.Value<string>()
                ?? "Workshop keeps queue state, mission pressure, and support posture together so attachment and dispatch work reads like one desk instead of three scattered panels.";
        }

        public string GetDevelopmentGrowthLaneCopy()
        {
            return GetSettlementDeskSurface()?["developmentRailSummary"]?.Value<string>()
                ?? "Growth keeps cadence, next resource pulse, and ready-now pressure together so city tempo stays readable before you commit the next civic order.";
        }

        public string GetResearchFocusSummary()
        {
            var activeResearch = RawSummary?["activeResearch"] as JObject;
            var visibleCount = GetAvailableTechCount();
            if (activeResearch != null)
            {
                var progress = GetResearchSummary();
                return visibleCount > 0
                    ? $"{progress}  •  {visibleCount} additional path{(visibleCount == 1 ? "" : "s")} still visible in the desk."
                    : progress;
            }

            var tech = GetAvailableTechAt(0);
            if (tech == null)
            {
                return HasCity ? "No visible research path is surfaced right now." : "Found a city to unlock research.";
            }

            var label = tech["name"]?.Value<string>() ?? tech["id"]?.Value<string>() ?? "Unknown tech";
            var detail = GetAvailableTechSecondaryDetail(tech);
            return string.IsNullOrWhiteSpace(detail) ? label : $"{label}  •  {detail}";
        }

        public string GetResearchNextUnlockSummary()
        {
            var tech = GetAvailableTechAt(0);
            if (tech == null)
            {
                return "No suggested unlock surfaced.";
            }

            var label = tech["name"]?.Value<string>() ?? tech["id"]?.Value<string>() ?? "Unknown tech";
            var cost = tech["cost"]?.Value<double?>();
            var identity = NormalizeTechFamily(tech);

            if (!string.IsNullOrWhiteSpace(identity) && cost.HasValue)
            {
                return $"{label}  •  {identity}  •  cost {FormatNumber(cost.Value)}";
            }

            if (!string.IsNullOrWhiteSpace(identity))
            {
                return $"{label}  •  {identity}";
            }

            return cost.HasValue ? $"{label}  •  cost {FormatNumber(cost.Value)}" : label;
        }

        public string GetResearchCommitPostureSummary()
        {
            var visibleCount = GetAvailableTechCount();
            if (HasActiveResearch())
            {
                return visibleCount > 0
                    ? $"Research is already running. {visibleCount} other path{(visibleCount == 1 ? " is" : "s are")} still visible, but no new commit can start until the current project finishes."
                    : "Research is already running. No additional visible paths are surfaced right now.";
            }

            if (visibleCount > 0)
            {
                return $"{visibleCount} visible research path{(visibleCount == 1 ? " is" : "s are")} open. Commit one from the desk when ready.";
            }

            return HasCity ? "No clean research path is visible in the current snapshot." : "Found a city to unlock research.";
        }

        public string GetSuggestedResearchId()
        {
            if (RawSummary?["activeResearch"] != null)
            {
                return null;
            }

            var first = GetAvailableTechAt(0);
            return first?["id"]?.Value<string>();
        }

        public string GetSuggestedResearchLabel()
        {
            var activeResearch = RawSummary?["activeResearch"] as JObject;
            var visibleCount = GetAvailableTechCount();
            if (activeResearch != null)
            {
                var name = activeResearch["name"]?.Value<string>() ?? activeResearch["techId"]?.Value<string>() ?? "Active research";
                return visibleCount > 0
                    ? $"Research running: {name}. {visibleCount} visible path{(visibleCount == 1 ? " remains" : "s remain")} in the desk."
                    : $"Research running: {name}.";
            }

            var tech = GetAvailableTechAt(0);
            if (tech == null)
            {
                return HasCity ? "No visible research path is surfaced right now." : "Found a city to unlock research.";
            }

            var id = tech["id"]?.Value<string>();
            var label = tech["name"]?.Value<string>() ?? id ?? "Unknown tech";
            var cost = tech["cost"]?.Value<double?>();
            return cost.HasValue ? $"Start research: {label} ({FormatNumber(cost.Value)})" : $"Start research: {label}";
        }

        public bool HasActiveResearch()
        {
            return RawSummary?["activeResearch"] != null;
        }

        public string GetAvailableTechIdAt(int index)
        {
            return GetAvailableTechAt(index)?["id"]?.Value<string>();
        }

        public string GetAvailableTechLabelAt(int index)
        {
            var tech = GetAvailableTechAt(index);
            if (tech == null)
            {
                return "No tech surfaced.";
            }

            var label = tech["name"]?.Value<string>() ?? tech["id"]?.Value<string>() ?? "Unknown tech";
            var cost = tech["cost"]?.Value<double?>();
            return cost.HasValue ? $"{label} ({FormatNumber(cost.Value)})" : label;
        }

        public string GetAvailableTechActionLabelAt(int index)
        {
            var tech = GetAvailableTechAt(index);
            if (tech == null)
            {
                return "No tech";
            }

            var label = tech["name"]?.Value<string>() ?? tech["id"]?.Value<string>() ?? "Unknown tech";
            return $"Start {label}";
        }

        public string GetVisibleResearchCardsSummary()
        {
            var count = GetAvailableTechCount();
            if (count <= 0)
            {
                return "No visible research cards are riding with the current snapshot.";
            }

            var familyCount = (RawSummary?["availableTechs"] as JArray)
                ?.OfType<JObject>()
                .Select(NormalizeTechFamily)
                .Where(label => !string.IsNullOrWhiteSpace(label))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() ?? 0;

            var visibleCards = Math.Min(count, 4);
            return $"{count} visible research path{(count == 1 ? "" : "s")} across {Math.Max(familyCount, 1)} family{(familyCount == 1 ? "" : "ies")}. Showing the first {visibleCards} cards with family, lore, and operator note from the desk snapshot.";
        }

        public string GetAvailableTechFamilyLabelAt(int index)
        {
            var tech = GetAvailableTechAt(index);
            return tech == null ? "Civic Works" : NormalizeTechFamily(tech);
        }

        public string GetAvailableTechTitleAt(int index)
        {
            var tech = GetAvailableTechAt(index);
            return tech?["name"]?.Value<string>() ?? tech?["id"]?.Value<string>() ?? "Unknown tech";
        }

        public string GetAvailableTechLoreAt(int index)
        {
            var tech = GetAvailableTechAt(index);
            return tech == null ? "No research lore surfaced." : GetAvailableTechPrimaryDetail(tech);
        }

        public string GetAvailableTechOperatorNoteAt(int index)
        {
            var tech = GetAvailableTechAt(index);
            return tech == null ? "No operator note surfaced." : GetAvailableTechSecondaryDetail(tech);
        }



        private JObject[] GetActiveWorkshopJobs()
        {
            return (RawSummary?["workshopJobs"] as JArray)
                ?.OfType<JObject>()
                .Where(job => !(job["completed"]?.Value<bool>() ?? false))
                .ToArray()
                ?? Array.Empty<JObject>();
        }

        private JObject[] GetActiveMissionEntries()
        {
            return (RawSummary?["activeMissions"] as JArray)
                ?.OfType<JObject>()
                .ToArray()
                ?? Array.Empty<JObject>();
        }

        private JObject[] GetGrowthOpeningOperations()
        {
            return (RawSummary?["city"]?["settlementOpeningOperations"] as JArray)
                ?.OfType<JObject>()
                .OrderByDescending(op => string.Equals(op["readiness"]?.Value<string>(), "ready_now", StringComparison.OrdinalIgnoreCase))
                .ThenBy(op => op["title"]?.Value<string>() ?? string.Empty)
                .ToArray()
                ?? Array.Empty<JObject>();
        }

        private JObject BuildWorkshopCardFromJob(JObject job)
        {
            var kind = job?["attachmentKind"]?.Value<string>() ?? "workshop_job";
            var title = Cap(kind.Replace("_", " "));
            var finishesAt = ParseUtcToken(job?["finishesAt"]);
            var timer = finishesAt.HasValue ? FormatRemaining(finishesAt.Value - DateTime.UtcNow) : "time unknown";
            return new JObject
            {
                ["family"] = "Workshop queue",
                ["title"] = title,
                ["lore"] = job?["summary"]?.Value<string>()
                    ?? job?["description"]?.Value<string>()
                    ?? "Queued in the workshop and waiting for completion.",
                ["note"] = $"Finishes {timer}."
            };
        }

        private JObject BuildWorkshopCardFromMission(JObject entry)
        {
            var mission = entry?["mission"] as JObject;
            var finishesAt = ParseUtcToken(entry?["finishesAt"]);
            var timer = finishesAt.HasValue ? FormatRemaining(finishesAt.Value - DateTime.UtcNow) : "time unknown";
            return new JObject
            {
                ["family"] = "Mission support",
                ["title"] = mission?["title"]?.Value<string>() ?? mission?["id"]?.Value<string>() ?? "Active mission",
                ["lore"] = mission?["summary"]?.Value<string>()
                    ?? mission?["description"]?.Value<string>()
                    ?? "Field work is currently drawing city support and attention.",
                ["note"] = $"Mission clock {timer}."
            };
        }

        private JObject[] BuildWorkshopCards()
        {
            var cards = GetActiveWorkshopJobs()
                .Select(BuildWorkshopCardFromJob)
                .Concat(GetActiveMissionEntries().Select(BuildWorkshopCardFromMission))
                .Take(4)
                .ToArray();
            return cards;
        }

        private JObject BuildGrowthCardFromOperation(JObject op)
        {
            var readiness = op?["readiness"]?.Value<string>();
            var note = op?["playerFacingSummary"]?.Value<string>()
                ?? op?["summary"]?.Value<string>()
                ?? op?["headline"]?.Value<string>()
                ?? op?["responseHint"]?.Value<string>()
                ?? (!string.IsNullOrWhiteSpace(readiness) ? $"Current readiness: {readiness.Replace("_", " ")}." : "Growth opening surfaced from the city board.");

            return new JObject
            {
                ["family"] = op?["laneDetail"]?.Value<string>()
                    ?? op?["category"]?.Value<string>()
                    ?? op?["kind"]?.Value<string>()
                    ?? "Growth opportunity",
                ["title"] = op?["title"]?.Value<string>() ?? "Opening operation",
                ["lore"] = op?["summary"]?.Value<string>()
                    ?? op?["description"]?.Value<string>()
                    ?? op?["headline"]?.Value<string>()
                    ?? "A visible growth opening is currently surfaced on the city board.",
                ["note"] = note
            };
        }

        private JObject[] BuildGrowthCards()
        {
            return GetGrowthOpeningOperations()
                .Select(BuildGrowthCardFromOperation)
                .Take(4)
                .ToArray();
        }

        private JObject GetWorkshopCardAt(int index)
        {
            if (index < 0)
            {
                return null;
            }

            return BuildWorkshopCards().Skip(index).FirstOrDefault();
        }

        private JObject GetGrowthCardAt(int index)
        {
            if (index < 0)
            {
                return null;
            }

            return BuildGrowthCards().Skip(index).FirstOrDefault();
        }

        private static string GetCardField(JObject card, string field, string fallback)
        {
            return card?[field]?.Value<string>() ?? fallback;
        }

        public int GetVisibleWorkshopCardCount()
        {
            return BuildWorkshopCards().Length;
        }

        public string GetVisibleWorkshopCardsSummary()
        {
            var cards = BuildWorkshopCards();
            if (cards.Length <= 0)
            {
                return "No visible workshop cards are riding with the current snapshot.";
            }

            var familyCount = cards
                .Select(card => card?["family"]?.Value<string>())
                .Where(label => !string.IsNullOrWhiteSpace(label))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            return $"{cards.Length} visible workshop card{(cards.Length == 1 ? string.Empty : "s")} across {Math.Max(familyCount, 1)} lane{(familyCount == 1 ? string.Empty : "s")}. Showing queue and mission-support pressure from the current snapshot.";
        }

        public string GetWorkshopCardFamilyLabelAt(int index)
        {
            return GetCardField(GetWorkshopCardAt(index), "family", "Workshop queue");
        }

        public string GetWorkshopCardTitleAt(int index)
        {
            return GetCardField(GetWorkshopCardAt(index), "title", "Workshop card");
        }

        public string GetWorkshopCardLoreAt(int index)
        {
            return GetCardField(GetWorkshopCardAt(index), "lore", "No workshop lore surfaced.");
        }

        public string GetWorkshopCardNoteAt(int index)
        {
            return GetCardField(GetWorkshopCardAt(index), "note", "No workshop note surfaced.");
        }

        public int GetVisibleGrowthCardCount()
        {
            return BuildGrowthCards().Length;
        }

        public string GetVisibleGrowthCardsSummary()
        {
            var cards = BuildGrowthCards();
            if (cards.Length <= 0)
            {
                return "No visible growth cards are riding with the current snapshot.";
            }

            var familyCount = cards
                .Select(card => card?["family"]?.Value<string>())
                .Where(label => !string.IsNullOrWhiteSpace(label))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            return $"{cards.Length} visible growth card{(cards.Length == 1 ? string.Empty : "s")} across {Math.Max(familyCount, 1)} lane{(familyCount == 1 ? string.Empty : "s")}. Showing live city openings from the current snapshot.";
        }

        public string GetGrowthCardFamilyLabelAt(int index)
        {
            return GetCardField(GetGrowthCardAt(index), "family", "Growth opportunity");
        }

        public string GetGrowthCardTitleAt(int index)
        {
            return GetCardField(GetGrowthCardAt(index), "title", "Growth card");
        }

        public string GetGrowthCardLoreAt(int index)
        {
            return GetCardField(GetGrowthCardAt(index), "lore", "No growth lore surfaced.");
        }

        public string GetGrowthCardNoteAt(int index)
        {
            return GetCardField(GetGrowthCardAt(index), "note", "No growth note surfaced.");
        }

        private void NotifyChanged()
        {
            Changed?.Invoke();
        }


        private static DateTime? ParseUtcToken(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return null;
            }

            if (token.Type == JTokenType.Date)
            {
                var value = token.Value<DateTime>();
                return value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
            }

            return ParseUtc(token.Value<string>());
        }

        private static string FormatUtc(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) : "-";
        }

        private static DateTime? ParseUtc(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var parsed)
                ? parsed
                : null;
        }

        private static string FormatRemaining(TimeSpan remaining)
        {
            if (remaining <= TimeSpan.Zero)
            {
                return "ready now";
            }

            if (remaining.TotalHours >= 1)
            {
                return $"{Math.Floor(remaining.TotalHours):0}h {remaining.Minutes:00}m left";
            }

            return $"{Math.Max(0, remaining.Minutes):0}m {Math.Max(0, remaining.Seconds):00}s left";
        }

        private static string FormatNormalizedRemaining(DateTime lastTickAtUtc, TimeSpan cadence, DateTime nowUtc)
        {
            if (cadence <= TimeSpan.Zero)
            {
                return "time unknown";
            }

            var normalizedLast = lastTickAtUtc > nowUtc + cadence ? nowUtc : lastTickAtUtc;
            var elapsed = nowUtc - normalizedLast;
            if (elapsed < TimeSpan.Zero)
            {
                elapsed = TimeSpan.Zero;
            }

            var elapsedTicks = Math.Floor(elapsed.TotalMilliseconds / cadence.TotalMilliseconds);
            var nextTickAt = normalizedLast + TimeSpan.FromMilliseconds((elapsedTicks + 1) * cadence.TotalMilliseconds);
            return FormatRemaining(nextTickAt - nowUtc);
        }

        private static string FormatElapsed(TimeSpan elapsed)
        {
            if (elapsed <= TimeSpan.Zero)
            {
                return "just started";
            }

            if (elapsed.TotalHours >= 1)
            {
                return $"{Math.Floor(elapsed.TotalHours):0}h {elapsed.Minutes:00}m";
            }

            return $"{Math.Max(0, elapsed.Minutes):0}m {Math.Max(0, elapsed.Seconds):00}s";
        }

        private static string FormatCadence(TimeSpan cadence)
        {
            if (cadence.TotalHours >= 1)
            {
                return $"{Math.Floor(cadence.TotalHours):0}h {cadence.Minutes:00}m";
            }

            if (cadence.TotalMinutes >= 1)
            {
                return $"{Math.Floor(cadence.TotalMinutes):0}m {cadence.Seconds:00}s";
            }

            return $"{Math.Max(0, cadence.Seconds):0}s";
        }

        private static string Cap(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            return char.ToUpperInvariant(value[0]) + value[1..];
        }

        private static string FormatNumber(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
