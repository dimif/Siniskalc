namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Entities;
    using Siniskalc.Core.Models;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Reads a <see cref="CastleModel"/> from a <see cref="CastleChunk"/>.
    /// </summary>
    public sealed class CastleDeserializer : ICastleDeserializer
    {
        private static readonly ILogger<CastleDeserializer> Logger = LogFactory.For<CastleDeserializer>();
        private readonly EntityCatalog catalog;
        private readonly CreatureCodes creatureCodes;

        /// <summary>
        /// Initializes a new instance of the <see cref="CastleDeserializer"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        public CastleDeserializer(EntityCatalog catalog)
        {
            this.catalog = catalog;
            this.creatureCodes = new CreatureCodes(catalog);
        }

        /// <inheritdoc/>
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
                this.DeserializeMageGuildSpells(chunk));
        }

        /// <summary>
        /// Reads the faction of the castle from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the castle.</param>
        /// <returns>The faction; the neutral faction when the byte is no faction.</returns>
        public Faction DeserializeFaction(CastleChunk chunk)
        {
            var code = chunk.GetByteAt(CastleLayout.Faction);
            var faction = this.catalog.Factions.Find(code);
            if (faction is null && code != Constants.ByteOff)
            {
                using var subject = Logger.BeginSubject("Castle", chunk.Name);
                Logger.LogTrace("Faction byte {Code:X2} read as {StandIn} and kept until the faction is changed", code, this.catalog.NeutralFaction.Name);
            }

            return faction ?? this.catalog.NeutralFaction;
        }

        /// <summary>
        /// Reads the player who owns the castle from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the castle.</param>
        /// <returns>The colour of the player; the neutral colour when nobody owns it or the byte is no colour.</returns>
        public PlayerColor DeserializeOwner(CastleChunk chunk)
        {
            var code = chunk.GetByteAt(CastleLayout.Owner);
            if (code != Constants.ByteOff && AbstractEnumeration.FromCode<PlayerColor>(code) is null)
            {
                using var subject = Logger.BeginSubject("Castle", chunk.Name);
                Logger.LogTrace("Owner byte {Code:X2} is no colour, read as neutral and kept", code);
            }

            return PlayerColor.FromByte(code);
        }

        /// <summary>
        /// Reads where the castle is on the map from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the castle.</param>
        /// <returns>The position, with the level of the map.</returns>
        public MapPosition DeserializePosition(CastleChunk chunk)
        {
            return new MapPosition(
                chunk.GetByteAt(CastleLayout.PositionX),
                chunk.GetByteAt(CastleLayout.PositionY),
                chunk.GetByteAt(CastleLayout.PositionZ));
        }

        /// <summary>
        /// Reads where the boats of the castle appear from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the castle.</param>
        /// <returns>The position, or <see langword="null"/> when the castle is not on the coast.</returns>
        public MapPosition? DeserializeShipyardLocation(CastleChunk chunk)
        {
            var x = chunk.GetByteAt(CastleLayout.ShipyardX);
            var y = chunk.GetByteAt(CastleLayout.ShipyardY);
            return x == byte.MaxValue && y == byte.MaxValue ? null : new MapPosition(x, y, chunk.GetByteAt(CastleLayout.PositionZ));
        }

        /// <summary>
        /// Reads the garrison of the castle from the chunk.
        /// </summary>
        /// <param name="chunk">The chunk of the castle.</param>
        /// <returns>The garrison.</returns>
        public IArmy<Creature> DeserializeGarrison(CastleChunk chunk)
        {
            using var subject = Logger.BeginSubject("Castle", chunk.Name);
            return ArmyReader.Read(chunk, CastleLayout.Garrison, this.creatureCodes);
        }

        /// <summary>
        /// Returns every building the castle has built, including the ones an upgrade has replaced.
        /// </summary>
        /// <param name="chunk">The chunk.</param>
        /// <returns>The buildings that have been built, including those an upgrade replaced.</returns>
        public IReadOnlySet<Building> DeserializeBuiltBuildings(CastleChunk chunk)
        {
            return DeserializeBuildings(chunk, this.DeserializeFaction(chunk), CastleLayout.BuiltBuildingsFromNameEnd);
        }

        /// <summary>
        /// Returns the buildings that are in the castle now, which leaves out the ones an upgrade has replaced.
        /// </summary>
        /// <param name="chunk">The chunk.</param>
        /// <returns>The buildings that are in the castle now.</returns>
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

        // The savefile has a count for the dwelling and one for its upgrade of each level, and keeps the count in the one that is built.
        private static Numbers DeserializeCreaturesAvailable(CastleChunk chunk, Faction faction, IReadOnlySet<Building> built)
        {
            var counts = new int[CastleLayout.CreatureLevelCount];
            for (var level = 0; level < counts.Length; level++)
            {
                var upgrade = faction.IsNeutral ? null : Building.FromDwellingLevel(faction, level, upgraded: true);
                var from = upgrade != null && built.Contains(upgrade) ? CastleLayout.UpgradedCreaturesAvailableFromNameEnd : CastleLayout.CreaturesAvailableFromNameEnd;
                counts[level] = chunk.GetUInt16At(chunk.NameEnd + from + (level * 2));
            }

            return new Numbers(counts);
        }

        private static HashSet<Building> DeserializeBuildings(CastleChunk chunk, Faction faction, int offsetFromNameEnd)
        {
            var mask = chunk.GetUInt64At(chunk.NameEnd + offsetFromNameEnd);
            var buildings = new HashSet<Building>();

            if (faction.IsNeutral)
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

        /// <summary>
        /// Reads the spells the guild holds or would hold on each level: as many as its stored count, but at least five on level 1
        /// down to one on level 5, which a guild gets when it is built (the count is 0 until then). A slot that holds no spell the editor knows
        /// (-1 for a level the faction cannot build, or a code that is no spell) is left out, and a chunk that ends before the lists
        /// (a savefile cut short) has none.
        /// </summary>
        private IReadOnlyList<Spell>[] DeserializeMageGuildSpells(CastleChunk chunk)
        {
            using var subject = Logger.BeginSubject("Castle", chunk.Name);
            var levels = new IReadOnlyList<Spell>[CastleLayout.SpellLevelCount];
            for (var level = 1; level <= levels.Length; level++)
            {
                var spells = new List<Spell>();
                for (var slot = 0; slot < MageGuildSpellSlots.Count(chunk, level); slot++)
                {
                    var offset = MageGuildSpellSlots.Offset(chunk, level, slot);
                    var code = offset < 0 ? -1 : chunk.GetInt32At(offset);
                    var spell = code is >= 0 and <= byte.MaxValue ? this.catalog.Spells.Find((byte)code) : null;
                    if (spell != null)
                    {
                        spells.Add(spell);
                    }
                    else if (code != -1)
                    {
                        Logger.LogTrace("Spell code {Code:X2} in Mage Guild level {Level} slot {Slot} is unknown, the slot is left out and its bytes are kept", code, level, slot);
                    }
                }

                levels[level - 1] = spells;
            }

            return levels;
        }
    }
}
