namespace Siniskalc.Homm3.Entities;

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

/// <summary>Catalog-scoped indexes; neither callers nor retained input arrays can mutate them.</summary>
public sealed class EntityCollection<T>
    where T : CatalogEntity
{
    private readonly FrozenDictionary<string, T> byId;
    private readonly FrozenDictionary<byte, T> byCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityCollection{T}"/> class.
    /// </summary>
    /// <param name="values">The entities, of every version of the game.</param>
    /// <param name="version">The version of the game; the collection lists the entities available in it.</param>
    /// <param name="contextualCodes">Whether the codes of the entities mean different things in different places, so they cannot be used to find one.</param>
    internal EntityCollection(IEnumerable<T> values, GameVersion version, bool contextualCodes = false)
    {
        var items = values.ToArray();
        this.byId = items.ToFrozenDictionary(e => e.Id, StringComparer.Ordinal);
        this.byCode = contextualCodes ? FrozenDictionary<byte, T>.Empty : items.ToFrozenDictionary(e => e.Code);
        this.All = items.Where(e => e.IsAvailableIn(version)).ToArray().AsReadOnly();
    }

    /// <summary>
    /// Gets the entities available in the version of the game.
    /// </summary>
    public IReadOnlyList<T> All { get; }

    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <returns>The entity.</returns>
    public T ById(string id) => this.byId[id];

    /// <summary>
    /// Finds an entity by the byte the savegame holds for it.
    /// </summary>
    /// <param name="code">The byte.</param>
    /// <returns>The entity, or <see langword="null"/> when no entity has the code.</returns>
    public T? Find(byte code) => this.byCode.GetValueOrDefault(code);

    /// <summary>
    /// Finds an entity by its name.
    /// </summary>
    /// <param name="name">The name, ignoring the case.</param>
    /// <returns>The entity, or <see langword="null"/> when none has the name.</returns>
    public T? FindByName(string name) => this.All.FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
}
