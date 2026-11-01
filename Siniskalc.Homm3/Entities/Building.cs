namespace Siniskalc.Homm3.Entities;

using System.Collections.Generic;
using System.Linq;
using Siniskalc.Homm3.Data;

public sealed class Building : CatalogEntity
{
    public const int DwellingLevelCount = 7;
    public const int MageGuildLevelCount = 5;

    private readonly string? upgrades;
    private readonly IReadOnlyList<string> requires;
    private readonly IReadOnlyList<string> unavailableFor;

    internal Building(EntityCatalog catalog, BuildingDefinition definition)
        : base(catalog, definition)
    {
        this.Type = definition.Type;
        this.Faction = definition.Faction is null ? null : catalog.Factions.ById(definition.Faction);
        this.upgrades = definition.Upgrades;
        this.requires = definition.Requires.ToArray().AsReadOnly();
        this.unavailableFor = definition.Except.ToArray().AsReadOnly();
        this.Automatic = definition.Automatic;
        this.GoldIncome = definition.GoldIncome;
    }

    public BuildingType Type { get; }

    public Faction? Faction { get; }

    public bool Automatic { get; }

    public int GoldIncome { get; }

    public bool IsGrail => this.Type == BuildingType.Grail;

    public bool IsDwelling => this.Type is >= BuildingType.Dwelling1 and <= BuildingType.Dwelling7;

    public bool IsUpgradedDwelling => this.Type is >= BuildingType.UpgradedDwelling1 and <= BuildingType.UpgradedDwelling7;

    public bool IsMageGuild => this.Type is >= BuildingType.MageGuild1 and <= BuildingType.MageGuild5;

    public int? MageGuildLevel => this.IsMageGuild ? this.Type - BuildingType.MageGuild1 + 1 : null;

    public int? DwellingLevel => this.IsDwelling ? this.Type - BuildingType.Dwelling1
        : this.IsUpgradedDwelling ? this.Type - BuildingType.UpgradedDwelling1 : null;

    public Building? Upgrades => this.upgrades is null ? null : this.Catalog.Buildings.ById(this.upgrades);

    public IReadOnlyList<Building> Requires => this.requires.Select(this.Catalog.Buildings.ById).ToArray().AsReadOnly();

    public IReadOnlyList<Building> Prerequisites => this.Upgrades is { } previous ? new[] { previous }.Concat(this.Requires).ToArray().AsReadOnly() : this.Requires;

    public static IReadOnlyList<Building> ForFaction(Faction faction) => faction.Catalog.Buildings.All.Where(b => b.IsAvailableFor(faction)).OrderBy(b => b.Code).ToArray().AsReadOnly();

    public static Building? FromBit(Faction faction, byte bit) => ForFaction(faction).FirstOrDefault(b => b.Code == bit);

    public static Building? FromType(Faction faction, BuildingType type) => ForFaction(faction).FirstOrDefault(b => b.Type == type);

    public static Building? FromDwellingLevel(Faction faction, int level, bool upgraded) => level is < 0 or >= DwellingLevelCount ? null
        : FromType(faction, (upgraded ? BuildingType.UpgradedDwelling1 : BuildingType.Dwelling1) + level);

    public bool IsAvailableFor(Faction faction) => ReferenceEquals(this.Catalog, faction.Catalog) &&
        (this.Faction is null ? !this.unavailableFor.Contains(faction.Id) : this.Faction == faction);
}
