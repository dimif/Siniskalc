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

        /// <summary>
        /// Initializes a new instance of the <see cref="GameCatalog"/> class.
        /// </summary>
        /// <param name="setId">The set id.</param>
        /// <param name="definitions">The definitions.</param>
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

        /// <summary>
        /// Gets the name of the set of definitions the catalog is made from.
        /// </summary>
        public string SetId { get; }

        /// <summary>
        /// Gets the factions.
        /// </summary>
        public EntityCatalog<Faction> Factions { get; }

        /// <summary>
        /// Gets the abilities of the creatures.
        /// </summary>
        public EntityCatalog<CreatureAbility> Abilities { get; }

        /// <summary>
        /// Gets the groups of spells.
        /// </summary>
        public EntityCatalog<SpellGroup> SpellTypes { get; }

        /// <summary>
        /// Gets the secondary skills.
        /// </summary>
        public EntityCatalog<SecondarySkill> Skills { get; }

        /// <summary>
        /// Gets the artifacts.
        /// </summary>
        public EntityCatalog<Artifact> Artifacts { get; }

        /// <summary>
        /// Gets the spells.
        /// </summary>
        public EntityCatalog<Spell> Spells { get; }

        /// <summary>
        /// Gets the creatures.
        /// </summary>
        public EntityCatalog<Creature> Creatures { get; }

        /// <summary>
        /// Gets the buildings of a town.
        /// </summary>
        public EntityCatalog<Building> Buildings { get; }

        /// <summary>
        /// Gets the places a hero can visit once.
        /// </summary>
        public EntityCatalog<MapPlace> Places { get; }

        /// <summary>
        /// Gets the kinds of mines.
        /// </summary>
        public EntityCatalog<Mine> Mines { get; }

        /// <summary>
        /// Gets the effects a visit leaves on a hero.
        /// </summary>
        public EntityCatalog<VisitEffect> VisitEffects { get; }

        /// <summary>
        /// Gets the objects of the map the editor lists.
        /// </summary>
        public EntityCatalog<MapObject> MapObjects { get; }

        /// <summary>
        /// Gets the faction a castle of no known faction has.
        /// </summary>
        public Faction NeutralFaction { get; }

        /// <summary>
        /// Gets the spell book.
        /// </summary>
        public Artifact SpellBook { get; }

        /// <summary>
        /// Gets the artifact a byte that is no artifact reads as.
        /// </summary>
        public Artifact FallbackArtifact { get; }

        /// <summary>
        /// Gets the creature a byte that is no creature reads as.
        /// </summary>
        public Creature FallbackCreature { get; }

        /// <summary>
        /// Gets the faction a hero belongs to.
        /// </summary>
        /// <param name="id">The number of the hero.</param>
        /// <returns>The faction; the one with code zero when the number is in none.</returns>
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
