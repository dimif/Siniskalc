namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// One spell in the spells list: whether the hero has learned it, and whether an artifact makes it available.
    /// </summary>
    public sealed class SpellRowViewModel : SpellChoiceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpellRowViewModel"/> class.
        /// </summary>
        /// <param name="spell">The spell.</param>
        /// <param name="onLearnedChanged">The on learned changed.</param>
        public SpellRowViewModel(Spell spell, Action<SpellRowViewModel, bool> onLearnedChanged)
            : base(spell.Name, SchoolOf(spell), spell.Level, spell.Type.ToString(), (row, learned) => onLearnedChanged((SpellRowViewModel)row, learned))
        {
            this.Spell = spell;
        }

        /// <summary>
        /// Gets the spell of the row.
        /// </summary>
        public Spell Spell { get; }

        /// <summary>
        /// Gets the heading the spell is listed under.
        /// </summary>
        public string School => this.Group;

        private static string SchoolOf(Spell spell) => spell.School.Description;
    }
}
