namespace Siniskalc.Homm3.Data;

public sealed record SchoolDefinition : EntityDefinition
{
    /// <summary>
    /// Gets the text that says what the school is.
    /// </summary>
    public required string Description { get; init; }
}
