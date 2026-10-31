namespace Siniskalc.Homm2.Entities
{
    using System;

    /// <summary>
    /// The names of the kinds of places a hero remembers having visited, as the pages and the pending changes call them.
    /// </summary>
    public static class PlaceKindNames
    {
        /// <summary>
        /// Returns the plural name of a kind of place.
        /// </summary>
        public static string Of(PlaceKind kind) => kind switch
        {
            PlaceKind.Gazebo => "Gazebos",
            PlaceKind.Fort => "Forts",
            PlaceKind.WitchDoctorHut => "Witch doctor's huts",
            PlaceKind.MercenaryCamp => "Mercenary camps",
            PlaceKind.StandingStones => "Standing stones",
            PlaceKind.Xanadu => "Xanadu",
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }
}
