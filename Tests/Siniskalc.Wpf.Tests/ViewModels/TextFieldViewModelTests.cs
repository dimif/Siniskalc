namespace Siniskalc.Wpf.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests a text box of a page: the text it reads and writes, its limit and the counter that goes with it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class TextFieldViewModelTests
    {
        private string stored = "Orrin";
        private List<string> written = null!;

        [SetUp]
        public void StartWithOrrin()
        {
            this.stored = "Orrin";
            this.written = [];
        }

        private TextFieldViewModel Field(Func<int>? maxLength = null, bool multiline = false, string toolTip = "") =>
            new("Name", () => this.stored, v =>
            {
                this.written.Add(v);
                this.stored = v;
            }, maxLength, multiline, toolTip);

        [Test]
        public void AFieldShowsWhatItWasGiven()
        {
            var field = this.Field(multiline: true, toolTip: "The name");

            Assert.That((field.Label, field.IsMultiline, field.ToolTip, field.Text), Is.EqualTo(("Name", true, "The name", "Orrin")));
        }

        [Test]
        public void ANewTextIsWritten()
        {
            var field = this.Field();

            field.Text = "Ufretin";

            Assert.That(this.written, Is.EqualTo(new[] { "Ufretin" }));
        }

        [Test]
        public void TheTextItAlreadyHasIsNotWrittenAgain()
        {
            var field = this.Field();

            field.Text = "Orrin";

            Assert.That(this.written, Is.Empty);
        }

        [Test]
        public void NoTextIsWrittenAsAnEmptyString()
        {
            var field = this.Field();

            field.Text = null!;

            Assert.That(this.written, Is.EqualTo(new[] { string.Empty }));
        }

        [Test]
        public void NoTextOnAFieldThatIsAlreadyEmptyWritesNothing()
        {
            this.stored = string.Empty;
            var field = this.Field();

            field.Text = null!;

            Assert.That(this.written, Is.Empty);
        }

        [Test]
        public void WithNoLimitTheMaximumAndTheCounterAreEmpty()
        {
            var field = this.Field();

            Assert.That((field.MaxLength, field.Counter), Is.EqualTo((0, string.Empty)));
        }

        [Test]
        public void WithALimitTheCounterTellsHowMuchOfItIsUsed()
        {
            var field = this.Field(() => 12);

            Assert.That((field.MaxLength, field.Counter), Is.EqualTo((12, "5 of 12")));

            field.Text = "Ufretin";

            Assert.That(field.Counter, Is.EqualTo("7 of 12"));
        }

        [Test]
        public void ALimitOfZeroMeansNoLimit()
        {
            var field = this.Field(() => 0);

            Assert.That(field.Counter, Is.Empty);
        }

        [Test]
        public void TheLimitIsAskedAgainEachTimeSoItCanChange()
        {
            var limit = 10;
            var field = this.Field(() => limit);

            limit = 20;

            Assert.That(field.MaxLength, Is.EqualTo(20));
        }

        [Test]
        public void RefreshTellsEveryBindingToReadAgain()
        {
            var field = this.Field();
            var raised = new List<string?>();
            field.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            field.Refresh();

            Assert.That(raised, Is.EqualTo(new[] { string.Empty }));
        }
    }
}
