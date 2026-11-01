namespace Siniskalc.Homm3.Tests.Integration.Data;

using System.Collections;
using System.Text.Json;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

/// <summary>Frozen exports from 190d2051, independent of the normalized shipped definitions.</summary>
[TestFixture]
[Category(TestKinds.Integration)]
internal sealed class CatalogParityTests
{
    [TestCase(GameVersion.RestorationOfErathia)]
    [TestCase(GameVersion.ArmageddonsBlade)]
    [TestCase(GameVersion.ShadowOfDeath)]
    [TestCase(GameVersion.HeroesChronicles)]
    [TestCase(GameVersion.HornOfTheAbyss)]
    public void EveryMigratedValueMatchesTheIndependentPreMigrationExport(GameVersion version)
    {
        var catalog = TestCatalog.For(version);
        Compare("Faction", catalog.Factions.All);
        Compare("CreatureAbility", catalog.Abilities.All);
        Compare("Building", catalog.Buildings.All);
        Compare("Creature", catalog.Creatures.All);
        Compare("HeroClass", catalog.HeroClasses.All);
        Compare("SecondarySkill", catalog.SecondarySkills.All);
        Compare("SpellSchool", catalog.SpellSchools.All);
        Compare("Spell", catalog.Spells.All);
        Compare("Artifact", catalog.Artifacts.All);
        Compare("WarMachine", catalog.WarMachines.All);

        void Compare<T>(string domain, IReadOnlyList<T> actual) where T : CatalogEntity
        {
            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "CatalogData", "Baseline", domain + ".json")));
            var expected = document.RootElement.EnumerateArray()
                .Where(e => Enum.Parse<GameVersion>(e.GetProperty("availableFrom").GetString()!, true) <= version)
                .Where(e => domain != "Artifact" || e.GetProperty("id").GetString() != "Catapult")
                .ToArray();
            Assert.That(actual.Select(e => e.Id), Is.EqualTo(expected.Select(e => e.GetProperty("id").GetString())), domain + ": order/availability");
            foreach (var (golden, entity) in expected.Zip(actual))
            {
                CompareProperties(golden, entity, domain + "." + entity.Id, "order", "versionInfo", "except", "info", "effectInfo");
                if (entity is Creature creature)
                {
                    var info = golden.GetProperty("versionInfo").TryGetProperty(version.ToString(), out var overridden) ? overridden : golden.GetProperty("info");
                    CompareProperties(info, creature.Info, creature.Id + ".info");
                }

                if (entity is Building building)
                {
                    foreach (var faction in catalog.Factions.All)
                    {
                        var owner = golden.GetProperty("faction");
                        var applies = owner.ValueKind == JsonValueKind.Null
                            ? !golden.GetProperty("except").EnumerateArray().Any(id => id.GetString() == faction.Id)
                            : owner.GetString() == faction.Id;
                        Assert.That(building.IsAvailableFor(faction), Is.EqualTo(applies), building.Id + "." + faction.Id);
                    }
                }

                if (entity is Artifact artifact)
                {
                    var effects = golden.GetProperty("effectInfo");
                    CompareProperties(effects, artifact.EffectInfo, artifact.Id + ".effects", "SpellGrants");
                    var grants = effects.GetProperty("SpellGrants");
                    var expectedGrants = grants.ValueKind == JsonValueKind.Null ? [] : grants.EnumerateArray().ToArray();
                    Assert.That(artifact.EffectInfo.SpellGrants!.Select(g => g.Description),
                        Is.EqualTo(expectedGrants.Select(g => g.GetProperty("description").GetString())), artifact.Id + ".grants");
                    using var spells = JsonDocument.Parse(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "CatalogData", "Baseline", "Spell.json")));
                    var eligible = spells.RootElement.EnumerateArray().Where(s => Enum.Parse<GameVersion>(s.GetProperty("availableFrom").GetString()!, true) <= version).ToArray();
                    var expectedSpells = expectedGrants.SelectMany(g => eligible.Where(s => g.GetProperty("kind").GetString() switch
                    {
                        "named" => g.GetProperty("spells").EnumerateArray().Any(id => id.GetString() == s.GetProperty("id").GetString()),
                        "school" => s.GetProperty("canBeInMageGuild").GetBoolean() && (s.GetProperty("school").GetString() == "all" || string.Equals(s.GetProperty("school").GetString(), g.GetProperty("school").GetString(), StringComparison.OrdinalIgnoreCase)),
                        "level" => s.GetProperty("canBeInMageGuild").GetBoolean() && s.GetProperty("level").GetInt32() == g.GetProperty("level").GetInt32(),
                        _ => false,
                    })).Select(s => s.GetProperty("id").GetString()).Distinct();
                    Assert.That(artifact.GrantedSpells(version).Select(s => s.Id), Is.EquivalentTo(expectedSpells), artifact.Id + ".grantedSpells");
                }
            }
        }
    }

    [Test]
    public void EveryVisitedPlacePreservesItsMetadataAndPhysicalStorage()
    {
        using var golden = JsonDocument.Parse(File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "CatalogData", "Baseline", "VisitedPlaceKind.json")));
        foreach (var entry in golden.RootElement.EnumerateArray())
        {
            var actual = TestCatalog.Hota.VisitablePlaces.ById(entry.GetProperty("id").GetString()!);
            CompareProperties(entry, actual, actual.Id, "order", "heroOffset");
            Assert.That(VisitedPlaceOffsets.For(actual.Slot), Is.EqualTo(entry.GetProperty("heroOffset").GetInt32()));
        }
    }

    private static void CompareProperties(JsonElement expected, object actual, string label, params string[] skip)
    {
        foreach (var property in expected.EnumerateObject().Where(p => !skip.Contains(p.Name)))
        {
            var name = char.ToUpperInvariant(property.Name[0]) + property.Name[1..];
            var reflection = actual.GetType().GetProperty(name);
            Assert.That(reflection, Is.Not.Null, label + "." + property.Name);
            var value = reflection!.GetValue(actual);
            var path = label + "." + property.Name;
            switch (property.Value.ValueKind)
            {
                case JsonValueKind.Null:
                    Assert.That(value is null || value is string text && text.Length == 0 || value is IEnumerable list && !list.Cast<object>().Any(), Is.True, path);
                    break;
                case JsonValueKind.Number:
                    Assert.That(Convert.ToInt64(value), Is.EqualTo(property.Value.GetInt64()), path);
                    break;
                case JsonValueKind.String:
                    if (property.Name == "availableFrom") Assert.That(Convert.ToInt32(value), Is.EqualTo((int)Enum.Parse<GameVersion>(property.Value.GetString()!, true)), path);
                    else if (value is CatalogEntity entity) Assert.That(entity.Id, Is.EqualTo(property.Value.GetString()), path);
                    else if (value is Enum) Assert.That(value, Is.EqualTo(Enum.Parse(value.GetType(), property.Value.GetString()!, true)), path);
                    else Assert.That(value, Is.EqualTo(property.Value.GetString()), path);
                    break;
                case JsonValueKind.Array:
                    Assert.That(((IEnumerable)value!).Cast<CatalogEntity>().Select(e => e.Id), Is.EqualTo(property.Value.EnumerateArray().Select(e => e.GetString())), path);
                    break;
                default:
                    Assert.That(value, Is.EqualTo(property.Value.GetBoolean()), path);
                    break;
            }
        }
    }
}
