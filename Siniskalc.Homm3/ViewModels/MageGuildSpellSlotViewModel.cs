namespace Siniskalc.Homm3.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// One spell of the Mage Guild of a castle, with the spells that can take its place. The view model stays the same while the castle is edited and
    /// is refreshed: a drop-down list that is replaced while one of its items is being picked raises an error in WPF.
    /// </summary>
    public sealed class MageGuildSpellSlotViewModel : AbstractViewModelBase
    {
        private readonly Func<Spell> getSpell;
        private readonly Action<Spell> pick;

        /// <summary>
        /// Initializes a new instance of the <see cref="MageGuildSpellSlotViewModel"/> class.
        /// </summary>
        /// <param name="getSpell">The get spell.</param>
        /// <param name="choices">The choices.</param>
        /// <param name="pick">The pick.</param>
        public MageGuildSpellSlotViewModel(Func<Spell> getSpell, IReadOnlyList<Spell> choices, Action<Spell> pick)
        {
            this.getSpell = getSpell;
            this.Choices = choices;
            this.pick = pick;
        }

        /// <summary>
        /// Gets or sets the spell in the slot; a spell that is not one of the choices is ignored.
        /// </summary>
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

        /// <summary>
        /// Gets the spells the slot can hold.
        /// </summary>
        public IReadOnlyList<Spell> Choices { get; }

        /// <summary>
        /// Shows the spell of the castle again.
        /// </summary>
        public void Refresh() => this.RaiseAllPropertiesChanged();
    }
}
