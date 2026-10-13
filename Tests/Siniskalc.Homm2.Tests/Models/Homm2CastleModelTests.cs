namespace Siniskalc.Homm2.Tests.Models
{
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    [TestFixture]
    internal sealed class Homm2CastleModelTests
    {
        private static Homm2CastleModel Xabran() => Homm2TestServices.LoadBrokenAlliance(5).History.Current.Castles().GetModel(3);

        [Test]
        public void BuildingTheCastleTakesAwayTownOnlyAndTakingItDownPutsItBack()
        {
            var town = Xabran().WithBuilding(Homm2TownBuildings.Castle, false);

            Assert.That(town.Town.HasFlag(Homm2TownBuildings.Castle), Is.False);
            Assert.That(town.Town.HasFlag(Homm2TownBuildings.TownOnly), Is.True);
            Assert.That(town.WithBuilding(Homm2TownBuildings.Castle, true), Is.EqualTo(Xabran()));
        }

        [Test]
        public void OtherBuildingsChangeOnlyTheirOwnBit()
        {
            var before = Xabran();

            var after = before.WithBuilding(Homm2TownBuildings.Tavern, true).WithBuilding(Homm2TownBuildings.Statue, false);

            Assert.That(after.Town, Is.EqualTo((before.Town | Homm2TownBuildings.Tavern) & ~Homm2TownBuildings.Statue));
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
            Assert.That(none.Town.HasFlag(Homm2TownBuildings.MageGuild), Is.False);

            var one = none.WithMageGuildLevel(1);
            Assert.That(one.MageGuildLevel, Is.EqualTo(1));
            Assert.That(one.Town.HasFlag(Homm2TownBuildings.MageGuild), Is.True);

            var two = one.WithMageGuildLevel(2);
            Assert.That(two.MageGuildLevel, Is.EqualTo(2));
            Assert.That(two.Dwellings.HasFlag(Homm2Dwellings.MageGuild1), Is.False);
            Assert.That(() => two.WithMageGuildLevel(3), Throws.InstanceOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void ASpellOfTheGuildIsChangedByLevelAndPlace()
        {
            var castle = Xabran().WithGuildSpell(2, 1, Homm2Spell.Fireball);

            Assert.That(castle.GuildSpell(2, 1), Is.EqualTo(Homm2Spell.Fireball));
            Assert.That(castle.WithGuildSpell(2, 1, null).GuildSpell(2, 1), Is.Null);
            Assert.That(castle.GuildSpell(1, 1), Is.EqualTo(Xabran().GuildSpell(1, 1)), "the other levels stay");
        }
    }
}
