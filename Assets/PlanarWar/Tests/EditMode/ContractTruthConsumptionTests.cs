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
                Is.EqualTo("Cooling • World-response receipts"));

            Assert.That(
                ContractTruthText.BuildCivicEffectsValue(civicEffects, "fallback"),
                Is.EqualTo("Queue cooling • Trust steadying • Services restoring"));

            Assert.That(
                ContractTruthText.BuildShadowEffectsValue(shadowEffects, "fallback"),
                Is.EqualTo("Receipt chain linked • Covert carry carried"));
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

            Assert.That(note, Does.Contain("only unlocked, affordable"));
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
                        ""summary"": ""Follow the counterfeit receipt chain before it cools.""
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

            Assert.That(receipt, Does.Contain("Outcome: Success"));
            Assert.That(receipt, Does.Contain("Rewards: wealth +12, materials +8"));
            Assert.That(receipt, Does.Contain("Summary: Frontline assault pushed hostile pressure back."));
            Assert.That(receipt, Does.Not.Contain("created at"));
            Assert.That(receipt, Does.Not.Contain("mission title"));
            Assert.That(title, Is.EqualTo("Frontline Assault: Heartland Basin"));
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
            Assert.That(receipt, Does.Contain("Outcome: released"));
            Assert.That(receipt, Does.Contain("Hero: Ser Kael the Stormguard"));
            Assert.That(receipt, Does.Contain("Effects:"));
            Assert.That(receipt, Does.Contain("item id workshop_arcane_focus_1"));
            Assert.That(receipt, Does.Contain("qty +1"));
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




    }
}
