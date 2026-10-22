namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the list of the mines of a map that every game shows, over the mines of a made-up game.
    /// </summary>
    [TestFixture]
    internal sealed class MinesListViewModelTests
    {
        [Test]
        public void TheMinesAreSortedByResourceOwnerOrLocationFromTheHeadings()
        {
            var mines = Mines();

            mines.List.Columns["X"].SortCommand.Execute(null);
            Assert.That(mines.Rows.Select(m => (m.X, m.Y, m.Level)), Is.EqualTo(new[] { (2, 9, 0), (12, 40, 0), (12, 40, 1), (30, 4, 0) }));

            mines.List.Columns["X"].SortCommand.Execute(null);
            Assert.That(mines.Rows.Select(m => m.X), Is.Ordered.Descending);

            mines.List.Columns["Y"].SortCommand.Execute(null);
            Assert.That(mines.Rows.Select(m => m.Y), Is.Ordered);

            mines.List.Columns["Resource"].SortCommand.Execute(null);
            Assert.That(mines.Rows.Select(m => m.ResourceName), Is.Ordered);

            mines.List.Columns["Owner"].SortCommand.Execute(null);
            Assert.That(mines.Rows.Select(m => m.OwnerField.Selected), Is.Ordered);
        }

        [Test]
        public void TheFirstMineIsPickedAtFirstAndPickingNoneKeepsTheMineThatWasPicked()
        {
            var mines = Mines();
            var first = mines.Rows[0];

            Assert.That(mines.SelectedMine, Is.SameAs(first));

            mines.SelectedMine = null;
            Assert.That(mines.SelectedMine, Is.SameAs(first));

            mines.SelectedMine = mines.Rows[2];
            Assert.That(mines.SelectedMine, Is.SameAs(mines.Rows[2]));
        }

        [Test]
        public void AMapWithoutMinesHasNoneToPick()
        {
            var mines = new MinesListViewModel<MineRowViewModel<IMineModel<TestColor>, TestColor>>([]);

            Assert.That(mines.HasMines, Is.False);
            Assert.That(mines.SelectedMine, Is.Null);
            Assert.That(mines.Summary, Is.EqualTo("0 mines, 0 of them owned"));
        }

        [Test]
        public void TheSummarySaysHowManyMinesThereAreAndHowManyAreOwned()
        {
            var mines = Mines();

            Assert.That(mines.HasMines, Is.True);
            Assert.That(mines.Summary, Is.EqualTo("4 mines, 2 of them owned"));
        }

        private static MineRowViewModel<IMineModel<TestColor>, TestColor> Row(MineKind kind, TestColor owner, int x, int y, int level = MapPosition.Above)
        {
            var mine = new Mock<IMineModel<TestColor>>();
            mine.SetupGet(m => m.Kind).Returns(kind);
            mine.SetupGet(m => m.Owner).Returns(owner);
            mine.SetupGet(m => m.Position).Returns(new MapPosition(x, y, level));
            var session = new Mock<IEditSession<IMineModel<TestColor>>>();
            session.SetupGet(s => s.Current).Returns(mine.Object);
            return new MineRowViewModel<IMineModel<TestColor>, TestColor>(session.Object, (m, _) => m, string.Empty, hasLevels: true);
        }

        private static MinesListViewModel<MineRowViewModel<IMineModel<TestColor>, TestColor>> Mines() => new(
        [
            Row(MineKind.Sawmill, TestColor.Red, 30, 4),
            Row(MineKind.GoldMine, TestColor.Neutral, 12, 40),
            Row(MineKind.OreMine, TestColor.Blue, 12, 40, MapPosition.Underground),
            Row(MineKind.CrystalMine, TestColor.Neutral, 2, 9),
        ]);
    }
}
