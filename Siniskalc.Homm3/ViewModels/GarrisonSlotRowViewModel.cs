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
        /// <summary>
        /// Initializes a new instance of the <see cref="GarrisonSlotRowViewModel"/> class.
        /// </summary>
        /// <param name="slotNumber">The slot number.</param>
        /// <param name="slot">The slot.</param>
        public GarrisonSlotRowViewModel(int slotNumber, ArmySlot<Creature>? slot)
        {
            this.SlotNumber = slotNumber;
            this.IsEmpty = slot == null;
            this.CreatureName = slot == null ? "Empty" : slot.Creature.Name;
            this.CountText = slot == null ? string.Empty : slot.Count.ToString("N0", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Gets the number of the slot, from 1.
        /// </summary>
        public int SlotNumber { get; }

        /// <summary>
        /// Gets whether the slot is empty.
        /// </summary>
        public bool IsEmpty { get; }

        /// <summary>
        /// Gets the name of the creature in the slot.
        /// </summary>
        public string CreatureName { get; }

        /// <summary>
        /// Gets the number of creatures as text.
        /// </summary>
        public string CountText { get; }
    }
}
