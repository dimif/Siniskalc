namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the lists of what a colour owns, which the pages of every game show.
    /// </summary>
    [TestFixture]
    internal sealed class OwnedLinksTests
    {
        private static IHeroSummaries<TestColor> Heroes()
        {
            var heroes = new Mock<IHeroSummaries<TestColor>>();
            heroes.Setup(h => h.GetSummaries()).Returns(
            [
                new HeroSummary<TestColor>(0, 10, "Zed", TestColor.Red, "Knight", 3),
                new HeroSummary<TestColor>(1, 11, "Ann", TestColor.Blue, "Wizard", 2),
                new HeroSummary<TestColor>(2, 12, "Bob", TestColor.Red, "Wizard", 5),
            ]);
            return heroes.Object;
        }

        [Test]
        public void TheHeroesOfAColourAreListedByName()
        {
            var links = OwnedLinks.Heroes(Heroes(), TestColor.Red, _ => { });

            Assert.That(links.Select(l => l.Name), Is.EqualTo(new[] { "Bob", "Zed" }));
        }

        [Test]
        public void OpeningAHeroOpensItsPlaceAmongTheHeroes()
        {
            var opened = new List<int>();

            OwnedLinks.Heroes(Heroes(), TestColor.Red, opened.Add).First().OpenCommand.Execute(null);

            Assert.That(opened, Is.EqualTo(new[] { 2 }));
        }

        [Test]
        public void WithoutAnOpenActionTheLinksDoNothing()
        {
            var links = OwnedLinks.Heroes(Heroes(), TestColor.Red, null);

            Assert.That(links.All(l => !l.OpenCommand.CanExecute(null)), Is.True);
        }
    }
}
