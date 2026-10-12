namespace Siniskalc.ViewModels
{
    using System.Collections.Generic;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// A spell level of the Mage Guild: whether the guild reaches it, how many spells it holds, and the spells themselves.
    /// </summary>
    public sealed class MageGuildLevelRowViewModel : AbstractViewModelBase
    {
        public MageGuildLevelRowViewModel(int level, bool isBuilt, int spells, IReadOnlyList<MageGuildSpellSlotViewModel> slots)
        {
            this.Level = level;
            this.Slots = slots;
            this.Update(isBuilt, spells);
        }

        public int Level { get; }

        public bool IsBuilt { get; private set; }

        public string SpellsText { get; private set; } = string.Empty;

        public IReadOnlyList<MageGuildSpellSlotViewModel> Slots { get; }

        /// <summary>
        /// Shows the level again after the castle was edited, and refreshes its spells.
        /// </summary>
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
