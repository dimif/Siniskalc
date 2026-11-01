namespace Siniskalc.Homm3.Entities;

using System.Collections.Generic;
using System.Linq;
using Siniskalc.Homm3.Data;

/// <summary>The deeply immutable definitions of one standard version, shared by every layer of its documents.</summary>
public sealed class EntityCatalog
{
    internal EntityCatalog(GameVersion version, CatalogDefinitions definitions)
    {
        this.Version = version;
        this.MapMetadata = new MapMetadata(definitions.MapMetadata);
        this.Factions = new(definitions.Factions.Select(d => new Faction(this, d)), version);
        this.Abilities = new(definitions.Abilities.Select(d => new CreatureAbility(this, d)), version);
        this.SpellSchools = new(definitions.SpellSchools.Select(d => new SpellSchool(this, d)), version);
        this.Spells = new(definitions.Spells.Select(d => new Spell(this, d)), version);
        this.Buildings = new(definitions.Buildings.Select(d => new Building(this, d)), version, contextualCodes: true);
        this.Creatures = new(definitions.Creatures.Select(d => new Creature(this, d)), version);
        this.HeroClasses = new(definitions.HeroClasses.Select(d => new HeroClass(this, d)), version);
        this.SecondarySkills = new(definitions.SecondarySkills.Select(d => new SecondarySkill(this, d)), version);
        this.WarMachines = new(definitions.WarMachines.Select(d => new WarMachine(this, d)), version);
        this.Artifacts = new(definitions.Artifacts.Select(d => new Artifact(this, d)), version);
        this.VisitedPlaces = new(definitions.VisitedPlaces.Select(d => new VisitedPlaceKind(this, d)), version);
        this.NeutralFaction = this.Factions.ById(definitions.Factions.Single(d => d.IsNeutral).Id);
        this.FallbackCreature = this.Creatures.ById(definitions.Creatures.Single(d => d.Fallback).Id);
        this.FallbackHeroClass = this.HeroClasses.ById(definitions.HeroClasses.Single(d => d.Fallback).Id);
        this.Scroll = this.Artifacts.ById(definitions.Artifacts.Single(d => d.Role == ArtifactRole.Scroll).Id);
        this.SpellBook = this.Artifacts.ById(definitions.Artifacts.Single(d => d.Role == ArtifactRole.SpellBook).Id);
        this.FallbackArtifact = this.Artifacts.ById(definitions.Artifacts.Single(d => d.Role == ArtifactRole.Fallback).Id);
    }

    public GameVersion Version { get; }
    public MapMetadata MapMetadata { get; }
    public EntityCollection<Faction> Factions { get; }
    public EntityCollection<CreatureAbility> Abilities { get; }
    public EntityCollection<Building> Buildings { get; }
    public EntityCollection<Creature> Creatures { get; }
    public EntityCollection<SpellSchool> SpellSchools { get; }
    public EntityCollection<Spell> Spells { get; }
    public EntityCollection<HeroClass> HeroClasses { get; }
    public EntityCollection<SecondarySkill> SecondarySkills { get; }
    public EntityCollection<WarMachine> WarMachines { get; }
    public EntityCollection<Artifact> Artifacts { get; }
    public EntityCollection<VisitedPlaceKind> VisitedPlaces { get; }
    public Faction NeutralFaction { get; }
    public Creature FallbackCreature { get; }
    public HeroClass FallbackHeroClass { get; }
    public Artifact Scroll { get; }
    public Artifact SpellBook { get; }
    public Artifact FallbackArtifact { get; }
    public IReadOnlyList<Artifact> WearableArtifacts => this.Artifacts.All.Where(a => a.Wearable).ToArray().AsReadOnly();
    public IReadOnlyList<Artifact> BackpackArtifacts => this.Artifacts.All.Where(a => a.CanBeInBackpack).ToArray().AsReadOnly();

    public Artifact? FindArtifact(string name) => this.Artifacts.FindByName(name)
        ?? (SpellScroll.TryParse(this, name, out _) ? this.Scroll : null);
}
