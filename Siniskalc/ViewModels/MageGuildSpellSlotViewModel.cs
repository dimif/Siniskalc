namespace Siniskalc.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One spell of the Mage Guild of a castle, with the spells that can take its place. The view model stays the same while the castle is edited and
    /// is refreshed: a drop-down list that is replaced while one of its items is being picked raises an error in WPF.
    /// </summary>
    public sealed class MageGuildSpellSlotViewModel : AbstractViewModelBase
    {
        private readonly Func<Spell> getSpell;
        private readonly Action<Spell> pick;

        public MageGuildSpellSlotViewModel(Func<Spell> getSpell, IReadOnlyList<Spell> choices, Action<Spell> pick)
        {
            this.getSpell = getSpell;
            this.Choices = choices;
            this.pick = pick;
        }

        public Spell Spell
        {
            get => this.getSpell();
            set
            {
                if (value != null && value != this.getSpell())
                {
                    this.pick(value);
                }
            }
        }

        public IReadOnlyList<Spell> Choices { get; }

        /// <summary>
        /// Shows the spell of the castle again.
        /// </summary>
        public void Refresh() => this.RaiseAllPropertiesChanged();
    }
}
