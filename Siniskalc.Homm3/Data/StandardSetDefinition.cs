namespace Siniskalc.Homm3.Data;

public sealed record StandardSetDefinition
{
    public required string? Parent { get; init; }
    public required string Directory { get; init; }
}
