namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The definition of a place a hero can visit once, as it is read from <c>visitablePlaces.json</c>.
    /// </summary>
    public sealed record PlaceDefinition : EntityDefinition
    {
        /// <summary>
        /// Gets the kind of the place.
        /// </summary>
        public required PlaceKind Kind { get; init; }

        /// <summary>
        /// Gets what the place gives.
        /// </summary>
        public required string Description { get; init; }
    }
}
