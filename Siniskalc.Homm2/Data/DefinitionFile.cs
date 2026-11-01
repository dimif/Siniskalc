namespace Siniskalc.Homm2.Data
{
    public sealed record DefinitionFile<T>
        where T : EntityDefinition
    {
        public required int SchemaVersion { get; init; }

        public required T[] Entries { get; init; }

        public required T[] Replacements { get; init; }
    }
}
