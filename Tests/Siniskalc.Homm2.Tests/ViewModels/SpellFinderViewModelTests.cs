namespace Siniskalc.Homm2.Tests.ViewModels
{
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests the list of spells of a hero of HoMM2: what a search matches, the summary of what is learned and the columns.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SpellFinderViewModelTests
    {
        private static SpellRowViewModel Row(Spell spell, bool learned = false)
        {
            var row = new SpellRowViewModel(spell, (_, _) => { });
            row.Show(learned, false);
            return row;
        }

        private static IEnumerable<string> Shown(SpellFinderViewModel finder) => finder.Items.Cast<SpellRowViewModel>().Select(r => r.Name);

        [Test]
        public void AllRowsAreShownAtFirstAndTheFinderAllowsSeveralToBeSelected()
        {
            var finder = new SpellFinderViewModel([Row(TestCatalog.Standard.Spells["Fireball"]), Row(TestCatalog.Standard.Spells["Cure"]), Row(TestCatalog.Standard.Spells["Teleport"])]);

            Assert.That(Shown(finder), Is.EquivalentTo(new[] { "Fireball", "Cure", "Teleport" }));
            Assert.That(finder.AllowsMultiSelect, Is.True);
            Assert.That(finder.Footer, Does.StartWith("Shift-click or Ctrl-click"));
        }

        [Test]
        public void ASearchMatchesTheNameTheLevelTheTypeAndTheMagic()
        {
            var finder = new SpellFinderViewModel([Row(TestCatalog.Standard.Spells["Fireball"]), Row(TestCatalog.Standard.Spells["Cure"]), Row(TestCatalog.Standard.Spells["Teleport"])]);

            finder.Filter = "fire";
            Assert.That(Shown(finder), Is.EqualTo(new[] { "Fireball" }));

            finder.Filter = "level 1";
            Assert.That(Shown(finder), Is.EqualTo(new[] { "Cure" }));

            finder.Filter = "elemental";
            Assert.That(Shown(finder), Is.EqualTo(new[] { "Fireball" }));

            finder.Filter = "combat";
            Assert.That(Shown(finder), Is.EquivalentTo(new[] { "Fireball", "Cure", "Teleport" }));
        }

        [Test]
        public void EveryWordOfTheSearchMustMatchSomething()
        {
            var finder = new SpellFinderViewModel([Row(TestCatalog.Standard.Spells["Fireball"]), Row(TestCatalog.Standard.Spells["Cure"]), Row(TestCatalog.Standard.Spells["Teleport"])]);

            finder.Filter = "combat level 3";

            Assert.That(Shown(finder), Is.EquivalentTo(new[] { "Fireball", "Teleport" }));
        }

        [Test]
        public void TheSummaryCountsWhatIsLearnedAndTellsHowManyAreShownWhenSearching()
        {
            var finder = new SpellFinderViewModel([Row(TestCatalog.Standard.Spells["Fireball"], true), Row(TestCatalog.Standard.Spells["Cure"]), Row(TestCatalog.Standard.Spells["Teleport"], true)]);

            Assert.That(finder.Summary, Is.EqualTo("2 of 3 learned"));

            finder.Filter = "cure";
            Assert.That(finder.Summary, Is.EqualTo("2 of 3 learned, showing 1"));
        }

        [Test]
        public void TheColumnsListTheSpellItsLevelTypeMagicCostAndWhetherItIsLearned()
        {
            var finder = new SpellFinderViewModel([Row(TestCatalog.Standard.Spells["Fireball"])]);

            Assert.That(finder.Columns.Select(c => c.Header), Is.EqualTo(new[] { "Spell", "Level", "Type", "Magic", "Cost", "Learned" }));
            Assert.That(finder.Columns.Single(c => c.Header == "Learned").Kind, Is.EqualTo(FinderColumnKind.CheckBox));
        }
    }
}
