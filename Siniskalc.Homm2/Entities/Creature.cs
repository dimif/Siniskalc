namespace Siniskalc.Homm2.Entities
{
    using System.Linq;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Data;

    public sealed class Creature : CatalogEntity, ICreature
    {
        internal Creature(GameCatalog catalog, CreatureDefinition definition, byte? baseCode)
            : base(catalog, definition)
        {
            this.Faction = definition.Faction is null ? null : catalog.Factions[definition.Faction];
            this.Tier = definition.Tier;
            this.BaseCreatureCode = baseCode;
            this.Dwelling = definition.Dwelling;
            var s = definition.Stats;
            this.Info = new CreatureInfo(s.Atk, s.Def, s.MinDamage, s.MaxDamage, s.Health, s.Speed, s.Growth, s.AiValue, s.FightValue, s.Gold, s.Wood, s.Ore, s.Mercury, s.Sulfur, s.Crystal, s.Gems, s.Shots, System.Array.AsReadOnly<ICreatureAbility>(definition.Abilities.Select(id => (ICreatureAbility)catalog.Abilities[id]).ToArray()));
        }

        public Faction? Faction { get; }

        public int Tier { get; }

        public CreatureInfo Info { get; }

        public byte? BaseCreatureCode { get; }

        public Dwellings? Dwelling { get; }

        bool ICreature.IsUpgrade => this.BaseCreatureCode is not null;

        IFaction? ICreature.Faction => this.Faction;
    }
}
