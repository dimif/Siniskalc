namespace Siniskalc.Homm2.Tests.Entities
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Tests what the second well and the special building of a castle are called in each town of HoMM2.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class TownBuildingNamesTests
    {
        [TestCase("Knight", "Farm", "Fortifications")]
        [TestCase("Barbarian", "Garbage Heap", "Coliseum")]
        [TestCase("Sorceress", "Crystal Garden", "Rainbow")]
        [TestCase("Warlock", "Waterfall", "Dungeon")]
        [TestCase("Wizard", "Orchard", "Library")]
        [TestCase("Necromancer", "Skull Pile", "Storm")]
        public void EachTownCallsItsSecondWellAndItsSpecialBuildingByItsOwnName(string faction, string secondWell, string special)
        {
            var town = Faction.All.Single(f => f.Name == faction);

            Assert.That(TownBuildingNames.SecondWellOf(town), Is.EqualTo(secondWell));
            Assert.That(TownBuildingNames.SpecialOf(town), Is.EqualTo(special));
        }

        [Test]
        public void ACastleOfNoTownHasGenericNames()
        {
            Assert.That(TownBuildingNames.SecondWellOf(Faction.Neutral), Is.EqualTo("Second well"));
            Assert.That(TownBuildingNames.SpecialOf(Faction.Neutral), Is.EqualTo("Special building"));
        }
    }
}
