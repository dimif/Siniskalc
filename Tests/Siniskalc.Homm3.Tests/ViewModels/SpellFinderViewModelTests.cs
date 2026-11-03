using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.ViewModels
{
    /// <summary>
    /// Tests the list of spells of a hero: what a search matches, the summary of what is learned and available, and the columns.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SpellFinderViewModelTests
    {
        private readonly List<(string Name, bool Learned)> changes = [];

        [SetUp]
        public void ForgetEarlierChanges()
        {
            this.changes.Clear();
        }

        private SpellRowViewModel Row(Spell spell) => new(spell, (row, learned) => this.changes.Add((row.Name, learned)));

        private SpellFinderViewModel Finder(params SpellRowViewModel[] rows) => new(rows);

        private static IEnumerable<string> Shown(SpellFinderViewModel finder) => finder.Items.Cast<SpellRowViewModel>().Select(r => r.Name);

        [Test]
        public void AllRowsAreShownAtFirstAndTheFinderAllowsSeveralToBeSelected()
        {
            var finder = this.Finder(this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste")), this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("MagicArrow")), this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Bless")));

            Assert.That(Shown(finder), Is.EquivalentTo(new[] { "Haste", "Magic Arrow", "Bless" }));
            Assert.That(finder.AllowsMultiSelect, Is.True);
        }

        [Test]
        public void ASearchMatchesTheNameTheSchoolTheLevelAndTheType()
        {
            var finder = this.Finder(this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste")), this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("MagicArrow")), this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Visions")));

            finder.Filter = "haste";
            Assert.That(Shown(finder), Is.EqualTo(new[] { "Haste" }));

            finder.Filter = "air";
            Assert.That(Shown(finder), Does.Contain("Haste").And.Not.Contain("Magic Arrow"));

            finder.Filter = "level 2";
            Assert.That(Shown(finder), Is.EqualTo(new[] { "Visions" }));

            finder.Filter = "adventure";
            Assert.That(Shown(finder), Is.EqualTo(new[] { "Visions" }));

            finder.Filter = "combat";
            Assert.That(Shown(finder), Does.Contain("Haste").And.Contain("Magic Arrow").And.Not.Contain("Visions"));
        }

        [Test]
        public void EveryWordOfTheSearchMustMatchSomething()
        {
            var finder = this.Finder(this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste")), this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("MagicArrow")));

            finder.Filter = "combat level 1 air";

            Assert.That(Shown(finder), Is.EqualTo(new[] { "Haste" }));
        }

        [Test]
        public void TheSummaryCountsWhatIsLearnedAndAvailableAndTellsHowManyAreShownWhenSearching()
        {
            var haste = this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste"));
            var arrow = this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("MagicArrow"));
            var bless = this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Bless"));
            haste.Show(true, false);
            arrow.Show(false, true, "Ring");
            bless.Show(true, true);
            var visions = this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Visions"));
            visions.Show(true, false);
            var finder = this.Finder(haste, arrow, bless, visions);

            Assert.That(finder.Summary, Is.EqualTo("3 of 4 learned, 2 available from artifacts"));

            finder.Filter = "haste";
            Assert.That(finder.Summary, Is.EqualTo("3 of 4 learned, 2 available from artifacts, showing 1"));
        }

        [Test]
        public void TheColumnsListTheSpellItsLevelTypeLearnedAvailableAndSource()
        {
            var finder = this.Finder(this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste")));

            Assert.That(finder.Columns.Select(c => c.Header), Is.EqualTo(new[] { "Spell", "Level", "Type", "Learned", "Available", "Source" }));
            Assert.That(finder.Columns.Single(c => c.Header == "Learned").Kind, Is.EqualTo(Siniskalc.Wpf.ViewModels.FinderColumnKind.CheckBox));
            Assert.That(finder.Columns.Single(c => c.Header == "Available").Kind, Is.EqualTo(Siniskalc.Wpf.ViewModels.FinderColumnKind.ReadOnlyCheckBox));
        }

        [Test]
        public void ARowReportsTheSchoolAndLevelOfItsSpellAndATickToTheCaller()
        {
            var row = this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste"));

            row.IsLearned = true;

            Assert.That((row.School, row.Level, row.Kind, row.Spell), Is.EqualTo(("Air magic", 1, "Combat", TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.ById("Haste"))));
            Assert.That(this.changes, Is.EqualTo(new[] { ("Haste", true) }));
        }

        [TestCase("Bless", "Water magic")]
        [TestCase("Magic Arrow", "All schools")]
        public void TheSchoolOfARowIsThatOfItsSpell(string name, string school)
        {
            Assert.That(this.Row(TestCatalog.For(GameVersion.HornOfTheAbyss).Spells.All.Single(s => s.Name == name)).School, Is.EqualTo(school));
        }
    }
}
