namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Entities;
    using Siniskalc.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// ViewModel for the spells page: whether the hero has a spell book, and every spell of the game version with a box for
    /// learned and one for available (see <see cref="Finder"/>).
    /// Learned spells are the hero's spell book, and can be edited. Available spells come from artifacts; the game
    /// works them out, so they are only shown.
    /// </summary>
    public sealed class SpellsViewModel : PageViewModelBase
    {
        private readonly Func<SpellBook> getSpellBook;
        private readonly Action<Func<SpellBook, SpellBook>> update;
        private readonly List<SpellRowViewModel> rows;
        private bool isRefreshing;

        public SpellsViewModel(GameVersion gameVersion, Func<SpellBook> getSpellBook, Action<Func<SpellBook, SpellBook>> update)
            : base("Spells")
        {
            this.getSpellBook = getSpellBook;
            this.update = update;

            // Spells are listed by school, and inside a school by level.
            this.rows = Spell.All
                .Where(s => s.IsAvailableIn(gameVersion))
                .Select(s => new SpellRowViewModel(s, this.OnLearnedChanged))
                .OrderBy(r => r.Spell.School).ThenBy(r => r.Spell.Level).ThenBy(r => r.Name)
                .ToList();
            this.Finder = new SpellFinderViewModel(this.rows);

            this.LearnAllCommand = new DelegateCommand(_ => this.SetLearned(true), _ => this.HasSpellBook);
            this.ForgetAllCommand = new DelegateCommand(_ => this.SetLearned(false), _ => this.HasSpellBook);

            this.Refresh();
        }

        /// <summary>
        /// Gets the list of spells, grouped by school and narrowed by its search text and level.
        /// </summary>
        public SpellFinderViewModel Finder { get; }

        public ICommand LearnAllCommand { get; }

        public ICommand ForgetAllCommand { get; }

        /// <summary>
        /// Gets or sets whether the hero carries a spell book. Without one, learned spells have no effect.
        /// </summary>
        public bool HasSpellBook
        {
            get => this.getSpellBook().HasSpellBook;
            set
            {
                if (!this.isRefreshing && value != this.HasSpellBook)
                {
                    this.update(book => book.WithHasSpellBook(value));
                }
            }
        }

        /// <summary>
        /// Gets whether the spell book can be added or taken away. It cannot while an artifact such as Titan's Thunder gives the hero one.
        /// </summary>
        public bool CanChangeSpellBook => !this.getSpellBook().IsSpellBookGranted;

        /// <summary>
        /// Shows the hero's spells again. Call after they changed.
        /// </summary>
        public void Refresh()
        {
            var book = this.getSpellBook();

            this.isRefreshing = true;
            try
            {
                foreach (var row in this.rows)
                {
                    row.Show(book.IsLearned(row.Spell), book.IsAvailable(row.Spell), book.Source(row.Spell));
                }

                this.RaiseAllPropertiesChanged();
            }
            finally
            {
                this.isRefreshing = false;
            }

            this.Finder.Refresh();
        }

        private void OnLearnedChanged(Spell spell, bool learned)
        {
            if (this.isRefreshing)
            {
                return;
            }

            // Ticking a selected spell does the same to every spell that is selected (and shown).
            var spells = new List<Spell> { spell };
            if (this.rows.Any(r => r.Spell == spell && r.IsSelected))
            {
                spells.AddRange(this.Finder.Items.Cast<SpellRowViewModel>().Where(r => r.IsSelected && r.Spell != spell).Select(r => r.Spell));
            }

            this.update(book => book.WithLearned(spells, learned));
        }

        /// <summary>
        /// Learns or forgets the spells the list shows, so a filter narrows what "all" means.
        /// </summary>
        private void SetLearned(bool learned)
        {
            var spells = this.Finder.Items.Cast<SpellRowViewModel>().Select(r => r.Spell).ToList();
            this.update(book => book.WithLearned(spells, learned));
        }
    }
}
