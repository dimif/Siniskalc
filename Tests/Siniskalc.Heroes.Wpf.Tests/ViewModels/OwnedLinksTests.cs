namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
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
            heroes.Setup(h => h.FindById(It.IsAny<int>())).Returns((int id) => heroes.Object.GetSummaries().FirstOrDefault(s => s.HeroId == id));
            return heroes.Object;
        }

        private static ICastleModel<TestColor, IGameEntity, IFaction> Castle(string name, TestColor owner, int? visitingHeroId = null)
        {
            var castle = new Mock<ICastleModel<TestColor, IGameEntity, IFaction>>();
            castle.Setup(c => c.Name).Returns(name);
            castle.Setup(c => c.IsUsed).Returns(name.Length > 0);
            castle.Setup(c => c.Owner).Returns(owner);
            castle.Setup(c => c.Faction).Returns(Mock.Of<IFaction>(f => f.Name == "Knight"));
            castle.Setup(c => c.VisitingHeroId).Returns(visitingHeroId);
            return castle.Object;
        }

        private static IReadOnlyList<ICastleModel<TestColor, IGameEntity, IFaction>> Castles() =>
        [
            Castle("Wesley", TestColor.Red),
            Castle(string.Empty, TestColor.Red),
            Castle("Anvil", TestColor.Red, visitingHeroId: 10),
            Castle("Bree", TestColor.Blue),
        ];

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

        [Test]
        public void TheCastlesOfAColourThatTheMapUsesAreListedByNameWithTheWordsOfTheGame()
        {
            var links = OwnedLinks.Castles<ICastleModel<TestColor, IGameEntity, IFaction>, TestColor, IGameEntity, IFaction>(
                Castles(), Heroes(), TestColor.Red, _ => { }, null, castle => [castle.Name == "Anvil" ? "Castle" : "Town"], castle => castle.Name == "Anvil", _ => "Colossus");

            Assert.That(links.Select(l => (l.Name, l.Status, l.IsCapital, l.GrailText)), Is.EqualTo(new[] { ("Anvil", "Castle · 0 dwellings", true, "Colossus"), ("Wesley", "Town · 0 dwellings", false, "Colossus") }));
            Assert.That(links[0].Heroes.Select(h => h.Name), Is.EqualTo(new[] { "Zed" }));
        }

        [Test]
        public void OpeningACastleOpensItsPlaceAmongTheCastlesAndAGameWithoutCapitalsMarksNone()
        {
            var opened = new List<int>();

            var links = OwnedLinks.Castles<ICastleModel<TestColor, IGameEntity, IFaction>, TestColor, IGameEntity, IFaction>(Castles(), Heroes(), TestColor.Red, opened.Add, null, _ => ["Town"]);
            links[0].OpenCommand.Execute(null);

            Assert.That(opened, Is.EqualTo(new[] { 2 }));
            Assert.That(links.Select(l => (l.IsCapital, l.HasGrail)), Has.All.EqualTo((false, false)));
        }
    }
}
