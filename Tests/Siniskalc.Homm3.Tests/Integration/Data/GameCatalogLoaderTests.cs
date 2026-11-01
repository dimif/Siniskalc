namespace Siniskalc.Homm3.Tests.Integration.Data;

using Siniskalc.Core.Data;
using Siniskalc.Core.Runtime;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;

[TestFixture]
[Category(TestKinds.Integration)]
internal sealed class GameCatalogLoaderTests
{
    [Test]
    public void AllSupportedVersionsArePreloadedWithIndependentImmutableIdentities()
    {
        var catalogs = new GameCatalogLoader(new JsonGameDefinitionReader()).Load();
        var hota = catalogs[GameVersion.HornOfTheAbyss];
        var roe = catalogs[GameVersion.RestorationOfErathia];

        Assert.Multiple(() =>
        {
            Assert.That(catalogs, Has.Count.EqualTo(6));
            Assert.That(hota.Creatures.All, Has.Count.EqualTo(189));
            Assert.That(hota.Buildings.All, Has.Count.EqualTo(269));
            Assert.That(hota.Abilities.All, Has.Count.EqualTo(138));
            Assert.That(hota.Artifacts.All, Has.Count.EqualTo(159));
            Assert.That(hota.Factions.All, Has.Count.EqualTo(13));
            Assert.That(hota.Spells.All, Has.Count.EqualTo(70));
            Assert.That(hota.Factions.ById("Castle"), Is.Not.EqualTo(roe.Factions.ById("Castle")));
            Assert.That(hota.Creatures.ById("FaerieDragon").Info.AiValue, Is.EqualTo(30501));
            Assert.That(roe.Creatures.ById("FaerieDragon").Info.AiValue, Is.EqualTo(19580));
        });
    }

    [Test]
    public void MutatingReturnedListsCannotChangeTheSnapshot()
    {
        var catalog = TestCatalog.Hota;
        var before = catalog.Creatures.All.Count;
        Assert.That(() => ((IList<Creature>)catalog.Creatures.All).Clear(), Throws.TypeOf<NotSupportedException>());
        Assert.That(catalog.Creatures.All, Has.Count.EqualTo(before));
        var abilities = catalog.Creatures.ById("Marksman").Info.Special!;
        Assert.That(() => ((IList<Siniskalc.Heroes.Entities.ICreatureAbility>)abilities).Clear(), Throws.TypeOf<NotSupportedException>());
    }

