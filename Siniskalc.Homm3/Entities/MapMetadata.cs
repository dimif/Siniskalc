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

    internal MapMetadata(MapMetadataDefinition definition)
    {
        this.mines = definition.Mines.ToFrozenDictionary(m => m.Kind);
        this.fallback = definition.Mines.Single(m => m.Fallback);
        this.VisitEffects = definition.VisitEffects.OrderBy(e => e.Order).ToArray().AsReadOnly();
        this.MerchantName = definition.MerchantName;
        this.DwellingName = definition.DwellingName;
    }

    public IReadOnlyList<VisitEffectDefinition> VisitEffects { get; }
    public string MerchantName { get; }
    public string DwellingName { get; }
    // TODO:LOG Trace: an unknown mine kind uses the catalog's explicit fallback metadata.
    public MineDefinition Mine(MineKind kind) => this.mines.GetValueOrDefault(kind) ?? this.fallback;
    public string MineNameAt(MineKind kind, MapPosition position) => $"{this.Mine(kind).Name} at {MapLevelText.Place(position, hasLevels: true)}";
    public string EffectName(VisitedBuildingEffects effect) => this.VisitEffects.Single(e => e.Effect == effect).Name;
}
