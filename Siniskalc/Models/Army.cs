namespace Siniskalc.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Entities;

    /// <summary>
    /// Immutable army of a hero/castle. Slots are numbered 1 to <see cref="Constants.CreatureSlotCount"/>; empty slots are absent.
    /// Edits return a new instance.
    /// </summary>
    public sealed class Army
    {
        private readonly IDictionary<int, ArmySlot> slotNumberToSlot;

        public Army(ArmySlot[] slots)
        {
            this.slotNumberToSlot = slots.ToDictionary(slot => slot.SlotNumber);
        }

        public ArmySlot[] Slots => [.. this.slotNumberToSlot.Values];

        public ArmySlot GetSlot(int index)
        {
            return this.slotNumberToSlot[index];
        }

        /// <summary>
        /// Returns the slot, or <c>null</c> if it is empty.
        /// </summary>
        public ArmySlot? FindSlot(int slotNumber)
        {
            return this.slotNumberToSlot.TryGetValue(slotNumber, out var slot) ? slot : null;
        }

        public Army WithSlot(int slotNumber, Creature creature, uint count)
        {
            ThrowIfOutOfRange(slotNumber);

            var slots = new Dictionary<int, ArmySlot>(this.slotNumberToSlot)
            {
                [slotNumber] = new ArmySlot(slotNumber, creature.Code, count),
            };

            return new Army([.. slots.Values]);
        }

        public Army WithoutSlot(int slotNumber)
        {
            ThrowIfOutOfRange(slotNumber);

            var slots = new Dictionary<int, ArmySlot>(this.slotNumberToSlot);
            slots.Remove(slotNumber);
            return new Army([.. slots.Values]);
        }

        /// <summary>
        /// Returns a copy with the contents of the two slots swapped. Swapping with an empty slot moves the stack.
        /// </summary>
        public Army Swap(int slotNumber, int otherSlotNumber)
        {
            ThrowIfOutOfRange(slotNumber);
            ThrowIfOutOfRange(otherSlotNumber);

            var slots = new Dictionary<int, ArmySlot>(this.slotNumberToSlot);
            var first = this.FindSlot(slotNumber);
            var second = this.FindSlot(otherSlotNumber);

            slots.Remove(slotNumber);
            slots.Remove(otherSlotNumber);

            if (first != null)
            {
                slots[otherSlotNumber] = first with { SlotNumber = otherSlotNumber };
            }

            if (second != null)
            {
                slots[slotNumber] = second with { SlotNumber = slotNumber };
            }

            return new Army([.. slots.Values]);
        }

        private static void ThrowIfOutOfRange(int slotNumber)
        {
            RangeGuard.ThrowIfOutOfRange(
                slotNumber,
                Constants.Army.MinSlotNumber,
                Constants.CreatureSlotCount,
                nameof(slotNumber),
                $"Slots are {Constants.Army.MinSlotNumber} to {Constants.CreatureSlotCount}.");
        }
    }
}
