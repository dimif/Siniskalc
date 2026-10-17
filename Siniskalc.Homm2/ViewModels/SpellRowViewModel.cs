namespace Siniskalc.Homm2.ViewModels
{
    using System;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One spell in the spells list: whether the hero has it in the spell book, and what it costs to cast.
    /// </summary>
    public sealed class SpellRowViewModel : SpellChoiceViewModel
    {
        public SpellRowViewModel(Spell spell, Action<SpellRowViewModel, bool> onLearnedChanged)
            : base(spell.Name, string.Empty, spell.Level, spell.Type.ToString(), (row, learned) => onLearnedChanged((SpellRowViewModel)row, learned))
        {
            this.Spell = spell;
        }

        public Spell Spell { get; }

        /// <summary>
        /// Gets "Mind" or "Elemental" for the spells the rules single out (the giants and the titans are immune to the mind spells), otherwise nothing.
        /// </summary>
        public string Magic => this.Spell.Group == SpellGroup.None ? string.Empty : this.Spell.Group.ToString();

        /// <summary>
        /// Gets how many spell points casting the spell costs.
        /// </summary>
        public int Cost => this.Spell.SpellPoints;
    }
}
