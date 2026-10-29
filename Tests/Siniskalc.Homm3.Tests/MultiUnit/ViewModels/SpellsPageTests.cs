using Siniskalc.Heroes.Models;
using Siniskalc.Heroes.Wpf.ViewModels;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Tests.Models;
using Siniskalc.Homm3.Tests.ViewModels;
using Siniskalc.Homm3.ViewModels;

namespace Siniskalc.Homm3.Tests.MultiUnit.ViewModels
{
    /// <summary>
    /// The spell book of a hero on its page: the list, the filters, learning and the spell book box, over a hero session with no
    /// savefile and the real resolver of the spells artifacts grant. The hero has a spell book and knows no spell.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.MultiUnit)]
    internal sealed class SpellsPageTests
    {
        private FakeHeroSession session = null!;
        private HeroViewModel page = null!;

        private SpellsViewModel Spells => this.page.SpellsViewModel;

        private List<SpellRowViewModel> Rows => this.Spells.Finder.Items.Cast<SpellRowViewModel>().ToList();

        [SetUp]
        public void StartWithAnEmptySpellBook()
        {
            this.session = new FakeHeroSession(HeroModels.Christian(new GrantedSpellsResolver(GameVersion.HornOfTheAbyss)));
            this.page = HeroPages.Create(this.session);
        }

        [TearDown]
        public void ClosePage()
        {
            this.page.Dispose();
        }

        private IEnumerable<Spell> SpellsOfTheVersion => Spell.All.Where(s => s.IsAvailableIn(this.session.GameVersion));

        [Test]
        public void ASpellAnArtifactGrantsNamesTheArtifactAsItsSource()
        {
            this.session.Update(h => h with { EquippedArtifacts = h.EquippedArtifacts.WithEquipped(ArtifactSlots.Helmet, Artifact.SeaCaptainsHat) });

            var row = this.Rows.Single(r => r.Spell == Spell.SummonBoat);

            Assert.That(row.IsAvailable, Is.True);
            Assert.That(row.Source, Is.EqualTo("Sea Captain's Hat"));
            Assert.That(this.Rows.Where(r => !r.IsAvailable).Select(r => r.Source), Is.All.Empty);
            Assert.That(this.Rows.Where(r => r.Source == "Unknown source").Select(r => r.Name), Is.Empty);
        }

        [Test]
        public void TickingOneSelectedSpellChangesAllSelectedSpells()
        {
            var rows = this.Rows;
            var picked = new[] { Spell.Bless, Spell.Cure, Spell.Fireball }.Select(s => rows.Single(r => r.Spell == s)).ToList();
            var bystander = rows.First(r => !picked.Contains(r));
            foreach (var row in picked)
            {
                row.IsSelected = true;
            }

            picked[0].IsLearned = true;

            Assert.That(picked.Select(r => this.session.Current.SpellBook.IsLearned(r.Spell)), Is.All.True);
            Assert.That(picked.Select(r => r.IsLearned), Is.All.True);
            Assert.That(this.session.Current.SpellBook.IsLearned(bystander.Spell), Is.False);

            picked[2].IsLearned = false;

            Assert.That(picked.Select(r => this.session.Current.SpellBook.IsLearned(r.Spell)), Is.All.False);
        }

        [Test]
        public void TickingAnUnselectedSpellChangesOnlyThatSpell()
        {
            var rows = this.Rows;
            var selected = rows[0];
            var other = rows[1];
            selected.IsSelected = true;

            other.IsLearned = true;

            Assert.That(this.session.Current.SpellBook.IsLearned(other.Spell), Is.True);
            Assert.That(this.session.Current.SpellBook.IsLearned(selected.Spell), Is.False);
        }

        [Test]
        public void TheListHasEverySpellOfTheVersionWithItsLearnedAndAvailableFlags()
        {
            this.session.Update(h => h with { SpellBook = h.SpellBook.WithLearned(Spell.Bless, true).WithOtherAvailable([Spell.Cure]) });
            var book = this.session.Current.SpellBook;

            var rows = this.Rows;

            Assert.That(rows.Select(r => r.Spell), Is.EquivalentTo(this.SpellsOfTheVersion));
            foreach (var row in rows)
            {
                Assert.That(row.IsLearned, Is.EqualTo(book.IsLearned(row.Spell)), row.Name);
                Assert.That(row.IsAvailable, Is.EqualTo(book.IsAvailable(row.Spell)), row.Name);
            }

            Assert.That(rows.Single(r => r.Spell == Spell.Bless).IsLearned, Is.True);
            Assert.That(rows.Single(r => r.Spell == Spell.Cure).IsAvailable, Is.True);
            Assert.That(this.Spells.Finder.Items.Groups, Is.Not.Empty);
        }

