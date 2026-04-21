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
