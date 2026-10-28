namespace Siniskalc.Homm2.Tests.Models
{
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Tests.Data;

    /// <summary>
    /// Tests the reducers of the model of a castle on a castle made by hand: buildings, dwellings with the creatures that wait in them,
    /// the Mage Guild and the Library.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class CastleModelTests
    {
        private static CastleModel Xabran() => DifferSamples.Castle();

        [Test]
        public void BuildingTheCastleTakesAwayTownOnlyAndTakingItDownPutsItBack()
        {
            var town = Xabran().WithBuilding(TownBuildings.Castle, false);

            Assert.That(town.Town.HasFlag(TownBuildings.Castle), Is.False);
            Assert.That(town.Town.HasFlag(TownBuildings.TownOnly), Is.True);
            Assert.That(town.WithBuilding(TownBuildings.Castle, true), Is.EqualTo(Xabran()));
        }

        [Test]
        public void OtherBuildingsChangeOnlyTheirOwnBit()
        {
            var before = Xabran() with { Town = TownBuildings.Castle | TownBuildings.Statue };

            var after = before.WithBuilding(TownBuildings.Tavern, true).WithBuilding(TownBuildings.Statue, false);

            Assert.That(after.Town, Is.EqualTo(TownBuildings.Castle | TownBuildings.Tavern));
        }

        [Test]
        public void ADwellingIsBuiltAndTakenDownWithoutTouchingTheCreaturesThatWait()
        {
            var before = Xabran().WithCreaturesWaiting(3, 20);

            var built = before.WithDwelling(3, true);
            Assert.That(built.HasDwelling(3), Is.True);
            Assert.That(built.CreaturesAvailable[2], Is.EqualTo(20));

            var down = built.WithDwelling(3, false);
            Assert.That(down.HasDwelling(3), Is.False);
            Assert.That(down.CreaturesAvailable[2], Is.EqualTo(20));
            Assert.That(() => before.WithDwelling(7, true), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TheMageGuildIsOneOfFiveLevelsAndTheLevelIsANumberInThreeBits()
        {
            var none = Xabran().WithMageGuildLevel(0);
            Assert.That(none.MageGuildLevel, Is.EqualTo(0));
            Assert.That(none.Town.HasFlag(TownBuildings.MageGuild), Is.False);

            var one = none.WithMageGuildLevel(1);
            Assert.That(one.MageGuildLevel, Is.EqualTo(1));
            Assert.That(one.Town.HasFlag(TownBuildings.MageGuild), Is.True);

            var two = one.WithMageGuildLevel(2);
            Assert.That(two.MageGuildLevel, Is.EqualTo(2));
            Assert.That(two.Dwellings.HasFlag(Dwellings.MageGuild1), Is.False);
            Assert.That(() => two.WithMageGuildLevel(6), Throws.InstanceOf<ArgumentOutOfRangeException>());

            var five = two.WithMageGuildLevel(5);
            Assert.That(five.MageGuildLevel, Is.EqualTo(5));
            Assert.That((uint)five.Dwellings & 0x70000, Is.EqualTo(0x50000u));
            Assert.That(five.MageGuildSpellCounts, Is.EqualTo(Numbers.Of(3, 3, 2, 2, 1)));
            Assert.That(five.WithMageGuildLevel(2).MageGuildSpellCounts, Is.EqualTo(Numbers.Of(3, 3, 0, 0, 0)));
        }

        [Test]
        public void ASpellOfTheGuildIsChangedByLevelAndPlace()
        {
            var castle = Xabran().WithMageGuildLevel(2).WithMageGuildSpell(2, 1, Spell.Fireball);

            Assert.That(castle.MageGuildSpell(2, 1), Is.EqualTo(Spell.Fireball));
            Assert.That(castle.WithMageGuildSpell(2, 1, null).MageGuildSpell(2, 1), Is.Null);
            Assert.That(castle.WithMageGuildSpell(1, 1, Spell.Fireblast).MageGuildSpell(2, 1), Is.EqualTo(Spell.Fireball), "the other levels stay");
        }

        [Test]
        public void ADwellingThatIsUpgradedLosesItsOwnBitAndTheUpgradesStopWhereTheCreaturesDo()
        {
            var warlock = (Xabran() with { Faction = Faction.Warlock }).WithDwelling(1, true).WithDwelling(4, true).WithDwellingLevel(4, 2);

            Assert.That(Enumerable.Range(1, 6).Select(d => warlock.DwellingLevel(d)), Is.EqualTo(new[] { 1, 0, 0, 2, 0, 0 }));
            Assert.That(((uint)warlock.Dwellings & 0x40) == 0 && ((uint)warlock.Dwellings & 0x800) != 0, Is.True);

            var downgraded = warlock.WithDwellingLevel(4, 1);
            Assert.That(((uint)downgraded.Dwellings & 0x40) != 0 && ((uint)downgraded.Dwellings & 0x800) == 0, Is.True);
            Assert.That(CastleModel.MaxDwellingLevel(Faction.Warlock, 6), Is.EqualTo(3), "the dragons upgrade twice");
            Assert.That(CastleModel.MaxDwellingLevel(Faction.Warlock, 1), Is.EqualTo(1));
            Assert.That(CastleModel.MaxDwellingLevel(Faction.Barbarian, 6), Is.EqualTo(1), "the Cyclops has no upgrade");
            Assert.That(() => warlock.WithDwellingLevel(3, 2), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void OnlyAWizardHasALibraryAndItAddsOneSpellToEveryLevelOfTheGuild()
        {
            var wizard = (Xabran() with { Faction = Faction.Wizard }).WithMageGuildLevel(5);

            var library = wizard.WithLibrary(true);

            Assert.That((library.HasLibrary, library.MageGuildSpellCounts), Is.EqualTo((true, Numbers.Of(4, 4, 3, 3, 2))));
            Assert.That(library.WithLibrary(false).MageGuildSpellCounts, Is.EqualTo(Numbers.Of(3, 3, 2, 2, 1)));
            Assert.That(Xabran().WithLibrary(true), Is.EqualTo(Xabran()), "only a Wizard has a Library");
            Assert.That(library.WithMageGuildLevel(3).MageGuildSpellCounts, Is.EqualTo(Numbers.Of(4, 4, 3, 0, 0)), "a guild built later shows the extra spell too");
        }

        [Test]
        public void ACastleCountsItsDwellingsAndHasNoGarrisonHero()
        {
            var castle = Xabran().WithDwelling(1, true).WithDwelling(2, true);

            ICastleModel<PlayerColor, Creature, Faction> shared = castle;

            Assert.That(shared.DwellingCount, Is.EqualTo(2));
            var all = Enumerable.Range(1, 6).Aggregate(castle, (c, d) => c.WithDwelling(d, true));
            var none = Enumerable.Range(1, 6).Aggregate(castle, (c, d) => c.WithDwelling(d, false));
            Assert.That((none.DwellingCount, all.DwellingCount), Is.EqualTo((0, 6)));
            Assert.That(shared.GarrisonedHeroId, Is.Null);
        }

        [Test]
        public void TheCreaturesOfAnUpgradedDwellingAreTheSameCountAsTheOnesOfTheDwelling()
        {
            var upgraded = Xabran().WithCreaturesWaiting(2, 7).WithDwellingLevel(2, 2);
            Assert.That((upgraded.CreaturesWaiting(2), upgraded.CreaturesAvailable[1]), Is.EqualTo((7, 7)));
            Assert.That(upgraded.WithDwellingLevel(2, 1).CreaturesWaiting(2), Is.EqualTo(7));

            var dragons = (Xabran() with { Faction = Faction.Warlock }).WithDwellingLevel(6, 2).WithCreaturesWaiting(6, 3);
            Assert.That(dragons.WithDwellingLevel(6, 3).CreaturesWaiting(6), Is.EqualTo(3), "the Black Dragon waits the same as the Red");
        }
    }
}
