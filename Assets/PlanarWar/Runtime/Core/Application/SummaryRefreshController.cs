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

            summaryState.Apply(raw, snapshot, workshopRecipes);
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

        private static double? ReadDouble(JToken token)
        {
            if (token == null) return null;
            return double.TryParse(token.ToString(), out var value) ? value : null;
        }
    }
}
