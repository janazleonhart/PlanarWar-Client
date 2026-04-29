using PlanarWar.Client.Core;
using PlanarWar.Client.Core.Application;
using PlanarWar.Client.Core.Contracts;
using PlanarWar.Client.Network;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace PlanarWar.Client.UI
{
    public sealed class ClientBootstrap : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PlanarWarWsClient networkClient;
        [SerializeField] private PlanarWarMessageRouter router;
        [SerializeField] private UIDocument uiDocument;

        [Header("HTTP")]
        [SerializeField] private string httpBaseUrl = "";
        [SerializeField] private bool autoFetchSummaryOnStart = true;
        [SerializeField] private bool autoRequestWhereAmIOnConnect = true;
        [SerializeField] private bool autoJoinLobbyOnConnect = true;
        [SerializeField] private bool reconnectWsAfterLogin = true;

        private SessionState sessionState;
        private SummaryState summaryState;
        private PlanarWarApiClient apiClient;

        private AuthSessionController authController;
        private SummaryRefreshController summaryController;
        private WsSessionController wsController;
        private ShellNavigationState navigationState;
        private ClientVersionState versionState;
        private AppShellController appShellController;
        private float nextClockRenderAt;
        private float nextTimedRefreshCheckAt;
        private bool timedRefreshInFlight;
        private DateTime lastResourceTickRefreshRequestedAtUtc = DateTime.MinValue;

        private TextField loginNameField;
        private TextField passwordField;
        private TextField registerDisplayNameField;
        private TextField registerEmailField;
        private TextField registerPasswordField;
        private TextField registerConfirmPasswordField;

        private void Awake()
        {
            networkClient ??= FindFirstObjectByType<PlanarWarWsClient>();
            router ??= FindFirstObjectByType<PlanarWarMessageRouter>();
            uiDocument ??= FindFirstObjectByType<UIDocument>();

            EnsureFallbackCamera();

            sessionState = new SessionState();
            summaryState = new SummaryState();
            navigationState = new ShellNavigationState();
            versionState = new ClientVersionState();

            var resolvedHttpBaseUrl = ResolveHttpBaseUrl();
            sessionState.SetUrls(networkClient != null ? networkClient.ServerUrl : "-", resolvedHttpBaseUrl);
            apiClient = new PlanarWarApiClient(() => sessionState.HttpBaseUrl, () => sessionState.BearerToken);

            authController = new AuthSessionController(sessionState, apiClient, networkClient, reconnectWsAfterLogin);
            summaryController = new SummaryRefreshController(apiClient, summaryState);
            wsController = new WsSessionController(networkClient, router, sessionState, autoRequestWhereAmIOnConnect, autoJoinLobbyOnConnect);

            if (uiDocument != null)
            {
                var root = uiDocument.rootVisualElement;
                BindUi(root);
                appShellController = new AppShellController(
                    root,
                    sessionState,
                    summaryState,
                    navigationState,
                    versionState,
                    HandleStartResearchRequestedAsync,
                    HandleStartWorkshopCraftRequestedAsync,
                    HandleCollectWorkshopRequestedAsync,
                    HandleRecruitHeroRequestedAsync,
                    HandleAcceptHeroRecruitCandidateRequestedAsync,
                    HandleDismissHeroRecruitCandidatesRequestedAsync,
                    HandleConstructBuildingRequestedAsync,
                    HandleUpgradeBuildingRequestedAsync,
                    HandleSwitchBuildingRoutingRequestedAsync,
                    HandleDestroyBuildingRequestedAsync,
                    HandleRemodelBuildingRequestedAsync,
                    HandleCancelActiveBuildRequestedAsync,
                    HandleReinforceArmyRequestedAsync,
                    HandleRenameArmyRequestedAsync,
                    HandleSplitArmyRequestedAsync,
                    HandleMergeArmyRequestedAsync,
                    HandleDisbandArmyRequestedAsync,
                    HandleAssignArmyHoldRequestedAsync,
                    HandleReleaseArmyHoldRequestedAsync,
                    HandleWarfrontAssaultRequestedAsync,
                    HandleGarrisonStrikeRequestedAsync,
                    HandleStartMissionRequestedAsync,
                    HandleCompleteMissionRequestedAsync,
                    HandleReleaseHeroRequestedAsync,
                    HandleEquipHeroFromArmoryRequestedAsync,
                    HandleUnequipHeroToArmoryRequestedAsync,
                    HandleBootstrapCityRequestedAsync,
                    RefreshSummary,
                    () => navigationState.SetActive(ShellScreen.Summary));
            }

            sessionState.Changed += Render;
            summaryState.Changed += Render;
            navigationState.Changed += Render;
        }

        private void Start()
        {
            networkClient?.Connect();
            if (autoFetchSummaryOnStart)
            {
                RefreshSummary();
            }

            Render();
        }

        private void OnDestroy()
        {
            wsController?.Dispose();
            sessionState.Changed -= Render;
            summaryState.Changed -= Render;
            navigationState.Changed -= Render;
        }

        private void Update()
        {
            if (Time.unscaledTime >= nextTimedRefreshCheckAt)
            {
                nextTimedRefreshCheckAt = Time.unscaledTime + 1f;
                MaybeRefreshTimedState();
            }

            if (Time.unscaledTime < nextClockRenderAt)
            {
                return;
            }

            nextClockRenderAt = Time.unscaledTime + 1f;
            Render();
        }

        private void MaybeRefreshTimedState()
        {
            if (timedRefreshInFlight || summaryState == null || !summaryState.IsLoaded || summaryState.IsActionBusy)
            {
                return;
            }

            var snapshot = summaryState.Snapshot;
            var nowUtc = DateTime.UtcNow;

            var heroRecruitment = snapshot.HeroRecruitment;
            var heroScoutingElapsed = heroRecruitment != null
                && string.Equals(heroRecruitment.Status, "scouting", StringComparison.OrdinalIgnoreCase)
                && heroRecruitment.FinishesAtUtc.HasValue
                && heroRecruitment.FinishesAtUtc.Value <= nowUtc;

            var heroCandidateReviewElapsed = heroRecruitment != null
                && string.Equals(heroRecruitment.Status, "candidates_ready", StringComparison.OrdinalIgnoreCase)
                && heroRecruitment.CandidateExpiresAtUtc.HasValue
                && heroRecruitment.CandidateExpiresAtUtc.Value <= nowUtc;

            var armyReinforcement = snapshot.ArmyReinforcement;
            var armyReinforcementElapsed = armyReinforcement != null
                && string.Equals(armyReinforcement.Status, "reinforcing", StringComparison.OrdinalIgnoreCase)
                && armyReinforcement.FinishesAtUtc.HasValue
                && armyReinforcement.FinishesAtUtc.Value <= nowUtc;

            var researchElapsed = (snapshot.ActiveResearches ?? new System.Collections.Generic.List<ResearchSnapshot>())
                .Any(r => r != null && r.FinishesAtUtc.HasValue && r.FinishesAtUtc.Value <= nowUtc);

            var missionElapsed = HasAnyMissionElapsed(snapshot.ActiveMissions, nowUtc);

            var resourceTickElapsed = HasResourceTickElapsed(snapshot.ResourceTickTiming, nowUtc);
            if (resourceTickElapsed && nowUtc - lastResourceTickRefreshRequestedAtUtc < TimeSpan.FromSeconds(5))
            {
                resourceTickElapsed = false;
            }

            if (!heroScoutingElapsed && !heroCandidateReviewElapsed && !armyReinforcementElapsed && !researchElapsed && !missionElapsed && !resourceTickElapsed)
            {
                return;
            }

            if (resourceTickElapsed)
            {
                lastResourceTickRefreshRequestedAtUtc = nowUtc;
            }

            TriggerTimedRefresh();
        }

        private static bool HasResourceTickElapsed(TimerSnapshot timing, DateTime nowUtc)
        {
            var cadence = GetResourceTickCadence(timing);
            var nextTickAtUtc = ResolveResourceTickAnchor(timing, cadence);
            if (!nextTickAtUtc.HasValue)
            {
                return false;
            }

            return nextTickAtUtc.Value <= nowUtc;
        }

        private static bool HasAnyMissionElapsed(System.Collections.Generic.IEnumerable<MissionSnapshot> missions, DateTime nowUtc)
        {
            return (missions ?? Enumerable.Empty<MissionSnapshot>())
                .Any(mission => mission != null && mission.FinishesAtUtc.HasValue && mission.FinishesAtUtc.Value <= nowUtc);
        }

        private static TimeSpan? GetResourceTickCadence(TimerSnapshot timing)
        {
            if (timing == null || !timing.TickMs.HasValue || timing.TickMs <= 0)
            {
                return null;
            }

            return TimeSpan.FromMilliseconds(timing.TickMs.Value);
        }

        private static DateTime? ResolveResourceTickAnchor(TimerSnapshot timing, TimeSpan? cadence)
        {
            if (timing == null)
            {
                return null;
            }

            var anchor = timing.NextTickAtUtc;
            if (!anchor.HasValue && timing.LastTickAtUtc.HasValue && cadence.HasValue)
            {
                anchor = timing.LastTickAtUtc.Value + cadence.Value;
            }

            return anchor;
        }

        private async void TriggerTimedRefresh()
        {
            if (timedRefreshInFlight)
            {
                return;
            }

            timedRefreshInFlight = true;
            try
            {
                await summaryController.RefreshAsync();
            }
            catch (Exception ex)
            {
                summaryState.SetError(ex.Message);
            }
            finally
            {
                timedRefreshInFlight = false;
            }
        }

        private async Task HandleStartResearchRequestedAsync(string techId)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(techId) || summaryState.IsActionBusy)
            {
                return;
            }

            try
            {
                var trimmedTechId = techId.Trim();
                var researchLabel = summaryState.ResolveResearchReceiptLabel(trimmedTechId);
                summaryState.BeginResearchAction(trimmedTechId);
                await apiClient.StartResearchAsync(trimmedTechId);
                summaryState.MarkResearchStartAccepted(trimmedTechId);
                await summaryController.RefreshAsync();
                summaryState.FinishAction(string.IsNullOrWhiteSpace(researchLabel)
                    ? "Research started."
                    : $"Research started: {researchLabel}");
            }
            catch (Exception ex)
            {
                summaryState.ClearRecentResearchStart();
                summaryState.FinishAction($"Research failed: {ex.Message}", failed: true);
            }
        }


        private async Task HandleConstructBuildingRequestedAsync(string kind)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(kind) || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedKind = kind.Trim();
            var kindLabel = summaryState.ResolveBuildingKindReceiptLabel(trimmedKind);
            try
            {
                summaryState.BeginBuildingConstruct(trimmedKind);
                await apiClient.ConstructBuildingAsync(trimmedKind);
                await summaryController.RefreshAsync();
                summaryState.FinishAction(string.IsNullOrWhiteSpace(kindLabel)
                    ? "Construction started."
                    : $"Construction started: {kindLabel}");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Construction failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleUpgradeBuildingRequestedAsync(string buildingId)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(buildingId) || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedBuildingId = buildingId.Trim();
            var buildingLabel = summaryState.ResolveBuildingReceiptLabel(trimmedBuildingId);
            try
            {
                summaryState.BeginBuildingUpgrade(trimmedBuildingId);
                await apiClient.UpgradeBuildingAsync(trimmedBuildingId);
                await summaryController.RefreshAsync();
                summaryState.FinishAction(string.IsNullOrWhiteSpace(buildingLabel)
                    ? "Building upgrade started."
                    : $"Building upgrade started: {buildingLabel}");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Building upgrade failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleSwitchBuildingRoutingRequestedAsync(string buildingId, string routingPreference)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(buildingId) || string.IsNullOrWhiteSpace(routingPreference) || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedBuildingId = buildingId.Trim();
            var trimmedRoutingPreference = routingPreference.Trim();
            var buildingLabel = summaryState.ResolveBuildingReceiptLabel(trimmedBuildingId);
            var routingLabel = SummaryState.ResolveBuildingRoutingReceiptLabel(trimmedRoutingPreference);
            try
            {
                summaryState.BeginBuildingRouting(trimmedBuildingId, trimmedRoutingPreference);
                await apiClient.SetBuildingRoutingPreferenceAsync(trimmedBuildingId, trimmedRoutingPreference);
                await summaryController.RefreshAsync();
                summaryState.ClearBuildingConfirm();
                summaryState.FinishAction(string.IsNullOrWhiteSpace(buildingLabel)
                    ? $"Building routing switched to {routingLabel}"
                    : $"{buildingLabel} routing switched to {routingLabel}");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Building routing failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleDestroyBuildingRequestedAsync(string buildingId)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(buildingId) || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedBuildingId = buildingId.Trim();
            var buildingLabel = summaryState.ResolveBuildingReceiptLabel(trimmedBuildingId);
            var confirm = summaryState.GetPendingBuildingConfirmToken("destroy", trimmedBuildingId);
            try
            {
                summaryState.BeginBuildingDestroy(trimmedBuildingId, !string.IsNullOrWhiteSpace(confirm));
                await apiClient.DestroyBuildingAsync(trimmedBuildingId, confirm);
                await summaryController.RefreshAsync();
                summaryState.ClearBuildingConfirm();
                summaryState.FinishAction(string.IsNullOrWhiteSpace(buildingLabel)
                    ? "Building demolished."
                    : $"Building demolished: {buildingLabel}");
            }
            catch (PlanarWarApiException ex) when (TryCaptureBuildingConfirm("destroy", ex, trimmedBuildingId))
            {
                summaryState.FinishAction(string.IsNullOrWhiteSpace(buildingLabel)
                    ? $"Confirm demolition required: {CleanApiError(ex)}"
                    : $"Confirm demolition required for {buildingLabel}: {CleanApiError(ex)}");
            }
            catch (Exception ex)
            {
                summaryState.ClearBuildingConfirm();
                summaryState.FinishAction($"Building demolition failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleRemodelBuildingRequestedAsync(string buildingId, string targetKind)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(buildingId) || string.IsNullOrWhiteSpace(targetKind) || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedBuildingId = buildingId.Trim();
            var trimmedTargetKind = targetKind.Trim();
            var buildingLabel = summaryState.ResolveBuildingReceiptLabel(trimmedBuildingId);
            var targetKindLabel = summaryState.ResolveBuildingKindReceiptLabel(trimmedTargetKind);
            var confirm = summaryState.GetPendingBuildingConfirmToken("remodel", trimmedBuildingId, trimmedTargetKind);
            try
            {
                summaryState.BeginBuildingRemodel(trimmedBuildingId, trimmedTargetKind, !string.IsNullOrWhiteSpace(confirm));
                await apiClient.RemodelBuildingAsync(trimmedBuildingId, trimmedTargetKind, confirm);
                await summaryController.RefreshAsync();
                summaryState.ClearBuildingConfirm();
                summaryState.FinishAction(string.IsNullOrWhiteSpace(buildingLabel)
                    ? $"Building remodel started: {targetKindLabel}"
                    : $"Building remodel started: {buildingLabel} -> {targetKindLabel}");
            }
            catch (PlanarWarApiException ex) when (TryCaptureBuildingConfirm("remodel", ex, trimmedBuildingId, trimmedTargetKind))
            {
                summaryState.FinishAction(string.IsNullOrWhiteSpace(buildingLabel)
                    ? $"Confirm remodel required: {CleanApiError(ex)}"
                    : $"Confirm remodel required for {buildingLabel}: {CleanApiError(ex)}");
            }
            catch (Exception ex)
            {
                summaryState.ClearBuildingConfirm();
                summaryState.FinishAction($"Building remodel failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleCancelActiveBuildRequestedAsync(string activeBuildId)
        {
            if (summaryState == null || apiClient == null || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedActiveBuildId = activeBuildId?.Trim() ?? string.Empty;
            var activeBuildLabel = summaryState.ResolveActiveBuildReceiptLabel(trimmedActiveBuildId);
            var confirm = summaryState.GetPendingBuildingConfirmToken("cancel_build", activeBuildId: trimmedActiveBuildId);
            try
            {
                summaryState.BeginActiveBuildCancel(trimmedActiveBuildId, !string.IsNullOrWhiteSpace(confirm));
                await apiClient.CancelActiveBuildAsync(trimmedActiveBuildId, confirm);
                await summaryController.RefreshAsync();
                summaryState.ClearBuildingConfirm();
                summaryState.FinishAction(string.IsNullOrWhiteSpace(activeBuildLabel) ? "Active building project canceled." : $"Active building project canceled: {activeBuildLabel}");
            }
            catch (PlanarWarApiException ex) when (TryCaptureBuildingConfirm("cancel_build", ex, activeBuildId: trimmedActiveBuildId))
            {
                summaryState.FinishAction(string.IsNullOrWhiteSpace(activeBuildLabel)
                    ? $"Confirm cancellation required: {CleanApiError(ex)}"
                    : $"Confirm cancellation required for {activeBuildLabel}: {CleanApiError(ex)}");
            }
            catch (Exception ex)
            {
                summaryState.ClearBuildingConfirm();
                summaryState.FinishAction($"Building cancellation failed: {ex.Message}", failed: true);
            }
        }

        private bool TryCaptureBuildingConfirm(string action, PlanarWarApiException ex, string buildingId = null, string targetKind = null, string activeBuildId = null)
        {
            var token = ex?.ConfirmToken;
            if (string.IsNullOrWhiteSpace(token))
            {
                return false;
            }

            summaryState.MarkBuildingConfirmRequired(action, token, buildingId, targetKind, activeBuildId);
            return true;
        }

        private static string CleanApiError(PlanarWarApiException ex)
        {
            return string.IsNullOrWhiteSpace(ex?.ApiError) ? ex?.Message ?? "Confirm token required." : ex.ApiError.Trim();
        }


        private string ResolveWorkshopRecipeLabel(string recipeId)
        {
            var trimmedRecipeId = recipeId?.Trim() ?? string.Empty;
            var recipe = summaryState?.WorkshopRecipes?.FirstOrDefault(candidate => candidate != null
                && string.Equals(candidate.RecipeId, trimmedRecipeId, StringComparison.OrdinalIgnoreCase));

            return FirstCleanWorkshopLabel(recipe?.Name, recipe?.OutputItemId, trimmedRecipeId, "workshop recipe");
        }

        private string ResolveWorkshopJobLabel(string jobId)
        {
            var trimmedJobId = jobId?.Trim() ?? string.Empty;
            var job = summaryState?.Snapshot?.WorkshopJobs?.FirstOrDefault(candidate => candidate != null
                && string.Equals(candidate.Id, trimmedJobId, StringComparison.OrdinalIgnoreCase));
            var recipe = summaryState?.WorkshopRecipes?.FirstOrDefault(candidate => candidate != null
                    && !string.IsNullOrWhiteSpace(job?.RecipeId)
                    && string.Equals(candidate.RecipeId, job.RecipeId, StringComparison.OrdinalIgnoreCase))
                ?? summaryState?.WorkshopRecipes?.FirstOrDefault(candidate => candidate != null
                    && !string.IsNullOrWhiteSpace(job?.OutputItemId)
                    && string.Equals(candidate.OutputItemId, job.OutputItemId, StringComparison.OrdinalIgnoreCase));

            return FirstCleanWorkshopLabel(job?.OutputName, recipe?.Name, job?.RecipeId, job?.OutputItemId, job?.AttachmentKind, trimmedJobId, "workshop job");
        }

        private static string FirstCleanWorkshopLabel(params string[] values)
        {
            foreach (var raw in values)
            {
                var cleaned = CleanWorkshopLabel(raw);
                if (!string.IsNullOrWhiteSpace(cleaned))
                {
                    return cleaned;
                }
            }

            return "workshop item";
        }

        private static string CleanWorkshopLabel(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            var value = raw.Trim();
            if (LooksLikeWorkshopId(value))
            {
                return HumanizeKey(value);
            }

            return value;
        }

        private static bool LooksLikeWorkshopId(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return false;
            var value = raw.Trim();
            return value.StartsWith("workshop_", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("recipe_", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("item_", StringComparison.OrdinalIgnoreCase)
                || value.StartsWith("gear_", StringComparison.OrdinalIgnoreCase)
                || value.IndexOf('_') >= 0;
        }

        private static string HumanizeKey(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var cleaned = raw.Replace('_', ' ').Trim();
            return string.Join(" ", cleaned.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpperInvariant(part[0]) + (part.Length > 1 ? part.Substring(1) : string.Empty)));
        }

        private async Task HandleStartWorkshopCraftRequestedAsync(string recipeId)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(recipeId) || summaryState.IsActionBusy)
            {
                return;
            }

            try
            {
                var craftLabel = ResolveWorkshopRecipeLabel(recipeId);
                summaryState.BeginWorkshopCraft(recipeId);
                await apiClient.StartWorkshopCraftAsync(recipeId.Trim());
                await summaryController.RefreshAsync();
                summaryState.FinishAction($"Workshop craft started: {craftLabel}");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Workshop craft failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleCollectWorkshopRequestedAsync(string jobId)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(jobId) || summaryState.IsActionBusy)
            {
                return;
            }

            try
            {
                var collectLabel = ResolveWorkshopJobLabel(jobId);
                summaryState.BeginWorkshopCollect(jobId);
                await apiClient.CollectWorkshopAsync(jobId.Trim());
                await summaryController.RefreshAsync();
                summaryState.FinishAction($"Workshop collect complete: {collectLabel}");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Workshop collect failed: {ex.Message}", failed: true);
            }
        }



        private async Task HandleRecruitHeroRequestedAsync(string role)
        {
            if (summaryState == null || apiClient == null || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedRole = role?.Trim() ?? string.Empty;
            var terms = ResolveHeroActionTerms();

            try
            {
                summaryState.BeginHeroRecruit(trimmedRole);
                var response = await apiClient.RecruitHeroAsync(trimmedRole);
                var action = terms.IsOperative ? "Operative scouting started" : "Hero recruitment scouting started";
                var receipt = SummaryState.FormatHeroActionReceipt(response?.ToString(), action, trimmedRole, terms.SubjectNoun);
                var title = SummaryState.ExtractHeroActionTitle(response?.ToString(), action, terms.SubjectNoun);
                await summaryController.RefreshAsync();
                summaryState.FinishHeroActionReceipt(action, receipt, title);
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"{terms.RecruitmentFamily} failed: {ex.Message}", failed: true);
            }
        }


        private async Task HandleAcceptHeroRecruitCandidateRequestedAsync(string candidateId)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(candidateId) || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedCandidateId = candidateId.Trim();
            var terms = ResolveHeroActionTerms();
            try
            {
                summaryState.BeginHeroRecruitAccept(trimmedCandidateId);
                var response = await apiClient.AcceptHeroRecruitCandidateAsync(trimmedCandidateId);
                var action = terms.IsOperative ? "Contact recruited" : "Hero candidate accepted";
                var receipt = SummaryState.FormatHeroActionReceipt(response?.ToString(), action, trimmedCandidateId, terms.SubjectNoun);
                var title = SummaryState.ExtractHeroActionTitle(response?.ToString(), action, terms.SubjectNoun);
                await summaryController.RefreshAsync();
                summaryState.FinishHeroActionReceipt(action, receipt, title);
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"{terms.AcceptFailureLabel} failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleDismissHeroRecruitCandidatesRequestedAsync()
        {
            if (summaryState == null || apiClient == null || summaryState.IsActionBusy)
            {
                return;
            }

            var terms = ResolveHeroActionTerms();
            try
            {
                summaryState.BeginHeroRecruitDismiss();
                var response = await apiClient.DismissHeroRecruitCandidatesAsync();
                var action = terms.IsOperative ? "Contact slate dismissed" : "Candidate slate dismissed";
                var receipt = SummaryState.FormatHeroActionReceipt(response?.ToString(), action, string.Empty, terms.SubjectNoun);
                var title = SummaryState.ExtractHeroActionTitle(response?.ToString(), action, terms.SubjectNoun);
                await summaryController.RefreshAsync();
                summaryState.FinishHeroActionReceipt(action, receipt, title);
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"{terms.DismissFailureLabel} failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleReleaseHeroRequestedAsync(string heroId)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(heroId) || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedHeroId = heroId.Trim();
            var terms = ResolveHeroActionTerms();
            try
            {
                summaryState.BeginHeroRelease(trimmedHeroId);
                var response = await apiClient.ReleaseHeroAsync(trimmedHeroId);
                var action = terms.IsOperative ? "Operative retired" : "Hero released";
                var receipt = SummaryState.FormatHeroActionReceipt(response?.ToString(), action, trimmedHeroId, terms.SubjectNoun);
                var title = SummaryState.ExtractHeroActionTitle(response?.ToString(), action, terms.SubjectNoun);
                await summaryController.RefreshAsync();
                summaryState.FinishHeroActionReceipt(action, receipt, title);
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"{terms.ReleaseFailureLabel} failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleEquipHeroFromArmoryRequestedAsync(string heroId, int armorySlotIndex)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(heroId) || armorySlotIndex < 0 || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedHeroId = heroId.Trim();
            var terms = ResolveHeroActionTerms();
            try
            {
                summaryState.BeginHeroEquipFromArmory(trimmedHeroId, armorySlotIndex);
                var response = await apiClient.EquipHeroFromArmoryAsync(trimmedHeroId, armorySlotIndex);
                var action = terms.IsOperative ? "Operative gear equipped" : "Hero gear equipped";
                var receipt = SummaryState.FormatHeroActionReceipt(response?.ToString(), action, $"{trimmedHeroId}:slot_{armorySlotIndex}", terms.SubjectNoun);
                var title = SummaryState.ExtractHeroActionTitle(response?.ToString(), action, terms.SubjectNoun);
                await summaryController.RefreshAsync();
                summaryState.FinishHeroActionReceipt(action, receipt, title);
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"{terms.SubjectNoun} gear equip failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleUnequipHeroToArmoryRequestedAsync(string heroId, string slot)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(heroId) || string.IsNullOrWhiteSpace(slot) || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedHeroId = heroId.Trim();
            var trimmedSlot = slot.Trim();
            var terms = ResolveHeroActionTerms();
            try
            {
                summaryState.BeginHeroUnequipToArmory(trimmedHeroId, trimmedSlot);
                var response = await apiClient.UnequipHeroToArmoryAsync(trimmedHeroId, trimmedSlot);
                var action = terms.IsOperative ? "Operative gear returned" : "Hero gear returned";
                var receipt = SummaryState.FormatHeroActionReceipt(response?.ToString(), action, $"{trimmedHeroId}:{trimmedSlot}", terms.SubjectNoun);
                var title = SummaryState.ExtractHeroActionTitle(response?.ToString(), action, terms.SubjectNoun);
                await summaryController.RefreshAsync();
                summaryState.FinishHeroActionReceipt(action, receipt, title);
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"{terms.SubjectNoun} gear return failed: {ex.Message}", failed: true);
            }
        }

        private HeroActionTerms ResolveHeroActionTerms()
        {
            var isOperative = string.Equals(summaryState?.Snapshot?.City?.SettlementLane, "black_market", StringComparison.OrdinalIgnoreCase)
                || string.Equals(summaryState?.Snapshot?.City?.SettlementLaneLabel, "Black Market", StringComparison.OrdinalIgnoreCase);

            return isOperative
                ? new HeroActionTerms(true, "Operative", "operative scouting", "contact recruitment", "contact dismiss", "operative retire")
                : new HeroActionTerms(false, "Hero", "hero recruitment", "candidate accept", "candidate dismiss", "hero release");
        }

        private readonly struct HeroActionTerms
        {
            public HeroActionTerms(bool isOperative, string subjectNoun, string recruitmentFamily, string acceptFailureLabel, string dismissFailureLabel, string releaseFailureLabel)
            {
                IsOperative = isOperative;
                SubjectNoun = subjectNoun;
                RecruitmentFamily = recruitmentFamily;
                AcceptFailureLabel = acceptFailureLabel;
                DismissFailureLabel = dismissFailureLabel;
                ReleaseFailureLabel = releaseFailureLabel;
            }

            public bool IsOperative { get; }
            public string SubjectNoun { get; }
            public string RecruitmentFamily { get; }
            public string AcceptFailureLabel { get; }
            public string DismissFailureLabel { get; }
            public string ReleaseFailureLabel { get; }
        }

        private async Task HandleStartMissionRequestedAsync(string missionId, string armyId, string heroId, string responsePosture)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(missionId) || summaryState.IsActionBusy)
            {
                return;
            }

            try
            {
                var trimmedMissionId = missionId.Trim();
                var missionLabel = summaryState.ResolveMissionOfferReceiptLabel(trimmedMissionId);
                summaryState.BeginMissionStartAction(trimmedMissionId);
                await apiClient.StartMissionAsync(trimmedMissionId, armyId, heroId, responsePosture);
                summaryState.FinishAction(string.IsNullOrWhiteSpace(missionLabel)
                    ? "Mission started."
                    : $"Mission started: {missionLabel}");
                await summaryController.RefreshAsync();
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Mission start failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleCompleteMissionRequestedAsync(string instanceId)
        {
            if (summaryState == null || apiClient == null || string.IsNullOrWhiteSpace(instanceId) || summaryState.IsActionBusy)
            {
                return;
            }

            try
            {
                var trimmedInstanceId = instanceId.Trim();
                var missionLabel = summaryState.ResolveMissionInstanceReceiptLabel(trimmedInstanceId);
                summaryState.BeginMissionCompleteAction(trimmedInstanceId);
                var response = await apiClient.CompleteMissionAsync(trimmedInstanceId);
                var responseText = response?.ToString();
                var receipt = SummaryState.FormatMissionCompletionReceipt(responseText, missionLabel);
                var title = SummaryState.ExtractMissionCompletionTitle(responseText);
                if (string.IsNullOrWhiteSpace(title))
                {
                    title = missionLabel;
                }

                summaryState.FinishMissionCompletion(trimmedInstanceId, receipt, title);
                await summaryController.RefreshAsync();
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Mission complete failed: {ex.Message}", failed: true);
            }
        }


        private async Task HandleReinforceArmyRequestedAsync(string armyId)
        {
            if (summaryState == null || apiClient == null || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedArmyId = armyId?.Trim() ?? string.Empty;
            var armyLabel = summaryState.ResolveArmyReceiptLabel(trimmedArmyId);

            try
            {
                summaryState.BeginArmyReinforcement(trimmedArmyId);
                await apiClient.ReinforceArmyAsync(trimmedArmyId);
                await summaryController.RefreshAsync();
                summaryState.FinishAction(string.IsNullOrWhiteSpace(armyLabel)
                    ? "Cell reinforcement started."
                    : $"Cell reinforcement started: {armyLabel}");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Cell reinforcement failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleRenameArmyRequestedAsync(string armyId, string requestedName)
        {
            if (summaryState == null || apiClient == null || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedArmyId = armyId?.Trim() ?? string.Empty;
            var trimmedName = requestedName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmedArmyId) || string.IsNullOrWhiteSpace(trimmedName))
            {
                return;
            }

            try
            {
                summaryState.BeginArmyRename(trimmedArmyId, trimmedName);
                await apiClient.RenameArmyAsync(trimmedArmyId, trimmedName);
                await summaryController.RefreshAsync();
                summaryState.FinishAction($"Formation renamed: {trimmedName}");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Formation rename failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleSplitArmyRequestedAsync(string armyId, int requestedSize, string requestedName)
        {
            if (summaryState == null || apiClient == null || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedArmyId = armyId?.Trim() ?? string.Empty;
            var trimmedName = requestedName?.Trim() ?? string.Empty;
            var armyLabel = summaryState.ResolveArmyReceiptLabel(trimmedArmyId);
            if (string.IsNullOrWhiteSpace(trimmedArmyId) || requestedSize <= 0)
            {
                return;
            }

            try
            {
                summaryState.BeginArmySplit(trimmedArmyId, requestedSize, trimmedName);
                await apiClient.SplitArmyAsync(trimmedArmyId, requestedSize, trimmedName);
                await summaryController.RefreshAsync();
                summaryState.FinishAction(string.IsNullOrWhiteSpace(trimmedName)
                    ? (string.IsNullOrWhiteSpace(armyLabel)
                        ? $"Formation split: {requestedSize} troops."
                        : $"Formation split from {armyLabel}: {requestedSize} troops.")
                    : $"Formation split into {trimmedName}: {requestedSize} troops.");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Formation split failed: {ex.Message}", failed: true);
            }
        }


        private async Task HandleMergeArmyRequestedAsync(string sourceArmyId, string targetArmyId)
        {
            if (summaryState == null || apiClient == null || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedSourceArmyId = sourceArmyId?.Trim() ?? string.Empty;
            var trimmedTargetArmyId = targetArmyId?.Trim() ?? string.Empty;
            var targetArmyLabel = summaryState.ResolveArmyReceiptLabel(trimmedTargetArmyId);
            if (string.IsNullOrWhiteSpace(trimmedSourceArmyId) || string.IsNullOrWhiteSpace(trimmedTargetArmyId))
            {
                return;
            }

            try
            {
                summaryState.BeginArmyMerge(trimmedSourceArmyId, trimmedTargetArmyId);
                await apiClient.MergeArmyAsync(trimmedSourceArmyId, trimmedTargetArmyId);
                await summaryController.RefreshAsync();
                summaryState.FinishAction(string.IsNullOrWhiteSpace(targetArmyLabel)
                    ? "Formation merged."
                    : $"Formation merged into {targetArmyLabel}.");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Formation merge failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleDisbandArmyRequestedAsync(string armyId)
        {
            if (summaryState == null || apiClient == null || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedArmyId = armyId?.Trim() ?? string.Empty;
            var armyLabel = summaryState.ResolveArmyReceiptLabel(trimmedArmyId);
            if (string.IsNullOrWhiteSpace(trimmedArmyId))
            {
                return;
            }

            try
            {
                summaryState.BeginArmyDisband(trimmedArmyId);
                await apiClient.DisbandArmyAsync(trimmedArmyId);
                await summaryController.RefreshAsync();
                summaryState.FinishAction(string.IsNullOrWhiteSpace(armyLabel)
                    ? "Formation disbanded."
                    : $"Formation disbanded: {armyLabel}.");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Formation disband failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleAssignArmyHoldRequestedAsync(string armyId, string regionId, string posture)
        {
            if (summaryState == null || apiClient == null || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedArmyId = armyId?.Trim() ?? string.Empty;
            var trimmedRegionId = regionId?.Trim() ?? string.Empty;
            var trimmedPosture = posture?.Trim() ?? string.Empty;
            var armyLabel = summaryState.ResolveArmyReceiptLabel(trimmedArmyId);
            var regionLabel = summaryState.ResolveRegionReceiptLabel(trimmedRegionId);
            var postureLabel = SummaryState.ResolvePostureReceiptLabel(trimmedPosture);
            if (string.IsNullOrWhiteSpace(trimmedArmyId) || string.IsNullOrWhiteSpace(trimmedRegionId))
            {
                return;
            }

            try
            {
                summaryState.BeginArmyHoldAssign(trimmedArmyId, trimmedRegionId, trimmedPosture);
                await apiClient.AssignArmyHoldAsync(trimmedArmyId, trimmedRegionId, trimmedPosture);
                await summaryController.RefreshAsync();
                summaryState.FinishAction($"Regional hold assigned: {regionLabel}."
                    + (string.IsNullOrWhiteSpace(armyLabel) ? string.Empty : $" Formation {armyLabel}.")
                    + (string.IsNullOrWhiteSpace(postureLabel) ? string.Empty : $" Posture {postureLabel}."));
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Regional hold failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleReleaseArmyHoldRequestedAsync(string armyId)
        {
            if (summaryState == null || apiClient == null || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedArmyId = armyId?.Trim() ?? string.Empty;
            var armyLabel = summaryState.ResolveArmyReceiptLabel(trimmedArmyId);
            if (string.IsNullOrWhiteSpace(trimmedArmyId))
            {
                return;
            }

            try
            {
                summaryState.BeginArmyHoldRelease(trimmedArmyId);
                await apiClient.ReleaseArmyHoldAsync(trimmedArmyId);
                await summaryController.RefreshAsync();
                summaryState.FinishAction(string.IsNullOrWhiteSpace(armyLabel)
                    ? "Regional hold released."
                    : $"Regional hold released: {armyLabel}.");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Release hold failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleWarfrontAssaultRequestedAsync(string regionId, string armyId, string heroId)
        {
            if (summaryState == null || apiClient == null || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedRegionId = regionId?.Trim() ?? string.Empty;
            var trimmedArmyId = armyId?.Trim() ?? string.Empty;
            var trimmedHeroId = heroId?.Trim() ?? string.Empty;
            var regionLabel = summaryState.ResolveRegionReceiptLabel(trimmedRegionId);
            var armyLabel = summaryState.ResolveArmyReceiptLabel(trimmedArmyId);
            var heroLabel = summaryState.ResolveHeroReceiptLabel(trimmedHeroId);
            if (string.IsNullOrWhiteSpace(trimmedRegionId) || string.IsNullOrWhiteSpace(trimmedArmyId))
            {
                return;
            }

            try
            {
                summaryState.BeginFrontlineDispatch("pressure deployment", trimmedRegionId, trimmedArmyId, trimmedHeroId);
                await apiClient.StartWarfrontAssaultAsync(trimmedRegionId, trimmedArmyId, trimmedHeroId);
                await summaryController.RefreshAsync();
                summaryState.FinishAction(string.IsNullOrWhiteSpace(heroLabel)
                    ? $"Pressure deployment opened for {regionLabel} with {armyLabel}."
                    : $"Pressure deployment opened for {regionLabel} with {armyLabel} under {heroLabel}.");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Pressure deployment failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleGarrisonStrikeRequestedAsync(string regionId, string armyId, string heroId)
        {
            if (summaryState == null || apiClient == null || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedRegionId = regionId?.Trim() ?? string.Empty;
            var trimmedArmyId = armyId?.Trim() ?? string.Empty;
            var trimmedHeroId = heroId?.Trim() ?? string.Empty;
            var regionLabel = summaryState.ResolveRegionReceiptLabel(trimmedRegionId);
            var armyLabel = summaryState.ResolveArmyReceiptLabel(trimmedArmyId);
            var heroLabel = summaryState.ResolveHeroReceiptLabel(trimmedHeroId);
            if (string.IsNullOrWhiteSpace(trimmedRegionId) || string.IsNullOrWhiteSpace(trimmedArmyId))
            {
                return;
            }

            try
            {
                summaryState.BeginFrontlineDispatch("disruption action", trimmedRegionId, trimmedArmyId, trimmedHeroId);
                await apiClient.StartGarrisonStrikeAsync(trimmedRegionId, trimmedArmyId, trimmedHeroId);
                await summaryController.RefreshAsync();
                summaryState.FinishAction(string.IsNullOrWhiteSpace(heroLabel)
                    ? $"Disruption action opened for {regionLabel} with {armyLabel}."
                    : $"Disruption action opened for {regionLabel} with {armyLabel} under {heroLabel}.");
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"Disruption action failed: {ex.Message}", failed: true);
            }
        }

        private async Task HandleBootstrapCityRequestedAsync(string name, string settlementLane)
        {
            if (summaryState == null || apiClient == null || summaryState.IsActionBusy)
            {
                return;
            }

            var trimmedLane = string.IsNullOrWhiteSpace(settlementLane) ? "city" : settlementLane.Trim();
            var trimmedName = name?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmedName) && summaryState.Snapshot != null && !string.IsNullOrWhiteSpace(summaryState.Snapshot.SuggestedCityName))
            {
                trimmedName = summaryState.Snapshot.SuggestedCityName.Trim();
            }

            if (string.IsNullOrWhiteSpace(trimmedName))
            {
                summaryState.FinishAction("Choose a settlement name before founding.", failed: true);
                return;
            }

            var laneLabel = ResolveSettlementLaneLabel(trimmedLane);
            try
            {
                summaryState.BeginSettlementBootstrap(trimmedName, trimmedLane);
                await apiClient.BootstrapCityAsync(trimmedName, trimmedLane);
                await summaryController.RefreshAsync();
                summaryState.FinishAction($"Founded {laneLabel}: {trimmedName}.");
            }
            catch (PlanarWarApiException ex)
            {
                summaryState.FinishAction(BuildSettlementBootstrapFailureMessage(laneLabel, trimmedName, ex), failed: true);
            }
            catch (Exception ex)
            {
                summaryState.FinishAction($"{laneLabel} founding failed: {ex.Message}", failed: true);
            }
        }

        private static string BuildSettlementBootstrapFailureMessage(string laneLabel, string settlementName, PlanarWarApiException ex)
        {
            var label = string.IsNullOrWhiteSpace(laneLabel) ? "Settlement" : laneLabel.Trim();
            var name = settlementName?.Trim() ?? string.Empty;
            var subject = string.IsNullOrWhiteSpace(name) ? "That settlement name" : $"\"{name}\"";
            var apiError = ex?.ApiError?.Trim() ?? string.Empty;

            if (string.Equals(apiError, "city_name_taken", StringComparison.OrdinalIgnoreCase))
            {
                return $"{label} founding failed: {subject} is already taken. Choose another settlement name.";
            }

            if (string.Equals(apiError, "city_exists", StringComparison.OrdinalIgnoreCase))
            {
                return $"{label} founding failed: this account already has a settlement. Refresh summary to load it.";
            }

            if (string.Equals(apiError, "city_name_required", StringComparison.OrdinalIgnoreCase))
            {
                return $"{label} founding failed: choose a settlement name first.";
            }

            if (string.Equals(apiError, "city_name_too_short", StringComparison.OrdinalIgnoreCase))
            {
                return $"{label} founding failed: {subject} is too short. Use at least 3 characters.";
            }

            if (string.Equals(apiError, "city_name_too_long", StringComparison.OrdinalIgnoreCase))
            {
                return $"{label} founding failed: {subject} is too long. Use 24 characters or fewer.";
            }

            if (string.Equals(apiError, "city_name_invalid_chars", StringComparison.OrdinalIgnoreCase))
            {
                return $"{label} founding failed: {subject} has unsupported characters.";
            }

            if (string.Equals(apiError, "city_name_reserved", StringComparison.OrdinalIgnoreCase)
                || string.Equals(apiError, "city_name_blocked", StringComparison.OrdinalIgnoreCase))
            {
                return $"{label} founding failed: {subject} is reserved or blocked. Choose another settlement name.";
            }

            if (string.Equals(apiError, "auth_required", StringComparison.OrdinalIgnoreCase))
            {
                return $"{label} founding failed: sign in again before founding a settlement.";
            }

            var detail = CleanApiError(ex);
            return string.IsNullOrWhiteSpace(detail)
                ? $"{label} founding failed. Choose another settlement name or refresh summary."
                : $"{label} founding failed: {detail}";
        }

        private static string ResolveSettlementLaneLabel(string settlementLane)
        {
            var normalized = (settlementLane ?? string.Empty).Trim().Replace("-", "_").Replace(" ", "_").ToLowerInvariant();
            if (normalized == "black_market" || normalized == "blackmarket")
            {
                return "Black Market";
            }

            return "City";
        }

        private static void EnsureFallbackCamera()
        {
            if (Camera.allCamerasCount > 0)
            {
                return;
            }

            var cameraObject = new GameObject("FallbackUICamera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.03f, 0.05f, 0.1f, 1f);
            camera.orthographic = true;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.depth = -100f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";
            DontDestroyOnLoad(cameraObject);
        }

        private void SendRoomChatFromInput()
        {
            var chatInputField = uiDocument?.rootVisualElement?.Q<TextField>("chat-input-field");
            if (chatInputField == null)
            {
                return;
            }

            var text = chatInputField.value?.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            wsController?.SendRoomChat(text);
            chatInputField.value = string.Empty;
        }

        private void BindUi(VisualElement root)
        {
            loginNameField = root.Q<TextField>("login-name-field");
            passwordField = root.Q<TextField>("password-field");
            registerDisplayNameField = root.Q<TextField>("register-handle-field");
            registerEmailField = root.Q<TextField>("register-email-field");
            registerPasswordField = root.Q<TextField>("register-password-field");
            registerConfirmPasswordField = root.Q<TextField>("register-confirm-password-field");

            foreach (var secretField in new[] { passwordField, registerPasswordField, registerConfirmPasswordField })
            {
                if (secretField != null)
                {
                    secretField.isPasswordField = true;
                }
            }

            root.Q<Button>("login-button")?.RegisterCallback<ClickEvent>(_ => Login());
            root.Q<Button>("register-button")?.RegisterCallback<ClickEvent>(_ => Register());
            root.Q<Button>("logout-button")?.RegisterCallback<ClickEvent>(_ => Logout());

            RegisterSubmitOnEnter(passwordField, Login);
            RegisterSubmitOnEnter(registerConfirmPasswordField, Register);
            root.Q<Button>("refresh-button")?.RegisterCallback<ClickEvent>(_ => RefreshSummary());
            root.Q<Button>("home-development-button")?.RegisterCallback<ClickEvent>(_ => navigationState.SetActive(ShellScreen.City));
            root.Q<Button>("whereami-button")?.RegisterCallback<ClickEvent>(_ => wsController?.RequestWhereAmI());
            root.Q<Button>("ping-button")?.RegisterCallback<ClickEvent>(_ => wsController?.SendPing());

            root.Q<Button>("nav-home-button")?.RegisterCallback<ClickEvent>(_ => navigationState.SetActive(ShellScreen.Summary));
            root.Q<Button>("nav-development-button")?.RegisterCallback<ClickEvent>(_ => navigationState.SetActive(ShellScreen.City));
            root.Q<Button>("nav-warfront-button")?.RegisterCallback<ClickEvent>(_ => navigationState.SetActive(ShellScreen.BlackMarket));
            root.Q<Button>("nav-heroes-button")?.RegisterCallback<ClickEvent>(_ => navigationState.SetActive(ShellScreen.Heroes));
            root.Q<Button>("nav-social-button")?.RegisterCallback<ClickEvent>(_ => navigationState.SetActive(ShellScreen.Social));

            root.Q<Button>("chat-all-button")?.RegisterCallback<ClickEvent>(_ => sessionState.SetActiveChatChannel("all"));
            root.Q<Button>("chat-room-button")?.RegisterCallback<ClickEvent>(_ => sessionState.SetActiveChatChannel("room"));
            root.Q<Button>("chat-system-button")?.RegisterCallback<ClickEvent>(_ => sessionState.SetActiveChatChannel("system"));

            root.Q<Button>("send-chat-button")?.RegisterCallback<ClickEvent>(_ => SendRoomChatFromInput());
            root.Q<TextField>("chat-input-field")?.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter)
                {
                    return;
                }

                evt.StopPropagation();
                SendRoomChatFromInput();
            });
        }

        private async void RefreshSummary()
        {
            try
            {
                await summaryController.RefreshAsync();
            }
            catch (Exception ex)
            {
                summaryState.SetError(ex.Message);
            }
        }

        private async void Login()
        {
            var user = loginNameField?.value?.Trim();
            var pass = passwordField?.value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(user))
            {
                sessionState.SetLoginStatus("Enter an email or handle to sign in.");
                return;
            }

            if (string.IsNullOrWhiteSpace(pass))
            {
                sessionState.SetLoginStatus("Enter a password to sign in.");
                return;
            }

            try
            {
                sessionState.SetLoginStatus("Signing in...");
                await authController.LoginAsync(user, pass);
                navigationState.SetActive(ShellScreen.Summary);
                RefreshSummary();
            }
            catch (Exception ex)
            {
                sessionState.SetLoginStatus($"Login failed: {ex.Message}");
            }
        }

        private async void Register()
        {
            var displayName = registerDisplayNameField?.value?.Trim();
            var email = registerEmailField?.value?.Trim();
            var pass = registerPasswordField?.value ?? string.Empty;
            var confirm = registerConfirmPasswordField?.value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(displayName))
            {
                sessionState.SetLoginStatus("Choose a display name before registering.");
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                sessionState.SetLoginStatus("Enter an email before registering.");
                return;
            }

            if (string.IsNullOrWhiteSpace(pass))
            {
                sessionState.SetLoginStatus("Choose a password before registering.");
                return;
            }

            if (!string.Equals(pass, confirm, StringComparison.Ordinal))
            {
                sessionState.SetLoginStatus("Password confirmation does not match.");
                return;
            }

            try
            {
                sessionState.SetLoginStatus("Creating account...");
                await authController.RegisterAsync(displayName, email, pass);
                if (sessionState.IsAuthenticated)
                {
                    navigationState.SetActive(ShellScreen.Summary);
                    RefreshSummary();
                }
            }
            catch (Exception ex)
            {
                sessionState.SetLoginStatus($"Registration failed: {ex.Message}");
            }
        }

        private static void RegisterSubmitOnEnter(TextField field, Action action)
        {
            if (field == null || action == null)
            {
                return;
            }

            field.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter)
                {
                    return;
                }

                evt.StopPropagation();
                action();
            });
        }

        private void Logout()
        {
            authController.Logout();
            navigationState.SetActive(ShellScreen.Summary);
        }

        private void Render() => appShellController?.Render();

        private string ResolveHttpBaseUrl()
        {
            var explicitBase = httpBaseUrl?.Trim();
            if (!string.IsNullOrWhiteSpace(explicitBase))
            {
                return explicitBase;
            }

            var wsUrl = networkClient != null ? networkClient.ServerUrl : string.Empty;
            if (Uri.TryCreate(wsUrl, UriKind.Absolute, out var wsUri))
            {
                var builder = new UriBuilder(wsUri)
                {
                    Scheme = wsUri.Scheme.Equals("wss", StringComparison.OrdinalIgnoreCase) ? "https" : "http",
                    Path = string.Empty,
                    Query = string.Empty
                };
                return builder.Uri.GetLeftPart(UriPartial.Authority);
            }

            return "http://127.0.0.1:7777";
        }
    }
}
