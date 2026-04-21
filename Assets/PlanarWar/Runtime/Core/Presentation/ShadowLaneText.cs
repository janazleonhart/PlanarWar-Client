using PlanarWar.Client.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlanarWar.Client.Core.Presentation
{
    public static class ShadowLaneText
    {
        public static string BuildGroundedContractValue(OperationSnapshot primaryOp, BlackMarketActiveOperationCardSnapshot card, BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var title = FirstNonBlank(primaryOp?.Title, primaryOp?.FocusLabel, card?.Headline, runtime?.Headline, payoff?.Headline, "No shadow contract surfaced.");
            return $"{title} • {BuildLifecycleLead(primaryOp, card, runtime, payoff)}";
        }

        public static string BuildGroundedContractNote(OperationSnapshot primaryOp, BlackMarketActiveOperationCardSnapshot card, BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            return FirstNonBlank(
                CompactSingleLine(primaryOp?.Summary),
                CompactSingleLine(primaryOp?.WhyNow),
                CompactSingleLine(card?.Summary),
                CompactSingleLine(runtime?.Detail),
                CompactSingleLine(payoff?.Detail),
                "The live shadow seam is grounded on the board and can be worked without inventing fake underworld drama.");
        }

        public static string BuildLifecycleValue(OperationSnapshot primaryOp, BlackMarketActiveOperationCardSnapshot card, BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            return $"{BuildLifecycleLead(primaryOp, card, runtime, payoff)} • {BuildLifecycleTail(primaryOp, card, runtime, payoff)}";
        }

        public static string BuildLifecycleNote(OperationSnapshot primaryOp, BlackMarketActiveOperationCardSnapshot card, BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            return FirstNonBlank(
                CompactSingleLine(card?.OperatorNote),
                CompactSingleLine(primaryOp?.WhyNow),
                CompactSingleLine(payoff?.StateReason),
                CompactSingleLine(runtime?.ActiveOperation?.Detail),
                CompactSingleLine(runtime?.OperatorFrontSummary),
                "Lifecycle stays tied to the live shadow line so the desk can tell whether the route is merely offered, already committed, already answered, or cooling.");
        }

        public static string BuildEffectsValue(OperationSnapshot primaryOp, BlackMarketActiveOperationCardSnapshot card, BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            return $"Receipt chain {ResolveReceiptChainState(primaryOp, card, runtime, payoff)} • Covert carry {ResolveCovertCarryState(primaryOp, card, runtime, payoff)}";
        }

        public static string BuildEffectsNote(OperationSnapshot primaryOp, BlackMarketActiveOperationCardSnapshot card, BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
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

        public static string BuildPressureBendValue(BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            return $"{ResolveHeatState(runtime, payoff)} • {ResolveExposureState(runtime, payoff)}";
        }

        public static string BuildPressureBendNote(OperationSnapshot primaryOp, BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
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
            if (ContainsAny(text, "counterfeit", "script", "ledger", "paper", "receipt", "book")) return "Receipt-chain routing / forged paper";
            if (ContainsAny(text, "cover", "cleanup", "contain", "cool", "wash")) return "Deniable cleanup / route cooling";
            if (ContainsAny(text, "bribe", "pressure", "warning", "heat", "leverage", "exposure")) return "Heat management / quiet leverage";
            if (ContainsAny(text, "covert", "exploit", "cash", "smuggle", "carry", "route")) return "Shadow books / covert cash-out";
            return "Covert carry / quiet pressure";
        }

        public static string HumanizeShadowKind(string kind)
        {
            switch ((kind ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "counterfeit_job": return "Receipt-chain routing";
                case "backbone_pressure": return "Heat pressure";
                case "cover_repair": return "Deniable cleanup";
                case "warning_window": return "Exposure window";
                case "containment": return "Route containment";
                case "bribery": return "Quiet leverage";
                case "exploit": return "Shadow books";
                default: return HumanizeWords(kind, "Shadow seam");
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

            return liveTimers > 0 ? $"{liveTimers} live timer(s) visible; covert cadence is readable." : "Carry cadence unavailable.";
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

            return $"Shadow books: {summary?.AvailableTechs?.Count ?? 0} tech option(s) • fronts {summary?.WorkshopJobs?.Count ?? 0} • {summary?.CityTimers?.Count ?? 0} live timer(s) • {summary?.OpeningOperations?.Count ?? 0} opening line(s).";
        }

        public static string BuildResearchLaneTitle() => "Shadow books";
        public static string BuildResearchLaneCopy() => "Shadow books keep permit memory, forged paper, and quiet leverage readable without pretending the lane is just public paperwork.";
        public static string BuildWorkshopLaneTitle() => "Covert supply";
        public static string BuildWorkshopLaneCopy() => "Covert supply keeps live fronts, ready pickups, and quiet fabrication visible in one place.";
        public static string BuildGrowthLaneTitle() => "Carry lane";
        public static string BuildGrowthLaneCopy() => "Carry lanes keep cadence, staffing, and live heat readable so the Black Market feels like covert logistics instead of a darker civic shell.";

        private static string BuildLifecycleLead(OperationSnapshot primaryOp, BlackMarketActiveOperationCardSnapshot card, BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
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

        private static string BuildLifecycleTail(OperationSnapshot primaryOp, BlackMarketActiveOperationCardSnapshot card, BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
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

        private static string ResolveReceiptChainState(OperationSnapshot primaryOp, BlackMarketActiveOperationCardSnapshot card, BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var text = CombinedContext(primaryOp, card, runtime, payoff).ToLowerInvariant();
            if (ContainsAny(text, "cooling", "cool", "cleanup", "washed")) return "cooling";
            if (ContainsAny(text, "trace", "traced", "audit", "expose", "exposed")) return "traced";
            if (ContainsAny(text, "counterfeit", "receipt", "script", "paper", "ledger", "book")) return "linked";
            if (IsLiveOrHot(card, runtime, payoff)) return "linked";
            return "staged";
        }

        private static string ResolveCovertCarryState(OperationSnapshot primaryOp, BlackMarketActiveOperationCardSnapshot card, BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var text = CombinedContext(primaryOp, card, runtime, payoff).ToLowerInvariant();
            if (ContainsAny(text, "cooling", "cool", "contain", "cleanup", "cover repair")) return "cooling";
            if (ContainsAny(text, "hot", "heat", "expose", "backlash")) return "hot";
            if (ContainsAny(text, "carry", "covert", "route", "window", "smuggle", "quiet")) return "carried";
            if (IsLiveOrHot(card, runtime, payoff)) return "carried";
            return "staged";
        }

        private static string ResolveHeatState(BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var state = (runtime?.PublicBackbonePressure?.State ?? string.Empty).Trim().ToLowerInvariant();
            if (state == "hot" || state == "active" || state == "live") return "Heat rising";
            if (state == "watch") return "Heat building";
            if (state == "cooling") return "Heat cooling";

            var band = (runtime?.RuntimeBand ?? string.Empty).Trim().ToLowerInvariant();
            if (band == "hot" || band == "active") return "Heat rising";
            if (band == "watch") return "Heat building";
            if (string.Equals((payoff?.Severity ?? string.Empty).Trim(), "critical", StringComparison.OrdinalIgnoreCase)) return "Heat rising";
            return "Heat low";
        }

        private static string ResolveExposureState(BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var phase = (payoff?.Phase ?? string.Empty).Trim().ToLowerInvariant();
            if (phase == "backlash_live") return "Exposure live";
            if (phase == "payoff_live") return "Carry open";
            if (phase == "cooling") return "Route cooling";

            var text = CombinedContext(runtime, payoff).ToLowerInvariant();
            if (ContainsAny(text, "expose", "backlash", "trace")) return "Exposure live";
            if (ContainsAny(text, "carry", "route", "covert")) return "Carry open";
            return "Quiet payoff";
        }

        private static bool IsLiveOrHot(BlackMarketActiveOperationCardSnapshot card, BlackMarketRuntimeTruthSurfaceSnapshot runtime, BlackMarketPayoffRecoverySurfaceSnapshot payoff)
        {
            var cardState = (card?.State ?? string.Empty).Trim().ToLowerInvariant();
            if (cardState == "live" || cardState == "forming")
            {
                return true;
            }

            var band = (runtime?.RuntimeBand ?? string.Empty).Trim().ToLowerInvariant();
            if (band == "hot" || band == "active")
            {
                return true;
            }

            var phase = (payoff?.Phase ?? string.Empty).Trim().ToLowerInvariant();
            return phase == "payoff_live" || phase == "backlash_live";
        }

        private static string CombinedContext(params object[] values)
        {
            var chunks = new List<string>();
            foreach (var value in values)
            {
                if (value == null)
                {
                    continue;
                }

                var operation = value as OperationSnapshot;
                if (operation != null)
                {
                    chunks.AddRange(new[] { operation.Title, operation.Summary, operation.Detail, operation.WhyNow, operation.FocusLabel, operation.Payoff, operation.Kind });
                    continue;
                }

                var card = value as BlackMarketActiveOperationCardSnapshot;
                if (card != null)
                {
                    chunks.AddRange(new[] { card.Kind, card.State, card.Headline, card.Summary, card.OperatorNote, card.Risk });
                    continue;
                }

                var runtime = value as BlackMarketRuntimeTruthSurfaceSnapshot;
                if (runtime != null)
                {
                    chunks.AddRange(new[] { runtime.RuntimeBand, runtime.Headline, runtime.Detail, runtime.OperatorFrontSummary, runtime.PublicBackbonePressure?.State, runtime.PublicBackbonePressure?.Headline, runtime.PublicBackbonePressure?.Detail, runtime.PublicBackbonePressure?.RecommendedAction, runtime.ActiveOperation?.State, runtime.ActiveOperation?.Headline, runtime.ActiveOperation?.Detail });
                    continue;
                }

                var payoff = value as BlackMarketPayoffRecoverySurfaceSnapshot;
                if (payoff != null)
                {
                    chunks.AddRange(new[] { payoff.Phase, payoff.Severity, payoff.Headline, payoff.Detail, payoff.StateReason, payoff.RecommendedAction, payoff.RecentReceipts?.FirstOrDefault()?.Title, payoff.RecentReceipts?.FirstOrDefault()?.Summary, payoff.RecentReceipts?.FirstOrDefault()?.Detail });
                    continue;
                }

                chunks.Add(value.ToString());
            }

            return string.Join(" ", chunks.Where(chunk => !string.IsNullOrWhiteSpace(chunk)));
        }

        private static bool ContainsAny(string text, params string[] needles)
        {
            if (string.IsNullOrWhiteSpace(text) || needles == null || needles.Length == 0)
            {
                return false;
            }

            foreach (var needle in needles)
            {
                if (!string.IsNullOrWhiteSpace(needle) && text.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static string CompactSingleLine(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return string.Join(" ", value.Split(new[] { '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries)).Trim();
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

        private static string HumanizeWords(string raw, string fallback)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return fallback;
            }

            var words = raw.Trim().Replace("_", " ").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", words.Select(word => char.ToUpperInvariant(word[0]) + word.Substring(1)));
        }

        private static string FormatRemaining(TimeSpan span)
        {
            if (span <= TimeSpan.Zero)
            {
                return "now";
            }

            return span.ToString(span.TotalHours >= 1 ? @"hh\:mm\:ss" : @"mm\:ss");
        }
    }
}
