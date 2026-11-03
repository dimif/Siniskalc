namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

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

        /// <summary>
        /// Initializes a new instance of the <see cref="SpellsViewModel"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        /// <param name="getSpellBook">The get spell book.</param>
        /// <param name="update">The update.</param>
        public SpellsViewModel(EntityCatalog catalog, Func<SpellBook> getSpellBook, Action<Func<SpellBook, SpellBook>> update)
            : base(learned => CreateRows(catalog, learned))
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

        /// <inheritdoc/>
        protected override bool ReadHasSpellBook() => this.getSpellBook().HasSpellBook;

        /// <inheritdoc/>
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
        private static IReadOnlyList<SpellRowViewModel> CreateRows(EntityCatalog catalog, Action<SpellRowViewModel, bool> learned) => catalog.SpellSchools.All
            .SelectMany(school => catalog.Spells.All.Where(spell => spell.School == school)
                .OrderBy(spell => spell.Level).ThenBy(spell => spell.Name))
            .Select(spell => new SpellRowViewModel(spell, learned))
            .ToList();
    }
}
