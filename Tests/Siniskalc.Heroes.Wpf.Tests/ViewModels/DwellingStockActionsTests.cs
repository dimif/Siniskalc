namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the buttons that fill the dwellings of a castle, which both games use.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class DwellingStockActionsTests
    {
        [Test]
        public void AWeekAddsTheGrowthOfThatDwellingAndStopsAtTheLimit()
        {
            var near = new FakeDwelling(true, 98, 7) { Limit = 100 };
            var far = new FakeDwelling(true, 10, 14);

            DwellingStockActions.AddWeekCommandOf(near).Execute(null);
            DwellingStockActions.AddWeekCommandOf(far).Execute(null);

            Assert.That((near.Stock, far.Stock), Is.EqualTo((100L, 24L)));
        }

        [Test]
        public void TheQuickButtonsLeaveTheDwellingsThatAreNotBuiltAlone()
        {
            var built = new FakeDwelling(true, 5, 10);
            var empty = new FakeDwelling(false, 0, 0);
            var actions = new DwellingStockActions(() => [built, empty]);

            actions.AddWeekToAllCommand.Execute(null);
            actions.AddHundredToAllCommand.Execute(null);
            Assert.That((built.Stock, empty.Stock), Is.EqualTo((115L, 0L)));
            Assert.That(DwellingStockActions.AddWeekCommandOf(empty).CanExecute(null), Is.False);

            actions.ClearAllCommand.Execute(null);
            Assert.That((built.Stock, empty.Stock), Is.EqualTo((0L, 0L)));
        }

        private sealed class FakeDwelling(bool canStock, long stock, int growth) : IStockedDwelling
        {
            private long stock = stock;

            public long Limit { get; init; } = 1000;

            public bool CanStock => canStock;

            public long Stock
            {
                get => this.stock;
                set => this.stock = Math.Clamp(value, 0, this.Limit);
            }

            public int WeeklyGrowthAmount => growth;
        }
    }
}
