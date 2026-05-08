using NUnit.Framework;
using PlanarWar.Client.Core.Application;
using PlanarWar.Client.Core.Contracts;
using PlanarWar.Client.Core.Mapping;
using PlanarWar.Client.UI.Screens.Summary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace PlanarWar.Client.Tests.EditMode
{
    public class ClientPressureSurfaceConsumptionTests
    {
        [Test]
        public void Client_summary_mapper_captures_client_pressure_surface_for_unity_consumption()
        {
            var summary = ShellSummarySnapshotMapper.Map(
                "{" +
                "\"hasCity\":true," +
                "\"city\":{\"name\":\"Tempest\",\"settlementLane\":\"black_market\",\"settlementLaneProfile\":{\"label\":\"Black Market\"}}," +
                "\"clientPressureSurface\":{" +
                "\"schemaVersion\":1," +
                "\"lane\":\"black_market\"," +
                "\"state\":\"being_answered\"," +
                "\"severity\":\"urgent\"," +
                "\"title\":\"Shadow pressure has a follow-up\"," +
                "\"summary\":\"A guided receipt kept this pressure seam readable.\"," +
                "\"primaryFocus\":\"missions\"," +
                "\"ctaLabel\":\"Open Operations and inspect the shadow follow-up\"," +
                "\"whyNow\":\"The board already has a deniable follow-up connected to the current pressure trail.\"," +
                "\"navigationIntent\":{\"workspace\":\"operations\",\"section\":\"mission_board\",\"emphasis\":\"recommended\",\"action\":\"inspect\",\"label\":\"Inspect shadow follow-up\",\"reason\":\"Client-safe navigation points to an existing shadow board offer.\"}," +
                "\"actionCards\":[{\"id\":\"mission_lead\",\"kind\":\"inspect_mission\",\"label\":\"Inspect shadow follow-up\",\"summary\":\"Use the existing board offer; do not invent a new action.\",\"priority\":\"recommended\",\"workspace\":\"operations\",\"section\":\"mission_board\",\"enabled\":true,\"source\":\"mission_lead\"}]," +
                "\"progressTrail\":[{\"phase\":\"mission_lead\",\"emphasis\":\"recommended\",\"label\":\"Follow-up Lead: Pressure the crooked handoff\",\"summary\":\"A recent guided receipt bent the mission board toward this follow-up.\",\"at\":null,\"outcome\":null}]," +
                "\"attentionBadge\":{\"tone\":\"warning\",\"label\":\"Shadow follow-up ready\",\"summary\":\"Inspect the existing board lead.\",\"placement\":\"operations\",\"actionCardId\":\"mission_lead\",\"missionId\":\"mission_shadow_quick_followup\",\"proofAt\":\"2026-05-07T08:00:00.000Z\",\"showInFastSession\":true,\"showInGameplayHud\":true}," +
                "\"consumptionContract\":{\"schemaVersion\":1,\"clientTargets\":[\"unity_gameplay\",\"web_fast_session\"],\"primaryActionCardId\":\"mission_lead\",\"primaryMissionId\":\"mission_shadow_quick_followup\",\"canInspectMission\":true,\"canInspectPressureStatus\":false,\"hasProgressTrail\":true,\"hasProof\":true,\"hasFollowupLead\":true,\"rewardsAreBackendAuthored\":true,\"recommendedPowerIsBackendAuthored\":true,\"executionEnabled\":false,\"clientMutationRequired\":false}," +
                "\"quickSessionSummary\":{\"headline\":\"Shadow follow-up ready\",\"body\":\"Inspect the existing board lead.\",\"bullets\":[\"Board lead: Follow-up Lead: Pressure the crooked handoff.\",\"Uses backend rewards and 240 recommended power.\"],\"primaryActionCardId\":\"mission_lead\",\"primaryMissionId\":\"mission_shadow_quick_followup\",\"showMissionLead\":true,\"showProofTrail\":true,\"emptyState\":false,\"clientTargets\":[\"unity_gameplay\",\"web_fast_session\"]}," +
                "\"pressureScore\":88," +
                "\"exposureScore\":61," +
                "\"openWindowCount\":1," +
                "\"latestProofTitle\":\"Guided receipt proof\"," +
                "\"latestProofAt\":\"2026-05-07T08:00:00.000Z\"," +
                "\"latestProofOutcome\":\"success\"," +
                "\"missionLead\":{\"missionId\":\"mission_shadow_quick_followup\",\"title\":\"Follow-up Lead: Pressure the crooked handoff\",\"summary\":\"Deniable operators can reuse the current pressure seam.\",\"reason\":\"A recent guided receipt bent the mission board toward this follow-up.\",\"priority\":\"recommended\",\"kind\":\"army\",\"difficulty\":\"medium\",\"recommendedPower\":240,\"expectedRewards\":{\"wealth\":28,\"influence\":4},\"responseTags\":[\"recon\",\"command\"]}," +
                "\"signals\":[\"guided receipt proof\",\"/api/internal should not render\"]," +
                "\"guardrails\":[\"Client summary only: this surface does not execute missions.\",\"runtimeActionId and action_hidden_leak should be sanitized\"]" +
                "}" +
                "}");

            var surface = summary.ClientPressureSurface;
            Assert.That(surface, Is.Not.Null);
            Assert.That(surface.SchemaVersion, Is.EqualTo(1));
            Assert.That(surface.Lane, Is.EqualTo("black_market"));
            Assert.That(surface.QuickSessionSummary.Headline, Is.EqualTo("Shadow follow-up ready"));
            Assert.That(surface.QuickSessionSummary.ClientTargets, Does.Contain("unity_gameplay"));
            Assert.That(surface.ConsumptionContract.ExecutionEnabled, Is.False);
            Assert.That(surface.ConsumptionContract.ClientMutationRequired, Is.False);
            Assert.That(surface.ConsumptionContract.RewardsAreBackendAuthored, Is.True);
            Assert.That(surface.ConsumptionContract.RecommendedPowerIsBackendAuthored, Is.True);
            Assert.That(surface.AttentionBadge.ShowInGameplayHud, Is.True);
            Assert.That(surface.MissionLead.MissionId, Is.EqualTo("mission_shadow_quick_followup"));
            Assert.That(surface.MissionLead.RecommendedPower, Is.EqualTo(240));
            Assert.That(surface.MissionLead.ExpectedRewards["wealth"], Is.EqualTo(28));
            Assert.That(surface.MissionLead.ResponseTags, Does.Contain("recon"));

            var rendered = string.Join("\n", CollectSnapshotStrings(surface));
            Assert.That(rendered, Does.Not.Contain("/api"));
            Assert.That(rendered, Does.Not.Contain("runtimeActionId"));
            Assert.That(rendered, Does.Not.Contain("action_hidden_leak"));
        }

        [Test]
        public void Unity_summary_controller_consumes_client_pressure_surface_without_mutation_hooks()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/Summary/SummaryScreenController.cs");
            Assert.That(File.Exists(controllerPath), Is.True, "SummaryScreenController.cs should be available from the Unity project root.");

            var source = File.ReadAllText(controllerPath);
            Assert.That(source, Does.Contain("RenderClientPressureSurface(clientPressureSurface, summary)"));
            Assert.That(source, Does.Contain("Inspect-only: this card does not start missions or change state."));
            Assert.That(source, Does.Contain("No client-side action is required here."));
            Assert.That(source, Does.Contain("Primary action: inspect the existing board offer."));
            Assert.That(source, Does.Contain("does not execute missions"));
            Assert.That(source, Does.Not.Contain("Primary mission: {contract.PrimaryMissionId}"));
            Assert.That(source, Does.Not.Contain("lead.Title, lead.MissionId"));
            Assert.That(source, Does.Not.Contain("/api/me clientPressureSurface"));
            Assert.That(source, Does.Not.Contain("PostJsonAsync"));
        }

        [Test]
        public void Unity_pressure_card_routes_existing_leads_to_safe_local_desks()
        {
            var summary = new ShellSummarySnapshot
            {
                HasCity = true,
                City = new CitySummarySnapshot
                {
                    Name = "Tempest",
                    SettlementLane = "black_market",
                    SettlementLaneLabel = "Black Market"
                }
            };

            AssertPressureRoute(
                new ClientPressureSurfaceSnapshot
                {
                    PrimaryFocus = "missions",
                    NavigationIntent = new ClientPressureNavigationIntentSnapshot { Workspace = "operations", Section = "mission_board" },
                    ActionCards = new List<ClientPressureActionCardSnapshot>
                    {
                        new() { Id = "mission_lead", Kind = "inspect_mission", Workspace = "operations", Section = "mission_board", Enabled = true }
                    },
                    ConsumptionContract = new ClientPressureConsumptionContractSnapshot { CanInspectMission = true, PrimaryMissionId = "mission_shadow_followup" },
                    MissionLead = new ClientPressureMissionLeadSnapshot { MissionId = "mission_shadow_followup", Title = "Follow-up Lead" }
                },
                summary,
                ShellScreen.BlackMarket);

            AssertPressureRoute(
                new ClientPressureSurfaceSnapshot
                {
                    PrimaryFocus = "development",
                    NavigationIntent = new ClientPressureNavigationIntentSnapshot { Workspace = "development", Section = "build_queue" },
                    ActionCards = new List<ClientPressureActionCardSnapshot>
                    {
                        new() { Id = "development_pressure", Kind = "review_development", Workspace = "development", Section = "build_queue", Enabled = true }
                    }
                },
                summary,
                ShellScreen.City);

            AssertPressureRoute(
                new ClientPressureSurfaceSnapshot
                {
                    PrimaryFocus = "heroes",
                    NavigationIntent = new ClientPressureNavigationIntentSnapshot { Workspace = "heroes", Section = "hero_readiness" },
                    ActionCards = new List<ClientPressureActionCardSnapshot>
                    {
                        new() { Id = "readiness_pressure", Kind = "review_readiness", Workspace = "heroes", Section = "hero_readiness", Enabled = true }
                    }
                },
                summary,
                ShellScreen.Heroes);

            AssertPressureRoute(
                new ClientPressureSurfaceSnapshot
                {
                    PrimaryFocus = "status",
                    NavigationIntent = new ClientPressureNavigationIntentSnapshot { Workspace = "status", Section = "pressure_status" },
                    ActionCards = new List<ClientPressureActionCardSnapshot>
                    {
                        new() { Id = "latest_proof", Kind = "review_pressure", Workspace = "status", Section = "pressure_status", Enabled = true }
                    },
                    ConsumptionContract = new ClientPressureConsumptionContractSnapshot { CanInspectPressureStatus = true, HasProof = true, HasProgressTrail = true },
                    LatestProofTitle = "Guided receipt proof"
                },
                summary,
                ShellScreen.Summary);

            AssertPressureRoute(
                new ClientPressureSurfaceSnapshot
                {
                    PrimaryFocus = "home",
                    NavigationIntent = new ClientPressureNavigationIntentSnapshot { Workspace = "home", Section = "overview" },
                    ActionCards = new List<ClientPressureActionCardSnapshot>
                    {
                        new() { Id = "overview", Kind = "monitor_overview", Workspace = "home", Section = "overview", Enabled = true }
                    }
                },
                summary,
                ShellScreen.Summary);
        }

        private static void AssertPressureRoute(ClientPressureSurfaceSnapshot surface, ShellSummarySnapshot summary, ShellScreen expected)
        {
            var method = typeof(SummaryScreenController).GetMethod("ResolveClientPressureScreen", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "ResolveClientPressureScreen should stay available for pressure-route guard coverage.");

            var actual = (ShellScreen)method.Invoke(null, new object[] { surface, summary });
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static IEnumerable<string> CollectSnapshotStrings(object value)
        {
            if (value == null)
            {
                yield break;
            }

            if (value is string text)
            {
                yield return text;
                yield break;
            }

            if (value is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    foreach (var nested in CollectSnapshotStrings(entry.Key))
                    {
                        yield return nested;
                    }

                    foreach (var nested in CollectSnapshotStrings(entry.Value))
                    {
                        yield return nested;
                    }
                }

                yield break;
            }

            if (value is IEnumerable enumerable)
            {
                foreach (var entry in enumerable)
                {
                    foreach (var nested in CollectSnapshotStrings(entry))
                    {
                        yield return nested;
                    }
                }

                yield break;
            }

            var type = value.GetType();
            if (type.IsPrimitive || type.IsEnum || type == typeof(decimal) || type == typeof(DateTime))
            {
                yield break;
            }

            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!property.CanRead || property.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                foreach (var nested in CollectSnapshotStrings(property.GetValue(value)))
                {
                    yield return nested;
                }
            }
        }
    }
}
