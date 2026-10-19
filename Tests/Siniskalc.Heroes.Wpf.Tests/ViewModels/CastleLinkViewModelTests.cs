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
        private static TestCastle Castle(int mageGuildLevel, IFaction? faction = null) => new("Stronghold", faction, mageGuildLevel);

        [Test]
        public void TheLinkNamesTheCastleAndItsFactionAndTellsWhatStandsInIt()
        {
            var link = CastleLinkViewModel.For(Castle(2, new TestFaction("Rampart", 1)), new DelegateCommand(_ => { }), ["City", "Citadel"], 3);

            Assert.That((link.Name, link.Detail, link.Status), Is.EqualTo(("Stronghold", "Rampart", "City · Citadel · Mage Guild 2 · 3 dwellings")));
        }

        [Test]
        public void ACastleWithoutAMageGuildOrAFactionLeavesThemOut()
        {
            var link = CastleLinkViewModel.For(Castle(0), new DelegateCommand(_ => { }), ["Town"], 1);

            Assert.That((link.Detail, link.Status), Is.EqualTo((string.Empty, "Town · 1 dwelling")));
        }

        private sealed record TestFaction(string Name, byte Code) : IFaction;

        private sealed record TestCreature(string Name, byte Code) : IGameEntity;

        private sealed record TestCastle(string Name, IFaction? Faction, int MageGuildLevel) : ICastleModel<TestColor, TestCreature>
        {
            public TestColor? Owner => null;

            public MapPosition Position => new(0, 0);

            public MapPosition? ShipyardPosition => null;

            public Army<TestCreature> Garrison => new(5);

            public int? VisitingHeroId => null;

            public Numbers CreaturesAvailable => Numbers.Of(0, 0, 0, 0, 0, 0);
        }
    }
}
