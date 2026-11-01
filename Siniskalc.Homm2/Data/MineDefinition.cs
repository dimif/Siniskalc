namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Entities;

    public sealed record MineDefinition : EntityDefinition
    {
        public required MineKind Kind { get; init; }

        public required Resource Resource { get; init; }
    }
}
