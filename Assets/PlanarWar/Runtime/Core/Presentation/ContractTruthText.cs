using PlanarWar.Client.Core.Contracts;
using System;

namespace PlanarWar.Client.Core.Presentation
{
    public static class ContractTruthText
    {
        public static string HumanizeLifecycle(string state)
        {
            switch (Normalize(state))
            {
                case "available": return "Available";
                case "committed": return "Committed";
                case "answered": return "Answered";
                case "cooling": return "Cooling";
                default: return "Untracked";
            }
        }

        public static string BuildContractSeamValue(ContractFollowThroughSnapshot followThrough, string fallback)
        {
            if (followThrough == null || string.IsNullOrWhiteSpace(followThrough.ContractTitle))
            {
                return fallback;
            }

            return $"{followThrough.ContractTitle} • {HumanizeLifecycle(followThrough.State)}";
        }

        public static string BuildContractLifecycleValue(ContractFollowThroughSnapshot followThrough, string fallback)
        {
            if (followThrough == null)
            {
                return fallback;
            }

            return $"{HumanizeLifecycle(followThrough.State)} • {HumanizeSourceSurface(followThrough.SourceSurface)}";
        }

        public static string BuildCivicEffectsValue(PublicBackboneContractEffectsSnapshot effects, string fallback)
        {
            if (effects == null)
            {
                return fallback;
            }

            return $"Queue {HumanizeWords(effects.QueueEffect, "unknown")} • Trust {HumanizeWords(effects.TrustEffect, "unknown")} • Services {HumanizeWords(effects.ServiceEffect, "unknown")}";
        }

        public static string BuildShadowEffectsValue(ShadowContractEffectsSnapshot effects, string fallback)
        {
            if (effects == null)
            {
                return fallback;
            }

            return $"Receipt chain {HumanizeWords(effects.ReceiptChainState, "unknown")} • Covert carry {HumanizeWords(effects.CovertCarryState, "unknown")}";
        }

        public static string BuildCivicEffectsNote(PublicBackboneContractEffectsSnapshot effects, ContractFollowThroughSnapshot followThrough, string fallback)
        {
            if (effects != null)
            {
                return FirstNonBlank(effects.Note, fallback);
            }

            if (followThrough != null)
            {
                return FirstNonBlank(
                    followThrough.Note,
                    followThrough.State == "available"
                        ? "The grounded contract is visible, but direct civic effects have not landed yet."
                        : fallback,
                    fallback);
            }

            return fallback;
        }

        public static string BuildShadowEffectsNote(ShadowContractEffectsSnapshot effects, ContractFollowThroughSnapshot followThrough, string fallback)
        {
            if (effects != null)
            {
                return FirstNonBlank(effects.Note, fallback);
            }

            if (followThrough != null)
            {
                return FirstNonBlank(
                    followThrough.Note,
                    followThrough.State == "available"
                        ? "The grounded contract is visible, but bounded shadow effects have not landed yet."
                        : fallback,
                    fallback);
            }

            return fallback;
        }



        public static string BuildCityContractRecoveryBoardValue(CityContractRecoveryBoardSnapshot board, string fallback)
        {
            if (board == null)
            {
                return fallback;
            }

            var count = board.CandidateCount > 0 ? board.CandidateCount : (board.Candidates?.Count ?? 0);
            var state = HumanizeWords(board.State, "watching");
            if (count > 0)
            {
                var lead = board.Candidates != null && board.Candidates.Count > 0 ? board.Candidates[0] : null;
                var leadTitle = FirstNonBlank(lead?.Title, board.Title, "Regional recovery candidate");
                return $"{count} candidate{(count == 1 ? string.Empty : "s")} • {leadTitle} • {state}";
            }

            return $"{FirstNonBlank(board.Title, "Regional recovery board")} • {state}";
        }

        public static string BuildCityContractRecoveryBoardNote(CityContractRecoveryBoardSnapshot board, string fallback)
        {
            if (board == null)
            {
                return fallback;
            }

            var joined = JoinNonBlank(
                board.RecommendedCityDeskAction,
                board.EligibleRegionIds != null && board.EligibleRegionIds.Count > 0
                    ? $"Regions: {HumanizeRegionList(board.EligibleRegionIds)}"
                    : string.Empty,
                board.Candidates != null && board.Candidates.Count > 0
                    ? BuildCityContractRecoveryCandidateNote(board.Candidates[0], string.Empty)
                    : string.Empty);
            return string.IsNullOrWhiteSpace(joined) ? fallback : joined;
        }

        public static string BuildCityContractRecoveryCandidateValue(CityContractRecoveryCandidateSnapshot candidate, string fallback)
        {
            if (candidate == null)
            {
                return fallback;
            }

            var priority = HumanizeWords(candidate.Priority, "watch");
            var desk = HumanizeWords(candidate.Desk, "world consequence watch");
            return $"{FirstNonBlank(candidate.Title, candidate.ActionId, "Recovery candidate")} • {priority} • {desk}";
        }

