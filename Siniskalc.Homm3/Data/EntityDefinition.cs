namespace Siniskalc.Homm3.Data;

using Siniskalc.Homm3.Entities;

/// <summary>The editable identity of an entity, independent of its text and save code.</summary>
public abstract record EntityDefinition
{
    /// <summary>
    /// Gets the identifier of the entity, which the other definitions refer to.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the name the editor shows.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the byte the savegame holds for the entity.
    /// </summary>
    public required byte Code { get; init; }

    /// <summary>
    /// Gets the first version of the game that has the entity.
    /// </summary>
    public required GameVersion AvailableFrom { get; init; }

    /// <summary>
    /// Gets whether the definition replaces the one of the set it extends that has the same identifier.
    /// </summary>
    public bool Replaces { get; init; }
}