    [TestCase("missingReference", "creatures.json")]
    [TestCase("negativeStats", "creatures.json")]
    [TestCase("upgradeCycle", "creatures.json")]
    [TestCase("wrongUpgradeFaction", "creatures.json")]
    [TestCase("duplicateCode", "factions.json")]
    [TestCase("duplicateId", "factions.json")]
    [TestCase("duplicateOrder", "factions.json")]
    [TestCase("duplicateRole", "factions.json")]
    [TestCase("nullPrerequisites", "buildings.json")]
    [TestCase("wrongContext", "buildings.json")]
    [TestCase("spellCapacity", "spells.json")]
    [TestCase("skillCapacity", "secondarySkills.json")]
    [TestCase("invalidSlots", "artifacts.json")]
    [TestCase("invalidBookRole", "artifacts.json")]
    [TestCase("nullGrant", "artifacts.json")]
    [TestCase("unsupportedGrant", "artifacts.json")]
    [TestCase("nullMine", "mapObjects.json")]
    [TestCase("invalidMachine", "warMachines.json")]
    [TestCase("undeclaredReplacement", "creatures.json")]
    [TestCase("missingVersion", "manifest.json")]
    [TestCase("parentCycle", "manifest.json")]
    [TestCase("nullVersion", "manifest.json")]
    [TestCase("emptyDirectory", "manifest.json")]
    [TestCase("outsideDirectory", "manifest.json")]
    public void InvalidDefinitionsFailActivationWithTheSourceFileAndCause(string error, string file)
    {
        var exception = Assert.Throws<GameDefinitionException>(() => new GameCatalogLoader(new ChangingReader((path, value) =>
        {
            if (value is CatalogManifest manifest)
            {
                if (error == "parentCycle") manifest.Sets["standard"] = manifest.Sets["standard"] with { Parent = "hota" };
                if (error == "missingVersion") manifest.Versions.Remove(GameVersion.ShadowOfDeath);
                if (error == "nullVersion") manifest.Versions[GameVersion.ShadowOfDeath] = null!;
                if (error == "emptyDirectory") manifest.Sets["standard"] = manifest.Sets["standard"] with { Directory = "" };
                if (error == "outsideDirectory") manifest.Sets["standard"] = manifest.Sets["standard"] with { Directory = @"..\Homm2" };
            }

            if (path.Contains(@"\standard\", StringComparison.Ordinal))
            {
                if (value is CreatureDefinition[] creatures)
                    creatures[0] = error switch
                    {
                        "missingReference" => creatures[0] with { Faction = "no-such-faction" },
                        "negativeStats" => creatures[0] with { Info = creatures[0].Info with { Health = 0 } },
                        "upgradeCycle" => creatures[0] with { BaseCreature = creatures[0].Id },
                        "wrongUpgradeFaction" => creatures[0] with { BaseCreature = "Pikeman" },
                        _ => creatures[0],
                    };
                if (value is FactionDefinition[] factions)
                {
                    if (error == "duplicateId") return factions.Append(factions[0]).ToArray();
                    factions[1] = error switch
                    {
                        "duplicateCode" => factions[1] with { Code = factions[0].Code },
                        "duplicateOrder" => factions[1] with { Order = factions[0].Order },
                        "duplicateRole" => factions[1] with { IsNeutral = true },
                        _ => factions[1],
                    };
                }

                if (value is BuildingDefinition[] buildings)
                    buildings[0] = error switch
                    {
                        "nullPrerequisites" => buildings[0] with { Requires = null! },
                        "wrongContext" => buildings[0] with { Requires = ["CastleGuardhouse"] },
                        _ => buildings[0],
                    };
                if (error == "spellCapacity" && value is SpellDefinition[] spells) spells[0] = spells[0] with { Code = 70 };
                if (error == "skillCapacity" && value is SkillDefinition[] skills) skills[0] = skills[0] with { Code = 30 };
                if (error == "invalidMachine" && value is WarMachineDefinition[] machines) machines[0] = machines[0] with { Encoding = (WarMachineEncoding)999 };
                if (value is ArtifactDefinition[] artifacts)
                {
                    if (error == "invalidSlots") artifacts[0] = artifacts[0] with { Slot = (ArtifactSlots)(1 << 20) };
                    if (error == "invalidBookRole")
                    {
                        var index = Array.FindIndex(artifacts, a => a.Role == ArtifactRole.SpellBook);
                        artifacts[index] = artifacts[index] with { Wearable = true };
                    }

                    if (error == "nullGrant") artifacts[0] = artifacts[0] with { EffectInfo = artifacts[0].EffectInfo with { SpellGrants = [null!] } };
                    if (error == "unsupportedGrant") artifacts[0] = artifacts[0] with { EffectInfo = artifacts[0].EffectInfo with { SpellGrants =
                        [new SpellGrantDefinition { Kind = (SpellGrantKind)999, Spells = [], Description = "", School = null, Level = null }] } };
                }
            }

            if (error == "nullMine" && value is MapMetadataDefinition metadata) metadata.Mines[0] = null!;
            if (error == "undeclaredReplacement" && path.Contains(@"\hota\", StringComparison.Ordinal) && value is CreatureDefinition[] replacements)
                replacements[0] = replacements[0] with { Replaces = false };
            return value;
        })).Load());
        Assert.That(exception!.Path, Does.EndWith(file));
        Assert.That(exception.Message, Does.Contain("HoMM3"));
    }

    [Test]
    public void FallbackAndScrollRolesDoNotDependOnDisplayNamesOrJsonOrder()
    {
        var catalog = new GameCatalogLoader(new ChangingReader((_, value) =>
        {
            if (value is Array array) Array.Reverse(array);
            if (value is FactionDefinition[] factions)
            {
                var at = Array.FindIndex(factions, f => f.IsNeutral);
                if (at >= 0) factions[at] = factions[at] with { Name = "Unaffiliated", Order = 1000 };
            }

            if (value is CreatureDefinition[] creatures)
            {
                var at = Array.FindIndex(creatures, c => c.Fallback);
                if (at >= 0) creatures[at] = creatures[at] with { Name = "Default creature", Order = 1000 };
            }

            if (value is ArtifactDefinition[] artifacts)
            {
                var at = Array.FindIndex(artifacts, a => a.Role == ArtifactRole.Fallback);
                if (at >= 0) artifacts[at] = artifacts[at] with { Name = "Default artifact", Order = 1000 };
                at = Array.FindIndex(artifacts, a => a.Role == ArtifactRole.Scroll);
                if (at >= 0) artifacts[at] = artifacts[at] with { Name = "Rune Paper" };
            }

            return value;
        })).Load()[GameVersion.HornOfTheAbyss];
        Assert.That(catalog.NeutralFaction.Name, Is.EqualTo("Unaffiliated"));
        Assert.That(catalog.FallbackCreature.Name, Is.EqualTo("Default creature"));
        Assert.That(catalog.FallbackArtifact.Name, Is.EqualTo("Default artifact"));
        Assert.That(SpellScroll.NameOf(catalog.Spells.ById("Haste")), Is.EqualTo("Rune Paper: Haste"));
        Assert.That(SpellScroll.TryParse(catalog, "Rune Paper: Haste", out var spell), Is.True);
        Assert.That(spell, Is.SameAs(catalog.Spells.ById("Haste")));
        var bytes = Enumerable.Repeat(byte.MaxValue, MerchantLayout.ListSize).ToArray();
        BitConverter.GetBytes(400u).CopyTo(bytes, MerchantLayout.SlotAt(0));
        var chunk = new RecordChunk("merchant", bytes, 0, 0);
        var model = new MerchantStockDeserializer(catalog).Deserialize(chunk);
        Assert.That(model.Slots[0], Is.SameAs(catalog.FallbackArtifact));
        Assert.That(new SafeMerchantStockSerializer(catalog).Serialize(chunk, model).HasSameBytes(chunk), Is.True);
    }

    private sealed class ChangingReader(Func<string, object, object> change) : IGameDefinitionReader
    {
        private readonly JsonGameDefinitionReader reader = new();
        public T Read<T>(string relativePath) where T : class => (T)change(relativePath, this.reader.Read<T>(relativePath));
    }
}
