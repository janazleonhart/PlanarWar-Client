using NUnit.Framework;
using PlanarWar.Client.Core;
using PlanarWar.Client.Core.Contracts;
using PlanarWar.Client.Core.Mapping;
using PlanarWar.Client.Core.Presentation;
using PlanarWar.Client.UI.Screens.City;
using PlanarWar.Client.UI.Screens.Heroes;
using PlanarWar.Client.UI.Screens.Summary;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine.UIElements;

namespace PlanarWar.Client.Tests.EditMode
{
    public class ContractTruthConsumptionTests
    {
        [Test]
        public void Left_rail_compact_badges_stay_readable_without_right_action_chips()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var stylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(stylePath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(stylePath);

            Assert.That(uxml, Does.Contain("text=\"Chat\" class=\"chapter-row__badge\""), "Social rail badge should use a short readable label instead of cramped comms copy.");
            Assert.That(uxml, Does.Contain("text=\"Help\" class=\"chapter-row__badge\""), "Tester Guide rail badge should stay short enough for the compact rail.");
            Assert.That(uss, Does.Contain("Left rail readability / badge cleanup v1"));
            Assert.That(uss, Does.Contain(".rail-panel--compact .chapter-row__action"));
            Assert.That(uss, Does.Contain("display: none"), "Compact rail hides redundant right-side action chips so badges do not collide or truncate.");
        }

        [Test]
        public void Operations_visible_card_slots_all_have_action_buttons()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            for (var i = 1; i <= 4; i++)
            {
                Assert.That(uxml, Does.Contain($"warfront-card-{i}-button"), $"Visible operation card {i} needs a button slot so actionable cards do not render as dead text.");
            }
        }

