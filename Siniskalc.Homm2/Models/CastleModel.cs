namespace Siniskalc.Homm2.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// A castle or a town of Heroes of Might and Magic II as the savegame has it. The table has 72 records and a map uses the first few;
    /// the others are empty (no name).
    /// </summary>
    /// <param name="Index">The number of the castle, which is its place in the table.</param>
    /// <param name="Name">The name of the castle, empty for a record the map does not use.</param>
    /// <param name="Owner">The player who owns the castle, or <c>null</c> for nobody.</param>
    /// <param name="Faction">The faction of the town, or <c>null</c> when the savegame has a number that is no faction.</param>
    /// <param name="Position">The tile of the castle.</param>
    /// <param name="Boat">The tile a ship built in the castle appears on, or <c>null</c> for a castle that is not on the coast.</param>
    /// <param name="Garrison">The five slots of the army that defends the castle when no hero is in it.</param>
    /// <param name="VisitingHero">The number of the hero in the castle, or <c>null</c> (a hero only visits: there are no garrison heroes).</param>
    /// <param name="Town">The buildings that are not dwellings.</param>
    /// <param name="Dwellings">The dwellings and the level of the Mage Guild.</param>
    /// <param name="Creatures">The creatures that wait in each of the six dwellings, while they are not upgraded.</param>
    /// <param name="UpgradedCreatures">The creatures that wait in the dwellings 2 to 6 once they are upgraded, and in the sixth upgraded twice (an upgraded dwelling keeps its count here).</param>
    /// <param name="GuildSpells">The spell numbers (1 to 65, 0 for none) of the Mage Guild: four for each of the five levels, the first level first.</param>
    /// <param name="GuildSpellCounts">How many of the four spells of each level the guild shows.</param>
    public sealed record CastleModel(
        int Index,
        string Name,
        Color? Owner,
        Faction? Faction,
        Position Position,
        Position? Boat,
        Army Garrison,
        int? VisitingHero,
        TownBuildings Town,
        Dwellings Dwellings,
        Numbers Creatures,
        Numbers UpgradedCreatures,
        Numbers GuildSpells,
        Numbers GuildSpellCounts)
    {
        // The spells a level of the Mage Guild shows: 3, 3, 2, 2 and 1 (the savegames of Broken Alliance: Xabran with level 2, Dragontooth with level 3, Peacekeeper with level 5).
        private static readonly int[] BaseGuildSpells = [3, 3, 2, 2, 1];

        /// <summary>
        /// Gets a value indicating whether the map uses the castle.
        /// </summary>
        public bool IsUsed => this.Name.Length > 0;

        public bool HasCastle => this.Town.HasFlag(TownBuildings.Castle);

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
        public static int MaxDwellingLevel(Faction? faction, int dwelling)
        {
            RangeGuard.ThrowIfOutOfRange(dwelling, 1, Constants.Castle.DwellingCount, nameof(dwelling));
            return faction is null || dwelling == 1 ? 1 : Math.Clamp(Creature.ForFaction(faction.Value).Count(c => c.Tier == dwelling), 1, 3);
        }

        /// <summary>
        /// Gets how far dwelling <paramref name="dwelling"/> (1 to 6) is built: 0 when it is not, 1 for the dwelling, 2 once it is upgraded and
        /// 3 for the sixth when it is upgraded twice.
        /// </summary>
        public int DwellingLevel(int dwelling)
        {
            RangeGuard.ThrowIfOutOfRange(dwelling, 1, Constants.Castle.DwellingCount, nameof(dwelling));
            var bits = (uint)this.Dwellings;
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
            var upgraded = this.UpgradedCreatures;
            foreach (var place in UpgradedPlaces(dwelling))
            {
                upgraded = upgraded.With(place, source.UpgradedCreatures[place]);
            }

            return this with
            {
                Dwellings = (Dwellings)(((uint)this.Dwellings & ~clear) | ((uint)source.Dwellings & clear)),
                Creatures = this.Creatures.With(dwelling - 1, source.Creatures[dwelling - 1]),
                UpgradedCreatures = upgraded,
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
            GuildSpellCounts = source.GuildSpellCounts,
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
            return town with { GuildSpellCounts = GuildCounts(town.MageGuildLevel, built) };
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
            var waiting = this.Waiting(dwelling);
            var changed = this with { Dwellings = (Dwellings)(((uint)this.Dwellings & ~clear) | bit) };
            if (level == 0)
            {
                return changed;
            }

            // the count moves with the dwelling: an upgraded one keeps it in its own list and leaves 0 in the other places
            var moved = changed.WithWaiting(dwelling, waiting);
            var plain = level == 1 ? moved.Creatures : moved.Creatures.With(dwelling - 1, 0);
            var upgraded = moved.UpgradedCreatures;
            for (var place = 0; place < upgraded.Count; place++)
            {
                if (dwelling >= 2 && (level == 1 || place != UpgradedPlace(dwelling, level)) && (place == dwelling - 2 || (dwelling == 6 && place == 5)))
                {
                    upgraded = upgraded.With(place, 0);
                }
            }

            return moved with { Creatures = plain, UpgradedCreatures = WithMarker(upgraded, level) };
        }

        /// <summary>
        /// Returns the castle with dwelling <paramref name="dwelling"/> (1 to 6) built (as the dwelling, or as it was when it is built
        /// already) or taken down. The creatures that wait in it stay.
        /// </summary>
        public CastleModel WithDwelling(int dwelling, bool built) =>
            built ? (this.DwellingLevel(dwelling) > 0 ? this : this.WithDwellingLevel(dwelling, 1)) : this.WithDwellingLevel(dwelling, 0);

        // The place of the upgraded list that the game also fills for the dragons upgraded twice (3 in the saves it made, 0 in E35 where the dragons did not show).
        private const int SecondUpgradeMarker = 4;
        private const int SecondUpgradeMarkerValue = 3;

        // E35, E36, E37: the game shows no dragons of a dwelling upgraded twice while the fifth place is 0.
        private static Numbers WithMarker(Numbers upgraded, int level) =>
            level >= 3 && upgraded[SecondUpgradeMarker] == 0 ? upgraded.With(SecondUpgradeMarker, SecondUpgradeMarkerValue) : upgraded;

        // The bits of the dwellings that belong to dwelling 1 to 6: the dwelling, its upgrade and (for the sixth) the second upgrade.
        private static uint DwellingBits(int dwelling) =>
            (0x08u << (dwelling - 1)) | (dwelling >= 2 ? 0x100u << (dwelling - 1) : 0u) | (dwelling == 6 ? (uint)Dwellings.Dwelling6UpgradedTwice : 0u);

        // The places of the list of upgraded creatures that belong to a dwelling: its own and, for the sixth, the one of the second upgrade.
        private static IEnumerable<int> UpgradedPlaces(int dwelling)
        {
            if (dwelling >= 2)
            {
                yield return dwelling - 2;
            }

            if (dwelling == 6)
            {
                yield return 5;
            }
        }

        // Where the creatures of an upgraded dwelling wait: the dwellings 2 to 6 have a place each, the dragons upgraded a second time the sixth
        // (E34DWUP2: the Black Dragon had none in the fifth).
        private static int UpgradedPlace(int dwelling, int level) => level >= 3 ? 5 : dwelling - 2;

        /// <summary>
        /// Gets the creatures that wait in dwelling <paramref name="dwelling"/> (1 to 6), wherever the savegame keeps them for its level.
        /// </summary>
        public int Waiting(int dwelling)
        {
            var level = this.DwellingLevel(dwelling);
            return level >= 2 ? this.UpgradedCreatures[UpgradedPlace(dwelling, level)] : this.Creatures[dwelling - 1];
        }

        /// <summary>
        /// Returns the castle with <paramref name="count"/> creatures waiting in dwelling <paramref name="dwelling"/> (1 to 6).
        /// </summary>
        public CastleModel WithWaiting(int dwelling, int count)
        {
            var level = this.DwellingLevel(dwelling);
            if (level < 2)
            {
                return this with { Creatures = this.Creatures.With(dwelling - 1, count) };
            }

            return this with { UpgradedCreatures = WithMarker(this.UpgradedCreatures.With(UpgradedPlace(dwelling, level), count), level) };
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
                GuildSpellCounts = GuildCounts(level, this.HasLibrary),
            };
        }

        /// <summary>
        /// Returns the castle with another spell (or none) in a place of the Mage Guild, at <paramref name="level"/> (1 to 5) and <paramref name="slot"/> (0 to 3).
        /// </summary>
        public CastleModel WithGuildSpell(int level, int slot, Spell? spell)
        {
            RangeGuard.ThrowIfOutOfRange(level, 1, Constants.Castle.MageGuildLevels, nameof(level));
            RangeGuard.ThrowIfOutOfRange(slot, 0, Constants.Castle.MageGuildSlotsPerLevel - 1, nameof(slot));
            return this with { GuildSpells = this.GuildSpells.With(((level - 1) * Constants.Castle.MageGuildSlotsPerLevel) + slot, spell?.Code ?? 0) };
        }

        private static Numbers GuildCounts(int level, bool library) =>
            new(BaseGuildSpells.Select((count, i) => i < level ? count + (library ? 1 : 0) : 0));

        /// <summary>
        /// Returns the spell of the Mage Guild at <paramref name="level"/> (1 to 5) and <paramref name="slot"/> (0 to 3), or <c>null</c> for none.
        /// </summary>
        public Spell? GuildSpell(int level, int slot) => Spell.FromByte((byte)this.GuildSpells[((level - 1) * Constants.Castle.MageGuildSlotsPerLevel) + slot]);
    }
}
