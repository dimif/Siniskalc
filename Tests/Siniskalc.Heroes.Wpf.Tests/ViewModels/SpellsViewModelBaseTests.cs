namespace Siniskalc.Heroes.Wpf.Tests.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// Tests what every game's spell page does: shows the spells the hero learned, passes a tick on a spell (or on all the selected
    /// ones) to the game, and learns or forgets all the spells that are listed. The game's part is a fake that notes what it was asked.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Unit)]
    internal sealed class SpellsViewModelBaseTests
    {
        private sealed class Page : SpellsViewModelBase<SpellChoiceViewModel>
        {
            private TestFinder? finder;

            public Page()
                : base(onChanged => [
                    new SpellChoiceViewModel("Bless", "Earth", 1, "Combat", onChanged),
                    new SpellChoiceViewModel("Cure", "Earth", 1, "Combat", onChanged),
                    new SpellChoiceViewModel("Fireball", "Fire", 3, "Combat", onChanged),
                ])
            {
            }

            public HashSet<string> Learned { get; } = [];

            public bool Book { get; set; } = true;

            public List<string> Calls { get; } = [];

            public Action<SpellChoiceViewModel>? OnShow { get; set; }

            public IReadOnlyList<SpellChoiceViewModel> AllRows => this.Rows;

            public override AbstractFinderViewModel Finder => this.finder ??= new TestFinder(items: this.Rows);

            public override bool CanChangeSpellBook => true;

            protected override bool ReadHasSpellBook() => this.Book;

            protected override void WriteHasSpellBook(bool value)
            {
                this.Calls.Add($"book {value}");
                this.Book = value;
            }

            protected override void ShowRow(SpellChoiceViewModel row)
            {
                row.Show(this.Learned.Contains(row.Name), false);
                this.OnShow?.Invoke(row);
            }

            protected override void SetLearned(IReadOnlyList<SpellChoiceViewModel> rows, bool learned)
            {
                this.Calls.Add($"{(learned ? "learn" : "forget")} {string.Join(",", rows.Select(r => r.Name))}");
                foreach (var row in rows)
                {
                    if (learned)
                    {
                        this.Learned.Add(row.Name);
                    }
                    else
                    {
                        this.Learned.Remove(row.Name);
                    }
                }
            }
        }

        private static SpellChoiceViewModel Spell(Page page, string name) => page.AllRows.Single(r => r.Name == name);

        [Test]
        public void ThePageIsCalledSpellsAndHoldsTheRowsTheGameMade()
        {
            var page = new Page();

            Assert.That(page.Title, Is.EqualTo("Spells"));
            Assert.That(page.AllRows.Select(r => r.Name), Is.EqualTo(new[] { "Bless", "Cure", "Fireball" }));
        }

        [Test]
        public void TheSpellBookIsReadFromTheGameAndTheNoteSaysWhatItIsFor()
        {
            var page = new Page { Book = false };

            Assert.That(page.HasSpellBook, Is.False);
            Assert.That(page.SpellBookNote, Does.Contain("cannot cast"));
        }

        [Test]
        public void GivingTheHeroASpellBookWritesItAndTheSameStateWritesNothing()
        {
            var page = new Page { Book = false };

            page.HasSpellBook = false;
            page.HasSpellBook = true;
            page.HasSpellBook = true;

            Assert.That(page.Calls, Is.EqualTo(new[] { "book True" }));
        }

        [Test]
        public void TheSpellBookIsNotWrittenWhileThePageIsBeingRefreshed()
        {
            var page = new Page { Book = false };
            page.OnShow = _ => page.HasSpellBook = true;

            page.Refresh();

            Assert.That(page.Calls, Is.Empty);
        }

        [Test]
        public void RefreshShowsWhichSpellsTheHeroLearnedAndWritesNothing()
        {
            var page = new Page();
            page.Learned.Add("Cure");

            page.Refresh();

            Assert.That(page.AllRows.Select(r => r.IsLearned), Is.EqualTo(new[] { false, true, false }));
            Assert.That(page.Calls, Is.Empty);
        }

        [Test]
        public void RefreshTellsEveryBindingAndTheFinder()
        {
            var page = new Page();
            var raised = new List<string?>();
            page.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
            var finderRaised = new List<string?>();
            page.Finder.PropertyChanged += (_, e) => finderRaised.Add(e.PropertyName);

            page.Refresh();

            Assert.That(raised, Does.Contain(string.Empty));
            Assert.That(finderRaised, Does.Contain("Summary"));
        }

        [Test]
        public void ATickWhileThePageIsBeingRefreshedIsNotPassedToTheGame()
        {
            var page = new Page();
            page.OnShow = row => row.IsLearned = !row.IsLearned;

            page.Refresh();

            Assert.That(page.Calls, Is.Empty);
        }

        [Test]
        public void TickingASpellThatIsNotSelectedLearnsOnlyThatSpell()
        {
            var page = new Page();

            Spell(page, "Cure").IsLearned = true;

            Assert.That(page.Calls, Is.EqualTo(new[] { "learn Cure" }));
        }

        [Test]
        public void TickingASpellInTheStateItIsAlreadyInTellsTheGameNothing()
        {
            var page = new Page();
            page.Learned.Add("Cure");
            page.Refresh();

            Spell(page, "Cure").IsLearned = true;
            Spell(page, "Bless").IsLearned = false;

            Assert.That(page.Calls, Is.Empty);
        }

        [Test]
        public void SelectingASpellTellsOnlyWhenItsSelectionChanges()
        {
            var page = new Page();
            var raised = new List<string?>();
            Spell(page, "Bless").PropertyChanged += (_, e) => raised.Add(e.PropertyName);

            Spell(page, "Bless").IsSelected = true;
            Spell(page, "Bless").IsSelected = true;

            Assert.That(raised, Is.EqualTo(new[] { nameof(SpellChoiceViewModel.IsSelected) }));
        }

        [Test]
        public void UntickingASpellForgetsIt()
        {
            var page = new Page();
            page.Learned.Add("Cure");
            page.Refresh();

            Spell(page, "Cure").IsLearned = false;

            Assert.That(page.Calls, Is.EqualTo(new[] { "forget Cure" }));
        }

        [Test]
        public void TickingASelectedSpellDoesTheSameToEveryOtherSelectedSpellThatIsListed()
        {
            var page = new Page();
            Spell(page, "Bless").IsSelected = true;
            Spell(page, "Fireball").IsSelected = true;

            Spell(page, "Bless").IsLearned = true;

            Assert.That(page.Calls, Is.EqualTo(new[] { "learn Bless,Fireball" }));
        }

        [Test]
        public void ASpellThatIsSelectedButTickedAloneAmongUnselectedOnesStaysAlone()
        {
            var page = new Page();
            Spell(page, "Bless").IsSelected = true;

            Spell(page, "Cure").IsLearned = true;

            Assert.That(page.Calls, Is.EqualTo(new[] { "learn Cure" }));
        }

        [Test]
        public void LearnAllLearnsEverySpellOfTheListAndForgetAllForgetsThem()
        {
            var page = new Page();

            page.LearnAllCommand.Execute(null);
            page.ForgetAllCommand.Execute(null);

            Assert.That(page.Calls, Is.EqualTo(new[] { "learn Bless,Cure,Fireball", "forget Bless,Cure,Fireball" }));
        }

        [Test]
        public void LearnAllAndForgetAllAreOnlyOfferedWithASpellBook()
        {
            var page = new Page { Book = false };
            Assert.That((page.LearnAllCommand.CanExecute(null), page.ForgetAllCommand.CanExecute(null)), Is.EqualTo((false, false)));

            page.Book = true;

            Assert.That((page.LearnAllCommand.CanExecute(null), page.ForgetAllCommand.CanExecute(null)), Is.EqualTo((true, true)));
        }
    }
}
