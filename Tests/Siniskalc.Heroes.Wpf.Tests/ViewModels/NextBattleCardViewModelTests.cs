namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using Moq;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the card of what visited places gave a hero for its next battle, on rows that are fakes: the count of places, the clear
    /// button and showing the rows again.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class NextBattleCardViewModelTests
    {
        private static Mock<IAmountRow> Amount(long value)
        {
            var row = new Mock<IAmountRow>();
            row.SetupGet(r => r.Value).Returns(value);
            return row;
        }

        private static Mock<IToggleRow> Toggle(bool isChecked)
        {
            var row = new Mock<IToggleRow>();
            row.SetupGet(r => r.IsChecked).Returns(isChecked);
            return row;
        }

        private static NextBattleCardViewModel Card(IAmountRow[] bonus, IToggleRow[] sources, Action? clearAll = null) =>
            new("Next battle", bonus, sources, "Tried in the game", clearAll);

        [Test]
        public void TheCardKeepsItsTitleItsRowsAndItsNote()
        {
            var bonus = new[] { Amount(1).Object };
            var sources = new[] { Toggle(true).Object };

            var card = Card(bonus, sources);

            Assert.That((card.Title, card.Note), Is.EqualTo(("Next battle", "Tried in the game")));
            Assert.That(card.Bonus, Is.EqualTo(bonus));
            Assert.That(card.Sources, Is.EqualTo(sources));
        }

        [Test]
        public void TheSourcesTextCountsTheCheckedPlacesOutOfAllOfThem()
        {
            var card = Card([], [Toggle(true).Object, Toggle(false).Object, Toggle(true).Object, Toggle(false).Object, Toggle(false).Object]);

            Assert.That(card.SourcesText, Is.EqualTo("2 of 5"));
        }

        [Test]
        public void ACardWithoutAClearActionHasNoClearButtonAndCannotClear()
        {
            var card = Card([Amount(2).Object], [Toggle(true).Object]);

            Assert.That(card.HasClear, Is.False);
            Assert.That(card.ClearCommand.CanExecute(null), Is.False);
        }

        [TestCase(0L, false, false)]
        [TestCase(-1L, false, true)]
        [TestCase(2L, false, true)]
        [TestCase(0L, true, true)]
        public void ClearIsPossibleWhenABonusIsNotZeroOrAPlaceIsChecked(long bonus, bool visited, bool canClear)
        {
            var card = Card([Amount(0).Object, Amount(bonus).Object], [Toggle(false).Object, Toggle(visited).Object], () => { });

            Assert.That(card.HasClear, Is.True);
            Assert.That(card.ClearCommand.CanExecute(null), Is.EqualTo(canClear));
        }

        [Test]
        public void TheClearCommandRunsTheActionOfTheGame()
        {
            var cleared = 0;
            var card = Card([Amount(1).Object], [], () => cleared++);

            card.ClearCommand.Execute(null);

            Assert.That(cleared, Is.EqualTo(1));
        }

        [Test]
        public void RefreshShowsEveryRowAgainAndRaisesTheCountOfPlaces()
        {
            var amount = Amount(1);
            var toggle = Toggle(true);
            var card = Card([amount.Object], [toggle.Object]);
            var raised = new List<string?>();
            ((System.ComponentModel.INotifyPropertyChanged)card).PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            card.Refresh();

            amount.Verify(r => r.Refresh(), Times.Once);
            toggle.Verify(r => r.Refresh(), Times.Once);
            Assert.That(raised, Is.EqualTo(new[] { nameof(NextBattleCardViewModel.SourcesText) }));
        }
    }
}
