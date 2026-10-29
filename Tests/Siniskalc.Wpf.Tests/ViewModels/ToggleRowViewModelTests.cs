namespace Siniskalc.Wpf.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests a row with a check box that turns one thing on or off, and says whether it may and why not.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ToggleRowViewModelTests
    {
        private bool stored;
        private List<bool> written = null!;

        [SetUp]
        public void StartUnchecked()
        {
            this.stored = false;
            this.written = [];
        }

        private ToggleRowViewModel Row(Func<bool>? canToggle = null, Func<string>? note = null, string toolTip = "") =>
            new("Fort", () => this.stored, v =>
            {
                this.written.Add(v);
                this.stored = v;
            }, canToggle, note, toolTip);

        [Test]
        public void ARowShowsWhatItWasGiven()
        {
            var row = this.Row(toolTip: "Needed by Citadel");

            Assert.That((row.Name, row.ToolTip, row.IsChecked), Is.EqualTo(("Fort", "Needed by Citadel", false)));
        }

        [Test]
        public void ACheckThatChangesTheValueIsWritten()
        {
            var row = this.Row();

            row.IsChecked = true;

            Assert.That(this.written, Is.EqualTo(new[] { true }));
            Assert.That(row.IsChecked, Is.True);
        }

        [Test]
        public void ACheckThatLeavesTheValueAsItIsWritesNothing()
        {
            var row = this.Row();

            row.IsChecked = false;

            Assert.That(this.written, Is.Empty);
        }

        [Test]
        public void UncheckingWhatWasCheckedIsWritten()
        {
            this.stored = true;
            var row = this.Row();

            row.IsChecked = false;

            Assert.That(this.written, Is.EqualTo(new[] { false }));
        }

        [Test]
        public void TheRowMayBeToggledUnlessTheConditionSaysNo()
        {
            var allowed = true;
            var row = this.Row(canToggle: () => allowed);
            Assert.That(row.CanToggle, Is.True);

            allowed = false;

            Assert.That(row.CanToggle, Is.False);
            Assert.That(this.Row().CanToggle, Is.True, "no condition means yes");
        }

        [Test]
        public void TheNoteIsWhatTheNoteFunctionSaysAndEmptyWhenThereIsNone()
        {
            Assert.That(this.Row(note: () => "Not allowed by the map").Note, Is.EqualTo("Not allowed by the map"));
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
    }
}
