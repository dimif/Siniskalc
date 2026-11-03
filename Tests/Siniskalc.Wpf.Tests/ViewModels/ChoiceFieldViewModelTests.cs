namespace Siniskalc.Wpf.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests a drop-down list of a page: the choice it shows, the choices it offers, and what it does when a box is cleared.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ChoiceFieldViewModelTests
    {
        private static readonly string[] Owners = ["Neutral", "Red", "Blue"];

        private string stored = "Red";
        private List<string> written = null!;

        [SetUp]
        public void StartWithRed()
        {
            this.stored = "Red";
            this.written = [];
        }

        private ChoiceFieldViewModel Field(Func<bool>? isEnabled = null, string toolTip = "") =>
            new("Owner", Owners, () => this.stored, v =>
            {
                this.written.Add(v);
                this.stored = v;
            }, isEnabled, toolTip);

        [Test]
        public void AFieldShowsWhatItWasGivenAndTheChoiceItHolds()
        {
            var field = this.Field(toolTip: "The owner");

            Assert.That((field.Label, field.ToolTip, field.Selected), Is.EqualTo(("Owner", "The owner", "Red")));
            Assert.That(field.Choices, Is.SameAs(Owners));
        }

        [Test]
        public void PickingAnotherChoiceWritesIt()
        {
            var field = this.Field();

            field.Selected = "Blue";

            Assert.That(this.written, Is.EqualTo(new[] { "Blue" }));
        }

        [Test]
        public void PickingTheChoiceItAlreadyHasWritesNothing()
        {
            var field = this.Field();

            field.Selected = "Red";

            Assert.That(this.written, Is.Empty);
        }

        [TestCase(null)]
        [TestCase("")]
        public void ABoxThatIsClearedWritesNothingSoTheStoredChoiceIsKept(string? value)
        {
            var field = this.Field();

            field.Selected = value!;

            Assert.That(this.written, Is.Empty);
            Assert.That(field.Selected, Is.EqualTo("Red"));
        }

        [Test]
        public void TheFieldIsEnabledUnlessTheConditionSaysNo()
        {
            var enabled = true;
            var field = this.Field(() => enabled);
            Assert.That(field.IsEnabled, Is.True);

            enabled = false;

            Assert.That(field.IsEnabled, Is.False);
            Assert.That(this.Field().IsEnabled, Is.True, "no condition means yes");
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
