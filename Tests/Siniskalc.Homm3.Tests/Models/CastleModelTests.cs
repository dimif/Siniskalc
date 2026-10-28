namespace Siniskalc.Homm3.Tests.Models
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Tests what a castle of HoMM3 tells through the interface every game's castle has, and its weekly growth, on a town made by hand.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class CastleModelTests
    {
        [Test]
        public void ACastleTellsWhatEveryGameCastleHasThroughTheSharedInterface()
        {
            var model = CastleModels.With(CastleModels.Village(Faction.Rampart), Building.RampartCentaurStables);

            ICastleModel<PlayerColor, Creature, Faction> castle = model;

            Assert.That((castle.Name, castle.Faction?.Name), Is.EqualTo(("Test Town", "Rampart")));
            Assert.That(castle.Faction, Is.SameAs(model.Faction));
            Assert.That((castle.CreaturesAvailable.Count, castle.Garrison.Slots.Count), Is.EqualTo((7, 7)));
            Assert.That(castle.Garrison, Is.SameAs(model.Garrison));
            Assert.That(castle.IsUsed, Is.True);
            Assert.That(castle.DwellingCount, Is.EqualTo(1));
            Assert.That(castle.GarrisonedHeroId, Is.EqualTo(model.GarrisonedHeroId));
        }

        [Test]
        public void TheBaseWeeklyGrowthIsTheGrowthOfTheCreatureOnceItsDwellingIsBuilt()
        {
            var still = CastleModels.Village(Faction.Rampart);

            Assert.That(still.BaseWeeklyGrowth(0), Is.EqualTo(0), "the town has no Centaur Stables yet");

            var built = CastleBuilder.Build(still, Building.RampartCentaurStables);
            Assert.That(built.BaseWeeklyGrowth(0), Is.EqualTo(14));

            var upgraded = CastleBuilder.Build(built, Building.RampartUpgCentaurStables);
            Assert.That(upgraded.BaseWeeklyGrowth(0), Is.EqualTo(14), "an upgraded dwelling has the growth of the creature it upgrades");
        }
    }
}
