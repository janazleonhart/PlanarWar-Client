using NUnit.Framework;
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
        public void Shadow_lane_formatter_hides_routine_lane_tags_and_internal_copy()
        {
            var tech = new TechOptionSnapshot
            {
                Name = "Front Ledger",
                Cost = 100,
                LaneIdentity = "black_market",
                OperatorNote = "Best opened when you want permit memory online.",
                UnlockPreview = new System.Collections.Generic.List<string> { "Quiet ledger office" }
            };

            var summary = new ShellSummarySnapshot
            {
                AvailableTechs = new System.Collections.Generic.List<TechOptionSnapshot>
                {
                    new TechOptionSnapshot { Name = "Front Ledger" },
                    new TechOptionSnapshot { Name = "Quiet Routes I" },
                    new TechOptionSnapshot { Name = "Quiet Stockpiles" }
                }
            };

            var note = ShadowLaneText.BuildTechNote(tech);
            var copy = ShadowLaneText.DescribeResearchCardsCopy(summary);

            Assert.That(note, Does.Not.Contain("Lane Black market"));
            Assert.That(note, Does.Not.Contain("Lane City"));
            Assert.That(copy, Does.Not.Contain("/api/me"));
            Assert.That(copy, Does.Contain("Front Ledger"));
        }

        [Test]
        public void Shadow_lane_formatter_surfaces_operator_front_visibility_without_city_leaks()
        {
            var buildings = new System.Collections.Generic.List<BuildingSnapshot>
            {
                new BuildingSnapshot { Name = "Front House", Kind = "front_house", Level = 1 },
                new BuildingSnapshot { Name = "Debt House", Kind = "debt_house", Level = 1 },
                new BuildingSnapshot { Name = "Cutout Bureau", Kind = "cutout_bureau", Level = 1 }
            };

            var growth = ShadowLaneText.DescribeGrowth(new TimerSnapshot(), new System.Collections.Generic.List<CityTimerEntrySnapshot>(), buildings);
            var copy = ShadowLaneText.DescribeGrowthCardsCopy(
                null,
                null,
                null,
                new System.Collections.Generic.List<CityTimerEntrySnapshot>(),
                buildings);
            var note = ShadowLaneText.BuildDeskNote(
                new ShellSummarySnapshot
                {
                    City = new CitySummarySnapshot { Buildings = buildings },
                    AvailableTechs = new System.Collections.Generic.List<TechOptionSnapshot> { new TechOptionSnapshot { Name = "Front Ledger" } }
                },
                string.Empty);

            Assert.That(growth, Does.Contain("live front"));
            Assert.That(copy, Does.Contain("operator front"));
            Assert.That(note, Does.Contain("operator fronts"));
            Assert.That(note, Does.Contain("Front House"));
            Assert.That(note, Does.Not.Contain("Lane City"));
        }
    }
}