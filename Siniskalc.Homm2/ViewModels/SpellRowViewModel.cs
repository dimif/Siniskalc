namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// One spell in the spells list: whether the hero has it in the spell book, and what it costs to cast.
    /// </summary>
    public sealed class SpellRowViewModel : SpellChoiceViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SpellRowViewModel"/> class.
        /// </summary>
        /// <param name="spell">The spell.</param>
        /// <param name="onLearnedChanged">The on learned changed.</param>
        public SpellRowViewModel(Spell spell, Action<SpellRowViewModel, bool> onLearnedChanged)
            : base(spell.Name, MagicOf(spell), spell.Level, spell.Type.ToString(), (row, learned) => onLearnedChanged((SpellRowViewModel)row, learned))
        {
            this.Spell = spell;
        }

        /// <summary>
        /// Gets the spell of the row.
        /// </summary>
        public Spell Spell { get; }

        /// <summary>
        /// Gets "Mind" or "Elemental" for the spells the rules single out (the giants and the titans are immune to the mind spells), otherwise nothing.
        /// </summary>
        public string Magic => this.Group;

        /// <summary>
        /// Gets how many spell points casting the spell costs.
        /// </summary>
        public int Cost => this.Spell.SpellPoints;

        private static string MagicOf(Spell spell) => spell.Group.Hidden ? string.Empty : spell.Group.ToString();
    }
}
