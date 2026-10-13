namespace Siniskalc.Homm2.Data
{
    using System;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Writes a castle model into the record of a castle, as it is given. A field whose bytes already read as the model says is left alone,
    /// and so is everything the model does not hold: the name, the number of the castle and the rest of the bytes of the record.
    /// </summary>
    public sealed class SafeHomm2CastleSerializer : IHomm2CastleSerializer
    {
        /// <inheritdoc/>
        public Homm2CastleChunk Serialize(Homm2CastleChunk chunk, Homm2CastleModel model)
        {
            var bytes = chunk.ToArray();

            bytes[Homm2CastleLayout.Owner] = Homm2Bytes.ColorByte(model.Owner, bytes[Homm2CastleLayout.Owner]);
            bytes[Homm2CastleLayout.Race] = Homm2Bytes.RaceByte(model.Race, bytes[Homm2CastleLayout.Race]);
            bytes[Homm2CastleLayout.X] = ToByte(model.Position.X, nameof(model.Position));
            bytes[Homm2CastleLayout.Y] = ToByte(model.Position.Y, nameof(model.Position));
            WriteBoat(bytes, model.Boat);
            bytes[Homm2CastleLayout.VisitingHero] = Homm2Bytes.IdByte(model.VisitingHero, bytes[Homm2CastleLayout.VisitingHero]);

            for (var slot = 0; slot < Constants.ArmySlotCount; slot++)
            {
                WriteGarrisonSlot(bytes, slot, model.Garrison.Slots[slot]);
            }

            ByteBufferHelper.WriteUInt16(bytes, Homm2CastleLayout.TownBuildings, (ushort)model.Town);
            ByteBufferHelper.WriteUInt32(bytes, Homm2CastleLayout.Dwellings, (uint)model.Dwellings);

            for (var dwelling = 0; dwelling < Constants.Castle.DwellingCount; dwelling++)
            {
                var offset = Homm2CastleLayout.Creatures + (dwelling * 2);
                if (BitConverter.ToUInt16(bytes, offset) != model.Creatures[dwelling])
                {
                    RangeGuard.ThrowIfOutOfRange<long>(model.Creatures[dwelling], Constants.Castle.MinCreatures, Constants.Castle.MaxCreatures, nameof(model.Creatures));
                    ByteBufferHelper.WriteUInt16(bytes, offset, (ushort)model.Creatures[dwelling]);
                }
            }

            for (var upgraded = 0; upgraded < Constants.Castle.DwellingCount; upgraded++)
            {
                var offset = Homm2CastleLayout.UpgradedCreatures + (upgraded * 2);
                if (BitConverter.ToUInt16(bytes, offset) != model.UpgradedCreatures[upgraded])
                {
                    RangeGuard.ThrowIfOutOfRange<long>(model.UpgradedCreatures[upgraded], Constants.Castle.MinCreatures, Constants.Castle.MaxCreatures, nameof(model.UpgradedCreatures));
                    ByteBufferHelper.WriteUInt16(bytes, offset, (ushort)model.UpgradedCreatures[upgraded]);
                }
            }

            for (var slot = 0; slot < Homm2Bytes.GuildSpellSlots; slot++)
            {
                bytes[Homm2CastleLayout.GuildSpells + slot] = Homm2Bytes.GuildSpellByte(model.GuildSpells[slot], bytes[Homm2CastleLayout.GuildSpells + slot]);
            }

            for (var level = 0; level < Constants.Castle.MageGuildLevels; level++)
            {
                bytes[Homm2CastleLayout.GuildSpellCounts + level] = (byte)model.GuildSpellCounts[level];
            }

            return chunk.WithBytes(bytes);
        }

        private static byte ToByte(int value, string name)
        {
            RangeGuard.ThrowIfOutOfRange(value, 0, byte.MaxValue, name);
            return (byte)value;
        }

        private static void WriteBoat(byte[] bytes, Homm2Position? boat)
        {
            var current = bytes[Homm2CastleLayout.Boat] == Homm2Bytes.Empty ? (Homm2Position?)null : new Homm2Position(bytes[Homm2CastleLayout.Boat], bytes[Homm2CastleLayout.Boat + 1]);
            if (current == boat)
            {
                return;
            }

            bytes[Homm2CastleLayout.Boat] = boat is { } at ? ToByte(at.X, nameof(boat)) : Homm2Bytes.Empty;
            bytes[Homm2CastleLayout.Boat + 1] = boat is { } to ? ToByte(to.Y, nameof(boat)) : Homm2Bytes.Empty;
        }

        private static void WriteGarrisonSlot(byte[] bytes, int slot, Homm2ArmySlot? wanted)
        {
            var typeOffset = Homm2CastleLayout.GarrisonTypes + slot;
            var countOffset = Homm2CastleLayout.GarrisonCounts + (slot * 2);
            var currentType = Homm2Bytes.Creature(bytes[typeOffset]);
            var currentCount = BitConverter.ToUInt16(bytes, countOffset);

            if (wanted is null)
            {
                if (currentType is not null)
                {
                    bytes[typeOffset] = Homm2Bytes.Empty;
                    ByteBufferHelper.WriteUInt16(bytes, countOffset, 0);
                }

                return;
            }

            if (currentType == wanted.Creature && currentCount == wanted.Count)
            {
                return;
            }

            RangeGuard.ThrowIfOutOfRange<long>(wanted.Count, 0, Constants.MaxCreatureCount, nameof(wanted));
            bytes[typeOffset] = Homm2Bytes.CreatureByte(wanted.Creature, bytes[typeOffset]);
            ByteBufferHelper.WriteUInt16(bytes, countOffset, (ushort)wanted.Count);
        }
    }
}
