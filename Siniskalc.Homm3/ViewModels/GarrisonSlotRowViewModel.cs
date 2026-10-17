namespace Siniskalc.Homm3.ViewModels
{
    using System.Globalization;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// A slot of the garrison of a castle, as a line of the page.
    /// </summary>
    public sealed class GarrisonSlotRowViewModel
    {
        public GarrisonSlotRowViewModel(int slotNumber, ArmySlot<Creature>? slot)
        {
            this.SlotNumber = slotNumber;
            this.IsEmpty = slot == null;
            this.CreatureName = slot == null ? "Empty" : slot.Creature.Name;
            this.CountText = slot == null ? string.Empty : slot.Count.ToString("N0", CultureInfo.CurrentCulture);
        }

        public int SlotNumber { get; }

        public bool IsEmpty { get; }

        public string CreatureName { get; }

        public string CountText { get; }
    }
}
