namespace Siniskalc.Data
{
    using System.Linq;
    using Siniskalc.Entities;

    /// <summary>
    /// Which creatures a creature dwelling of a castle serves, read from <see cref="Creature.Dwelling"/>. 
    /// A faction has one dwelling for each creature level and one upgraded dwelling.
    /// </summary>
    public static class DwellingCreatures
    {
        /// <summary>
        /// Returns the creature that <paramref name="building"/> recruits, or null when it is not a dwelling.
        /// </summary>
        public static Creature? Of(Building building) => Creature.All.FirstOrDefault(c => building.Equals(c.Dwelling));

        /// <summary>
        /// Returns the creature of <paramref name="faction"/> at the zero-based creature <paramref name="level"/>, or null if it has none.
        /// Gives the creature of the upgraded dwelling when <paramref name="upgraded"/> is set.
        /// </summary>
        public static Creature? Find(Faction faction, int level, bool upgraded)
        {
            var building = Building.FromDwellingLevel(faction, level, upgraded);
            return building == null ? null : Of(building);
        }

        /// <summary>
        /// Returns what to call the creatures of a creature level: both, as "Behemoth\Ancient Behemoth", when the dwelling has its upgrade,
        /// otherwise just the creature of the dwelling. Empty when the faction has no creature of that level.
        /// </summary>
        public static string NamesOf(Faction faction, int level, bool hasUpgrade)
        {
            var creature = Find(faction, level, upgraded: false);
            var upgraded = hasUpgrade ? Find(faction, level, upgraded: true) : null;
            return upgraded == null ? creature?.Name ?? string.Empty : creature?.Name + "\\" + upgraded.Name;
        }
    }
}
