namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One spell in the spells list: whether the hero has it in the spell book, and what it costs to cast.
    /// </summary>
    public sealed class Homm2SpellRowViewModel : SpellChoiceViewModel
    {
        public Homm2SpellRowViewModel(Homm2Spell spell, Action<Homm2SpellRowViewModel, bool> onLearnedChanged)
            : base(spell.Name, string.Empty, 0, string.Empty, (row, learned) => onLearnedChanged((Homm2SpellRowViewModel)row, learned))
        {
            this.Spell = spell;
        }

        public Homm2Spell Spell { get; }

        /// <summary>
        /// Gets how many spell points casting the spell costs.
        /// </summary>
        public int Cost => this.Spell.SpellPoints;
    }
}
