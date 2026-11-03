namespace Siniskalc.Homm2.Tests.Integration.Models
{
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// What a castle model holds when it is read from a savegame.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class CastleModelSavegameTests
    {
        private static CastleModel Xabran() => TestServices.LoadBrokenAlliance(5).History.Current.Castles().GetModel(3);

        [Test]
        public void ADwellingThatIsUpgradedInASaveHasItsOwnBitClearedAndTheBitOfTheUpgradeSet()
        {
            var warlock = TestServices.LoadBrokenAlliance(24).History.Current.Castles().GetModel(1);

            Assert.That(warlock.Faction, Is.EqualTo(TestCatalog.Standard.Factions["Warlock"]));
            Assert.That(Enumerable.Range(1, 6).Select(d => warlock.DwellingLevel(d)), Is.EqualTo(new[] { 1, 1, 1, 2, 1, 1 }), "Whiteshield has a Minotaur King: dwelling 4 is upgraded");
            Assert.That(((uint)warlock.Dwellings & 0x40) == 0 && ((uint)warlock.Dwellings & 0x800) != 0, Is.True);
        }

        [Test]
        public void TheGuildOfTheCastlesOfTheSavesHasTheLevelInThreeBitsAndTheSpellsOfItsLevels()
        {
            var castles = TestServices.LoadBrokenAlliance(24).History.Current.Castles();

            var peacekeeper = castles.GetModel(castles.FindByName("Peacekeeper"));
            Assert.That((peacekeeper.MageGuildLevel, peacekeeper.MageGuildSpellCounts), Is.EqualTo((5, Numbers.Of(3, 3, 2, 2, 1))));
            var dragontooth = castles.GetModel(castles.FindByName("Dragontooth"));
            Assert.That((dragontooth.MageGuildLevel, dragontooth.MageGuildSpellCounts), Is.EqualTo((3, Numbers.Of(3, 3, 2, 0, 0))));
            var vertigo = castles.GetModel(castles.FindByName("Vertigo"));
            Assert.That((vertigo.MageGuildLevel, vertigo.HasLibrary, vertigo.MageGuildSpellCounts), Is.EqualTo((5, true, Numbers.Of(4, 4, 3, 3, 2))));
            Assert.That(castles.GetModel(castles.FindByName("Woodhaven")).HasLibrary, Is.False);
            Assert.That(vertigo.WithLibrary(false).MageGuildSpellCounts, Is.EqualTo(Numbers.Of(3, 3, 2, 2, 1)));
        }

        [Test]
        public void ACastleTellsWhatEveryGameCastleHasThroughTheSharedInterface()
        {
            ICastleModel<PlayerColor, Creature, Faction> castle = Xabran();

            Assert.That((castle.Name, castle.Faction.Name, castle.Owner), Is.EqualTo(("Xabran", "Knight", (PlayerColor?)PlayerColor.Blue)));
            Assert.That((castle.MageGuildLevel, castle.CreaturesAvailable.Count, castle.Garrison.Slots.Count), Is.EqualTo((0, 6, 5)));
        }

        [Test]
        public void ACastleIsUsedByTheMapWhenItHasANameAndTheTableHoldsRecordsThatAreNot()
        {
            var castles = TestServices.LoadBrokenAlliance(10).History.Current.Castles().GetModels();

            Assert.That(((ICastleModel<PlayerColor, Creature, Faction>)Xabran()).IsUsed, Is.True);
            Assert.That(castles.Count(c => c.IsUsed), Is.GreaterThan(0).And.LessThan(castles.Count));
            Assert.That(castles.Where(c => !c.IsUsed).Select(c => c.Name), Is.All.Empty);
        }

        [Test]
        public void TheCreaturesOfAnUpgradedDwellingOfASaveAreTheSameCountAsTheOnesOfTheDwelling()
        {
            var castles = TestServices.LoadBrokenAlliance(24).History.Current.Castles();

            var hillstone = castles.GetModel(castles.FindByName("Hillstone"));

            Assert.That((hillstone.DwellingLevel(2), hillstone.CreaturesWaiting(2), hillstone.CreaturesAvailable[1]), Is.EqualTo((2, 10, 10)));
        }
    }
}
