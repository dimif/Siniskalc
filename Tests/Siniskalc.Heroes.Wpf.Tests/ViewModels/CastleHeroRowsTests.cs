namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the rows of the heroes in a castle, which both games make from the ids the castle holds.
    /// </summary>
    [TestFixture]
    internal sealed class CastleHeroRowsTests
    {
        private static ICastleModel<TestColor, IGameEntity, IFaction> Castle(int? garrisoned, int? visiting)
        {
            var castle = new Mock<ICastleModel<TestColor, IGameEntity, IFaction>>();
            castle.Setup(c => c.GarrisonedHeroId).Returns(garrisoned);
            castle.Setup(c => c.VisitingHeroId).Returns(visiting);
            return castle.Object;
        }

        private static IHeroSummaries<TestColor> Heroes()
        {
            var heroes = new Mock<IHeroSummaries<TestColor>>();
            heroes.Setup(h => h.FindById(10)).Returns(new HeroSummary<TestColor>(4, 10, "Ann", TestColor.Red, "Knight", 3));
            heroes.Setup(h => h.FindById(11)).Returns(new HeroSummary<TestColor>(7, 11, "Bob", TestColor.Red, "Wizard", 2));
            return heroes.Object;
        }

        [Test]
        public void ACastleWithNobodyInItHasNoRows()
        {
            Assert.That(CastleHeroRows.For(Castle(null, null), Heroes(), _ => { }), Is.Empty);
        }

        [Test]
        public void TheGarrisonedHeroComesBeforeTheOneAtTheGate()
        {
            var rows = CastleHeroRows.For(Castle(10, 11), Heroes(), _ => { });

            Assert.That(rows.Select(r => (r.Name, r.Detail)), Is.EqualTo(new[] { ("Ann", "Garrisoned"), ("Bob", "Visiting") }));
        }

        [Test]
        public void ARowOpensTheHeroAtItsPlaceAmongTheHeroesNotAtItsId()
        {
            var opened = new List<int>();
            var rows = CastleHeroRows.For(Castle(null, 11), Heroes(), opened.Add);

            rows.Single().OpenCommand.Execute(null);

            Assert.That(opened, Is.EqualTo(new[] { 7 }));
        }

        [Test]
        public void AnIdNoHeroHasIsKeptAsAnUnknownHeroThatCannotBeOpened()
        {
            var rows = CastleHeroRows.For(Castle(null, 99), Heroes(), _ => { });

            Assert.That((rows.Single().Name, rows.Single().OpenCommand.CanExecute(null)), Is.EqualTo(("Unknown hero 99", false)));
        }

        [Test]
        public void WithoutAnOpenActionTheLinksDoNothing()
        {
            var rows = CastleHeroRows.For(Castle(10, null), Heroes(), null);

            Assert.That(rows.Single().OpenCommand.CanExecute(null), Is.False);
        }
    }
}
