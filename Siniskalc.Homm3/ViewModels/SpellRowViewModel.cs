namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One spell in the spells list: whether the hero has learned it, and whether an artifact makes it available.
    /// </summary>
    public sealed class SpellRowViewModel : SpellChoiceViewModel
    {
        public SpellRowViewModel(Spell spell, Action<SpellRowViewModel, bool> onLearnedChanged)
            : base(spell.Name, SchoolOf(spell), spell.Level, spell.Type.ToString(), (row, learned) => onLearnedChanged((SpellRowViewModel)row, learned))
        {
            this.Spell = spell;
        }

        public Spell Spell { get; }

        /// <summary>
        /// Gets the heading the spell is listed under.
        /// </summary>
        public string School => this.Group;

        private static string SchoolOf(Spell spell) => spell.School switch
        {
            SpellSchool.Wind => "Air magic",
            SpellSchool.Water => "Water magic",
            SpellSchool.Fire => "Fire magic",
            SpellSchool.Earth => "Earth magic",
            _ => "All schools",
        };
    }
}
