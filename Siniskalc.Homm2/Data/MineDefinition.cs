namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The definition of a kind of mine, as it is read from <c>mines.json</c>.
    /// </summary>
    public sealed record MineDefinition : EntityDefinition
    {
        /// <summary>
        /// Gets the kind of the mine.
        /// </summary>
        public required MineKind Kind { get; init; }

        /// <summary>
        /// Gets the resource the mine produces.
        /// </summary>
        public required Resource Resource { get; init; }
    }
}
