namespace Siniskalc.Heroes.Wpf.Tests.MultiUnit.ViewModels
{
    using Moq;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.Tests.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the two places of the tavern of a player, which both games make from the heroes of their savefile.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class TavernPlaceViewModelTests
    {
        private TavernHeroes tavern = TavernHeroes.Empty;

        private static Mock<IHeroSummaries<TestColor>> Heroes()
        {
            var heroes = new Mock<IHeroSummaries<TestColor>>();
            heroes.Setup(h => h.GetSummaries()).Returns(
            [
                new HeroSummary<TestColor>(0, 10, "Ann", TestColor.Neutral, "Knight", 1),
                new HeroSummary<TestColor>(1, 11, "Bob", TestColor.Neutral, "Wizard", 2),
                new HeroSummary<TestColor>(2, 12, "Cid", TestColor.Red, "Knight", 3),
            ]);
            heroes.Setup(h => h.IndexOfId(It.IsAny<int>())).Returns((int id) => id switch { 10 => 0, 11 => 1, 12 => 2, _ => -1 });
            return heroes;
        }

        [SetUp]
        public void StartWithAnEmptyTavern() => this.tavern = TavernHeroes.Empty;

        private IReadOnlyList<TavernPlaceViewModel> Places(IHeroSummaries<TestColor> heroes, Action<int>? open = null) =>
            TavernPlaceViewModel.ForTavern(() => this.tavern, edit => this.tavern = edit(this.tavern), () => heroes, open);

        [Test]
        public void ThereAreTwoPlacesLabelledFirstAndSecond()
        {
            Assert.That(this.Places(Heroes().Object).Select(p => p.Label), Is.EqualTo(new[] { "First hero", "Second hero" }));
        }

        [Test]
        public void APlaceOffersNobodyAndTheHeroesNoPlayerOwns()
        {
            var places = this.Places(Heroes().Object);

            Assert.That(places[0].Choices.Select(c => c.HeroId), Is.EqualTo(new int?[] { null, 10, 11 }));
        }

        [Test]
        public void PickingAHeroPutsItInTheTavernAndTheOtherPlaceNoLongerOffersIt()
        {
            var places = this.Places(Heroes().Object);

            places[0].Selected = places[0].Choices.Single(c => c.HeroId == 10);
            places[1].Refresh();

            Assert.That(this.tavern.First, Is.EqualTo(10));
            Assert.That(places[1].Choices.Select(c => c.HeroId), Is.EqualTo(new int?[] { null, 11 }));
        }

        [Test]
        public void TheLinkOfAPlaceOpensTheHeroInIt()
        {
            this.tavern = new TavernHeroes(11, null);
            var opened = new List<int>();
            var places = this.Places(Heroes().Object, opened.Add);

            places[0].OpenCommand.Execute(null);

            Assert.That(opened, Is.EqualTo(new[] { 1 }));
            Assert.That(places[1].OpenCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void TheHeroesAreAskedForEachTimeTheChoicesAreWorkedOut()
        {
            var heroes = Heroes();
            var places = this.Places(heroes.Object);

            places[0].Refresh();

            heroes.Verify(h => h.GetSummaries(), Times.AtLeast(2));
        }
    }
}
