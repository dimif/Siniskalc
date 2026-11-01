namespace Siniskalc.Homm3.Data;

public sealed record SchoolDefinition : EntityDefinition
{
    public required string Description { get; init; }
    public required bool Universal { get; init; }
}
