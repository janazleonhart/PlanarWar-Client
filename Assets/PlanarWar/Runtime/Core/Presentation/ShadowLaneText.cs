using PlanarWar.Client.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanarWar.Client.Core.Presentation
{
    public static class ShadowLaneText
    {
        public static string BuildGroundedContractValue(
            OperationSnapshot primaryOp,
            BlackMarketActiveOperationCardSnapshot card,
            BlackMarketRuntimeTruthSurfaceSnapshot runtime,
            BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var title = FirstNonBlank(
                primaryOp?.Title,
                primaryOp?.FocusLabel,
                card?.Headline,
                runtime?.Headline,
                payoff?.Headline,
                "No shadow contract surfaced.");

            return $"{title} • {BuildLifecycleLead(primaryOp, card, runtime, payoff)}";
        }

        public static string BuildGroundedContractNote(
            OperationSnapshot primaryOp,
            BlackMarketActiveOperationCardSnapshot card,
            BlackMarketRuntimeTruthSurfaceSnapshot runtime,
            BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            return FirstNonBlank(
                CompactSingleLine(primaryOp?.Summary),
                CompactSingleLine(primaryOp?.WhyNow),
                CompactSingleLine(card?.Summary),
                CompactSingleLine(runtime?.Detail),
                CompactSingleLine(payoff?.Detail),
                "The live shadow seam is grounded on the board and can be worked without inventing fake underworld drama.");
        }

        public static string BuildLifecycleValue(
            OperationSnapshot primaryOp,
            BlackMarketActiveOperationCardSnapshot card,
            BlackMarketRuntimeTruthSurfaceSnapshot runtime,
            BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            return $"{BuildLifecycleLead(primaryOp, card, runtime, payoff)} • {BuildLifecycleTail(primaryOp, card, runtime, payoff)}";
        }

        public static string BuildLifecycleNote(
            OperationSnapshot primaryOp,
            BlackMarketActiveOperationCardSnapshot card,
            BlackMarketRuntimeTruthSurfaceSnapshot runtime,
            BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            return FirstNonBlank(
                CompactSingleLine(card?.OperatorNote),
                CompactSingleLine(primaryOp?.WhyNow),
                CompactSingleLine(payoff?.StateReason),
                CompactSingleLine(runtime?.ActiveOperation?.Detail),
                CompactSingleLine(runtime?.OperatorFrontSummary),
                "Lifecycle stays tied to the live shadow line so the desk can tell whether the route is merely offered, already committed, already answered, or cooling.");
        }

        public static string BuildEffectsValue(
            OperationSnapshot primaryOp,
            BlackMarketActiveOperationCardSnapshot card,
            BlackMarketRuntimeTruthSurfaceSnapshot runtime,
            BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            return $"Receipt chain {ResolveReceiptChainState(primaryOp, card, runtime, payoff)} • Covert carry {ResolveCovertCarryState(primaryOp, card, runtime, payoff)}";
        }

        public static string BuildEffectsNote(
            OperationSnapshot primaryOp,
            BlackMarketActiveOperationCardSnapshot card,
            BlackMarketRuntimeTruthSurfaceSnapshot runtime,
            BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var receipt = payoff?.RecentReceipts?.FirstOrDefault();
            if (receipt != null)
            {
                return $"Latest receipt: {FirstNonBlank(receipt.Title, "Receipt")} • {FirstNonBlank(CompactSingleLine(receipt.Summary), CompactSingleLine(receipt.Detail), "Shadow paperwork is still moving.")}";
            }

            return FirstNonBlank(
                CompactSingleLine(primaryOp?.Payoff),
                CompactSingleLine(card?.Summary),
                CompactSingleLine(runtime?.PublicBackbonePressure?.Detail),
                CompactSingleLine(payoff?.Detail),
                "Use this line to show what the shadow contract is already carrying: linked receipts, quiet routes, and whether cleanup is holding.");
        }

        public static string BuildPressureBendValue(
            BlackMarketRuntimeTruthSurfaceSnapshot runtime,
            BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            return $"{ResolveHeatState(runtime, payoff)} • {ResolveExposureState(runtime, payoff)}";
        }

        public static string BuildPressureBendNote(
            OperationSnapshot primaryOp,
            BlackMarketRuntimeTruthSurfaceSnapshot runtime,
            BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var receipt = payoff?.RecentReceipts?.FirstOrDefault();
            if (receipt != null)
            {
                return $"Latest receipt: {FirstNonBlank(receipt.Title, "Receipt")} • {FirstNonBlank(CompactSingleLine(receipt.Summary), CompactSingleLine(receipt.Detail), "No extra receipt detail surfaced.")}";
            }

            return FirstNonBlank(
                CompactSingleLine(runtime?.PublicBackbonePressure?.RecommendedAction),
                CompactSingleLine(payoff?.RecommendedAction),
                CompactSingleLine(primaryOp?.WhyNow),
                CompactSingleLine(runtime?.Detail),
                "Shadow pressure should read as heat, exposure, and quiet carry pressure instead of collapsing back into a generic throughput bar.");
        }

        public static string BuildOperationPostureFromKind(string kind)
        {
            switch ((kind ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "counterfeit_job":
                    return "Receipt-chain routing / forged paper";
                case "exploit":
                    return "Shadow books / covert cash-out";
                case "containment":
                case "cover_repair":
                    return "Deniable cleanup / route cooling";
                case "backbone_pressure":
                case "bribery":
                case "warning_window":
                    return "Heat management / quiet leverage";
                default:
                    return string.Empty;
            }
        }

        public static string BuildOperationPostureFromText(OperationSnapshot operation)
        {
            var text = CombinedContext(operation).ToLowerInvariant();

            if (ContainsAny(text, "counterfeit", "script", "ledger", "paper", "receipt", "book"))
            {
                return "Receipt-chain routing / forged paper";
            }

            if (ContainsAny(text, "cover", "cleanup", "contain", "cool", "wash"))
            {
                return "Deniable cleanup / route cooling";
            }

            if (ContainsAny(text, "bribe", "pressure", "warning", "heat", "leverage", "exposure"))
            {
                return "Heat management / quiet leverage";
            }

            if (ContainsAny(text, "covert", "exploit", "cash", "smuggle", "carry", "route"))
            {
                return "Shadow books / covert cash-out";
            }

            return "Covert carry / quiet pressure";
        }

        public static string HumanizeShadowKind(string kind)
        {
            switch ((kind ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "counterfeit_job":
                    return "Receipt-chain routing";
                case "backbone_pressure":
                    return "Heat pressure";
                case "cover_repair":
                    return "Deniable cleanup";
                case "warning_window":
                    return "Exposure window";
                case "containment":
                    return "Route containment";
                case "bribery":
                    return "Quiet leverage";
                case "exploit":
                    return "Shadow books";
                default:
                    return HumanizeWords(kind, "Shadow seam");
            }
        }

        public static string DescribeDevelopmentHeadline(CitySummarySnapshot city)
        {
            return city == null ? "Black Market desk unavailable" : $"{FirstNonBlank(city.Name, "Black Market")} • Black Market";
        }

        public static string DescribeDevelopmentCopy(CitySummarySnapshot city)
        {
            return city == null
                ? "Black Market desks stay read-only until a settlement snapshot exists."
                : "Black Market desks keep shadow books, covert supply, and carry posture visible without pretending the lane is just civic growth with darker wallpaper.";
        }

        public static string DescribeGrowth(TimerSnapshot timing, IReadOnlyList<CityTimerEntrySnapshot> timers)
        {
            var liveTimers = timers?.Count ?? 0;
            if (timing?.NextTickAtUtc.HasValue == true)
            {
                return $"Next carry check in {FormatRemaining(timing.NextTickAtUtc.Value - DateTime.UtcNow)} • {liveTimers} live timer(s).";
            }

            return liveTimers > 0
                ? $"{liveTimers} live timer(s) visible; covert cadence is readable."
                : "Carry cadence unavailable.";
        }

        public static string DescribeSupport(ShellSummarySnapshot summary)
        {
            if (summary?.ThreatWarnings?.Count > 0)
            {
                return $"Heat: {summary.ThreatWarnings[0].Headline}";
            }

            if (summary?.OpeningOperations?.Count > 0)
            {
                return $"Opening operations visible: {string.Join(" • ", summary.OpeningOperations.Take(2).Select(o => FirstNonBlank(o?.Title, "Shadow line")))}";
            }

            return "No live heat or shadow opening is visible right now.";
        }

        public static string DescribeSupportTitle(ShellSummarySnapshot summary)
        {
            if (summary?.ThreatWarnings?.Count > 0) return "Heat posture";
            if (summary?.OpeningOperations?.Count > 0) return "Shadow opening posture";
            return "Shadow support posture";
        }

        public static string BuildDeskNote(ShellSummarySnapshot summary, string actionStatus)
        {
            if (!string.IsNullOrWhiteSpace(actionStatus))
            {
                return actionStatus;
            }

            var firstTech = summary?.AvailableTechs?.FirstOrDefault(tech => tech != null && !string.IsNullOrWhiteSpace(tech.Name))?.Name;
            var techLead = string.IsNullOrWhiteSpace(firstTech) ? "no front staged" : $"next front {firstTech}";
            var workshopLead = (summary?.WorkshopJobs?.Count ?? 0) > 0 ? "live front visible" : "no live front surfaced";

            return $"Shadow books: {summary?.AvailableTechs?.Count ?? 0} tech option(s) • {techLead} • covert supply {summary?.WorkshopJobs?.Count ?? 0} live front(s) • {workshopLead} • {summary?.CityTimers?.Count ?? 0} live timer(s) • {summary?.OpeningOperations?.Count ?? 0} opening line(s).";
        }

        public static string BuildResearchLaneTitle() => "Shadow books";

        public static string BuildResearchLaneCopy()
        {
            return "Shadow books keep permit memory, forged paper, and quiet leverage readable without pretending the lane is just public paperwork.";
        }

        public static string BuildWorkshopLaneTitle() => "Covert supply";

        public static string BuildWorkshopLaneCopy()
        {
            return "Covert supply keeps live fronts, ready pickups, and quiet fabrication visible in one place.";
        }

        public static string BuildGrowthLaneTitle() => "Carry lane";

        public static string BuildGrowthLaneCopy()
        {
            return "Carry lanes keep cadence, staffing, and live heat readable so the Black Market feels like covert logistics instead of a darker civic shell.";
        }

        public static string DescribeResearchLaneValue(IReadOnlyList<TechOptionSnapshot> techs)
        {
            var list = techs?.Where(tech => tech != null).ToList() ?? new List<TechOptionSnapshot>();
            if (list.Count == 0)
            {
                return "No active shadow-book or front option surfaced.";
            }

            var preview = BuildFrontPreview(list.Select(tech => tech.Name), 2);
            return $"{list.Count} shadow-book/front option{(list.Count == 1 ? string.Empty : "s")} ready{preview}";
        }

        public static string BuildNextTechValue(IReadOnlyList<TechOptionSnapshot> techs)
        {
            var first = techs?.FirstOrDefault(tech => tech != null && !string.IsNullOrWhiteSpace(tech.Name));
            if (first == null)
            {
                return "No quiet leverage unlock surfaced.";
            }

            return $"{first.Name} • {BuildTechFamily(first)}";
        }


        public static string BuildTechFamily(TechOptionSnapshot tech)
        {
            var text = JoinNonBlank(
                tech?.Name,
                tech?.Description,
                tech?.Category,
                tech?.IdentityFamily,
                tech?.IdentitySummary,
                tech?.LaneIdentity,
                tech?.UnlockPreview != null ? string.Join(" ", tech.UnlockPreview) : string.Empty).ToLowerInvariant();

            if (ContainsAny(text, "charter", "registry", "ledger", "permit", "seal", "paper", "record"))
            {
                return "Paper front";
            }

            if (ContainsAny(text, "road", "route", "bridge", "calendar", "caravan", "convoy", "traffic"))
            {
                return "Carry route";
            }

            if (ContainsAny(text, "granary", "reserve", "stock", "warehouse", "stash", "husbandry", "breeding"))
            {
                return "Quiet stock";
            }

            if (ContainsAny(text, "forge", "craft", "recipe", "workshop", "fabrication"))
            {
                return "Supply front";
            }

            return "Shadow front";
        }

        public static string BuildTechLore(TechOptionSnapshot tech)
        {
            return FirstNonBlank(
                CompactSingleLine(tech?.IdentitySummary),
                CompactSingleLine(tech?.Description),
                tech?.UnlockPreview?.FirstOrDefault(),
                "Quiet leverage from the current book.");
        }

        public static string BuildTechNote(TechOptionSnapshot tech)
        {
            var parts = new List<string>();

            if (tech?.Cost.HasValue == true)
            {
                parts.Add($"Cost {tech.Cost.Value:0.#}");
            }

            if (!string.IsNullOrWhiteSpace(tech?.OperatorNote))
            {
                parts.Add(CompactSingleLine(tech.OperatorNote));
            }

            var preview = tech?.UnlockPreview?.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(preview))
            {
                parts.Add(CompactSingleLine(preview));
            }

            if (!string.IsNullOrWhiteSpace(tech?.LaneIdentity) && !string.Equals(tech.LaneIdentity, "neutral", StringComparison.OrdinalIgnoreCase))
            {
                parts.Add($"Lane {HumanizeWords(tech.LaneIdentity, "Shadow")}");
            }

            return parts.Count > 0
                ? string.Join(" • ", parts)
                : "Quiet leverage entry from the current shadow book.";
        }

        public static string DescribeResearchCardsCopy(ShellSummarySnapshot summary)
        {
            var active = summary?.ActiveResearch != null;
            var techs = summary?.AvailableTechs?.Where(tech => tech != null).ToList() ?? new List<TechOptionSnapshot>();
            var count = techs.Count;
            var visibleSlots = Math.Max(0, 4 - (active ? 1 : 0));
            var hiddenPreview = BuildFrontPreview(techs.Skip(visibleSlots).Select(tech => tech.Name), 2);

            if (active)
            {
                return count > 0
                    ? $"Live front plus {count} shadow book option(s) surfaced{hiddenPreview}"
                    : "Live front is active; no extra shadow book entry is visible right now.";
            }

            if (count > 0)
            {
                var frontPreview = BuildFrontPreview(techs.Select(tech => tech.Name), 2);
                return $"Showing {count} shadow book option(s) from /api/me{frontPreview}";
            }

            return "No shadow book entry is visible right now.";
        }

        public static string BuildWorkshopJobFamily(bool ready)
        {
            return ready ? "Ready drop" : "Live front";
        }

        public static string BuildWorkshopJobLore(WorkshopJobSnapshot job, bool ready, DateTime nowUtc)
        {
            if (ready)
            {
                return !string.IsNullOrWhiteSpace(job?.OutputName)
                    ? $"{job.OutputName} is ready for pickup."
                    : "Quiet drop ready for pickup.";
            }

            if (job?.FinishesAtUtc.HasValue == true)
            {
                return $"Ready in {FormatRemaining(job.FinishesAtUtc.Value - nowUtc)}";
            }

            return !string.IsNullOrWhiteSpace(job?.OutputName)
                ? $"{job.OutputName} is still moving through the front."
                : "Covert fabrication is still moving through the front.";
        }

        public static string BuildWorkshopJobNote(WorkshopJobSnapshot job, bool ready)
        {
            if (ready)
            {
                return !string.IsNullOrWhiteSpace(job?.OutputName)
                    ? $"Carry time elapsed. Collect {job.OutputName} while the route is still quiet."
                    : "Carry time elapsed. Collect the drop before the route heats up.";
            }

            return !string.IsNullOrWhiteSpace(job?.OutputName)
                ? $"{job.OutputName} is still moving through the front."
                : "Covert fabrication is still moving through the front.";
        }

        public static string BuildWorkshopTimerFamily(CityTimerEntrySnapshot timer)
        {
            var text = JoinNonBlank(timer?.Category, timer?.Label, timer?.Detail).ToLowerInvariant();

            if (ContainsAny(text, "ready", "pickup", "collect"))
            {
                return "Ready timer";
            }

            return "Carry timer";
        }

        public static string BuildWorkshopTimerLore(CityTimerEntrySnapshot timer, DateTime nowUtc)
        {
            if (timer?.FinishesAtUtc.HasValue == true)
            {
                return $"{HumanizeWords(timer.Status, "Active")} • {FormatRemaining(timer.FinishesAtUtc.Value - nowUtc)}";
            }

            return FirstNonBlank(HumanizeWords(timer?.Status, string.Empty), "Carry watch");
        }

        public static string BuildWorkshopTimerNote(CityTimerEntrySnapshot timer)
        {
            return FirstNonBlank(
                CompactSingleLine(timer?.Detail),
                "Workshop cadence is still visible even without a clean job body.");
        }

        public static string BuildWorkshopRecipeFamily(WorkshopRecipeSnapshot recipe)
        {
            var text = JoinNonBlank(
                recipe?.GearFamily,
                recipe?.Name,
                recipe?.Summary,
                recipe?.ResponseTags != null ? string.Join(" ", recipe.ResponseTags) : string.Empty).ToLowerInvariant();

            if (ContainsAny(text, "armor", "weapon", "gear", "blade", "bow", "staff"))
            {
                return "Quiet gear";
            }

            if (ContainsAny(text, "food", "meal", "supply", "kit", "pack", "crate"))
            {
                return "Covert supply";
            }

            return "Shadow recipe";
        }

        public static string BuildWorkshopRecipeLore(WorkshopRecipeSnapshot recipe)
        {
            return FirstNonBlank(
                CompactSingleLine(recipe?.Summary),
                recipe?.ResponseTags?.FirstOrDefault(),
                !string.IsNullOrWhiteSpace(recipe?.GearFamily) ? HumanizeWords(recipe.GearFamily, string.Empty) : string.Empty,
                "Quiet fabrication recipe from the current book.");
        }

        public static string BuildWorkshopRecipeNote(WorkshopRecipeSnapshot recipe)
        {
            var parts = new List<string>();

            if (recipe?.WealthCost.HasValue == true) parts.Add($"Wealth {recipe.WealthCost.Value:0.#}");
            if (recipe?.ManaCost.HasValue == true) parts.Add($"Mana {recipe.ManaCost.Value:0.#}");
            if (recipe?.MaterialsCost.HasValue == true) parts.Add($"Materials {recipe.MaterialsCost.Value:0.#}");
            if (recipe?.CraftMinutes.HasValue == true) parts.Add($"Time {FormatMinutes(recipe.CraftMinutes.Value)}");
            if (recipe?.ResponseTags?.Count > 0) parts.Add(string.Join("/", recipe.ResponseTags.Take(2).Select(t => HumanizeWords(t, t))));

            return parts.Count > 0
                ? string.Join(" • ", parts)
                : "Quiet fabrication recipe is ready to run.";
        }

        public static string DescribeWorkshopCardsCopy(int activeJobs, int readyJobs, int recipeCount, int workshopTimers)
        {
            return DescribeWorkshopCardsCopy(activeJobs, readyJobs, recipeCount, workshopTimers, null);
        }

        public static string DescribeWorkshopCardsCopy(int activeJobs, int readyJobs, int recipeCount, int workshopTimers, IReadOnlyList<WorkshopRecipeSnapshot> recipes)
        {
            if (activeJobs > 0 || readyJobs > 0)
            {
                var suffix = recipeCount > 0 ? $" • {recipeCount} covert recipe(s) staged" : string.Empty;
                return $"{activeJobs} live front(s) • {readyJobs} ready drop(s){suffix}";
            }

            if (recipeCount > 0)
            {
                var preview = BuildFrontPreview((recipes ?? Array.Empty<WorkshopRecipeSnapshot>()).Select(recipe => recipe?.Name), 2);
                return $"{recipeCount} covert recipe(s) ready{preview}";
            }

            if (workshopTimers > 0)
            {
                return $"{workshopTimers} carry timer(s) visible.";
            }

            return "No covert supply front is visible right now.";
        }

        public static string DescribeWorkshopLane(int activeJobs, int readyJobs, int recipeCount, int workshopTimers)
        {
            return DescribeWorkshopCardsCopy(activeJobs, readyJobs, recipeCount, workshopTimers);
        }

        public static string DescribeWorkshopLane(int activeJobs, int readyJobs, IReadOnlyList<WorkshopRecipeSnapshot> recipes, int workshopTimers)
        {
            return DescribeWorkshopCardsCopy(activeJobs, readyJobs, recipes?.Count ?? 0, workshopTimers, recipes);
        }

        public static string BuildProductionFamily() => "Throughput";

        public static string BuildProductionTitle() => "Per-tick throughput";

        public static string BuildProductionNote(TimerSnapshot timing, DateTime nowUtc)
        {
            if (timing?.NextTickAtUtc.HasValue == true)
            {
                return $"Next carry check in {FormatRemaining(timing.NextTickAtUtc.Value - nowUtc)}";
            }

            return "Throughput cadence is visible without a live anchor.";
        }

        public static string BuildSupportCardNote(ShellSummarySnapshot summary)
        {
            if (summary?.OpeningOperations?.Count > 0)
            {
                return $"Opening lines visible: {string.Join(" • ", summary.OpeningOperations.Take(2).Select(o => FirstNonBlank(o?.Title, "Shadow line")))}";
            }

            if (summary?.ThreatWarnings?.Count > 0)
            {
                return $"Heat warning visible: {summary.ThreatWarnings[0].Headline}";
            }

            return "No extra heat or opening line is surfaced right now.";
        }

        public static string DescribeGrowthCardsCopy(
            HeroRecruitmentSnapshot heroRecruitment,
            CityTimerEntrySnapshot recruitTimer,
            OperationSnapshot recruitOp,
            IReadOnlyList<CityTimerEntrySnapshot> cityTimers)
        {
            var liveTimers = cityTimers?.Count ?? 0;

            if (heroRecruitment != null && string.Equals(heroRecruitment.Status, "candidates_ready", StringComparison.OrdinalIgnoreCase))
            {
                return $"Shadow candidate review is live with {heroRecruitment.Candidates.Count} option(s) ready for pickup.";
            }

            if (heroRecruitment != null && string.Equals(heroRecruitment.Status, "scouting", StringComparison.OrdinalIgnoreCase))
            {
                if (heroRecruitment.FinishesAtUtc.HasValue && heroRecruitment.FinishesAtUtc.Value <= DateTime.UtcNow)
                {
                    return "Scout window elapsed. Refresh now to load fresh candidate cards.";
                }

                return "Scout front is live and will resolve into recruitable candidates when the timer closes.";
            }

            if (heroRecruitment != null && string.Equals(heroRecruitment.Status, "idle", StringComparison.OrdinalIgnoreCase))
            {
                return heroRecruitment.StartEligible
                    ? "Hero recruitment can be opened directly from the carry lane."
                    : FirstNonBlank(heroRecruitment.BlockedReason, "Recruitment is idle but blocked until shortfalls clear.");
            }

            if (recruitTimer != null)
            {
                return $"Showing carry cadence, scout timer, and {liveTimers} live timer(s).";
            }

            if (recruitOp != null)
            {
                return "Scout opening is visible from the current shadow books.";
            }

            return $"Showing carry cadence, {liveTimers} live timer(s), and current heat posture from the summary payload.";
        }

        private static string BuildLifecycleLead(
            OperationSnapshot primaryOp,
            BlackMarketActiveOperationCardSnapshot card,
            BlackMarketRuntimeTruthSurfaceSnapshot runtime,
            BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var readiness = (primaryOp?.Readiness ?? string.Empty).Trim().ToLowerInvariant();
            if (readiness == "ready_now") return "Available";
            if (readiness == "prepare_soon") return "Committed";
            if (readiness == "blocked") return "Cooling";

            var cardState = (card?.State ?? string.Empty).Trim().ToLowerInvariant();
            if (cardState == "forming") return "Committed";
            if (cardState == "live") return "Answered";
            if (cardState == "cooling") return "Cooling";

            var phase = (payoff?.Phase ?? string.Empty).Trim().ToLowerInvariant();
            if (phase == "payoff_live" || phase == "backlash_live") return "Answered";
            if (phase == "cooling") return "Cooling";

            var band = (runtime?.RuntimeBand ?? string.Empty).Trim().ToLowerInvariant();
            if (band == "hot" || band == "active") return "Committed";

            return "Available";
        }

        private static string BuildLifecycleTail(
            OperationSnapshot primaryOp,
            BlackMarketActiveOperationCardSnapshot card,
            BlackMarketRuntimeTruthSurfaceSnapshot runtime,
            BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var readiness = (primaryOp?.Readiness ?? string.Empty).Trim().ToLowerInvariant();
            if (readiness == "ready_now") return "Current offers";
            if (readiness == "prepare_soon") return "Quiet carry forming";
            if (readiness == "blocked") return "Route cooling";

            var phase = (payoff?.Phase ?? string.Empty).Trim().ToLowerInvariant();
            if (phase == "payoff_live") return "Receipt window live";
            if (phase == "backlash_live") return "Exposure live";
            if (phase == "cooling") return "Cooling route";

            var cardState = (card?.State ?? string.Empty).Trim().ToLowerInvariant();
            if (cardState == "live") return "Quiet carry in motion";
            if (cardState == "forming") return "Quiet carry forming";
            if (cardState == "cooling") return "Cooling route";

            if (string.Equals((runtime?.RuntimeBand ?? string.Empty).Trim(), "watch", StringComparison.OrdinalIgnoreCase))
            {
                return "Quiet pressure watch";
            }

            return "Current offers";
        }

        private static string ResolveReceiptChainState(
            OperationSnapshot primaryOp,
            BlackMarketActiveOperationCardSnapshot card,
            BlackMarketRuntimeTruthSurfaceSnapshot runtime,
            BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var text = CombinedContext(primaryOp, card, runtime, payoff).ToLowerInvariant();

            if (ContainsAny(text, "cooling", "cool", "cleanup", "washed"))
            {
                return "cooling";
            }

            if (ContainsAny(text, "trace", "traced", "audit", "expose", "exposed"))
            {
                return "traced";
            }

            if (ContainsAny(text, "counterfeit", "receipt", "script", "paper", "ledger", "book"))
            {
                return "linked";
            }

            if (IsLiveOrHot(card, runtime, payoff))
            {
                return "linked";
            }

            return "staged";
        }

        private static string ResolveCovertCarryState(
            OperationSnapshot primaryOp,
            BlackMarketActiveOperationCardSnapshot card,
            BlackMarketRuntimeTruthSurfaceSnapshot runtime,
            BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var text = CombinedContext(primaryOp, card, runtime, payoff).ToLowerInvariant();

            if (ContainsAny(text, "cooling", "cool", "contain", "cleanup", "cover repair"))
            {
                return "cooling";
            }

            if (ContainsAny(text, "hot", "heat", "expose", "backlash"))
            {
                return "hot";
            }

            if (ContainsAny(text, "carry", "covert", "route", "window", "smuggle", "quiet"))
            {
                return "carried";
            }

            if (IsLiveOrHot(card, runtime, payoff))
            {
                return "carried";
            }

            return "staged";
        }

        private static string ResolveHeatState(
            BlackMarketRuntimeTruthSurfaceSnapshot runtime,
            BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var pressureState = (runtime?.PublicBackbonePressure?.State ?? string.Empty).Trim().ToLowerInvariant();
            if (pressureState == "hot" || pressureState == "critical")
            {
                return "Heat rising";
            }

            var severity = (payoff?.Severity ?? string.Empty).Trim().ToLowerInvariant();
            if (severity == "critical" || severity == "high")
            {
                return "Heat rising";
            }

            if (string.Equals((runtime?.RuntimeBand ?? string.Empty).Trim(), "hot", StringComparison.OrdinalIgnoreCase))
            {
                return "Heat rising";
            }

            if (string.Equals((runtime?.RuntimeBand ?? string.Empty).Trim(), "watch", StringComparison.OrdinalIgnoreCase))
            {
                return "Heat watch";
            }

            return "Heat steady";
        }

        private static string ResolveExposureState(
            BlackMarketRuntimeTruthSurfaceSnapshot runtime,
            BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var phase = (payoff?.Phase ?? string.Empty).Trim().ToLowerInvariant();
            if (phase == "backlash_live")
            {
                return "Exposure live";
            }

            if (phase == "cooling")
            {
                return "Exposure cooling";
            }

            var warning = (runtime?.WarningWindow?.State ?? string.Empty).Trim().ToLowerInvariant();
            if (warning == "open" || warning == "live" || warning == "hot")
            {
                return "Exposure live";
            }

            var pressureState = (runtime?.PublicBackbonePressure?.State ?? string.Empty).Trim().ToLowerInvariant();
            if (pressureState == "hostile" || pressureState == "critical")
            {
                return "Exposure live";
            }

            return "Exposure manageable";
        }

        private static bool IsLiveOrHot(
            BlackMarketActiveOperationCardSnapshot card,
            BlackMarketRuntimeTruthSurfaceSnapshot runtime,
            BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var cardState = (card?.State ?? string.Empty).Trim().ToLowerInvariant();
            if (cardState == "live" || cardState == "forming")
            {
                return true;
            }

            var runtimeBand = (runtime?.RuntimeBand ?? string.Empty).Trim().ToLowerInvariant();
            if (runtimeBand == "hot" || runtimeBand == "active")
            {
                return true;
            }

            var phase = (payoff?.Phase ?? string.Empty).Trim().ToLowerInvariant();
            return phase == "payoff_live" || phase == "backlash_live";
        }

        private static string CombinedContext(OperationSnapshot operation)
        {
            return JoinNonBlank(
                operation?.Title,
                operation?.Summary,
                operation?.Detail,
                operation?.WhyNow,
                operation?.Payoff,
                operation?.Risk,
                operation?.FocusLabel,
                operation?.Kind,
                operation?.ResponsePosture);
        }

        private static string CombinedContext(
            OperationSnapshot operation,
            BlackMarketActiveOperationCardSnapshot card,
            BlackMarketRuntimeTruthSurfaceSnapshot runtime,
            BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            return JoinNonBlank(
                operation?.Title,
                operation?.Summary,
                operation?.Detail,
                operation?.WhyNow,
                operation?.Payoff,
                operation?.Risk,
                operation?.FocusLabel,
                operation?.Kind,
                operation?.ResponsePosture,
                card?.Headline,
                card?.Summary,
                card?.OperatorNote,
                card?.Risk,
                card?.Kind,
                card?.State,
                runtime?.Headline,
                runtime?.Detail,
                runtime?.OperatorFrontSummary,
                runtime?.RuntimeBand,
                runtime?.WarningWindow?.Headline,
                runtime?.WarningWindow?.Detail,
                runtime?.ActiveOperation?.Headline,
                runtime?.ActiveOperation?.Detail,
                runtime?.PayoffWindow?.Headline,
                runtime?.PayoffWindow?.Detail,
                runtime?.PublicBackbonePressure?.Headline,
                runtime?.PublicBackbonePressure?.Detail,
                runtime?.PublicBackbonePressure?.RecommendedAction,
                payoff?.Headline,
                payoff?.Detail,
                payoff?.StateReason,
                payoff?.RecommendedAction,
                payoff?.Phase,
                payoff?.Severity,
                payoff?.RecentReceipts?.FirstOrDefault()?.Title,
                payoff?.RecentReceipts?.FirstOrDefault()?.Summary,
                payoff?.RecentReceipts?.FirstOrDefault()?.Detail);
        }

        private static string BuildFrontPreview(IEnumerable<string> names, int maxCount)
        {
            if (names == null)
            {
                return string.Empty;
            }

            var cleaned = names
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (cleaned.Count == 0)
            {
                return string.Empty;
            }

            var shown = cleaned.Take(Math.Max(1, maxCount)).ToList();
            var hidden = Math.Max(0, cleaned.Count - shown.Count);

            var preview = $" • {string.Join(" • ", shown)}";
            if (hidden > 0)
            {
                preview += $" • +{hidden} more";
            }

            return preview;
        }

        private static string JoinNonBlank(params string[] values)
        {
            return string.Join(" ", values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()));
        }

        private static bool ContainsAny(string text, params string[] needles)
        {
            if (string.IsNullOrWhiteSpace(text) || needles == null || needles.Length == 0)
            {
                return false;
            }

            return needles.Any(n => !string.IsNullOrWhiteSpace(n) && text.Contains(n, StringComparison.OrdinalIgnoreCase));
        }

        private static string FirstNonBlank(params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return string.Empty;
        }

        private static string CompactSingleLine(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return string.Join(" ", value.Split(new[] { '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)).Trim();
        }

        private static string HumanizeWords(string raw, string fallback)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return fallback;
            }

            var normalized = raw.Replace("_", " ").Replace("-", " ").Trim();
            if (normalized.Length == 0)
            {
                return fallback;
            }

            return char.ToUpperInvariant(normalized[0]) + normalized.Substring(1);
        }

        private static string FormatMinutes(double minutes)
        {
            if (minutes <= 0) return "now";
            var span = TimeSpan.FromMinutes(minutes);
            return span.TotalHours >= 1 ? span.ToString(@"hh\:mm") : span.ToString(@"mm\:ss");
        }

        private static string FormatRemaining(TimeSpan remaining)
        {
            if (remaining.TotalSeconds <= 0)
            {
                return "now";
            }

            if (remaining.TotalHours >= 1)
            {
                return $"{(int)remaining.TotalHours}h {remaining.Minutes}m";
            }

            if (remaining.TotalMinutes >= 1)
            {
                return $"{(int)remaining.TotalMinutes}m {remaining.Seconds}s";
            }

            return $"{Math.Max(1, remaining.Seconds)}s";
        }
    }
}