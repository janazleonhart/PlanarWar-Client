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
            Assert.That(controller, Does.Contain("SetFilterActive(chatRoomButton"), "Room filter should still be wired through SetFilterActive.");
            Assert.That(controller, Does.Contain("SetFilterActive(chatSystemButton"), "System filter should still be wired through SetFilterActive.");
            Assert.That(controller, Does.Contain("ActiveChatChannel, \"all\""), "All filter should still be driven by live SessionState channel truth.");
            Assert.That(controller, Does.Contain("ActiveChatChannel, \"room\""), "Room filter should still be driven by live SessionState channel truth.");
            Assert.That(controller, Does.Contain("ActiveChatChannel, \"system\""), "System filter should still be driven by live SessionState channel truth.");
            Assert.That(controller, Does.Contain("sessionState.GetVisibleChatLines()"), "Comms board and bottom log should keep consuming filtered live chat lines instead of fake channel rows.");
            Assert.That(controller, Does.Contain("No chat lines visible for this filter yet."), "Room/System empty states should stay readable when a filter has no visible lines.");
            Assert.That(controller, Does.Contain("Room comms are live"), "Outbound room chat hint should stay tied to real room attachment state.");
            Assert.That(controller, Does.Contain("friend roster, DMs, and moderation surfaces remain deferred"), "Social closeout should keep deferred social-system scope explicit.");
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
