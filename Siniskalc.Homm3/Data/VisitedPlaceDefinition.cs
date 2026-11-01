namespace Siniskalc.Homm3.Data;

public sealed record VisitedPlaceDefinition : EntityDefinition
{
    public required int? ObjectType { get; init; }
    public required VisitSlot Slot { get; init; }
    public required string ToolTip { get; init; }
    public required byte NumberMask { get; init; }
}
