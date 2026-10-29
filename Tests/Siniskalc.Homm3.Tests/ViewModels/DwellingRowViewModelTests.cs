using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.ViewModels
{
    /// <summary>
    /// Tests the row of one dwelling of a castle: what it shows, and the count of creatures waiting that can be edited while the dwelling
    /// is built.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class DwellingRowViewModelTests
    {
        [Test]
        public void ARowShowsWhatItWasGiven()
        {
            var row = new DwellingRowViewModel(3, "Griffin Tower", "Built", "Griffin", true, 12, _ => { }, "Griffin, Royal Griffin", 7);

            Assert.That((row.Level, row.Dwelling, row.State, row.Creatures, row.IsBuilt, row.Count, row.CreatureNames), Is.EqualTo((3, "Griffin Tower", "Built", "Griffin", true, 12L, "Griffin, Royal Griffin")));
            Assert.That(row.Maximum, Is.EqualTo(Constants.Castle.MaxDwellingCreatures));
            Assert.That(row.WeeklyGrowthText, Is.Not.Empty);
        }

        [Test]
        public void ARowWithoutACountStartsAtZeroAndNoGrowthHasNoGrowthText()
        {
            var row = new DwellingRowViewModel(0, "d", "s", "c", true);

            Assert.That(row.Count, Is.EqualTo(0));
            Assert.That(row.WeeklyGrowthText, Is.EqualTo(Siniskalc.Heroes.Wpf.ViewModels.WeeklyGrowthText.Of(0)));
        }

        [Test]
        public void TheCountCanBeEditedOnlyWhenTheDwellingIsBuiltAndThereIsAnActionToWriteIt()
        {
            Assert.That(new DwellingRowViewModel(0, "d", "s", "c", true, 1, _ => { }).CanEdit, Is.True);
            Assert.That(new DwellingRowViewModel(0, "d", "s", "c", false, 1, _ => { }).CanEdit, Is.False);
            Assert.That(new DwellingRowViewModel(0, "d", "s", "c", true, 1, null).CanEdit, Is.False);
        }

        [Test]
        public void SettingTheCountWritesItKeptWithinTheLimits()
        {
            var written = new List<int>();
            var row = new DwellingRowViewModel(0, "d", "s", "c", true, 5, written.Add);

            row.Count = 40;
            row.Count = -3;
            row.Count = Constants.Castle.MaxDwellingCreatures + 1000;

            Assert.That(written, Is.EqualTo(new[] { 40, (int)Constants.Castle.MinDwellingCreatures, (int)Constants.Castle.MaxDwellingCreatures }));
            Assert.That(row.Count, Is.EqualTo(Constants.Castle.MaxDwellingCreatures));
        }

        [Test]
        public void SettingTheCountOfARowWithNoActionOnlyChangesTheRow()
        {
            var row = new DwellingRowViewModel(0, "d", "s", "c", true, 5);

            Assert.DoesNotThrow(() => row.Count = 9);
            Assert.That(row.Count, Is.EqualTo(9));
        }
    }
}
