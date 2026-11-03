namespace Siniskalc.Homm2.Data
{
    /// <summary>Validated construction input, never exposed by the runtime catalog.</summary>
    internal sealed record CatalogDefinitions(
        FactionDefinition[] Factions,
        AbilityDefinition[] Abilities,
        SkillDefinition[] Skills,
        ArtifactDefinition[] Artifacts,
        CreatureDefinition[] Creatures,
        SpellDefinition[] Spells,
        SpellTypeDefinition[] SpellTypes,
        BuildingDefinition[] Buildings,
        PlaceDefinition[] Places,
        MineDefinition[] Mines,
        VisitEffectDefinition[] VisitEffects,
        MapObjectDefinition[] MapObjects);
}
