using NUnit.Framework;
using PlanarWar.Client.Core;
using PlanarWar.Client.Core.Contracts;
using PlanarWar.Client.Core.Mapping;
using PlanarWar.Client.Core.Presentation;
using PlanarWar.Client.UI.Screens.City;
using System;
using System.Collections.Generic;
using System.Reflection;

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

    }
}
