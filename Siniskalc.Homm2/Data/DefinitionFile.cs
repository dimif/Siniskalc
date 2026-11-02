namespace Siniskalc.Homm2.Data
{
    /// <summary>
    /// The content of a definition file of a set: its entries and what it replaces in the set it extends.
    /// </summary>
    public sealed record DefinitionFile<T>
        where T : EntityDefinition
    {
        /// <summary>
        /// Gets the version of the format of the file.
        /// </summary>
        public required int SchemaVersion { get; init; }

        /// <summary>
        /// Gets the entities the file adds to the set.
        /// </summary>
        public required T[] Entries { get; init; }

        /// <summary>
        /// Gets the entities that take the place of those with the same identifier in the set this one extends.
        /// </summary>
        public required T[] Replacements { get; init; }
    }
}
