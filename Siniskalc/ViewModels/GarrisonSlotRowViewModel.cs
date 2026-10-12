namespace Siniskalc.ViewModels
{
    using System.Globalization;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// A slot of the garrison of a castle, as a line of the page.
    /// </summary>
    public sealed class GarrisonSlotRowViewModel
    {
        public GarrisonSlotRowViewModel(int slotNumber, ArmySlot? slot)
        {
            this.SlotNumber = slotNumber;
            this.IsEmpty = slot == null;
            this.CreatureName = slot == null ? "Empty" : Creature.NameOf(slot.CreatureCode);
            this.CountText = slot == null ? string.Empty : slot.CreatureCount.ToString("N0", CultureInfo.CurrentCulture);
        }

        public int SlotNumber { get; }

        public bool IsEmpty { get; }

        public string CreatureName { get; }

        public string CountText { get; }
    }
}
