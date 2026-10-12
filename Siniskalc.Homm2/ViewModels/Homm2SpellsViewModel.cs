namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The spells page of a hero of Heroes of Might and Magic II: whether the hero has a spell book (the Magic Book is an artifact) and
    /// every spell with a box for learned. The page is in <see cref="SpellsViewModelBase{TRow}"/>; this reads the hero and writes every
    /// edit back through <c>update</c>. The spells stay marked when the book is taken away, as they do in the game.
    /// </summary>
    public sealed class Homm2SpellsViewModel : SpellsViewModelBase<Homm2SpellRowViewModel>
    {
        private readonly Func<Homm2HeroModel> getHero;
        private readonly Action<Func<Homm2HeroModel, Homm2HeroModel>> update;

        public Homm2SpellsViewModel(Homm2GameVersion gameVersion, Func<Homm2HeroModel> getHero, Action<Func<Homm2HeroModel, Homm2HeroModel>> update)
            : base(learned => CreateRows(gameVersion, learned))
        {
            this.getHero = getHero;
            this.update = update;
            this.Finder = new Homm2SpellFinderViewModel(this.Rows);

            this.Refresh();
        }

        public override Homm2SpellFinderViewModel Finder { get; }

        /// <summary>
        /// Gets whether the spell book can be added or taken away: adding needs a free artifact slot for the Magic Book.
        /// </summary>
        public override bool CanChangeSpellBook => this.getHero().HasSpellBook || !this.getHero().Artifacts.IsFull;

        public override string SpellBookNote => "Without a spell book the hero cannot cast the learned spells. The book is the Magic Book artifact, which needs a free artifact slot.";

        protected override bool ReadHasSpellBook() => this.getHero().HasSpellBook;

        protected override void WriteHasSpellBook(bool value) => this.update(hero => value ? hero.WithSpellBook() : hero.WithoutSpellBook());

        protected override void ShowRow(Homm2SpellRowViewModel row) => row.Show(this.getHero().Spells.Contains(row.Spell), false);

        protected override void SetLearned(IReadOnlyList<Homm2SpellRowViewModel> rows, bool learned)
        {
            var spells = rows.Select(r => r.Spell).ToList();
            this.update(hero => hero with { Spells = spells.Aggregate(hero.Spells, (book, spell) => learned ? book.With(spell) : book.Without(spell)) });
        }

        private static IReadOnlyList<Homm2SpellRowViewModel> CreateRows(Homm2GameVersion gameVersion, Action<Homm2SpellRowViewModel, bool> learned) => Homm2Spell.All
            .Where(s => s.IsAvailableIn(gameVersion))
            .OrderBy(s => s.Name)
            .Select(s => new Homm2SpellRowViewModel(s, learned))
            .ToList();
    }
}
