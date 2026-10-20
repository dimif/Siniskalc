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
        private static ICastleModel<TestColor, IGameEntity, IFaction> Castle(int mageGuildLevel, string faction)
        {
            var factionMock = new Mock<IFaction>();
            factionMock.Setup(f => f.Name).Returns(faction);

            var castle = new Mock<ICastleModel<TestColor, IGameEntity, IFaction>>();
            castle.Setup(c => c.Name).Returns("Stronghold");
            castle.Setup(c => c.Faction).Returns(factionMock.Object);
            castle.Setup(c => c.MageGuildLevel).Returns(mageGuildLevel);
            return castle.Object;
        }

        [Test]
        public void TheLinkNamesTheCastleAndItsFactionAndTellsWhatStandsInIt()
        {
            var link = CastleLinkViewModel.For(Castle(2, "Rampart"), new DelegateCommand(_ => { }), ["City", "Citadel"], 3);

            Assert.That((link.Name, link.Detail, link.Status), Is.EqualTo(("Stronghold", "Rampart", "City · Citadel · Mage Guild 2 · 3 dwellings")));
        }

        [Test]
        public void ACastleWithoutAMageGuildLeavesItOutAndANeutralFactionIsNamedNeutral()
        {
            var link = CastleLinkViewModel.For(Castle(0, "Neutral"), new DelegateCommand(_ => { }), ["Town"], 1);

            Assert.That((link.Detail, link.Status), Is.EqualTo(("Neutral", "Town · 1 dwelling")));
        }

        [Test]
        public void ACapitalAndItsGrailAreCarriedOntoTheLink()
        {
            var link = CastleLinkViewModel.For(Castle(1, "Castle"), new DelegateCommand(_ => { }), ["Capitol"], 2, isCapital: true, grailText: "Colossus");

            Assert.That((link.IsCapital, link.HasGrail, link.GrailText), Is.EqualTo((true, true, "Colossus")));
        }
    }
}
