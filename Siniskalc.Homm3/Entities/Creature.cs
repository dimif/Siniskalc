namespace Siniskalc.Homm3.Entities;

using System.Collections.Generic;
using System.Linq;
using Siniskalc.Heroes.Entities;
using Siniskalc.Homm3.Data;

public sealed class Creature : CatalogEntity, ICreature
{
    private readonly string? dwelling;
    private readonly string? baseCreature;

    internal Creature(EntityCatalog catalog, CreatureDefinition definition)
        : base(catalog, definition)
    {
        this.Faction = catalog.Factions.ById(definition.Faction);
        this.Tier = definition.Tier;
        this.dwelling = definition.Dwelling;
        this.baseCreature = definition.BaseCreature;
        var s = definition.Info;
        this.Info = new CreatureInfo(s.Atk, s.Def, s.MinDamage, s.MaxDamage, s.Health, s.Speed, s.Growth,
            s.AiValue, s.FightValue, s.Gold, s.Wood, s.Ore, s.Mercury, s.Sulfur, s.Crystal, s.Gems, s.Shots,
            s.Special.Select(id => (ICreatureAbility)catalog.Abilities.ById(id)).ToArray().AsReadOnly());
    }

    public int Tier { get; }

    public Faction Faction { get; }

    public CreatureInfo Info { get; }

    public byte? BaseCreatureCode => this.baseCreature is null ? null : this.Catalog.Creatures.ById(this.baseCreature).Code;

    public Building? Dwelling => this.dwelling is null ? null : this.Catalog.Buildings.ById(this.dwelling);

    bool ICreature.IsUpgrade => this.baseCreature is not null;

    IFaction ICreature.Faction => this.Faction;

    public static IReadOnlyList<Creature> ForFaction(Faction faction) => faction.Catalog.Creatures.All.Where(c => c.Faction == faction).ToArray().AsReadOnly();
}
