namespace Siniskalc.Homm2.Data
{
    public sealed record AbilityDefinition : EntityDefinition
    {
        public required bool IsRanged { get; init; }
    }
}
