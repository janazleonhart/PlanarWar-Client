using NUnit.Framework;
using PlanarWar.Client.Core.Contracts;
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

    }
}
