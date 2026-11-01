namespace Siniskalc.Homm2.Data
{
    public sealed record SpellTypeDefinition : EntityDefinition
    {
        public required bool Hidden { get; init; }
    }
}
