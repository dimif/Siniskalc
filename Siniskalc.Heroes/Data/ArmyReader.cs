namespace Siniskalc.Heroes.Data
{
    using System;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Reads the army of a hero or the garrison of a castle from a chunk, in whatever width the game keeps its tables.
    /// A type that is no creature reads as the first known one, and the slot is never lost.
    /// </summary>
    public static class ArmyReader
    {
        /// <summary>
        /// Reads the army that the tables of <paramref name="layout"/> hold.
        /// </summary>
        public static IArmy<TCreature> Read<TCreature>(BytesChunk chunk, ArmyLayout layout, ICreatureCodes<TCreature> codes)
            where TCreature : class, IGameEntity
        {
            var slots = new ArmySlot<TCreature>?[layout.SlotCount];
            for (var slot = 0; slot < slots.Length; slot++)
            {
                var type = ReadNumber(chunk, layout.TypeAt(slot), layout.TypeSize);
                if (type == layout.EmptyType)
                {
                    continue;
                }

                // TODO:LOG warning: the record "{chunk.Name}" holds creature type {type} in slot {slot + 1}, which this editor does not know; it reads as the first known creature and the bytes are kept.
                slots[slot] = new ArmySlot<TCreature>(codes.Find(type) ?? codes.Fallback, ReadNumber(chunk, layout.CountAt(slot), layout.CountSize));
            }

            return new Army<TCreature>(slots);
        }

        private static uint ReadNumber(BytesChunk chunk, int offset, int size) => size switch
        {
            1 => chunk.GetByteAt(offset),
            2 => chunk.GetUInt16At(offset),
            4 => chunk.GetUInt32At(offset),
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, "A number in a table is 1, 2 or 4 bytes."),
        };
    }
}
