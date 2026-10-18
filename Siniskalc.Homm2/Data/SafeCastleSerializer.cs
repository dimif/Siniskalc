namespace Siniskalc.Homm2.Data
{
    using System;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Writes a castle model into the record of a castle, as it is given. A field whose bytes already read as the model says is left alone,
    /// and so is everything the model does not hold: the name, the number of the castle and the rest of the bytes of the record.
    /// </summary>
    public sealed class SafeCastleSerializer : ICastleSerializer
    {
        /// <inheritdoc/>
        public CastleChunk Serialize(CastleChunk chunk, CastleModel model)
        {
            var bytes = chunk.ToArray();

            bytes[CastleLayout.Owner] = Bytes.ColorByte(model.Owner, bytes[CastleLayout.Owner]);
            bytes[CastleLayout.Faction] = Bytes.FactionByte(model.Faction, bytes[CastleLayout.Faction]);
            bytes[CastleLayout.X] = ToByte(model.Position.X, nameof(model.Position));
            bytes[CastleLayout.Y] = ToByte(model.Position.Y, nameof(model.Position));
            WriteBoat(bytes, model.Boat);
            bytes[CastleLayout.VisitingHero] = Bytes.IdByte(model.VisitingHero, bytes[CastleLayout.VisitingHero]);

            for (var slot = 0; slot < Constants.ArmySlotCount; slot++)
            {
                Bytes.WriteArmySlot(bytes, CastleLayout.GarrisonTypes + slot, CastleLayout.GarrisonCounts + (slot * 2), model.Garrison.Slots[slot], nameof(model.Garrison));
            }

            RangeGuard.ThrowIfOutOfRange<long>((long)model.Town, 0, Constants.Castle.MaxTownBuildings, nameof(model.Town));
            ByteBufferHelper.WriteUInt16(bytes, CastleLayout.TownBuildings, (ushort)model.Town);
            ByteBufferHelper.WriteUInt32(bytes, CastleLayout.Dwellings, (uint)model.Dwellings);

            for (var dwelling = 0; dwelling < Constants.Castle.DwellingCount; dwelling++)
            {
                var offset = CastleLayout.Creatures + (dwelling * 2);
                if (BitConverter.ToUInt16(bytes, offset) != model.Creatures[dwelling])
                {
                    RangeGuard.ThrowIfOutOfRange<long>(model.Creatures[dwelling], Constants.Castle.MinCreatures, Constants.Castle.MaxCreatures, nameof(model.Creatures));
                    ByteBufferHelper.WriteUInt16(bytes, offset, (ushort)model.Creatures[dwelling]);
                }
            }

            for (var upgraded = 0; upgraded < Constants.Castle.DwellingCount; upgraded++)
            {
                var offset = CastleLayout.UpgradedCreatures + (upgraded * 2);
                if (BitConverter.ToUInt16(bytes, offset) != model.UpgradedCreatures[upgraded])
                {
                    RangeGuard.ThrowIfOutOfRange<long>(model.UpgradedCreatures[upgraded], Constants.Castle.MinCreatures, Constants.Castle.MaxCreatures, nameof(model.UpgradedCreatures));
                    ByteBufferHelper.WriteUInt16(bytes, offset, (ushort)model.UpgradedCreatures[upgraded]);
                }
            }

            for (var slot = 0; slot < Bytes.GuildSpellSlots; slot++)
            {
                bytes[CastleLayout.GuildSpells + slot] = Bytes.GuildSpellByte(model.GuildSpells[slot], bytes[CastleLayout.GuildSpells + slot]);
            }

            for (var level = 0; level < Constants.Castle.MageGuildLevels; level++)
            {
                RangeGuard.ThrowIfOutOfRange<long>(model.GuildSpellCounts[level], Constants.Castle.MinGuildSpellCount, Constants.Castle.MaxGuildSpellCount, nameof(model.GuildSpellCounts));
                bytes[CastleLayout.GuildSpellCounts + level] = (byte)model.GuildSpellCounts[level];
            }

            return chunk.WithBytes(bytes);
        }

        private static byte ToByte(int value, string name)
        {
            RangeGuard.ThrowIfOutOfRange<long>(value, Constants.Castle.MinCoordinate, Constants.Castle.MaxCoordinate, name);
            return (byte)value;
        }

        private static void WriteBoat(byte[] bytes, MapPosition? boat)
        {
            var current = bytes[CastleLayout.Boat] == Bytes.Empty ? (MapPosition?)null : new MapPosition(bytes[CastleLayout.Boat], bytes[CastleLayout.Boat + 1]);
            if (current == boat)
            {
                return;
            }

            bytes[CastleLayout.Boat] = boat is { } at ? ToByte(at.X, nameof(boat)) : Bytes.Empty;
            bytes[CastleLayout.Boat + 1] = boat is { } to ? ToByte(to.Y, nameof(boat)) : Bytes.Empty;
        }
    }
}
