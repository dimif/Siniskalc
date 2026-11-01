namespace Siniskalc.Homm2.Data
{
    public record EntityDefinition
    {
        public required string Id { get; init; }

        public required string Name { get; init; }

        public required byte Code { get; init; }
    }
}
