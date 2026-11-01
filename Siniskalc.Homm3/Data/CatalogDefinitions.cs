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
    public IReadOnlyDictionary<string, string> SourcePaths { get; init; } = new Dictionary<string, string>();
}
