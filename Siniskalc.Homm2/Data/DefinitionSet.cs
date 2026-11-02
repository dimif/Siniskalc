namespace Siniskalc.Homm2.Data
{
    /// <summary>
    /// One set of definitions in the manifest.
    /// </summary>
    public sealed record DefinitionSet
    {
        /// <summary>
        /// Gets the name of the set this one extends, or <see langword="null"/> for a set that stands alone.
        /// </summary>
        public required string? Parent { get; init; }
    }
}
