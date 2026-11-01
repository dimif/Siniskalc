namespace Siniskalc.Homm3.Data;

public sealed record VisitablePlaceDefinition : EntityDefinition
{
    public required int? ObjectType { get; init; }

    public required VisitSlot Slot { get; init; }

    public required string Description { get; init; }

    public required byte NumberMask { get; init; }
}
