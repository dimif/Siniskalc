namespace Siniskalc.Homm2.Models
{
    using Siniskalc.Core.Models;

    /// <summary>
    /// The parts of a hero, a player or a castle a pending change belongs to.
    /// </summary>
    public static class ChangeAreas
    {
        public static readonly ChangeArea General = new("General");

        public static readonly ChangeArea Skills = new("Skills");

        public static readonly ChangeArea Army = new("Army");

        public static readonly ChangeArea Artifacts = new("Artifacts");

        public static readonly ChangeArea Spells = new("Spells");

        public static readonly ChangeArea Resources = new("Resources");

        public static readonly ChangeArea Heroes = new("Heroes");

        public static readonly ChangeArea Castles = new("Castles");

        public static readonly ChangeArea Garrison = new("Garrison");

        public static readonly ChangeArea Buildings = new("Buildings");

        public static readonly ChangeArea Dwellings = new("Dwellings");

        public static readonly ChangeArea MageGuild = new("Mage Guild");
    }
}
