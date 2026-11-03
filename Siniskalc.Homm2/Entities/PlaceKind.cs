namespace Siniskalc.Homm2.Entities
{
    /// <summary>
    /// The kinds of places on the map that give a hero something once, and that the hero remembers having visited (a bit of one of the sets
    /// in its record, <see cref="Models.VisitedPlaces"/>).
    /// </summary>
    public enum PlaceKind
    {
        Gazebo,
        Fort,
        WitchDoctorHut,
        MercenaryCamp,
        StandingStones,

        /// <summary>
        /// Xanadu: a point for each primary skill, once per hero.
        /// </summary>
        Xanadu,
    }
}
