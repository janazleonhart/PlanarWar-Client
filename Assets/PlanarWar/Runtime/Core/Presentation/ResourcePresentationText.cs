using PlanarWar.Client.Core.Contracts;
using System.Collections.Generic;

namespace PlanarWar.Client.Core.Presentation
{
    public static class ResourcePresentationText
    {
        public static ResourcePresentationSnapshot DefaultForLane(string lane)
        {
            var isBlackMarket = IsBlackMarketLane(lane);
            return new ResourcePresentationSnapshot
            {
                Food = isBlackMarket ? "Provisions" : "Food",
                Materials = isBlackMarket ? "Supplies" : "Materials",
                Wealth = isBlackMarket ? "Cashflow" : "Wealth",
                Mana = isBlackMarket ? "Arcana" : "Mana",
                Knowledge = isBlackMarket ? "Intel" : "Knowledge",
                Unity = isBlackMarket ? "Loyalty" : "Unity",
            };
        }

        public static string Label(ResourcePresentationSnapshot labels, string key)
        {
            var resolved = labels ?? DefaultForLane(string.Empty);
            return (key ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "food" => resolved.Food,
                "materials" => resolved.Materials,
                "wealth" => resolved.Wealth,
                "mana" => resolved.Mana,
                "knowledge" => resolved.Knowledge,
                "unity" => resolved.Unity,
                _ => key ?? string.Empty,
            };
        }

        public static void AppendResource(List<string> chunks, ResourcePresentationSnapshot labels, string key, double? value, string suffix = "")
        {
            if (value.HasValue)
            {
                chunks.Add($"{Label(labels, key)} {value.Value:0.#}{suffix}");
            }
        }

        public static string Cost(ResourcePresentationSnapshot labels, string key, double? value)
        {
            return value.HasValue ? $"{Label(labels, key)} {value.Value:0.#}" : string.Empty;
        }

        private static bool IsBlackMarketLane(string lane)
        {
            var normalized = (lane ?? string.Empty).Trim().ToLowerInvariant();
            return normalized == "black_market" || normalized == "black market" || normalized == "black-market" || normalized == "blackmarket" || normalized == "shadow";
        }
    }
}
