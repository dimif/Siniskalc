namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Core.Entities;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the link to a castle that any game can make from what every castle has.
    /// </summary>
    [TestFixture]
    internal sealed class CastleLinkViewModelTests
    {
        private static TestCastle Castle(int mageGuildLevel, TestFaction faction) => new("Stronghold", faction, mageGuildLevel);

        [Test]
        public void TheLinkNamesTheCastleAndItsFactionAndTellsWhatStandsInIt()
        {
            var link = CastleLinkViewModel.For(Castle(2, new TestFaction("Rampart", 1)), new DelegateCommand(_ => { }), ["City", "Citadel"], 3);

            Assert.That((link.Name, link.Detail, link.Status), Is.EqualTo(("Stronghold", "Rampart", "City · Citadel · Mage Guild 2 · 3 dwellings")));
        }

        [Test]
        public void ACastleWithoutAMageGuildLeavesItOutAndANeutralFactionIsNamedNeutral()
        {
            var link = CastleLinkViewModel.For(Castle(0, new TestFaction("Neutral", 99)), new DelegateCommand(_ => { }), ["Town"], 1);

            Assert.That((link.Detail, link.Status), Is.EqualTo(("Neutral", "Town · 1 dwelling")));
        }

        private sealed record TestFaction(string Name, byte Code) : IFaction;

        private sealed record TestCreature(string Name, byte Code) : IGameEntity;

        private sealed record TestCastle(string Name, TestFaction Faction, int MageGuildLevel) : ICastleModel<TestColor, TestCreature, TestFaction>
        {
            public TestColor? Owner => null;

            public MapPosition Position => new(0, 0);

            public MapPosition? ShipyardPosition => null;

            public IArmy<TestCreature> Garrison => new Army<TestCreature>(5);

            public int? VisitingHeroId => null;

            public Numbers CreaturesAvailable => Numbers.Of(0, 0, 0, 0, 0, 0);
        }
    }
}
