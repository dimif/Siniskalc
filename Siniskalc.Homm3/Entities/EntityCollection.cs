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

    internal EntityCollection(IEnumerable<T> values, GameVersion version, bool contextualCodes = false)
    {
        var items = values.OrderBy(e => e.Order).ToArray();
        this.byId = items.ToFrozenDictionary(e => e.Id, StringComparer.Ordinal);
        this.byCode = contextualCodes ? FrozenDictionary<byte, T>.Empty : items.ToFrozenDictionary(e => e.Code);
        this.All = items.Where(e => e.IsAvailableIn(version)).ToArray().AsReadOnly();
    }

    public IReadOnlyList<T> All { get; }

    public T ById(string id) => this.byId[id];

    public T? Find(byte code) => this.byCode.GetValueOrDefault(code);

    public T? FindByName(string name) => this.All.FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase));
}
