namespace Siniskalc.Heroes.Wpf.Tests.MultiUnit.ViewModels
{
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the Banishing card of a player page.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class DaysLeftCardViewModelTests
    {
        private readonly List<int> written = [];
        private int? daysLeft;

        [SetUp]
        public void GiveThePlayerATown()
        {
            this.daysLeft = null;
            this.written.Clear();
        }

        [Test]
        public void TheCardIsShownOnlyWhileThePlayerOwnsNoTown()
        {
            var card = this.Card();

            Assert.That(card.HasBanishing, Is.False);

            this.daysLeft = 6;
            Assert.That(card.HasBanishing, Is.True);

            this.daysLeft = 0;
            Assert.That(card.HasBanishing, Is.True, "a banished player is shown, but cannot be given days");
        }

        [Test]
        public void SevenButtonsOfSevenDownToOneMarkTheDaysLeft()
        {
            this.daysLeft = 6;
            var card = this.Card();

            Assert.That(card.DayChoices.Select(c => c.Days), Is.EqualTo(new[] { 7, 6, 5, 4, 3, 2, 1 }));
            Assert.That(card.DayChoices.Where(c => c.IsCurrent).Select(c => c.Days), Is.EqualTo(new[] { 6 }));
        }

        [Test]
        public void PickingADayGivesThePlayerThoseDaysAndMarksTheButtonOnceRefreshed()
        {
            this.daysLeft = 6;
            var card = this.Card();
            var raised = new List<string?>();
            var two = card.DayChoices.Single(c => c.Days == 2);
            two.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            two.SelectCommand.Execute(null);
            card.Refresh();

            Assert.That(this.written, Is.EqualTo(new[] { 2 }));
            Assert.That(two.IsCurrent, Is.True);
            Assert.That(raised, Does.Contain(nameof(DaysLeftChoiceViewModel.IsCurrent)));
        }

        [Test]
        public void ABanishedPlayerCannotBeGivenDays()
        {
            this.daysLeft = 0;
            var card = this.Card(isBanished: true);

            card.SetDaysLeft(4);

            Assert.That(card.DayChoices.Select(c => c.SelectCommand.CanExecute(null)), Has.All.False);
            Assert.That(this.written, Is.Empty);
        }

        [TestCase(0)]
        [TestCase(8)]
        public void AnAmountOfDaysThatTheGameDoesNotCountIsRefused(int days)
        {
            this.daysLeft = 6;
            var card = this.Card();

            Assert.That(() => card.SetDaysLeft(days), Throws.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(this.written, Is.Empty);
        }

        private DaysLeftCardViewModel Card(bool isBanished = false) =>
            new(() => this.daysLeft, days => { this.written.Add(days); this.daysLeft = days; }, () => isBanished);
    }
}
