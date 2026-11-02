namespace Siniskalc.Homm3.Data;

public sealed record StandardSetDefinition
{
    /// <summary>
    /// Gets the name of the set this one extends, or <see langword="null"/> for a set that stands alone.
    /// </summary>
    public required string? Parent { get; init; }

    /// <summary>
    /// Gets the folder of the definition files of the set.
    /// </summary>
    public required string Directory { get; init; }
}
