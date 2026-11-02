namespace Siniskalc.Homm3.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A spell level of the Mage Guild: whether the guild reaches it, how many spells it holds, and the spells themselves.
    /// </summary>
    public sealed class MageGuildLevelRowViewModel : AbstractViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MageGuildLevelRowViewModel"/> class.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="isBuilt">The is built.</param>
        /// <param name="spells">The spells.</param>
        /// <param name="slots">The slots.</param>
        public MageGuildLevelRowViewModel(int level, bool isBuilt, int spells, IReadOnlyList<MageGuildSpellSlotViewModel> slots)
        {
            this.Level = level;
            this.Slots = slots;
            this.Update(isBuilt, spells);
        }

        /// <summary>
        /// Gets the level of the Mage Guild, from 1.
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// Gets whether the level is built.
        /// </summary>
        public bool IsBuilt { get; private set; }

        /// <summary>
        /// Gets the spells of the level as text.
        /// </summary>
        public string SpellsText { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the slots that hold the spells of the level.
        /// </summary>
        public IReadOnlyList<MageGuildSpellSlotViewModel> Slots { get; }

        /// <summary>
        /// Shows the level again after the castle was edited, and refreshes its spells.
        /// </summary>
        /// <param name="isBuilt">Whether the level is built.</param>
        /// <param name="spells">The number of spells the level has.</param>
        public void Update(bool isBuilt, int spells)
        {
            this.IsBuilt = isBuilt;
            this.SpellsText = isBuilt ? $"{spells} spells" : "Not built";
            foreach (var slot in this.Slots)
            {
                slot.Refresh();
            }

            this.RaiseAllPropertiesChanged();
        }
    }
}