        public static string BuildCityContractRecoveryCandidateNote(CityContractRecoveryCandidateSnapshot candidate, string fallback)
        {
            if (candidate == null)
            {
                return fallback;
            }

            var resources = BuildCityContractRecoveryResourcesValue(candidate.RequiredResources, string.Empty);
            var receipt = string.IsNullOrWhiteSpace(candidate.NextReceiptFamily) ? string.Empty : $"Next report: {HumanizeWords(candidate.NextReceiptFamily, "watch report")}";
            var regions = candidate.EligibleRegionIds != null && candidate.EligibleRegionIds.Count > 0 ? $"Regions: {HumanizeRegionList(candidate.EligibleRegionIds)}" : string.Empty;
            var note = JoinNonBlank(resources, receipt, regions);
            return string.IsNullOrWhiteSpace(note) ? fallback : note;
        }

        public static string BuildCityContractRecoveryResourcesValue(CityContractRecoveryResourcesRequirementSnapshot resources, string fallback)
        {
            if (resources == null)
            {
                return fallback;
            }

            var state = HumanizeWords(resources.Affordability, "advisory only");
            if (resources.Shortfall != null && HasAnyResource(resources.Shortfall))
            {
                return $"{state} • Shortfall {FormatResources(resources.Shortfall)}";
            }

            if (resources.Required != null && HasAnyResource(resources.Required))
            {
                return $"{state} • Required {FormatResources(resources.Required)}";
            }

            return state;
        }

        public static string HumanizeSourceSurface(string sourceSurface)
        {
            switch (Normalize(sourceSurface))
            {
                case "/api/me/currentoffers":
                case "/api/me.currentoffers":
                    return "Current offers";
                case "/api/me/activemissions":
                case "/api/me.activemissions":
                    return "Active missions";
                case "/api/me/missionreceipts":
                case "/api/me.missionreceipts":
                    return "Mission receipts";
                case "/api/me/worldconsequenceresponsereceipts":
                case "/api/me.worldconsequenceresponsereceipts":
                    return "World-response receipts";
                default:
                    return HumanizeWords(sourceSurface, "Unknown surface");
            }
        }


        private static string JoinNonBlank(params string[] values)
        {
            if (values == null) return string.Empty;
            var parts = new System.Collections.Generic.List<string>();
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    parts.Add(value.Trim());
                }
            }
            return string.Join(" • ", parts);
        }

        private static bool HasAnyResource(ResourceSnapshot resources)
        {
            if (resources == null) return false;
            return (resources.Food ?? 0) > 0
                || (resources.Materials ?? 0) > 0
                || (resources.Wealth ?? 0) > 0
                || (resources.Mana ?? 0) > 0
                || (resources.Knowledge ?? 0) > 0
                || (resources.Unity ?? 0) > 0;
        }

        private static string FormatResources(ResourceSnapshot resources)
        {
            if (resources == null) return string.Empty;
            var parts = new System.Collections.Generic.List<string>();
            AddResourcePart(parts, resources.Food, "food");
            AddResourcePart(parts, resources.Materials, "materials");
            AddResourcePart(parts, resources.Wealth, "wealth");
            AddResourcePart(parts, resources.Mana, "mana");
            AddResourcePart(parts, resources.Knowledge, "knowledge");
            AddResourcePart(parts, resources.Unity, "unity");
            return string.Join(", ", parts);
        }

        private static void AddResourcePart(System.Collections.Generic.List<string> parts, double? value, string label)
        {
            if (!value.HasValue || value.Value <= 0) return;
            parts.Add($"{value.Value:0.#} {label}");
        }

        private static string HumanizeRegionList(System.Collections.Generic.IEnumerable<string> regionIds)
        {
            if (regionIds == null) return string.Empty;
            var parts = new System.Collections.Generic.List<string>();
            foreach (var regionId in regionIds)
            {
                var region = HumanizeWords(regionId, string.Empty);
                if (!string.IsNullOrWhiteSpace(region)) parts.Add(region);
                if (parts.Count >= 4) break;
            }
            return string.Join(", ", parts);
        }

        private static string FirstNonBlank(params string[] values)
        {
            if (values == null) return string.Empty;
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value)) return value.Trim();
            }
            return string.Empty;
        }

        private static string HumanizeWords(string value, string fallback)
        {
            var normalized = Normalize(value);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return fallback;
            }

            return normalized.Replace("_", " ").Replace("-", " ").Trim();
        }

        private static string Normalize(string value)
        {
            return (value ?? string.Empty).Trim().ToLowerInvariant().Replace(" ", string.Empty);
        }
    }
}
