using Siniskalc.Data;
using Siniskalc.Models;
using Siniskalc;
using Siniskalc.Entities;
using Siniskalc.Runtime;
using Siniskalc.ViewModels;
using Siniskalc.Tests.Models;

namespace Siniskalc.Tests.ViewModels
{
    [TestFixture]
    internal sealed class SkillsAndSpellsViewModelTests
    {
        private Game game;
        private HeroViewModel hero;

        private SecondarySkillsViewModel Skills => this.hero.SecondarySkillsViewModel;

        private SpellsViewModel Spells => this.hero.SpellsViewModel;

        [SetUp]
        public void SetUp()
        {
            // Christian has Archery (basic), Wisdom (expert) and Luck (advanced), in that order.
            this.game = HeroEditingTests.LoadPrepared();
            this.hero = TestServices.CreateHeroViewModel(this.game.History.CreateHeroSession(this.game.History.Current.Heroes().FindByName("Christian")));
        }

        [TearDown]
        public void TearDown()
        {
            this.hero.Dispose();
        }

        [Test]
        public void ASpellAnArtifactGrantsNamesTheArtifactAsItsSource()
        {
            this.hero.Session.Update(h => h with { EquippedArtifacts = h.EquippedArtifacts.WithEquipped(ArtifactSlots.Helmet, Artifact.SeaCaptainsHat) });

            var row = this.Spells.Finder.Items.Cast<SpellRowViewModel>().Single(r => r.Spell == Spell.SummonBoat);

            Assert.That(row.IsAvailable, Is.True);
            Assert.That(row.Source, Is.EqualTo("Sea Captain's Hat"));
            var rows = this.Spells.Finder.Items.Cast<SpellRowViewModel>().ToList();
            Assert.That(rows.Where(r => !r.IsAvailable).Select(r => r.Source), Is.All.Empty);
            TestContext.Out.WriteLine("Unknown source: " + string.Join(", ", rows.Where(r => r.Source == "Unknown source").Select(r => r.Name)));
        }

        [Test]
        public void TickingOneSelectedSpellChangesAllSelectedSpells()
        {
            var rows = this.Spells.Finder.Items.Cast<SpellRowViewModel>().ToList();
            var picked = new[] { Spell.Bless, Spell.Cure, Spell.Fireball }.Select(s => rows.Single(r => r.Spell == s)).ToList();
            var bystander = rows.First(r => !picked.Contains(r) && !r.IsLearned && !r.IsAvailable);
            foreach (var row in picked)
            {
                row.IsLearned = false;
                row.IsSelected = true;
            }

            picked[0].IsLearned = true;

            Assert.That(picked.Select(r => this.hero.HeroModel.SpellBook.IsLearned(r.Spell)), Is.All.True);
            Assert.That(picked.Select(r => r.IsLearned), Is.All.True);
            Assert.That(this.hero.HeroModel.SpellBook.IsLearned(bystander.Spell), Is.False);

            picked[2].IsLearned = false;

            Assert.That(picked.Select(r => this.hero.HeroModel.SpellBook.IsLearned(r.Spell)), Is.All.False);
        }

        [Test]
        public void TickingAnUnselectedSpellChangesOnlyThatSpell()
        {
            var rows = this.Spells.Finder.Items.Cast<SpellRowViewModel>().ToList();
            var selected = rows.First(r => !r.IsLearned);
            var other = rows.First(r => !r.IsLearned && r != selected);
            selected.IsSelected = true;

            other.IsLearned = true;

            Assert.That(this.hero.HeroModel.SpellBook.IsLearned(other.Spell), Is.True);
            Assert.That(this.hero.HeroModel.SpellBook.IsLearned(selected.Spell), Is.False);
        }

