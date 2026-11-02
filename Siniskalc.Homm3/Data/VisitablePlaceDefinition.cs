namespace Siniskalc.Homm3.Data;

public sealed record VisitablePlaceDefinition : EntityDefinition
{
    /// <summary>
    /// Gets the number of the type of the object of the map that is the place, or <see langword="null"/> when the map does not list it.
    /// </summary>
    public required int? ObjectType { get; init; }

    /// <summary>
    /// Gets where the visit of the place is kept in the record of a hero.
    /// </summary>
    public required VisitSlot Slot { get; init; }

    /// <summary>
    /// Gets what the place gives.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the bits of the number of an object that tell which place of the type it is.
    /// </summary>
    public required byte NumberMask { get; init; }
}
