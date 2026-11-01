namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm2.Data;

    /// <summary>The fully resolved, immutable definitions of one standard set, explicitly shared by its mapped versions.</summary>
    public sealed class GameCatalog
    {
        private static readonly ILogger<GameCatalog> Logger = LogFactory.For<GameCatalog>();
        private readonly IReadOnlyDictionary<int, Faction> heroFactions;
        private readonly Faction heroFallback;

        internal GameCatalog(string setId, CatalogDefinitions definitions)
        {
            this.SetId = setId;
            this.Factions = new(definitions.Factions.Select(d => new Faction(this, d)));
            this.NeutralFaction = this.Factions["Neutral"];
            this.Abilities = new(definitions.Abilities.Select(d => new CreatureAbility(this, d)));
            this.SpellTypes = new(definitions.SpellTypes.Select(d => new SpellGroup(this, d)));
            this.Skills = new(definitions.Skills.Select(d => new SecondarySkill(this, d)));
            this.Artifacts = new(definitions.Artifacts.Select(d => new Artifact(this, d)));
            this.SpellBook = this.Artifacts.Single(a => a.IsSpellBook);
            this.FallbackArtifact = this.Artifacts.Find(0)!;
            this.Spells = new(definitions.Spells.Select(d => new Spell(this, d)));
            var creatureCodes = definitions.Creatures.ToDictionary(c => c.Id, c => c.Code);
            this.Creatures = new(definitions.Creatures.Select(d => new Creature(this, d, d.BaseCreature is null ? null : creatureCodes[d.BaseCreature])));
            this.FallbackCreature = this.Creatures.Find(0)!;
            this.Buildings = new(definitions.Buildings.Select(d => new Building(this, d)));
            this.Places = new(definitions.Places.Select(d => new MapPlace(this, d)));
            this.Mines = new(definitions.Mines.Select(d => new Mine(this, d)));
            this.VisitEffects = new(definitions.VisitEffects.Select(d => new VisitEffect(this, d)));
            this.MapObjects = new(definitions.MapObjects.Select(d => new MapObject(this, d)));
            this.heroFactions = new ReadOnlyDictionary<int, Faction>(definitions.Factions.SelectMany(f => f.HeroIds.Select(id => (id, faction: this.Factions[f.Id]))).ToDictionary(p => p.id, p => p.faction));
            this.heroFallback = this.Factions.Find(0)!;
        }

        public string SetId { get; }

        public EntityCatalog<Faction> Factions { get; }

        public EntityCatalog<CreatureAbility> Abilities { get; }

        public EntityCatalog<SpellGroup> SpellTypes { get; }

        public EntityCatalog<SecondarySkill> Skills { get; }

        public EntityCatalog<Artifact> Artifacts { get; }

        public EntityCatalog<Spell> Spells { get; }

        public EntityCatalog<Creature> Creatures { get; }

        public EntityCatalog<Building> Buildings { get; }

        public EntityCatalog<MapPlace> Places { get; }

        public EntityCatalog<Mine> Mines { get; }

        public EntityCatalog<VisitEffect> VisitEffects { get; }

        public EntityCatalog<MapObject> MapObjects { get; }

        public Faction NeutralFaction { get; }

        public Artifact SpellBook { get; }

        public Artifact FallbackArtifact { get; }

        public Creature FallbackCreature { get; }

        public Faction FactionForHero(int id)
        {
            if (this.heroFactions.TryGetValue(id, out var faction))
            {
                return faction;
            }

            Logger.LogTrace("Hero id {HeroId} has no faction mapping and reads as the code-zero faction {StandIn}", id, this.heroFallback.Name);
            return this.heroFallback;
        }
    }
}