        [Test]
        public void SlotsShowTheSkillsInOrderAndFreeSlotsAreEmpty()
        {
            var slots = this.Skills.Slots;

            Assert.That(slots, Has.Count.EqualTo(Constants.SkillSlots.Max));
            Assert.That(slots.Take(4).Select(s => s.SkillName), Is.EqualTo(new[] { "Archery", "Wisdom", "Luck", null }));
            Assert.That(slots[1].LevelName, Is.EqualTo(SkillLevelNames.Expert));
            Assert.That(slots[2].LevelName, Is.EqualTo(SkillLevelNames.Advanced));
            Assert.That(slots[1].Pips, Is.EqualTo(new[] { true, true, true }));
            Assert.That(slots[3].IsFilled, Is.False);
            Assert.That(this.Skills.SlotSummary, Is.EqualTo("3 of 8 slots used"));
        }

        [Test]
        public void MovingASkillSwapsItWithItsNeighbour()
        {
            var slots = this.Skills.Slots;

            Assert.That(slots[0].MoveUpCommand.CanExecute(null), Is.False);
            Assert.That(slots[2].MoveDownCommand.CanExecute(null), Is.False);

            slots[0].MoveDownCommand.Execute(null);

            Assert.That(slots.Take(3).Select(s => s.SkillName), Is.EqualTo(new[] { "Wisdom", "Archery", "Luck" }));
            Assert.That(slots.Take(2).Select(s => s.LevelName), Is.EqualTo(new[] { SkillLevelNames.Expert, SkillLevelNames.Basic }));
        }

        [Test]
        public void ClearingASkillMovesTheLaterOnesUp()
        {
            var slots = this.Skills.Slots;

            slots[0].ClearCommand.Execute(null);

            Assert.That(slots.Take(3).Select(s => s.SkillName), Is.EqualTo(new[] { "Wisdom", "Luck", null }));
            Assert.That(slots[2].ClearCommand.CanExecute(null), Is.False);
        }

        [Test]
        public void PickingSkillsAndLevelsInSlots()
        {
            var slots = this.Skills.Slots;

            slots[3].SkillName = "Tactics";
            Assert.That(slots[3].SkillName, Is.EqualTo("Tactics"));
            Assert.That(slots[3].LevelName, Is.EqualTo(SkillLevelNames.Basic));

            // Replacing a skill keeps the slot's level.
            slots[1].SkillName = "Offense";
            Assert.That(slots.Take(4).Select(s => s.SkillName), Is.EqualTo(new[] { "Archery", "Offense", "Luck", "Tactics" }));
            Assert.That(slots[1].LevelName, Is.EqualTo(SkillLevelNames.Expert));

            slots[1].LevelName = SkillLevelNames.Basic;
            Assert.That(slots[1].LevelName, Is.EqualTo(SkillLevelNames.Basic));

            // A skill the hero already has is not added twice, and the box goes back to what is stored.
            slots[0].SkillName = "Luck";
            Assert.That(slots[0].SkillName, Is.EqualTo("Archery"));

            // Only the first free slot can be filled.
            slots[5].SkillName = "Scouting";
            Assert.That(slots[5].IsFilled, Is.False);
        }

