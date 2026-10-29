namespace Siniskalc.Homm3.Tests.Models
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Tests.Data;

    /// <summary>
    /// Tests what a castle of HoMM3 tells through the interface every game's castle has.
    /// </summary>
    [TestFixture]
    internal sealed class CastleModelTests
    {
        [Test]
        public void ACastleTellsWhatEveryGameCastleHasThroughTheSharedInterface()
        {
            var castles = CastleSamples.Load(CastleSamples.Folder("Bulwark").First()).History.Current.Castles();
            var model = TestServices.CastleDeserializer.Deserialize(castles.First(c => c.Name == "Still Water"));

            ICastleModel<PlayerColor, Creature, Faction> castle = model;

            Assert.That((castle.Name, castle.Faction?.Name), Is.EqualTo(("Still Water", model.Faction!.Name)));
            Assert.That(castle.Faction, Is.SameAs(model.Faction));
            Assert.That((castle.CreaturesAvailable.Count, castle.Garrison.Slots.Count), Is.EqualTo((7, 7)));
            Assert.That(castle.Garrison, Is.SameAs(model.Garrison));
            Assert.That(castle.IsUsed, Is.True);
            Assert.That(castle.DwellingCount, Is.EqualTo(model.Buildings.Count(b => b.IsDwelling)));
            Assert.That(castle.GarrisonedHeroId, Is.EqualTo(model.GarrisonedHeroId));
        }

        [Test]
        public void TheBaseWeeklyGrowthIsTheGrowthOfTheCreatureOnceItsDwellingIsBuilt()
        {
            var castles = CastleSamples.Load(CastleSamples.Folder("Bulwark").Single(p => p.EndsWith("bulwark_13_mage_guild_4.GM1"))).History.Current.Castles();
            var still = TestServices.CastleDeserializer.Deserialize(castles.First(c => c.Name == "Still Water"));

            Assert.That(still.BaseWeeklyGrowth(0), Is.EqualTo(0), "Still Water has no Centaur Stables in this save");

            var built = Siniskalc.Homm3.Models.CastleBuilder.Build(still, Building.RampartCentaurStables);
            Assert.That(built.BaseWeeklyGrowth(0), Is.EqualTo(14));

            var upgraded = Siniskalc.Homm3.Models.CastleBuilder.Build(built, Building.RampartUpgCentaurStables);
            Assert.That(upgraded.BaseWeeklyGrowth(0), Is.EqualTo(14), "an upgraded dwelling has the growth of the creature it upgrades");
        }
    }
}
