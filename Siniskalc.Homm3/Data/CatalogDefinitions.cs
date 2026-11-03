namespace Siniskalc.Homm3.Data;

using System.Collections.Generic;

public sealed record CatalogDefinitions(
    FactionDefinition[] Factions,
    AbilityDefinition[] Abilities,
    BuildingDefinition[] Buildings,
    CreatureDefinition[] Creatures,
    SchoolDefinition[] SpellSchools,
    SpellDefinition[] Spells,
    HeroClassDefinition[] HeroClasses,
    SkillDefinition[] SecondarySkills,
    WarMachineDefinition[] WarMachines,
    ArtifactDefinition[] Artifacts,
    VisitablePlaceDefinition[] VisitablePlaces,
    MineDefinition[] Mines,
    VisitEffectDefinition[] VisitEffects,
    MapMetadataDefinition MapMetadata)
{
    /// <summary>
    /// Gets the file each definition was read from, by domain and identifier, for the messages about a definition that is not valid.
    /// </summary>
    public IReadOnlyDictionary<string, string> SourcePaths { get; init; } = new Dictionary<string, string>();
}
