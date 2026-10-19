namespace Siniskalc.Homm3.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Reads a <see cref="CastleModel"/> from a <see cref="CastleChunk"/>.
    /// </summary>
    public sealed class CastleDeserializer : ICastleDeserializer
    {
        public CastleModel Deserialize(CastleChunk chunk)
        {
            var faction = this.DeserializeFaction(chunk);
            var built = DeserializeBuildings(chunk, faction, CastleLayout.BuiltBuildingsFromNameEnd);

            return new CastleModel(
                chunk.Name,
                faction,
                this.DeserializeOwner(chunk),
                this.DeserializePosition(chunk),
                this.DeserializeShipyardLocation(chunk),
                this.DeserializeGarrison(chunk),
                DeserializeHeroIndex(chunk, CastleLayout.VisitingHero),
                DeserializeCreaturesAvailable(chunk, faction, built),
                DeserializeHeroIndex(chunk, CastleLayout.GarrisonedHero),
                DeserializeBuildings(chunk, faction, CastleLayout.EnabledBuildingsFromNameEnd),
                built,
                DeserializeBuildings(chunk, faction, CastleLayout.VisibleBuildingsFromNameEnd),
                chunk.GetByteAt(CastleLayout.BuiltThisTurn) != 0,
                DeserializeSpellCounts(chunk),
                DeserializeMageGuildSpells(chunk));
        }

        public Faction DeserializeFaction(CastleChunk chunk)
        {
            return Faction.FromByte(chunk.GetByteAt(CastleLayout.Faction));
        }

        public PlayerColor DeserializeOwner(CastleChunk chunk)
        {
            // TODO:LOG info, when the byte is neither a colour nor FF: the castle "{chunk.Name}" has owner byte {value}, which reads as neutral and is kept.
            return PlayerColor.FromByte(chunk.GetByteAt(CastleLayout.Owner));
        }

        public MapPosition DeserializePosition(CastleChunk chunk)
        {
            return new MapPosition(
                chunk.GetByteAt(CastleLayout.PositionX),
                chunk.GetByteAt(CastleLayout.PositionY),
                chunk.GetByteAt(CastleLayout.PositionZ));
        }

        public MapPosition? DeserializeShipyardLocation(CastleChunk chunk)
        {
            var x = chunk.GetByteAt(CastleLayout.ShipyardX);
            var y = chunk.GetByteAt(CastleLayout.ShipyardY);
            return x == byte.MaxValue && y == byte.MaxValue ? null : new MapPosition(x, y, chunk.GetByteAt(CastleLayout.PositionZ));
        }

        public IArmy<Creature> DeserializeGarrison(CastleChunk chunk) => ArmyReader.Read(chunk, CastleLayout.Garrison, CreatureCodes.Instance);

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
        public IReadOnlySet<Building> DeserializeVisibleBuildings(CastleChunk chunk)
        {
            return DeserializeBuildings(chunk, this.DeserializeFaction(chunk), CastleLayout.VisibleBuildingsFromNameEnd);
        }

        private static int? DeserializeHeroIndex(CastleChunk chunk, int offset)
        {
            var index = chunk.GetByteAt(offset);
            return index == byte.MaxValue ? null : index;
        }

        private static Numbers DeserializeSpellCounts(CastleChunk chunk)
        {
            var counts = new int[CastleLayout.SpellLevelCount];
            for (var level = 0; level < counts.Length; level++)
            {
                counts[level] = chunk.GetByteAt(chunk.NameEnd + CastleLayout.SpellCountsFromNameEnd + level);
            }

            return new Numbers(counts);
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

        // The savefile has a count for the dwelling and one for its upgrade of each level, and keeps the count in the one that is built.
        private static Numbers DeserializeCreaturesAvailable(CastleChunk chunk, Faction faction, IReadOnlySet<Building> built)
        {
            var counts = new int[CastleLayout.CreatureLevelCount];
            for (var level = 0; level < counts.Length; level++)
            {
                var upgrade = faction == Faction.Neutral ? null : Building.FromDwellingLevel(faction, level, upgraded: true);
                var from = upgrade != null && built.Contains(upgrade) ? CastleLayout.UpgradedCreaturesAvailableFromNameEnd : CastleLayout.CreaturesAvailableFromNameEnd;
                counts[level] = chunk.GetUInt16At(chunk.NameEnd + from + (level * 2));
            }

            return new Numbers(counts);
        }

        private static HashSet<Building> DeserializeBuildings(CastleChunk chunk, Faction faction, int offsetFromNameEnd)
        {
            var mask = chunk.GetUInt64At(chunk.NameEnd + offsetFromNameEnd);
            var buildings = new HashSet<Building>();

            if (faction == Faction.Neutral)
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