        [Test]
        public void AnySkillCanBeGivenALevelAndBeyondTheSlotsItBecomesExtra()
        {
            var all = this.Skills.AllSkills;
            SkillRowViewModel Row(string name) => all.Single(r => r.Name == name);

            Assert.That(Row("Archery").LevelName, Is.EqualTo(SkillLevelNames.Basic));
            Assert.That(Row("Archery").Placement, Is.EqualTo("Slot 1"));
            Assert.That(Row("Sorcery").LevelName, Is.EqualTo(SkillLevelNames.None));
            Assert.That(Row("Sorcery").Placement, Is.Empty);

            // Fill the remaining slots, then one more.
            foreach (var name in new[] { "Tactics", "Scouting", "Learning", "Offense", "Armorer", "Sorcery", "Mysticism" })
            {
                Row(name).LevelName = SkillLevelNames.Advanced;
            }

            // Three skills plus five of the new ones fill the eight slots; Sorcery and Mysticism have no slot left.
            Assert.That(this.Skills.Slots.Count(s => s.IsFilled), Is.EqualTo(Constants.SkillSlots.Max));
            Assert.That(Row("Armorer").Placement, Is.EqualTo("Slot 8"));
            Assert.That(Row("Mysticism").LevelName, Is.EqualTo(SkillLevelNames.Advanced));
            Assert.That(Row("Mysticism").IsExtra, Is.True);
            Assert.That(Row("Mysticism").Placement, Does.Contain("Not shown"));
            Assert.That(this.Skills.HasExtraSkills, Is.True);
            Assert.That(this.Skills.ExtraSummary, Does.StartWith("2 skills"));
            Assert.That(this.hero.HeroModel.SecondarySkills.GetSkillLevel(SecondarySkill.Mysticism), Is.EqualTo(SkillLevel.Intermediate));

            // An extra skill can be raised, and removing it with level "None" leaves the slots alone.
            Row("Mysticism").LevelName = SkillLevelNames.Expert;
            Assert.That(this.hero.HeroModel.SecondarySkills.GetSkillLevel(SecondarySkill.Mysticism), Is.EqualTo(SkillLevel.Expert));
            Row("Mysticism").LevelName = SkillLevelNames.None;
            Row("Sorcery").LevelName = SkillLevelNames.None;
            Assert.That(this.Skills.HasExtraSkills, Is.False);
            Assert.That(this.Skills.Slots.Count(s => s.IsFilled), Is.EqualTo(Constants.SkillSlots.Max));
        }

        [Test]
        public void HidingASkillKeepsItsLevelAndShowingPutsItBackInAFreeSlot()
        {
            var all = this.Skills.AllSkills;
            var wisdom = all.Single(r => r.Name == "Wisdom");

            wisdom.HideCommand.Execute(null);

            Assert.That(this.Skills.Slots.Take(3).Select(s => s.SkillName), Is.EqualTo(new[] { "Archery", "Luck", null }));
            Assert.That(wisdom.LevelName, Is.EqualTo(SkillLevelNames.Expert));
            Assert.That(wisdom.IsExtra, Is.True);
            Assert.That(wisdom.ShowCommand.CanExecute(null), Is.True);

            wisdom.ShowCommand.Execute(null);

            Assert.That(this.Skills.Slots.Take(3).Select(s => s.SkillName), Is.EqualTo(new[] { "Archery", "Luck", "Wisdom" }));
            Assert.That(wisdom.IsExtra, Is.False);
        }

        [Test]
        public void SpellsListEverySpellOfTheVersionWithLearnedAndAvailable()
        {
            var rows = this.Spells.Finder.Items.Cast<SpellRowViewModel>().ToList();
            var book = this.hero.HeroModel.SpellBook;

            Assert.That(rows.Select(r => r.Spell), Is.EquivalentTo(Spell.All.Where(s => s.IsAvailableIn(this.hero.GameVersion))));
            foreach (var row in rows)
            {
                Assert.That(row.IsLearned, Is.EqualTo(book.IsLearned(row.Spell)), row.Name);
                Assert.That(row.IsAvailable, Is.EqualTo(book.IsAvailable(row.Spell)), row.Name);
            }

            Assert.That(this.Spells.Finder.Items.Groups, Is.Not.Empty);
        }

        [Test]
        public void LearningASpellWritesTheSpellBookAndUndoShowsItAgain()
        {
            var row = this.Spells.Finder.Items.Cast<SpellRowViewModel>().First(r => !r.IsLearned);

            row.IsLearned = true;
            Assert.That(this.hero.HeroModel.SpellBook.IsLearned(row.Spell), Is.True);

            this.game.History.Undo();
            Assert.That(row.IsLearned, Is.False);
            Assert.That(this.hero.HeroModel.SpellBook.IsLearned(row.Spell), Is.False);
        }

        [Test]
        public void AvailableSpellsCannotBeEditedThroughTheRow()
        {
            var row = this.Spells.Finder.Items.Cast<SpellRowViewModel>().First();
            var typeOfIsAvailable = typeof(SpellRowViewModel).GetProperty(nameof(SpellRowViewModel.IsAvailable));

            Assert.That(typeOfIsAvailable?.CanWrite, Is.False);
            Assert.That(row, Is.Not.Null);
        }

