namespace Siniskalc.Wpf.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests a row that edits one amount (a resource, a count) within limits and says how far it is from the savefile.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class AmountRowViewModelTests
    {
        private long stored;
        private List<long> written = null!;

        [SetUp]
        public void StartWithTenHundred()
        {
            this.stored = 1000;
            this.written = [];
        }

        private AmountRowViewModel Row(string name = "Gold", long minimum = 0, long maximum = 5000, Func<string>? note = null) =>
            new(name, "#FFD700", minimum, maximum, 100, () => this.stored, () => 1000, v =>
            {
                this.written.Add(v);
                this.stored = v;
            }, note);

        [Test]
        public void ARowShowsWhatItWasGiven()
        {
            var row = this.Row();

            Assert.That((row.Name, row.ColorHex, row.Minimum, row.Maximum, row.Step), Is.EqualTo(("Gold", "#FFD700", 0L, 5000L, 100L)));
            Assert.That(row.Value, Is.EqualTo(1000));
        }

        [TestCase("Gold", "Go")]
        [TestCase("Ore", "Or")]
        [TestCase("G", "G")]
        [TestCase("", "")]
        public void TheBadgeIsTheFirstTwoLettersOfTheName(string name, string badge)
        {
            Assert.That(this.Row(name).Badge, Is.EqualTo(badge));
        }

        [Test]
        public void AValueWithinTheLimitsIsWrittenAsItIs()
        {
            var row = this.Row();

            row.Value = 2500;

            Assert.That(this.written, Is.EqualTo(new[] { 2500L }));
        }

        [Test]
        public void AValueBeyondTheLimitsIsWrittenAsTheNearestLimit()
        {
            var row = this.Row(minimum: 10, maximum: 5000);

            row.Value = 99999;
            row.Value = -5;

            Assert.That(this.written, Is.EqualTo(new[] { 5000L, 10L }));
        }

        [Test]
        public void TheLimitsThemselvesAreAllowed()
        {
            var row = this.Row(minimum: 10, maximum: 5000);

            row.Value = 10;
            row.Value = 5000;

            Assert.That(this.written, Is.EqualTo(new[] { 10L, 5000L }));
        }

        [Test]
        public void TheRowIsChangedWhenTheValueIsNotTheOneInTheSavefile()
        {
            var row = this.Row();
            Assert.That(row.IsChanged, Is.False);

            row.Value = 1001;

            Assert.That(row.IsChanged, Is.True);
        }

        [Test]
        public void TheDeltaIsEmptyWhenNothingChangedAndSaysHowMuchMoreOrLess()
        {
            var row = this.Row();
            Assert.That(row.DeltaText, Is.Empty);

            row.Value = 3500;
            Assert.That(row.DeltaText, Is.EqualTo($"+{2500.ToString("N0", CultureInfo.CurrentCulture)} from save"));

            row.Value = 400;
            Assert.That(row.DeltaText, Is.EqualTo($"{(-600).ToString("N0", CultureInfo.CurrentCulture)} from save"));
        }

        [Test]
        public void TheNoteIsWhatTheNoteFunctionSaysAndEmptyWhenThereIsNone()
        {
            var noted = this.Row(note: () => "limit reached");

            Assert.That(noted.Note, Is.EqualTo("limit reached"));
            Assert.That(this.Row().Note, Is.Empty);
        }

        [Test]
        public void RefreshTellsEveryBindingToReadAgain()
        {
            var row = this.Row();
            var raised = new List<string?>();
            row.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            row.Refresh();

            Assert.That(raised, Is.EqualTo(new[] { string.Empty }));
        }

        [Test]
        public void TheValueIsReadFromTheSourceEachTime()
        {
            var row = this.Row();

            this.stored = 4321;

            Assert.That(row.Value, Is.EqualTo(4321));
        }
    }
}
