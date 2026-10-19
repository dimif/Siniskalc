namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
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
            WriteBoat(bytes, model.ShipyardPosition);
            bytes[CastleLayout.VisitingHero] = Bytes.IdByte(model.VisitingHeroId, bytes[CastleLayout.VisitingHero]);

            ArmyWriter.Write(bytes, CastleLayout.Garrison, model.Garrison, CreatureCodes.Instance, nameof(model.Garrison));

            RangeGuard.ThrowIfOutOfRange<long>((long)model.Town, 0, Constants.Castle.MaxTownBuildings, nameof(model.Town));
            ByteBufferHelper.WriteUInt16(bytes, CastleLayout.TownBuildings, (ushort)model.Town);
            ByteBufferHelper.WriteUInt32(bytes, CastleLayout.Dwellings, (uint)model.Dwellings);

            WriteCreatures(chunk, bytes, model);

            for (var slot = 0; slot < Bytes.GuildSpellSlots; slot++)
            {
                bytes[CastleLayout.GuildSpells + slot] = Bytes.GuildSpellByte(model.MageGuildSpells[slot], bytes[CastleLayout.GuildSpells + slot]);
            }

            for (var level = 0; level < Constants.Castle.MageGuildLevels; level++)
            {
                RangeGuard.ThrowIfOutOfRange<long>(model.MageGuildSpellCounts[level], Constants.Castle.MinGuildSpellCount, Constants.Castle.MaxGuildSpellCount, nameof(model.MageGuildSpellCounts));
                bytes[CastleLayout.GuildSpellCounts + level] = (byte)model.MageGuildSpellCounts[level];
            }

            return chunk.WithBytes(bytes);
        }

        // The creatures of a dwelling wait in the list of its own or in the one of its upgrade, whichever its level says. The count is written
        // there. When the level changed the count moves: the other places of the dwelling are emptied, as the game does, and the dragons upgraded
        // twice get the mark the game shows them with. A place the count was not in is left as it is while the level stays.
        private static void WriteCreatures(CastleChunk chunk, byte[] bytes, CastleModel model)
        {
            var before = (Dwellings)chunk.GetUInt32At(CastleLayout.Dwellings);
            for (var dwelling = 1; dwelling <= Constants.Castle.DwellingCount; dwelling++)
            {
                var count = model.CreaturesAvailable[dwelling - 1];
                RangeGuard.ThrowIfOutOfRange<long>(count, Constants.Castle.MinCreatures, Constants.Castle.MaxCreatures, nameof(model.CreaturesAvailable));
                var level = model.DwellingLevel(dwelling);
                var active = CastleLayout.CreaturesOffset(dwelling, level);
                var moved = CastleModel.DwellingLevel(before, dwelling) != level;
                var changed = moved || BitConverter.ToUInt16(bytes, active) != count;

                if (moved)
                {
                    foreach (var other in CastleLayout.CreaturesOffsets(dwelling).Where(offset => offset != active))
                    {
                        ByteBufferHelper.WriteUInt16(bytes, other, 0);
                    }
                }

                if (BitConverter.ToUInt16(bytes, active) != count)
                {
                    ByteBufferHelper.WriteUInt16(bytes, active, (ushort)count);
                }

                var mark = CastleLayout.UpgradedCreatures + (CastleLayout.SecondUpgradeMarker * 2);
                if (level >= 3 && changed && BitConverter.ToUInt16(bytes, mark) == 0)
                {
                    ByteBufferHelper.WriteUInt16(bytes, mark, CastleLayout.SecondUpgradeMarkerValue);
                }
            }
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