        [Test]
        public void FilterNarrowsTheListAndLearnAllOnlyTouchesWhatIsShown()
        {
            this.Spells.Finder.Filter = "magic arrow";
            var shown = this.Spells.Finder.Items.Cast<SpellRowViewModel>().ToList();
            Assert.That(shown.Select(r => r.Name), Is.EqualTo(new[] { "Magic Arrow" }));

            this.Spells.ForgetAllCommand.Execute(null);
            this.Spells.LearnAllCommand.Execute(null);

            var book = this.hero.HeroModel.SpellBook;
            Assert.That(book.IsLearned(Spell.MagicArrow), Is.True);

            // Everything else is as it was loaded.
            var original = this.game.History.Original;
            var changed = this.game.History.GetPendingChanges().Where(c => c.Change.Area == ChangeAreas.Spells).ToList();
            Assert.That(original, Is.Not.Null);
            Assert.That(changed.All(c => c.Change.After.Contains("Magic Arrow") || c.Change.After == "nothing learned"), Is.True);
        }

        [Test]
        public void LevelFilterShowsOneLevelAndCombinesWithTheNameFilter()
        {
            this.Spells.Finder.Filter = "level 3";

            var shown = this.Spells.Finder.Items.Cast<SpellRowViewModel>().ToList();
            Assert.That(shown, Is.Not.Empty);
            Assert.That(shown.Select(r => r.Level), Is.All.EqualTo(3));
            Assert.That(shown.Count, Is.EqualTo(Spell.All.Count(s => s.Level == 3 && s.IsAvailableIn(this.hero.GameVersion))));
            Assert.That(this.Spells.Finder.Summary, Does.Contain($"showing {shown.Count}"));

            this.Spells.Finder.Filter = "level 3 fire";
            Assert.That(this.Spells.Finder.Items.Cast<SpellRowViewModel>().Select(r => r.Name), Is.EquivalentTo(shown.Where(r => r.School == "Fire magic").Select(r => r.Name)));
            Assert.That(this.Spells.Finder.Items.Cast<SpellRowViewModel>(), Is.Not.Empty);

            this.Spells.Finder.Filter = string.Empty;
            Assert.That(this.Spells.Finder.Items.Cast<SpellRowViewModel>().Count(), Is.EqualTo(Spell.All.Count(s => s.IsAvailableIn(this.hero.GameVersion))));
        }

        [Test]
        public void LearnAllWithALevelFilterOnlyLearnsThatLevel()
        {
            var spells = Spell.All.Where(s => s.IsAvailableIn(this.hero.GameVersion)).ToList();
            var learnedBefore = spells.Where(s => this.hero.HeroModel.SpellBook.IsLearned(s)).ToHashSet();

            this.Spells.Finder.Filter = "level 5";
            this.Spells.LearnAllCommand.Execute(null);

            var book = this.hero.HeroModel.SpellBook;
            foreach (var spell in spells)
            {
                var expected = spell.Level == 5 || learnedBefore.Contains(spell);
                Assert.That(book.IsLearned(spell), Is.EqualTo(expected), spell.Name);
            }

            this.Spells.ForgetAllCommand.Execute(null);

            foreach (var spell in spells)
            {
                var expected = spell.Level != 5 && learnedBefore.Contains(spell);
                Assert.That(this.hero.HeroModel.SpellBook.IsLearned(spell), Is.EqualTo(expected), spell.Name);
            }
        }

        [Test]
        public void SpellBookCheckBoxWritesTheHeroAndFollowsUndo()
        {
            var before = this.Spells.HasSpellBook;

            this.Spells.HasSpellBook = !before;
            Assert.That(this.hero.HeroModel.SpellBook.HasSpellBook, Is.EqualTo(!before));

            this.game.History.Undo();
            Assert.That(this.Spells.HasSpellBook, Is.EqualTo(before));
        }
    }
}
