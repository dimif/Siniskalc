namespace Siniskalc.Homm2.Tests.Models
{
    using Siniskalc.Core.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    [TestFixture]
    internal sealed class CastleModelTests
    {
        private static CastleModel Xabran() => TestServices.LoadBrokenAlliance(5).History.Current.Castles().GetModel(3);

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
            var before = Xabran();

            var after = before.WithBuilding(TownBuildings.Tavern, true).WithBuilding(TownBuildings.Statue, false);

            Assert.That(after.Town, Is.EqualTo((before.Town | TownBuildings.Tavern) & ~TownBuildings.Statue));
        }

        [Test]
        public void ADwellingIsBuiltAndTakenDownWithoutTouchingTheCreaturesThatWait()
        {
            var before = Xabran().WithWaiting(3, 20);

            var built = before.WithDwelling(3, true);
            Assert.That(built.HasDwelling(3), Is.True);
            Assert.That(built.Creatures[2], Is.EqualTo(20));

            var down = built.WithDwelling(3, false);
            Assert.That(down.HasDwelling(3), Is.False);
            Assert.That(down.Creatures[2], Is.EqualTo(20));
            Assert.That(() => before.WithDwelling(7, true), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TheMageGuildIsOneOfTwoLevelsAndTheSecondReplacesTheFirst()
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
            Assert.That((uint)five.Dwellings & 0x70000, Is.EqualTo(0x50000u), "the level is a number in three bits, as in Peacekeeper");
            Assert.That(five.GuildSpellCounts, Is.EqualTo(Numbers.Of(3, 3, 2, 2, 1)));
            Assert.That(five.WithMageGuildLevel(2).GuildSpellCounts, Is.EqualTo(Numbers.Of(3, 3, 0, 0, 0)));
        }

        [Test]
        public void ASpellOfTheGuildIsChangedByLevelAndPlace()
        {
            var castle = Xabran().WithGuildSpell(2, 1, Spell.Fireball);

            Assert.That(castle.GuildSpell(2, 1), Is.EqualTo(Spell.Fireball));
            Assert.That(castle.WithGuildSpell(2, 1, null).GuildSpell(2, 1), Is.Null);
            Assert.That(castle.GuildSpell(1, 1), Is.EqualTo(Xabran().GuildSpell(1, 1)), "the other levels stay");
        }

        [Test]
        public void ADwellingThatIsUpgradedLosesItsOwnBitAndTheUpgradesStopWhereTheCreaturesDo()
        {
            var warlock = TestServices.LoadBrokenAlliance(24).History.Current.Castles().GetModel(1);

            Assert.That(warlock.Faction, Is.EqualTo(Faction.Warlock));
            Assert.That(Enumerable.Range(1, 6).Select(d => warlock.DwellingLevel(d)), Is.EqualTo(new[] { 1, 1, 1, 2, 1, 1 }), "Whiteshield has a Minotaur King: dwelling 4 is upgraded");
            Assert.That(((uint)warlock.Dwellings & 0x40) == 0 && ((uint)warlock.Dwellings & 0x800) != 0, Is.True);

            var downgraded = warlock.WithDwellingLevel(4, 1);
            Assert.That(((uint)downgraded.Dwellings & 0x40) != 0 && ((uint)downgraded.Dwellings & 0x800) == 0, Is.True);
            Assert.That(CastleModel.MaxDwellingLevel(Faction.Warlock, 6), Is.EqualTo(3), "the dragons upgrade twice");
            Assert.That(CastleModel.MaxDwellingLevel(Faction.Warlock, 1), Is.EqualTo(1));
            Assert.That(CastleModel.MaxDwellingLevel(Faction.Barbarian, 6), Is.EqualTo(1), "the Cyclops has no upgrade");
            Assert.That(() => warlock.WithDwellingLevel(3, 2), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void TheGuildOfTheCastlesOfTheSavesHasTheLevelInThreeBitsAndTheSpellsOfItsLevels()
        {
            var castles = TestServices.LoadBrokenAlliance(24).History.Current.Castles();

            var peacekeeper = castles.GetModel(castles.FindByName("Peacekeeper"));
            Assert.That((peacekeeper.MageGuildLevel, peacekeeper.GuildSpellCounts), Is.EqualTo((5, Numbers.Of(3, 3, 2, 2, 1))));
            var dragontooth = castles.GetModel(castles.FindByName("Dragontooth"));
            Assert.That((dragontooth.MageGuildLevel, dragontooth.GuildSpellCounts), Is.EqualTo((3, Numbers.Of(3, 3, 2, 0, 0))));
            var vertigo = castles.GetModel(castles.FindByName("Vertigo"));
            Assert.That((vertigo.MageGuildLevel, vertigo.HasLibrary, vertigo.GuildSpellCounts), Is.EqualTo((5, true, Numbers.Of(4, 4, 3, 3, 2))));
            Assert.That(castles.GetModel(castles.FindByName("Woodhaven")).HasLibrary, Is.False);
            Assert.That(vertigo.WithLibrary(false).GuildSpellCounts, Is.EqualTo(Numbers.Of(3, 3, 2, 2, 1)));
            Assert.That(Xabran().WithLibrary(true), Is.EqualTo(Xabran()), "only a Wizard has a Library");
        }

        [Test]
        public void TheCreaturesOfAnUpgradedDwellingWaitInTheirOwnListAsInTheSaves()
        {
            var castles = TestServices.LoadBrokenAlliance(24).History.Current.Castles();

            var hillstone = castles.GetModel(castles.FindByName("Hillstone"));
            Assert.That((hillstone.DwellingLevel(2), hillstone.Waiting(2), hillstone.Creatures[1]), Is.EqualTo((2, 10, 0)));

            var moved = Xabran().WithWaiting(2, 7).WithDwellingLevel(2, 2);
            Assert.That((moved.Waiting(2), moved.Creatures[1], moved.UpgradedCreatures[0]), Is.EqualTo((7, 0, 7)));
            Assert.That(moved.WithDwellingLevel(2, 1).Waiting(2), Is.EqualTo(7));

            var dragons = TestServices.LoadBrokenAlliance(24).History.Current.Castles().GetModel(1).WithDwellingLevel(6, 2).WithWaiting(6, 3);
            var black = dragons.WithDwellingLevel(6, 3);
            Assert.That((black.Waiting(6), black.UpgradedCreatures[4], black.UpgradedCreatures[5]), Is.EqualTo((3, 3, 3)), "the Black Dragon waits in the sixth place and the game has 3 in the fifth as well");
            Assert.That(black.WithDwellingLevel(6, 1).UpgradedCreatures, Is.EqualTo(Numbers.Of(0, 0, 0, 0, 0, 0)));
        }
    }
}
