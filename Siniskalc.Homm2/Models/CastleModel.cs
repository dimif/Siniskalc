namespace Siniskalc.Homm2.Models
{
    using System;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Core.Models;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// A castle or a town of Heroes of Might and Magic II as the savegame has it. The table has 72 records and a map uses the first few;
    /// the others are empty (no name).
    /// </summary>
    /// <param name="Name">The name of the castle, empty for a record the map does not use.</param>
    /// <param name="Faction">The faction of the town; a savegame number that is no faction reads as the faction <c>Neutral</c>.</param>
    /// <param name="Owner">The player who owns the castle, or the colour <c>Neutral</c> for nobody.</param>
    /// <param name="Position">The tile of the castle.</param>
    /// <param name="ShipyardPosition">The tile a ship built in the castle appears on, or <c>null</c> for a castle that is not on the coast.</param>
    /// <param name="Garrison">The five slots of the army that defends the castle when no hero is in it.</param>
    /// <param name="VisitingHeroId">The number of the hero in the castle, or <c>null</c> (a hero only visits: there are no garrison heroes).</param>
    /// <param name="CreaturesAvailable">The creatures that wait in each of the six dwellings, whether it is built as it was or upgraded: the savegame keeps the count in the list of its own or in the one of the upgraded dwellings, which is up to the level the dwelling is built to.</param>
    /// <param name="Index">The number of the castle, which is its place in the table.</param>
    /// <param name="Town">The buildings that are not dwellings.</param>
    /// <param name="Dwellings">The dwellings and the level of the Mage Guild.</param>
    /// <param name="MageGuildSpellCounts">How many of the four spells of each level the guild shows.</param>
    /// <param name="MageGuildSpells">The spell numbers (1 to 65, 0 for none) of the Mage Guild: four for each of the five levels, the first level first.</param>
    public sealed record CastleModel(
        string Name,
        Faction Faction,
        PlayerColor Owner,
        MapPosition Position,
        MapPosition? ShipyardPosition,
        IArmy<Creature> Garrison,
        int? VisitingHeroId,
        Numbers CreaturesAvailable,
        int Index,
        TownBuildings Town,
        Dwellings Dwellings,
        Numbers MageGuildSpellCounts,
        Numbers MageGuildSpells)
        : ICastleModel<PlayerColor, Creature, Faction>
    {
        private static readonly int[] BaseGuildSpells = [3, 3, 2, 2, 1];

        /// <inheritdoc/>
        public bool IsUsed => this.Name.Length > 0;

        public bool HasCastle => this.Town.HasFlag(TownBuildings.Castle);

        /// <summary>
        /// Gets the number of the hero that garrisons the castle: none, as the castles of Heroes of Might and Magic II have no garrison hero.
        /// </summary>
        public int? GarrisonedHeroId => null;

        /// <summary>
        /// Gets how many of the six dwellings are built, as the dwelling or as an upgrade of it.
        /// </summary>
        public int DwellingCount => Enumerable.Range(1, Constants.Castle.DwellingCount).Count(d => this.DwellingLevel(d) > 0);

        /// <summary>
        /// Gets the level of the Mage Guild, 0 when there is none: the number in the bits 16 to 18 of the dwellings.
        /// </summary>
        public int MageGuildLevel => (int)(((uint)this.Dwellings & (uint)Dwellings.MageGuildLevelMask) >> 16);

        /// <summary>
        /// Gets a value indicating whether the castle is a Wizard castle with its Library, which gives each level of the Mage Guild a
        /// fourth spell.
        /// </summary>
        public bool HasLibrary => this.Faction == Entities.Faction.Wizard && this.Town.HasFlag(TownBuildings.Special);

        /// <summary>
        /// Gets the most levels a dwelling can have in a town of the faction: the creatures of that level, one for the dwelling and one more for
        /// each upgrade (the first dwelling has none).
        /// </summary>
        public static int MaxDwellingLevel(Faction faction, int dwelling)
        {
            RangeGuard.ThrowIfOutOfRange(dwelling, 1, Constants.Castle.DwellingCount, nameof(dwelling));
            return faction == Entities.Faction.Neutral || dwelling == 1 ? 1 : Math.Clamp(Creature.ForFaction(faction).Count(c => c.Tier == dwelling), 1, 3);
        }

        /// <summary>
        /// Gets how far dwelling <paramref name="dwelling"/> (1 to 6) is built: 0 when it is not, 1 for the dwelling, 2 once it is upgraded and
        /// 3 for the sixth when it is upgraded twice.
        /// </summary>
        public int DwellingLevel(int dwelling) => DwellingLevel(this.Dwellings, dwelling);

        /// <summary>
        /// Gets how far dwelling <paramref name="dwelling"/> (1 to 6) is built in a mask of the dwellings: 0 when it is not, 1 for the dwelling,
        /// 2 once it is upgraded and 3 for the sixth when it is upgraded twice.
        /// </summary>
        public static int DwellingLevel(Dwellings dwellings, int dwelling)
        {
            RangeGuard.ThrowIfOutOfRange(dwelling, 1, Constants.Castle.DwellingCount, nameof(dwelling));
            var bits = (uint)dwellings;
            return dwelling == 6 && (bits & (uint)Dwellings.Dwelling6UpgradedTwice) != 0 ? 3
                : dwelling >= 2 && (bits & (0x100u << (dwelling - 1))) != 0 ? 2
                : (bits & (0x08u << (dwelling - 1))) != 0 ? 1
                : 0;
        }

        /// <summary>
        /// Returns the castle with everything of dwelling <paramref name="dwelling"/> (1 to 6) as it is in <paramref name="source"/>: its
        /// bits of the dwellings and the creatures that wait in it, wherever the savegame keeps them.
        /// </summary>
        public CastleModel WithDwellingFrom(CastleModel source, int dwelling)
        {
            RangeGuard.ThrowIfOutOfRange(dwelling, 1, Constants.Castle.DwellingCount, nameof(dwelling));
            var clear = DwellingBits(dwelling);
            return this with
            {
                Dwellings = (Dwellings)(((uint)this.Dwellings & ~clear) | ((uint)source.Dwellings & clear)),
                CreaturesAvailable = this.CreaturesAvailable.With(dwelling - 1, source.CreaturesAvailable[dwelling - 1]),
            };
        }

        /// <summary>
        /// Gets a value indicating whether dwelling <paramref name="dwelling"/> (1 to 6) is the same in <paramref name="other"/>: built to
        /// the same level, with the same creatures waiting wherever the savegame keeps them.
        /// </summary>
        public bool HasSameDwelling(CastleModel other, int dwelling) => this.WithDwellingFrom(other, dwelling).Equals(this);

        /// <summary>
        /// Returns the castle with its Mage Guild as it is in <paramref name="source"/>: its level, the building and the number of
        /// spells each level shows (not the spells themselves).
        /// </summary>
        public CastleModel WithMageGuildFrom(CastleModel source) => this with
        {
            Dwellings = (Dwellings)(((uint)this.Dwellings & ~(uint)Dwellings.MageGuildLevelMask) | ((uint)source.Dwellings & (uint)Dwellings.MageGuildLevelMask)),
            Town = (this.Town & ~TownBuildings.MageGuild) | (source.Town & TownBuildings.MageGuild),
            MageGuildSpellCounts = source.MageGuildSpellCounts,
        };

        /// <summary>
        /// Gets a value indicating whether the Mage Guild is the same in <paramref name="other"/>.
        /// </summary>
        public bool HasSameMageGuild(CastleModel other) => this.WithMageGuildFrom(other).Equals(this);

        public bool HasDwelling(int dwelling) => this.DwellingLevel(dwelling) > 0;

        /// <summary>
        /// Returns the castle with the building built or taken down. Building the castle takes away "town only" and taking it down puts it
        /// back, as the game has it. The other bits of the mask are kept.
        /// </summary>
        public CastleModel WithBuilding(TownBuildings building, bool built)
        {
            if (building == TownBuildings.Castle)
            {
                return this with { Town = built ? (this.Town | TownBuildings.Castle) & ~TownBuildings.TownOnly : (this.Town & ~TownBuildings.Castle) | TownBuildings.TownOnly };
            }

            return this with { Town = built ? this.Town | building : this.Town & ~building };
        }

        /// <summary>
        /// Returns the castle with the Library of a Wizard built or taken down, and the spells each level of its Mage Guild shows changed
        /// with it. Nothing changes in a castle of another faction.
        /// </summary>
        public CastleModel WithLibrary(bool built)
        {
            if (this.Faction != Entities.Faction.Wizard)
            {
                return this;
            }

            var town = this.WithBuilding(TownBuildings.Special, built);
            return town with { MageGuildSpellCounts = GuildCounts(town.MageGuildLevel, built) };
        }

        /// <summary>
        /// Returns the castle with dwelling <paramref name="dwelling"/> (1 to 6) at <paramref name="level"/>: 0 for none, 1 for the dwelling,
        /// 2 and 3 for the upgrades. An upgrade replaces the bit of the dwelling, as the game has it. The creatures that wait stay.
        /// </summary>
        public CastleModel WithDwellingLevel(int dwelling, int level)
        {
            RangeGuard.ThrowIfOutOfRange(level, 0, MaxDwellingLevel(this.Faction, dwelling), nameof(level));
            var clear = DwellingBits(dwelling);
            var bit = level switch
            {
                0 => 0u,
                1 => 0x08u << (dwelling - 1),
                2 => 0x100u << (dwelling - 1),
                _ => (uint)Dwellings.Dwelling6UpgradedTwice,
            };
            return this with { Dwellings = (Dwellings)(((uint)this.Dwellings & ~clear) | bit) };
        }

        /// <summary>
        /// Returns the castle with dwelling <paramref name="dwelling"/> (1 to 6) built (as the dwelling, or as it was when it is built
        /// already) or taken down. The creatures that wait in it stay.
        /// </summary>
        public CastleModel WithDwelling(int dwelling, bool built) =>
            built ? (this.DwellingLevel(dwelling) > 0 ? this : this.WithDwellingLevel(dwelling, 1)) : this.WithDwellingLevel(dwelling, 0);

        // The bits of the dwellings that belong to dwelling 1 to 6: the dwelling, its upgrade and (for the sixth) the second upgrade.
        private static uint DwellingBits(int dwelling) =>
            (0x08u << (dwelling - 1)) | (dwelling >= 2 ? 0x100u << (dwelling - 1) : 0u) | (dwelling == 6 ? (uint)Dwellings.Dwelling6UpgradedTwice : 0u);

        /// <summary>
        /// Gets the creatures that wait in dwelling <paramref name="dwelling"/> (1 to 6), wherever the savegame keeps them for its level.
        /// </summary>
        public int CreaturesWaiting(int dwelling)
        {
            RangeGuard.ThrowIfOutOfRange(dwelling, 1, Constants.Castle.DwellingCount, nameof(dwelling));
            return this.CreaturesAvailable[dwelling - 1];
        }

        /// <summary>
        /// Returns the castle with <paramref name="count"/> creatures waiting in dwelling <paramref name="dwelling"/> (1 to 6).
        /// </summary>
        public CastleModel WithCreaturesWaiting(int dwelling, int count)
        {
            RangeGuard.ThrowIfOutOfRange(dwelling, 1, Constants.Castle.DwellingCount, nameof(dwelling));
            return this with { CreaturesAvailable = this.CreaturesAvailable.With(dwelling - 1, count) };
        }

        /// <summary>
        /// Returns the castle with the Mage Guild at <paramref name="level"/> (0 to 5). The level is a number in the bits 16 to 18 of the
        /// dwellings, the bit of the buildings tells that there is a guild, and each level that is built shows 3, 3, 2, 2 and 1 spells (one more
        /// with the Library of a Wizard); the levels that are not built show none.
        /// </summary>
        public CastleModel WithMageGuildLevel(int level)
        {
            RangeGuard.ThrowIfOutOfRange(level, 0, Constants.Castle.MageGuildLevels, nameof(level));
            var dwellings = ((uint)this.Dwellings & ~(uint)Dwellings.MageGuildLevelMask) | ((uint)level << 16);
            return this with
            {
                Dwellings = (Dwellings)dwellings,
                Town = level == 0 ? this.Town & ~TownBuildings.MageGuild : this.Town | TownBuildings.MageGuild,
                MageGuildSpellCounts = GuildCounts(level, this.HasLibrary),
            };
        }

        /// <summary>
        /// Returns the castle with another spell (or none) in a place of the Mage Guild, at <paramref name="level"/> (1 to 5) and <paramref name="slot"/> (0 to 3).
        /// </summary>
        public CastleModel WithMageGuildSpell(int level, int slot, Spell? spell)
        {
            RangeGuard.ThrowIfOutOfRange(level, 1, Constants.Castle.MageGuildLevels, nameof(level));
            RangeGuard.ThrowIfOutOfRange(slot, 0, Constants.Castle.MageGuildSlotsPerLevel - 1, nameof(slot));
            return this with { MageGuildSpells = this.MageGuildSpells.With(((level - 1) * Constants.Castle.MageGuildSlotsPerLevel) + slot, spell?.Code ?? 0) };
        }

        private static Numbers GuildCounts(int level, bool library) =>
            new(BaseGuildSpells.Select((count, i) => i < level ? count + (library ? 1 : 0) : 0));

        /// <summary>
        /// Returns the spell of the Mage Guild at <paramref name="level"/> (1 to 5) and <paramref name="slot"/> (0 to 3), or <c>null</c> for none.
        /// </summary>
        public Spell? MageGuildSpell(int level, int slot) => Spell.FromByte((byte)this.MageGuildSpells[((level - 1) * Constants.Castle.MageGuildSlotsPerLevel) + slot]);
    }
}
