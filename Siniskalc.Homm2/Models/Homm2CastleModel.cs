namespace Siniskalc.Homm2.Models
{
    using System;
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
    /// <param name="Race">The race of the town, or <c>null</c> when the savegame has a number that is no race.</param>
    /// <param name="Position">The tile of the castle.</param>
    /// <param name="Boat">The tile a ship built in the castle appears on, or <c>null</c> for a castle that is not on the coast.</param>
    /// <param name="Garrison">The five slots of the army that defends the castle when no hero is in it.</param>
    /// <param name="VisitingHero">The number of the hero in the castle, or <c>null</c> (a hero only visits: there are no garrison heroes).</param>
    /// <param name="Town">The buildings that are not dwellings.</param>
    /// <param name="Dwellings">The dwellings and the level of the Mage Guild.</param>
    /// <param name="Creatures">The creatures that wait in each of the six dwellings, while they are not upgraded.</param>
    /// <param name="UpgradedCreatures">The creatures that wait in the dwellings 2 to 6 once they are upgraded (an upgraded dwelling keeps its count here).</param>
    /// <param name="GuildSpells">The spell numbers (1 to 65, 0 for none) of the Mage Guild: four for each of the five levels, the first level first.</param>
    /// <param name="GuildSpellCounts">How many of the four spells of each level the guild shows.</param>
    public sealed record Homm2CastleModel(
        int Index,
        string Name,
        Homm2Color? Owner,
        Homm2Race? Race,
        Homm2Position Position,
        Homm2Position? Boat,
        Homm2Army Garrison,
        int? VisitingHero,
        Homm2TownBuildings Town,
        Homm2Dwellings Dwellings,
        Homm2Numbers Creatures,
        Homm2Numbers UpgradedCreatures,
        Homm2Numbers GuildSpells,
        Homm2Numbers GuildSpellCounts)
    {
        // The spells a level of the Mage Guild shows: 3, 3, 2, 2 and 1 (the savegames of Broken Alliance: Xabran with level 2, Dragontooth with level 3, Peacekeeper with level 5).
        private static readonly int[] BaseGuildSpells = [3, 3, 2, 2, 1];

        /// <summary>
        /// Gets a value indicating whether the map uses the castle.
        /// </summary>
        public bool IsUsed => this.Name.Length > 0;

        public bool HasCastle => this.Town.HasFlag(Homm2TownBuildings.Castle);

        /// <summary>
        /// Gets the level of the Mage Guild, 0 when there is none: the number in the bits 16 to 18 of the dwellings.
        /// </summary>
        public int MageGuildLevel => (int)(((uint)this.Dwellings & (uint)Homm2Dwellings.MageGuildLevelMask) >> 16);

        /// <summary>
        /// Gets a value indicating whether the castle is a Wizard castle with its Library, which gives each level of the Mage Guild a
        /// fourth spell.
        /// </summary>
        public bool HasLibrary => this.Race == Homm2Race.Wizard && this.Town.HasFlag(Homm2TownBuildings.Special);

        /// <summary>
        /// Gets the most levels a dwelling can have in a town of the race: the creatures of that level, one for the dwelling and one more for
        /// each upgrade (the first dwelling has none).
        /// </summary>
        public static int MaxDwellingLevel(Homm2Race? race, int dwelling)
        {
            RangeGuard.ThrowIfOutOfRange(dwelling, 1, Constants.Castle.DwellingCount, nameof(dwelling));
            return race is null || dwelling == 1 ? 1 : Math.Clamp(Homm2Creature.ForRace(race.Value).Count(c => c.Level == dwelling), 1, 3);
        }

        /// <summary>
        /// Gets how far dwelling <paramref name="dwelling"/> (1 to 6) is built: 0 when it is not, 1 for the dwelling, 2 once it is upgraded and
        /// 3 for the sixth when it is upgraded twice.
        /// </summary>
        public int DwellingLevel(int dwelling)
        {
            RangeGuard.ThrowIfOutOfRange(dwelling, 1, Constants.Castle.DwellingCount, nameof(dwelling));
            var bits = (uint)this.Dwellings;
            return dwelling == 6 && (bits & (uint)Homm2Dwellings.Dwelling6UpgradedTwice) != 0 ? 3
                : dwelling >= 2 && (bits & (0x100u << (dwelling - 1))) != 0 ? 2
                : (bits & (0x08u << (dwelling - 1))) != 0 ? 1
                : 0;
        }

        public bool HasDwelling(int dwelling) => this.DwellingLevel(dwelling) > 0;

        /// <summary>
        /// Returns the castle with the building built or taken down. Building the castle takes away "town only" and taking it down puts it
        /// back, as the game has it. The other bits of the mask are kept.
        /// </summary>
        public Homm2CastleModel WithBuilding(Homm2TownBuildings building, bool built)
        {
            if (building == Homm2TownBuildings.Castle)
            {
                return this with { Town = built ? (this.Town | Homm2TownBuildings.Castle) & ~Homm2TownBuildings.TownOnly : (this.Town & ~Homm2TownBuildings.Castle) | Homm2TownBuildings.TownOnly };
            }

            return this with { Town = built ? this.Town | building : this.Town & ~building };
        }

        /// <summary>
        /// Returns the castle with the Library of a Wizard built or taken down, and the spells each level of its Mage Guild shows changed
        /// with it. Nothing changes in a castle of another race.
        /// </summary>
        public Homm2CastleModel WithLibrary(bool built)
        {
            if (this.Race != Homm2Race.Wizard)
            {
                return this;
            }

            var town = this.WithBuilding(Homm2TownBuildings.Special, built);
            return town with { GuildSpellCounts = GuildCounts(town.MageGuildLevel, built) };
        }

        /// <summary>
        /// Returns the castle with dwelling <paramref name="dwelling"/> (1 to 6) at <paramref name="level"/>: 0 for none, 1 for the dwelling,
        /// 2 and 3 for the upgrades. An upgrade replaces the bit of the dwelling, as the game has it. The creatures that wait stay.
        /// </summary>
        public Homm2CastleModel WithDwellingLevel(int dwelling, int level)
        {
            RangeGuard.ThrowIfOutOfRange(level, 0, MaxDwellingLevel(this.Race, dwelling), nameof(level));
            var clear = (0x08u << (dwelling - 1)) | (dwelling >= 2 ? 0x100u << (dwelling - 1) : 0u) | (dwelling == 6 ? (uint)Homm2Dwellings.Dwelling6UpgradedTwice : 0u);
            var bit = level switch
            {
                0 => 0u,
                1 => 0x08u << (dwelling - 1),
                2 => 0x100u << (dwelling - 1),
                _ => (uint)Homm2Dwellings.Dwelling6UpgradedTwice,
            };
            var waiting = this.Waiting(dwelling);
            var changed = this with { Dwellings = (Homm2Dwellings)(((uint)this.Dwellings & ~clear) | bit) };
            if (level == 0)
            {
                return changed;
            }

            // the count moves with the dwelling: an upgraded one keeps it in its own list and leaves 0 in the slot of the plain dwelling
            var moved = changed.WithWaiting(dwelling, waiting);
            return level >= 2
                ? moved with { Creatures = moved.Creatures.With(dwelling - 1, 0) }
                : dwelling >= 2 ? moved with { UpgradedCreatures = moved.UpgradedCreatures.With(dwelling - 2, 0) } : moved;
        }

        /// <summary>
        /// Returns the castle with dwelling <paramref name="dwelling"/> (1 to 6) built (as the dwelling, or as it was when it is built
        /// already) or taken down. The creatures that wait in it stay.
        /// </summary>
        public Homm2CastleModel WithDwelling(int dwelling, bool built) =>
            built ? (this.DwellingLevel(dwelling) > 0 ? this : this.WithDwellingLevel(dwelling, 1)) : this.WithDwellingLevel(dwelling, 0);

        /// <summary>
        /// Returns the castle with <paramref name="count"/> creatures waiting in dwelling <paramref name="dwelling"/> (1 to 6).
        /// </summary>
        public int Waiting(int dwelling) => this.DwellingLevel(dwelling) >= 2 ? this.UpgradedCreatures[dwelling - 2] : this.Creatures[dwelling - 1];

        public Homm2CastleModel WithWaiting(int dwelling, int count)
        {
            return this.DwellingLevel(dwelling) >= 2
                ? this with { UpgradedCreatures = this.UpgradedCreatures.With(dwelling - 2, count) }
                : this with { Creatures = this.Creatures.With(dwelling - 1, count) };
        }

        /// <summary>
        /// Returns the castle with the Mage Guild at <paramref name="level"/> (0 to 5). The level is a number in the bits 16 to 18 of the
        /// dwellings, the bit of the buildings tells that there is a guild, and each level that is built shows 3, 3, 2, 2 and 1 spells (one more
        /// with the Library of a Wizard); the levels that are not built show none.
        /// </summary>
        public Homm2CastleModel WithMageGuildLevel(int level)
        {
            RangeGuard.ThrowIfOutOfRange(level, 0, Constants.Castle.MageGuildLevels, nameof(level));
            var dwellings = ((uint)this.Dwellings & ~(uint)Homm2Dwellings.MageGuildLevelMask) | ((uint)level << 16);
            return this with
            {
                Dwellings = (Homm2Dwellings)dwellings,
                Town = level == 0 ? this.Town & ~Homm2TownBuildings.MageGuild : this.Town | Homm2TownBuildings.MageGuild,
                GuildSpellCounts = GuildCounts(level, this.HasLibrary),
            };
        }

        /// <summary>
        /// Returns the castle with another spell (or none) in a place of the Mage Guild, at <paramref name="level"/> (1 to 5) and <paramref name="slot"/> (0 to 3).
        /// </summary>
        public Homm2CastleModel WithGuildSpell(int level, int slot, Homm2Spell? spell)
        {
            RangeGuard.ThrowIfOutOfRange(level, 1, Constants.Castle.MageGuildLevels, nameof(level));
            RangeGuard.ThrowIfOutOfRange(slot, 0, Constants.Castle.MageGuildSlotsPerLevel - 1, nameof(slot));
            return this with { GuildSpells = this.GuildSpells.With(((level - 1) * Constants.Castle.MageGuildSlotsPerLevel) + slot, spell?.Code ?? 0) };
        }

        private static Homm2Numbers GuildCounts(int level, bool library) =>
            new(BaseGuildSpells.Select((count, i) => i < level ? count + (library ? 1 : 0) : 0));

        /// <summary>
        /// Returns the spell of the Mage Guild at <paramref name="level"/> (1 to 5) and <paramref name="slot"/> (0 to 3), or <c>null</c> for none.
        /// </summary>
        public Homm2Spell? GuildSpell(int level, int slot) => Homm2Spell.FromByte((byte)this.GuildSpells[((level - 1) * Constants.Castle.MageGuildSlotsPerLevel) + slot]);
    }
}
