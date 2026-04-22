using NUnit.Framework;
using System.Collections.Generic;
using PlanarWar.Client.Core.Contracts;
using PlanarWar.Client.Core.Presentation;

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
        public void Shadow_lane_formatter_keeps_black_market_development_wording_specific()
        {
            var tech = new TechOptionSnapshot
            {
                Name = "Charter Registry",
                Description = "Turn early expansion into a visible charter machine instead of loose growth.",
                Cost = 100
            };

            var summary = new ShellSummarySnapshot
            {
                OpeningOperations = new List<OperationSnapshot>
                {
                    new OperationSnapshot
                    {
                        Title = "Trace Counterfeit Routes"
                    }
                }
            };

            Assert.That(ShadowLaneText.BuildTechFamily(tech), Is.EqualTo("Paper front"));
            Assert.That(ShadowLaneText.BuildResearchLaneTitle(), Is.EqualTo("Shadow books"));
            Assert.That(ShadowLaneText.DescribeWorkshopLane(2, 1, 0, 0), Is.EqualTo("2 live front(s) • 1 ready drop(s)"));
            Assert.That(ShadowLaneText.BuildProductionTitle(), Is.EqualTo("Per-tick throughput"));
            Assert.That(ShadowLaneText.BuildSupportCardNote(summary), Does.Contain("Trace Counterfeit Routes"));
        }
    }
}