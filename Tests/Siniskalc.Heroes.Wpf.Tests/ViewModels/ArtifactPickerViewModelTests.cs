namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Heroes.Wpf.ViewModels;

    /// <summary>
    /// Tests the button of a slot that shows an artifact and opens a list to pick another or to clear it.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class ArtifactPickerViewModelTests
    {
        private List<string?> picked = null!;
        private Action<string>? choose;
        private int finders;

        [SetUp]
        public void StartWithNothingPicked()
        {
            this.picked = [];
            this.choose = null;
            this.finders = 0;
        }

        private ArtifactPickerViewModel Picker() =>
            new("Helm", onPicked =>
            {
                this.finders++;
                this.choose = onPicked;
                return new TestFinder();
            }, this.picked.Add);

        [Test]
        public void ANewPickerHoldsNoArtifactAndSaysEmpty()
        {
            var picker = this.Picker();

            Assert.That((picker.Label, picker.ArtifactName, picker.HasArtifact, picker.DisplayText, picker.IsOpen), Is.EqualTo(("Helm", (string?)null, false, "(empty)", false)));
        }

        [Test]
        public void UpdatingFromTheModelShowsTheArtifactAndTellsTheBindingsOnce()
        {
            var picker = this.Picker();
            var raised = new List<string?>();
            picker.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            picker.UpdateFromModel("Skull Helmet");
            picker.UpdateFromModel("Skull Helmet");

            Assert.That((picker.ArtifactName, picker.HasArtifact, picker.DisplayText), Is.EqualTo(("Skull Helmet", true, "Skull Helmet")));
            Assert.That(raised, Is.EqualTo(new[] { nameof(ArtifactPickerViewModel.ArtifactName), nameof(ArtifactPickerViewModel.HasArtifact), nameof(ArtifactPickerViewModel.DisplayText) }));
        }

        [Test]
        public void UpdatingFromTheModelWithNothingSaysEmptyAgain()
        {
            var picker = this.Picker();
            picker.UpdateFromModel("Skull Helmet");

            picker.UpdateFromModel(null);

            Assert.That((picker.HasArtifact, picker.DisplayText), Is.EqualTo((false, "(empty)")));
        }

        [Test]
        public void AnArtifactWithAnEmptyNameCountsAsNone()
        {
            var picker = this.Picker();

            picker.UpdateFromModel(string.Empty);

            Assert.That((picker.HasArtifact, picker.DisplayText), Is.EqualTo((false, "(empty)")));
        }

        [Test]
        public void TheButtonOpensAndClosesTheListAndTellsOnlyWhenItChanges()
        {
            var picker = this.Picker();
            var raised = new List<string?>();
            picker.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            picker.OpenCommand.Execute(null);
            Assert.That(picker.IsOpen, Is.True);
            picker.OpenCommand.Execute(null);
            Assert.That(picker.IsOpen, Is.False);
            picker.IsOpen = false;

            Assert.That(raised, Is.EqualTo(new[] { nameof(ArtifactPickerViewModel.IsOpen), nameof(ArtifactPickerViewModel.IsOpen) }));
        }

        [Test]
        public void TheListIsMadeOnlyWhenItIsFirstAskedFor()
        {
            var picker = this.Picker();
            Assert.That(this.finders, Is.Zero);

            _ = picker.Finder;
            _ = picker.Finder;

            Assert.That(this.finders, Is.EqualTo(1));
        }

        [Test]
        public void PickingFromTheListClosesItAndTellsThePageTheArtifact()
        {
            var picker = this.Picker();
            _ = picker.Finder;
            picker.IsOpen = true;

            this.choose!("Thunder Helmet");

            Assert.That(picker.IsOpen, Is.False);
            Assert.That(this.picked, Is.EqualTo(new[] { "Thunder Helmet" }));
        }

        [Test]
        public void ClearIsOfferedOnlyWithAnArtifactAndTellsThePageNothingAndClosesTheList()
        {
            var picker = this.Picker();
            Assert.That(picker.ClearCommand.CanExecute(null), Is.False);

            picker.UpdateFromModel("Skull Helmet");
            picker.IsOpen = true;
            Assert.That(picker.ClearCommand.CanExecute(null), Is.True);

            picker.ClearCommand.Execute(null);

            Assert.That(this.picked, Is.EqualTo(new string?[] { null }));
            Assert.That(picker.IsOpen, Is.False);
        }

        [Test]
        public void PickingDoesNotChangeTheShownArtifactUntilTheModelSaysSo()
        {
            var picker = this.Picker();
            _ = picker.Finder;

            this.choose!("Thunder Helmet");

            Assert.That(picker.HasArtifact, Is.False);
        }
    }
}
