namespace Siniskalc.Homm2.Data
{
    public sealed record GroupDefinition : EntityDefinition
    {
        public required bool Hidden { get; init; }
    }
}
