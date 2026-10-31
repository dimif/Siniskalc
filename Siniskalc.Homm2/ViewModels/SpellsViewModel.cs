namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// The spells page of a hero of Heroes of Might and Magic II: whether the hero has a spell book (the Magic Book is an artifact) and
    /// every spell with a box for learned. The page is in <see cref="SpellsViewModelBase{TRow}"/>; this reads the hero and writes every
    /// edit back through <c>update</c>. The spells stay marked when the book is taken away, as they do in the game.
    /// </summary>
    public sealed class SpellsViewModel : SpellsViewModelBase<SpellRowViewModel>
    {
        private readonly Func<HeroModel> getHero;
        private readonly Action<Func<HeroModel, HeroModel>> update;

        public SpellsViewModel(GameVersion gameVersion, Func<HeroModel> getHero, Action<Func<HeroModel, HeroModel>> update)
            : base(learned => CreateRows(gameVersion, learned))
        {
            this.getHero = getHero;
            this.update = update;
            this.Finder = new SpellFinderViewModel(this.Rows);

            this.Refresh();
        }

        /// <inheritdoc/>
        public override SpellFinderViewModel Finder { get; }

        /// <summary>
        /// Gets whether the spell book can be added or taken away: adding needs a free artifact slot for the Magic Book.
        /// </summary>
        public override bool CanChangeSpellBook => this.getHero().HasSpellBook || !this.getHero().Artifacts.IsFull;

        /// <inheritdoc/>
        public override string SpellBookNote => "Without a spell book the hero cannot cast the learned spells. The book is the Magic Book artifact, which needs a free artifact slot.";

        protected override bool ReadHasSpellBook() => this.getHero().HasSpellBook;

        protected override void WriteHasSpellBook(bool value) => this.update(hero => value ? hero.WithSpellBook() : hero.WithoutSpellBook());

        /// <inheritdoc/>
        protected override void ShowRow(SpellRowViewModel row) => row.Show(this.getHero().Spells.Contains(row.Spell), false);

        /// <inheritdoc/>
        protected override void SetLearned(IReadOnlyList<SpellRowViewModel> rows, bool learned)
        {
            var spells = rows.Select(r => r.Spell).ToList();
            this.update(hero => hero with { Spells = spells.Aggregate(hero.Spells, (book, spell) => learned ? book.With(spell) : book.Without(spell)) });
        }

        private static IReadOnlyList<SpellRowViewModel> CreateRows(GameVersion gameVersion, Action<SpellRowViewModel, bool> learned) => Spell.All
            .AvailableIn(gameVersion)
            .OrderBy(s => s.Name)
            .Select(s => new SpellRowViewModel(s, learned))
            .ToList();
    }
}
