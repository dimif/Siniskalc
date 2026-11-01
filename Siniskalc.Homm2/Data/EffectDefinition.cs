namespace Siniskalc.Homm2.Data
{
    public sealed record EffectDefinition : EntityDefinition
    {
        public required int Attack { get; init; }

        public required int Defense { get; init; }

        public required int Power { get; init; }

        public required int Knowledge { get; init; }
    }
}
