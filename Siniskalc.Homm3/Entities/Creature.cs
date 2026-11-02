namespace Siniskalc.Homm3.Entities;

using System.Collections.Generic;
using System.Linq;
using Siniskalc.Heroes.Entities;
using Siniskalc.Homm3.Data;

public sealed class Creature : CatalogEntity, ICreature
{
    private readonly string? dwelling;
    private readonly string? baseCreature;

    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    /// <param name="catalog">The catalog.</param>
    /// <param name="definition">The definition.</param>
    internal Creature(EntityCatalog catalog, CreatureDefinition definition)
        : base(catalog, definition)
    {
        this.Faction = catalog.Factions.ById(definition.Faction);
        this.Tier = definition.Tier;
        this.dwelling = definition.Dwelling;
        this.baseCreature = definition.BaseCreature;
        var s = definition.VersionInfo.GetValueOrDefault(catalog.Version) ?? definition.Info;
        this.Info = new CreatureInfo(
            s.Atk,
            s.Def,
            s.MinDamage,
            s.MaxDamage,
            s.Health,
            s.Speed,
            s.Growth,
            s.AiValue,
            s.FightValue,
            s.Gold,
            s.Wood,
            s.Ore,
            s.Mercury,
            s.Sulfur,
            s.Crystal,
            s.Gems,
            s.Shots,
            s.Special.Select(id => (ICreatureAbility)catalog.Abilities.ById(id)).ToArray().AsReadOnly());
    }

    /// <summary>
    /// Gets the level of the creature in its faction.
    /// </summary>
    public int Tier { get; }

    /// <summary>
    /// Gets the faction of the creature.
    /// </summary>
    public Faction Faction { get; }

    /// <summary>
    /// Gets the numbers the game keeps for the creature.
    /// </summary>
    public CreatureInfo Info { get; }

    /// <summary>
    /// Gets the code of the creature this one is the upgrade of, or <see langword="null"/> when it is not an upgrade.
    /// </summary>
    public byte? BaseCreatureCode => this.baseCreature is null ? null : this.Catalog.Creatures.ById(this.baseCreature).Code;

    /// <summary>
    /// Gets the building that recruits the creature, or <see langword="null"/> for a creature no dwelling offers.
    /// </summary>
    public Building? Dwelling => this.dwelling is null ? null : this.Catalog.Buildings.ById(this.dwelling);

    bool ICreature.IsUpgrade => this.baseCreature is not null;

    IFaction ICreature.Faction => this.Faction;

    /// <summary>
    /// Gets the creatures of a faction.
    /// </summary>
    /// <param name="faction">The faction.</param>
    /// <returns>The creatures.</returns>
    public static IReadOnlyList<Creature> ForFaction(Faction faction) => faction.Catalog.Creatures.All.Where(c => c.Faction == faction).ToArray().AsReadOnly();
}
