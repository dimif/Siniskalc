namespace Siniskalc.Homm3.Data;

using Siniskalc.Heroes.Entities;

public sealed record MineDefinition
{
    public required MineKind Kind { get; init; }

    public required string Name { get; init; }

    public required Resource Resource { get; init; }

    public required int DailyAmount { get; init; }
}
