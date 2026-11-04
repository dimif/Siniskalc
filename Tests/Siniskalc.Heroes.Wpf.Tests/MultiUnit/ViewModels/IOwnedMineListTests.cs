namespace Siniskalc.Heroes.Wpf.Tests.MultiUnit.ViewModels
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.Tests.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests what a page that lists the mines of one owner tells the owned-mines card through its list, over the mines of a made-up game.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class IOwnedMineListTests
    {
        [Test]
        public void TheCardIsToldOfAnotherMinePickedByTheListOfThePage()
        {
            IOwnedMines page = new Page(OwnedMines.List(4, TestMineRows.OfFour, TestColor.Red));
            var raised = new List<string?>();
            page.MineSelection.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            ((Page)page).Mines.PickMine(2);

            Assert.That(raised, Does.Contain(nameof(MinesListViewModel<MineRowViewModelBase>.SelectedMine)));
            Assert.That(page.SelectedMineIndex, Is.EqualTo(2));
        }

        [Test]
        public void APageWhoseOwnerHasNoMineHasNoMinePicked()
        {
            IOwnedMines page = new Page(OwnedMines.List(1, TestMineRows.OfFour, TestColor.Blue));

            Assert.That(page.SelectedMineIndex, Is.Null);
        }

        private sealed class Page(MinesListViewModel<MineRowViewModel<IMineModel<TestColor>, TestColor>> mines) : IOwnedMineList<MineRowViewModel<IMineModel<TestColor>, TestColor>>
        {
            public MinesListViewModel<MineRowViewModel<IMineModel<TestColor>, TestColor>> Mines { get; } = mines;

            public IByteEditTarget? CreateMineByteEditTarget() => null;
        }
    }
}
