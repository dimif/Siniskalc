namespace Siniskalc.Heroes.Models
{
    using System.Collections.Generic;
    using Siniskalc.Core.Models;

    /// <summary>
    /// The parts of the savegame an edit can belong to, which is how the pending changes are grouped and named. A game uses the ones it has.
    /// </summary>
    public static class ChangeAreas
    {
        public static readonly ChangeArea General = new("General");

        public static readonly ChangeArea PrimarySkills = new("Primary skills");

        public static readonly ChangeArea SecondarySkills = new("Secondary skills");

        public static readonly ChangeArea Creatures = new("Creatures");

        public static readonly ChangeArea WarMachines = new("War machines");

        public static readonly ChangeArea Artifacts = new("Artifacts");

        public static readonly ChangeArea Backpack = new("Backpack");

        public static readonly ChangeArea Spells = new("Spells");

        public static readonly ChangeArea Resources = new("Resources");

        public static readonly ChangeArea Heroes = new("Heroes");

        public static readonly ChangeArea Castles = new("Castles");

        public static readonly ChangeArea Garrison = new("Garrison");

        public static readonly ChangeArea Dwellings = new("Dwellings");

        public static readonly ChangeArea Buildings = new("Buildings");

        public static readonly ChangeArea MageGuild = new("Mage Guild");

        public static IReadOnlyList<ChangeArea> All { get; } =
        [
            General, PrimarySkills, SecondarySkills, Creatures, WarMachines, Artifacts, Backpack, Spells, Resources, Heroes, Castles, Garrison, Dwellings, Buildings, MageGuild,
        ];
    }
}
