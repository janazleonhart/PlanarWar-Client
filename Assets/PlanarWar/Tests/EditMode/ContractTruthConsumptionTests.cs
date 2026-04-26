using NUnit.Framework;
using PlanarWar.Client.Core;
using PlanarWar.Client.Core.Contracts;
using PlanarWar.Client.Core.Mapping;
using PlanarWar.Client.Core.Presentation;
using PlanarWar.Client.UI.Screens.City;
using PlanarWar.Client.UI.Screens.Summary;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace PlanarWar.Client.Tests.EditMode
{
    public class ContractTruthConsumptionTests
    {
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

            var buildings = new List<BuildingSnapshot>
            {
                new BuildingSnapshot { Id = "low_quarter", Type = "housing", Name = "Low Quarter", Status = "active" }
            };
            var timers = new List<CityTimerEntrySnapshot>();

            var note = (string)builder.Invoke(null, new object[] { buildings, timers, false, DateTime.UtcNow });

            Assert.That(note, Does.Contain("only unlocked, affordable"));
            Assert.That(note, Does.Contain("Destroy/remodel requires a backend endpoint"));
            Assert.That(note, Does.Not.Contain("Route:"));
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

    }
}
