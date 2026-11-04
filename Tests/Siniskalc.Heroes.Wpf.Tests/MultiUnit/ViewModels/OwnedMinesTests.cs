namespace Siniskalc.Heroes.Wpf.Tests.MultiUnit.ViewModels
{
    using Siniskalc.Heroes.Wpf.Tests.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the list of the mines of one owner that every game shows on the owner's page, over the mines of a made-up game.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class OwnedMinesTests
    {
        [Test]
        public void TheListOfAnOwnerHoldsOnlyTheMinesThatOwnerHasAndPicksTheFirstOfThem()
        {
            var red = OwnedMines.List(4, TestMineRows.OfFour, TestColor.Red);

            Assert.That(red.Rows.Select(m => m.Index), Is.EquivalentTo(new[] { 0, 2 }));
            Assert.That(red.SelectedMine, Is.SameAs(red.Rows[0]));
        }

        [Test]
        public void TheListOfNeutralHoldsTheMinesNobodyOwns()
        {
            var neutral = OwnedMines.List(4, TestMineRows.OfFour, TestColor.Neutral);

            Assert.That(neutral.Rows.Select(m => m.Index), Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void AnOwnerWithNoMineHasAnEmptyListWithNoMinePicked()
        {
            var none = OwnedMines.List(1, TestMineRows.OfFour, TestColor.Blue);

            Assert.That(none.HasMines, Is.False);
            Assert.That(none.SelectedMine, Is.Null);
        }
    }
}
