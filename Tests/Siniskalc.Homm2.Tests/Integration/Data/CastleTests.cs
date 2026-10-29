namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// What the castles of Broken Alliance hold, as written in the README of the saves (TestData/Homm2/BrokenAlliance).
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class CastleTests
    {
        private const int Whiteshield = 1;
        private const int Corackston = 2;
        private const int Xabran = 3;

        private static CastleModel Castle(int save, int castle) => TestServices.LoadBrokenAlliance(save).History.Current.Castles().GetModel(castle);

        [Test]
        public void XabranStartsAsTheCastleOfBlueWithOnlyTheFirstDwelling()
        {
            var xabran = Castle(1, Xabran);

            Assert.That(xabran.Name, Is.EqualTo("Xabran"));
            Assert.That(xabran.Index, Is.EqualTo(Xabran));
            Assert.That(xabran.Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That(xabran.Faction, Is.EqualTo(Faction.Knight));
            Assert.That(xabran.Position, Is.EqualTo(new MapPosition(28, 25)));
            Assert.That(xabran.ShipyardPosition, Is.Null);
            Assert.That(xabran.HasCastle, Is.True);
            Assert.That(xabran.Dwellings, Is.EqualTo(Dwellings.Dwelling1));
            Assert.That(xabran.Town, Is.EqualTo(TownBuildings.Castle));
            Assert.That(xabran.CreaturesAvailable[0], Is.EqualTo(12));
            Assert.That(xabran.MageGuildLevel, Is.Zero);
            Assert.That(xabran.VisitingHeroId, Is.Null);
            Assert.That(xabran.Garrison.Slots, Is.All.Null);
        }

        [Test]
        public void TheArcheryRangeIsTheSecondDwellingAndBringsTheEightArchersOfTheWeek()
        {
            var before = Castle(4, Xabran);
            var after = Castle(5, Xabran);

            Assert.That(before.HasDwelling(2), Is.False);
            Assert.That(after.HasDwelling(1), Is.True);
            Assert.That(after.HasDwelling(2), Is.True);
            Assert.That(after.Dwellings, Is.EqualTo(Dwellings.Dwelling1 | Dwellings.Dwelling2));
            Assert.That(after.CreaturesAvailable.Take(2), Is.EqualTo(new[] { 12, 8 }));
        }

        [Test]
        public void TheMageGuildHasOneLevelAtATimeAndTheSpellsTheGameShowed()
        {
            var one = Castle(6, Xabran);
            var two = Castle(10, Xabran);

            Assert.That(one.MageGuildLevel, Is.EqualTo(1));
            Assert.That(one.Town.HasFlag(TownBuildings.MageGuild), Is.True);
            Assert.That(two.MageGuildLevel, Is.EqualTo(2));
            Assert.That(two.Dwellings.HasFlag(Dwellings.MageGuild1), Is.False);
            Assert.That(two.MageGuildSpellCounts.Take(3), Is.EqualTo(new[] { 3, 3, 0 }));
            Assert.That(Enumerable.Range(0, 3).Select(slot => two.MageGuildSpell(1, slot)), Is.EqualTo(new[] { Spell.MagicArrow, Spell.ViewResources, Spell.Cure }));
            Assert.That(Enumerable.Range(0, 3).Select(slot => two.MageGuildSpell(2, slot)), Is.EqualTo(new[] { Spell.DisruptingRay, Spell.ViewArtifacts, Spell.Steelskin }));
        }

        [Test]
        public void TheOtherBuildingsAreBitsOfTheTownAndTheWeekGrowsTheDwellingsWithTheWell()
        {
            Assert.That(Castle(10, Xabran).Town, Is.EqualTo(TownBuildings.Castle | TownBuildings.MageGuild));
            Assert.That(Castle(11, Xabran).Town.HasFlag(TownBuildings.Statue), Is.True);
            Assert.That(Castle(12, Xabran).Town.HasFlag(TownBuildings.Well), Is.True);
            Assert.That(Castle(13, Xabran).Town, Is.EqualTo(TownBuildings.Castle | TownBuildings.MageGuild | TownBuildings.Statue | TownBuildings.Well | TownBuildings.Tavern | TownBuildings.ThievesGuild));
            Assert.That(Castle(11, Xabran).CreaturesAvailable.Take(2), Is.EqualTo(new[] { 12, 8 }));
            Assert.That(Castle(12, Xabran).CreaturesAvailable.Take(2), Is.EqualTo(new[] { 26, 18 }));
        }

        [Test]
        public void ATownWithoutACastleIsCapturedWithItsGarrisonEmptiedAndOnlyTheFirstDwelling()
        {
            var neutral = Castle(13, Corackston);
            var captured = Castle(14, Corackston);

            Assert.That(neutral.Owner, Is.EqualTo(PlayerColor.Neutral));
            Assert.That(neutral.Faction, Is.EqualTo(Faction.Wizard));
            Assert.That(neutral.Garrison.Slots.Count(s => s != null), Is.EqualTo(3));
            Assert.That(captured.Owner, Is.EqualTo(PlayerColor.Blue));
            Assert.That(captured.Garrison.Slots, Is.All.Null);
            Assert.That(captured.HasCastle, Is.False);
            Assert.That(captured.Town, Is.EqualTo(TownBuildings.TownOnly));
            Assert.That(captured.Dwellings, Is.EqualTo(Dwellings.Dwelling1));
            Assert.That(captured.VisitingHeroId, Is.EqualTo(7));
        }

        [Test]
        public void ACastleLostToAnotherPlayerChangesOwnerAndGetsTheGarrisonOfTheConqueror()
        {
            var lost = Castle(18, Corackston);

            Assert.That(lost.Owner, Is.EqualTo(PlayerColor.Yellow));
            Assert.That(lost.Garrison.Slots.Count(s => s != null), Is.EqualTo(1));
        }

        [Test]
        public void TheShipyardAndTheGreenTowerOfAWarlockCastleAreBitsToo()
        {
            var shipyard = Castle(20, Whiteshield);
            var tower = Castle(24, Whiteshield);

            Assert.That(shipyard.Faction, Is.EqualTo(Faction.Warlock));
            Assert.That(shipyard.Town.HasFlag(TownBuildings.Shipyard), Is.True);
            Assert.That(shipyard.ShipyardPosition, Is.EqualTo(new MapPosition(62, 10)));
            Assert.That(Castle(19, Whiteshield).Town.HasFlag(TownBuildings.Shipyard), Is.False);
            Assert.That(shipyard.HasDwelling(6), Is.False);
            Assert.That(tower.HasDwelling(6), Is.True);
            Assert.That(tower.CreaturesAvailable[5], Is.EqualTo(1));
            Assert.That(tower.Owner, Is.EqualTo(PlayerColor.Blue));
        }

        [Test]
        public void TheMapUsesElevenOfTheSeventyTwoCastlesAndTheOthersHaveNoName()
        {
            var castles = TestServices.LoadBrokenAlliance(1).History.Current.Castles();

            Assert.That(castles, Has.Count.EqualTo(72));
            Assert.That(castles.UsedIndexes(), Is.EqualTo(Enumerable.Range(0, 11)));
            Assert.That(castles.GetModels().Select(c => c.Index), Is.EqualTo(Enumerable.Range(0, 72)));
            Assert.That(castles.FindByName("xabran"), Is.EqualTo(Xabran));
            Assert.That(castles.FindByName(string.Empty), Is.EqualTo(-1));
            Assert.That(castles.GetModels().Where(c => c.IsUsed).Select(c => c.Name).Take(4), Is.EqualTo(new[] { "Willow", "Whiteshield", "Corackston", "Xabran" }));
            Assert.That(castles.GetOwners()[Xabran], Is.EqualTo(PlayerColor.Blue));
        }
    }
}
