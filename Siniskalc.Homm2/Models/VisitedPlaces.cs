namespace Siniskalc.Homm2.Models
{
    using System;
    using System.Numerics;
    using Siniskalc.Homm2.Entities;

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
    /// <param name="Xanadu">Xanadu (a point of every primary skill).</param>
    public readonly record struct VisitedPlaces(uint Gazebos, uint Forts, uint WitchDoctorHuts, uint MercenaryCamps, uint StandingStones, uint Xanadu = 0)
    {
        /// <summary>
        /// Gets the number of places the hero remembers.
        /// </summary>
        public int Count => BitOperations.PopCount(this.Gazebos) + BitOperations.PopCount(this.Forts) + BitOperations.PopCount(this.WitchDoctorHuts) + BitOperations.PopCount(this.MercenaryCamps) + BitOperations.PopCount(this.StandingStones) + BitOperations.PopCount(this.Xanadu);

        /// <summary>
        /// Returns the set of places of a kind: a bit for each, the first place on the map being bit 1.
        /// </summary>
        /// <param name="kind">The kind of place.</param>
        /// <returns>The set.</returns>
        public uint Of(PlaceKind kind) => kind switch
        {
            PlaceKind.Gazebo => this.Gazebos,
            PlaceKind.Fort => this.Forts,
            PlaceKind.WitchDoctorHut => this.WitchDoctorHuts,
            PlaceKind.MercenaryCamp => this.MercenaryCamps,
            PlaceKind.StandingStones => this.StandingStones,
            PlaceKind.Xanadu => this.Xanadu,
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };

        /// <summary>
        /// Returns the places with the set of a kind replaced.
        /// </summary>
        /// <param name="kind">The kind of place.</param>
        /// <param name="bits">The new set.</param>
        /// <returns>The new places; these are not changed.</returns>
        public VisitedPlaces With(PlaceKind kind, uint bits) => kind switch
        {
            PlaceKind.Gazebo => this with { Gazebos = bits },
            PlaceKind.Fort => this with { Forts = bits },
            PlaceKind.WitchDoctorHut => this with { WitchDoctorHuts = bits },
            PlaceKind.MercenaryCamp => this with { MercenaryCamps = bits },
            PlaceKind.StandingStones => this with { StandingStones = bits },
            PlaceKind.Xanadu => this with { Xanadu = bits },
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }
}
