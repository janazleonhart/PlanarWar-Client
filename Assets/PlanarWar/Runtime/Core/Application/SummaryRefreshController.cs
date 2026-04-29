using Newtonsoft.Json.Linq;
using PlanarWar.Client.Core.Contracts;
using PlanarWar.Client.Core.Mapping;
using PlanarWar.Client.Network;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlanarWar.Client.Core.Application
{
    public sealed class SummaryRefreshController
    {
        private readonly PlanarWarApiClient apiClient;
        private readonly SummaryState summaryState;

        public SummaryRefreshController(PlanarWarApiClient apiClient, SummaryState summaryState)
        {
            this.apiClient = apiClient;
            this.summaryState = summaryState;
        }

        public async Task RefreshAsync()
        {
            var raw = await apiClient.FetchSummaryAsync();
            var snapshot = ShellSummarySnapshotMapper.Map(raw?.ToString(Newtonsoft.Json.Formatting.None) ?? "{}");

            List<WorkshopRecipeSnapshot> workshopRecipes = new();
            try
            {
                var recipePayload = await apiClient.FetchWorkshopRecipesAsync();
                workshopRecipes = ParseWorkshopRecipes(recipePayload);
            }
            catch
            {
                workshopRecipes = new List<WorkshopRecipeSnapshot>();
            }

            List<MissionOfferSnapshot> missionOffers = new();
            try
            {
                var missionPayload = await apiClient.FetchMissionOffersAsync();
                missionOffers = ParseMissionOffers(missionPayload);
            }
            catch
            {
                missionOffers = new List<MissionOfferSnapshot>();
            }

            summaryState.Apply(raw, snapshot, workshopRecipes, missionOffers);
        }

        private static List<WorkshopRecipeSnapshot> ParseWorkshopRecipes(JObject payload)
        {
            var recipes = payload?["recipes"] as JArray;
            if (recipes == null) return new List<WorkshopRecipeSnapshot>();

            return recipes
                .OfType<JObject>()
                .Select(r => new WorkshopRecipeSnapshot
                {
                    RecipeId = r["recipeId"]?.ToString()?.Trim() ?? "recipe",
                    Name = r["name"]?.ToString()?.Trim() ?? r["recipeId"]?.ToString()?.Trim() ?? "Recipe",
                    Summary = r["summary"]?.ToString()?.Trim() ?? string.Empty,
                    GearFamily = r["gearFamily"]?.ToString()?.Trim() ?? string.Empty,
                    GearSlot = r["gearSlot"]?.ToString()?.Trim() ?? r["gear_slot"]?.ToString()?.Trim() ?? r["slot"]?.ToString()?.Trim() ?? r["equipmentSlot"]?.ToString()?.Trim() ?? r["equipment_slot"]?.ToString()?.Trim() ?? r["targetSlot"]?.ToString()?.Trim() ?? r["target_slot"]?.ToString()?.Trim() ?? r["template"]?["slot"]?.ToString()?.Trim() ?? string.Empty,
                    OutputItemId = r["outputItemId"]?.ToString()?.Trim() ?? string.Empty,
                    WealthCost = ReadDouble(r["effectiveWealthCost"] ?? r["craftWealthCost"] ?? r["wealthCost"]),
                    ManaCost = ReadDouble(r["manaCost"]),
                    MaterialsCost = ReadDouble(r["craftMaterialsCost"] ?? r["materialsCost"]),
                    CraftMinutes = ReadDouble(r["craftMinutes"]),
                    ResponseTags = (r["responseTags"] as JArray)?.Select(tag => tag?.ToString()?.Trim()).Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList() ?? new List<string>(),
                })
                .Where(r => !string.IsNullOrWhiteSpace(r.RecipeId))
                .ToList();
        }

        private static List<MissionOfferSnapshot> ParseMissionOffers(JObject payload)
        {
            var offers = payload?["missions"] as JArray
                ?? payload?["missionOffers"] as JArray
                ?? payload?["mission_offers"] as JArray
                ?? payload?["offers"] as JArray;
            if (offers == null) return new List<MissionOfferSnapshot>();

            return offers
                .OfType<JObject>()
                .Select(m => new MissionOfferSnapshot
                {
                    Id = m["id"]?.ToString()?.Trim() ?? m["missionId"]?.ToString()?.Trim() ?? "mission",
                    Title = m["title"]?.ToString()?.Trim() ?? m["name"]?.ToString()?.Trim() ?? m["id"]?.ToString()?.Trim() ?? "Mission",
                    Kind = m["kind"]?.ToString()?.Trim() ?? m["missionKind"]?.ToString()?.Trim() ?? string.Empty,
                    RegionId = m["regionId"]?.ToString()?.Trim() ?? m["region_id"]?.ToString()?.Trim() ?? string.Empty,
                    BoardState = m["boardState"]?.ToString()?.Trim() ?? m["board_state"]?.ToString()?.Trim() ?? string.Empty,
                    BoardCategory = m["boardCategory"]?.ToString()?.Trim() ?? m["board_category"]?.ToString()?.Trim() ?? string.Empty,
                    BoardSourceKind = m["boardSourceKind"]?.ToString()?.Trim() ?? m["board_source_kind"]?.ToString()?.Trim() ?? string.Empty,
                    BoardLaneTone = m["boardLaneTone"]?.ToString()?.Trim() ?? m["board_lane_tone"]?.ToString()?.Trim() ?? string.Empty,
                    Difficulty = m["difficulty"]?.ToString()?.Trim() ?? string.Empty,
                    Summary = m["summary"]?.ToString()?.Trim() ?? m["authoredSummary"]?.ToString()?.Trim() ?? m["description"]?.ToString()?.Trim() ?? string.Empty,
                    Payoff = m["payoff"]?.ToString()?.Trim()
                        ?? m["reward"]?.ToString()?.Trim()
                        ?? m["rewards"]?.ToString(Newtonsoft.Json.Formatting.None)?.Trim()
                        ?? m["effect"]?.ToString()?.Trim()
                        ?? m["effects"]?.ToString(Newtonsoft.Json.Formatting.None)?.Trim()
                        ?? m["authoredActionSummary"]?.ToString()?.Trim()
                        ?? string.Empty,
                    Risk = m["risk"]?.ToString()?.Trim() ?? m["riskSummary"]?.ToString()?.Trim() ?? m["risk_summary"]?.ToString()?.Trim() ?? string.Empty,
                    ResponseTags = (m["responseTags"] as JArray)?.Select(tag => tag?.ToString()?.Trim()).Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList()
                        ?? (m["response_tags"] as JArray)?.Select(tag => tag?.ToString()?.Trim()).Where(tag => !string.IsNullOrWhiteSpace(tag)).ToList()
                        ?? new List<string>(),
                })
                .Where(m => !string.IsNullOrWhiteSpace(m.Id))
                .ToList();
        }

        private static double? ReadDouble(JToken token)
        {
            if (token == null) return null;
            return double.TryParse(token.ToString(), out var value) ? value : null;
        }
    }
}
