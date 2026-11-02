namespace Siniskalc.Homm3.Entities;

using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using Siniskalc.Heroes.Entities;
using Siniskalc.Heroes.Models;
using Siniskalc.Heroes.ViewModels;
using Siniskalc.Homm3.Data;

public sealed class MapMetadata
{
    private readonly FrozenDictionary<MineKind, MineDefinition> mines;
    private readonly MineDefinition fallback;

    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    /// <param name="definition">The definition.</param>
    /// <param name="mines">The mines.</param>
    /// <param name="visitEffects">The visit effects.</param>
    internal MapMetadata(MapMetadataDefinition definition, MineDefinition[] mines, VisitEffectDefinition[] visitEffects)
    {
        this.mines = mines.ToFrozenDictionary(m => m.Kind);
        this.fallback = mines.Single(m => (int)m.Kind == 0);
        this.VisitEffects = visitEffects.ToArray().AsReadOnly();
        this.MerchantName = definition.MerchantName;
        this.DwellingName = definition.DwellingName;
    }

    /// <summary>
    /// Gets the effects a visit leaves on a hero.
    /// </summary>
    public IReadOnlyList<VisitEffectDefinition> VisitEffects { get; }

    /// <summary>
    /// Gets the name of the objects of the map that are merchants.
    /// </summary>
    public string MerchantName { get; }

    /// <summary>
    /// Gets the name of the objects of the map that are dwellings.
    /// </summary>
    public string DwellingName { get; }

    /// <summary>
    /// Gets the definition of a kind of mine.
    /// </summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The definition; a stand-in for a kind that is not defined.</returns>
    public MineDefinition Mine(MineKind kind) => this.mines.GetValueOrDefault(kind) ?? this.fallback;

    /// <summary>
    /// Gets the name of a mine at a place.
    /// </summary>
    /// <param name="kind">The kind.</param>
    /// <param name="position">Where the mine is.</param>
    /// <returns>The name, with the position.</returns>
    public string MineNameAt(MineKind kind, MapPosition position) => $"{this.Mine(kind).Name} at {MapLevelText.Place(position, hasLevels: true)}";

    /// <summary>
    /// Gets the name of an effect of a visit.
    /// </summary>
    /// <param name="effect">The effect.</param>
    /// <returns>The name.</returns>
    public string EffectName(VisitedBuildingEffects effect) => this.VisitEffects.Single(e => e.Effect == effect).Name;
}
