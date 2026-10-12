namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// Reads a <see cref="CastleModel"/> from a <see cref="CastleChunk"/>.
    /// </summary>
    public sealed class CastleDeserializer : ICastleDeserializer
    {
        public CastleModel Deserialize(CastleChunk chunk)
        {
            var faction = this.DeserializeFaction(chunk);

            return new CastleModel(
                chunk.Name,
                faction,
                this.DeserializeOwner(chunk),
                this.DeserializePosition(chunk),
                this.DeserializeGarrison(chunk),
                DeserializeBuildings(chunk, faction, CastleLayout.BuiltBuildingsFromNameEnd),
                this.DeserializeShipyardLocation(chunk),
                DeserializeHeroIndex(chunk, CastleLayout.GarrisonedHero),
                DeserializeHeroIndex(chunk, CastleLayout.VisitingHero),
                DeserializeBuildings(chunk, faction, CastleLayout.EnabledBuildingsFromNameEnd),
                chunk.GetByteAt(CastleLayout.BuiltThisTurn) != 0,
                chunk.GetByteAt(CastleLayout.MageGuildLevel),
                DeserializeCreaturesAvailable(chunk, CastleLayout.CreaturesAvailableFromNameEnd),
                DeserializeCreaturesAvailable(chunk, CastleLayout.UpgradedCreaturesAvailableFromNameEnd),
                DeserializeBuildings(chunk, faction, CastleLayout.CurrentBuildingsFromNameEnd),
                DeserializeSpellCounts(chunk),
                DeserializeMageGuildSpells(chunk));
        }

        public Faction? DeserializeFaction(CastleChunk chunk)
        {
            return AbstractEnumeration.FromCode<Faction>(chunk.GetByteAt(CastleLayout.Faction));
        }

        public PlayerColor? DeserializeOwner(CastleChunk chunk)
        {
            // TODO:LOG info, when the byte is neither a colour nor FF: the castle "{chunk.Name}" has owner byte {value}, which reads as no owner and is kept.
            return ByteEnum.TryRead<PlayerColor>(chunk.GetByteAt(CastleLayout.Owner), out var owner) ? owner : null;
        }

        public Position DeserializePosition(CastleChunk chunk)
        {
            return new Position(
                chunk.GetByteAt(CastleLayout.PositionX),
                chunk.GetByteAt(CastleLayout.PositionY),
                chunk.GetByteAt(CastleLayout.PositionZ));
        }

        public Position? DeserializeShipyardLocation(CastleChunk chunk)
        {
            var x = chunk.GetByteAt(CastleLayout.ShipyardX);
            var y = chunk.GetByteAt(CastleLayout.ShipyardY);
            return x == byte.MaxValue && y == byte.MaxValue ? null : new Position(x, y, chunk.GetByteAt(CastleLayout.PositionZ));
        }

        public Army DeserializeGarrison(CastleChunk chunk)
        {
            var slots = new List<ArmySlot>();

            for (var i = 0; i < Constants.CreatureSlotCount; i++)
            {
                var type = chunk.GetInt32At(CastleLayout.GarrisonTypes + (i * 4));
                if (type < 0 || type > byte.MaxValue || Creature.FromByte((byte)type) == null)
                {
                    // TODO:LOG warning: the castle "{chunk.Name}" holds creature type {type} in garrison slot {i + 1}, which this editor does not know; the slot reads as empty and its bytes are kept (-1 is a slot that is empty, not to be logged).
                    continue;
                }

                // Slots are 1 - 7.
                slots.Add(new ArmySlot(i + 1, (byte)type, chunk.GetUInt32At(CastleLayout.GarrisonCounts + (i * 4))));
            }

            return new Army([.. slots]);
        }

        /// <summary>
        /// Returns every building the castle has built, including the ones an upgrade has replaced.
        /// </summary>
        public IReadOnlySet<Building> DeserializeBuiltBuildings(CastleChunk chunk)
        {
            return DeserializeBuildings(chunk, this.DeserializeFaction(chunk), CastleLayout.BuiltBuildingsFromNameEnd);
        }

        /// <summary>
        /// Returns the buildings that are in the castle now, which leaves out the ones an upgrade has replaced.
        /// </summary>
        public IReadOnlySet<Building> DeserializeCurrentBuildings(CastleChunk chunk)
        {
            return DeserializeBuildings(chunk, this.DeserializeFaction(chunk), CastleLayout.CurrentBuildingsFromNameEnd);
        }

        private static int? DeserializeHeroIndex(CastleChunk chunk, int offset)
        {
            var index = chunk.GetByteAt(offset);
            return index == byte.MaxValue ? null : index;
        }

        private static int[] DeserializeSpellCounts(CastleChunk chunk)
        {
            var counts = new int[CastleLayout.SpellLevelCount];
            for (var level = 0; level < counts.Length; level++)
            {
                counts[level] = chunk.GetByteAt(chunk.NameEnd + CastleLayout.SpellCountsFromNameEnd + level);
            }

            return counts;
        }

        /// <summary>
        /// Reads the spells the guild holds or would hold on each level: as many as its stored count, but at least five on level 1
        /// down to one on level 5, which a guild gets when it is built (the count is 0 until then). A slot that holds no spell the editor knows
        /// (-1 for a level the faction cannot build, or a code that is no spell) is left out, and a chunk that ends before the lists
        /// (a savefile cut short) has none.
        /// </summary>
        private static IReadOnlyList<Spell>[] DeserializeMageGuildSpells(CastleChunk chunk)
        {
            var levels = new IReadOnlyList<Spell>[CastleLayout.SpellLevelCount];
            for (var level = 1; level <= levels.Length; level++)
            {
                var spells = new List<Spell>();
                for (var slot = 0; slot < MageGuildSpellSlots.Count(chunk, level); slot++)
                {
                    var offset = MageGuildSpellSlots.Offset(chunk, level, slot);
                    var code = offset < 0 ? -1 : chunk.GetInt32At(offset);
                    var spell = code is >= 0 and <= byte.MaxValue ? AbstractEnumeration.FromCode<Spell>((byte)code) : null;
                    if (spell != null)
                    {
                        spells.Add(spell);
                    }

                    // TODO:LOG warning, when spell is null and code is not -1: the castle "{chunk.Name}" holds spell code {code} in its level {level} Mage Guild slot {slot}, which this editor does not know; the slot is left out and its bytes are kept.
                }

                levels[level - 1] = spells;
            }

            return levels;
        }

        private static int[] DeserializeCreaturesAvailable(CastleChunk chunk, int offsetFromNameEnd)
        {
            var counts = new int[CastleLayout.CreatureLevelCount];
            for (var level = 0; level < counts.Length; level++)
            {
                counts[level] = chunk.GetUInt16At(chunk.NameEnd + offsetFromNameEnd + (level * 2));
            }

            return counts;
        }

        private static HashSet<Building> DeserializeBuildings(CastleChunk chunk, Faction? faction, int offsetFromNameEnd)
        {
            var mask = chunk.GetUInt64At(chunk.NameEnd + offsetFromNameEnd);
            var buildings = new HashSet<Building>();

            if (faction == null)
            {
                return buildings;
            }

            foreach (var building in Building.ForFaction(faction))
            {
                if (building.Code < 64 && (mask & (1UL << building.Code)) != 0)
                {
                    buildings.Add(building);
                }
            }

            return buildings;
        }
    }
}
