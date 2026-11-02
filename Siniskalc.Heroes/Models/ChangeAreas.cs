namespace Siniskalc.Heroes.Models
{
    using System.Collections.Generic;
    using Siniskalc.Core.Models;

    /// <summary>
    /// The parts of the savegame an edit can belong to, which is how the pending changes are grouped and named. A game uses the ones it has.
    /// </summary>
    public static class ChangeAreas
    {
        /// <summary>
        /// The general facts of a hero, castle or player.
        /// </summary>
        public static readonly ChangeArea General = new("General");

        /// <summary>
        /// The primary skills of a hero.
        /// </summary>
        public static readonly ChangeArea PrimarySkills = new("Primary skills");

        /// <summary>
        /// The secondary skills of a hero.
        /// </summary>
        public static readonly ChangeArea SecondarySkills = new("Secondary skills");

        /// <summary>
        /// The creatures of an army.
        /// </summary>
        public static readonly ChangeArea Creatures = new("Creatures");

        /// <summary>
        /// The war machines of a hero.
        /// </summary>
        public static readonly ChangeArea WarMachines = new("War machines");

        /// <summary>
        /// The artifacts a hero wears.
        /// </summary>
        public static readonly ChangeArea Artifacts = new("Artifacts");

        /// <summary>
        /// The artifacts a hero carries in the backpack.
        /// </summary>
        public static readonly ChangeArea Backpack = new("Backpack");

        /// <summary>
        /// The spells of a hero.
        /// </summary>
        public static readonly ChangeArea Spells = new("Spells");

        /// <summary>
        /// The resources of a player.
        /// </summary>
        public static readonly ChangeArea Resources = new("Resources");

        /// <summary>
        /// The heroes of a player or castle.
        /// </summary>
        public static readonly ChangeArea Heroes = new("Heroes");

        /// <summary>
        /// The castles of a player.
        /// </summary>
        public static readonly ChangeArea Castles = new("Castles");

        /// <summary>
        /// The army that guards a castle.
        /// </summary>
        public static readonly ChangeArea Garrison = new("Garrison");

        /// <summary>
        /// The dwellings of a castle or of the map.
        /// </summary>
        public static readonly ChangeArea Dwellings = new("Dwellings");

        /// <summary>
        /// The buildings of a castle.
        /// </summary>
        public static readonly ChangeArea Buildings = new("Buildings");

        /// <summary>
        /// The spells of the Mage Guild of a castle.
        /// </summary>
        public static readonly ChangeArea MageGuild = new("Mage Guild");

        /// <summary>
        /// Gets every area, in the order the editor lists them.
        /// </summary>
        public static IReadOnlyList<ChangeArea> All { get; } =
        [
            General, PrimarySkills, SecondarySkills, Creatures, WarMachines, Artifacts, Backpack, Spells, Resources, Heroes, Castles, Garrison, Dwellings, Buildings, MageGuild,
        ];
    }
}
