namespace Siniskalc.Homm3.Data;

using Siniskalc.Homm3.Entities;

/// <summary>The editable identity of an entity, independent of its text and save code.</summary>
public abstract record EntityDefinition
{
    public required string Id { get; init; }

    public required int Order { get; init; }

    public required string Name { get; init; }

    public required byte Code { get; init; }

    public required GameVersion AvailableFrom { get; init; }

    public bool Replaces { get; init; }
}
