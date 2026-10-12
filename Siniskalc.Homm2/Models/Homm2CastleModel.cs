namespace Siniskalc.Homm2.Models
{
    using System;
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
    /// <param name="Creatures">The creatures that wait in each of the six dwellings.</param>
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
        Homm2Numbers GuildSpells,
        Homm2Numbers GuildSpellCounts)
    {
        /// <summary>
        /// Gets a value indicating whether the map uses the castle.
        /// </summary>
        public bool IsUsed => this.Name.Length > 0;

        public bool HasCastle => this.Town.HasFlag(Homm2TownBuildings.Castle);

        /// <summary>
        /// Gets the level of the Mage Guild, 0 when there is none.
        /// </summary>
        public int MageGuildLevel => this.Dwellings.HasFlag(Homm2Dwellings.MageGuild2) ? 2 : this.Dwellings.HasFlag(Homm2Dwellings.MageGuild1) ? 1 : 0;

        public bool HasDwelling(int dwelling)
        {
            return dwelling is >= 1 and <= Constants.Castle.DwellingCount
                ? (this.Dwellings & (Homm2Dwellings)(0x08u << (dwelling - 1))) != 0
                : throw new ArgumentOutOfRangeException(nameof(dwelling), dwelling, "A town has six dwellings, numbered from 1.");
        }

        /// <summary>
        /// Returns the spell of the Mage Guild at <paramref name="level"/> (1 to 5) and <paramref name="slot"/> (0 to 3), or <c>null</c> for none.
        /// </summary>
        public Homm2Spell? GuildSpell(int level, int slot) => Homm2Spell.FromByte((byte)this.GuildSpells[((level - 1) * Constants.Castle.MageGuildSlotsPerLevel) + slot]);
    }
}
