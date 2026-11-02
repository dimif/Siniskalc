namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Input;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The spells page of a hero: whether the hero has a spell book, and every spell of the game with a box for learned (and one for
    /// available through artifacts, where the game has them), in a finder. It knows no game: the game's view model gives the rows and says
    /// how the hero's spells are read and written. The view (<c>SpellsView</c>) only binds to this. Call <see cref="Refresh"/> after the
    /// spells changed.
    /// </summary>
    /// <typeparam name="TRow">The game's row of a spell.</typeparam>
    public abstract class SpellsViewModelBase<TRow> : PageViewModelBase
        where TRow : SpellChoiceViewModel
    {
        private bool isRefreshing;

        /// <param name="createRows">Makes the rows, in the order they are listed, given what a row calls when the user ticks it.</param>
        protected SpellsViewModelBase(System.Func<System.Action<TRow, bool>, IReadOnlyList<TRow>> createRows)
            : base("Spells")
        {
            this.Rows = createRows(this.OnLearnedChanged);
            this.LearnAllCommand = new DelegateCommand(_ => this.SetLearnedInList(true), _ => this.HasSpellBook);
            this.ForgetAllCommand = new DelegateCommand(_ => this.SetLearnedInList(false), _ => this.HasSpellBook);
        }

        /// <summary>
        /// Gets the list of spells, narrowed by its search text.
        /// </summary>
        public abstract AbstractFinderViewModel Finder { get; }

        /// <summary>
        /// Gets the command that gives the hero every spell.
        /// </summary>
        public ICommand LearnAllCommand { get; }

        /// <summary>
        /// Gets the command that takes every spell away from the hero.
        /// </summary>
        public ICommand ForgetAllCommand { get; }

        /// <summary>
        /// Gets or sets whether the hero carries a spell book. Without one, learned spells have no effect.
        /// </summary>
        public bool HasSpellBook
        {
            get => this.ReadHasSpellBook();
            set
            {
                if (!this.isRefreshing && value != this.HasSpellBook)
                {
                    this.WriteHasSpellBook(value);
                }
            }
        }

        /// <summary>
        /// Gets whether the spell book can be added or taken away.
        /// </summary>
        public abstract bool CanChangeSpellBook { get; }

        /// <summary>
        /// Gets what the tooltip of the spell book box says.
        /// </summary>
        public virtual string SpellBookNote => "Without a spell book the hero cannot cast the learned spells.";

        /// <summary>
        /// Gets a row for each spell the game has.
        /// </summary>
        protected IReadOnlyList<TRow> Rows { get; }

        /// <summary>
        /// Shows the hero's spells again. Call after they changed.
        /// </summary>
        public void Refresh()
        {
            this.isRefreshing = true;
            try
            {
                foreach (var row in this.Rows)
                {
                    this.ShowRow(row);
                }

                this.RaiseAllPropertiesChanged();
            }
            finally
            {
                this.isRefreshing = false;
            }

            this.Finder.Refresh();
        }

        /// <summary>
        /// Called by a row when the user ticks or unticks it.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="learned">Whether it is now ticked.</param>
        protected void OnLearnedChanged(TRow row, bool learned)
        {
            if (this.isRefreshing)
            {
                return;
            }

            // Ticking a selected spell does the same to every spell that is selected (and shown).
            var rows = new List<TRow> { row };
            if (row.IsSelected)
            {
                rows.AddRange(this.Finder.Items.Cast<TRow>().Where(r => r.IsSelected && r != row));
            }

            this.SetLearned(rows, learned);
        }

        /// <summary>
        /// Returns whether the hero has a spell book.
        /// </summary>
        /// <returns><see langword="true"/> if the hero has one.</returns>
        protected abstract bool ReadHasSpellBook();

        /// <summary>
        /// Gives the hero a spell book or takes it away.
        /// </summary>
        /// <param name="value">Whether the hero has one.</param>
        protected abstract void WriteHasSpellBook(bool value);

        /// <summary>
        /// Takes the hero's state of a spell into its row.
        /// </summary>
        /// <param name="row">The row.</param>
        protected abstract void ShowRow(TRow row);

        /// <summary>
        /// Makes the hero learn or forget the spells of the rows.
        /// </summary>
        /// <param name="rows">The rows.</param>
        /// <param name="learned">Whether the hero learns the spells or forgets them.</param>
        protected abstract void SetLearned(IReadOnlyList<TRow> rows, bool learned);

        /// <summary>
        /// Learns or forgets the spells the list shows, so a filter narrows what "all" means.
        /// </summary>
        private void SetLearnedInList(bool learned) => this.SetLearned(this.Finder.Items.Cast<TRow>().ToList(), learned);
    }
}
