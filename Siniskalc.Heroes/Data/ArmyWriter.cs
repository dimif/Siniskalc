namespace Siniskalc.Heroes.Data
{
    using System;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Writes the army of a hero or the garrison of a castle into the bytes of a record. A slot whose bytes already read as the model says
    /// is left alone, so a creature the editor does not know keeps its number until the model holds something else.
    /// </summary>
    public static class ArmyWriter
    {
        /// <summary>
        /// Writes <paramref name="army"/> into the tables of <paramref name="layout"/>.
        /// </summary>
        /// <param name="bytes">The bytes of the record.</param>
        /// <param name="layout">Where the tables are.</param>
        /// <param name="army">The army to write.</param>
        /// <param name="codes">How the game numbers its creatures.</param>
        /// <param name="name">The name of the parameter the army came from, for the error of a count that does not fit.</param>
        /// <exception cref="ArgumentOutOfRangeException">A count is more than <see cref="ArmyLayout.MaxCount"/>.</exception>
        public static void Write<TCreature>(byte[] bytes, ArmyLayout layout, IArmy<TCreature> army, ICreatureCodes<TCreature> codes, string name)
            where TCreature : class, IGameEntity
        {
            for (var slot = 0; slot < layout.SlotCount; slot++)
            {
                var typeAt = layout.TypeAt(slot);
                var countAt = layout.CountAt(slot);
                var currentType = ReadNumber(bytes, typeAt, layout.TypeSize);
                var current = currentType == layout.EmptyType ? null : codes.Find(currentType) ?? codes.Fallback;
                var wanted = army.Slots[slot];

                if (wanted is null)
                {
                    if (current is not null)
                    {
                        WriteNumber(bytes, typeAt, layout.TypeSize, layout.EmptyType);
                        WriteNumber(bytes, countAt, layout.CountSize, 0);
                    }

                    continue;
                }

                if (current == wanted.Creature && ReadNumber(bytes, countAt, layout.CountSize) == wanted.Count)
                {
                    continue;
                }

                RangeGuard.ThrowIfOutOfRange(wanted.Count, 0, layout.MaxCount, name);
                if (current != wanted.Creature)
                {
                    WriteNumber(bytes, typeAt, layout.TypeSize, wanted.Creature.Code);
                }

                WriteNumber(bytes, countAt, layout.CountSize, (uint)wanted.Count);
            }
        }

        private static uint ReadNumber(byte[] bytes, int offset, int size) => size switch
        {
            1 => bytes[offset],
            2 => BitConverter.ToUInt16(bytes, offset),
            4 => ByteBufferHelper.ReadUInt32(bytes, offset),
            _ => throw new ArgumentOutOfRangeException(nameof(size), size, "A number in a table is 1, 2 or 4 bytes."),
        };

        private static void WriteNumber(byte[] bytes, int offset, int size, uint value)
        {
            switch (size)
            {
                case 1:
                    bytes[offset] = (byte)value;
                    break;
                case 2:
                    ByteBufferHelper.WriteUInt16(bytes, offset, (ushort)value);
                    break;
                case 4:
                    ByteBufferHelper.WriteUInt32(bytes, offset, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(size), size, "A number in a table is 1, 2 or 4 bytes.");
            }
        }
    }
}
