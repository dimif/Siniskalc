namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Core.Entities;
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
        private static ICastleModel<TestColor, IGameEntity, IFaction> Castle(int mageGuildLevel, string faction, int dwellings)
        {
            var factionMock = new Mock<IFaction>();
            factionMock.Setup(f => f.Name).Returns(faction);

            var castle = new Mock<ICastleModel<TestColor, IGameEntity, IFaction>>();
            castle.Setup(c => c.Name).Returns("Stronghold");
            castle.Setup(c => c.Faction).Returns(factionMock.Object);
            castle.Setup(c => c.MageGuildLevel).Returns(mageGuildLevel);
            castle.Setup(c => c.DwellingCount).Returns(dwellings);
            return castle.Object;
        }

        [Test]
        public void TheLinkNamesTheCastleAndItsFactionAndTellsWhatStandsInIt()
        {
            var link = CastleLinkViewModel.For(Castle(2, "Rampart", 3), new DelegateCommand(_ => { }), ["City", "Citadel"]);

            Assert.That((link.Name, link.Detail, link.Status), Is.EqualTo(("Stronghold", "Rampart", "City · Citadel · Mage Guild 2 · 3 dwellings")));
        }

        [Test]
        public void ACastleWithoutAMageGuildLeavesItOutAndANeutralFactionIsNamedNeutral()
        {
            var link = CastleLinkViewModel.For(Castle(0, "Neutral", 1), new DelegateCommand(_ => { }), ["Town"]);

            Assert.That((link.Detail, link.Status), Is.EqualTo(("Neutral", "Town · 1 dwelling")));
        }

        [Test]
        public void ACapitalAndItsGrailAreCarriedOntoTheLink()
        {
            var link = CastleLinkViewModel.For(Castle(1, "Castle", 2), new DelegateCommand(_ => { }), ["Capitol"], isCapital: true, grailText: "Colossus");

            Assert.That((link.IsCapital, link.HasGrail, link.GrailText), Is.EqualTo((true, true, "Colossus")));
        }
    }
}
