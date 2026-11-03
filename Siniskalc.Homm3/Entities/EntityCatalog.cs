namespace Siniskalc.Homm3.Entities;

using System.Collections.Generic;
using System.Linq;
using Siniskalc.Homm3.Data;

/// <summary>The deeply immutable definitions of one standard version, shared by every layer of its documents.</summary>
public sealed class EntityCatalog
{
    /// <summary>
    /// Initializes a new instance of the <see cref="None"/> class.
    /// </summary>
    /// <param name="version">The version.</param>
    /// <param name="definitions">The definitions.</param>
    internal EntityCatalog(GameVersion version, CatalogDefinitions definitions)
    {
        this.Version = version;
        this.MapMetadata = new MapMetadata(definitions.MapMetadata, definitions.Mines, definitions.VisitEffects);
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
        this.VisitablePlaces = new(definitions.VisitablePlaces.Select(d => new VisitedPlaceKind(this, d)), version);
        this.NeutralFaction = this.Factions.ById("Neutral");
        this.FallbackCreature = this.Creatures.Find(0)!;
        this.FallbackHeroClass = this.HeroClasses.Find(0)!;
        this.Scroll = this.Artifacts.ById(definitions.Artifacts.Single(d => d.Role == ArtifactRole.Scroll).Id);
        this.SpellBook = this.Artifacts.ById(definitions.Artifacts.Single(d => d.Role == ArtifactRole.SpellBook).Id);
        this.FallbackArtifact = this.Artifacts.Find(0)!;
    }

    /// <summary>
    /// Gets the version of the game the catalog is for.
    /// </summary>
    public GameVersion Version { get; }

    /// <summary>
    /// Gets what the editor knows of the objects of the map.
    /// </summary>
    public MapMetadata MapMetadata { get; }

    /// <summary>
    /// Gets the factions.
    /// </summary>
    public EntityCollection<Faction> Factions { get; }

    /// <summary>
    /// Gets the abilities of the creatures.
    /// </summary>
    public EntityCollection<CreatureAbility> Abilities { get; }

    /// <summary>
    /// Gets the buildings of a town.
    /// </summary>
    public EntityCollection<Building> Buildings { get; }

    /// <summary>
    /// Gets the creatures.
    /// </summary>
    public EntityCollection<Creature> Creatures { get; }

    /// <summary>
    /// Gets the schools of magic.
    /// </summary>
    public EntityCollection<SpellSchool> SpellSchools { get; }

    /// <summary>
    /// Gets the spells.
    /// </summary>
    public EntityCollection<Spell> Spells { get; }

    /// <summary>
    /// Gets the classes of heroes.
    /// </summary>
    public EntityCollection<HeroClass> HeroClasses { get; }

    /// <summary>
    /// Gets the secondary skills.
    /// </summary>
    public EntityCollection<SecondarySkill> SecondarySkills { get; }

    /// <summary>
    /// Gets the war machines.
    /// </summary>
    public EntityCollection<WarMachine> WarMachines { get; }

    /// <summary>
    /// Gets the artifacts.
    /// </summary>
    public EntityCollection<Artifact> Artifacts { get; }

    /// <summary>
    /// Gets the kinds of places a hero can visit.
    /// </summary>
    public EntityCollection<VisitedPlaceKind> VisitablePlaces { get; }

    /// <summary>
    /// Gets the faction a castle of no known faction has.
    /// </summary>
    public Faction NeutralFaction { get; }

    /// <summary>
    /// Gets the creature a code that is no creature reads as.
    /// </summary>
    public Creature FallbackCreature { get; }

    /// <summary>
    /// Gets the class a byte that is no class reads as.
    /// </summary>
    public HeroClass FallbackHeroClass { get; }

    /// <summary>
    /// Gets the spell scroll.
    /// </summary>
    public Artifact Scroll { get; }

    /// <summary>
    /// Gets the spell book.
    /// </summary>
    public Artifact SpellBook { get; }

    /// <summary>
    /// Gets the artifact a code that is no artifact reads as.
    /// </summary>
    public Artifact FallbackArtifact { get; }

    /// <summary>
    /// Gets the artifacts a hero can wear.
    /// </summary>
    public IReadOnlyList<Artifact> WearableArtifacts => this.Artifacts.All.Where(a => a.Wearable).ToArray().AsReadOnly();

    /// <summary>
    /// Gets the artifacts a hero can carry in the backpack.
    /// </summary>
    public IReadOnlyList<Artifact> BackpackArtifacts => this.Artifacts.All.Where(a => a.CanBeInBackpack).ToArray().AsReadOnly();

    /// <summary>
    /// Finds an artifact by its name.
    /// </summary>
    /// <param name="name">The name, ignoring the case.</param>
    /// <returns>The artifact, or <see langword="null"/> when none has the name.</returns>
    public Artifact? FindArtifact(string name) => this.Artifacts.FindByName(name)
        ?? (SpellScroll.TryParse(this, name, out _) ? this.Scroll : null);
}