        [Test]
        public void Shell_has_dedicated_hero_roster_lane_and_controls()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            Assert.That(uxml, Does.Contain("nav-heroes-button"));
            Assert.That(uxml, Does.Contain("nav-heroes-title"));
            Assert.That(uxml, Does.Contain("nav-heroes-copy"));
            Assert.That(uxml, Does.Contain("heroes-screen"));
            Assert.That(uxml, Does.Contain("heroes-manage-hero-field"));
            Assert.That(uxml, Does.Contain("heroes-release-button"));
            Assert.That(uxml, Does.Contain("heroes-manage-candidate-field"));
            Assert.That(uxml, Does.Contain("heroes-candidate-picker"));
            Assert.That(uxml, Does.Contain("heroes-candidate-accept-button"));
            Assert.That(uxml, Does.Contain("heroes-candidate-dismiss-button"));
            Assert.That(uxml, Does.Contain("Hero / Operative desk"));
        }

        [Test]
        public void Operations_city_lane_translates_shadow_force_terms_to_troop_language()
        {
            var formatter = typeof(PlanarWar.Client.UI.Screens.BlackMarket.BlackMarketScreenController)
                .GetMethod("ApplyCityForceTerms", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(formatter, Is.Not.Null);

            var result = (string)formatter.Invoke(null, new object[]
            {
                "Cell lead • 220 agents • Supplies 40 • Cashflow 23 • Rename cell • Assign route • Open pressure line • Run disruption action"
            });

            Assert.That(result, Does.Contain("Formation lead"));
            Assert.That(result, Does.Contain("220 troops"));
            Assert.That(result, Does.Contain("Materials 40"));
            Assert.That(result, Does.Contain("Wealth 23"));
            Assert.That(result, Does.Contain("Rename formation"));
            Assert.That(result, Does.Contain("Assign line"));
            Assert.That(result, Does.Contain("Launch warfront assault"));
            Assert.That(result, Does.Contain("Launch quick strike"));
            Assert.That(result, Does.Not.Contain("agents"));
            Assert.That(result, Does.Not.Contain("Cell"));
        }

        [Test]
        public void Formatter_keeps_lifecycle_and_effects_language_specific_instead_of_generic()
        {
            var followThrough = new ContractFollowThroughSnapshot
            {
                ContractTitle = "Route Bread and Lamp Oil",
                State = "cooling",
                SourceSurface = "/api/me.worldConsequenceResponseReceipts",
                Note = "Cooling the same civic line."
            };

            var civicEffects = new PublicBackboneContractEffectsSnapshot
            {
                QueueEffect = "cooling",
                TrustEffect = "steadying",
                ServiceEffect = "restoring"
            };

            var shadowEffects = new ShadowContractEffectsSnapshot
            {
                ReceiptChainState = "linked",
                CovertCarryState = "carried"
            };

            Assert.That(
                ContractTruthText.BuildContractSeamValue(followThrough, "fallback"),
                Is.EqualTo("Route Bread and Lamp Oil • Cooling"));

            Assert.That(
                ContractTruthText.BuildContractLifecycleValue(followThrough, "fallback"),
                Is.EqualTo("Cooling • World-response reports"));

            Assert.That(
                ContractTruthText.BuildCivicEffectsValue(civicEffects, "fallback"),
                Is.EqualTo("Queue cooling • Trust steadying • Services restoring"));

            Assert.That(
                ContractTruthText.BuildShadowEffectsValue(shadowEffects, "fallback"),
                Is.EqualTo("Report chain linked • Covert carry carried"));
        }


        [Test]
        public void Formatter_builds_city_contract_recovery_board_copy_without_execution_language()
        {
            var board = new CityContractRecoveryBoardSnapshot
            {
                State = "opportunities_available",
                Title = "City contract recovery board has candidate opportunities",
                RecommendedCityDeskAction = "Target the regional recovery desk at the hottest eligible region.",
                EligibleRegionIds = new List<string> { "ash_road" },
                CandidateCount = 1,
                Candidates = new List<CityContractRecoveryCandidateSnapshot>
                {
                    new CityContractRecoveryCandidateSnapshot
                    {
                        Title = "Stabilize Ash Road",
                        Priority = "high",
                        Desk = "regional_recovery",
                        EligibleRegionIds = new List<string> { "ash_road" },
                        NextReceiptFamily = "city_contract_regional_stabilization_receipt",
                        RequiredResources = new CityContractRecoveryResourcesRequirementSnapshot
                        {
                            Affordability = "insufficient_resources",
                            Required = new ResourceSnapshot { Food = 25, Materials = 40 },
                            Shortfall = new ResourceSnapshot { Materials = 12 },
                        },
                    },
                },
            };

            Assert.That(
                ContractTruthText.BuildCityContractRecoveryBoardValue(board, "fallback"),
                Is.EqualTo("1 candidate • Stabilize Ash Road • opportunities available"));
            Assert.That(
                ContractTruthText.BuildCityContractRecoveryResourcesValue(board.Candidates[0].RequiredResources, "fallback"),
                Is.EqualTo("insufficient resources • Shortfall 12 materials"));
            Assert.That(
                ContractTruthText.BuildCityContractRecoveryBoardNote(board, "fallback"),
                Does.Contain("Target the regional recovery desk"));
            Assert.That(
                ContractTruthText.BuildCityContractRecoveryBoardNote(board, "fallback"),
                Does.Contain("Next report: city contract regional stabilization report"));
        }

        [Test]
        public void Client_summary_mapper_captures_city_contract_recovery_board()
        {
            var summary = ShellSummarySnapshotMapper.Map(
                "{" +
                "\"hasCity\":true," +
                "\"city\":{\"name\":\"Tempest\",\"settlementLane\":\"city\",\"settlementLaneProfile\":{\"label\":\"City\"}}," +
                "\"cityContractRecoveryBoard\":{" +
                "\"state\":\"opportunities_available\"," +
                "\"title\":\"Recovery board\"," +
                "\"summary\":\"One candidate can be summarized.\"," +
                "\"settlementLane\":\"city\"," +
                "\"recommendedFocus\":\"regional_recovery\"," +
                "\"recommendedCityDeskAction\":\"Target regional recovery.\"," +
                "\"eligibleRegionIds\":[\"ash_road\"]," +
                "\"candidateCount\":1," +
                "\"sourceSurfaces\":[\"/api/me.worldConsequences\",\"/api/me.cityMudWorldConsequenceBridge\"]," +
                "\"guardrails\":[\"Read-only board\",\"Does not grant items or rewards\"]," +
                "\"latestRelevantReceipt\":{\"id\":\"r1\",\"createdAt\":\"2026-05-03T00:00:00.000Z\",\"title\":\"Recovered route\",\"summary\":\"Relief moved.\",\"severity\":\"medium\",\"outcome\":\"success\",\"source\":\"runtime_response\",\"regionId\":\"ash_road\"}," +
                "\"candidates\":[{" +
                "\"id\":\"recovery_action_region_ash_road\"," +
                "\"actionId\":\"action_region_ash_road\"," +
                "\"title\":\"Stabilize Ash Road\"," +
                "\"summary\":\"Recover a pressured route.\"," +
                "\"priority\":\"high\"," +
                "\"desk\":\"regional_recovery\"," +
                "\"recommendedCityDeskAction\":\"Target the regional recovery desk.\"," +
                "\"eligibleRegionIds\":[\"ash_road\"]," +
                "\"sourcePressureConsequence\":{\"sourceHook\":\"motherBrainBurden\",\"sourceRegionId\":\"ash_road\",\"sourceLane\":\"regional\",\"priority\":\"high\",\"actionTitle\":\"Stabilize Ash Road\",\"actionSummary\":\"Recover route pressure.\",\"evidence\":[{\"label\":\"destabilization\",\"value\":42,\"tone\":\"high\"}]}," +
                "\"requiredPosture\":{\"settlementLane\":\"city\",\"bridgeState\":\"pressured\",\"bridgeFocus\":\"regional_recovery\",\"bridgePosture\":\"support\",\"actionPriority\":\"high\"}," +
                "\"requiredResources\":{\"required\":{\"food\":25,\"materials\":40},\"shortfall\":{\"materials\":12},\"affordability\":\"insufficient_resources\",\"executable\":false}," +
                "\"nextReceiptFamily\":\"city_contract_regional_stabilization_receipt\"," +
                "\"guardrails\":[\"Candidate only\"]," +
                "\"latestRelevantSummary\":{\"id\":\"c1\",\"createdAt\":\"2026-05-03T00:00:00.000Z\",\"title\":\"Ash Road unstable\",\"summary\":\"Pressure rose.\",\"severity\":\"high\",\"source\":\"world\",\"regionId\":\"ash_road\"}," +
                "\"recommendedMoves\":[\"Review supply support\"]" +
                "}]" +
                "}" +
                "}");

            Assert.That(summary.CityContractRecoveryBoard, Is.Not.Null);
            Assert.That(summary.CityContractRecoveryBoard.State, Is.EqualTo("opportunities_available"));
            Assert.That(summary.CityContractRecoveryBoard.CandidateCount, Is.EqualTo(1));
            Assert.That(summary.CityContractRecoveryBoard.EligibleRegionIds, Does.Contain("ash_road"));
            Assert.That(summary.CityContractRecoveryBoard.Guardrails, Does.Contain("Read-only board"));
            Assert.That(summary.CityContractRecoveryBoard.LatestRelevantReceipt.Title, Is.EqualTo("Recovered route"));
            Assert.That(summary.CityContractRecoveryBoard.Candidates, Has.Count.EqualTo(1));
            Assert.That(summary.CityContractRecoveryBoard.Candidates[0].Title, Is.EqualTo("Stabilize Ash Road"));
            Assert.That(summary.CityContractRecoveryBoard.Candidates[0].SourcePressureConsequence.Evidence[0].Value, Is.EqualTo(42));
            Assert.That(summary.CityContractRecoveryBoard.Candidates[0].RequiredResources.Shortfall.Materials, Is.EqualTo(12));
            Assert.That(summary.CityContractRecoveryBoard.Candidates[0].RequiredPosture.BridgeFocus, Is.EqualTo("regional_recovery"));
            Assert.That(summary.CityContractRecoveryBoard.Candidates[0].RecommendedMoves, Does.Contain("Review supply support"));
        }

        [Test]
        public void Client_summary_mapper_ignores_oversized_city_contract_recovery_board_numbers_without_breaking_city_summary()
        {
            var summary = ShellSummarySnapshotMapper.Map(
                "{" +
                "\"hasCity\":true," +
                "\"city\":{\"name\":\"Tempest\",\"settlementLane\":\"city\",\"settlementLaneProfile\":{\"label\":\"City\"}}," +
                "\"cityContractRecoveryBoard\":{" +
                "\"state\":\"opportunities_available\"," +
                "\"candidateCount\":9223372036854775807," +
                "\"eligibleRegionIds\":[\"ash_road\"]," +
                "\"candidates\":[{" +
                "\"title\":\"Stabilize Ash Road\"," +
                "\"sourcePressureConsequence\":{\"evidence\":[{\"label\":\"oversized signal\",\"value\":9223372036854775807,\"tone\":\"watch\"}]}," +
                "\"requiredResources\":{\"required\":{\"food\":25},\"affordability\":\"advisory_only\",\"cooldownMsRemaining\":9223372036854775807}" +
                "}]" +
                "}" +
                "}");

            Assert.That(summary.HasCity, Is.True);
            Assert.That(summary.City.Name, Is.EqualTo("Tempest"));
            Assert.That(summary.CityContractRecoveryBoard, Is.Not.Null);
            Assert.That(summary.CityContractRecoveryBoard.CandidateCount, Is.EqualTo(1));
            Assert.That(summary.CityContractRecoveryBoard.Candidates, Has.Count.EqualTo(1));
            Assert.That(summary.CityContractRecoveryBoard.Candidates[0].SourcePressureConsequence.Evidence[0].Value, Is.Null);
            Assert.That(summary.CityContractRecoveryBoard.Candidates[0].RequiredResources.CooldownMsRemaining, Is.Null);
        }

        [Test]
        public void Home_surface_exposes_city_contract_recovery_board_without_execution_claims()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var summaryPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/Summary/SummaryScreenController.cs");
            var guidePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Docs/PLAYER_TESTER_GUIDE_V1.md");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(summaryPath), Is.True, "SummaryScreenController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(guidePath), Is.True, "PLAYER_TESTER_GUIDE_V1.md should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var summary = File.ReadAllText(summaryPath);
            var guide = File.ReadAllText(guidePath);

            Assert.That(uxml, Does.Contain("city-contract-recovery-board-card"));
            Assert.That(uxml, Does.Contain("city-contract-recovery-board-candidate-value"));
            Assert.That(uxml, Does.Contain("city-contract-recovery-board-resources-value"));
            Assert.That(uxml, Does.Contain("city-contract-recovery-board-receipt-value"));
            Assert.That(summary, Does.Contain("RenderCityContractRecoveryBoard"));
            Assert.That(summary, Does.Contain("CityContractRecoveryBoard"));
            Assert.That(guide, Does.Contain("Recovery opportunities"));
            Assert.That(guide, Does.Contain("live server recovery truth"));
            Assert.That(guide, Does.Contain("does not execute contracts"));
            Assert.That(guide, Does.Not.Contain("recovery reward button"));
        }

        [Test]
        public void Development_front_lane_counts_operator_front_timers_as_visible_front_timing()
        {
            var summary = new ShellSummarySnapshot
            {
                HasCity = true,
                City = new CitySummarySnapshot
                {
                    Name = "Black Market Tester",
                    SettlementLane = "black_market",
                    SettlementLaneLabel = "Black Market"
                },
                Buildings = new List<BuildingSnapshot>
                {
                    new BuildingSnapshot
                    {
                        Id = "safehouse_ring",
                        Name = "Safehouse Ring",
                        Lane = "black_market",
                        Status = "active"
                    }
                },
                CityTimers = new List<CityTimerEntrySnapshot>
                {
                    new CityTimerEntrySnapshot
                    {
                        Id = "front_timer_1",
                        Category = "operator_front",
                        Label = "Quiet route front timer",
                        Status = "active",
                        FinishesAtUtc = DateTime.UtcNow.AddMinutes(3)
                    },
                    new CityTimerEntrySnapshot
                    {
                        Id = "city_research_1",
                        Category = "research",
                        Label = "Research timer",
                        Status = "active",
                        FinishesAtUtc = DateTime.UtcNow.AddMinutes(5)
                    }
                }
            };

            var selector = typeof(CityScreenController).GetMethod("SelectBuildTimers", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(selector, Is.Not.Null);

            var blackMarketTimers = (List<CityTimerEntrySnapshot>)selector.Invoke(null, new object[] { summary, true });
            var cityTimers = (List<CityTimerEntrySnapshot>)selector.Invoke(null, new object[] { summary, false });

            Assert.That(blackMarketTimers, Has.Count.EqualTo(1));
            Assert.That(blackMarketTimers[0].Id, Is.EqualTo("front_timer_1"));
            Assert.That(cityTimers, Is.Empty);
        }

        [Test]
        public void Mapper_promotes_active_research_arrays_and_research_timers_without_counting_front_timers()
        {
            const string payload = @"{
                ""hasCity"": true,
                ""city"": { ""name"": ""Black Market Tester"", ""settlementLane"": ""black_market"", ""settlementLaneProfile"": { ""label"": ""Black Market"" } },
                ""activeResearches"": [
                    { ""techId"": ""urban_planning_1"", ""name"": ""Urban Planning I"", ""status"": ""active"", ""finishesAt"": ""2026-04-25T12:00:00Z"" }
                ],
                ""cityTimers"": [
                    { ""id"": ""basic_sanitation"", ""category"": ""research"", ""label"": ""Basic Sanitation"", ""status"": ""active"", ""finishesAt"": ""2026-04-25T11:59:00Z"" },
                    { ""id"": ""heartland_basin"", ""category"": ""operator_front"", ""label"": ""Operations window heartland_basin"", ""status"": ""active"", ""finishesAt"": ""2026-04-25T11:58:00Z"" }
                ]
            }";

            var summary = ShellSummarySnapshotMapper.Map(payload);
            var ids = summary.ActiveResearches.ConvertAll(r => r.Id);

            Assert.That(summary.ActiveResearches, Has.Count.EqualTo(2));
            Assert.That(ids, Does.Contain("urban_planning_1"));
            Assert.That(ids, Does.Contain("basic_sanitation"));
            Assert.That(ids, Does.Not.Contain("heartland_basin"));
            Assert.That(summary.ActiveResearch.Id, Is.EqualTo("basic_sanitation"));
        }

        [Test]
        public void Development_research_start_block_respects_canonical_active_research_and_recent_accepted_start()
        {
            var selector = typeof(CityScreenController).GetMethod("IsResearchStartBlocked", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(selector, Is.Not.Null);

            var canonical = new ShellSummarySnapshot
            {
                ActiveResearches = new List<ResearchSnapshot>
                {
                    new ResearchSnapshot { Id = "urban_planning_1", Name = "Urban Planning I", FinishesAtUtc = DateTime.UtcNow.AddSeconds(30) }
                }
            };

            var empty = new ShellSummarySnapshot();
            var state = new SummaryState();

            Assert.That((bool)selector.Invoke(null, new object[] { canonical, state, DateTime.UtcNow }), Is.True);
            Assert.That((bool)selector.Invoke(null, new object[] { empty, state, DateTime.UtcNow }), Is.False);

            state.MarkResearchStartAccepted("basic_sanitation");
            Assert.That((bool)selector.Invoke(null, new object[] { empty, state, DateTime.UtcNow }), Is.True);
        }

        [Test]
        public void Mapper_captures_researched_tech_ids_for_completion_reconciliation()
        {
            const string payload = @"{
                ""researchedTechIds"": [""militia_training_1"", ""urban_planning_1""],
                ""availableTechs"": [
                    { ""id"": ""district_roads_1"", ""name"": ""District Roads I"" }
                ]
            }";

            var summary = ShellSummarySnapshotMapper.Map(payload);

            Assert.That(summary.ResearchedTechIds, Does.Contain("militia_training_1"));
            Assert.That(summary.ResearchedTechIds, Does.Contain("urban_planning_1"));
        }

        [Test]
        public void Research_start_guard_clears_when_accepted_tech_is_completed_before_timer_can_display()
        {
            var state = new SummaryState();
            var startedAt = DateTime.UtcNow;
            state.MarkResearchStartAccepted("militia_training_1");

            state.ReconcileRecentResearchStartWithSnapshot(new ShellSummarySnapshot
            {
                ResearchedTechIds = new List<string> { "militia_training_1" },
                AvailableTechs = new List<TechOptionSnapshot>
                {
                    new TechOptionSnapshot { Id = "district_watch", Name = "District Watch" }
                }
            }, startedAt.AddSeconds(2));

            Assert.That(state.HasRecentResearchStartGuard(startedAt.AddSeconds(2)), Is.False);
            Assert.That(state.HasRecentResearchCompletionNotice(startedAt.AddSeconds(2)), Is.True);
            Assert.That(state.RecentCompletedResearchTechId, Is.EqualTo("militia_training_1"));
        }

        [Test]
        public void Research_start_guard_clears_when_accepted_tech_leaves_available_options_without_active_timer()
        {
            var state = new SummaryState();
            var startedAt = DateTime.UtcNow;
            state.MarkResearchStartAccepted("militia_training_1");

            state.ReconcileRecentResearchStartWithSnapshot(new ShellSummarySnapshot
            {
                AvailableTechs = new List<TechOptionSnapshot>
                {
                    new TechOptionSnapshot { Id = "district_watch", Name = "District Watch" }
                }
            }, startedAt.AddSeconds(2));

            Assert.That(state.HasRecentResearchStartGuard(startedAt.AddSeconds(2)), Is.False);
            Assert.That(state.HasRecentResearchCompletionNotice(startedAt.AddSeconds(2)), Is.True);
        }

        [Test]
        public void Research_start_guard_persists_until_canonical_research_truth_arrives()
        {
            var state = new SummaryState();
            var startedAt = DateTime.UtcNow;
            state.MarkResearchStartAccepted("animal_husbandry_1");

            Assert.That(state.HasRecentResearchStartGuard(startedAt.AddMinutes(20)), Is.True);
            Assert.That(state.HasRecentResearchStartGuard(startedAt.AddMinutes(20), guardSeconds: 12), Is.False);

            state.ReconcileRecentResearchStartWithSnapshot(new ShellSummarySnapshot(), startedAt.AddMinutes(20));
            Assert.That(state.HasRecentResearchStartGuard(startedAt.AddMinutes(20)), Is.True);

            state.ReconcileRecentResearchStartWithSnapshot(new ShellSummarySnapshot
            {
                ActiveResearches = new List<ResearchSnapshot>
                {
                    new ResearchSnapshot
                    {
                        Id = "animal_husbandry_1",
                        Name = "Animal Husbandry I",
                        FinishesAtUtc = startedAt.AddMinutes(1)
                    }
                }
            }, startedAt.AddMinutes(20));

            Assert.That(state.HasRecentResearchStartGuard(startedAt.AddMinutes(20)), Is.False);
        }


        [Test]
        public void Mapper_merges_research_timer_with_matching_active_research_card()
        {
            const string payload = @"{
                ""activeResearches"": [
                    { ""techId"": ""black_market_contacts_1"", ""name"": ""Black Market Contacts"", ""status"": ""active"", ""progress"": 180, ""cost"": 200 }
                ],
                ""cityTimers"": [
                    { ""id"": ""research:black_market_contacts_1"", ""category"": ""research"", ""label"": ""Research Black Market Contacts"", ""status"": ""active"", ""finishesAt"": ""2026-04-25T14:45:00Z"", ""detail"": ""progress:180/200 • rate:3/tick"" }
                ]
            }";

            var summary = ShellSummarySnapshotMapper.Map(payload);

            Assert.That(summary.ActiveResearches, Has.Count.EqualTo(1));
            Assert.That(summary.ActiveResearches[0].Id, Is.EqualTo("black_market_contacts_1"));
            Assert.That(summary.ActiveResearches[0].Name, Is.EqualTo("Black Market Contacts"));
            Assert.That(summary.ActiveResearches[0].Progress, Is.EqualTo(180));
            Assert.That(summary.ActiveResearches[0].Cost, Is.EqualTo(200));
            Assert.That(summary.ActiveResearches[0].FinishesAtUtc, Is.EqualTo(new DateTime(2026, 4, 25, 14, 45, 0, DateTimeKind.Utc)));
        }

        [Test]
        public void Development_research_lane_hides_available_option_that_matches_active_research()
        {
            var selector = typeof(CityScreenController).GetMethod("SelectAvailableResearchOptions", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(selector, Is.Not.Null);

            var summary = new ShellSummarySnapshot
            {
                ActiveResearches = new List<ResearchSnapshot>
                {
                    new ResearchSnapshot
                    {
                        Id = "black_market_contacts_1",
                        Name = "Black Market Contacts",
                        FinishesAtUtc = DateTime.UtcNow.AddMinutes(6)
                    }
                },
                AvailableTechs = new List<TechOptionSnapshot>
                {
                    new TechOptionSnapshot { Id = "black_market_contacts_1", Name = "Black Market Contacts" },
                    new TechOptionSnapshot { Id = "hush_routes_1", Name = "Hush Routes" }
                }
            };

            var visible = (List<TechOptionSnapshot>)selector.Invoke(null, new object[] { summary, summary.ActiveResearches });

            Assert.That(visible, Has.Count.EqualTo(1));
            Assert.That(visible[0].Id, Is.EqualTo("hush_routes_1"));
        }


        [Test]
        public void Mapper_promotes_singular_active_build_into_building_snapshot()
        {
            const string payload = @"{
                ""hasCity"": true,
                ""city"": { ""name"": ""City Tester"", ""settlementLane"": ""city"", ""settlementLaneProfile"": { ""label"": ""City"" } },
                ""activeBuild"": {
                    ""id"": ""build_123"",
                    ""action"": ""construct"",
                    ""kind"": ""housing"",
                    ""name"": ""Charter Ward 1"",
                    ""buildingId"": ""bld_123"",
                    ""targetLevel"": 1,
                    ""startedAt"": ""2026-04-25T15:00:00Z"",
                    ""finishesAt"": ""2026-04-25T15:05:00Z""
                }
            }";

            var summary = ShellSummarySnapshotMapper.Map(payload);

            Assert.That(summary.Buildings, Has.Count.EqualTo(1));
            Assert.That(summary.Buildings[0].Id, Is.EqualTo("build_123"));
            Assert.That(summary.Buildings[0].BuildingId, Is.EqualTo("bld_123"));
            Assert.That(summary.Buildings[0].Name, Is.EqualTo("Charter Ward 1"));
            Assert.That(summary.Buildings[0].Type, Is.EqualTo("housing"));
            Assert.That(summary.Buildings[0].Status, Is.EqualTo("construct"));
            Assert.That(summary.Buildings[0].Level, Is.EqualTo(1));
            Assert.That(summary.Buildings[0].FinishesAtUtc, Is.EqualTo(new DateTime(2026, 4, 25, 15, 5, 0, DateTimeKind.Utc)));
        }

        [Test]
        public void Development_build_lane_dedupes_timer_that_matches_active_building_project()
        {
            var selector = typeof(CityScreenController).GetMethod("SelectBuildTimers", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(selector, Is.Not.Null);

            var summary = new ShellSummarySnapshot
            {
                HasCity = true,
                City = new CitySummarySnapshot
                {
                    Name = "City Tester",
                    SettlementLane = "city",
                    SettlementLaneLabel = "City"
                },
                Buildings = new List<BuildingSnapshot>
                {
                    new BuildingSnapshot
                    {
                        Id = "build_123",
                        BuildingId = "bld_123",
                        Name = "Charter Ward 1",
                        Lane = "city",
                        Status = "construct",
                        StartedAtUtc = DateTime.UtcNow.AddMinutes(-1),
                        FinishesAtUtc = DateTime.UtcNow.AddMinutes(4)
                    }
                },
                CityTimers = new List<CityTimerEntrySnapshot>
                {
                    new CityTimerEntrySnapshot
                    {
                        Id = "build_123",
                        Category = "build",
                        Label = "Construct Charter Ward 1",
                        Status = "active",
                        FinishesAtUtc = DateTime.UtcNow.AddMinutes(4)
                    },
                    new CityTimerEntrySnapshot
                    {
                        Id = "expansion_1",
                        Category = "expansion",
                        Label = "Expand to tier 2",
                        Status = "active",
                        FinishesAtUtc = DateTime.UtcNow.AddMinutes(8)
                    }
                }
            };

            var cityTimers = (List<CityTimerEntrySnapshot>)selector.Invoke(null, new object[] { summary, false });

            Assert.That(cityTimers, Is.Empty);
        }

        [Test]
        public void Development_desk_note_suppresses_stale_research_started_status_when_canonical_research_is_active()
        {
            var builder = typeof(CityScreenController).GetMethod("BuildDeskNote", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(builder, Is.Not.Null);

            var state = new SummaryState();
            state.FinishAction("Research started: hush_routes_1");

            var summary = new ShellSummarySnapshot
            {
                HasCity = true,
                City = new CitySummarySnapshot
                {
                    Name = "Black Market Tester",
                    SettlementLane = "black_market",
                    SettlementLaneLabel = "Black Market"
                },
                ActiveResearches = new List<ResearchSnapshot>
                {
                    new ResearchSnapshot
                    {
                        Id = "hush_routes_1",
                        Name = "Hush Routes",
                        Status = "active",
                        Progress = 180,
                        Cost = 240,
                        FinishesAtUtc = DateTime.UtcNow.AddMinutes(20)
                    }
                },
                AvailableTechs = new List<TechOptionSnapshot>
                {
                    new TechOptionSnapshot { Id = "safehouse_network_1", Name = "Safehouse Network" }
                }
            };

            var note = (string)builder.Invoke(null, new object[] { summary, state, true });

            Assert.That(note, Does.Contain("Shadow-book active"));
            Assert.That(note, Does.Contain("Hush Routes"));
            Assert.That(note, Does.Not.Contain("Research started: hush_routes_1"));
        }

        [Test]
        public void Development_desk_note_drops_old_research_started_status_after_research_is_no_longer_active()
        {
            var builder = typeof(CityScreenController).GetMethod("BuildDeskNote", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(builder, Is.Not.Null);

            var state = new SummaryState();
            state.FinishAction("Research started: caravan_trails_1");

            var summary = new ShellSummarySnapshot
            {
                HasCity = true,
                City = new CitySummarySnapshot
                {
                    Name = "Black Market Tester",
                    SettlementLane = "black_market",
                    SettlementLaneLabel = "Black Market"
                },
                AvailableTechs = new List<TechOptionSnapshot>
                {
                    new TechOptionSnapshot { Id = "black_market_contacts_1", Name = "Black Market Contacts" }
                }
            };

            var note = (string)builder.Invoke(null, new object[] { summary, state, true });

            Assert.That(note, Does.Contain("Shadow books:"));
            Assert.That(note, Does.Not.Contain("Research started: caravan_trails_1"));
        }

        [Test]
        public void Development_building_card_labels_standing_buildings_as_built_not_active_action()
        {
            var builder = typeof(CityScreenController).GetMethod("BuildBuildingStatusButtonText", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(builder, Is.Not.Null);

            var building = new BuildingSnapshot
            {
                Id = "low_quarter",
                Name = "Low Quarter",
                Lane = "city",
                Status = "active",
                Level = 1
            };

            var label = (string)builder.Invoke(null, new object[] { building, false, DateTime.UtcNow });

            Assert.That(label, Is.EqualTo("Built"));
        }

        [Test]
        public void Development_building_lane_copy_distinguishes_standing_cards_from_timed_builds()
        {
            var builder = typeof(CityScreenController).GetMethod("DescribeBuildingLane", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(builder, Is.Not.Null);

            var summary = new ShellSummarySnapshot
            {
                HasCity = true,
                City = new CitySummarySnapshot
                {
                    Name = "City Tester",
                    SettlementLane = "city",
                    SettlementLaneLabel = "City"
                },
                Buildings = new List<BuildingSnapshot>
                {
                    new BuildingSnapshot { Id = "low_quarter", Name = "Low Quarter", Lane = "city", Status = "active" },
                    new BuildingSnapshot { Id = "farmlands", Name = "Outer Farmlands", Lane = "city", Status = "active" }
                }
            };

            var text = (string)builder.Invoke(null, new object[] { summary, false });

            Assert.That(text, Does.Contain("2 standing"));
            Assert.That(text, Does.Not.Contain("2 active"));
        }

        [Test]
        public void Mapper_captures_building_routing_preference_for_switch_controls()
        {
            const string payload = @"{
                ""buildings"": [
                    { ""id"": ""bld_1"", ""kind"": ""housing"", ""name"": ""Low Quarter"", ""routingPreference"": ""prefer_local"" }
                ]
            }";

            var summary = ShellSummarySnapshotMapper.Map(payload);

            Assert.That(summary.Buildings, Has.Count.EqualTo(1));
            Assert.That(summary.Buildings[0].RoutingPreference, Is.EqualTo("prefer_local"));
        }

        [Test]
        public void Development_build_options_prioritize_missing_building_types_for_player_choice()
        {
            var selector = typeof(CityScreenController).GetMethod("SelectBuildOptions", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(selector, Is.Not.Null);

            var buildings = new List<BuildingSnapshot>
            {
                new BuildingSnapshot { Type = "housing", Name = "Low Quarter" },
                new BuildingSnapshot { Type = "housing", Name = "Low Quarter" },
                new BuildingSnapshot { Type = "farmland", Name = "Outer Farmlands" }
            };

            var options = ((System.Collections.IEnumerable)selector.Invoke(null, new object[] { false, buildings })).Cast<object>().ToList();
            var kind = options[0].GetType().GetProperty("Kind")?.GetValue(options[0]) as string;

            Assert.That(kind, Is.EqualTo("mine"));
        }


        [Test]
        public void Development_building_inventory_note_does_not_present_routing_as_destroy_or_switch()
        {
            var builder = typeof(CityScreenController).GetMethod("BuildBuildingInventoryNote", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(builder, Is.Not.Null);

            var summary = new ShellSummarySnapshot { EffectiveBuildingSlots = 8 };
            var buildings = new List<BuildingSnapshot>
            {
                new BuildingSnapshot { Id = "low_quarter", Type = "housing", Name = "Low Quarter", Status = "active" }
            };
            var timers = new List<CityTimerEntrySnapshot>();

            var note = (string)builder.Invoke(null, new object[] { summary, buildings, timers, false, DateTime.UtcNow });

            Assert.That(note, Does.Contain("Only unlocked, affordable"));
            Assert.That(note, Does.Contain("1 manageable building"));
            Assert.That(note, Does.Contain("7 open of 8 building slots"));
            Assert.That(note, Does.Contain("backend confirm-token"));
            Assert.That(note, Does.Not.Contain("Route:"));
        }

        [Test]
        public void Mapper_captures_building_slot_capacity_aliases()
        {
            const string payload = @"{
                ""hasCity"": true,
                ""effectiveBuildingSlots"": 8,
                ""maxBuildingSlots"": 10,
                ""city"": { ""name"": ""Slot City"", ""settlementLane"": ""city"" }
            }";

            var summary = ShellSummarySnapshotMapper.Map(payload);

            Assert.That(summary.EffectiveBuildingSlots, Is.EqualTo(8));
            Assert.That(summary.MaxBuildingSlots, Is.EqualTo(10));
        }

        [Test]
        public void Development_building_selector_lists_all_completed_manageable_buildings()
        {
            var selector = typeof(CityScreenController).GetMethod("SelectManageableBuildings", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(selector, Is.Not.Null);

            var now = DateTime.UtcNow;
            var buildings = new List<BuildingSnapshot>
            {
                new BuildingSnapshot { Id = "b_housing_1", Type = "housing", Name = "Low Quarter", Status = "active", Slot = 2 },
                new BuildingSnapshot { Id = "b_farm_1", Type = "farmland", Name = "Outer Farmlands", Status = "active", Slot = 1 },
                new BuildingSnapshot { Id = "build_active_1", Type = "mine", Name = "Mine Dig", Status = "construct", StartedAtUtc = now.AddMinutes(-1), FinishesAtUtc = now.AddMinutes(5) }
            };

            var manageable = ((System.Collections.IEnumerable)selector.Invoke(null, new object[] { buildings, now })).Cast<BuildingSnapshot>().ToList();

            Assert.That(manageable, Has.Count.EqualTo(2));
            Assert.That(manageable[0].Id, Is.EqualTo("b_farm_1"));
            Assert.That(manageable[1].Id, Is.EqualTo("b_housing_1"));
        }

        [Test]
        public void Development_build_options_hide_locked_and_unaffordable_city_targets()
        {
            var selector = typeof(CityScreenController).GetMethod("SelectCurrentlyBuildableOptions", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(selector, Is.Not.Null);

            var summary = new ShellSummarySnapshot
            {
                Resources = new ResourceSnapshot
                {
                    Materials = 65,
                    Wealth = 100,
                    Mana = 0
                },
                ResearchedTechIds = new List<string> { "urban_planning_1" }
            };

            var options = ((System.Collections.IEnumerable)selector.Invoke(null, new object[] { summary, false, new List<BuildingSnapshot>() })).Cast<object>().ToList();
            var kinds = options.Select(option => option.GetType().GetProperty("Kind")?.GetValue(option) as string).ToList();

            Assert.That(kinds, Does.Contain("housing"));
            Assert.That(kinds, Does.Contain("farmland"));
            Assert.That(kinds, Does.Contain("hall_of_records"));
            Assert.That(kinds, Does.Not.Contain("mine"));
            Assert.That(kinds, Does.Not.Contain("arcane_spire"));
            Assert.That(kinds, Does.Not.Contain("watch_barracks"));
            Assert.That(kinds, Does.Not.Contain("provincial_office"));
        }

        [Test]
        public void Development_front_options_hide_locked_black_market_depth_targets()
        {
            var selector = typeof(CityScreenController).GetMethod("SelectCurrentlyBuildableOptions", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(selector, Is.Not.Null);

            var lockedSummary = new ShellSummarySnapshot
            {
                Resources = new ResourceSnapshot
                {
                    Materials = 100,
                    Wealth = 100,
                    Mana = 100
                },
                ResearchedTechIds = new List<string>()
            };

            var lockedOptions = ((System.Collections.IEnumerable)selector.Invoke(null, new object[] { lockedSummary, true, new List<BuildingSnapshot>() })).Cast<object>().ToList();
            var lockedKinds = lockedOptions.Select(option => option.GetType().GetProperty("Kind")?.GetValue(option) as string).ToList();

            Assert.That(lockedKinds, Does.Contain("safehouse"));
            Assert.That(lockedKinds, Does.Not.Contain("front_house"));
            Assert.That(lockedKinds, Does.Not.Contain("debt_house"));
            Assert.That(lockedKinds, Does.Not.Contain("cutout_bureau"));

            lockedSummary.ResearchedTechIds.Add("front_businesses_1");
            var unlockedOptions = ((System.Collections.IEnumerable)selector.Invoke(null, new object[] { lockedSummary, true, new List<BuildingSnapshot>() })).Cast<object>().ToList();
            var unlockedKinds = unlockedOptions.Select(option => option.GetType().GetProperty("Kind")?.GetValue(option) as string).ToList();

            Assert.That(unlockedKinds, Does.Contain("front_house"));
            Assert.That(unlockedKinds, Does.Not.Contain("debt_house"));
        }



        [Test]
        public void Summary_resource_tick_countdown_rolls_forward_when_payload_anchor_is_stale()
        {
            var staleNext = DateTime.UtcNow.AddMinutes(-5);
            var timing = new TimerSnapshot
            {
                TickMs = 60_000,
                LastTickAtUtc = staleNext.AddMinutes(-1),
                NextTickAtUtc = staleNext
            };

            var resolver = typeof(SummaryScreenController).GetMethod("ResolveNextTickAtUtc", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(resolver, Is.Not.Null);

            var resolved = (DateTime?)resolver.Invoke(null, new object[] { timing, TimeSpan.FromMinutes(1) });

            Assert.That(resolved.HasValue, Is.True);
            Assert.That(resolved.Value, Is.GreaterThan(DateTime.UtcNow));
        }


        [Test]
        public void Client_bootstrap_treats_elapsed_resource_tick_as_timed_refresh_trigger()
        {
            var timing = new TimerSnapshot
            {
                TickMs = 60_000,
                LastTickAtUtc = DateTime.UtcNow.AddMinutes(-2),
                NextTickAtUtc = DateTime.UtcNow.AddMinutes(-1)
            };

            var checker = typeof(PlanarWar.Client.UI.ClientBootstrap).GetMethod("HasResourceTickElapsed", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(checker, Is.Not.Null);

            var elapsed = (bool)checker.Invoke(null, new object[] { timing, DateTime.UtcNow });

            Assert.That(elapsed, Is.True);
        }


        [Test]
        public void Building_confirm_state_tracks_destroy_remodel_and_cancel_tokens()
        {
            var state = new SummaryState();

            state.MarkBuildingConfirmRequired("destroy", "destroy:b_housing_1", buildingId: "b_housing_1");
            Assert.That(state.HasPendingBuildingConfirm("destroy", "b_housing_1"), Is.True);
            Assert.That(state.GetPendingBuildingConfirmToken("destroy", "b_housing_1"), Is.EqualTo("destroy:b_housing_1"));
            Assert.That(state.HasPendingBuildingConfirm("destroy", "b_mine_1"), Is.False);

            state.MarkBuildingConfirmRequired("remodel", "remodel:b_housing_1:farmland", buildingId: "b_housing_1", targetKind: "farmland");
            Assert.That(state.HasPendingBuildingConfirm("remodel", "b_housing_1", "farmland"), Is.True);
            Assert.That(state.GetPendingBuildingConfirmToken("remodel", "b_housing_1", "mine"), Is.Empty);

            state.MarkBuildingConfirmRequired("cancel_build", "cancel_build:build_1", activeBuildId: "build_1");
            Assert.That(state.HasPendingBuildingConfirm("cancel_build", activeBuildId: "build_1"), Is.True);

            state.ClearBuildingConfirm();
            Assert.That(state.HasPendingBuildingConfirm("cancel_build", activeBuildId: "build_1"), Is.False);
            Assert.That(state.PendingBuildingConfirmToken, Is.Empty);
        }

        [Test]
        public void Mapper_captures_mission_board_offers_from_missions_payload()
        {
            const string payload = @"{
                ""missions"": [
                    {
                        ""id"": ""counterfeit_trace_1"",
                        ""title"": ""Trace Counterfeit Scrip"",
                        ""kind"": ""hero"",
                        ""regionId"": ""heartland_basin"",
                        ""boardCategory"": ""counterfeit"",
                        ""difficulty"": ""normal"",
                        ""summary"": ""Follow the counterfeit report chain before it cools.""
                    }
                ]
            }";

            var summary = ShellSummarySnapshotMapper.Map(payload);

            Assert.That(summary.MissionOffers, Has.Count.EqualTo(1));
            Assert.That(summary.MissionOffers[0].Id, Is.EqualTo("counterfeit_trace_1"));
            Assert.That(summary.MissionOffers[0].Title, Is.EqualTo("Trace Counterfeit Scrip"));
            Assert.That(summary.MissionOffers[0].BoardCategory, Is.EqualTo("counterfeit"));
        }


        [Test]
        public void Mapper_captures_active_mission_timer_and_effect_copy()
        {
            const string payload = @"{
                ""activeMissions"": [
                    {
                        ""id"": ""lair_strike_1"",
                        ""title"": ""Lair Strike: Heartland Basin"",
                        ""instanceId"": ""mission_123"",
                        ""regionId"": ""heartland_basin"",
                        ""assignedArmyId"": ""army_1"",
                        ""summary"": ""Hit a minor lair before it fortifies."",
                        ""payoff"": ""materials and control pressure"",
                        ""risk"": ""readiness loss"",
                        ""finishesAt"": ""2099-01-01T00:05:00Z""
                    }
                ],
                ""armies"": [
                    { ""id"": ""army_1"", ""name"": ""First Tempest Cell"" }
                ]
            }";

            var summary = ShellSummarySnapshotMapper.Map(payload);

            Assert.That(summary.ActiveMissions, Has.Count.EqualTo(1));
            Assert.That(summary.ActiveMissions[0].Title, Is.EqualTo("Lair Strike: Heartland Basin"));
            Assert.That(summary.ActiveMissions[0].AssignedArmyName, Is.EqualTo("First Tempest Cell"));
            Assert.That(summary.ActiveMissions[0].Payoff, Does.Contain("control pressure"));
            Assert.That(summary.ActiveMissions[0].Risk, Does.Contain("readiness"));
            Assert.That(summary.ActiveMissions[0].FinishesAtUtc, Is.Not.Null);
        }

        [Test]
        public void Client_bootstrap_treats_elapsed_active_mission_as_timed_refresh_trigger()
        {
            var checker = typeof(PlanarWar.Client.UI.ClientBootstrap).GetMethod("HasAnyMissionElapsed", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(checker, Is.Not.Null);

            var missions = new List<MissionSnapshot>
            {
                new MissionSnapshot
                {
                    InstanceId = "mission_123",
                    Title = "Lair Strike",
                    FinishesAtUtc = DateTime.UtcNow.AddSeconds(-1)
                }
            };

            var elapsed = (bool)checker.Invoke(null, new object[] { missions, DateTime.UtcNow });

            Assert.That(elapsed, Is.True);
        }

        [Test]
        public void Summary_state_stores_mission_offers_from_refresh_side_payload()
        {
            var state = new SummaryState();
            var offers = new[]
            {
                new MissionOfferSnapshot { Id = "relief_1", Title = "Relief Convoy", BoardCategory = "relief" }
            };

            state.ApplySnapshot(new ShellSummarySnapshot(), missionOffers: offers);

            Assert.That(state.MissionOffers, Has.Count.EqualTo(1));
            Assert.That(state.MissionOffers[0].Id, Is.EqualTo("relief_1"));
        }

        [Test]
        public void Summary_state_resolves_operations_action_labels_from_loaded_snapshot_truth()
        {
            var state = new SummaryState();
            state.ApplySnapshot(new ShellSummarySnapshot
            {
                MissionOffers = new List<MissionOfferSnapshot>
                {
                    new MissionOfferSnapshot { Id = "counterfeit_trace_1", Title = "Trace Counterfeit Scrip", RegionId = "heartland_basin" }
                },
                ActiveMissions = new List<MissionSnapshot>
                {
                    new MissionSnapshot
                    {
                        InstanceId = "mission_123",
                        Title = "Lair Strike: Heartland Basin",
                        AssignedArmyId = "army_1",
                        AssignedHeroId = "hero_1"
                    }
                },
                Armies = new List<ArmySnapshot>
                {
                    new ArmySnapshot { Id = "army_1", Name = "First Tempest Cell" }
                },
                Heroes = new List<HeroSnapshot>
                {
                    new HeroSnapshot { Id = "hero_1", Name = "Lyra of the Veiled Paths" }
                }
            });

            Assert.That(state.ResolveMissionOfferReceiptLabel("counterfeit_trace_1"), Is.EqualTo("Trace Counterfeit Scrip"));
            Assert.That(state.ResolveMissionInstanceReceiptLabel("mission_123"), Is.EqualTo("Lair Strike: Heartland Basin"));
            Assert.That(state.ResolveArmyReceiptLabel("army_1"), Is.EqualTo("First Tempest Cell"));
            Assert.That(state.ResolveHeroReceiptLabel("hero_1"), Is.EqualTo("Lyra of the Veiled Paths"));
            Assert.That(state.ResolveRegionReceiptLabel("heartland_basin"), Is.EqualTo("Heartland Basin"));
            Assert.That(SummaryState.ResolvePostureReceiptLabel("frontier_hold"), Is.EqualTo("Frontier Hold"));

            state.BeginMissionStartAction("counterfeit_trace_1");
            Assert.That(state.ActionStatus, Is.EqualTo("Starting mission: Trace Counterfeit Scrip"));
            Assert.That(state.ActionStatus, Does.Not.Contain("counterfeit_trace_1"));
            state.FinishAction("done");

            state.BeginArmyHoldAssign("army_1", "heartland_basin", "frontier_hold");
            Assert.That(state.ActionStatus, Does.Contain("First Tempest Cell"));
            Assert.That(state.ActionStatus, Does.Contain("Heartland Basin"));
            Assert.That(state.ActionStatus, Does.Contain("Frontier Hold"));
            Assert.That(state.ActionStatus, Does.Not.Contain("army_1"));
            Assert.That(state.ActionStatus, Does.Not.Contain("heartland_basin"));
            Assert.That(state.ActionStatus, Does.Not.Contain("frontier_hold"));
        }

        [Test]
        public void Summary_state_humanizes_operations_action_label_fallbacks_without_inventing_snapshot_truth()
        {
            var state = new SummaryState();
            state.ApplySnapshot(new ShellSummarySnapshot());

            Assert.That(state.ResolveMissionOfferReceiptLabel("counterfeit_trace_1"), Is.EqualTo("Counterfeit Trace 1"));
            Assert.That(state.ResolveArmyReceiptLabel("army_1"), Is.EqualTo("Army 1"));
            Assert.That(state.ResolveHeroReceiptLabel("hero_1"), Is.EqualTo("Hero 1"));
            Assert.That(state.ResolveRegionReceiptLabel("heartland_basin"), Is.EqualTo("Heartland Basin"));
            Assert.That(SummaryState.ResolvePostureReceiptLabel("balanced_response"), Is.EqualTo("Balanced Response"));
        }

        [Test]
        public void Summary_state_resolves_development_action_labels_from_loaded_snapshot_truth()
        {
            var state = new SummaryState();
            state.ApplySnapshot(new ShellSummarySnapshot
            {
                AvailableTechs = new List<TechOptionSnapshot>
                {
                    new TechOptionSnapshot { Id = "civic_foundation_v1", Name = "Civic Foundation" }
                },
                ActiveResearches = new List<ResearchSnapshot>
                {
                    new ResearchSnapshot { Id = "urban_planning_1", Name = "Urban Planning I" }
                },
                Buildings = new List<BuildingSnapshot>
                {
                    new BuildingSnapshot { Id = "building_abc123", BuildingId = "housing", Type = "housing", Name = "Charter Ward", Level = 2 },
                    new BuildingSnapshot { Id = "front_1", BuildingId = "safehouse", Type = "safehouse", Name = "Safehouse Ring" }
                },
                CityTimers = new List<CityTimerEntrySnapshot>
                {
                    new CityTimerEntrySnapshot { Id = "active_build_1", Label = "Beacon Tower" }
                }
            });

            Assert.That(state.ResolveResearchReceiptLabel("civic_foundation_v1"), Is.EqualTo("Civic Foundation"));
            Assert.That(state.ResolveResearchReceiptLabel("urban_planning_1"), Is.EqualTo("Urban Planning I"));
            Assert.That(state.ResolveBuildingKindReceiptLabel("safehouse"), Is.EqualTo("Safehouse Ring"));
            Assert.That(state.ResolveBuildingReceiptLabel("building_abc123"), Is.EqualTo("Charter Ward"));
            Assert.That(state.ResolveBuildingReceiptLabel("front_1"), Is.EqualTo("Safehouse Ring"));
            Assert.That(state.ResolveActiveBuildReceiptLabel("active_build_1"), Is.EqualTo("Beacon Tower"));
            Assert.That(SummaryState.ResolveBuildingRoutingReceiptLabel("prefer_reserve"), Is.EqualTo("Reserve • protected stock"));

            state.BeginResearchAction("civic_foundation_v1");
            Assert.That(state.ActionStatus, Is.EqualTo("Starting research: Civic Foundation"));
            Assert.That(state.ActionStatus, Does.Not.Contain("civic_foundation_v1"));
            state.FinishAction("done");

            state.BeginBuildingRouting("building_abc123", "prefer_reserve");
            Assert.That(state.ActionStatus, Does.Contain("Charter Ward"));
            Assert.That(state.ActionStatus, Does.Contain("Reserve • protected stock"));
            Assert.That(state.ActionStatus, Does.Not.Contain("building_abc123"));
            Assert.That(state.ActionStatus, Does.Not.Contain("prefer_reserve"));
        }

        [Test]
        public void Summary_state_humanizes_development_action_label_fallbacks_without_inventing_snapshot_truth()
        {
            var state = new SummaryState();
            state.ApplySnapshot(new ShellSummarySnapshot());

            Assert.That(state.ResolveResearchReceiptLabel("civic_foundation_v1"), Is.EqualTo("Civic Foundation V1"));
            Assert.That(state.ResolveBuildingKindReceiptLabel("quiet_provisioning"), Is.EqualTo("Quiet Provisioning Cell"));
            Assert.That(state.ResolveBuildingReceiptLabel("building_abc123"), Is.EqualTo("Building Abc123"));
            Assert.That(state.ResolveActiveBuildReceiptLabel("active_build_1"), Is.EqualTo("Active Build 1"));
            Assert.That(SummaryState.ResolveBuildingRoutingReceiptLabel("exchange"), Is.EqualTo("Exchange • trade flow"));
        }

        [Test]
        public void Operations_action_receipt_cleanup_uses_summary_label_resolvers_without_hardcoded_client_routes()
        {
            var bootstrapPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/ClientBootstrap.cs");
            Assert.That(File.Exists(bootstrapPath), Is.True, "ClientBootstrap.cs should be available from the Unity project root.");

            var bootstrap = File.ReadAllText(bootstrapPath);
            foreach (var marker in new[]
            {
                "ResolveMissionOfferReceiptLabel",
                "ResolveMissionInstanceReceiptLabel",
                "ResolveArmyReceiptLabel",
                "ResolveHeroReceiptLabel",
                "ResolveRegionReceiptLabel",
                "ResolvePostureReceiptLabel",
            })
            {
                Assert.That(bootstrap, Does.Contain(marker), $"Operations receipt cleanup should route visible action text through {marker}.");
            }

            Assert.That(bootstrap, Does.Not.Contain("Mission started: {trimmedMissionId}"));
            Assert.That(bootstrap, Does.Not.Contain("Cell reinforcement started: {trimmedArmyId}"));
            Assert.That(bootstrap, Does.Not.Contain("Formation merged into {trimmedTargetArmyId}"));
            Assert.That(bootstrap, Does.Not.Contain("Formation disbanded: {trimmedArmyId}"));
            Assert.That(bootstrap, Does.Not.Contain("Regional hold assigned: {trimmedRegionId}"));
            Assert.That(bootstrap, Does.Not.Contain("Regional hold released: {trimmedArmyId}"));
            Assert.That(bootstrap, Does.Not.Contain("Pressure deployment opened for {trimmedRegionId} with {trimmedArmyId}"));
            Assert.That(bootstrap, Does.Not.Contain("Disruption action opened for {trimmedRegionId} with {trimmedArmyId}"));
        }

        [Test]
        public void Development_action_receipt_cleanup_uses_summary_label_resolvers_without_hardcoded_client_routes()
        {
            var bootstrapPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/ClientBootstrap.cs");
            Assert.That(File.Exists(bootstrapPath), Is.True, "ClientBootstrap.cs should be available from the Unity project root.");

            var bootstrap = File.ReadAllText(bootstrapPath);
            foreach (var marker in new[]
            {
                "ResolveResearchReceiptLabel",
                "ResolveBuildingKindReceiptLabel",
                "ResolveBuildingReceiptLabel",
                "ResolveActiveBuildReceiptLabel",
                "ResolveBuildingRoutingReceiptLabel",
            })
            {
                Assert.That(bootstrap, Does.Contain(marker), $"Development receipt cleanup should route visible action text through {marker}.");
            }

            Assert.That(bootstrap, Does.Not.Contain("Research started: {trimmedTechId}"));
            Assert.That(bootstrap, Does.Not.Contain("Construction started: {trimmedKind}"));
            Assert.That(bootstrap, Does.Not.Contain("Building upgrade started: {trimmedBuildingId}"));
            Assert.That(bootstrap, Does.Not.Contain("Building routing switched: {trimmedBuildingId} -> {trimmedRoutingPreference}"));
            Assert.That(bootstrap, Does.Not.Contain("Building demolished: {trimmedBuildingId}"));
            Assert.That(bootstrap, Does.Not.Contain("Building remodel started: {trimmedBuildingId} -> {trimmedTargetKind}"));
            Assert.That(bootstrap, Does.Not.Contain("Active building project canceled: {trimmedActiveBuildId}"));
        }

        [Test]
        public void Summary_state_resolves_pending_action_labels_for_workshop_and_hero_surfaces()
        {
            var state = new SummaryState();
            state.ApplySnapshot(new ShellSummarySnapshot
            {
                WorkshopJobs = new List<WorkshopJobSnapshot>
                {
                    new WorkshopJobSnapshot
                    {
                        Id = "job_arcane_focus_1",
                        RecipeId = "recipe_arcane_focus_1",
                        OutputItemId = "workshop_arcane_focus_1",
                        AttachmentKind = "workshop_job",
                    }
                },
                Heroes = new List<HeroSnapshot>
                {
                    new HeroSnapshot { Id = "hero_1", Name = "Lyra of the Veiled Paths" }
                },
                HeroRecruitment = new HeroRecruitmentSnapshot
                {
                    Role = "provost",
                    StartRole = "provost",
                    Candidates = new List<HeroRecruitCandidateSnapshot>
                    {
                        new HeroRecruitCandidateSnapshot
                        {
                            CandidateId = "candidate_1",
                            DisplayName = "Provost Sel Varo",
                            ClassName = "Tactician",
                            Role = "provost",
                        }
                    }
                },
                HeroArmoryBridge = new HeroArmoryBridgeSnapshot
                {
                    ArmoryItems = new List<HeroArmoryItemSnapshot>
                    {
                        new HeroArmoryItemSnapshot
                        {
                            SlotIndex = 0,
                            ItemId = "arcane_focus",
                            Template = new HeroEquipmentTemplateSnapshot
                            {
                                Name = "Arcane Focus",
                                Slot = "offhand",
                            }
                        }
                    }
                }
            },
            workshopRecipes: new[]
            {
                new WorkshopRecipeSnapshot
                {
                    RecipeId = "recipe_arcane_focus_1",
                    Name = "Arcane Focus",
                    OutputItemId = "workshop_arcane_focus_1",
                }
            });

            Assert.That(state.ResolveWorkshopRecipeReceiptLabel("recipe_arcane_focus_1"), Is.EqualTo("Arcane Focus"));
            Assert.That(state.ResolveWorkshopJobReceiptLabel("job_arcane_focus_1"), Is.EqualTo("Arcane Focus"));
            Assert.That(state.ResolveHeroRecruitCandidateReceiptLabel("candidate_1"), Is.EqualTo("Provost Sel Varo"));
            Assert.That(state.ResolveHeroArmoryItemReceiptLabel(0), Is.EqualTo("Arcane Focus"));
            Assert.That(state.ResolveHeroEquipPendingActionLabel("hero_1", 0), Is.EqualTo("Equipping Arcane Focus to Lyra of the Veiled Paths"));
            Assert.That(state.ResolveHeroUnequipPendingActionLabel("hero_1", "offhand"), Is.EqualTo("Returning Off Hand gear from Lyra of the Veiled Paths to armory"));

            state.BeginWorkshopCraft("recipe_arcane_focus_1");
            Assert.That(state.ActionStatus, Is.EqualTo("Starting workshop craft: Arcane Focus"));
            Assert.That(state.ActionStatus, Does.Not.Contain("recipe_arcane_focus_1"));
            state.FinishAction("done");

            state.BeginWorkshopCollect("job_arcane_focus_1");
            Assert.That(state.ActionStatus, Is.EqualTo("Collecting workshop item: Arcane Focus"));
            Assert.That(state.ActionStatus, Does.Not.Contain("job_arcane_focus_1"));
            state.FinishAction("done");

            state.BeginHeroRecruitAccept("candidate_1");
            Assert.That(state.ActionStatus, Does.Contain("Provost Sel Varo"));
            Assert.That(state.ActionStatus, Does.Not.Contain("candidate_1"));
            state.FinishAction("done");

            state.BeginHeroRelease("hero_1");
            Assert.That(state.ActionStatus, Does.Contain("Lyra of the Veiled Paths"));
            Assert.That(state.ActionStatus, Does.Not.Contain("hero_1"));
            state.FinishAction("done");

            state.BeginHeroEquipFromArmory("hero_1", 0);
            Assert.That(state.ActionStatus, Is.EqualTo("Equipping Arcane Focus to Lyra of the Veiled Paths"));
            Assert.That(state.ActionStatus, Does.Not.Contain("hero_1"));
            Assert.That(state.ActionStatus, Does.Not.Contain("armory slot"));
        }

        [Test]
        public void Pending_action_status_cleanup_removes_raw_id_busy_text_from_runtime_state()
        {
            var summaryStatePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Core/SummaryState.cs");
            Assert.That(File.Exists(summaryStatePath), Is.True, "SummaryState.cs should be available from the Unity project root.");

            var stateSource = File.ReadAllText(summaryStatePath);
            foreach (var marker in new[]
            {
                "ResolveWorkshopRecipeReceiptLabel",
                "ResolveWorkshopJobReceiptLabel",
                "ResolveHeroRecruitCandidateReceiptLabel",
                "ResolveHeroEquipPendingActionLabel",
                "ResolveHeroUnequipPendingActionLabel",
            })
            {
                Assert.That(stateSource, Does.Contain(marker), $"Pending action status text should route through {marker}.");
            }

            Assert.That(stateSource, Does.Not.Contain("Starting workshop craft: {recipeId.Trim()}"));
            Assert.That(stateSource, Does.Not.Contain("Collecting workshop job: {jobId.Trim()}"));
            Assert.That(stateSource, Does.Not.Contain("Completing mission: {instanceId.Trim()}"));
            Assert.That(stateSource, Does.Not.Contain("Accepting hero candidate: {candidateId.Trim()}"));
            Assert.That(stateSource, Does.Not.Contain("Releasing hero: {heroId.Trim()}"));
            Assert.That(stateSource, Does.Not.Contain("Equipping shared gear: {heroId.Trim()} <- armory slot {armorySlotIndex}"));
            Assert.That(stateSource, Does.Not.Contain("Returning shared gear: {heroId.Trim()} {slot}"));
        }

        [Test]
        public void Summary_state_formats_mission_completion_receipt_from_reward_payload()
        {
            const string response = "{ \"summary\": \"Raid resolved cleanly.\", \"rewards\": { \"wealth\": 15, \"materials\": 4 }, \"effects\": { \"controlDelta\": 2, \"threatDelta\": -1 } }";

            var receipt = SummaryState.FormatMissionCompletionReceipt(response, "mission_1");

            Assert.That(receipt, Does.Contain("Mission completed"));
            Assert.That(receipt, Does.Contain("Raid resolved cleanly"));
            Assert.That(receipt, Does.Contain("wealth +15"));
            Assert.That(receipt, Does.Contain("materials +4"));
            Assert.That(receipt, Does.Contain("control +2"));
        }

        [Test]
        public void Summary_state_formats_nested_backend_mission_completion_result_readably()
        {
            const string response = @"{
                ""ok"": true,
                ""result"": {
                    ""status"": ""ok"",
                    ""rewards"": { ""wealth"": 12, ""materials"": 8, ""influence"": 0 },
                    ""outcome"": { ""kind"": ""success"", ""score"": 1.2 },
                    ""receipt"": {
                        ""id"": ""receipt_1"",
                        ""missionId"": ""frontline_1"",
                        ""missionTitle"": ""Frontline Assault: Heartland Basin"",
                        ""createdAt"": ""2026-04-26T08:20:12.387Z"",
                        ""outcome"": ""success"",
                        ""posture"": ""balanced"",
                        ""summary"": ""Frontline assault pushed hostile pressure back."",
                        ""setbacks"": []
                    }
                },
                ""resources"": { ""wealth"": 123, ""materials"": 44 }
            }";

            var receipt = SummaryState.FormatMissionCompletionReceipt(response, "active_1");
            var title = SummaryState.ExtractMissionCompletionTitle(response);

            Assert.That(receipt, Does.Contain("Mission: Frontline Assault: Heartland Basin"));
            Assert.That(receipt, Does.Contain("Outcome: Success"));
            Assert.That(receipt, Does.Contain("Rewards: wealth +12, materials +8"));
            Assert.That(receipt, Does.Contain("Summary: Frontline assault pushed hostile pressure back."));
            Assert.That(receipt, Does.Not.Contain("created at"));
            Assert.That(receipt, Does.Not.Contain("mission title"));
            Assert.That(title, Is.EqualTo("Frontline Assault: Heartland Basin"));
        }

        [Test]
        public void Summary_state_formats_sparse_mission_completion_as_a_status_report()
        {
            const string response = @"{ ""ok"": true, ""result"": { ""status"": ""ok"" } }";

            var receipt = SummaryState.FormatMissionCompletionReceipt(response, "Contain the Fallout in Heartland Basin — Escalation 2");

            Assert.That(receipt, Does.Contain("Mission: Contain the Fallout in Heartland Basin — Escalation 2"));
            Assert.That(receipt, Does.Contain("Status: Completion accepted"));
            Assert.That(receipt, Does.Contain("Rewards: no direct resource reward returned"));
            Assert.That(receipt, Does.Not.Contain("active_"));
        }

        [Test]
        public void Operations_recent_mission_receipt_is_promoted_into_the_mission_board()
        {
            var screenPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/BlackMarket/BlackMarketScreenController.cs");
            Assert.That(File.Exists(screenPath), Is.True, "BlackMarketScreenController.cs should be available from the Unity project root.");

            var screen = File.ReadAllText(screenPath);
            Assert.That(screen, Does.Contain("RenderRecentMissionReceiptBoard"));
            Assert.That(screen, Does.Contain("Latest completion report is shown before the next mission offer"));
            Assert.That(screen, Does.Contain("BuildRecentMissionReceiptReportBody(summaryState?.RecentMissionReceipt)"));
            Assert.That(screen, Does.Contain("Report received"));
            Assert.That(screen, Does.Not.Contain("Receipt visible"));
        }

        [Test]
        public void Operations_recent_mission_receipt_body_keeps_full_report_readable()
        {
            var method = typeof(PlanarWar.Client.UI.Screens.BlackMarket.BlackMarketScreenController)
                .GetMethod("BuildRecentMissionReceiptReportBody", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "Completion reports should be formatted as a readable board body instead of a clipped one-line receipt.");

            const string receipt = "Mission completed. Mission: Frontline Assault: Heartland Basin • Outcome: Success • Rewards: wealth +60, materials +80, influence +5 • Summary: Frontline Assault: Heartland Basin: SUCCESS with balanced posture. Field payoff: TestTest vanguard pressure was pushed back and the full completion report should remain readable.";

            var body = (string)method.Invoke(null, new object[] { receipt });

            Assert.That(body, Does.Contain("Mission completed."));
            Assert.That(body, Does.Contain("Mission: Frontline Assault: Heartland Basin"));
            Assert.That(body, Does.Contain("Outcome: Success"));
            Assert.That(body, Does.Contain("Rewards: wealth +60, materials +80, influence +5"));
            Assert.That(body, Does.Contain("the full completion report should remain readable"));
            Assert.That(body, Does.Contain("\nOutcome: Success"));
            Assert.That(body, Does.Not.Contain(" • "));
        }

        [Test]
        public void Summary_state_keeps_recent_mission_receipt_visible_briefly()
        {
            var state = new SummaryState();

            state.FinishMissionCompletion("mission_1", "Mission completed. Rewards: wealth +15.", "Lair Strike");

            Assert.That(state.HasRecentMissionReceipt(DateTime.UtcNow), Is.True);
            Assert.That(state.RecentMissionReceipt, Does.Contain("wealth +15"));
            Assert.That(state.RecentMissionTitle, Is.EqualTo("Lair Strike"));
            Assert.That(state.RecentMissionInstanceId, Is.EqualTo("mission_1"));
        }


        [Test]
        public void Summary_state_formats_hero_acceptance_receipt_readably()
        {
            const string response = @"{
                ""ok"": true,
                ""result"": {
                    ""status"": ""accepted"",
                    ""displayName"": ""Lyra of the Veiled Paths"",
                    ""className"": ""scout"",
                    ""receipt"": {
                        ""summary"": ""Lyra joined the roster and is ready for assignment.""
                    },
                    ""effects"": { ""rosterDelta"": 1 }
                }
            }";

            var receipt = SummaryState.FormatHeroActionReceipt(response, "Contact recruited", "candidate_1", "Operative");
            var title = SummaryState.ExtractHeroActionTitle(response, "Contact recruited", "Operative");

            Assert.That(receipt, Does.Contain("Contact recruited"));
            Assert.That(receipt, Does.Contain("Outcome: Accepted"));
            Assert.That(receipt, Does.Contain("Operative: Lyra of the Veiled Paths"));
            Assert.That(receipt, Does.Contain("Role: Scout"));
            Assert.That(receipt, Does.Contain("Summary: Lyra joined the roster"));
            Assert.That(receipt, Does.Not.Contain("display name"));
            Assert.That(title, Is.EqualTo("Operative: Lyra of the Veiled Paths"));
        }

        [Test]
        public void Summary_state_keeps_recent_hero_receipt_visible_briefly()
        {
            var state = new SummaryState();

            state.FinishHeroActionReceipt("Hero released", "Hero released. Summary: Ser Kael left the roster.", "Hero: Ser Kael");

            Assert.That(state.HasRecentHeroReceipt(DateTime.UtcNow), Is.True);
            Assert.That(state.RecentHeroReceipt, Does.Contain("Ser Kael"));
            Assert.That(state.RecentHeroReceiptTitle, Is.EqualTo("Hero: Ser Kael"));
            Assert.That(state.RecentHeroReceiptAction, Is.EqualTo("Hero released"));
        }


        [Test]
        public void Hero_recruitment_button_explains_slate_scouting_and_roster_cap()
        {
            var terminologyType = typeof(HeroScreenController).GetNestedType("HeroTerminology", BindingFlags.NonPublic);
            Assert.That(terminologyType, Is.Not.Null);
            var forMethod = terminologyType.GetMethod("For", BindingFlags.Public | BindingFlags.Static);
            var formatter = typeof(HeroScreenController).GetMethod("BuildRecruitmentButtonText", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(forMethod, Is.Not.Null);
            Assert.That(formatter, Is.Not.Null);

            var heroTerms = forMethod.Invoke(null, new object[] { new ShellSummarySnapshot() });
            var openSlateText = (string)formatter.Invoke(null, new object[]
            {
                new HeroRecruitmentSnapshot { Status = "ready", StartEligible = false },
                new List<HeroRecruitCandidateSnapshot> { new HeroRecruitCandidateSnapshot { CandidateId = "candidate_1", DisplayName = "Provost Sel Varo" } },
                heroTerms,
                false,
                false
            });
            var scoutingText = (string)formatter.Invoke(null, new object[]
            {
                new HeroRecruitmentSnapshot { Status = "scouting", StartEligible = false },
                new List<HeroRecruitCandidateSnapshot>(),
                heroTerms,
                false,
                false
            });
            var rosterFullText = (string)formatter.Invoke(null, new object[]
            {
                new HeroRecruitmentSnapshot { Status = "ready", StartEligible = true, CtaLabel = "Open provost recruitment" },
                new List<HeroRecruitCandidateSnapshot>(),
                heroTerms,
                false,
                true
            });

            Assert.That(openSlateText, Is.EqualTo("Recruitment slate open"));
            Assert.That(scoutingText, Is.EqualTo("Recruitment scouting in progress"));
            Assert.That(rosterFullText, Is.EqualTo("Release a hero to recruit"));
            Assert.That(rosterFullText, Does.Not.Contain("blocked"));
        }

        [Test]
        public void Hero_recruitment_description_surfaces_roster_cap_without_fake_backend_fields()
        {
            var terminologyType = typeof(HeroScreenController).GetNestedType("HeroTerminology", BindingFlags.NonPublic);
            Assert.That(terminologyType, Is.Not.Null);
            var forMethod = terminologyType.GetMethod("For", BindingFlags.Public | BindingFlags.Static);
            var formatter = typeof(HeroScreenController).GetMethod("DescribeRecruitment", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(forMethod, Is.Not.Null);
            Assert.That(formatter, Is.Not.Null);

            var heroTerms = forMethod.Invoke(null, new object[] { new ShellSummarySnapshot() });
            var fullRoster = (string)formatter.Invoke(null, new object[]
            {
                new HeroRecruitmentSnapshot { Status = "ready", StartEligible = true },
                new List<HeroRecruitCandidateSnapshot>(),
                heroTerms,
                5,
                true
            });
            var fullRosterWithSlate = (string)formatter.Invoke(null, new object[]
            {
                new HeroRecruitmentSnapshot { Status = "ready", StartEligible = false },
                new List<HeroRecruitCandidateSnapshot> { new HeroRecruitCandidateSnapshot { CandidateId = "candidate_1" } },
                heroTerms,
                5,
                true
            });

            Assert.That(fullRoster, Does.Contain("Hero roster full"));
            Assert.That(fullRoster, Does.Contain("5/5 heroes"));
            Assert.That(fullRosterWithSlate, Does.Contain("1 candidate ready"));
            Assert.That(fullRosterWithSlate, Does.Contain("roster full 5/5 heroes"));
            Assert.That(fullRosterWithSlate, Does.Not.Contain("Recruitment blocked"));
        }

        [Test]
        public void Hero_recruitment_state_copy_keeps_black_market_contact_language()
        {
            var terminologyType = typeof(HeroScreenController).GetNestedType("HeroTerminology", BindingFlags.NonPublic);
            Assert.That(terminologyType, Is.Not.Null);
            var forMethod = terminologyType.GetMethod("For", BindingFlags.Public | BindingFlags.Static);
            var formatter = typeof(HeroScreenController).GetMethod("BuildRecruitmentButtonText", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(forMethod, Is.Not.Null);
            Assert.That(formatter, Is.Not.Null);

            var operativeTerms = forMethod.Invoke(null, new object[]
            {
                new ShellSummarySnapshot
                {
                    City = new CitySummarySnapshot
                    {
                        SettlementLane = "black_market",
                        SettlementLaneLabel = "Black Market"
                    }
                }
            });
            var openSlateText = (string)formatter.Invoke(null, new object[]
            {
                new HeroRecruitmentSnapshot { Status = "ready", StartEligible = false },
                new List<HeroRecruitCandidateSnapshot> { new HeroRecruitCandidateSnapshot { CandidateId = "contact_1" } },
                operativeTerms,
                false,
                false
            });
            var rosterFullText = (string)formatter.Invoke(null, new object[]
            {
                new HeroRecruitmentSnapshot { Status = "ready", StartEligible = true },
                new List<HeroRecruitCandidateSnapshot>(),
                operativeTerms,
                false,
                true
            });

            Assert.That(openSlateText, Is.EqualTo("Contact slate open"));
            Assert.That(rosterFullText, Is.EqualTo("Retire an operative to scout"));
            Assert.That(openSlateText.ToLowerInvariant(), Does.Not.Contain("hero"));
            Assert.That(rosterFullText.ToLowerInvariant(), Does.Not.Contain("hero"));
        }

        [Test]
        public void Hero_screen_recent_roster_receipt_renders_as_readable_report()
        {
            var screenPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/Heroes/HeroScreenController.cs");
            Assert.That(File.Exists(screenPath), Is.True, "HeroScreenController.cs should be available from the Unity project root.");

            var screen = File.ReadAllText(screenPath);
            Assert.That(screen, Does.Contain("BuildRecentHeroReceiptReportBody(summaryState?.RecentHeroReceipt, terms)"));
            Assert.That(screen, Does.Contain("Report received"));
            Assert.That(screen, Does.Contain("Latest {terms.SingularLower} report received"));
            Assert.That(screen, Does.Not.Contain("Truncate(summaryState?.RecentHeroReceipt, 160)"));
            Assert.That(screen, Does.Not.Contain("Receipt visible"));
        }

        [Test]
        public void Hero_screen_recent_roster_receipt_body_keeps_full_report_readable()
        {
            var method = typeof(PlanarWar.Client.UI.Screens.Heroes.HeroScreenController)
                .GetMethod("BuildRecentHeroReceiptReportBody", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "Hero/operative reports should be formatted as readable report bodies instead of clipped one-line receipts.");

            const string receipt = "Hero released. Outcome: Released • Hero: Ser Kael the Stormguard • Gear: returned Training Blade, returned Worn Shield • Effects: roster -1, shared armory +2 • Summary: Ser Kael left the roster cleanly and equipped gear returned through backend truth without inventing extra rewards.";

            var body = (string)method.Invoke(null, new object[] { receipt, null });

            Assert.That(body, Does.Contain("Hero released."));
            Assert.That(body, Does.Contain("Outcome: Released"));
            Assert.That(body, Does.Contain("Hero: Ser Kael the Stormguard"));
            Assert.That(body, Does.Contain("Gear: returned Training Blade, returned Worn Shield"));
            Assert.That(body, Does.Contain("Effects: roster -1, shared armory +2"));
            Assert.That(body, Does.Contain("equipped gear returned through backend truth"));
            Assert.That(body, Does.Contain("\nOutcome: Released"));
            Assert.That(body, Does.Not.Contain(" • "));
        }

        [Test]
        public void Shell_has_hero_armory_controls()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            Assert.That(uxml, Does.Contain("heroes-armory-value"));
            Assert.That(uxml, Does.Contain("heroes-armory-item-field"));
            Assert.That(uxml, Does.Contain("heroes-armory-item-picker"));
            Assert.That(uxml, Does.Contain("heroes-equip-armory-button"));
            Assert.That(uxml, Does.Contain("heroes-gear-slot-field"));
            Assert.That(uxml, Does.Contain("heroes-gear-slot-picker"));
            Assert.That(uxml, Does.Contain("heroes-selected-slot-current-value"));
            Assert.That(uxml, Does.Contain("heroes-selected-slot-compatible-value"));
            Assert.That(uxml, Does.Contain("heroes-unequip-gear-button"));
        }

        [Test]
        public void Mapper_captures_hero_armory_bridge_from_me_payload()
        {
            const string payload = @"{
                ""heroArmoryBridge"": {
                    ""summary"": { ""slotCount"": 12, ""occupiedSlots"": 2, ""distinctItemIds"": 2, ""totalItemCount"": 3 },
                    ""armoryItems"": [
                        { ""slotIndex"": 0, ""itemId"": ""iron_sword_1"", ""qty"": 1, ""template"": { ""name"": ""Iron Sword"", ""slot"": ""mainhand"", ""stats"": { ""power"": 4 } } }
                    ],
                    ""heroEquipment"": [
                        {
                            ""heroId"": ""hero_1"",
                            ""equipment"": [
                                { ""slot"": ""mainhand"", ""itemId"": ""training_blade"", ""qty"": 1, ""template"": { ""name"": ""Training Blade"", ""slot"": ""mainhand"" } }
                            ],
                            ""emptySlots"": [""offhand""],
                            ""bestLoadoutPlan"": [
                                { ""slotIndex"": 0, ""itemId"": ""iron_sword_1"", ""template"": { ""name"": ""Iron Sword"", ""slot"": ""mainhand"" }, ""comparison"": { ""targetSlot"": ""mainhand"", ""state"": ""upgrade"", ""deltaScore"": 5, ""summary"": ""Iron Sword is an upgrade."" } }
                            ],
                            ""bestLoadoutSummary"": { ""note"": ""One upgrade ready."" },
                            ""loadoutResetSummary"": { ""note"": ""One equipped item can return."" }
                        }
                    ]
                }
            }";

            var summary = ShellSummarySnapshotMapper.Map(payload);

            Assert.That(summary.HeroArmoryBridge, Is.Not.Null);
            Assert.That(summary.HeroArmoryBridge.Summary.TotalItemCount, Is.EqualTo(3));
            Assert.That(summary.HeroArmoryBridge.ArmoryItems, Has.Count.EqualTo(1));
            Assert.That(summary.HeroArmoryBridge.ArmoryItems[0].Template.Name, Is.EqualTo("Iron Sword"));
            Assert.That(summary.HeroArmoryBridge.HeroEquipment, Has.Count.EqualTo(1));
            Assert.That(summary.HeroArmoryBridge.HeroEquipment[0].Equipment[0].Slot, Is.EqualTo("mainhand"));
            Assert.That(summary.HeroArmoryBridge.HeroEquipment[0].BestLoadoutPlan[0].State, Is.EqualTo("upgrade"));
        }

        [Test]
        public void Summary_state_formats_hero_armory_receipts_with_equipped_items()
        {
            const string response = @"{
                ""ok"": true,
                ""hero"": { ""name"": ""Ser Kael the Stormguard"" },
                ""equippedSlot"": ""mainhand"",
                ""equippedItem"": { ""itemId"": ""iron_sword_1"", ""qty"": 1 },
                ""cityArmorySummary"": { ""occupiedSlots"": 1, ""totalItemCount"": 2 }
            }";

            var receipt = SummaryState.FormatHeroActionReceipt(response, "Hero gear equipped", "hero_1:slot_0", "Hero");

            Assert.That(receipt, Does.Contain("Hero gear equipped"));
            Assert.That(receipt, Does.Contain("Hero: Ser Kael the Stormguard"));
            Assert.That(receipt, Does.Contain("Gear:"));
            Assert.That(receipt, Does.Contain("Equipped iron_sword_1"));
        }


        [Test]
        public void Hero_armory_slot_surface_exposes_standard_mud_slot_contract()
        {
            Assert.That(HeroArmorySlotWorkflow.StandardSlots.ToArray(), Is.EqualTo(new[]
            {
                "head",
                "chest",
                "legs",
                "feet",
                "hands",
                "mainhand",
                "offhand",
                "ring1",
                "ring2",
                "neck",
            }));

            Assert.That(HeroArmorySlotWorkflow.IsStandardSlot("main hand"), Is.True);
            Assert.That(HeroArmorySlotWorkflow.IsStandardSlot("ring_1"), Is.True);
            Assert.That(HeroArmorySlotWorkflow.IsStandardSlot("waist"), Is.False);
        }

        [Test]
        public void Hero_armory_compatible_filter_uses_backend_item_slot_truth_only()
        {
            var armory = new HeroArmoryBridgeSnapshot
            {
                ArmoryItems = new List<HeroArmoryItemSnapshot>
                {
                    new HeroArmoryItemSnapshot
                    {
                        SlotIndex = 0,
                        ItemId = "iron_helm",
                        Template = new HeroEquipmentTemplateSnapshot { Name = "Iron Helm", Slot = "head" }
                    },
                    new HeroArmoryItemSnapshot
                    {
                        SlotIndex = 1,
                        ItemId = "iron_chest",
                        Template = new HeroEquipmentTemplateSnapshot { Name = "Iron Chest", Slot = "chest" }
                    },
                    new HeroArmoryItemSnapshot
                    {
                        SlotIndex = 2,
                        ItemId = "plain_ring",
                        Template = new HeroEquipmentTemplateSnapshot { Name = "Plain Ring", Slot = "ring" }
                    },
                    new HeroArmoryItemSnapshot
                    {
                        SlotIndex = 3,
                        ItemId = "unknown_token",
                        Template = new HeroEquipmentTemplateSnapshot { Name = "Unknown Token" }
                    },
                    new HeroArmoryItemSnapshot
                    {
                        ItemId = "no_slot_index",
                        Template = new HeroEquipmentTemplateSnapshot { Name = "No Slot Index", Slot = "head" }
                    }
                }
            };

            var headItems = HeroArmorySlotWorkflow.GetCompatibleArmoryItems(armory, "head");
            var ringOneItems = HeroArmorySlotWorkflow.GetCompatibleArmoryItems(armory, "ring1");

            Assert.That(headItems.Select(item => item.ItemId).ToArray(), Is.EqualTo(new[] { "iron_helm" }));
            Assert.That(ringOneItems, Is.Empty, "Generic ring is not silently treated as ring1/ring2 until backend exposes that compatibility explicitly.");
        }

        [Test]
        public void Hero_armory_selected_equipped_slot_controls_return_truth()
        {
            var equipment = new HeroEquipmentSnapshot
            {
                HeroId = "hero_1",
                Equipment = new List<HeroEquipmentEntrySnapshot>
                {
                    new HeroEquipmentEntrySnapshot
                    {
                        Slot = "mainhand",
                        ItemId = "training_blade",
                        Template = new HeroEquipmentTemplateSnapshot { Name = "Training Blade", Slot = "mainhand" }
                    }
                }
            };

            Assert.That(HeroArmorySlotWorkflow.HasEquippedSlot(equipment, "main hand"), Is.True);
            Assert.That(HeroArmorySlotWorkflow.HasEquippedSlot(equipment, "offhand"), Is.False);
            Assert.That(HeroArmorySlotWorkflow.BuildSelectedSlotCurrentText(equipment, "mainhand", false), Does.Contain("Training Blade"));
            Assert.That(HeroArmorySlotWorkflow.BuildSelectedSlotCurrentText(equipment, "offhand", false), Does.Contain("empty gear slot"));
        }

        [Test]
        public void Hero_armory_black_market_slot_copy_uses_operative_kit_language()
        {
            var emptyKitText = HeroArmorySlotWorkflow.BuildCompatibleItemSummary(new List<HeroArmoryItemSnapshot>(), "offhand", true);
            var title = HeroArmorySlotWorkflow.BuildSlotSurfaceTitle(true);
            var current = HeroArmorySlotWorkflow.BuildSelectedSlotCurrentText(new HeroEquipmentSnapshot(), "head", true);

            Assert.That(title, Is.EqualTo("Operative kit slots"));
            Assert.That(emptyKitText, Does.Contain("kit"));
            Assert.That(current, Does.Contain("empty kit slot"));
            Assert.That(emptyKitText.ToLowerInvariant(), Does.Not.Contain("gear"));
        }

        [Test]
        public void Hero_armory_item_choice_hides_internal_slot_index_and_formats_stats_for_players()
        {
            var item = new HeroArmoryItemSnapshot
            {
                SlotIndex = 0,
                ItemId = "arcane_focus",
                Qty = 1,
                Template = new HeroEquipmentTemplateSnapshot
                {
                    Name = "Arcane Focus",
                    Slot = "offhand",
                    Stats = new Dictionary<string, double>
                    {
                        ["int"] = 3,
                        ["wis"] = 1,
                    }
                }
            };

            var choice = HeroArmorySlotWorkflow.BuildArmoryItemChoice(item, false);

            Assert.That(choice, Is.EqualTo("Arcane Focus x1 • Off Hand • Int +3, Wis +1"));
            Assert.That(choice, Does.Not.Contain("[0]"), "The armory slot index is API truth, not player-facing item copy.");
            Assert.That(choice, Does.Not.Contain("Off Hand gear"));
            Assert.That(choice, Does.Not.Contain("int 3"));
        }

        [Test]
        public void Hero_armory_equip_button_names_selected_item_when_available()
        {
            var item = new HeroArmoryItemSnapshot
            {
                SlotIndex = 0,
                ItemId = "arcane_focus",
                Template = new HeroEquipmentTemplateSnapshot { Name = "Arcane Focus", Slot = "offhand" }
            };

            Assert.That(
                HeroArmorySlotWorkflow.BuildEquipButtonText(item, "Lyra of the Veiled Paths", "offhand", false),
                Is.EqualTo("Equip Arcane Focus to Lyra of the Veiled Paths"));

            Assert.That(
                HeroArmorySlotWorkflow.BuildEquipButtonText(null, "Lyra of the Veiled Paths", "head", false),
                Is.EqualTo("Select compatible Head gear"));
        }

        [Test]
        public void Hero_armory_item_choice_marks_selected_item_when_already_equipped()
        {
            var item = new HeroArmoryItemSnapshot
            {
                SlotIndex = 0,
                ItemId = "arcane_focus",
                Qty = 1,
                Template = new HeroEquipmentTemplateSnapshot
                {
                    Name = "Arcane Focus",
                    Slot = "offhand",
                    Stats = new Dictionary<string, double>
                    {
                        ["int"] = 3,
                        ["wis"] = 1,
                    }
                }
            };
            var equipped = new HeroEquipmentEntrySnapshot
            {
                Slot = "offhand",
                ItemId = "arcane_focus",
                Template = new HeroEquipmentTemplateSnapshot
                {
                    Name = "Arcane Focus",
                    Slot = "offhand",
                    Stats = new Dictionary<string, double>
                    {
                        ["int"] = 3,
                        ["wis"] = 1,
                    }
                }
            };

            var choice = HeroArmorySlotWorkflow.BuildArmoryItemChoice(item, false, equipped);

            Assert.That(HeroArmorySlotWorkflow.IsSameEquippedItem(item, equipped), Is.True);
            Assert.That(choice, Is.EqualTo("Arcane Focus x1 • Off Hand • Int +3, Wis +1 • already equipped"));
            Assert.That(choice, Does.Not.Contain("[0]"));
        }

        [Test]
        public void Hero_armory_equip_button_blocks_same_item_already_equipped()
        {
            var item = new HeroArmoryItemSnapshot
            {
                SlotIndex = 0,
                ItemId = "arcane_focus",
                Template = new HeroEquipmentTemplateSnapshot { Name = "Arcane Focus", Slot = "offhand" }
            };
            var equipped = new HeroEquipmentEntrySnapshot
            {
                Slot = "offhand",
                ItemId = "arcane_focus",
                Template = new HeroEquipmentTemplateSnapshot { Name = "Arcane Focus", Slot = "offhand" }
            };
            var differentSlotEquipped = new HeroEquipmentEntrySnapshot
            {
                Slot = "mainhand",
                ItemId = "arcane_focus",
                Template = new HeroEquipmentTemplateSnapshot { Name = "Arcane Focus", Slot = "mainhand" }
            };

            Assert.That(
                HeroArmorySlotWorkflow.BuildEquipButtonText(item, equipped, "Lyra of the Veiled Paths", "offhand", false),
                Is.EqualTo("Arcane Focus already equipped"));
            Assert.That(HeroArmorySlotWorkflow.IsSameEquippedItem(item, equipped), Is.True);
            Assert.That(HeroArmorySlotWorkflow.IsSameEquippedItem(item, differentSlotEquipped), Is.False);
        }

        [Test]
        public void Shell_uses_dark_inline_gear_pickers_instead_of_visible_native_dropdown_popups()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);

            Assert.That(uxml, Does.Contain("heroes-gear-slot-picker"));
            Assert.That(uxml, Does.Contain("heroes-armory-item-picker"));
            Assert.That(uxml, Does.Contain("heroes-native-picker-hidden"), "Native dropdowns stay bound as backing controls, but should not expose bright popup menus in the player-facing gear surface.");
            Assert.That(uss, Does.Contain(".heroes-slot-chip-grid"));
            Assert.That(uss, Does.Contain(".heroes-armory-choice-list"));
            Assert.That(uss, Does.Contain(".heroes-native-picker-hidden"));
            Assert.That(uss, Does.Contain("display: none"));
        }

        [Test]
        public void Shell_places_slot_first_gear_surface_before_roster_quick_cards()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            Assert.That(uxml, Does.Contain("heroes-management-card"));
            Assert.That(uxml, Does.Contain("heroes-selection-card"));
            Assert.That(uxml, Does.Contain("heroes-equipment-card"));
            Assert.That(uxml, Does.Contain("heroes-recruitment-card"));
            Assert.That(uxml, Does.Contain("heroes-roster-cards-card"));
            Assert.That(uxml, Does.Contain("Shared armory truth only"));

            var managementIndex = uxml.IndexOf("heroes-management-card", StringComparison.Ordinal);
            var equipmentIndex = uxml.IndexOf("heroes-equipment-card", StringComparison.Ordinal);
            var recruitmentIndex = uxml.IndexOf("heroes-recruitment-card", StringComparison.Ordinal);
            var rosterCardsIndex = uxml.IndexOf("heroes-roster-cards-card", StringComparison.Ordinal);

            Assert.That(managementIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(equipmentIndex, Is.GreaterThan(managementIndex));
            Assert.That(recruitmentIndex, Is.GreaterThan(equipmentIndex));
            Assert.That(rosterCardsIndex, Is.GreaterThan(recruitmentIndex), "Slot-first controls should render before quick roster cards so release-card duplication does not bury equipment actions.");
        }

        [Test]
        public void Shell_keeps_hero_action_status_inside_slot_surface_management_card()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);

            Assert.That(uxml, Does.Contain("heroes-action-strip"));
            Assert.That(uxml, Does.Contain("heroes-action-strip-value"));
            Assert.That(uss, Does.Contain(".heroes-action-strip"));
            Assert.That(uss, Does.Contain(".heroes-action-strip-value"));

            var managementIndex = uxml.IndexOf("heroes-management-card", StringComparison.Ordinal);
            var actionStripIndex = uxml.IndexOf("heroes-action-strip", StringComparison.Ordinal);
            var noteIndex = uxml.IndexOf("heroes-note-value", StringComparison.Ordinal);
            var selectionIndex = uxml.IndexOf("heroes-selection-card", StringComparison.Ordinal);
            var equipmentIndex = uxml.IndexOf("heroes-equipment-card", StringComparison.Ordinal);

            Assert.That(managementIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(actionStripIndex, Is.GreaterThan(managementIndex));
            Assert.That(noteIndex, Is.GreaterThan(actionStripIndex));
            Assert.That(selectionIndex, Is.GreaterThan(noteIndex), "Hero action receipts/status should stay anchored above roster selection instead of being clipped in the preflight summary cards.");
            Assert.That(equipmentIndex, Is.GreaterThan(selectionIndex));
        }

        [Test]
        public void Shell_keeps_single_hero_desk_note_binding_for_receipt_status()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var first = uxml.IndexOf("heroes-note-value", StringComparison.Ordinal);
            var last = uxml.LastIndexOf("heroes-note-value", StringComparison.Ordinal);

            Assert.That(first, Is.GreaterThanOrEqualTo(0));
            Assert.That(last, Is.EqualTo(first), "The hero result/status binding should appear once so the controller cannot drift between duplicate receipt surfaces.");
            Assert.That(uxml, Does.Not.Contain("<ui:Label name=\"heroes-note-value\" text=\"Hero controls load from live summary payload.\" class=\"summary-value summary-value--glance\" />"), "Hero desk status should live with the slot-first surface, not in the clipped preflight support cards.");
        }
        [Test]
        public void Shell_uses_dark_inline_candidate_picker_instead_of_visible_native_candidate_dropdown()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);

            Assert.That(uxml, Does.Contain("heroes-candidate-picker"));
            Assert.That(uxml, Does.Contain("heroes-candidate-choice-list"));
            Assert.That(uxml, Does.Contain("heroes-manage-candidate-field"));
            Assert.That(uxml, Does.Contain("heroes-native-picker-hidden"), "Native candidate dropdown should stay bound as a hidden backing control, not a visible bright popup picker.");
            Assert.That(uss, Does.Contain(".heroes-candidate-choice-list"));
            Assert.That(uss, Does.Contain(".heroes-candidate-choice"));
            Assert.That(uss, Does.Contain(".heroes-candidate-choice--selected"));
            Assert.That(uss, Does.Contain(".heroes-candidate-choice-empty"));
        }

        [Test]
        public void Hero_candidate_picker_copy_keeps_black_market_contact_language()
        {
            var terminologyType = typeof(HeroScreenController).GetNestedType("HeroTerminology", BindingFlags.NonPublic);
            Assert.That(terminologyType, Is.Not.Null);
            var forMethod = terminologyType.GetMethod("For", BindingFlags.Public | BindingFlags.Static);
            var formatter = typeof(HeroScreenController).GetMethod("BuildCandidateChoiceText", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(forMethod, Is.Not.Null);
            Assert.That(formatter, Is.Not.Null);

            var terms = forMethod.Invoke(null, new object[]
            {
                new ShellSummarySnapshot
                {
                    City = new CitySummarySnapshot
                    {
                        SettlementLane = "black_market",
                        SettlementLaneLabel = "Black Market"
                    }
                }
            });

            var copy = (string)formatter.Invoke(null, new object[]
            {
                new HeroRecruitCandidateSnapshot
                {
                    CandidateId = "contact_1",
                    DisplayName = "Mirelle the Knife Broker",
                    ClassName = "rogue",
                    WealthCost = 25
                },
                terms
            });

            Assert.That(copy, Does.Contain("Mirelle the Knife Broker"));
            Assert.That(copy, Does.Contain("rogue"));
            Assert.That(copy, Does.Contain("wealth 25"));
            Assert.That(copy, Does.Contain("Operative contact from live scouting truth."));
            Assert.That(copy.ToLowerInvariant(), Does.Not.Contain("hero candidate from live recruitment truth"));
        }

        [Test]
        public void Shell_uses_dark_inline_roster_picker_instead_of_visible_native_roster_dropdown()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);

            Assert.That(uxml, Does.Contain("heroes-roster-picker"));
            Assert.That(uxml, Does.Contain("heroes-roster-choice-list"));
            Assert.That(uxml, Does.Contain("heroes-manage-hero-field"));
            Assert.That(uxml, Does.Contain("<ui:DropdownField name=\"heroes-manage-hero-field\" label=\"Hero\" class=\"heroes-native-picker-hidden\" />"), "Native roster dropdown should remain as a hidden backing control instead of opening a bright popup menu.");
            Assert.That(uss, Does.Contain(".heroes-roster-choice-list"));
            Assert.That(uss, Does.Contain(".heroes-roster-choice"));
            Assert.That(uss, Does.Contain(".heroes-roster-choice--selected"));
            Assert.That(uss, Does.Contain(".heroes-roster-choice-empty"));
        }

        [Test]
        public void Hero_roster_picker_copy_keeps_black_market_operative_language()
        {
            var terminologyType = typeof(HeroScreenController).GetNestedType("HeroTerminology", BindingFlags.NonPublic);
            Assert.That(terminologyType, Is.Not.Null);
            var forMethod = terminologyType.GetMethod("For", BindingFlags.Public | BindingFlags.Static);
            var formatter = typeof(HeroScreenController).GetMethod("BuildHeroChoiceText", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(forMethod, Is.Not.Null);
            Assert.That(formatter, Is.Not.Null);

            var terms = forMethod.Invoke(null, new object[]
            {
                new ShellSummarySnapshot
                {
                    City = new CitySummarySnapshot
                    {
                        SettlementLane = "black_market",
                        SettlementLaneLabel = "Black Market"
                    }
                }
            });

            var copy = (string)formatter.Invoke(null, new object[]
            {
                new HeroSnapshot
                {
                    Id = "op_1",
                    Name = "Provost Oren Peel",
                    Status = "idle"
                },
                terms
            });

            Assert.That(copy, Does.Contain("Provost Oren Peel"));
            Assert.That(copy, Does.Contain("operative"));
            Assert.That(copy, Does.Contain("idle"));
            Assert.That(copy.ToLowerInvariant(), Does.Not.Contain("hero"));
        }

        [Test]
        public void Summary_state_formats_hero_release_receipts_with_returned_gear_effects()
        {
            const string response = @"{
                ""ok"": true,
                ""release"": {
                    ""outcome"": ""released"",
                    ""name"": ""Ser Kael the Stormguard"",
                    ""returnedItems"": [
                        { ""itemId"": ""workshop_arcane_focus_1"", ""qty"": 1 }
                    ]
                }
            }";

            var receipt = SummaryState.FormatHeroActionReceipt(response, "Hero released", "hero_1", "Hero");

            Assert.That(receipt, Does.Contain("Hero released"));
            Assert.That(receipt, Does.Contain("Outcome: Released"));
            Assert.That(receipt, Does.Contain("Hero: Ser Kael the Stormguard"));
            Assert.That(receipt, Does.Contain("Effects:"));
            Assert.That(receipt, Does.Contain("Qty +1").Or.Contain("qty +1"));
            Assert.That(receipt, Does.Not.Contain("workshop_arcane_focus_1"));
        }

        [Test]
        public void Hero_roster_picker_survives_release_snapshot_and_selects_next_visible_member()
        {
            var root = BuildMinimalHeroControllerRoot();
            var state = new SummaryState();
            var controller = new HeroScreenController(
                root,
                state,
                _ => System.Threading.Tasks.Task.CompletedTask,
                _ => System.Threading.Tasks.Task.CompletedTask,
                () => System.Threading.Tasks.Task.CompletedTask,
                _ => System.Threading.Tasks.Task.CompletedTask,
                (_, _) => System.Threading.Tasks.Task.CompletedTask,
                (_, _) => System.Threading.Tasks.Task.CompletedTask,
                () => { });

            state.ApplySnapshot(new ShellSummarySnapshot
            {
                Heroes = new List<HeroSnapshot>
                {
                    new HeroSnapshot { Id = "hero_kael", Name = "Ser Kael the Stormguard", Status = "idle", Role = "champion", Level = 1 },
                    new HeroSnapshot { Id = "hero_lyra", Name = "Lyra of the Veiled Paths", Status = "idle", Role = "scout", Level = 1 }
                }
            });
            controller.Render(state.Snapshot);

            var selectedField = typeof(HeroScreenController).GetField("selectedHeroId", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(selectedField, Is.Not.Null);
            Assert.That(selectedField.GetValue(controller), Is.EqualTo("hero_kael"));
            Assert.That(root.Q<Button>("heroes-release-button").text, Does.Contain("Ser Kael"));

            state.ApplySnapshot(new ShellSummarySnapshot
            {
                Heroes = new List<HeroSnapshot>
                {
                    new HeroSnapshot { Id = "hero_lyra", Name = "Lyra of the Veiled Paths", Status = "idle", Role = "scout", Level = 1 }
                }
            });
            controller.Render(state.Snapshot);

            Assert.That(selectedField.GetValue(controller), Is.EqualTo("hero_lyra"), "When the selected hero disappears after release, the client should move selection to the next live roster member instead of leaving gear/release controls pointed at a stale hero.");
            Assert.That(root.Q<Button>("heroes-release-button").text, Does.Contain("Lyra"));
            Assert.That(root.Q<Label>("heroes-selected-slot-current-value").text, Does.Not.Contain("Ser Kael"));
        }

        [Test]
        public void Shell_hides_all_native_hero_lane_dropdown_backing_controls_from_player_surface()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var hiddenBackers = new[]
            {
                "heroes-manage-hero-field",
                "heroes-gear-slot-field",
                "heroes-armory-item-field",
                "heroes-manage-candidate-field",
            };

            foreach (var id in hiddenBackers)
            {
                var idIndex = uxml.IndexOf($"name=\"{id}\"", StringComparison.Ordinal);
                Assert.That(idIndex, Is.GreaterThanOrEqualTo(0), $"{id} should remain present as a bound backing control.");
                var elementEnd = uxml.IndexOf("/>", idIndex, StringComparison.Ordinal);
                Assert.That(elementEnd, Is.GreaterThan(idIndex), $"{id} should be rendered as a single UXML field element.");
                var element = uxml.Substring(idIndex, elementEnd - idIndex);
                Assert.That(element, Does.Contain("heroes-native-picker-hidden"), $"{id} should be hidden from the player-facing surface; no bright native dropdown goblins, thank you.");
            }
        }


        [Test]
        public void Operations_management_uses_dark_inline_pickers_instead_of_visible_native_dropdowns()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);

            foreach (var id in new[]
            {
                "warfront-manage-army-picker",
                "warfront-manage-merge-target-picker",
                "warfront-manage-hold-region-picker",
                "warfront-manage-hold-posture-picker",
                "warfront-manage-dispatch-hero-picker",
            })
            {
                Assert.That(uxml, Does.Contain(id), $"{id} should be present as the player-facing operations picker.");
            }

            Assert.That(uxml, Does.Contain("operations-choice-list"));
            Assert.That(uxml, Does.Contain("Live payload truth only"));
            Assert.That(uss, Does.Contain(".operations-choice-list"));
            Assert.That(uss, Does.Contain(".operations-choice"));
            Assert.That(uss, Does.Contain(".operations-choice--selected"));
            Assert.That(uss, Does.Contain(".operations-choice-empty"));
            Assert.That(uss, Does.Contain("Operations / Dispatch cursed-layout fix v1a"));
            Assert.That(uxml, Does.Contain("operations-action-board"));
            Assert.That(uxml, Does.Contain("operations-support-grid--hidden"));
            Assert.That(uss, Does.Contain(".operations-action-board"));
            Assert.That(uss, Does.Contain(".operations-support-grid--hidden"));
            Assert.That(uss, Does.Contain("Operations / Dispatch overview density cleanup v1b"));
        }

        [Test]
        public void Operations_management_removes_native_dropdown_backing_controls_after_inline_picker_cutover()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            foreach (var id in new[]
            {
                "warfront-manage-army-field",
                "warfront-manage-merge-target-field",
                "warfront-manage-hold-region-field",
                "warfront-manage-hold-posture-field",
                "warfront-manage-dispatch-hero-field",
            })
            {
                Assert.That(uxml, Does.Not.Contain($"name=\"{id}\""), $"{id} should not survive as a native DropdownField in the player-facing operations surface.");
            }
        }

        [Test]
        public void Operations_management_surfaces_named_control_groups_and_lane_copy()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/BlackMarket/BlackMarketScreenController.cs");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "BlackMarketScreenController.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);
            var controller = File.ReadAllText(controllerPath);

            foreach (var marker in new[]
            {
                "warfront-management-subtitle-value",
                "warfront-manage-selected-label-value",
                "warfront-manage-selected-hint-value",
                "warfront-manage-merge-label-value",
                "warfront-manage-merge-hint-value",
                "warfront-manage-hold-label-value",
                "warfront-manage-hold-hint-value",
                "warfront-manage-dispatch-label-value",
                "warfront-manage-dispatch-hint-value",
            })
            {
                Assert.That(uxml, Does.Contain(marker), $"Operations management should keep named control-copy binding: {marker}");
                Assert.That(controller, Does.Contain(marker), $"Operations management controller should bind and update {marker}.");
            }

            Assert.That(uss, Does.Contain(".operations-section-hint"));
            Assert.That(uss, Does.Contain("max-height: 116px"));
            Assert.That(controller, Does.Contain("RenderFormationManagementControlText"));
            Assert.That(controller, Does.Contain("Selected formation"));
            Assert.That(controller, Does.Contain("Deniable dispatch"));
            Assert.That(controller, Does.Contain("Troops to split"));
            Assert.That(controller, Does.Contain("Agents to split"));
        }

        [Test]
        public void Operations_management_note_formats_controls_as_readable_lines()
        {
            var noteMethod = typeof(PlanarWar.Client.UI.Screens.BlackMarket.BlackMarketScreenController)
                .GetMethod("BuildFormationManagementNote", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(noteMethod, Is.Not.Null, "Operations management note formatter should stay available for readability coverage.");

            var note = (string)noteMethod.Invoke(null, new object[]
            {
                new ArmySnapshot { Id = "army_1", Name = "TestTest", Status = "idle", Size = 160, Power = 500, Readiness = 91 },
                60,
                120,
                new ArmySnapshot { Id = "army_2", Name = "Reserve Cell", Status = "idle", Size = 80, Power = 200, Readiness = 80 },
                2,
                "heartland_basin",
                "frontier_hold",
                "Heartland Basin",
                new HeroSnapshot { Id = "hero_1", Name = "Lyra of the Veiled Paths", Status = "idle", Role = "scout", ResponseRoles = new List<string> { "relief", "pressure" } },
                1
            });

            Assert.That(note, Does.Contain("Split:"));
            Assert.That(note, Does.Contain("Merge:"));
            Assert.That(note, Does.Contain("Hold:"));
            Assert.That(note, Does.Contain("Dispatch:"));
            Assert.That(note, Does.Contain("Roster:"));
            Assert.That(note, Does.Contain(Environment.NewLine));
            Assert.That(note, Does.Contain("Heartland Basin"));
            Assert.That(note, Does.Contain("Lyra of the Veiled Paths"));
            Assert.That(note, Does.Not.Contain("Current draft:"));
        }

        [Test]
        public void Home_surface_uses_compact_command_overview_classes()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);

            foreach (var className in new[]
            {
                "home-command-hero",
                "home-resource-strip",
                "home-quick-orders",
                "home-pressure-desk",
                "home-pressure-grid--hidden",
                "home-fast-option-strip",
                "home-timer-strip",
                "home-status-grid",
            })
            {
                Assert.That(uxml, Does.Contain(className), $"{className} should be wired into the Home command surface.");
            }

            Assert.That(uxml, Does.Contain("Command desk"));
            Assert.That(uss, Does.Contain("Home command surface cleanup v1"));
            Assert.That(uss, Does.Contain(".home-pressure-grid--hidden"));
            Assert.That(uss, Does.Contain(".home-fast-option-strip"));
            Assert.That(uss, Does.Contain(".pressure-op-card--compact"));
        }

        [Test]
        public void Home_fast_options_use_compact_action_cards_instead_of_detail_dump_cards()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/Summary/SummaryScreenController.cs");
            Assert.That(File.Exists(controllerPath), Is.True, "SummaryScreenController.cs should be available from the Unity project root.");

            var source = File.ReadAllText(controllerPath);
            Assert.That(source, Does.Contain("pressure-op-card--compact"));
            Assert.That(source, Does.Contain("BuildHomeOperationSummary"));
            Assert.That(source, Does.Contain("BuildHomeOperationSignal"));
            Assert.That(source, Does.Not.Contain("var whyTitle = new Label(\"Why now\")"), "Home fast option cards should no longer render the old multi-section detail dump.");
            Assert.That(source, Does.Not.Contain("var consequenceTitle = new Label(\"Consequence hint\")"), "Home fast option cards should keep consequence signal compact instead of rendering another section wall.");
        }


        [Test]
        public void Home_timer_diagnostic_controls_are_dev_gated_by_default()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/Summary/SummaryScreenController.cs");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "SummaryScreenController.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);
            var source = File.ReadAllText(controllerPath);

            Assert.That(uxml, Does.Contain("name=\"timer-diagnostic-card\""));
            Assert.That(uxml, Does.Contain("name=\"toggle-timer-diagnostics-button\""));
            Assert.That(uxml, Does.Contain("home-dev-diagnostic-gated"));
            Assert.That(uss, Does.Contain("Home dev diagnostic gate v1a"));
            Assert.That(uss, Does.Contain(".home-dev-diagnostic-gated"));
            Assert.That(source, Does.Contain("TimerDiagnosticsDevFlagEnabled = false"));
            Assert.That(source, Does.Contain("RenderTimerDiagnostics"));
            Assert.That(source, Does.Contain("timerDiagnosticCard.style.display = diagnosticsEnabled ? DisplayStyle.Flex : DisplayStyle.None"));
            Assert.That(source, Does.Contain("timerDiagnosticsButton.style.display = diagnosticsEnabled ? DisplayStyle.Flex : DisplayStyle.None"));
        }


        [Test]
        public void Home_surface_closeout_keeps_command_diagnostics_and_rail_status_checkpointed()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/Summary/SummaryScreenController.cs");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "SummaryScreenController.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);
            var source = File.ReadAllText(controllerPath);

            foreach (var className in new[]
            {
                "home-command-hero",
                "home-resource-strip",
                "home-quick-orders",
                "home-pressure-desk",
                "home-fast-option-strip",
                "home-timer-strip",
                "home-status-grid",
                "home-pressure-grid--hidden",
            })
            {
                Assert.That(uxml, Does.Contain(className), $"Home closeout should keep {className} wired into the command surface.");
            }

            Assert.That(uxml, Does.Contain("Command desk"), "Home should keep the player-facing command desk label.");
            Assert.That(uxml, Does.Contain("home-dev-diagnostic-gated"), "Home timer diagnostics should stay present but dev-gated.");
            Assert.That(uxml, Does.Contain("name=\"timer-diagnostic-card\""));
            Assert.That(uxml, Does.Contain("name=\"toggle-timer-diagnostics-button\""));
            Assert.That(uxml, Does.Contain("name=\"nav-home-badge\""));
            Assert.That(uxml, Does.Contain("class=\"chapter-row__action\""));

            Assert.That(uss, Does.Contain("Home command surface cleanup v1"));
            Assert.That(uss, Does.Contain("Home dev diagnostic gate v1a"));
            Assert.That(uss, Does.Contain("Chapter rail status polish v1"));
            Assert.That(uss, Does.Contain(".home-dev-diagnostic-gated"));
            Assert.That(uss, Does.Contain(".home-pressure-grid--hidden"));
            Assert.That(uss, Does.Contain(".pressure-op-card--compact"));

            Assert.That(source, Does.Contain("TimerDiagnosticsDevFlagEnabled = false"));
            Assert.That(source, Does.Contain("RenderTimerDiagnostics"));
            Assert.That(source, Does.Contain("timerDiagnosticCard.style.display = diagnosticsEnabled ? DisplayStyle.Flex : DisplayStyle.None"));
            Assert.That(source, Does.Contain("timerDiagnosticsButton.style.display = diagnosticsEnabled ? DisplayStyle.Flex : DisplayStyle.None"));
            Assert.That(source, Does.Contain("BuildHomeOperationSummary"));
            Assert.That(source, Does.Contain("BuildHomeOperationSignal"));
        }

        [Test]
        public void Home_snapshot_cards_stay_above_pressure_detail_stack_for_quick_resource_reads()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var quickOrdersIndex = uxml.IndexOf("home-quick-orders", StringComparison.Ordinal);
            var timerSnapshotIndex = uxml.IndexOf("home-snapshot-grid--timers", StringComparison.Ordinal);
            var statusSnapshotIndex = uxml.IndexOf("home-snapshot-grid--status", StringComparison.Ordinal);
            var postureIndex = uxml.IndexOf("post-founder-handoff-card", StringComparison.Ordinal);
            var pressureDeskIndex = uxml.IndexOf("home-pressure-desk", StringComparison.Ordinal);

            Assert.That(quickOrdersIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(timerSnapshotIndex, Is.GreaterThan(quickOrdersIndex), "Home timer/resource snapshot cards should sit directly after quick orders so testers can read city output without diving past pressure reports.");
            Assert.That(statusSnapshotIndex, Is.GreaterThan(timerSnapshotIndex), "Production snapshot should stay paired with the timer strip near the top of Home.");
            Assert.That(postureIndex, Is.GreaterThan(statusSnapshotIndex), "Detailed posture/action reports should stay below the quick resource snapshot block.");
            Assert.That(pressureDeskIndex, Is.GreaterThan(statusSnapshotIndex), "Recommended-action detail should not push production/resource output to the bottom of Home.");
        }

        [Test]
        public void Home_recommended_actions_card_keeps_pressure_details_clickable_below_snapshot_reads()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/Summary/SummaryScreenController.cs");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "SummaryScreenController.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);
            var controller = File.ReadAllText(controllerPath);

            var statusSnapshotIndex = uxml.IndexOf("home-snapshot-grid--status", StringComparison.Ordinal);
            var recommendedIndex = uxml.IndexOf("home-recommended-actions-card", StringComparison.Ordinal);
            var postureIndex = uxml.IndexOf("post-founder-handoff-card", StringComparison.Ordinal);
            var pressureIndex = uxml.IndexOf("mother-brain-action-path-card", StringComparison.Ordinal);

            Assert.That(statusSnapshotIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(recommendedIndex, Is.GreaterThan(statusSnapshotIndex), "Recommended actions should appear below the quick city output snapshot, not above resource reads.");
            Assert.That(postureIndex, Is.GreaterThan(recommendedIndex), "Deep posture cards should stay below the compact recommended actions drawer.");
            Assert.That(pressureIndex, Is.GreaterThan(recommendedIndex), "Pressure details should remain reachable below the compact Home drawer.");

            foreach (var marker in new[]
            {
                "home-recommended-actions-card",
                "home-recommended-actions-primary-button",
                "home-recommended-actions-details-button",
                "Recommended actions",
                "Review pressure details"
            })
            {
                Assert.That(uxml, Does.Contain(marker), $"Home recommended actions marker {marker} should stay wired.");
            }

            Assert.That(uss, Does.Contain("Home recommended actions drawer v1"));
            Assert.That(uss, Does.Contain(".home-recommended-actions-card"));
            Assert.That(uss, Does.Contain(".home-recommended-actions__button"));

            Assert.That(controller, Does.Contain("BuildHomeRecommendedAction"));
            Assert.That(controller, Does.Contain("SelectPrimaryClientPressureActionCard"));
            Assert.That(controller, Does.Contain("ResolveClientPressureScreen"));
            Assert.That(controller, Does.Contain("ScrollToPressureDetails"));
            Assert.That(controller, Does.Contain("does not create setup progress, rewards, timers, inventory, or town layout state"));
        }




        [Test]
        public void Home_pressure_details_collapse_under_recommended_actions_button_by_default()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/Summary/SummaryScreenController.cs");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "SummaryScreenController.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);
            var controller = File.ReadAllText(controllerPath);

            var snapshotIndex = uxml.IndexOf("home-snapshot-grid--status", StringComparison.Ordinal);
            var recommendedIndex = uxml.IndexOf("home-recommended-actions-card", StringComparison.Ordinal);
            var detailsIndex = uxml.IndexOf("home-pressure-detail-card", StringComparison.Ordinal);

            Assert.That(snapshotIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(recommendedIndex, Is.GreaterThan(snapshotIndex), "Recommended actions should stay below the quick Home snapshot block.");
            Assert.That(detailsIndex, Is.GreaterThan(recommendedIndex), "The detailed pressure stack should sit behind the recommended-actions button instead of crowding the first Home view.");

            foreach (var marker in new[]
            {
                "home-pressure-detail-card",
                "home-pressure-desk-card",
                "post-founder-handoff-card",
                "mother-brain-action-path-card",
                "city-contract-recovery-board-card"
            })
            {
                Assert.That(uxml, Does.Contain(marker), $"Home pressure-detail marker {marker} should stay wired.");
            }

            Assert.That(uss, Does.Contain("Home pressure detail collapse v1"));
            Assert.That(uss, Does.Contain(".home-pressure-detail-card--collapsed"));
            Assert.That(controller, Does.Contain("homePressureDetailsExpanded"));
            Assert.That(controller, Does.Contain("ToggleHomePressureDetails"));
            Assert.That(controller, Does.Contain("ApplyHomePressureDetailsVisibility"));
            Assert.That(controller, Does.Contain("SetHomePressureDetailCardVisible"));
            Assert.That(controller, Does.Contain("Hide pressure details"));
            Assert.That(controller, Does.Contain("Review pressure details"));
            Assert.That(controller, Does.Not.Contain("Create pressure detail action"));
            Assert.That(controller, Does.Not.Contain("Execute pressure detail action"));
            Assert.That(controller, Does.Not.Contain("Generate pressure detail reward"));
        }


        [Test]
        public void Development_surface_uses_compact_action_boards_and_hides_duplicate_support_grid()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);

            Assert.That(uxml, Does.Contain("Research action board"));
            Assert.That(uxml, Does.Contain("Workshop action board"));
            Assert.That(uxml, Does.Contain("Building / front action board"));
            Assert.That(uxml, Does.Contain("development-action-board"));
            Assert.That(uxml, Does.Contain("development-support-grid--hidden"));
            Assert.That(uxml, Does.Contain("development-desk-actions-card"));

            Assert.That(uss, Does.Contain("Development surface cleanup v1"));
            Assert.That(uss, Does.Contain(".development-action-board"));
            Assert.That(uss, Does.Contain(".development-support-grid--hidden"));
            Assert.That(uss, Does.Contain(".development-desk-actions-card"));
        }


        [Test]
        public void Development_building_selector_uses_inline_picker_buttons_instead_of_native_dropdown()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/City/CityScreenController.cs");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            Assert.That(File.Exists(controllerPath), Is.True, "CityScreenController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var controller = File.ReadAllText(controllerPath);
            var uss = File.ReadAllText(appStylePath);

            Assert.That(controller, Does.Contain("development-inline-selector"));
            Assert.That(controller, Does.Contain("development-inline-selector-choice"));
            Assert.That(controller, Does.Contain("RenderInlineSelector(view)"));
            Assert.That(controller, Does.Not.Contain("new DropdownField()"), "Development building selector should not dynamically create a native DropdownField; inline buttons keep the surface shell-native.");

            Assert.That(uss, Does.Contain("Development building selector inline picker v1a"));
            Assert.That(uss, Does.Contain(".development-inline-selector"));
            Assert.That(uss, Does.Contain(".development-inline-selector-choice"));
            Assert.That(uss, Does.Contain(".development-inline-selector-choice--selected"));
        }


        [Test]
        public void Development_building_selector_labels_hide_raw_ids_when_name_or_type_is_available()
        {
            var formatter = typeof(CityScreenController).GetMethod("FormatBuildingSelectorLabel", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(formatter, Is.Not.Null);

            var namedBuilding = new BuildingSnapshot
            {
                Id = "bid_17737976998318_29296",
                BuildingId = "b_arcane_1",
                Type = "arcane_spire",
                Name = "Arcane Spire",
                Status = "active",
                Level = 1,
            };

            var namedLabel = (string)formatter.Invoke(null, new object[] { namedBuilding, false });
            Assert.That(namedLabel, Does.Contain("Arcane Spire Lv 1"));
            Assert.That(namedLabel, Does.Contain("Operational"));
            Assert.That(namedLabel, Does.Not.Contain("bid_"));
            Assert.That(namedLabel, Does.Not.Contain("b_arcane_1"));

            var typedBuilding = new BuildingSnapshot
            {
                Id = "bid_hidden_123",
                BuildingId = "b_farmland_4",
                Type = "farmland_plot",
                Name = "Building",
                Status = "active",
                Level = 2,
            };

            var typedLabel = (string)formatter.Invoke(null, new object[] { typedBuilding, false });
            Assert.That(typedLabel, Does.Contain("Farmland Plot Lv 2"));
            Assert.That(typedLabel, Does.Not.Contain("bid_hidden_123"));
            Assert.That(typedLabel, Does.Not.Contain("b_farmland_4"));

            var rawFallbackBuilding = new BuildingSnapshot
            {
                Id = "bid_only_987",
                BuildingId = string.Empty,
                Type = string.Empty,
                Name = string.Empty,
                Status = "active",
            };

            var fallbackLabel = (string)formatter.Invoke(null, new object[] { rawFallbackBuilding, false });
            Assert.That(fallbackLabel, Does.Contain("bid_only_987"), "Raw ids should only appear as the last honest fallback when no player-facing name or type exists.");
        }


        [Test]
        public void Development_building_routing_surface_uses_existing_backend_values_without_fake_options()
        {
            var valuesMethod = typeof(CityScreenController).GetMethod("BuildBuildingRoutingPreferenceValues", BindingFlags.NonPublic | BindingFlags.Static);
            var labelsMethod = typeof(CityScreenController).GetMethod("BuildBuildingRoutingPreferenceLabels", BindingFlags.NonPublic | BindingFlags.Static);
            var normalizeMethod = typeof(CityScreenController).GetMethod("NormalizeBuildingRoutingPreference", BindingFlags.NonPublic | BindingFlags.Static);
            var labelMethod = typeof(CityScreenController).GetMethod("BuildBuildingRoutingSelectorLabel", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(valuesMethod, Is.Not.Null);
            Assert.That(labelsMethod, Is.Not.Null);
            Assert.That(normalizeMethod, Is.Not.Null);
            Assert.That(labelMethod, Is.Not.Null);

            var values = ((System.Collections.IEnumerable)valuesMethod.Invoke(null, Array.Empty<object>())).Cast<string>().ToList();
            var labels = ((System.Collections.IEnumerable)labelsMethod.Invoke(null, Array.Empty<object>())).Cast<string>().ToList();

            CollectionAssert.AreEqual(new[] { "balanced", "prefer_local", "prefer_reserve", "prefer_exchange" }, values);
            CollectionAssert.AreEqual(new[] { "Balanced • spread output", "Local • nearby demand", "Reserve • protected stock", "Exchange • trade flow" }, labels);
            Assert.That((string)normalizeMethod.Invoke(null, new object[] { "local" }), Is.EqualTo("prefer_local"));
            Assert.That((string)normalizeMethod.Invoke(null, new object[] { "protected_reserve" }), Is.EqualTo("prefer_reserve"));
            Assert.That((string)normalizeMethod.Invoke(null, new object[] { "exchange" }), Is.EqualTo("prefer_exchange"));
            Assert.That((string)normalizeMethod.Invoke(null, new object[] { "goblin_theater" }), Is.EqualTo("balanced"));
            var cityRoutingLabel = (string)labelMethod.Invoke(null, new object[] { false, string.Empty });
            var marketRoutingLabel = (string)labelMethod.Invoke(null, new object[] { true, "prefer_exchange" });
            Assert.That(cityRoutingLabel, Does.StartWith("Output routing —"));
            Assert.That(cityRoutingLabel, Does.Contain("Balanced spreads output"));
            Assert.That(cityRoutingLabel, Does.Contain("Local feeds nearby demand"));
            Assert.That(cityRoutingLabel, Does.Contain("Reserve protects stock"));
            Assert.That(cityRoutingLabel, Does.Contain("Exchange pushes trade"));
            Assert.That(marketRoutingLabel, Does.StartWith("Front output routing • switching to Exchange —"));
            Assert.That(marketRoutingLabel, Does.Contain("Balanced spreads output"));
        }

        [Test]
        public void Development_building_management_card_surfaces_routing_selector_without_backend_renames()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/City/CityScreenController.cs");
            Assert.That(File.Exists(controllerPath), Is.True, "CityScreenController.cs should be available from the Unity project root.");

            var controller = File.ReadAllText(controllerPath);
            Assert.That(controller, Does.Contain("selectorLabel: BuildBuildingRoutingSelectorLabel"));
            Assert.That(controller, Does.Contain("selectorOptions: routingLabels"));
            Assert.That(controller, Does.Contain("TriggerSwitchBuildingRouting(buildingId, nextRouting)"));
            Assert.That(controller, Does.Contain("PendingBuildingRoutingPreference"));
            Assert.That(controller, Does.Contain("BuildBuildingRoutingManagementNote"));
            Assert.That(controller, Does.Contain("Balanced • spread output"));
            Assert.That(controller, Does.Contain("Local • nearby demand"));
            Assert.That(controller, Does.Contain("Reserve • protected stock"));
            Assert.That(controller, Does.Contain("Exchange • trade flow"));
            Assert.That(controller, Does.Contain("Balanced spreads output; Local feeds nearby demand; Reserve protects stock; Exchange pushes trade."));
            Assert.That(controller, Does.Not.Contain("/api/buildings/routing"), "Routing controls should use the existing callback seam rather than inventing route strings in the UI controller.");
        }

        [Test]
        public void Development_building_build_options_use_inline_choice_selector_for_all_affordable_choices()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/City/CityScreenController.cs");
            Assert.That(File.Exists(controllerPath), Is.True, "CityScreenController.cs should be available from the Unity project root.");

            var controller = File.ReadAllText(controllerPath);
            Assert.That(controller, Does.Contain("ResolveSelectedBuildOption(visibleBuildOptions, isBlackMarket)"));
            Assert.That(controller, Does.Contain("BuildConstructOptionCard(selectedBuildOption, visibleBuildOptions, isBlackMarket, hasActiveBuildWork)"));
            Assert.That(controller, Does.Contain("selectorLabel: isBlackMarket ? \"Choose front to open\" : \"Choose building to build\""));
            Assert.That(controller, Does.Contain("selectorOptions: choices"));
            Assert.That(controller, Does.Contain("SelectBuildOption(safeOptions, isBlackMarket, index)"));
            Assert.That(controller, Does.Contain("Choose from {safeOptions.Count} unlocked affordable {noun} choices before starting work."));
            Assert.That(controller, Does.Contain("CanAffordBuildOption(s?.Resources, option)"), "Build choices should stay gated by visible material/resource truth.");
            Assert.That(controller, Does.Not.Contain("foreach (var option in visibleBuildOptions.Take"), "Growth build choices should not be constrained by leftover card-slot count when a selector can expose all affordable options.");
            Assert.That(controller, Does.Not.Contain("new DropdownField()"), "Build choice selection should keep using inline shell-native buttons, not native dropdowns.");
        }


        [Test]
        public void Development_completed_build_projects_surface_refresh_cta_without_fake_claim_endpoint()
        {
            var statusMethod = typeof(CityScreenController).GetMethod("BuildBuildingCompletionStatusText", BindingFlags.NonPublic | BindingFlags.Static);
            var buttonMethod = typeof(CityScreenController).GetMethod("BuildBuildingCompletionRefreshButtonText", BindingFlags.NonPublic | BindingFlags.Static);
            var noteMethod = typeof(CityScreenController).GetMethod("BuildBuildingCompletionRefreshNote", BindingFlags.NonPublic | BindingFlags.Static);
            var timerReadyMethod = typeof(CityScreenController).GetMethod("IsBuildTimerReady", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(statusMethod, Is.Not.Null);
            Assert.That(buttonMethod, Is.Not.Null);
            Assert.That(noteMethod, Is.Not.Null);
            Assert.That(timerReadyMethod, Is.Not.Null);

            Assert.That((string)statusMethod.Invoke(null, new object[] { false }), Is.EqualTo("Building successfully completed."));
            Assert.That((string)buttonMethod.Invoke(null, new object[] { false }), Is.EqualTo("Update building list"));
            Assert.That((string)statusMethod.Invoke(null, new object[] { true }), Is.EqualTo("Front successfully opened."));
            Assert.That((string)buttonMethod.Invoke(null, new object[] { true }), Is.EqualTo("Update front list"));

            var cityNote = (string)noteMethod.Invoke(null, new object[] { false });
            Assert.That(cityNote, Does.Contain("completed building"));
            Assert.That(cityNote, Does.Contain("backend truth"));
            Assert.That(cityNote, Does.Not.Contain("claim"));

            var readyTimer = new CityTimerEntrySnapshot
            {
                Id = "build_timer_1",
                Category = "construction",
                Label = "Works Quarry",
                Status = "active",
                FinishesAtUtc = DateTime.UtcNow.AddSeconds(-1),
            };
            Assert.That((bool)timerReadyMethod.Invoke(null, new object[] { readyTimer, DateTime.UtcNow }), Is.True);
        }

        [Test]
        public void Development_completed_build_projects_refresh_instead_of_canceling_ready_work()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/City/CityScreenController.cs");
            Assert.That(File.Exists(controllerPath), Is.True, "CityScreenController.cs should be available from the Unity project root.");

            var controller = File.ReadAllText(controllerPath);
            Assert.That(controller, Does.Contain("var readyBuild = IsBuildingReady(building, nowUtc);"));
            Assert.That(controller, Does.Contain("var canRefreshCompletedBuild = readyBuild"));
            Assert.That(controller, Does.Contain("var readyTimer = IsBuildTimerReady(timer, nowUtc);"));
            Assert.That(controller, Does.Contain("buttonText: readyTimer ? BuildBuildingCompletionRefreshButtonText(isBlackMarket) : \"Timed\""));
            Assert.That(controller, Does.Contain("onClick: canRefreshCompletedTimer ? TriggerRefreshDesk : null"));
            Assert.That(controller, Does.Contain("onClick: canRefreshCompletedBuild ? TriggerRefreshDesk : canUpgrade ? () => TriggerUpgradeBuilding(buildingId) : null"));
            Assert.That(controller, Does.Contain("&& !readyBuild\n                && onCancelActiveBuildRequested != null"), "Ready building projects should not keep offering cancel as the primary visible resolution path.");
            Assert.That(controller, Does.Contain("&& !readyTimer;"), "Ready build timers should refresh the desk, not keep offering cancellation.");
            Assert.That(controller, Does.Not.Contain("Complete building"), "Do not fake a completion/claim endpoint when the client only has refresh truth.");
            Assert.That(controller, Does.Not.Contain("Claim building"), "Do not fake a completion/claim endpoint when the client only has refresh truth.");
        }


        [Test]
        public void Development_building_routing_closeout_keeps_visible_copy_honest_without_future_protection_math()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/City/CityScreenController.cs");
            Assert.That(File.Exists(controllerPath), Is.True, "CityScreenController.cs should be available from the Unity project root.");

            var controller = File.ReadAllText(controllerPath);
            Assert.That(controller, Does.Contain("Balanced • spread output"));
            Assert.That(controller, Does.Contain("Local • nearby demand"));
            Assert.That(controller, Does.Contain("Reserve • protected stock"));
            Assert.That(controller, Does.Contain("Exchange • trade flow"));
            Assert.That(controller, Does.Contain("Balanced spreads output; Local feeds nearby demand; Reserve protects stock; Exchange pushes trade."));
            Assert.That(controller, Does.Not.Contain("NPC attack"), "Routing UI must not claim NPC-attack protection until backend truth exists.");
            Assert.That(controller, Does.Not.Contain("raid loss"), "Routing UI must not claim raid-loss math until backend truth exists.");
            Assert.That(controller, Does.Not.Contain("disruption loss"), "Routing UI must not claim disruption-loss math until backend truth exists.");
            Assert.That(controller, Does.Not.Contain("%"), "Routing UI should not expose fake percentage math before the number-nerd/protection model is implemented.");
        }


        [Test]
        public void Development_workshop_recipe_board_has_slots_for_full_current_catalog()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/City/CityScreenController.cs");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "CityScreenController.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var controller = File.ReadAllText(controllerPath);
            for (var i = 1; i <= 10; i++)
            {
                Assert.That(uxml, Does.Contain($"dev-workshop-card-{i}"), $"Workshop recipe card slot {i} should exist so the current ten-recipe catalog is not collapsed to four visible cards.");
                Assert.That(uxml, Does.Contain($"dev-workshop-card-{i}-button"), $"Workshop recipe card slot {i} needs a craft/collect action button.");
            }

            Assert.That(controller, Does.Contain("private const int VisibleWorkshopCardSlots = 10;"));
            Assert.That(controller, Does.Contain("workshopCards = Enumerable.Range(1, VisibleWorkshopCardSlots)"));
            Assert.That(controller, Does.Contain(".Take(Math.Max(0, VisibleWorkshopCardSlots - cards.Count))"));
            Assert.That(controller, Does.Not.Contain("workshopCards = Enumerable.Range(1, 4)"));
        }

        [Test]
        public void Development_workshop_jobs_prefer_player_facing_recipe_labels_over_raw_ids()
        {
            var titleMethod = typeof(CityScreenController)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .FirstOrDefault(method => method.Name == "GetWorkshopJobTitle"
                    && method.GetParameters().Length == 2);
            var noteMethod = typeof(CityScreenController).GetMethod("BuildWorkshopReadyPickupNote", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(titleMethod, Is.Not.Null);
            Assert.That(noteMethod, Is.Not.Null);

            var recipes = new List<WorkshopRecipeSnapshot>
            {
                new WorkshopRecipeSnapshot
                {
                    RecipeId = "recipe_arcane_focus_1",
                    Name = "Arcane Focus",
                    OutputItemId = "workshop_arcane_focus_1",
                }
            };
            var job = new WorkshopJobSnapshot
            {
                Id = "job_very_raw_123",
                RecipeId = "recipe_arcane_focus_1",
                OutputItemId = "workshop_arcane_focus_1",
                AttachmentKind = "workshop_job",
                Completed = true,
            };

            var title = (string)titleMethod.Invoke(null, new object[] { job, recipes });
            var note = (string)noteMethod.Invoke(null, new object[] { job, recipes });

            Assert.That(title, Is.EqualTo("Arcane Focus"));
            Assert.That(note, Does.Contain("Ready to collect: Arcane Focus"));
            Assert.That(note, Does.Not.Contain("job_very_raw_123"));
            Assert.That(note, Does.Not.Contain("recipe_arcane_focus_1"));
        }

        [Test]
        public void Development_workshop_jobs_humanize_raw_recipe_fallbacks_without_losing_truth()
        {
            var titleMethod = typeof(CityScreenController)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .FirstOrDefault(method => method.Name == "GetWorkshopJobTitle"
                    && method.GetParameters().Length == 2);
            Assert.That(titleMethod, Is.Not.Null);

            var job = new WorkshopJobSnapshot
            {
                Id = "job_without_recipe_name",
                RecipeId = "workshop_command_standard_1",
                AttachmentKind = "workshop_job",
            };

            var title = (string)titleMethod.Invoke(null, new object[] { job, new List<WorkshopRecipeSnapshot>() });

            Assert.That(title, Is.EqualTo("Workshop Command Standard 1"));
            Assert.That(title, Does.Not.Contain("_"));
        }

        [Test]
        public void Development_workshop_timer_titles_hide_database_like_payload_names()
        {
            var titleMethod = typeof(CityScreenController).GetMethod("GetWorkshopTimerTitle", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(titleMethod, Is.Not.Null);

            var recipes = new List<WorkshopRecipeSnapshot>
            {
                new WorkshopRecipeSnapshot
                {
                    RecipeId = "recipe_arcane_focus_1",
                    Name = "Arcane Focus",
                    OutputItemId = "workshop_arcane_focus_1",
                }
            };
            var timer = new CityTimerEntrySnapshot
            {
                Id = "timer_workshop_arcane_focus",
                Category = "workshop_job",
                Label = "Workshop arcane_focus",
                Status = "active",
            };

            var title = (string)titleMethod.Invoke(null, new object[] { timer, recipes });

            Assert.That(title, Is.EqualTo("Arcane Focus"));
            Assert.That(title, Does.Not.Contain("arcane_focus"));
            Assert.That(title.StartsWith("Workshop ", StringComparison.OrdinalIgnoreCase), Is.False);
        }

        [Test]
        public void Home_workshop_timer_summary_humanizes_workshop_job_fallback_ids()
        {
            var titleMethod = typeof(SummaryScreenController).GetMethod("GetWorkshopJobTitle", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(titleMethod, Is.Not.Null);

            var job = new WorkshopJobSnapshot
            {
                RecipeId = "workshop_courier_boots_1",
                AttachmentKind = "workshop_job",
            };

            var title = (string)titleMethod.Invoke(null, new object[] { job });

            Assert.That(title, Is.EqualTo("Workshop Courier Boots 1"));
            Assert.That(title, Does.Not.Contain("_"));
        }

        [Test]
        public void Workshop_action_receipts_use_resolved_labels_instead_of_raw_recipe_or_job_ids()
        {
            var bootstrapPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/ClientBootstrap.cs");
            Assert.That(File.Exists(bootstrapPath), Is.True, "ClientBootstrap.cs should be available from the Unity project root.");

            var source = File.ReadAllText(bootstrapPath);
            Assert.That(source, Does.Contain("var craftLabel = ResolveWorkshopRecipeLabel(recipeId);"));
            Assert.That(source, Does.Contain("var collectLabel = ResolveWorkshopJobLabel(jobId);"));
            Assert.That(source, Does.Contain("Workshop craft started: {craftLabel}"));
            Assert.That(source, Does.Contain("Workshop collect complete: {collectLabel}"));
            Assert.That(source, Does.Not.Contain("Workshop craft started: {recipeId.Trim()}"));
            Assert.That(source, Does.Not.Contain("Workshop collect complete: {jobId.Trim()}"));
        }


        [Test]
        public void Workshop_label_cleanup_closeout_keeps_player_facing_labels_without_mutating_backend_ids()
        {
            var cityControllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/City/CityScreenController.cs");
            var summaryControllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/Summary/SummaryScreenController.cs");
            var bootstrapPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/ClientBootstrap.cs");
            Assert.That(File.Exists(cityControllerPath), Is.True, "CityScreenController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(summaryControllerPath), Is.True, "SummaryScreenController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(bootstrapPath), Is.True, "ClientBootstrap.cs should be available from the Unity project root.");

            var citySource = File.ReadAllText(cityControllerPath);
            var summarySource = File.ReadAllText(summaryControllerPath);
            var bootstrapSource = File.ReadAllText(bootstrapPath);

            Assert.That(citySource, Does.Contain("GetWorkshopJobTitle(job, summaryState.WorkshopRecipes)"));
            Assert.That(citySource, Does.Contain("BuildWorkshopReadyPickupNote(job, summaryState.WorkshopRecipes)"));
            Assert.That(citySource, Does.Contain("GetWorkshopTimerTitle(timer, summaryState.WorkshopRecipes)"));
            Assert.That(citySource, Does.Contain("ResolveWorkshopRecipeDisplayName(payloadName, recipes)"));
            Assert.That(citySource, Does.Contain("ExtractWorkshopTimerPayloadName"));
            Assert.That(citySource, Does.Contain("CleanWorkshopDisplayName"));
            Assert.That(citySource, Does.Not.Contain("title: FirstNonBlank(timer.Label"), "Workshop timer cards should not route raw timer labels directly to player-facing titles.");

            Assert.That(summarySource, Does.Contain("HumanizeWords"));
            Assert.That(summarySource, Does.Not.Contain("job.RecipeId ?? job.AttachmentKind"), "Home workshop summaries should not fall back to raw job ids before humanizing them.");

            Assert.That(bootstrapSource, Does.Contain("ResolveWorkshopRecipeLabel(recipeId)"));
            Assert.That(bootstrapSource, Does.Contain("ResolveWorkshopJobLabel(jobId)"));
            Assert.That(bootstrapSource, Does.Contain("Workshop craft started: {craftLabel}"));
            Assert.That(bootstrapSource, Does.Contain("Workshop collect complete: {collectLabel}"));
            Assert.That(bootstrapSource, Does.Not.Contain("Workshop craft started: {recipeId.Trim()}"));
            Assert.That(bootstrapSource, Does.Not.Contain("Workshop collect complete: {jobId.Trim()}"));
        }


        [Test]
        public void Development_surface_closeout_keeps_action_boards_and_inline_building_picker_checkpointed()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/City/CityScreenController.cs");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "CityScreenController.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);
            var controller = File.ReadAllText(controllerPath);

            foreach (var label in new[]
            {
                "Research action board",
                "Workshop action board",
                "Building / front action board",
            })
            {
                Assert.That(uxml, Does.Contain(label), $"Development closeout should keep {label} visible as the player-facing action surface.");
            }

            Assert.That(uxml, Does.Contain("development-action-board"));
            Assert.That(uxml, Does.Contain("development-support-grid--hidden"));
            Assert.That(uxml, Does.Not.Contain("dev-building-selector-field"), "Development should not reintroduce a native building DropdownField by UXML id.");
            Assert.That(controller, Does.Contain("RenderInlineSelector(view)"), "Development card selectors should keep using inline shell-native buttons.");
            Assert.That(controller, Does.Contain("FormatBuildingSelectorLabel"), "Development building labels should stay behind the player-facing formatter instead of leaking raw ids directly.");
            Assert.That(controller, Does.Not.Contain("new DropdownField()"), "Development closeout should not regress to dynamic native dropdown creation.");
            Assert.That(uss, Does.Contain(".development-inline-selector-choice--selected"));
            Assert.That(uss, Does.Contain(".development-desk-actions-card"));
        }



        [Test]
        public void Social_comms_surface_uses_compact_board_and_hidden_duplicate_support_grid()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/AppShellController.cs");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "AppShellController.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);
            var controller = File.ReadAllText(controllerPath);

            Assert.That(uxml, Does.Contain("social-command-hero"));
            Assert.That(uxml, Does.Contain("social-overview-strip"));
            Assert.That(uxml, Does.Contain("social-comms-board"));
            Assert.That(uxml, Does.Contain("social-comms-card-grid"));
            Assert.That(uxml, Does.Contain("social-support-grid--hidden"));
            Assert.That(uxml, Does.Contain("Comms board"));

            Assert.That(uss, Does.Contain("Social / Comms surface cleanup v1"));
            Assert.That(uss, Does.Contain(".social-comms-card"));
            Assert.That(uss, Does.Contain(".social-support-grid--hidden"));
            Assert.That(uss, Does.Contain(".comms-panel"));

            Assert.That(controller, Does.Contain("Comms desk"));
            Assert.That(controller, Does.Contain("friend roster, DMs, and moderation surfaces remain deferred"));
        }


        [Test]
        public void Social_comms_closeout_locks_filter_buttons_and_live_truth_copy()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/AppShellController.cs");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "AppShellController.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var controller = File.ReadAllText(controllerPath);

            foreach (var id in new[]
            {
                "chat-all-button",
                "chat-room-button",
                "chat-system-button",
            })
            {
                Assert.That(uxml, Does.Contain($"name=\"{id}\""), $"{id} should remain wired as a bottom Comms filter control.");
            }

            Assert.That(controller, Does.Contain("SetFilterActive(chatAllButton"), "All filter should still be wired through SetFilterActive.");
            Assert.That(controller, Does.Contain("SetFilterActive(chatRoomButton"), "Chat room filter should still be wired through SetFilterActive.");
            Assert.That(controller, Does.Contain("SetFilterActive(chatSystemButton"), "System filter should still be wired through SetFilterActive.");
            Assert.That(controller, Does.Contain("ActiveChatChannel, \"all\""), "All filter should still be driven by live SessionState channel truth.");
            Assert.That(controller, Does.Contain("ActiveChatChannel, \"room\""), "Chat room filter should still be driven by live SessionState channel truth.");
            Assert.That(controller, Does.Contain("ActiveChatChannel, \"system\""), "System filter should still be driven by live SessionState channel truth.");
            Assert.That(controller, Does.Contain("sessionState.GetVisibleChatLines()"), "Comms board and bottom log should keep consuming filtered live chat lines instead of fake channel rows.");
            Assert.That(controller, Does.Contain("No chat lines visible for this filter yet."), "Room/System empty states should stay readable when a filter has no visible lines.");
            Assert.That(controller, Does.Contain("Chat room comms are live"), "Outbound chat-room hint should stay tied to real websocket chat-room attachment state.");
            Assert.That(controller, Does.Contain("friend roster, DMs, and moderation surfaces remain deferred"), "Social closeout should keep deferred social-system scope explicit.");
        }



        [Test]
        public void Unity_chat_room_pocket_terminology_distinction_locks_separate_truth()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/AppShellController.cs");
            var guidePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Docs/PLAYER_TESTER_GUIDE_V1.md");

            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "AppShellController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(guidePath), Is.True, "PLAYER_TESTER_GUIDE_V1.md should ship with the Unity client.");

            var uxml = File.ReadAllText(appShellPath);
            var controller = File.ReadAllText(controllerPath);
            var guide = File.ReadAllText(guidePath);

            Assert.That(uxml, Does.Contain("Chat room / pocket context"), "Social cards should label the two state families without collapsing them into one room concept.");
            Assert.That(uxml, Does.Contain("text=\"Chat room\""), "The room filter should read as chat-room filtering, not physical room attachment.");
            Assert.That(uxml, Does.Contain("label=\"Chat message\""), "The send box should not imply local physical-room Say semantics yet.");
            Assert.That(controller, Does.Contain("chat room {chatRoomText} • context {physicalContextText}"), "Social overview should show websocket chat-room truth and physical/pocket context side by side.");
            Assert.That(controller, Does.Contain("WS chat room {chatRoomText} is live while"), "Joined websocket room copy should not hide that City/Market pocket context remains separate.");
            Assert.That(controller, Does.Contain("no physical MUD room • no WS chat room"), "Pocket-only copy should avoid implying either fake physical room attachment or fake chat-room send access.");
            Assert.That(guide, Does.Contain("Chat room lobby"), "Tester guide should explain that chat room lobby and City pocket can both be true.");
            Assert.That(guide, Does.Contain("websocket chat-room truth and physical/pocket context are separate"), "Tester guide checklist should preserve the distinction for screenshots and reports.");
            Assert.That(controller, Does.Contain("dedicated City/Market channels and relays remain deferred"), "This slice should keep City/Market channels and relays explicitly deferred.");
        }

        [Test]
        public void Unity_pocket_context_room_label_clarification_locks_no_fake_room_truth()
        {
            var guidePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Docs/PLAYER_TESTER_GUIDE_V1.md");
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/AppShellController.cs");
            var hudPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/ConnectionHud.cs");
            Assert.That(File.Exists(guidePath), Is.True, "Tester guide should be available from the Unity project root.");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "AppShellController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(hudPath), Is.True, "ConnectionHud.cs should be available from the Unity project root.");

            var guide = File.ReadAllText(guidePath);
            var uxml = File.ReadAllText(appShellPath);
            var controller = File.ReadAllText(controllerPath);
            var hud = File.ReadAllText(hudPath);

            Assert.That(uxml, Does.Contain("Room / Pocket"), "Top status strip should label the value as room/pocket truth instead of implying only physical rooms.");
            Assert.That(uxml, Does.Contain("chat-room state, physical/pocket context"), "Social navigation copy should distinguish websocket chat-room truth from physical/pocket context.");
            Assert.That(controller, Does.Contain("IsPocketManagementContext"), "Shell controller should derive pocket context from live summary + session room truth.");
            Assert.That(controller, Does.Contain("Pocket context is expected for City/Black Market command shells"), "Comms copy should explain expected City/Black Market pocket contexts.");
            Assert.That(controller, Does.Contain("WS chat room"), "Comms copy should label websocket chat-room attachment distinctly from physical room/pocket state.");
            Assert.That(controller, Does.Contain("No fake physical room is attached"), "Social copy must not pretend a settlement shell has physical MUD room membership.");
            Assert.That(controller, Does.Contain("dedicated City/Market channels and relays remain deferred"), "The slice must not smuggle in dedicated channels or relay implementation.");
            Assert.That(hud, Does.Contain("Pocket shells may be unattached."), "Debug HUD should not make expected pocket detachment look like a generic broken room state.");
            Assert.That(guide, Does.Contain("pocket-management contexts"), "Tester guide should explain why City/Black Market shells can be room-unattached.");
            Assert.That(guide, Does.Contain("should not fake regional room membership"), "Tester guide should keep no-fake-region guardrail explicit.");
        }


        [Test]
        public void Chapter_rail_status_labels_have_separation_styles()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);

            Assert.That(uxml, Does.Contain("name=\"nav-home-badge\""));
            Assert.That(uxml, Does.Contain("class=\"chapter-row__action\""));
            Assert.That(uss, Does.Contain("Chapter rail status polish v1"));
            Assert.That(uss, Does.Contain(".chapter-row__badge"));
            Assert.That(uss, Does.Contain(".chapter-row__action"));
            Assert.That(uss, Does.Contain("margin-left: 8px"));
            Assert.That(uss, Does.Contain("-unity-text-align: middle-center"));
        }




        [Test]
        public void Left_rail_vertical_density_cleanup_keeps_compact_navigation_copy()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);

            foreach (var marker in new[]
            {
                "Client-first shell. Scan the empire, choose a desk, then act.",
                "Home scans the empire. Desks handle focused actions.",
                "Manifest pending.",
                "Patch source: box manifest.",
                "Empire, timers, quick orders.",
                "Comms, chat room, pocket context.",
                "Flow, desks, reporting checklist.",
            })
            {
                Assert.That(uxml, Does.Contain(marker), $"Compact left-rail marker should stay present: {marker}");
            }

            Assert.That(uss, Does.Contain("Left rail vertical density cleanup v1"));
            Assert.That(uss, Does.Contain(".rail-panel--compact .chapter-row"));
            Assert.That(uss, Does.Contain("min-height: 74px"));
            Assert.That(uss, Does.Contain(".rail-panel--compact .rail-version-card .rail-copy"));
        }


        [Test]
        public void Home_resource_summary_wrap_cleanup_keeps_readable_strip_contract()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);

            Assert.That(uxml, Does.Contain("name=\"resources-value\""));
            Assert.That(uxml, Does.Contain("home-resource-strip__value"));
            Assert.That(uxml, Does.Contain("Empire at a glance"), "The resource strip should remain a live summary surface rather than fake setup or reward copy.");

            Assert.That(uss, Does.Contain("Home resource summary wrap cleanup v1"));
            Assert.That(uss, Does.Contain(".home-resource-strip__value"));
            Assert.That(uss, Does.Contain("max-width: 100%"));
            Assert.That(uss, Does.Contain("white-space: normal"));
            Assert.That(uss, Does.Contain("overflow: visible"));
        }


        [Test]
        public void Home_quick_orders_button_width_cleanup_keeps_aligned_button_contract()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);

            Assert.That(uxml, Does.Contain("home-quick-orders__row"));
            Assert.That(uxml, Does.Contain("home-quick-orders__button"));

            foreach (var buttonName in new[]
            {
                "refresh-button",
                "home-development-button",
                "home-guide-button",
                "whereami-button",
                "ping-button",
            })
            {
                Assert.That(uxml, Does.Contain($"name=\"{buttonName}\""), $"Quick order button should remain wired: {buttonName}");
            }

            Assert.That(uss, Does.Contain("Home quick orders button width cleanup v1"));
            Assert.That(uss, Does.Contain(".home-quick-orders__button"));
            Assert.That(uss, Does.Contain("width: 142px"));
            Assert.That(uss, Does.Contain("-unity-text-align: middle-center"));
        }


        [Test]
        public void Home_next_desk_button_width_cleanup_keeps_aligned_button_contract()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);

            Assert.That(uxml, Does.Contain("post-founder-handoff-actions__row"));
            Assert.That(uxml, Does.Contain("post-founder-handoff-actions__button"));

            foreach (var buttonName in new[]
            {
                "post-founder-development-button",
                "post-founder-operations-button",
                "post-founder-roster-button",
            })
            {
                Assert.That(uxml, Does.Contain($"name=\"{buttonName}\""), $"Next-desk button should remain wired: {buttonName}");
            }

            Assert.That(uss, Does.Contain("Home next-desk button width cleanup v1"));
            Assert.That(uss, Does.Contain(".post-founder-handoff-actions__button"));
            Assert.That(uss, Does.Contain("width: 142px"));
            Assert.That(uss, Does.Contain("-unity-text-align: middle-center"));
        }


        [Test]
        public void Home_snapshot_card_grid_cleanup_keeps_live_snapshot_cards_readable()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);

            foreach (var marker in new[]
            {
                "home-snapshot-grid",
                "home-snapshot-grid--timers",
                "home-snapshot-grid--status",
                "home-snapshot-card",
                "home-snapshot-card__value",
            })
            {
                Assert.That(uxml, Does.Contain(marker), $"Home snapshot marker should stay present: {marker}");
            }

            foreach (var valueName in new[]
            {
                "research-timer-value",
                "workshop-timer-value",
                "mission-timer-value",
                "resource-tick-value",
                "production-value",
                "research-value",
                "warnings-value",
                "ready-ops-value",
                "hero-status-value",
                "army-status-value",
            })
            {
                Assert.That(uxml, Does.Contain($"name=\"{valueName}\""), $"Home snapshot value should remain wired: {valueName}");
            }

            Assert.That(uss, Does.Contain("Home snapshot card grid cleanup v1"));
            Assert.That(uss, Does.Contain(".home-snapshot-grid .home-snapshot-card"));
            Assert.That(uss, Does.Contain("width: 260px"));
            Assert.That(uss, Does.Contain("min-width: 240px"));
            Assert.That(uss, Does.Contain("white-space: normal"));
        }


        [Test]
        public void Gameplay_shell_closeout_locks_cleaned_surface_markers()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);

            foreach (var marker in new[]
            {
                "home-command-hero",
                "home-resource-strip",
                "home-quick-orders",
                "home-pressure-desk",
                "development-action-board",
                "development-support-grid--hidden",
                "operations-action-board",
                "operations-support-grid--hidden",
                "heroes-roster-picker",
                "heroes-gear-slot-picker",
                "heroes-armory-item-picker",
                "heroes-candidate-picker",
                "social-comms-board",
                "social-support-grid--hidden",
                "chapter-row__action",
                "home-dev-diagnostic-gated",
            })
            {
                Assert.That(uxml, Does.Contain(marker), $"Cleaned gameplay shell marker {marker} should remain in AppShell.uxml.");
            }

            foreach (var marker in new[]
            {
                ".home-fast-option-card",
                ".development-action-board",
                ".operations-action-board",
                ".heroes-roster-picker",
                ".social-comms-board",
                ".chapter-row__action",
                ".home-dev-diagnostic-gated",
            })
            {
                Assert.That(uss, Does.Contain(marker), $"Cleaned gameplay shell style {marker} should remain in AppShell.uss.");
            }
        }

        [Test]
        public void Gameplay_shell_closeout_prevents_native_dropdown_regressions_in_cleaned_surfaces()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var cityControllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/City/CityScreenController.cs");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(cityControllerPath), Is.True, "CityScreenController.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var cityController = File.ReadAllText(cityControllerPath);

            foreach (var id in new[]
            {
                "warfront-manage-army-field",
                "warfront-manage-merge-target-field",
                "warfront-manage-hold-region-field",
                "warfront-manage-hold-posture-field",
                "warfront-manage-dispatch-hero-field",
                "dev-building-selector-field",
            })
            {
                Assert.That(uxml, Does.Not.Contain($"name=\"{id}\""), $"{id} should not return as a player-facing native DropdownField.");
            }

            foreach (var id in new[]
            {
                "heroes-manage-hero-field",
                "heroes-gear-slot-field",
                "heroes-armory-item-field",
                "heroes-manage-candidate-field",
            })
            {
                var idIndex = uxml.IndexOf($"name=\"{id}\"", StringComparison.Ordinal);
                Assert.That(idIndex, Is.GreaterThanOrEqualTo(0), $"{id} should remain present as a bound backing control.");
                var elementEnd = uxml.IndexOf("/>", idIndex, StringComparison.Ordinal);
                Assert.That(elementEnd, Is.GreaterThan(idIndex), $"{id} should be rendered as a single hidden backing element.");
                var element = uxml.Substring(idIndex, elementEnd - idIndex);
                Assert.That(element, Does.Contain("heroes-native-picker-hidden"), $"{id} should stay hidden behind inline player-facing pickers.");
            }

            Assert.That(cityController, Does.Not.Contain("new DropdownField()"), "Development selectors should not regress to dynamic native DropdownField creation.");
            Assert.That(cityController, Does.Contain("RenderInlineSelector(view)"), "Development selectors should stay shell-native inline controls.");
        }

        [Test]
        public void Gameplay_shell_closeout_keeps_diagnostics_gated_and_number_breakdowns_deferred()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            var summaryControllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/Summary/SummaryScreenController.cs");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");
            Assert.That(File.Exists(summaryControllerPath), Is.True, "SummaryScreenController.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);
            var summaryController = File.ReadAllText(summaryControllerPath);

            Assert.That(uxml, Does.Contain("name=\"timer-diagnostic-card\""));
            Assert.That(uxml, Does.Contain("name=\"toggle-timer-diagnostics-button\""));
            Assert.That(uxml, Does.Contain("home-dev-diagnostic-gated"));
            Assert.That(uss, Does.Contain(".home-dev-diagnostic-gated"));
            Assert.That(summaryController, Does.Contain("TimerDiagnosticsDevFlagEnabled = false"));
            Assert.That(summaryController, Does.Contain("timerDiagnosticCard.style.display = diagnosticsEnabled ? DisplayStyle.Flex : DisplayStyle.None"));
            Assert.That(summaryController, Does.Contain("timerDiagnosticsButton.style.display = diagnosticsEnabled ? DisplayStyle.Flex : DisplayStyle.None"));

            foreach (var deferred in new[]
            {
                "number-nerd",
                "number nerd",
                "troop-power-breakdown",
                "economy-breakdown",
                "production-breakdown",
                "pressure-breakdown",
            })
            {
                Assert.That(uxml, Does.Not.Contain(deferred), $"Detailed formula/breakdown surfaces stay deferred until their own bounded slice: {deferred}");
            }
        }

        [Test]
        public void Operations_mission_board_surfaces_assignment_and_offer_truth()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/BlackMarket/BlackMarketScreenController.cs");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "BlackMarketScreenController.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);
            var controller = File.ReadAllText(controllerPath);

            foreach (var marker in new[]
            {
                "operations-mission-board",
                "warfront-mission-board-copy-value",
                "warfront-mission-board-title-value",
                "warfront-mission-board-status-value",
                "warfront-mission-board-effect-value",
                "warfront-mission-board-assignment-value",
                "warfront-mission-offer-picker",
                "warfront-mission-primary-button",
            })
            {
                Assert.That(uxml, Does.Contain(marker), $"Mission board marker {marker} should stay present in Operations.");
            }

            Assert.That(uss, Does.Contain("Operations mission board / dispatch clarity v1"));
            Assert.That(uss, Does.Contain(".operations-mission-board"));
            Assert.That(uss, Does.Contain(".operations-mission-offer-picker"));
            Assert.That(controller, Does.Contain("RenderMissionBoard(summary, rankedArmies, activeMission, primaryWarning, nowUtc)"));
            Assert.That(controller, Does.Contain("BuildMissionStartAssignmentSummary"));
            Assert.That(controller, Does.Contain("CleanMissionPayloadText"));
            Assert.That(controller, Does.Contain("LooksLikeRawMissionPayload"));
            Assert.That(controller, Does.Contain("selected cell, selected operative/hero, and balanced response posture"));
            Assert.That(controller, Does.Contain("TriggerStartMission(selectedOffer.Id)"));
            Assert.That(controller, Does.Contain("TriggerCompleteMission(activeMission.InstanceId)"));
            Assert.That(controller, Does.Not.Contain("/api/missions/start"), "Operations controller should reuse the existing callback seam instead of hardcoding mission routes.");
            Assert.That(controller, Does.Not.Contain("/api/missions/complete"), "Operations controller should reuse the existing callback seam instead of hardcoding mission routes.");
        }

        [Test]
        public void Operations_mission_board_sanitizes_raw_payload_text_before_rendering()
        {
            var effectMethod = typeof(PlanarWar.Client.UI.Screens.BlackMarket.BlackMarketScreenController).GetMethod("BuildMissionEffectSummary", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(effectMethod, Is.Not.Null, "Mission effect summary formatter should be available for payload-text cleanup coverage.");

            var effect = (string)effectMethod.Invoke(null, new object[]
            {
                "{ \"summary\": \"Contain the fallout before pressure spreads.\" }",
                "{ \"effect\": \"relief support\" }",
                "{ \"notes\": \"Raw object should not leak.\", \"severity\": \"high\" }"
            });

            Assert.That(effect, Does.Contain("Contain the fallout before pressure spreads."));
            Assert.That(effect, Does.Contain("Gain/effect: relief support"));
            Assert.That(effect, Does.Contain("Risk: high"));
            Assert.That(effect, Does.Not.Contain("{"), "Mission board should never render raw object braces in player-facing copy.");
            Assert.That(effect, Does.Not.Contain("\"notes\""), "Mission board should not render raw JSON-ish keys in player-facing copy.");
            Assert.That(effect, Does.Not.Contain("Raw object should not leak."), "Risk should prefer severity/threat fields over debug-ish nested notes when available.");
        }

        [Test]
        public void Operations_mission_board_keeps_black_market_and_city_assignment_language()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/BlackMarket/BlackMarketScreenController.cs");
            Assert.That(File.Exists(controllerPath), Is.True, "BlackMarketScreenController.cs should be available from the Unity project root.");

            var controller = File.ReadAllText(controllerPath);
            Assert.That(controller, Does.Contain("Cell: no selected idle cell"));
            Assert.That(controller, Does.Contain("Formation: no selected idle formation"));
            Assert.That(controller, Does.Contain("Operative: no selected idle operative"));
            Assert.That(controller, Does.Contain("Hero: no selected idle hero"));
            Assert.That(controller, Does.Contain("Posture: balanced"));
            Assert.That(controller, Does.Contain("Mission board stays honest instead of inventing fake work."));
        }

        [Test]
        public void Operations_mission_board_humanizes_embedded_region_keys_in_mission_titles()
        {
            var titleMethod = typeof(PlanarWar.Client.UI.Screens.BlackMarket.BlackMarketScreenController)
                .GetMethod("BuildMissionDisplayTitle", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(titleMethod, Is.Not.Null, "Mission title formatter should stay available for region-key cleanup coverage.");

            var falloutTitle = (string)titleMethod.Invoke(null, new object[]
            {
                "Contain the Fallout in heartland_basin",
                "contain_fallout_1",
                "heartland_basin",
                "Mission offer"
            });

            var borderTitle = (string)titleMethod.Invoke(null, new object[]
            {
                "Quiet Border Petitioners in ancient-elwynn",
                "quiet_border_1",
                "ancient_elwynn",
                "Mission offer"
            });

            var fallbackTitle = (string)titleMethod.Invoke(null, new object[]
            {
                string.Empty,
                "counterfeit_trace_1",
                string.Empty,
                "Mission offer"
            });

            Assert.That(falloutTitle, Is.EqualTo("Contain the Fallout in Heartland Basin"));
            Assert.That(falloutTitle, Does.Not.Contain("heartland_basin"));
            Assert.That(borderTitle, Is.EqualTo("Quiet Border Petitioners in Ancient Elwynn"));
            Assert.That(borderTitle, Does.Not.Contain("ancient-elwynn"));
            Assert.That(fallbackTitle, Is.EqualTo("Counterfeit Trace 1"));
        }

        [Test]
        public void Operations_surface_humanizes_embedded_region_keys_in_timer_and_recent_result_titles()
        {
            var controllerType = typeof(PlanarWar.Client.UI.Screens.BlackMarket.BlackMarketScreenController);
            var timerMethod = controllerType.GetMethod("NormalizeOperationsTimerLabel", BindingFlags.NonPublic | BindingFlags.Static);
            var titleMethod = controllerType.GetMethod("BuildMissionDisplayTitle", BindingFlags.NonPublic | BindingFlags.Static);

            Assert.That(timerMethod, Is.Not.Null, "Operations timer label formatter should stay available for raw region-key cleanup coverage.");
            Assert.That(titleMethod, Is.Not.Null, "Mission title formatter should stay available for raw region-key cleanup coverage.");

            var heartlandTimer = (string)timerMethod.Invoke(null, new object[] { "Warfront window heartland_basin" });
            var sunfallTimer = (string)timerMethod.Invoke(null, new object[] { "Warfront window sunfall_coast" });
            var recentTitle = (string)titleMethod.Invoke(null, new object[]
            {
                "Contain the Fallout in heartland_basin — Escalation 2",
                "active_1",
                string.Empty,
                "Recent mission result"
            });

            Assert.That(heartlandTimer, Is.EqualTo("Operations window Heartland Basin"));
            Assert.That(sunfallTimer, Is.EqualTo("Operations window Sunfall Coast"));
            Assert.That(recentTitle, Is.EqualTo("Contain the Fallout in Heartland Basin — Escalation 2"));
            Assert.That(heartlandTimer, Does.Not.Contain("heartland_basin"));
            Assert.That(sunfallTimer, Does.Not.Contain("sunfall_coast"));
            Assert.That(recentTitle, Does.Not.Contain("heartland_basin"));
        }

        [Test]
        public void Operations_active_mission_note_defers_outcome_copy_until_completion()
        {
            var noteMethod = typeof(PlanarWar.Client.UI.Screens.BlackMarket.BlackMarketScreenController)
                .GetMethod("BuildActiveMissionCardNote", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(noteMethod, Is.Not.Null, "Active mission note formatter should stay available for outcome-deferral coverage.");

            var activeMission = new MissionSnapshot
            {
                Id = "contain_fallout_1",
                Title = "Contain the Fallout in Heartland Basin",
                InstanceId = "active_1",
                RegionId = "heartland_basin",
                AssignedArmyId = "army_1",
                AssignedArmyName = "TestTest",
                Summary = "The mission failed loudly enough that the aftermath is now its own problem.",
                Payoff = "public relief",
                Risk = "extreme",
                FinishesAtUtc = DateTime.UtcNow.AddMinutes(8)
            };
            var summary = new ShellSummarySnapshot
            {
                Armies = new List<ArmySnapshot>
                {
                    new ArmySnapshot { Id = "army_1", Name = "TestTest", Status = "on_mission" }
                }
            };

            var note = (string)noteMethod.Invoke(null, new object[] { activeMission, summary, "The mission failed before the player pressed complete." });

            Assert.That(note, Does.Contain("Outcome report appears after completion."));
            Assert.That(note, Does.Contain("Cell: TestTest").Or.Contain("Formation: TestTest"));
            Assert.That(note, Does.Not.Contain("failed loudly"));
            Assert.That(note, Does.Not.Contain("failed before"));
            Assert.That(note, Does.Not.Contain("Gain/effect"));
            Assert.That(note, Does.Not.Contain("Risk:"));
        }

        [Test]
        public void Operations_mission_board_closeout_keeps_dispatch_surface_checkpointed()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var appStylePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/BlackMarket/BlackMarketScreenController.cs");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(appStylePath), Is.True, "AppShell.uss should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "BlackMarketScreenController.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var uss = File.ReadAllText(appStylePath);
            var controller = File.ReadAllText(controllerPath);

            foreach (var marker in new[]
            {
                "operations-mission-board",
                "warfront-mission-board-copy-value",
                "warfront-mission-board-title-value",
                "warfront-mission-board-status-value",
                "warfront-mission-board-effect-value",
                "warfront-mission-board-assignment-value",
                "warfront-mission-offer-picker",
                "warfront-mission-primary-button",
            })
            {
                Assert.That(uxml, Does.Contain(marker), $"Mission dispatch board marker {marker} must remain present.");
            }

            foreach (var styleMarker in new[]
            {
                ".operations-mission-board",
                ".operations-mission-card",
                ".operations-mission-offer-picker",
                ".operations-mission-offer-choice",
                ".operations-mission-offer-choice--selected",
            })
            {
                Assert.That(uss, Does.Contain(styleMarker), $"Mission dispatch board style {styleMarker} must remain present.");
            }

            foreach (var callbackMarker in new[]
            {
                "RenderMissionBoard(summary, rankedArmies, activeMission, primaryWarning, nowUtc)",
                "BuildMissionStartAssignmentSummary",
                "TriggerStartMission(selectedOffer.Id)",
                "TriggerCompleteMission(activeMission.InstanceId)",
            })
            {
                Assert.That(controller, Does.Contain(callbackMarker), $"Mission board should keep using the existing callback seam: {callbackMarker}");
            }

            Assert.That(controller, Does.Contain("CleanMissionPayloadText"));
            Assert.That(controller, Does.Contain("LooksLikeRawMissionPayload"));
            Assert.That(controller, Does.Contain("Posture: balanced"));
            Assert.That(controller, Does.Not.Contain("/api/missions/start"), "UI controller should not invent or hardcode mission start routes.");
            Assert.That(controller, Does.Not.Contain("/api/missions/complete"), "UI controller should not invent or hardcode mission complete routes.");
        }

        [Test]
        public void Operations_mission_board_closeout_keeps_raw_payload_text_sanitized()
        {
            var effectMethod = typeof(PlanarWar.Client.UI.Screens.BlackMarket.BlackMarketScreenController)
                .GetMethod("BuildMissionEffectSummary", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(effectMethod, Is.Not.Null, "Mission effect summary formatter should remain available for closeout coverage.");

            var effect = (string)effectMethod.Invoke(null, new object[]
            {
                "{ \"summary\": \"Recover evidence before pressure spreads.\" }",
                "{ \"payoff\": \"public relief\" }",
                "{ \"notes\": \"Nested notes should not leak.\", \"severity\": \"extreme\" }"
            });

            Assert.That(effect, Does.Contain("Recover evidence before pressure spreads."));
            Assert.That(effect, Does.Contain("Gain/effect: public relief"));
            Assert.That(effect, Does.Contain("Risk: extreme"));
            Assert.That(effect, Does.Not.Contain("{"));
            Assert.That(effect, Does.Not.Contain("}"));
            Assert.That(effect, Does.Not.Contain("\"notes\""));
            Assert.That(effect, Does.Not.Contain("Nested notes should not leak."));
        }

        [Test]
        public void Client_lifecycle_copy_uses_update_and_collect_language_instead_of_ready_finished_shorthand()
        {
            var citySource = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/City/CityScreenController.cs"));
            var summarySource = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/Summary/SummaryScreenController.cs"));
            var blackMarketSource = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/BlackMarket/BlackMarketScreenController.cs"));
            var shadowLaneSource = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Core/Presentation/ShadowLaneText.cs"));
            var combined = string.Join("\n", citySource, summarySource, blackMarketSource, shadowLaneSource);

            Assert.That(combined, Does.Contain("Ready to update"));
            Assert.That(combined, Does.Contain("Ready to collect"));
            Assert.That(combined, Does.Contain("ready to update for result"));
            Assert.That(combined, Does.Contain("Drop ready"));
            Assert.That(combined, Does.Contain("Timer ready"));
            Assert.That(combined, Does.Not.Contain("ready/finished"));
            Assert.That(combined, Does.Not.Contain("ready / refresh for result"));
            Assert.That(combined, Does.Not.Contain("Ready pickup"));
            Assert.That(combined, Does.Not.Contain("Ready drop"));
            Assert.That(combined, Does.Not.Contain("Ready timer"));
        }


        [Test]
        public void Client_summary_mapper_captures_settlement_setup_choices()
        {
            var snapshot = ShellSummarySnapshotMapper.Map(
                "{" +
                "\"username\":\"Rimuru\"," +
                "\"founderMode\":true," +
                "\"canCreateCity\":true," +
                "\"suggestedCityName\":\"Tempest\"," +
                "\"citySetupChoices\":[" +
                "{\"lane\":\"city\",\"label\":\"City\",\"summary\":\"Public growth lane\",\"strength\":\"Visible production\",\"liability\":\"Public pressure\",\"ctaLabel\":\"Found City\",\"checklist\":[\"Name settlement\",\"Choose civic lane\"]}," +
                "{\"lane\":\"black_market\",\"label\":\"Black Market\",\"summary\":\"Shadow operation lane\",\"strength\":[\"Covert contacts\",\"Deniable leverage\"],\"liability\":[\"Deniable risk\",\"Hotter opening pressure\"],\"responseFocus\":{\"openingChecklist\":[\"Cool cartel heat\",\"Secure throughput\"]},\"ctaLabel\":\"Found Black Market\"}" +
                "]" +
                "}");

            Assert.That(snapshot.HasCity, Is.False);
            Assert.That(snapshot.FounderMode, Is.True);
            Assert.That(snapshot.CanCreateCity, Is.True);
            Assert.That(snapshot.SuggestedCityName, Is.EqualTo("Tempest"));
            Assert.That(snapshot.CitySetupChoices, Has.Count.EqualTo(2));
            Assert.That(snapshot.CitySetupChoices[0].Lane, Is.EqualTo("city"));
            Assert.That(snapshot.CitySetupChoices[0].Checklist, Does.Contain("Name settlement"));
            Assert.That(snapshot.CitySetupChoices[1].Lane, Is.EqualTo("black_market"));
            Assert.That(snapshot.CitySetupChoices[1].Strength, Is.EqualTo("Covert contacts • Deniable leverage"));
            Assert.That(snapshot.CitySetupChoices[1].Liability, Is.EqualTo("Deniable risk • Hotter opening pressure"));
            Assert.That(snapshot.CitySetupChoices[1].Checklist, Does.Contain("Cool cartel heat"));
        }

        [Test]
        public void Client_has_city_bootstrap_http_contract()
        {
            var apiClientPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Network/Http/PlanarWarApiClient.cs");
            Assert.That(File.Exists(apiClientPath), Is.True, "PlanarWarApiClient.cs should be available from the Unity project root.");

            var source = File.ReadAllText(apiClientPath);
            Assert.That(source, Does.Contain("BootstrapCityAsync"));
            Assert.That(source, Does.Contain("/api/city/bootstrap"));
            Assert.That(source, Does.Contain("[\"name\"]"));
            Assert.That(source, Does.Contain("[\"settlementLane\"]"));
            Assert.That(source, Does.Contain("[\"laneChoice\"]"));
            Assert.That(source, Does.Contain("includeBearerToken: true"));
        }

        [Test]
        public void Home_surface_exposes_founder_setup_controls_without_fake_layout_claims()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            Assert.That(uxml, Does.Contain("founder-setup-card"));
            Assert.That(uxml, Does.Contain("founder-city-name-field"));
            Assert.That(uxml, Does.Contain("founder-action-status-value"));
            Assert.That(uxml, Does.Contain("founder-city-primary-button"));
            Assert.That(uxml, Does.Contain("founder-market-primary-button"));
            Assert.That(uxml, Does.Contain("founder-city-button"));
            Assert.That(uxml, Does.Contain("founder-market-button"));
            Assert.That(uxml, Does.Contain("Live bootstrap"));
            Assert.That(uxml, Does.Not.Contain("2D town layout"));
            Assert.That(uxml, Does.Not.Contain("generated town layout"));
            Assert.That(uxml, Does.Not.Contain("protection percentage"));
        }

        [Test]
        public void Client_bootstrap_wires_settlement_setup_action_to_live_bootstrap_route()
        {
            var bootstrapPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/ClientBootstrap.cs");
            var shellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/AppShellController.cs");
            var summaryPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/Summary/SummaryScreenController.cs");
            Assert.That(File.Exists(bootstrapPath), Is.True, "ClientBootstrap.cs should be available from the Unity project root.");
            Assert.That(File.Exists(shellPath), Is.True, "AppShellController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(summaryPath), Is.True, "SummaryScreenController.cs should be available from the Unity project root.");

            var bootstrap = File.ReadAllText(bootstrapPath);
            var shell = File.ReadAllText(shellPath);
            var summary = File.ReadAllText(summaryPath);

            Assert.That(bootstrap, Does.Contain("HandleBootstrapCityRequestedAsync"));
            Assert.That(bootstrap, Does.Contain("BeginSettlementBootstrap"));
            Assert.That(bootstrap, Does.Contain("BootstrapCityAsync"));
            Assert.That(shell, Does.Contain("onBootstrapCityRequested"));
            Assert.That(summary, Does.Contain("founderCityPrimaryButton"));
            Assert.That(summary, Does.Contain("founderMarketPrimaryButton"));
            Assert.That(summary, Does.Contain("RequestSettlementBootstrap"));
            Assert.That(summary, Does.Contain("\"city\""));
            Assert.That(summary, Does.Contain("\"black_market\""));
        }

        [Test]
        public void Founder_setup_surfaces_bootstrap_failures_instead_of_silent_duplicate_name_failures()
        {
            var bootstrapPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/ClientBootstrap.cs");
            var shellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/AppShellController.cs");
            var summaryPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/Summary/SummaryScreenController.cs");
            Assert.That(File.Exists(bootstrapPath), Is.True, "ClientBootstrap.cs should be available from the Unity project root.");
            Assert.That(File.Exists(shellPath), Is.True, "AppShellController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(summaryPath), Is.True, "SummaryScreenController.cs should be available from the Unity project root.");

            var bootstrap = File.ReadAllText(bootstrapPath);
            var shell = File.ReadAllText(shellPath);
            var summary = File.ReadAllText(summaryPath);

            Assert.That(bootstrap, Does.Contain("PlanarWarApiException ex"));
            Assert.That(bootstrap, Does.Contain("city_name_taken"));
            Assert.That(bootstrap, Does.Contain("Choose another settlement name"));
            Assert.That(shell, Does.Contain("summaryState.ActionStatus"));
            Assert.That(summary, Does.Contain("founderActionStatus"));
            Assert.That(summary, Does.Contain("founder-action-status--error"));
        }


        [Test]
        public void Auth_gate_exposes_login_and_register_screen_before_gameplay_setup()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            Assert.That(uxml, Does.Contain("auth-screen"));
            Assert.That(uxml, Does.Contain("login-name-field"));
            Assert.That(uxml, Does.Contain("password-field"));
            Assert.That(uxml, Does.Contain("register-handle-field"));
            Assert.That(uxml, Does.Contain("Display name"));
            Assert.That(uxml, Does.Contain("register-email-field"));
            Assert.That(uxml, Does.Contain("register-password-field"));
            Assert.That(uxml, Does.Contain("register-confirm-password-field"));
            Assert.That(uxml, Does.Contain("login-button"));
            Assert.That(uxml, Does.Contain("register-button"));
            Assert.That(uxml, Does.Contain("After sign-in, Home opens the current settlement setup flow"));
            Assert.That(uxml, Does.Not.Contain("starter city created"));
            Assert.That(uxml, Does.Not.Contain("starter inventory granted"));
        }

        [Test]
        public void Client_has_account_registration_http_contract()
        {
            var apiClientPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Network/Http/PlanarWarApiClient.cs");
            Assert.That(File.Exists(apiClientPath), Is.True, "PlanarWarApiClient.cs should be available from the Unity project root.");

            var source = File.ReadAllText(apiClientPath);
            Assert.That(source, Does.Contain("RegisterAsync"));
            Assert.That(source, Does.Contain("/api/auth/register"));
            Assert.That(source, Does.Contain("[\"displayName\"]"));
            Assert.That(source, Does.Contain("[\"email\"]"));
            Assert.That(source, Does.Contain("[\"password\"]"));
            Assert.That(source, Does.Contain("includeBearerToken: false"));
        }

        [Test]
        public void Client_bootstrap_wires_registration_to_auth_gate_without_bypassing_summary_setup()
        {
            var bootstrapPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/ClientBootstrap.cs");
            var authControllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Core/Application/AuthSessionController.cs");
            var shellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/AppShellController.cs");
            Assert.That(File.Exists(bootstrapPath), Is.True, "ClientBootstrap.cs should be available from the Unity project root.");
            Assert.That(File.Exists(authControllerPath), Is.True, "AuthSessionController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(shellPath), Is.True, "AppShellController.cs should be available from the Unity project root.");

            var bootstrap = File.ReadAllText(bootstrapPath);
            var authController = File.ReadAllText(authControllerPath);
            var shell = File.ReadAllText(shellPath);

            Assert.That(bootstrap, Does.Contain("Register()"));
            Assert.That(bootstrap, Does.Contain("authController.RegisterAsync"));
            Assert.That(bootstrap, Does.Contain("navigationState.SetActive(ShellScreen.Summary)"));
            Assert.That(authController, Does.Contain("apiClient.RegisterAsync"));
            Assert.That(authController, Does.Contain("displayName"));
            Assert.That(authController, Does.Contain("Account created. Signing in..."));
            Assert.That(shell, Does.Contain("authRoot"));
            Assert.That(shell, Does.Contain("isAuthenticated && navigationState.ActiveScreen"));
            Assert.That(bootstrap, Does.Not.Contain("BootstrapCityAsync(handle"));
            Assert.That(bootstrap, Does.Not.Contain("BootstrapCityAsync(email"));
        }

        [Test]
        public void Home_surface_exposes_post_founder_handoff_without_fake_progress_claims()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            Assert.That(uxml, Does.Contain("post-founder-handoff-card"));
            Assert.That(uxml, Does.Contain("post-founder-development-button"));
            Assert.That(uxml, Does.Contain("post-founder-operations-button"));
            Assert.That(uxml, Does.Contain("post-founder-roster-button"));
            Assert.That(uxml, Does.Contain("Client route only"));
            Assert.That(uxml, Does.Contain("do not invent setup progress, rewards, timers, inventory, or town layout state"));
            Assert.That(uxml, Does.Not.Contain("Use City / Black Market tabs"));
            Assert.That(uxml, Does.Not.Contain("starter rewards"));
            Assert.That(uxml, Does.Not.Contain("2D town layout"));
        }

        [Test]
        public void Client_wires_post_founder_handoff_to_existing_desks_only()
        {
            var shellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/AppShellController.cs");
            var summaryPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/Summary/SummaryScreenController.cs");
            Assert.That(File.Exists(shellPath), Is.True, "AppShellController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(summaryPath), Is.True, "SummaryScreenController.cs should be available from the Unity project root.");

            var shell = File.ReadAllText(shellPath);
            var summary = File.ReadAllText(summaryPath);

            Assert.That(shell, Does.Contain("BuildPostFounderActionHint"));
            Assert.That(shell, Does.Contain("Development for buildings and research"));
            Assert.That(shell, Does.Contain("Development for fronts and shadow-book research"));
            Assert.That(shell, Does.Not.Contain("Use City / Black Market tabs"));
            Assert.That(summary, Does.Contain("RenderPostFounderHandoff"));
            Assert.That(summary, Does.Contain("postFounderDevelopmentButton"));
            Assert.That(summary, Does.Contain("postFounderOperationsButton"));
            Assert.That(summary, Does.Contain("postFounderRosterButton"));
            Assert.That(summary, Does.Contain("ShellScreen.City"));
            Assert.That(summary, Does.Contain("ShellScreen.BlackMarket"));
            Assert.That(summary, Does.Contain("ShellScreen.Heroes"));
            Assert.That(summary, Does.Not.Contain("BootstrapCityAsync"));
        }

        [Test]
        public void Home_quick_orders_open_development_instead_of_exposing_a_dead_research_button()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var bootstrapPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/ClientBootstrap.cs");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(bootstrapPath), Is.True, "ClientBootstrap.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var bootstrap = File.ReadAllText(bootstrapPath);

            Assert.That(uxml, Does.Contain("home-development-button"));
            Assert.That(uxml, Does.Contain("Open Development"));
            Assert.That(uxml, Does.Not.Contain("name=\"start-research-button\" text=\"Start suggested research\""));
            Assert.That(bootstrap, Does.Contain("home-development-button"));
            Assert.That(bootstrap, Does.Contain("navigationState.SetActive(ShellScreen.City)"));
            Assert.That(bootstrap, Does.Not.Contain("root.Q<Button>(\"start-research-button\")"));
        }

        [Test]
        public void Workshop_surface_exposes_slot_and_recipe_picker_for_catalog_navigation()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var cityControllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/City/CityScreenController.cs");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(cityControllerPath), Is.True, "CityScreenController.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var city = File.ReadAllText(cityControllerPath);

            Assert.That(uxml, Does.Contain("dev-workshop-recipe-picker"));
            Assert.That(uxml, Does.Contain("dev-workshop-slot-field"));
            Assert.That(uxml, Does.Contain("dev-workshop-recipe-field"));
            Assert.That(uxml, Does.Contain("dev-workshop-craft-selected-button"));
            Assert.That(city, Does.Contain("FilterWorkshopRecipesBySelectedSlot"));
            Assert.That(city, Does.Contain("GetWorkshopRecipeSlotKey"));
            Assert.That(city, Does.Contain("selectedWorkshopRecipeId"));
            Assert.That(city, Does.Contain("TriggerStartWorkshopCraft(selectedWorkshopRecipeId)"));
        }

        [Test]
        public void Workshop_recipe_contract_preserves_optional_gear_slot_truth()
        {
            var contractPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Core/Contracts/ShellSummarySnapshot.cs");
            var refreshPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Core/Application/SummaryRefreshController.cs");
            Assert.That(File.Exists(contractPath), Is.True, "ShellSummarySnapshot.cs should be available from the Unity project root.");
            Assert.That(File.Exists(refreshPath), Is.True, "SummaryRefreshController.cs should be available from the Unity project root.");

            var contract = File.ReadAllText(contractPath);
            var refresh = File.ReadAllText(refreshPath);

            Assert.That(contract, Does.Contain("GearSlot"));
            Assert.That(refresh, Does.Contain("gearSlot"));
            Assert.That(refresh, Does.Contain("equipmentSlot"));
            Assert.That(refresh, Does.Contain("targetSlot"));
            Assert.That(refresh, Does.Contain("[\"template\"]?[\"slot\"]"));
        }


        [Test]
        public void Player_tester_guide_doc_is_checkpointed_without_packaging_dev_closeout_notes()
        {
            var closeoutPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Docs/CLIENT_GAMEPLAY_SURFACE_CLOSEOUT_V1.md");
            var guidePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Docs/PLAYER_TESTER_GUIDE_V1.md");

            Assert.That(File.Exists(closeoutPath), Is.False, "Client gameplay surface closeout notes are dev-facing and should not be packaged into the Unity tester client.");
            Assert.That(File.Exists(guidePath), Is.True, "Player tester guide should be present from the Unity project root.");

            var guide = File.ReadAllText(guidePath);

            Assert.That(guide, Does.Contain("First run"));
            Assert.That(guide, Does.Contain("Founder mode"));
            Assert.That(guide, Does.Contain("Workshop crafting"));
            Assert.That(guide, Does.Contain("Lane posture and first-hour action path"));
            Assert.That(guide, Does.Contain("live server action-path truth"));
            Assert.That(guide, Does.Contain("Urgent pressure"));
            Assert.That(guide, Does.Contain("live server pressure truth"));
            Assert.That(guide, Does.Contain("The urgent pressure button is route-only"));
            Assert.That(guide, Does.Contain("It does not complete objectives, grant rewards, start timers, or fake progress"));
            Assert.That(guide, Does.Contain("Operations desk"));
            Assert.That(guide, Does.Contain("Heroes / Operatives desk"));
            Assert.That(guide, Does.Contain("What testers should report"));
            Assert.That(guide, Does.Contain("generated 2D town layout images"));
            Assert.That(guide, Does.Contain("Good smoke-test route"));
        }

        [Test]
        public void Tester_guide_is_accessible_from_client_shell_without_backend_actions()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            Assert.That(uxml, Does.Contain("guide-screen"));
            Assert.That(uxml, Does.Contain("nav-guide-button"));
            Assert.That(uxml, Does.Contain("auth-guide-button"));
            Assert.That(uxml, Does.Contain("home-guide-button"));
            Assert.That(uxml, Does.Contain("guide-back-home-button"));
            Assert.That(uxml, Does.Contain("No fake tutorial progress"));
            Assert.That(uxml, Does.Contain("What to report"));
            Assert.That(uxml, Does.Contain("City or Black Market"));
            Assert.That(uxml, Does.Contain("Workshop"));
            Assert.That(uxml, Does.Contain("Lane posture"));
            Assert.That(uxml, Does.Contain("First-hour action path"));
            Assert.That(uxml, Does.Contain("Urgent pressure"));
            Assert.That(uxml, Does.Contain("Pressure lead"));
            Assert.That(uxml, Does.Contain("does not spawn events, rewards, timers, Rogue Director, TOMS, or Crucible behavior"));
            Assert.That(uxml, Does.Contain("wrong first-hour action path"));
            Assert.That(uxml, Does.Contain("wrong urgent pressure card"));
            Assert.That(uxml, Does.Not.Contain("tutorial complete"));
        }

        [Test]
        public void Shell_navigation_includes_tester_guide_as_static_help_surface()
        {
            var navigationPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Core/Application/ShellNavigationState.cs");
            var bootstrapPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/ClientBootstrap.cs");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/AppShellController.cs");

            Assert.That(File.Exists(navigationPath), Is.True);
            Assert.That(File.Exists(bootstrapPath), Is.True);
            Assert.That(File.Exists(controllerPath), Is.True);

            var navigation = File.ReadAllText(navigationPath);
            var bootstrap = File.ReadAllText(bootstrapPath);
            var controller = File.ReadAllText(controllerPath);

            Assert.That(navigation, Does.Contain("Guide"));
            Assert.That(bootstrap, Does.Contain("nav-guide-button"));
            Assert.That(bootstrap, Does.Contain("auth-guide-button"));
            Assert.That(bootstrap, Does.Contain("home-guide-button"));
            Assert.That(bootstrap, Does.Contain("guide-back-home-button"));
            Assert.That(controller, Does.Contain("guideRoot"));
            Assert.That(controller, Does.Contain("ShellScreen.Guide"));
            Assert.That(controller, Does.Contain("navGuideButton?.SetEnabled(true)"));
        }

        [Test]
        public void Tester_guide_mentions_lane_posture_action_path_without_progress_claims()
        {
            var guidePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Docs/PLAYER_TESTER_GUIDE_V1.md");
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(guidePath), Is.True);
            Assert.That(File.Exists(appShellPath), Is.True);

            var guide = File.ReadAllText(guidePath);
            var uxml = File.ReadAllText(appShellPath);

            Assert.That(guide, Does.Contain("Lane posture and first-hour action path"));
            Assert.That(guide, Does.Contain("It does not complete objectives, grant rewards, start timers, or fake progress"));
            Assert.That(guide, Does.Contain("Use recommended route buttons only to move to suggested live desks"));
            Assert.That(guide, Does.Contain("Urgent pressure"));
            Assert.That(guide, Does.Contain("does not launch events, complete objectives, spawn rewards, bypass blockers, start timers, or make Mother Brain autonomous"));
            Assert.That(guide, Does.Contain("Rogue Director, TOMS, Crucible, and full world-director behavior remain future work"));
            Assert.That(uxml, Does.Contain("This guide does not complete objectives, grant rewards, start timers, or fake tutorial progress"));
            Assert.That(uxml, Does.Contain("missing lane posture, wrong first-hour action path"));
            Assert.That(uxml, Does.Contain("urgent pressure card"));
        }

        private static VisualElement BuildMinimalHeroControllerRoot()
        {
            var root = new VisualElement();

            void AddLabel(string name) => root.Add(new Label { name = name });
            void AddDropdown(string name) => root.Add(new DropdownField { name = name });
            void AddElement(string name) => root.Add(new VisualElement { name = name });
            void AddButton(string name) => root.Add(new Button { name = name });

            foreach (var name in new[]
            {
                "heroes-headline-value",
                "heroes-copy-value",
                "heroes-overview-value",
                "heroes-recruitment-value",
                "heroes-roster-value",
                "heroes-availability-value",
                "heroes-armory-value",
                "heroes-selected-slot-current-value",
                "heroes-selected-slot-compatible-value",
                "heroes-note-value",
            })
            {
                AddLabel(name);
            }

            foreach (var name in new[]
            {
                "heroes-manage-hero-field",
                "heroes-manage-candidate-field",
                "heroes-gear-slot-field",
                "heroes-armory-item-field",
            })
            {
                AddDropdown(name);
            }

            foreach (var name in new[]
            {
                "heroes-roster-picker",
                "heroes-gear-slot-picker",
                "heroes-armory-item-picker",
                "heroes-candidate-picker",
            })
            {
                AddElement(name);
            }

            foreach (var name in new[]
            {
                "heroes-release-button",
                "heroes-equip-armory-button",
                "heroes-unequip-gear-button",
                "heroes-recruit-button",
                "heroes-candidate-accept-button",
                "heroes-candidate-dismiss-button",
                "heroes-refresh-button",
            })
            {
                AddButton(name);
            }

            return root;
        }

        [Test]
        public void Mapper_captures_early_lane_posture_truth()
        {
            const string payload = @"{
                ""hasCity"": true,
                ""city"": { ""name"": ""Quiet Ledger"", ""settlementLane"": ""black_market"", ""settlementLaneProfile"": { ""label"": ""Black Market"" } },
                ""earlyLanePosture"": {
                    ""lane"": ""black_market"",
                    ""label"": ""Black Market"",
                    ""headline"": ""Shadow settlement building deniable leverage."",
                    ""summary"": ""The first honest decisions should protect route leverage."",
                    ""strengths"": [""cashflow"", ""intel""],
                    ""liabilities"": [""exposure risk""],
                    ""recommendedDesk"": ""operations"",
                    ""recommendedActionLabel"": ""Open Operations for a low-exposure route"",
                    ""nextStepReason"": ""Offers are visible."",
                    ""proofSignals"": [""Settlement lane: black_market.""],
                    ""actionPath"": {
                        ""lane"": ""black_market"",
                        ""title"": ""Build deniable leverage"",
                        ""currentStep"": ""Choose a low-exposure route or cell action from Operations."",
                        ""recommendedDesk"": ""operations"",
                        ""recommendedActionLabel"": ""Open Operations for a low-exposure route"",
                        ""whyThisMatters"": ""The Black Market first hour should begin converting cashflow and intel into controlled pressure."",
                        ""liveProofSignals"": [""Shadow opening stock: wealth 18 / knowledge 4 / materials 6.""],
                        ""nextReceiptFamily"": ""shadow_opening_operation""
                    }
                }
            }";

            var summary = ShellSummarySnapshotMapper.Map(payload);

            Assert.That(summary.EarlyLanePosture, Is.Not.Null);
            Assert.That(summary.EarlyLanePosture.Lane, Is.EqualTo("black_market"));
            Assert.That(summary.EarlyLanePosture.Label, Is.EqualTo("Black Market"));
            Assert.That(summary.EarlyLanePosture.Headline, Does.Contain("Shadow settlement"));
            Assert.That(summary.EarlyLanePosture.Strengths, Does.Contain("cashflow"));
            Assert.That(summary.EarlyLanePosture.Liabilities, Does.Contain("exposure risk"));
            Assert.That(summary.EarlyLanePosture.RecommendedDesk, Is.EqualTo("operations"));
            Assert.That(summary.EarlyLanePosture.RecommendedActionLabel, Does.Contain("Open Operations"));
            Assert.That(summary.EarlyLanePosture.NextStepReason, Does.Contain("Offers"));
            Assert.That(summary.EarlyLanePosture.ProofSignals, Does.Contain("Settlement lane: black_market."));
            Assert.That(summary.EarlyLanePosture.ActionPath, Is.Not.Null);
            Assert.That(summary.EarlyLanePosture.ActionPath.Title, Is.EqualTo("Build deniable leverage"));
            Assert.That(summary.EarlyLanePosture.ActionPath.CurrentStep, Does.Contain("low-exposure route"));
            Assert.That(summary.EarlyLanePosture.ActionPath.RecommendedDesk, Is.EqualTo("operations"));
            Assert.That(summary.EarlyLanePosture.ActionPath.RecommendedActionLabel, Does.Contain("Open Operations"));
            Assert.That(summary.EarlyLanePosture.ActionPath.WhyThisMatters, Does.Contain("Black Market first hour"));
            Assert.That(summary.EarlyLanePosture.ActionPath.LiveProofSignals, Does.Contain("Shadow opening stock: wealth 18 / knowledge 4 / materials 6."));
            Assert.That(summary.EarlyLanePosture.ActionPath.NextReceiptFamily, Is.EqualTo("shadow_opening_operation"));
        }

        [Test]
        public void Home_surface_consumes_early_lane_posture_without_bootstrap_side_effects()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            Assert.That(uxml, Does.Contain("early-lane-posture-card"));
            Assert.That(uxml, Does.Contain("early-lane-posture-headline-value"));
            Assert.That(uxml, Does.Contain("early-lane-posture-recommended-value"));
            Assert.That(uxml, Does.Contain("early-lane-posture-strengths-value"));
            Assert.That(uxml, Does.Contain("early-lane-posture-liabilities-value"));
            Assert.That(uxml, Does.Contain("early-lane-posture-proof-value"));
            Assert.That(uxml, Does.Contain("early-lane-posture-action-button"));
            Assert.That(uxml, Does.Contain("early-lane-posture-action-path-title-value"));
            Assert.That(uxml, Does.Contain("early-lane-posture-action-path-step-value"));
            Assert.That(uxml, Does.Contain("early-lane-posture-action-path-why-value"));
            Assert.That(uxml, Does.Contain("early-lane-posture-action-path-receipt-value"));
            Assert.That(uxml, Does.Contain("This card shows live lane posture and action-path guidance when the server provides it."));
            Assert.That(uxml, Does.Not.Contain("early-lane-posture-bootstrap"));
        }



        [Test]
        public void Tester_guide_explains_mother_brain_pressure_path_without_fiat_claims()
        {
            var guidePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Docs/PLAYER_TESTER_GUIDE_V1.md");
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(guidePath), Is.True);
            Assert.That(File.Exists(appShellPath), Is.True);

            var guide = File.ReadAllText(guidePath);
            var uxml = File.ReadAllText(appShellPath);

            Assert.That(guide, Does.Contain("Urgent pressure"));
            Assert.That(guide, Does.Contain("live server pressure truth"));
            Assert.That(guide, Does.Contain("blockers that currently prevent clean follow-through"));
            Assert.That(guide, Does.Contain("proof signals that explain which pressure seam produced the recommendation"));
            Assert.That(guide, Does.Contain("next report type testers should expect"));
            Assert.That(guide, Does.Contain("report follow-through state, latest report, outcome, server response, and source-region proof"));
            Assert.That(guide, Does.Contain("blocker recovery"));
            Assert.That(guide, Does.Contain("Report follow-through is report/ledger truth only"));
            Assert.That(guide, Does.Contain("response history"));
            Assert.That(guide, Does.Contain("does not launch events, complete objectives, spawn rewards, bypass blockers, start timers, or make Mother Brain autonomous"));
            Assert.That(guide, Does.Contain("Rogue Director, TOMS, Crucible, and full world-director behavior remain future work"));
            Assert.That(guide, Does.Contain("missing or wrong urgent pressure card, blockers, blocker recovery, response history, proof signals, report follow-through, or report family"));
            Assert.That(uxml, Does.Contain("Urgent pressure"));
            Assert.That(uxml, Does.Contain("Pressure lead"));
            Assert.That(uxml, Does.Contain("blockers, proof signals, recent reports, and next report type"));
            Assert.That(uxml, Does.Contain("blocker-recovery"));
            Assert.That(uxml, Does.Contain("response history"));
            Assert.That(uxml, Does.Contain("does not spawn events, rewards, timers, Rogue Director, TOMS, or Crucible behavior"));
            Assert.That(uxml, Does.Contain("missing or wrong urgent pressure card"));
        }


        [Test]
        public void Mapper_captures_mother_brain_pressure_action_path_truth()
        {
            const string payload = @"{
                ""hasCity"": true,
                ""city"": { ""name"": ""TesterCity"", ""settlementLane"": ""city"", ""settlementLaneProfile"": { ""label"": ""City"" } },
                ""motherBrainPressureStatus"": {
                    ""severity"": ""urgent"",
                    ""headline"": ""Mother Brain says hostile-force pressure is opening a live civic seam."",
                    ""detail"": ""The route will harden if the answer stalls."",
                    ""recommendedAction"": ""Launch the hottest Mother Brain follow-through contract."",
                    ""incidentReady"": false,
                    ""incidentBlockedBy"": [""missing_response_lane""],
                    ""topPressureId"": ""mb_pressure_demo"",
                    ""topThreatFamily"": ""organized_hostile_forces"",
                    ""topReplayStatus"": ""live"",
                    ""topReplayQuality"": ""backsliding"",
                    ""topBurdenReceipt"": { ""state"": ""answered"" },
                    ""actionPath"": {
                        ""title"": ""Prepare the blocked response lane"",
                        ""currentStep"": ""Clear blockers before launching the follow-through contract."",
                        ""recommendedDesk"": ""operations"",
                        ""recommendedActionLabel"": ""Open Operations and prepare the response lane"",
                        ""whyThisMatters"": ""Mother Brain has a live pressure seam, but blockers prevent clean follow-through."",
                        ""blockers"": [""missing_response_lane""],
                        ""liveProofSignals"": [""Pressure: urgent."", ""Replay: backsliding.""],
                        ""nextReceiptFamily"": ""mother_brain_blocked_followthrough"",
                        ""receiptFollowThrough"": {
                            ""state"": ""blocked"",
                            ""title"": ""Prepare the blocked response lane"",
                            ""summary"": ""The seam is blocked by missing response lane; this does not spawn events, grant rewards, fake timers, or bypass blockers."",
                            ""latestReceiptTitle"": ""Broken salt line"",
                            ""latestReceiptAt"": ""2026-05-01T10:00:00.000Z"",
                            ""latestReceiptOutcome"": ""failure"",
                            ""latestReceiptState"": ""answered"",
                            ""latestRuntimeActionId"": ""action_region_ancient_elwynn"",
                            ""sourceRegionId"": ""ancient_elwynn"",
                            ""signals"": [""Runtime responses: 1."", ""Burden receipt state: answered.""],
                            ""responseHistory"": [
                                {
                                    ""id"": ""receipt_broken_salt_line"",
                                    ""createdAt"": ""2026-05-01T10:00:00.000Z"",
                                    ""title"": ""Broken salt line"",
                                    ""summary"": ""The answer landed, but the corridor started slipping again."",
                                    ""outcome"": ""failure"",
                                    ""severity"": ""pressure"",
                                    ""receiptState"": ""answered"",
                                    ""runtimeActionId"": ""action_region_ancient_elwynn"",
                                    ""sourceRegionId"": ""ancient_elwynn"",
                                    ""threatFamily"": ""organized_hostile_forces"",
                                    ""contractKind"": ""repair_works"",
                                    ""signals"": [""Runtime action: action_region_ancient_elwynn.""]
                                }
                            ],
                            ""blockerRecovery"": {
                                ""state"": ""blocked"",
                                ""title"": ""Recent follow-through is still cooling"",
                                ""summary"": ""Wait for the latest receipt or replay state to clear before launching another Mother Brain answer."",
                                ""blockers"": [""recent_followthrough""],
                                ""clearWhen"": [""The recent follow-through receipt exits the cooling/recovery window.""],
                                ""recommendedDesk"": ""operations"",
                                ""recommendedActionLabel"": ""Open Operations and monitor pressure readiness"",
                                ""signals"": [""Recent follow-through blocker is active.""]
                            }
                        }
                    }
                }
            }";

            var summary = ShellSummarySnapshotMapper.Map(payload);

            Assert.That(summary.MotherBrainPressureStatus, Is.Not.Null);
            Assert.That(summary.MotherBrainPressureStatus.Severity, Is.EqualTo("urgent"));
            Assert.That(summary.MotherBrainPressureStatus.Headline, Does.Contain("Mother Brain"));
            Assert.That(summary.MotherBrainPressureStatus.IncidentReady, Is.False);
            Assert.That(summary.MotherBrainPressureStatus.IncidentBlockedBy, Does.Contain("missing_response_lane"));
            Assert.That(summary.MotherBrainPressureStatus.TopPressureId, Is.EqualTo("mb_pressure_demo"));
            Assert.That(summary.MotherBrainPressureStatus.TopThreatFamily, Is.EqualTo("organized_hostile_forces"));
            Assert.That(summary.MotherBrainPressureStatus.TopReplayStatus, Is.EqualTo("live"));
            Assert.That(summary.MotherBrainPressureStatus.TopReplayQuality, Is.EqualTo("backsliding"));
            Assert.That(summary.MotherBrainPressureStatus.TopBurdenReceiptState, Is.EqualTo("answered"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath, Is.Not.Null);
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.Title, Is.EqualTo("Prepare the blocked response lane"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.CurrentStep, Does.Contain("Clear blockers"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.RecommendedDesk, Is.EqualTo("operations"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.RecommendedActionLabel, Does.Contain("Open Operations"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.WhyThisMatters, Does.Contain("live pressure seam"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.Blockers, Does.Contain("missing_response_lane"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.LiveProofSignals, Does.Contain("Pressure: urgent."));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.NextReceiptFamily, Is.EqualTo("mother_brain_blocked_followthrough"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough, Is.Not.Null);
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.State, Is.EqualTo("blocked"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.LatestReceiptTitle, Is.EqualTo("Broken salt line"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.LatestReceiptOutcome, Is.EqualTo("failure"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.LatestReceiptState, Is.EqualTo("answered"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.LatestRuntimeActionId, Is.EqualTo("action_region_ancient_elwynn"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.SourceRegionId, Is.EqualTo("ancient_elwynn"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.Signals, Does.Contain("Runtime responses: 1."));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.ResponseHistory, Has.Count.EqualTo(1));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.ResponseHistory[0].Id, Is.EqualTo("receipt_broken_salt_line"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.ResponseHistory[0].Title, Is.EqualTo("Broken salt line"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.ResponseHistory[0].Outcome, Is.EqualTo("failure"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.ResponseHistory[0].ReceiptState, Is.EqualTo("answered"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.ResponseHistory[0].RuntimeActionId, Is.EqualTo("action_region_ancient_elwynn"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.ResponseHistory[0].SourceRegionId, Is.EqualTo("ancient_elwynn"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.ResponseHistory[0].ThreatFamily, Is.EqualTo("organized_hostile_forces"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.ResponseHistory[0].ContractKind, Is.EqualTo("repair_works"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.ResponseHistory[0].Signals, Does.Contain("Runtime action: action_region_ancient_elwynn."));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.BlockerRecovery, Is.Not.Null);
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.BlockerRecovery.State, Is.EqualTo("blocked"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.BlockerRecovery.Blockers, Does.Contain("recent_followthrough"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.BlockerRecovery.ClearWhen, Does.Contain("The recent follow-through receipt exits the cooling/recovery window."));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.BlockerRecovery.RecommendedDesk, Is.EqualTo("operations"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.BlockerRecovery.RecommendedActionLabel, Does.Contain("Open Operations"));
            Assert.That(summary.MotherBrainPressureStatus.ActionPath.ReceiptFollowThrough.BlockerRecovery.Signals, Does.Contain("Recent follow-through blocker is active."));
        }

        [Test]
        public void Home_surface_consumes_mother_brain_pressure_action_path_without_fake_event_claims()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            Assert.That(uxml, Does.Contain("mother-brain-action-path-card"));
            Assert.That(uxml, Does.Contain("mother-brain-action-path-headline-value"));
            Assert.That(uxml, Does.Contain("mother-brain-action-path-recommended-value"));
            Assert.That(uxml, Does.Contain("mother-brain-action-path-blockers-value"));
            Assert.That(uxml, Does.Contain("mother-brain-action-path-proof-value"));
            Assert.That(uxml, Does.Contain("mother-brain-action-path-receipt-value"));
            Assert.That(uxml, Does.Contain("Recent pressure reports"));
            Assert.That(uxml, Does.Contain("blocker-recovery"));
            Assert.That(uxml, Does.Contain("response history"));
            Assert.That(uxml, Does.Contain("Report, blocker-recovery, and response history truth is waiting on server state."));
            Assert.That(uxml, Does.Contain("This card shows live pressure guidance when the server provides it."));
            Assert.That(uxml, Does.Not.Contain("Mother Brain starts events"));
            Assert.That(uxml, Does.Not.Contain("Mother Brain completes objectives"));
        }


        [Test]
        public void Mapper_captures_public_infrastructure_economy_spine_truth()
        {
            const string payload = @"{
                ""hasCity"": true,
                ""city"": { ""name"": ""TesterCity"", ""settlementLane"": ""city"", ""settlementLaneProfile"": { ""label"": ""City"" } },
                ""publicInfrastructureSummary"": {
                    ""permitTier"": ""trusted"",
                    ""serviceHeat"": 18,
                    ""queuePressure"": 7,
                    ""cityStressStage"": ""strained"",
                    ""cityStressTotal"": 24,
                    ""subsidyCreditsRemaining"": 3,
                    ""strainBand"": ""elevated"",
                    ""recommendedMode"": ""npc_public"",
                    ""pressureScore"": 42,
                    ""economySpine"": {
                        ""state"": ""strained"",
                        ""title"": ""Public economy spine is carrying visible strain"",
                        ""summary"": ""NPC public services remain viable, but public pressure is now visible."",
                        ""recommendedMode"": ""npc_public"",
                        ""recommendedService"": ""workshop_craft"",
                        ""recommendedActionLabel"": ""Open Development and compare public workshop service"",
                        ""whyThisMatters"": ""NPC public services remain the baseline spine; player-city infrastructure is an optimization lane, not a replacement."",
                        ""nextReceiptFamily"": ""public_infrastructure_service_receipt"",
                        ""publicBackboneSignals"": [""Public services remain reachable.""],
                        ""cityEconomySignals"": [""City infrastructure can reduce strain.""],
                        ""shadowRiskSignals"": [""No shadow exposure detected.""],
                        ""receiptFollowThrough"": {
                            ""state"": ""public_receipt_logged"",
                            ""title"": ""Public service receipt is logged"",
                            ""summary"": ""Latest receipt: workshop craft through npc public at 2026-05-02T04:44:00.000Z; queue 12m, strain 38/100. This surface reads existing public-service receipts only."",
                            ""latestReceiptId"": ""public_receipt_workshop_01"",
                            ""latestReceiptAt"": ""2026-05-02T04:44:00.000Z"",
                            ""latestService"": ""workshop_craft"",
                            ""latestMode"": ""npc_public"",
                            ""latestPermitTier"": ""trusted"",
                            ""latestQueueMinutes"": 12,
                            ""latestStrainScore"": 38,
                            ""latestRunwayDoctrine"": ""convoy_wardens"",
                            ""latestRunwayStatus"": ""cooling"",
                            ""receiptCount"": 2,
                            ""recommendedMode"": ""npc_public"",
                            ""recommendedService"": ""workshop_craft"",
                            ""nextReceiptFamily"": ""public_infrastructure_service_receipt"",
                            ""signals"": [""Receipt count: 2."", ""Runway context: convoy_wardens / cooling.""]
                        }
                    }
                }
            }";

            var summary = ShellSummarySnapshotMapper.Map(payload);

            Assert.That(summary.PublicInfrastructureSummary, Is.Not.Null);
            Assert.That(summary.PublicInfrastructureSummary.PermitTier, Is.EqualTo("trusted"));
            Assert.That(summary.PublicInfrastructureSummary.ServiceHeat, Is.EqualTo(18));
            Assert.That(summary.PublicInfrastructureSummary.QueuePressure, Is.EqualTo(7));
            Assert.That(summary.PublicInfrastructureSummary.CityStressStage, Is.EqualTo("strained"));
            Assert.That(summary.PublicInfrastructureSummary.CityStressTotal, Is.EqualTo(24));
            Assert.That(summary.PublicInfrastructureSummary.SubsidyCreditsRemaining, Is.EqualTo(3));
            Assert.That(summary.PublicInfrastructureSummary.StrainBand, Is.EqualTo("elevated"));
            Assert.That(summary.PublicInfrastructureSummary.RecommendedMode, Is.EqualTo("npc_public"));
            Assert.That(summary.PublicInfrastructureSummary.PressureScore, Is.EqualTo(42));

            var spine = summary.PublicInfrastructureSummary.EconomySpine;
            Assert.That(spine, Is.Not.Null);
            Assert.That(spine.State, Is.EqualTo("strained"));
            Assert.That(spine.Title, Does.Contain("Public economy spine"));
            Assert.That(spine.Summary, Does.Contain("NPC public services remain viable"));
            Assert.That(spine.RecommendedMode, Is.EqualTo("npc_public"));
            Assert.That(spine.RecommendedService, Is.EqualTo("workshop_craft"));
            Assert.That(spine.RecommendedActionLabel, Does.Contain("Open Development"));
            Assert.That(spine.WhyThisMatters, Does.Contain("baseline spine"));
            Assert.That(spine.NextReceiptFamily, Is.EqualTo("public_infrastructure_service_receipt"));
            Assert.That(spine.PublicBackboneSignals, Does.Contain("Public services remain reachable."));
            Assert.That(spine.CityEconomySignals, Does.Contain("City infrastructure can reduce strain."));
            Assert.That(spine.ShadowRiskSignals, Does.Contain("No shadow exposure detected."));
            Assert.That(spine.ReceiptFollowThrough, Is.Not.Null);
            Assert.That(spine.ReceiptFollowThrough.State, Is.EqualTo("public_receipt_logged"));
            Assert.That(spine.ReceiptFollowThrough.LatestReceiptId, Is.EqualTo("public_receipt_workshop_01"));
            Assert.That(spine.ReceiptFollowThrough.LatestService, Is.EqualTo("workshop_craft"));
            Assert.That(spine.ReceiptFollowThrough.LatestMode, Is.EqualTo("npc_public"));
            Assert.That(spine.ReceiptFollowThrough.LatestPermitTier, Is.EqualTo("trusted"));
            Assert.That(spine.ReceiptFollowThrough.LatestQueueMinutes, Is.EqualTo(12));
            Assert.That(spine.ReceiptFollowThrough.LatestStrainScore, Is.EqualTo(38));
            Assert.That(spine.ReceiptFollowThrough.LatestRunwayDoctrine, Is.EqualTo("convoy_wardens"));
            Assert.That(spine.ReceiptFollowThrough.LatestRunwayStatus, Is.EqualTo("cooling"));
            Assert.That(spine.ReceiptFollowThrough.ReceiptCount, Is.EqualTo(2));
            Assert.That(spine.ReceiptFollowThrough.Signals, Does.Contain("Receipt count: 2."));
        }

        [Test]
        public void Home_surfaces_public_infrastructure_economy_spine_without_fake_service_claims()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            Assert.That(uxml, Does.Contain("public-infrastructure-economy-spine-card"));
            Assert.That(uxml, Does.Contain("public-infrastructure-economy-spine-badge-value"));
            Assert.That(uxml, Does.Contain("public-infrastructure-economy-spine-recommended-value"));
            Assert.That(uxml, Does.Contain("public-infrastructure-economy-spine-public-signals-value"));
            Assert.That(uxml, Does.Contain("public-infrastructure-economy-spine-city-signals-value"));
            Assert.That(uxml, Does.Contain("public-infrastructure-economy-spine-shadow-signals-value"));
            Assert.That(uxml, Does.Contain("public-infrastructure-economy-spine-receipt-value"));
            Assert.That(uxml, Does.Contain("Recent service reports"));
            Assert.That(uxml, Does.Contain("Recent service reports are waiting on live server state."));
            Assert.That(uxml, Does.Contain("Public services"));
            Assert.That(uxml, Does.Not.Contain("publicInfrastructureSummary.economySpine"));
            Assert.That(uxml, Does.Not.Contain("public service taxes are live"));
            Assert.That(uxml, Does.Not.Contain("public queue timers are live"));
            Assert.That(uxml, Does.Not.Contain("public services grant rewards"));
            Assert.That(uxml, Does.Not.Contain("public services protect stock"));
        }

        [Test]
        public void Tester_guide_explains_public_infrastructure_economy_spine_guardrails()
        {
            var guidePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Docs/PLAYER_TESTER_GUIDE_V1.md");
            Assert.That(File.Exists(guidePath), Is.True, "PLAYER_TESTER_GUIDE_V1.md should ship with the Unity client.");

            var guide = File.ReadAllText(guidePath);
            Assert.That(guide, Does.Contain("Public services"));
            Assert.That(guide, Does.Contain("live server public-service truth"));
            Assert.That(guide, Does.Contain("NPC public services"));
            Assert.That(guide, Does.Contain("player-city infrastructure"));
            Assert.That(guide, Does.Contain("public-service report follow-through"));
            Assert.That(guide, Does.Contain("latest report"));
            Assert.That(guide, Does.Contain("runway context"));
            Assert.That(guide, Does.Contain("does not apply fake taxes"));
            Assert.That(guide, Does.Contain("queue timers"));
            Assert.That(guide, Does.Contain("service outcomes"));
            Assert.That(guide, Does.Contain("rewards"));
            Assert.That(guide, Does.Contain("Rogue Director"));
            Assert.That(guide, Does.Contain("TOMS"));
            Assert.That(guide, Does.Contain("Crucible"));
        }


        [Test]
        public void Mapper_captures_city_mud_world_consequence_bridge_truth()
        {
            const string payload = @"{
                ""hasCity"": true,
                ""city"": { ""name"": ""TesterCity"", ""settlementLane"": ""city"", ""settlementLaneProfile"": { ""label"": ""City"" } },
                ""cityMudWorldConsequenceBridge"": {
                    ""state"": ""pressured"",
                    ""title"": ""City-to-MUD consequence bridge is pressured"",
                    ""summary"": ""City support lanes are still usable, but pressure is high enough that MUD-facing support should be selective and receipt-backed."",
                    ""recommendedFocus"": ""city_support"",
                    ""recommendedActionLabel"": ""Use city support without making it mandatory"",
                    ""whyThisMatters"": ""This bridge reads existing city bridge, public infrastructure, world-consequence, and receipt truth only; it does not grant items or fake progression."",
                    ""nextReceiptFamily"": ""city_mud_support_receipt"",
                    ""followThrough"": {
                        ""state"": ""represented"",
                        ""title"": ""Bridge support is represented by receipts"",
                        ""summary"": ""Latest bridge/runtime receipt already represents this city-to-MUD support lane."",
                        ""recommendedFocus"": ""city_support"",
                        ""recommendedActionLabel"": ""Monitor the represented bridge lane"",
                        ""clearWhen"": [""Latest bridge/runtime receipt remains relevant.""],
                        ""watchNext"": [""Regional pressure and bridge band stay stable.""],
                        ""latestRuntimeResponseTitle"": ""Relief line"",
                        ""latestRuntimeResponseAt"": ""2026-05-02T09:05:00.000Z"",
                        ""latestRuntimeResponseOutcome"": ""partial"",
                        ""latestRuntimeActionId"": ""action_relief_line"",
                        ""latestWorldConsequenceTitle"": ""Regional pressure exported"",
                        ""latestWorldConsequenceAt"": ""2026-05-02T09:00:00.000Z"",
                        ""latestBridgeReceiptTitle"": ""City support receipt"",
                        ""latestBridgeReceiptAt"": ""2026-05-02T09:06:00.000Z"",
                        ""nextReceiptFamily"": ""city_mud_bridge_followthrough_receipt"",
                        ""signals"": [""Bridge follow-through state: represented.""]
                    },
                    ""bridgeBand"": ""strained"",
                    ""recommendedPosture"": ""balanced"",
                    ""supportCapacity"": 67,
                    ""logisticsPressure"": 22,
                    ""frontierPressure"": 19,
                    ""stabilityPressure"": 31,
                    ""exportableResources"": { ""food"": 44, ""materials"": 12, ""wealth"": 8 },
                    ""affectedRegionIds"": [""ancient_elwynn""],
                    ""worldConsequenceTotal"": 3,
                    ""severeConsequenceCount"": 1,
                    ""destabilizationScore"": 52,
                    ""cityMudSignals"": [""Bridge band: strained.""],
                    ""mudProgressionSignals"": [""Vendor supply: supported.""],
                    ""regionalLifeSignals"": [""Affected regions: ancient_elwynn.""],
                    ""receiptSignals"": [""Latest runtime response: Relief line.""],
                    ""guardrails"": [""Does not make player cities mandatory for baseline MUD viability."", ""Does not grant items, rewards, levels, or fake MUD progression.""],
                    ""latestWorldConsequence"": {
                        ""id"": ""wce_001"",
                        ""createdAt"": ""2026-05-02T09:00:00.000Z"",
                        ""title"": ""Regional pressure exported"",
                        ""summary"": ""A world consequence was logged."",
                        ""severity"": ""severe"",
                        ""source"": ""mission_setback"",
                        ""regionId"": ""ancient_elwynn""
                    },
                    ""latestRuntimeResponse"": {
                        ""id"": ""resp_001"",
                        ""createdAt"": ""2026-05-02T09:05:00.000Z"",
                        ""title"": ""Relief line"",
                        ""summary"": ""A bounded response was logged."",
                        ""severity"": ""pressure"",
                        ""outcome"": ""partial"",
                        ""source"": ""recovery_contract"",
                        ""regionId"": ""ancient_elwynn"",
                        ""runtimeActionId"": ""action_relief_line""
                    }
                }
            }";

            var summary = ShellSummarySnapshotMapper.Map(payload);
            var bridge = summary.CityMudWorldConsequenceBridge;

            Assert.That(bridge, Is.Not.Null);
            Assert.That(bridge.State, Is.EqualTo("pressured"));
            Assert.That(bridge.RecommendedFocus, Is.EqualTo("city_support"));
            Assert.That(bridge.RecommendedActionLabel, Does.Contain("Use city support"));
            Assert.That(bridge.NextReceiptFamily, Is.EqualTo("city_mud_support_receipt"));
            Assert.That(bridge.FollowThrough, Is.Not.Null);
            Assert.That(bridge.FollowThrough.State, Is.EqualTo("represented"));
            Assert.That(bridge.FollowThrough.RecommendedActionLabel, Does.Contain("Monitor"));
            Assert.That(bridge.FollowThrough.ClearWhen, Does.Contain("Latest bridge/runtime receipt remains relevant."));
            Assert.That(bridge.FollowThrough.WatchNext, Does.Contain("Regional pressure and bridge band stay stable."));
            Assert.That(bridge.FollowThrough.LatestRuntimeResponseTitle, Is.EqualTo("Relief line"));
            Assert.That(bridge.FollowThrough.LatestRuntimeResponseOutcome, Is.EqualTo("partial"));
            Assert.That(bridge.FollowThrough.LatestRuntimeActionId, Is.EqualTo("action_relief_line"));
            Assert.That(bridge.FollowThrough.LatestWorldConsequenceTitle, Is.EqualTo("Regional pressure exported"));
            Assert.That(bridge.FollowThrough.LatestBridgeReceiptTitle, Is.EqualTo("City support receipt"));
            Assert.That(bridge.FollowThrough.NextReceiptFamily, Is.EqualTo("city_mud_bridge_followthrough_receipt"));
            Assert.That(bridge.FollowThrough.Signals, Does.Contain("Bridge follow-through state: represented."));
            Assert.That(bridge.BridgeBand, Is.EqualTo("strained"));
            Assert.That(bridge.RecommendedPosture, Is.EqualTo("balanced"));
            Assert.That(bridge.SupportCapacity, Is.EqualTo(67));
            Assert.That(bridge.LogisticsPressure, Is.EqualTo(22));
            Assert.That(bridge.FrontierPressure, Is.EqualTo(19));
            Assert.That(bridge.StabilityPressure, Is.EqualTo(31));
            Assert.That(bridge.ExportableResources.Food, Is.EqualTo(44));
            Assert.That(bridge.ExportableResources.Materials, Is.EqualTo(12));
            Assert.That(bridge.ExportableResources.Wealth, Is.EqualTo(8));
            Assert.That(bridge.AffectedRegionIds, Does.Contain("ancient_elwynn"));
            Assert.That(bridge.WorldConsequenceTotal, Is.EqualTo(3));
            Assert.That(bridge.SevereConsequenceCount, Is.EqualTo(1));
            Assert.That(bridge.DestabilizationScore, Is.EqualTo(52));
            Assert.That(bridge.CityMudSignals, Does.Contain("Bridge band: strained."));
            Assert.That(bridge.MudProgressionSignals, Does.Contain("Vendor supply: supported."));
            Assert.That(bridge.RegionalLifeSignals, Does.Contain("Affected regions: ancient_elwynn."));
            Assert.That(bridge.ReceiptSignals, Does.Contain("Latest runtime response: Relief line."));
            Assert.That(bridge.Guardrails, Does.Contain("Does not make player cities mandatory for baseline MUD viability."));
            Assert.That(bridge.LatestWorldConsequence, Is.Not.Null);
            Assert.That(bridge.LatestWorldConsequence.Title, Is.EqualTo("Regional pressure exported"));
            Assert.That(bridge.LatestRuntimeResponse, Is.Not.Null);
            Assert.That(bridge.LatestRuntimeResponse.RuntimeActionId, Is.EqualTo("action_relief_line"));
        }

        [Test]
        public void Home_surfaces_city_mud_world_consequence_bridge_without_fake_progression_claims()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            Assert.That(uxml, Does.Contain("city-mud-consequence-bridge-card"));
            Assert.That(uxml, Does.Contain("city-mud-consequence-bridge-badge-value"));
            Assert.That(uxml, Does.Contain("city-mud-consequence-bridge-recommended-value"));
            Assert.That(uxml, Does.Contain("city-mud-consequence-bridge-bridge-signals-value"));
            Assert.That(uxml, Does.Contain("city-mud-consequence-bridge-progression-signals-value"));
            Assert.That(uxml, Does.Contain("city-mud-consequence-bridge-regional-signals-value"));
            Assert.That(uxml, Does.Contain("city-mud-consequence-bridge-receipt-signals-value"));
            Assert.That(uxml, Does.Contain("city-mud-consequence-bridge-follow-through-value"));
            Assert.That(uxml, Does.Contain("Support follow-through"));
            Assert.That(uxml, Does.Contain("city-mud-consequence-bridge-guardrails-value"));
            Assert.That(uxml, Does.Contain("Regional support"));
            Assert.That(uxml, Does.Contain("Waiting on regional support truth"));
            Assert.That(uxml, Does.Not.Contain("cityMudWorldConsequenceBridge"));
            Assert.That(uxml, Does.Not.Contain("cities grant item rewards"));
            Assert.That(uxml, Does.Not.Contain("player cities are mandatory"));
            Assert.That(uxml, Does.Not.Contain("fake MUD levels"));
        }

        [Test]
        public void Tester_guide_explains_city_mud_world_consequence_bridge_guardrails()
        {
            var guidePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Docs/PLAYER_TESTER_GUIDE_V1.md");
            Assert.That(File.Exists(guidePath), Is.True, "PLAYER_TESTER_GUIDE_V1.md should ship with the Unity client.");

            var guide = File.ReadAllText(guidePath);
            Assert.That(guide, Does.Contain("Regional support"));
            Assert.That(guide, Does.Contain("live server support truth"));
            Assert.That(guide, Does.Contain("city support"));
            Assert.That(guide, Does.Contain("MUD progression signals"));
            Assert.That(guide, Does.Contain("regional life signals"));
            Assert.That(guide, Does.Contain("recent report and consequence signals"));
            Assert.That(guide, Does.Contain("support follow-through"));
            Assert.That(guide, Does.Contain("clear-when guidance"));
            Assert.That(guide, Does.Contain("watch-next signals"));
            Assert.That(guide, Does.Contain("waiting, ready, restricted, strained, or represented"));
            Assert.That(guide, Does.Contain("player cities support/optimize public play without becoming mandatory"));
            Assert.That(guide, Does.Contain("does not grant items"));
            Assert.That(guide, Does.Contain("fake MUD progression"));
            Assert.That(guide, Does.Contain("Rogue Director"));
            Assert.That(guide, Does.Contain("TOMS"));
            Assert.That(guide, Does.Contain("Crucible"));
        }


        [Test]
        public void Bottom_chat_minimize_toggle_keeps_latest_line_visible_without_chat_architecture_changes()
        {
            var uxmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/AppShellController.cs");

            Assert.That(File.Exists(uxmlPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "AppShellController.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(uxmlPath);
            var uss = File.ReadAllText(ussPath);
            var controller = File.ReadAllText(controllerPath);

            Assert.That(uxml, Does.Contain("name=\"comms-toggle-button\""), "Bottom comms should expose a tester-facing minimize/expand toggle.");
            Assert.That(uxml, Does.Contain("text=\"Minimize\""), "Expanded chat starts with a Minimize action.");
            Assert.That(uxml, Does.Contain("name=\"chat-compose-row\""), "Compose row should be addressable so minimize can hide it without removing chat wiring.");
            Assert.That(uxml, Does.Contain("name=\"chat-filter-row\""), "Filter row should stay addressable when the compact state hides filter chips.");
            Assert.That(uss, Does.Contain("Bottom chat minimize toggle v1"));
            Assert.That(uss, Does.Contain(".comms-panel--minimized"));
            Assert.That(uss, Does.Contain(".chat-toggle-button"));
            Assert.That(controller, Does.Contain("private bool isCommsMinimized"));
            Assert.That(controller, Does.Contain("ToggleCommsMinimized"));
            Assert.That(controller, Does.Contain("commsToggleButton.clicked += ToggleCommsMinimized"));
            Assert.That(controller, Does.Contain("commsToggleButton.text = isCommsMinimized ? \"Expand\" : \"Minimize\""));
            Assert.That(controller, Does.Contain("chatLogScroll.style.display = isCommsMinimized ? DisplayStyle.None : DisplayStyle.Flex"));
            Assert.That(controller, Does.Contain("chatComposeRow.style.display = isCommsMinimized ? DisplayStyle.None : DisplayStyle.Flex"));
            Assert.That(controller, Does.Contain("BuildMinimizedCommsHint"));
            Assert.That(controller, Does.Not.Contain("city chat channel"));
            Assert.That(controller, Does.Not.Contain("guild relay"));
        }


        [Test]
        public void Bottom_chat_minimized_height_cleanup_keeps_future_attention_hooks_honest()
        {
            var uxmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/AppShellController.cs");

            Assert.That(File.Exists(uxmlPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "AppShellController.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(uxmlPath);
            var uss = File.ReadAllText(ussPath);
            var controller = File.ReadAllText(controllerPath);

            Assert.That(uxml, Does.Contain("name=\"comms-status-stack\""), "The minimized header should have a compact status stack instead of relying on loose anonymous layout.");
            Assert.That(uxml, Does.Contain("comms-hint-line"), "The minimized hint should be addressable for single-line compact styling.");
            Assert.That(uss, Does.Contain("Bottom chat minimized height cleanup v1"));
            Assert.That(uss, Does.Contain("min-height: 56px"), "Minimized chat should behave like a slim status bar, not a half-open chat log.");
            Assert.That(uss, Does.Contain(".comms-panel--attention"), "Future unread/priority comms can visually call attention without adding chat architecture in this slice.");
            Assert.That(uss, Does.Contain(".comms-panel--private-message"), "Future private-message styling hook should be present without faking private-message delivery.");
            Assert.That(uss, Does.Contain(".chat-toggle-button--private-message"), "The expand button can be highlighted later when private-message state exists.");
            Assert.That(controller, Does.Contain("RenderCommsAttentionState"));
            Assert.That(controller, Does.Contain("hasPrivateMessage"));
            Assert.That(controller, Does.Contain("hasUnreadPriorityComms: false, hasPrivateMessage: false"), "Current slice must not invent unread/private-message state before the chat system supplies it.");
            Assert.That(controller, Does.Not.Contain("city chat channel"));
            Assert.That(controller, Does.Not.Contain("guild relay"));
        }


        [Test]
        public void Bottom_chat_minimized_latest_line_cleanup_keeps_collapsed_copy_useful_without_fake_pm_state()
        {
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/AppShellController.cs");

            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "AppShellController.cs should be available from the Unity project root.");

            var uss = File.ReadAllText(ussPath);
            var controller = File.ReadAllText(controllerPath);

            Assert.That(uss, Does.Contain("Bottom chat minimized latest-line cleanup v1"));
            Assert.That(uss, Does.Contain(".comms-panel--minimized .comms-status-line"));
            Assert.That(controller, Does.Contain("BuildCommsLatestLine"));
            Assert.That(controller, Does.Contain("BuildMinimizedCommsHint"));
            Assert.That(controller, Does.Contain("CompactCommsText"));
            Assert.That(controller, Does.Contain("Latest: "), "Minimized chat should label the visible latest line instead of showing a generic tray headline.");
            Assert.That(controller, Does.Contain("Expand for log/send"), "Minimized hint should explain the collapse state without repeating the full expanded hint.");
            Assert.That(controller, Does.Contain("MinimizedCommsLatestMaxLength = 96"), "Collapsed latest text should be bounded so long messages do not eat the tray.");
            Assert.That(controller, Does.Contain("hasUnreadPriorityComms: false, hasPrivateMessage: false"), "This cleanup must not fake private-message or unread-priority state before chat truth exposes it.");
            Assert.That(controller, Does.Not.Contain("city chat channel"));
            Assert.That(controller, Does.Not.Contain("guild relay"));
        }


        [Test]
        public void Bottom_chat_expand_button_placement_cleanup_keeps_collapsed_toggle_aligned_without_chat_architecture_changes()
        {
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/AppShellController.cs");

            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "AppShellController.cs should be available from the Unity project root.");

            var uss = File.ReadAllText(ussPath);
            var controller = File.ReadAllText(controllerPath);

            Assert.That(uss, Does.Contain("Bottom chat expand button placement cleanup v1"));
            Assert.That(uss, Does.Contain(".comms-panel--minimized .comms-header-row"), "Collapsed comms should align the latest-line stack and Expand toggle through the minimized header row.");
            Assert.That(uss, Does.Contain(".comms-panel--minimized .chat-chip-row--minimized"), "Collapsed comms should keep the minimized chip row as a compact right-side toggle lane.");
            Assert.That(uss, Does.Contain("max-width: 92px"), "Collapsed Expand button should stay compact instead of stretching like a full chat filter chip.");
            Assert.That(controller, Does.Contain("chat-chip-row--minimized"), "Controller should keep toggling the minimized filter-row class instead of replacing chat state.");
            Assert.That(controller, Does.Contain("commsToggleButton.text = isCommsMinimized ? \"Expand\" : \"Minimize\""));
            Assert.That(controller, Does.Contain("hasUnreadPriorityComms: false, hasPrivateMessage: false"), "This layout cleanup must not fake private-message or unread-priority state before chat truth exposes it.");
            Assert.That(controller, Does.Not.Contain("city chat channel"));
            Assert.That(controller, Does.Not.Contain("guild relay"));
        }


        [Test]
        public void Header_card_height_cleanup_tightens_top_strip_without_changing_payload_truth()
        {
            var uxmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");

            Assert.That(File.Exists(uxmlPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(uxmlPath);
            var uss = File.ReadAllText(ussPath);

            Assert.That(uxml, Does.Contain("name=\"top-strip\""), "The top strip must remain the canonical header container.");
            Assert.That(uxml, Does.Contain("name=\"connection-value\""), "Connection payload truth should stay wired through the existing label.");
            Assert.That(uxml, Does.Contain("name=\"shard-value\""), "Shard payload truth should stay wired through the existing label.");
            Assert.That(uxml, Does.Contain("name=\"room-value\""), "Room / Pocket payload truth should stay wired through the existing label.");
            Assert.That(uxml, Does.Contain("name=\"last-updated-value\""), "Summary-status payload truth should stay wired through the existing label.");
            Assert.That(uxml, Does.Contain("name=\"account-value\""), "Account payload truth should stay wired through the existing label.");
            Assert.That(uss, Does.Contain("Header card height cleanup v1"));
            Assert.That(uss, Does.Contain(".top-strip .metric-card"), "Header card tightening should be scoped to the top strip instead of shrinking every metric card in the client.");
            Assert.That(uss, Does.Contain("min-height: 72px"), "Top header cards should be tightened enough to give the workspace more vertical room.");
            Assert.That(uss, Does.Contain(".top-strip .auth-button-row .action-button"), "The sign-out button should be compacted only inside the top strip auth card.");
        }


        [Test]
        public void Home_lane_posture_spacing_cleanup_tightens_card_without_changing_backend_truth()
        {
            var uxmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");

            Assert.That(File.Exists(uxmlPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(uxmlPath);
            var uss = File.ReadAllText(ussPath);

            Assert.That(uxml, Does.Contain("home-lane-posture-card"), "The Home lane posture card should have a scoped class for spacing-only layout cleanup.");
            Assert.That(uxml, Does.Contain("home-lane-posture__headline"));
            Assert.That(uxml, Does.Contain("home-lane-posture__summary"));
            Assert.That(uxml, Does.Contain("home-lane-posture__grid"));
            Assert.That(uxml, Does.Contain("This card shows live lane posture and action-path guidance when the server provides it."), "Server posture truth should remain the source instead of introducing client-side fake state.");
            Assert.That(uxml, Does.Contain("early-lane-posture-action-button"), "Existing client navigation affordance should remain wired.");
            Assert.That(uss, Does.Contain("Home lane posture spacing cleanup v1"));
            Assert.That(uss, Does.Contain(".home-lane-posture-card"));
            Assert.That(uss, Does.Contain("gap: 7px"));
            Assert.That(uss, Does.Contain("padding-top: 12px"));
            Assert.That(uss, Does.Contain(".home-lane-posture__grid .glance-card"));
            var postureStart = uxml.IndexOf("home-lane-posture-card", StringComparison.Ordinal);
            var postureEnd = uxml.IndexOf("mother-brain-action-path-card", postureStart >= 0 ? postureStart : 0, StringComparison.Ordinal);
            var postureSection = postureStart >= 0 && postureEnd > postureStart
                ? uxml.Substring(postureStart, postureEnd - postureStart)
                : string.Empty;

            Assert.That(uxml, Does.Not.Contain("early-lane-posture-bootstrap"));
            Assert.That(postureSection, Does.Not.Contain("fake tutorial progress"));
        }


        [Test]
        public void Home_content_scrollbar_width_cleanup_keeps_main_scrollbars_slim_without_touching_chat_or_rail_behavior()
        {
            var uxmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");

            Assert.That(File.Exists(uxmlPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(uxmlPath);
            var uss = File.ReadAllText(ussPath);

            Assert.That(uxml, Does.Contain("main-content-scroll"), "Main workspace screens should get the slim scrollbar class without changing screen identity or behavior.");
            Assert.That(uxml, Does.Contain("name=\"summary-screen\" class=\"screen-panel screen-scroll main-content-scroll\""));
            Assert.That(uxml, Does.Contain("name=\"development-screen\" class=\"screen-panel screen-scroll main-content-scroll\""));
            Assert.That(uxml, Does.Contain("name=\"heroes-screen\" class=\"screen-panel screen-scroll main-content-scroll\""));
            Assert.That(uxml, Does.Contain("name=\"social-screen\" class=\"screen-panel screen-scroll main-content-scroll\""));
            Assert.That(uxml, Does.Contain("name=\"chat-log-scroll\" class=\"chat-log-scroll\""), "Chat log scroll behavior should stay on its existing class instead of inheriting the main content scrollbar pass.");
            Assert.That(uxml, Does.Contain("name=\"left-rail-scroll\" class=\"rail-scroll\""), "Left rail scrolling should stay separate from main content scrollbar styling.");
            Assert.That(uss, Does.Contain("Home/content scrollbar width cleanup v1"));
            Assert.That(uss, Does.Contain(".main-content-scroll .unity-scroll-view__vertical-scroller"));
            Assert.That(uss, Does.Contain("width: 10px"), "Main workspace scrollbar track should be less visually chunky while keeping normal scroll behavior.");
            Assert.That(uss, Does.Contain(".main-content-scroll .unity-scroller__slider"));
            Assert.That(uss, Does.Not.Contain("fake scroll state"));
            Assert.That(uss, Does.Not.Contain("disable scrolling"));
        }


        [Test]
        public void Left_rail_scrollbar_width_cleanup_keeps_rail_scroll_slim_without_touching_main_or_chat_scrolls()
        {
            var uxmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");

            Assert.That(File.Exists(uxmlPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(uxmlPath);
            var uss = File.ReadAllText(ussPath);

            Assert.That(uxml, Does.Contain("name=\"left-rail-scroll\" class=\"rail-scroll\""), "Left rail scrolling should stay on its dedicated rail scrollview.");
            Assert.That(uxml, Does.Contain("name=\"chat-log-scroll\" class=\"chat-log-scroll\""), "Chat log scrolling should remain independent from rail scrollbar styling.");
            Assert.That(uxml, Does.Contain("main-content-scroll"), "Main workspace scrolling should remain independently styled from the left rail.");
            Assert.That(uss, Does.Contain("Left rail scrollbar width cleanup v1"));
            Assert.That(uss, Does.Contain(".rail-scroll .unity-scroll-view__vertical-scroller"));
            Assert.That(uss, Does.Contain("width: 9px"), "Left rail scrollbar track should be slimmer without disabling rail scrolling.");
            Assert.That(uss, Does.Contain(".rail-scroll .unity-scroller__slider"));
            Assert.That(uss, Does.Contain("width: 7px"), "Left rail scrollbar thumb should stay visible but less chunky.");
            Assert.That(uss, Does.Contain(".main-content-scroll .unity-scroll-view__vertical-scroller"), "Main content scrollbar styling should remain scoped to main workspace screens.");
            Assert.That(uss, Does.Contain(".chat-log-scroll"), "Chat log scroll styling should remain separately scoped.");
            Assert.That(uss, Does.Not.Contain("disable rail scrolling"));
            Assert.That(uss, Does.Not.Contain("fake scroll state"));
        }


        [Test]
        public void Bottom_chat_input_width_cleanup_aligns_compose_row_without_changing_send_truth()
        {
            var uxmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/AppShellController.cs");
            var bootstrapPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/ClientBootstrap.cs");

            Assert.That(File.Exists(uxmlPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "AppShellController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(bootstrapPath), Is.True, "ClientBootstrap.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(uxmlPath);
            var uss = File.ReadAllText(ussPath);
            var controller = File.ReadAllText(controllerPath);
            var bootstrap = File.ReadAllText(bootstrapPath);

            Assert.That(uxml, Does.Contain("name=\"chat-compose-row\" class=\"chat-compose-row chat-compose-row--aligned\""), "The bottom chat compose row should have a scoped alignment class instead of changing send behavior.");
            Assert.That(uxml, Does.Contain("chat-input-field--bottom-comms"), "The bottom chat input should have scoped width cleanup styling.");
            Assert.That(uxml, Does.Contain("chat-send-button--aligned"), "The Send button should be aligned through scoped styling, not rebuilt.");
            Assert.That(uss, Does.Contain("Bottom chat input width cleanup v1"));
            Assert.That(uss, Does.Contain(".chat-compose-row--aligned"));
            Assert.That(uss, Does.Contain(".chat-input-field--bottom-comms .unity-base-field__label"));
            Assert.That(uss, Does.Contain("min-width: 76px"), "The Chat message label and Send button should use a tighter, predictable width rhythm.");
            Assert.That(uss, Does.Contain(".chat-send-button--aligned"));
            Assert.That(controller, Does.Contain("sendChatButton?.SetEnabled(canSendRoomChat)"), "Existing send-button availability should remain driven by honest chat-room state.");
            Assert.That(bootstrap, Does.Contain("SendRoomChatFromInput"), "Existing websocket send path should remain the bootstrap truth.");
            Assert.That(bootstrap, Does.Contain("chatInputField.value"), "Existing input value handling should remain wired.");
            Assert.That(bootstrap, Does.Contain("wsController?.SendRoomChat(text)"), "Send should still route through the websocket room-chat controller.");
            Assert.That(controller, Does.Not.Contain("fake private message"));
            Assert.That(controller, Does.Not.Contain("city chat channel"));
            Assert.That(controller, Does.Not.Contain("guild relay"));
        }

        [Test]
        public void Bottom_chat_filter_button_width_cleanup_aligns_header_chips_without_changing_filter_truth()
        {
            var uxmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/AppShellController.cs");

            Assert.That(File.Exists(uxmlPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "AppShellController.cs should be available from the Unity project root.");

            var uxml = File.ReadAllText(uxmlPath);
            var uss = File.ReadAllText(ussPath);
            var controller = File.ReadAllText(controllerPath);

            Assert.That(uxml, Does.Contain("name=\"chat-filter-row\" class=\"button-row button-row--tight chat-chip-row chat-chip-row--aligned\""), "The bottom chat filter row should have a scoped alignment class instead of changing chat state.");
            Assert.That(uxml, Does.Contain("chat-chip--toggle chat-toggle-button"), "The Minimize/Expand control should keep its existing toggle identity while gaining a bounded width class.");
            Assert.That(uxml, Does.Contain("chat-chip--short action-button--primary"), "The All filter should stay primary while using the short chip width.");
            Assert.That(uxml, Does.Contain("chat-chip--wide"), "The wider Chat room filter should use a scoped width class.");
            Assert.That(uss, Does.Contain("Bottom chat filter button width cleanup v1"));
            Assert.That(uss, Does.Contain(".chat-chip-row--aligned"));
            Assert.That(uss, Does.Contain(".chat-chip--toggle,"));
            Assert.That(uss, Does.Contain("width: 92px"), "Minimize and Chat room chips should share a predictable wider width.");
            Assert.That(uss, Does.Contain(".chat-chip--short"));
            Assert.That(uss, Does.Contain("width: 76px"), "All and System chips should share a predictable short width.");
            Assert.That(uss, Does.Contain(".comms-panel--minimized .chat-chip-row--aligned .chat-toggle-button"), "Collapsed chat should still reveal only the Expand toggle instead of exposing hidden filters.");
            Assert.That(controller, Does.Contain("SetFilterActive(chatAllButton"), "All filter behavior should stay wired to existing live chat state.");
            Assert.That(controller, Does.Contain("SetFilterActive(chatRoomButton"), "Chat room filter behavior should stay wired to existing live chat state.");
            Assert.That(controller, Does.Contain("SetFilterActive(chatSystemButton"), "System filter behavior should stay wired to existing live chat state.");
            Assert.That(controller, Does.Not.Contain("fake private message"));
            Assert.That(controller, Does.Not.Contain("city chat channel"));
            Assert.That(controller, Does.Not.Contain("guild relay"));
        }


        [Test]
        public void Home_main_panel_padding_cleanup_tightens_home_spacing_without_changing_payload_or_actions()
        {
            var uxmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");

            Assert.That(File.Exists(uxmlPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(uxmlPath);
            var uss = File.ReadAllText(ussPath);

            Assert.That(uxml, Does.Contain("screen-scroll-content summary-screen-content home-main-panel"), "The Home summary content should have a scoped main-panel spacing class.");
            Assert.That(uxml, Does.Contain("name=\"resources-value\""), "Home resource payload truth should remain wired through the existing label.");
            Assert.That(uxml, Does.Contain("name=\"refresh-button\""), "Refresh summary action should remain wired through the existing button.");
            Assert.That(uxml, Does.Contain("name=\"home-development-button\""), "Home Development route action should remain wired through the existing button.");
            Assert.That(uxml, Does.Contain("name=\"post-founder-development-button\""), "Next-desk Development action should remain wired through the existing button.");
            Assert.That(uxml, Does.Contain("name=\"early-lane-posture-card\""), "Lane posture card should remain present and backend-driven.");
            Assert.That(uss, Does.Contain("Home main panel padding cleanup v1"));
            Assert.That(uss, Does.Contain(".home-main-panel"), "Home panel tightening should be scoped to the Home summary surface.");
            Assert.That(uss, Does.Contain("gap: 10px"), "Home panel vertical rhythm should be tighter without collapsing sections.");
            Assert.That(uss, Does.Contain("padding-bottom: 4px"), "Home panel bottom padding should be trimmed to show more useful content before scrolling.");
            Assert.That(uss, Does.Contain(".home-main-panel .summary-card--action"), "Action-card padding should be tightened only inside the Home panel.");
            Assert.That(uss, Does.Not.Contain("fake tutorial progress"));
            Assert.That(uss, Does.Not.Contain("fake resource grant"));
            Assert.That(uss, Does.Not.Contain("disable scrolling"));
        }


        [Test]
        public void Client_shell_style_contract_consolidates_recent_layout_surfaces_without_adding_fake_pending_actions()
        {
            var uxmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");

            Assert.That(File.Exists(uxmlPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var uxml = File.ReadAllText(uxmlPath);
            var uss = File.ReadAllText(ussPath);

            Assert.That(uxml, Does.Contain("shell-root client-shell-style-contract-v1"), "The shell root should carry a marker class for future layout work to preserve the current scoped contracts.");
            Assert.That(uss, Does.Contain("Client shell style contract consolidation v1"));
            Assert.That(uss, Does.Contain("navigation rail: rail-panel--compact, rail-scroll, chapter-row__badge"));
            Assert.That(uss, Does.Contain("Home workspace: home-main-panel, home-resource-strip, home-snapshot-grid, home-lane-posture-card"));
            Assert.That(uss, Does.Contain("chat tray: comms-panel--minimized, comms-status-stack, chat-chip-row--aligned, chat-compose-row--aligned"));
            Assert.That(uss, Does.Contain("independent scroll lanes: main-content-scroll, rail-scroll, chat-log-scroll"));
            Assert.That(uss, Does.Contain(".shell-pending-control"), "Future pending placeholders should have an explicit disabled/pending style hook instead of being active-looking fake actions.");
            Assert.That(uss, Does.Contain(".action-button--pending"), "Future pending buttons should use a muted style contract when a surface is intentionally unwired.");
            Assert.That(uss, Does.Contain("Pending future UI may use shell-pending-control or action-button--pending only when visibly disabled/action-free."));
            Assert.That(uss, Does.Contain("Do not add fake rewards, fake timers, fake private messages, fake room attachment, or fake pending gameplay actions here."));
            Assert.That(uxml, Does.Not.Contain("action-button--pending"), "This consolidation slice should not add visible pending gameplay buttons by itself.");
            Assert.That(uxml, Does.Not.Contain("shell-pending-control"), "This consolidation slice should add style hooks only, not placeholder UI inventory.");
        }



        [Test]
        public void Black_market_operations_desk_consumes_active_operation_surface_without_fake_execution()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/BlackMarket/BlackMarketScreenController.cs");
            Assert.That(File.Exists(controllerPath), Is.True, "BlackMarketScreenController.cs should be available from the Unity project root.");

            var controller = File.ReadAllText(controllerPath);
            Assert.That(controller, Does.Contain("summary.BlackMarketActiveOperation"), "Operations desk should consume /api/me.blackMarketActiveOperationSurface from mapped summary truth.");
            Assert.That(controller, Does.Contain("BuildBlackMarketActiveOperationCards"), "Operations desk should promote active-operation cards into the visible action board.");
            Assert.That(controller, Does.Contain("Action hook not live"), "Action-backed cards stay honest/read-only until a real execution handler is wired.");
            Assert.That(controller, Does.Contain("Select mission"), "Mission-backed operation cards may select the existing mission board instead of inventing a new action path.");
            Assert.That(controller, Does.Not.Contain("Execute shadow operation"), "This slice must not fake first-class operation execution.");
        }

        [Test]
        public void Black_market_operations_cards_show_receipt_detail_without_fake_execution()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/BlackMarket/BlackMarketScreenController.cs");
            var uxmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");

            Assert.That(File.Exists(controllerPath), Is.True, "BlackMarketScreenController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(uxmlPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var controller = File.ReadAllText(controllerPath);
            var uxml = File.ReadAllText(uxmlPath);
            var uss = File.ReadAllText(ussPath);

            Assert.That(controller, Does.Contain("BuildBlackMarketActiveOperationReceiptDetail"), "Active operation cards should expose receipt/proof/detail text from existing payload fields.");
            Assert.That(controller, Does.Contain("BuildBlackMarketActiveOperationReferenceSummary"), "Report detail should summarize source surface and server references instead of inventing execution." );
            Assert.That(controller, Does.Contain("operation signal verified"), "Player-facing operation proof should acknowledge source truth without exposing raw route strings or dev-only ref wording." );
            Assert.That(controller, Does.Contain("Report:"), "Visible operation cards should label the existing operator note/summary as report detail." );
            Assert.That(controller, Does.Contain("Proof:"), "Visible operation cards should label source/action/mission hooks as proof detail." );
            Assert.That(controller, Does.Not.Contain("source surface visible"), "Player-facing operation cards should not show dev/source-surface wording." );
            Assert.That(controller, Does.Not.Contain("world-action ref"), "Player-facing operation cards should not show backend-ref wording." );
            Assert.That(controller, Does.Not.Contain("mission ref"), "Player-facing operation cards should not show backend-ref wording." );
            Assert.That(controller, Does.Contain("Action hook not live"), "Action-backed cards remain honest/read-only until Unity has a real handler." );
            Assert.That(uxml, Does.Contain("operations-receipt-detail-surface-v1"), "The operations action board should carry a scoped receipt-detail styling hook." );
            Assert.That(uss, Does.Contain("Black Market operations receipt detail surface v1"));
            Assert.That(uss, Does.Contain(".operations-receipt-detail-surface-v1 .warfront-desk-card .metric-subvalue"));
            Assert.That(controller, Does.Not.Contain("Execute shadow operation"), "This slice must not fake shadow-operation execution." );
            Assert.That(controller, Does.Not.Contain("Grant shadow reward"), "This slice must not fake rewards for operation cards." );
        }


        [Test]
        public void Black_market_operations_cards_select_detail_panel_without_fake_execution()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/BlackMarket/BlackMarketScreenController.cs");
            var uxmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");

            Assert.That(File.Exists(controllerPath), Is.True, "BlackMarketScreenController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(uxmlPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var controller = File.ReadAllText(controllerPath);
            var uxml = File.ReadAllText(uxmlPath);
            var uss = File.ReadAllText(ussPath);

            Assert.That(controller, Does.Contain("selectedBlackMarketOperationCardId"), "Operation cards should maintain local selected-card state for the focused detail panel.");
            Assert.That(controller, Does.Contain("RenderBlackMarketOperationDetail"), "The Operations desk should render a focused selected-operation detail panel from existing payload truth.");
            Assert.That(controller, Does.Contain("SelectBlackMarketOperationCard"), "Selecting a visible operation card should update detail focus without inventing execution.");
            Assert.That(controller, Does.Contain("BuildBlackMarketOperationDetailBlockers"), "Focused details should call out blockers/read-only execution gaps from current truth.");
            Assert.That(controller, Does.Contain("covert action hook visible, execution pending"), "Action hooks must stay honest/read-only until a real handler exists.");
            Assert.That(controller, Does.Not.Contain("source {card.SourceSurface}"), "Focused detail refs should not expose raw source-surface strings to players.");
            Assert.That(controller, Does.Not.Contain("Truncate(string.Join(\", \", card.ActionIds)"), "Focused detail refs should show counts instead of raw action ids.");
            Assert.That(controller, Does.Not.Contain("Truncate(string.Join(\", \", card.MissionOfferIds)"), "Focused detail refs should show counts instead of raw mission ids.");
            Assert.That(controller, Does.Not.Contain("source surface visible"), "Focused detail refs should not show dev/source-surface wording to players.");
            Assert.That(controller, Does.Not.Contain("world-action ref"), "Focused detail refs should not show backend-ref wording to players.");
            Assert.That(controller, Does.Not.Contain("mission ref"), "Focused detail refs should not show backend-ref wording to players.");
            Assert.That(controller, Does.Contain("covert action hook"), "Focused detail refs should use player-facing covert action hook wording.");
            Assert.That(controller, Does.Contain("mission lead"), "Focused detail refs should use player-facing mission lead wording.");
            Assert.That(controller, Does.Contain("Signals:"), "Focused detail refs should label player-facing operation signals instead of raw refs.");
            Assert.That(uxml, Does.Contain("warfront-operation-detail-root"), "Operations should include a selected-operation detail panel in the existing action board.");
            Assert.That(uxml, Does.Contain("warfront-operation-detail-blockers-value"), "Focused detail should have a blockers line, even when it only says no blocker field was supplied.");
            Assert.That(uxml, Does.Contain("operations-operation-detail-selection-v1"), "The detail panel should use a scoped styling hook.");
            Assert.That(uss, Does.Contain("Black Market operation detail selection v1"));
            Assert.That(uss, Does.Contain(".warfront-desk-card--selected"), "Selected operation cards should get a visual focus state.");
            Assert.That(controller, Does.Not.Contain("Execute shadow operation"), "This slice must not fake shadow-operation execution." );
            Assert.That(controller, Does.Not.Contain("Grant shadow reward"), "This slice must not fake rewards for operation cards." );
        }


        [Test]
        public void Black_market_operation_cards_expand_readability_without_fake_execution()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/BlackMarket/BlackMarketScreenController.cs");
            var uxmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");

            Assert.That(File.Exists(controllerPath), Is.True, "BlackMarketScreenController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(uxmlPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var controller = File.ReadAllText(controllerPath);
            var uxml = File.ReadAllText(uxmlPath);
            var uss = File.ReadAllText(ussPath);

            Assert.That(controller, Does.Contain("), 232)"), "Operation card lore should allow more player-facing context before truncating.");
            Assert.That(controller, Does.Contain("), 320)"), "Selected operation receipt detail should allow more existing payload truth before truncating.");
            Assert.That(controller, Does.Contain("Truncate(operatorNote, 112)"), "Card receipt notes should be less aggressively shortened while staying bounded.");
            Assert.That(uxml, Does.Contain("operations-operation-card-expansion-v1"), "Operations action board should carry a scoped card-expansion readability hook.");
            Assert.That(uxml, Does.Contain("operations-operation-detail--expanded"), "Selected operation detail should carry a scoped expanded readability hook.");
            Assert.That(uss, Does.Contain("Black Market operation card expansion cleanup v1"));
            Assert.That(uss, Does.Contain(".operations-operation-card-expansion-v1 .warfront-desk-card"), "Operation card expansion should be scoped to the Black Market operations board.");
            Assert.That(uss, Does.Contain("max-height: 92px"), "Focused operation detail lines should allow more receipt/proof/blocker copy before clipping.");
            Assert.That(controller, Does.Not.Contain("Execute shadow operation"), "This slice must not fake shadow-operation execution.");
            Assert.That(controller, Does.Not.Contain("Grant shadow reward"), "This slice must not fake rewards for operation cards.");
            Assert.That(controller, Does.Not.Contain("/api/me.worldConsequenceActions.playerActions"), "Player-facing card expansion must not reintroduce raw API route strings.");
        }




        [Test]
        public void Black_market_operation_mission_leads_route_to_existing_mission_board_without_fake_creation()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/BlackMarket/BlackMarketScreenController.cs");

            Assert.That(File.Exists(controllerPath), Is.True, "BlackMarketScreenController.cs should be available from the Unity project root.");

            var controller = File.ReadAllText(controllerPath);

            Assert.That(controller, Does.Contain("Black Market operation mission lead selection cleanup v1"), "Mission-lead selection should be explicitly protected as a client-only clarity slice.");
            Assert.That(controller, Does.Contain("BuildBlackMarketOperationMissionLeadGuidance"), "Mission-backed operation cards should explain where the Select mission lead button goes.");
            Assert.That(controller, Does.Contain("button opens the existing Mission Board offer; it does not create a new mission."), "Selectable mission leads should clearly route to the existing Mission Board instead of implying fake creation.");
            Assert.That(controller, Does.Contain("Next: select this mission lead to review the existing Mission Board offer."), "Focused operation detail should give the same mission-board path when a lead is available.");
            Assert.That(controller, Does.Contain("finish the active mission before reviewing this lead."), "Blocked mission leads should explain the active-mission gate without inventing bypasses.");
            Assert.That(controller, Does.Not.Contain("Create mission lead"), "This slice must not invent mission creation.");
            Assert.That(controller, Does.Not.Contain("Generate mission reward"), "This slice must not invent mission rewards.");
            Assert.That(controller, Does.Not.Contain("Execute shadow operation"), "This slice must not fake shadow-operation execution.");
        }

        [Test]
        public void Black_market_operation_cards_show_clear_readiness_labels_without_fake_execution()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/BlackMarket/BlackMarketScreenController.cs");

            Assert.That(File.Exists(controllerPath), Is.True, "BlackMarketScreenController.cs should be available from the Unity project root.");

            var controller = File.ReadAllText(controllerPath);

            Assert.That(controller, Does.Contain("BuildBlackMarketOperationActionLabel"), "Operation card buttons should derive player-facing readiness labels from existing payload truth.");
            Assert.That(controller, Does.Contain("BuildBlackMarketOperationReadinessLabel"), "Operation cards should expose a readable readiness line instead of vague dev-state labels.");
            Assert.That(controller, Does.Contain("Select mission lead"), "Mission-backed cards should clearly tell the player that the visible action selects an existing mission lead.");
            Assert.That(controller, Does.Contain("Mission lead blocked"), "Mission-backed cards should stay honest when a current mission blocks selection.");
            Assert.That(controller, Does.Contain("Action hook not live"), "Action-backed cards should avoid looking executable until a real Unity handler exists.");
            Assert.That(controller, Does.Contain("Read-only signal"), "Cards without selectable mission/action hooks should describe themselves as read-only signals.");
            Assert.That(controller, Does.Contain("Readiness:"), "Visible card receipt detail should include a readiness line players can understand.");
            Assert.That(controller, Does.Contain("covert action hook visible, execution pending"), "Covert action hooks should be visible but explicitly unwired instead of pretending execution exists.");
            Assert.That(controller, Does.Not.Contain("Execute shadow operation"), "This slice must not fake shadow-operation execution.");
            Assert.That(controller, Does.Not.Contain("Grant shadow reward"), "This slice must not fake rewards for operation cards.");
            Assert.That(controller, Does.Not.Contain("World action visible"), "Player-facing operation card buttons should not use backend-ish world-action wording.");
        }


        [Test]
        public void Black_market_operation_cards_show_potential_impact_without_promising_rewards()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/BlackMarket/BlackMarketScreenController.cs");
            var uxmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");

            Assert.That(File.Exists(controllerPath), Is.True, "BlackMarketScreenController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(uxmlPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var controller = File.ReadAllText(controllerPath);
            var uxml = File.ReadAllText(uxmlPath);
            var uss = File.ReadAllText(ussPath);

            Assert.That(controller, Does.Contain("Black Market operation impact preview v1"), "Impact preview should be an explicit player-facing clarity slice, not an execution slice.");
            Assert.That(controller, Does.Contain("BuildBlackMarketOperationImpactPreview"), "Operation cards and selected detail should derive potential impact from existing operation truth.");
            Assert.That(controller, Does.Contain("Potential impact:"), "Operation cards should show a player-facing payoff/impact hint.");
            Assert.That(controller, Does.Contain("May soften patrol or guard friction"), "Bribery/patrol operations should explain how they may help later mission/action choices without promising success.");
            Assert.That(controller, Does.Contain("covert mission leads or action hooks"), "Impact copy should help players decide what supports their next covert move.");
            Assert.That(controller, Does.Contain("payoff only appears after a supported action path is wired or completed"), "Action-backed impact copy must stay honest while Unity execution is not live.");
            Assert.That(uxml, Does.Contain("warfront-operation-detail-impact-value"), "Selected operation detail should include a dedicated impact preview line.");
            Assert.That(uxml, Does.Contain("operations-operation-detail__impact"), "Impact preview should have a scoped styling hook.");
            Assert.That(uss, Does.Contain("Black Market operation impact preview v1"));
            Assert.That(uss, Does.Contain(".operations-operation-detail__impact"));
            Assert.That(controller, Does.Not.Contain("guaranteed reward"), "Impact preview must not promise rewards.");
            Assert.That(controller, Does.Not.Contain("guaranteed success"), "Impact preview must not promise mission success.");
            Assert.That(controller, Does.Not.Contain("Execute shadow operation"), "Impact preview must not fake shadow-operation execution.");
            Assert.That(controller, Does.Not.Contain("Grant shadow reward"), "Impact preview must not fake rewards.");
        }



        [Test]
        public void Black_market_operation_type_focus_filters_candidates_without_fake_variants()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/BlackMarket/BlackMarketScreenController.cs");
            var uxmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");

            Assert.That(File.Exists(controllerPath), Is.True, "BlackMarketScreenController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(uxmlPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var controller = File.ReadAllText(controllerPath);
            var uxml = File.ReadAllText(uxmlPath);
            var uss = File.ReadAllText(ussPath);

            Assert.That(controller, Does.Contain("selectedBlackMarketOperationTypeKey"), "The Operations desk should remember the selected operation type locally.");
            Assert.That(controller, Does.Contain("RenderBlackMarketOperationTypeFocus"), "The Operations desk should render a type-focus picker before showing candidate cards.");
            Assert.That(controller, Does.Contain("GetFocusedBlackMarketOperationCards"), "Operation cards should be filtered by the selected payload kind/type instead of dumping every card at once.");
            Assert.That(controller, Does.Contain("Type focus:"), "The focused picker should explain how many candidate operations and risks are visible for the selected type.");
            Assert.That(controller, Does.Contain("Focused candidates"), "The type selector should frame visible cards as focused candidates, not fake new operation variants.");
            Assert.That(uxml, Does.Contain("operations-operation-type-focus-v1"), "Operations action board should carry a scoped type-focus hook.");
            Assert.That(uxml, Does.Contain("warfront-operation-type-picker"), "Operations action board should include a local type picker.");
            Assert.That(uxml, Does.Contain("warfront-operation-type-summary-value"), "Operations action board should include a focused-type summary line.");
            Assert.That(uss, Does.Contain("Black Market operation type focus v1"));
            Assert.That(uss, Does.Contain(".operations-choice-list--operation-types"));
            Assert.That(controller, Does.Not.Contain("Generate risk variant"), "This slice must not invent low/medium/high operation variants client-side.");
            Assert.That(controller, Does.Not.Contain("Create shadow operation"), "This slice must not invent operation creation.");
            Assert.That(controller, Does.Not.Contain("Execute shadow operation"), "This slice must not fake shadow-operation execution.");
            Assert.That(controller, Does.Not.Contain("Grant shadow reward"), "This slice must not fake rewards for operation cards.");
        }


        [Test]
        public void Unity_operations_pressure_leads_highlight_existing_mission_board_truth_without_fake_execution()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/BlackMarket/BlackMarketScreenController.cs");
            var ussPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/USS/AppShell.uss");

            Assert.That(File.Exists(controllerPath), Is.True, "BlackMarketScreenController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(ussPath), Is.True, "AppShell.uss should be available from the Unity project root.");

            var controller = File.ReadAllText(controllerPath);
            var uss = File.ReadAllText(ussPath);

            Assert.That(controller, Does.Contain("Unity Operations Pressure Lead Highlight v1"), "Pressure lead highlight should be an explicit Unity client clarity slice.");
            Assert.That(controller, Does.Contain("ResolveClientPressureMissionLeadId"), "Operations should resolve pressure mission leads from the client pressure surface without new backend calls.");
            Assert.That(controller, Does.Contain("ApplyClientPressureMissionLeadSelection"), "Operations should focus a matching active-operation card when one points at the pressure lead.");
            Assert.That(controller, Does.Contain("BuildPressureMissionLeadBoardCopy"), "Mission Board copy should explain whether the pressure lead is available, hidden, or missing.");
            Assert.That(controller, Does.Contain("Pressure lead matched this Mission Board offer"), "Available pressure leads should select existing Mission Board offers.");
            Assert.That(controller, Does.Contain("Pressure lead is hidden by current board state"), "Hidden leads should be distinguished from missing leads when active missions or reports block selection.");
            Assert.That(controller, Does.Contain("Pressure lead is missing from the current Mission Board payload"), "Missing leads should be honest instead of inventing client-side offers.");
            Assert.That(controller, Does.Contain("Select pressure lead"), "Pressure-backed operation cards should use a player-facing select label, not backend IDs.");
            Assert.That(controller, Does.Contain("operations-choice--pressure-lead"), "Mission Board picker should expose a scoped visual highlight for matched pressure leads.");
            Assert.That(controller, Does.Contain("warfront-desk-card--pressure-lead"), "Operation cards should expose a scoped visual highlight for matched pressure leads.");
            Assert.That(uss, Does.Contain("Unity Operations Pressure Lead Highlight v1"));
            Assert.That(uss, Does.Contain(".operations-choice--pressure-lead"));
            Assert.That(uss, Does.Contain(".warfront-desk-card--pressure-lead"));
            Assert.That(controller, Does.Not.Contain("Create pressure mission"), "This slice must not invent mission creation.");
            Assert.That(controller, Does.Not.Contain("Execute pressure mission"), "This slice must not execute missions from the pressure card.");
            Assert.That(controller, Does.Not.Contain("Generate pressure reward"), "This slice must not fake pressure rewards.");
            Assert.That(controller, Does.Not.Contain("/api/me clientPressureSurface"), "Player-facing pressure lead highlight must not expose raw API contract text.");
        }


        [Test]
        public void Unity_pressure_receipt_outcome_copy_stays_player_facing_without_raw_runtime_ids()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/Summary/SummaryScreenController.cs");
            var formatterPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Core/Presentation/ContractTruthText.cs");

            Assert.That(File.Exists(controllerPath), Is.True, "SummaryScreenController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(formatterPath), Is.True, "ContractTruthText.cs should be available from the Unity project root.");

            var controller = File.ReadAllText(controllerPath);
            var formatter = File.ReadAllText(formatterPath);

            Assert.That(controller, Does.Contain("Unity Player-Facing Copy Sanitization v1"), "Player-facing copy cleanup should be an explicit streamer-safety slice.");
            Assert.That(controller, Does.Contain("live posture"), "Lane posture badges should use player-facing live posture wording.");
            Assert.That(controller, Does.Contain("CleanPlayerFacingText"), "Summary receipt copy should pass raw-ish labels through a small player-facing sanitizer.");
            Assert.That(controller, Does.Contain("FormatRegionList"), "Region lists should show human names instead of raw runtime ids.");
            Assert.That(controller, Does.Contain("Select(CleanPlayerFacingText)"), "Follow-through lines should stay readable and pass through player-facing cleanup.");
            Assert.That(controller, Does.Contain("Available on: {FormatClientTargets(contract.ClientTargets)}."), "Client targets should be translated into player-facing surfaces.");
            Assert.That(controller, Does.Not.Contain("region {receipt.RegionId}"), "Receipt signals must not show raw region-id interpolation.");
            Assert.That(controller, Does.Not.Contain("action {receipt.RuntimeActionId}"), "Receipt signals must not show runtime action ids.");
            Assert.That(controller, Does.Not.Contain("Runtime action:"), "Mother Brain report copy should not print raw runtime action ids.");
            Assert.That(controller, Does.Not.Contain("Execution disabled: this Unity card is inspect-only."), "Pressure status copy should avoid debug-ish execution labels.");
            Assert.That(controller, Does.Not.Contain("No client mutation required."), "Pressure status copy should avoid debug-ish mutation labels.");
            Assert.That(controller, Does.Not.Contain("Latest runtime response:"), "Bridge copy should say server response instead of runtime response.");
            Assert.That(controller, Does.Not.Contain("backend posture"), "Lane posture badges should not expose backend wording.");
            Assert.That(controller, Does.Not.Contain("Backend rewards:"), "Reward previews should not use backend-ish labels in player-facing copy.");
            Assert.That(controller, Does.Not.Contain("/api/me exposes the bridge"), "Fallback copy should not show raw API routes.");
            Assert.That(formatter, Does.Contain("Next report:"), "Shared formatter should use report language for recovery follow-through.");
        }

        [Test]
        public void Home_pressure_detail_copy_hides_raw_report_ids_and_off_scale_debug_numbers()
        {
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/Summary/SummaryScreenController.cs");
            Assert.That(File.Exists(controllerPath), Is.True, "SummaryScreenController.cs should be available from the Unity project root.");

            var controller = File.ReadAllText(controllerPath);

            Assert.That(controller, Does.Contain("Unity Pressure Detail Readability v1"), "Deep Home pressure details should have an explicit readability guard.");
            Assert.That(controller, Does.Contain("FormatLoggedReportLine"), "Logged reports should summarize server truth without printing raw report ids.");
            Assert.That(controller, Does.Contain("NormalizeScientificNotationForPlayerFacingCopy"), "Off-scale scientific notation should be converted before player-facing display.");
            Assert.That(controller, Does.Contain("Regex.Replace(value"), "Scientific notation cleanup should be centralized instead of hand-patched per card.");
            Assert.That(controller, Does.Contain("lines.Add(CleanPlayerFacingText(followThrough.Title));"));
            Assert.That(controller, Does.Contain("lines.Add(CleanPlayerFacingText(followThrough.Summary));"));
            Assert.That(controller, Does.Not.Contain("Latest report: {FirstNonBlank(followThrough.LatestReceiptId"), "Home public-service detail copy must not print raw report ids.");
            Assert.That(controller, Does.Not.Contain("lines.Add(followThrough.Title);"), "Follow-through titles should pass through player-facing cleanup.");
            Assert.That(controller, Does.Not.Contain("lines.Add(followThrough.Summary);"), "Follow-through summaries should pass through player-facing cleanup.");
        }

        [Test]
        public void Home_pressure_headers_translate_backend_system_names_to_player_facing_labels()
        {
            var appShellPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/UI/UXML/AppShell.uxml");
            var controllerPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Runtime/Shell/Screens/Summary/SummaryScreenController.cs");
            var guidePath = Path.Combine(Directory.GetCurrentDirectory(), "Assets/PlanarWar/Docs/PLAYER_TESTER_GUIDE_V1.md");
            Assert.That(File.Exists(appShellPath), Is.True, "AppShell.uxml should be available from the Unity project root.");
            Assert.That(File.Exists(controllerPath), Is.True, "SummaryScreenController.cs should be available from the Unity project root.");
            Assert.That(File.Exists(guidePath), Is.True, "PLAYER_TESTER_GUIDE_V1.md should be available from the Unity project root.");

            var uxml = File.ReadAllText(appShellPath);
            var controller = File.ReadAllText(controllerPath);
            var guide = File.ReadAllText(guidePath);

            Assert.That(controller, Does.Contain("Unity Home Pressure Header Translation v1"), "The Home pressure label pass should be explicitly guarded.");
            Assert.That(controller, Does.Contain("TranslateHomePressureTitle"), "Dynamic server titles should route through player-facing pressure label translation.");
            Assert.That(uxml, Does.Contain("text=\"Urgent pressure\""));
            Assert.That(uxml, Does.Contain("text=\"Public services\""));
            Assert.That(uxml, Does.Contain("text=\"Regional support\""));
            Assert.That(uxml, Does.Contain("text=\"Recovery opportunities\""));
            Assert.That(guide, Does.Contain("Urgent pressure"));
            Assert.That(guide, Does.Contain("Public services"));
            Assert.That(guide, Does.Contain("Regional support"));
            Assert.That(guide, Does.Contain("Recovery opportunities"));
            Assert.That(uxml, Does.Not.Contain("text=\"Mother Brain pressure path\""));
            Assert.That(uxml, Does.Not.Contain("text=\"Public infrastructure economy spine\""));
            Assert.That(uxml, Does.Not.Contain("text=\"City ↔ MUD world-consequence bridge\""));
            Assert.That(uxml, Does.Not.Contain("text=\"Regional recovery board\""));
            Assert.That(guide, Does.Not.Contain("### Mother Brain pressure path"));
            Assert.That(guide, Does.Not.Contain("### Public infrastructure economy spine"));
            Assert.That(guide, Does.Not.Contain("### City ↔ MUD world-consequence bridge"));
            Assert.That(guide, Does.Not.Contain("### Regional recovery board"));
        }


    }
}
