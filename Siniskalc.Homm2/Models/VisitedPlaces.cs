namespace Siniskalc.Homm2.Models
{
    using System.Numerics;

    /// <summary>
    /// The places of the map that give a hero something once, and that the hero remembers having visited: a bit for each place of a kind, in
    /// the order the map has them (the savegames of Threedom: the fort the hero visited second sets the second bit). A place the hero has not
    /// visited can be visited, a visited one gives nothing again, and clearing the bits lets the hero visit them all again.
    /// </summary>
    /// <param name="Gazebos">The gazebos (a thousand experience points).</param>
    /// <param name="Forts">The forts (a point of defense).</param>
    /// <param name="WitchDoctorHuts">The witch doctor's huts (a point of knowledge, or a secondary skill).</param>
    /// <param name="MercenaryCamps">The mercenary camps (a point of attack).</param>
    /// <param name="StandingStones">The standing stones (a point of spell power).</param>
    public readonly record struct VisitedPlaces(uint Gazebos, uint Forts, uint WitchDoctorHuts, uint MercenaryCamps, uint StandingStones)
    {
        /// <summary>
        /// Gets the number of places the hero remembers.
        /// </summary>
        public int Count => BitOperations.PopCount(this.Gazebos) + BitOperations.PopCount(this.Forts) + BitOperations.PopCount(this.WitchDoctorHuts) + BitOperations.PopCount(this.MercenaryCamps) + BitOperations.PopCount(this.StandingStones);
    }
}
