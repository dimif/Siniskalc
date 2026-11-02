namespace Siniskalc.Homm3.Data;

using Siniskalc.Heroes.Entities;

public sealed record MineDefinition
{
    /// <summary>
    /// Gets the kind of the mine.
    /// </summary>
    public required MineKind Kind { get; init; }

    /// <summary>
    /// Gets the name the editor shows.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the resource the mine produces.
    /// </summary>
    public required Resource Resource { get; init; }

    /// <summary>
    /// Gets how much of the resource the mine gives each day.
    /// </summary>
    public required int DailyAmount { get; init; }
}
