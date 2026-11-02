namespace Siniskalc.Homm3.Data
{
    using System.Linq;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Which creatures a creature dwelling of a castle serves, read from <see cref="Creature.Dwelling"/>.
    /// A faction has one dwelling for each creature level and one upgraded dwelling.
    /// </summary>
    public static class DwellingCreatures
    {
        /// <summary>
        /// Returns the creature that <paramref name="building"/> recruits, or null when it is not a dwelling.
        /// </summary>
        /// <param name="building">The building.</param>
        /// <returns>The creature, or <see langword="null"/> when no creature is recruited in the building.</returns>
        public static Creature? Of(Building building) => building.Catalog.Creatures.All.FirstOrDefault(c => building.Equals(c.Dwelling));

        /// <summary>
        /// Returns the creature of <paramref name="faction"/> at the zero-based creature <paramref name="level"/>, or null if it has none.
        /// Gives the creature of the upgraded dwelling when <paramref name="upgraded"/> is set.
        /// </summary>
        /// <param name="faction">The faction.</param>
        /// <param name="level">The level, from 0.</param>
        /// <param name="upgraded">Whether to take the upgraded dwelling.</param>
        /// <returns>The creature, or <see langword="null"/> when the faction has none.</returns>
        public static Creature? Find(Faction faction, int level, bool upgraded)
        {
            var building = Building.FromDwellingLevel(faction, level, upgraded);
            return building == null ? null : Of(building);
        }

        /// <summary>
        /// Returns what to call the creatures of a creature level: both, as "Behemoth\Ancient Behemoth", when the dwelling has its upgrade,
        /// otherwise just the creature of the dwelling. Empty when the faction has no creature of that level.
        /// </summary>
        /// <param name="faction">The faction.</param>
        /// <param name="level">The level, from 0.</param>
        /// <param name="hasUpgrade">Whether the dwelling has an upgrade.</param>
        /// <returns>The names of the creatures, the base one and its upgrade.</returns>
        public static string NamesOf(Faction faction, int level, bool hasUpgrade)
        {
            var creature = Find(faction, level, upgraded: false);
            var upgraded = hasUpgrade ? Find(faction, level, upgraded: true) : null;
            return upgraded == null ? creature?.Name ?? string.Empty : creature?.Name + "\\" + upgraded.Name;
        }
    }
}
