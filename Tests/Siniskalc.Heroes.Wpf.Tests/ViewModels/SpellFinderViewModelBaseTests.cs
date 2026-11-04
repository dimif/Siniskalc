namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests what every game's spell finder does: it allows several rows to be selected, matches the search text against the name, the
    /// group, the level and the type, and counts what is learned. The game's part is a made-up finder.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SpellFinderViewModelBaseTests
    {
        private sealed class Finder : SpellFinderViewModelBase<SpellChoiceViewModel>
        {
            public Finder(IReadOnlyList<SpellChoiceViewModel> rows)
                : base(rows, null, [SpellColumn, LevelColumn, TypeColumn, LearnedColumn])
            {
                this.StartFiltering();
            }

            public override string SearchPlaceholder => "placeholder";

            public override string SearchHint => "hint";

            public int Available { get; set; }

            protected override string SummaryDetail => this.Available == 0 ? string.Empty : $", {this.Available} extra";
        }

        private static SpellChoiceViewModel Row(string name, string group, int level, string kind, bool learned = false)
        {
            var row = new SpellChoiceViewModel(name, group, level, kind, (_, _) => { });
            row.Show(learned, false);
            return row;
        }

        private static Finder Sample(bool learnedFireball = false, bool learnedCure = false)
        {
            return new Finder([Row("Fireball", "Fire", 3, "Combat", learnedFireball), Row("Cure", "Earth", 1, "Combat", learnedCure), Row("Visions", "Air", 2, "Adventure")]);
        }

        private static IEnumerable<string> Shown(Finder finder) => finder.Items.Cast<SpellChoiceViewModel>().Select(r => r.Name);

        [Test]
        public void AllRowsAreShownAtFirstAndSeveralCanBeSelectedWithTheFooterSayingHow()
        {
            var finder = Sample();

            Assert.That(Shown(finder), Is.EquivalentTo(new[] { "Fireball", "Cure", "Visions" }));
            Assert.That(finder.AllowsMultiSelect, Is.True);
            Assert.That(finder.Footer, Does.StartWith("Shift-click or Ctrl-click"));
        }

        [Test]
        public void ASearchMatchesTheNameTheGroupTheLevelAndTheType()
        {
            var finder = Sample();

            finder.Filter = "fire";
            Assert.That(Shown(finder), Is.EqualTo(new[] { "Fireball" }));

            finder.Filter = "earth";
            Assert.That(Shown(finder), Is.EqualTo(new[] { "Cure" }));

            finder.Filter = "level 2";
            Assert.That(Shown(finder), Is.EqualTo(new[] { "Visions" }));

            finder.Filter = "adventure";
            Assert.That(Shown(finder), Is.EqualTo(new[] { "Visions" }));
        }

        [Test]
        public void EveryWordOfTheSearchMustMatchSomething()
        {
            var finder = Sample();

            finder.Filter = "combat level 3";

            Assert.That(Shown(finder), Is.EqualTo(new[] { "Fireball" }));
        }

        [Test]
        public void TheSummaryCountsWhatIsLearnedAndTellsHowManyAreShownWhenSearching()
        {
            var finder = Sample(learnedFireball: true, learnedCure: true);

            Assert.That(finder.Summary, Is.EqualTo("2 of 3 learned"));

            finder.Filter = "cure";
            Assert.That(finder.Summary, Is.EqualTo("2 of 3 learned, showing 1"));
        }

        [Test]
        public void ASummaryDetailOfTheGameComesBetweenWhatIsLearnedAndWhatIsShown()
        {
            var finder = Sample();
            finder.Available = 2;
            finder.Filter = "cure";

            Assert.That(finder.Summary, Is.EqualTo("0 of 3 learned, 2 extra, showing 1"));
        }

        [Test]
        public void TheCommonColumnsAreTheSpellTheLevelTheTypeAndTheLearnedCheckBox()
        {
            var finder = Sample();

            Assert.That(finder.Columns.Select(c => c.Header), Is.EqualTo(new[] { "Spell", "Level", "Type", "Learned" }));
            Assert.That(finder.Columns.Single(c => c.Header == "Learned").Kind, Is.EqualTo(FinderColumnKind.CheckBox));
        }
    }
}
