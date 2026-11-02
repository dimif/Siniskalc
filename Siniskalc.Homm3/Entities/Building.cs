namespace Siniskalc.Homm3.Entities;

using System.Collections.Generic;
using System.Linq;
using Siniskalc.Homm3.Data;

public sealed class Building : CatalogEntity
{
    /// <summary>
    /// The number of levels of creatures of a faction, which is the number of dwellings.
    /// </summary>
    public const int DwellingLevelCount = 7;

    /// <summary>
    /// The number of levels of a Mage Guild.
    /// </summary>
    public const int MageGuildLevelCount = 5;

    private readonly string? upgrades;
    private readonly IReadOnlyList<string> requires;
    private readonly IReadOnlyList<string> unavailableFor;

    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <param name="definition">The definition.</param>
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

    /// <summary>
    /// Gets the type of the building.
    /// </summary>
    public BuildingType Type { get; }

    /// <summary>
    /// Gets the faction that has the building, or <see langword="null"/> for a building every faction has but the ones it is not available for.
    /// </summary>
    public Faction? Faction { get; }

    /// <summary>
    /// Gets whether the game builds the building once its prerequisites are met.
    /// </summary>
    public bool Automatic { get; }

    /// <summary>
    /// Gets the gold the building gives each day.
    /// </summary>
    public int GoldIncome { get; }

    /// <summary>
    /// Gets whether the building is the Grail.
    /// </summary>
    public bool IsGrail => this.Type == BuildingType.Grail;

    /// <summary>
    /// Gets whether the building is a dwelling that has not been upgraded.
    /// </summary>
    public bool IsDwelling => this.Type is >= BuildingType.Dwelling1 and <= BuildingType.Dwelling7;

    /// <summary>
    /// Gets whether the building is the upgrade of a dwelling.
    /// </summary>
    public bool IsUpgradedDwelling => this.Type is >= BuildingType.UpgradedDwelling1 and <= BuildingType.UpgradedDwelling7;

    /// <summary>
    /// Gets whether the building is a level of the Mage Guild.
    /// </summary>
    public bool IsMageGuild => this.Type is >= BuildingType.MageGuild1 and <= BuildingType.MageGuild5;

    /// <summary>
    /// Gets the level of the Mage Guild the building is, from 1, or <see langword="null"/> when it is not one.
    /// </summary>
    public int? MageGuildLevel => this.IsMageGuild ? this.Type - BuildingType.MageGuild1 + 1 : null;

    /// <summary>
    /// Gets the level of the creatures the dwelling or its upgrade recruits, from 0, or <see langword="null"/> when the building is not a dwelling.
    /// </summary>
    public int? DwellingLevel => this.IsDwelling ? this.Type - BuildingType.Dwelling1
        : this.IsUpgradedDwelling ? this.Type - BuildingType.UpgradedDwelling1 : null;

    /// <summary>
    /// Gets the building this one replaces when it is built, or <see langword="null"/> when it replaces none.
    /// </summary>
    public Building? Upgrades => this.upgrades is null ? null : this.Catalog.Buildings.ById(this.upgrades);

    /// <summary>
    /// Gets the buildings that must be built first, besides the one it replaces.
    /// </summary>
    public IReadOnlyList<Building> Requires => this.requires.Select(this.Catalog.Buildings.ById).ToArray().AsReadOnly();

    /// <summary>
    /// Gets the building this one replaces and the buildings that must be built first.
    /// </summary>
    public IReadOnlyList<Building> Prerequisites => this.Upgrades is { } previous ? new[] { previous }.Concat(this.Requires).ToArray().AsReadOnly() : this.Requires;

    /// <summary>
    /// Gets the buildings a faction can build.
    /// </summary>
    /// <param name="faction">The faction.</param>
    /// <returns>The buildings, in the order of their codes.</returns>
    public static IReadOnlyList<Building> ForFaction(Faction faction) => faction.Catalog.Buildings.All.Where(b => b.IsAvailableFor(faction)).OrderBy(b => b.Code).ToArray().AsReadOnly();

    /// <summary>
    /// Finds a building of a faction by its bit in the building masks.
    /// </summary>
    /// <param name="faction">The faction.</param>
    /// <param name="bit">The bit.</param>
    /// <returns>The building, or <see langword="null"/> when the faction has none with the bit.</returns>
    public static Building? FromBit(Faction faction, byte bit) => ForFaction(faction).FirstOrDefault(b => b.Code == bit);

    /// <summary>
    /// Finds a building of a faction by its type.
    /// </summary>
    /// <param name="faction">The faction.</param>
    /// <param name="type">The type.</param>
    /// <returns>The building, or <see langword="null"/> when the faction has none of the type.</returns>
    public static Building? FromType(Faction faction, BuildingType type) => ForFaction(faction).FirstOrDefault(b => b.Type == type);

    /// <summary>
    /// Finds the dwelling of a faction that recruits the creatures of a level.
    /// </summary>
    /// <param name="faction">The faction.</param>
    /// <param name="level">The level, from 0.</param>
    /// <param name="upgraded">Whether to find the upgrade of the dwelling.</param>
    /// <returns>The building, or <see langword="null"/> when the level is out of range or the faction has none.</returns>
    public static Building? FromDwellingLevel(Faction faction, int level, bool upgraded) => level is < 0 or >= DwellingLevelCount ? null
        : FromType(faction, (upgraded ? BuildingType.UpgradedDwelling1 : BuildingType.Dwelling1) + level);

    /// <summary>
    /// Tells whether a faction can build the building.
    /// </summary>
    /// <param name="faction">The faction.</param>
    /// <returns><see langword="true"/> if the faction can build it.</returns>
    public bool IsAvailableFor(Faction faction) => ReferenceEquals(this.Catalog, faction.Catalog) &&
        (this.Faction is null ? !this.unavailableFor.Contains(faction.Id) : this.Faction == faction);
}
