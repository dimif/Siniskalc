using Siniskalc.Heroes.Entities;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.MultiUnit.ViewModels
{
    /// <summary>
    /// Tests the slot of the backpack of a hero: the number it shows, the artifact it holds and the report of a change, on the real picker
    /// and finder.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class BackpackSlotViewModelTests
    {
        private readonly List<(int Slot, string? Name)> reported = [];

        [SetUp]
        public void ForgetEarlierReports()
        {
            this.reported.Clear();
        }

        private BackpackSlotViewModel Slot(int number = 3) =>
            new(number, (slot, name) => this.reported.Add((slot, name)), choose => new ArtifactFinderViewModel(GameVersion.ShadowOfDeath, TestServices.EffectLines, string.Empty, "Choose", choose, null, includeScrolls: false));

        [Test]
        public void ASlotKnowsItsNumberAndLabelsItself()
        {
            var slot = this.Slot(5);

            Assert.That((slot.SlotNumber, slot.SlotLabel, slot.Picker.Label), Is.EqualTo((5, "Slot 5", "Slot 5")));
        }

        [Test]
        public void SettingTheArtifactShowsItInThePickerAndReportsItOnce()
        {
            var slot = this.Slot();

            slot.SelectedArtifact = Artifact.SkullHelmet.Name;
            slot.SelectedArtifact = Artifact.SkullHelmet.Name;

            Assert.That(slot.Picker.ArtifactName, Is.EqualTo(Artifact.SkullHelmet.Name));
            Assert.That(this.reported, Is.EqualTo(new[] { (3, (string?)Artifact.SkullHelmet.Name) }));
        }

        [Test]
        public void PickingInTheFinderOfThePickerSetsTheSlot()
        {
            var slot = this.Slot();
            var choice = slot.Picker.Finder.Artifacts.Cast<ArtifactChoiceViewModel>().Single(c => c.Artifact == Artifact.OrbOfInhibition);

            slot.Picker.Finder.ChooseCommand.Execute(choice);

            Assert.That(slot.SelectedArtifact, Is.EqualTo(Artifact.OrbOfInhibition.Name));
            Assert.That(this.reported.Single(), Is.EqualTo((3, (string?)Artifact.OrbOfInhibition.Name)));
        }

        [Test]
        public void ClearingThePickerEmptiesTheSlotAndReportsIt()
        {
            var slot = this.Slot();
            slot.SelectedArtifact = Artifact.SkullHelmet.Name;
            this.reported.Clear();

            slot.Picker.ClearCommand.Execute(null);

            Assert.That(slot.SelectedArtifact, Is.Null);
            Assert.That(slot.Picker.HasArtifact, Is.False);
            Assert.That(this.reported, Is.EqualTo(new[] { (3, (string?)null) }));
        }
    }
}