        [Test]
        public void LearningASpellWritesTheSpellBookAndPuttingTheHeroBackShowsItAgain()
        {
            var row = this.Rows.First();

            row.IsLearned = true;
            Assert.That(this.session.Current.SpellBook.IsLearned(row.Spell), Is.True);

            this.session.Update(_ => this.session.Original);

            Assert.That(row.IsLearned, Is.False);
            Assert.That(this.session.Current.SpellBook.IsLearned(row.Spell), Is.False);
        }

        [Test]
        public void AvailableSpellsCannotBeEditedThroughTheRow()
        {
            var property = typeof(SpellRowViewModel).GetProperty(nameof(SpellRowViewModel.IsAvailable));

            Assert.That(property?.CanWrite, Is.False);
        }

        [Test]
        public void ANameFilterNarrowsTheListAndLearnAllOnlyTouchesWhatIsShown()
        {
            this.Spells.Finder.Filter = "magic arrow";
            Assert.That(this.Rows.Select(r => r.Name), Is.EqualTo(new[] { "Magic Arrow" }));

            this.Spells.LearnAllCommand.Execute(null);

            var learned = this.SpellsOfTheVersion.Where(s => this.session.Current.SpellBook.IsLearned(s)).ToList();
            Assert.That(learned, Is.EqualTo(new[] { Spell.MagicArrow }));
        }

        [Test]
        public void ForgetAllOnlyForgetsWhatIsShown()
        {
            this.session.Update(h => h with { SpellBook = h.SpellBook.WithLearned(Spell.MagicArrow, true).WithLearned(Spell.Bless, true) });
            this.Spells.Finder.Filter = "magic arrow";

            this.Spells.ForgetAllCommand.Execute(null);

            Assert.That(this.session.Current.SpellBook.IsLearned(Spell.MagicArrow), Is.False);
            Assert.That(this.session.Current.SpellBook.IsLearned(Spell.Bless), Is.True);
        }

        [Test]
        public void ALevelFilterShowsOneLevelAndCombinesWithTheNameFilter()
        {
            this.Spells.Finder.Filter = "level 3";

            var shown = this.Rows;
            Assert.That(shown, Is.Not.Empty);
            Assert.That(shown.Select(r => r.Level), Is.All.EqualTo(3));
            Assert.That(shown.Count, Is.EqualTo(this.SpellsOfTheVersion.Count(s => s.Level == 3)));
            Assert.That(this.Spells.Finder.Summary, Does.Contain($"showing {shown.Count}"));

            this.Spells.Finder.Filter = "level 3 fire";
            Assert.That(this.Rows.Select(r => r.Name), Is.EquivalentTo(shown.Where(r => r.School == "Fire magic").Select(r => r.Name)));
            Assert.That(this.Rows, Is.Not.Empty);

            this.Spells.Finder.Filter = string.Empty;
            Assert.That(this.Rows.Count, Is.EqualTo(this.SpellsOfTheVersion.Count()));
        }

        [Test]
        public void LearnAllWithALevelFilterOnlyLearnsThatLevelAndForgetAllOnlyForgetsIt()
        {
            var spells = this.SpellsOfTheVersion.ToList();
            this.session.Update(h => h with { SpellBook = h.SpellBook.WithLearned(Spell.MagicArrow, true) });

            this.Spells.Finder.Filter = "level 5";
            this.Spells.LearnAllCommand.Execute(null);

            foreach (var spell in spells)
            {
                Assert.That(this.session.Current.SpellBook.IsLearned(spell), Is.EqualTo(spell.Level == 5 || spell == Spell.MagicArrow), spell.Name);
            }

            this.Spells.ForgetAllCommand.Execute(null);

            foreach (var spell in spells)
            {
                Assert.That(this.session.Current.SpellBook.IsLearned(spell), Is.EqualTo(spell == Spell.MagicArrow), spell.Name);
            }
        }

        [Test]
        public void LearnAllAndForgetAllAreOnlyOfferedToAHeroWithASpellBook()
        {
            this.Spells.HasSpellBook = false;

            Assert.That(this.Spells.LearnAllCommand.CanExecute(null), Is.False);
            Assert.That(this.Spells.ForgetAllCommand.CanExecute(null), Is.False);

            this.Spells.HasSpellBook = true;

            Assert.That(this.Spells.LearnAllCommand.CanExecute(null), Is.True);
            Assert.That(this.Spells.ForgetAllCommand.CanExecute(null), Is.True);
        }

        [Test]
        public void TheSpellBookBoxWritesTheHeroAndFollowsPuttingTheHeroBack()
        {
            var before = this.Spells.HasSpellBook;

            this.Spells.HasSpellBook = !before;
            Assert.That(this.session.Current.SpellBook.HasSpellBook, Is.EqualTo(!before));

            this.session.Update(_ => this.session.Original);
            Assert.That(this.Spells.HasSpellBook, Is.EqualTo(before));
        }
    }
}
