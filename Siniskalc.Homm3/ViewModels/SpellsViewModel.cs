namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// ViewModel for the spells page: whether the hero has a spell book, and every spell of the game version with a box for
    /// learned and one for available (see <see cref="Finder"/>).
    /// Learned spells are the hero's spell book, and can be edited. Available spells come from artifacts; the game
    /// works them out, so they are only shown.
    /// </summary>
    public sealed class SpellsViewModel : SpellsViewModelBase<SpellRowViewModel>
    {
        private readonly Func<SpellBook> getSpellBook;
        private readonly Action<Func<SpellBook, SpellBook>> update;

        public SpellsViewModel(GameVersion gameVersion, Func<SpellBook> getSpellBook, Action<Func<SpellBook, SpellBook>> update)
            : base(learned => CreateRows(gameVersion, learned))
        {
            this.getSpellBook = getSpellBook;
            this.update = update;
            this.Finder = new SpellFinderViewModel(this.Rows);

            this.Refresh();
        }

        /// <summary>
        /// Gets the list of spells, grouped by school and narrowed by its search text and level.
        /// </summary>
        public override SpellFinderViewModel Finder { get; }

        /// <summary>
        /// Gets whether the spell book can be added or taken away. It cannot while an artifact such as Titan's Thunder gives the hero one.
        /// </summary>
        public override bool CanChangeSpellBook => !this.getSpellBook().IsSpellBookGranted;

        /// <inheritdoc/>
        public override string SpellBookNote => "Without a spell book the hero cannot cast the learned spells. An artifact such as Titan's Thunder gives one.";

        protected override bool ReadHasSpellBook() => this.getSpellBook().HasSpellBook;

        protected override void WriteHasSpellBook(bool value) => this.update(book => book.WithHasSpellBook(value));

        /// <inheritdoc/>
        protected override void ShowRow(SpellRowViewModel row)
        {
            var book = this.getSpellBook();
            row.Show(book.IsLearned(row.Spell), book.IsAvailable(row.Spell), book.Source(row.Spell));
        }

        /// <inheritdoc/>
        protected override void SetLearned(IReadOnlyList<SpellRowViewModel> rows, bool learned)
        {
            var spells = rows.Select(r => r.Spell).ToList();
            this.update(book => book.WithLearned(spells, learned));
        }

        /// <summary>
        /// Lists spells by school, and inside a school by level.
        /// </summary>
        private static IReadOnlyList<SpellRowViewModel> CreateRows(GameVersion gameVersion, Action<SpellRowViewModel, bool> learned) => Spell.All
            .Where(s => s.IsAvailableIn(gameVersion))
            .Select(s => new SpellRowViewModel(s, learned))
            .OrderBy(r => r.Spell.School).ThenBy(r => r.Spell.Level).ThenBy(r => r.Name)
            .ToList();
    }
}
