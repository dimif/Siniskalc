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
            Assert.That(hota.Artifacts.All, Has.Count.EqualTo(158));
            Assert.That(hota.Factions.All, Has.Count.EqualTo(13));
            Assert.That(hota.Spells.All, Has.Count.EqualTo(70));
            Assert.That(hota.Factions.ById("Castle"), Is.Not.EqualTo(roe.Factions.ById("Castle")));
            Assert.That(hota.Creatures.ById("FaerieDragon").Info.AiValue, Is.EqualTo(30501));
            Assert.That(roe.Creatures.ById("FaerieDragon").Info.AiValue, Is.EqualTo(19580));
        });
    }

    [Test]
    public void WarMachinesAreNotArtifactsAndRemainAvailableSeparately()
    {
        var catalogs = new GameCatalogLoader(new JsonGameDefinitionReader()).Load();
        foreach (var catalog in catalogs.Values)
        {
            Assert.That(catalog.Artifacts.All.Select(a => a.Id),
                Has.None.EqualTo("Catapult"));
            Assert.That(catalog.Artifacts.All.Select(a => a.Id).Intersect(catalog.WarMachines.All.Select(w => w.Id)), Is.Empty);
            Assert.That(catalog.WarMachines.ById("Ballista").Code, Is.EqualTo(4));
            Assert.That(catalog.WarMachines.ById("AmmoCart").Code, Is.EqualTo(5));
            Assert.That(catalog.WarMachines.ById("FirstAidTent").Code, Is.EqualTo(6));
        }

        Assert.That(catalogs[GameVersion.HornOfTheAbyss].WarMachines.ById("Cannon").Code, Is.EqualTo(146));
    }

    [Test]
    public void EveryShippedVersionUsesTheStandardFilesAndNoDefinitionIsReadTwice()
    {
        var paths = new List<string>();
        var catalogs = new GameCatalogLoader(new ChangingReader((path, value) =>
        {
            paths.Add(path);
            return value;
        })).Load();

        Assert.That(paths, Has.None.Contains(@"\hota\"));
        Assert.That(paths, Does.Contain(@"GameData\Homm3\standard\mines.json"));
        Assert.That(paths, Does.Contain(@"GameData\Homm3\standard\visitEffects.json"));
        Assert.That(paths, Does.Contain(@"GameData\Homm3\standard\mapObjects.json"));
        Assert.That(paths, Does.Not.Contain(@"GameData\Homm3\mapObjects.json"));
        Assert.That(paths.Distinct().Count(), Is.EqualTo(paths.Count));
        Assert.That(catalogs[GameVersion.HornOfTheAbyss].Creatures.ById("FaerieDragon").Info.AiValue, Is.EqualTo(30501));
        Assert.That(catalogs[GameVersion.ShadowOfDeath].Creatures.ById("FaerieDragon").Info.AiValue, Is.EqualTo(19580));
    }

    [Test]
    public void SeparateMapDefinitionFilesSupplyTheirNamesDescriptionsAndParameters()
    {
        var catalog = new GameCatalogLoader(new ChangingReader((_, value) => value switch
        {
            MineDefinition[] mines => mines.Select(m => m with { Name = m.Name + " fixture", DailyAmount = 7 }).ToArray(),
            VisitEffectDefinition[] effects => effects.Select(e => e with { Description = "A fixture visit effect" }).ToArray(),
            VisitablePlaceDefinition[] places => places.Select(p => p with { Description = "A fixture permanent visit" }).ToArray(),
            MapMetadataDefinition metadata => metadata with { MerchantName = "Fixture market" },
            _ => value,
        })).Load()[GameVersion.HornOfTheAbyss];

        var mine = catalog.MapMetadata.Mine(Siniskalc.Heroes.Entities.MineKind.Sawmill);
        Assert.That(mine.Name, Is.EqualTo("Sawmill fixture"));
        Assert.That(mine.DailyAmount, Is.EqualTo(7));
        Assert.That(catalog.MapMetadata.VisitEffects.All(e => e.Description == "A fixture visit effect"), Is.True);
        Assert.That(catalog.VisitablePlaces.All.All(p => p.ToolTip == "A fixture permanent visit"), Is.True);
        Assert.That(catalog.MapMetadata.MerchantName, Is.EqualTo("Fixture market"));
    }

    [Test]
    public void OmittedArtifactAndSpellFlagsUseTheCommonDefaults()
    {
        var reader = new JsonGameDefinitionReader();
        var artifacts = reader.Read<ArtifactDefinition[]>(@"GameData\Homm3\standard\artifacts.json");
        var spells = reader.Read<SpellDefinition[]>(@"GameData\Homm3\standard\spells.json");

        Assert.That(artifacts.Single(a => a.Id == "HelmOfTheAlabasterUnicorn").Wearable, Is.True);
        Assert.That(artifacts.Single(a => a.Id == "HelmOfTheAlabasterUnicorn").Backpack, Is.True);
        Assert.That(artifacts.Single(a => a.Role == ArtifactRole.SpellBook).Wearable, Is.False);
        Assert.That(artifacts.Single(a => a.Role == ArtifactRole.SpellBook).Backpack, Is.False);
        Assert.That(spells.Single(s => s.Id == "Haste").HeroOnly, Is.False);
        Assert.That(spells.Any(s => s.HeroOnly), Is.True);
    }

    [Test]
    public void FactionsFollowTheirJsonArrayOrderWithoutChangingTheirSaveCodesOrNeutralIdentity()
    {
        var catalog = new GameCatalogLoader(new ChangingReader((_, value) =>
            value is FactionDefinition[] factions ? factions.AsEnumerable().Reverse().ToArray() : value))
            .Load()[GameVersion.HornOfTheAbyss];

        Assert.That(catalog.Factions.All.Select(f => f.Id), Is.EqualTo(TestCatalog.Hota.Factions.All.Select(f => f.Id).Reverse()));
        Assert.That(catalog.Factions.Find(0)!.Id, Is.EqualTo("Castle"));
        Assert.That(catalog.NeutralFaction.Id, Is.EqualTo("Neutral"));
    }

    [Test]
    public void TheWellKnownAllSchoolIsUniversalRegardlessOfItsNameAndArrayPosition()
    {
        var catalog = new GameCatalogLoader(new ChangingReader((_, value) =>
        {
            if (value is not SchoolDefinition[] schools)
            {
                return value;
            }

            return schools.AsEnumerable().Reverse().Select(s => s.Id == "all" ? s with { Name = "Every school" } : s).ToArray();
        })).Load()[GameVersion.HornOfTheAbyss];

        Assert.That(catalog.SpellSchools.ById("all").Universal, Is.True);
        Assert.That(catalog.SpellSchools.All.Where(s => s.Universal).Select(s => s.Id), Is.EqualTo(new[] { "all" }));
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
    [TestCase("missingNeutral", "factions.json")]
    [TestCase("missingUniversalSchool", "spellSchools.json")]
    [TestCase("nullPrerequisites", "buildings.json")]
    [TestCase("wrongContext", "buildings.json")]
    [TestCase("spellCapacity", "spells.json")]
    [TestCase("skillCapacity", "secondarySkills.json")]
    [TestCase("invalidSlots", "artifacts.json")]
    [TestCase("invalidBookRole", "artifacts.json")]
    [TestCase("nullGrant", "artifacts.json")]
    [TestCase("unsupportedGrant", "artifacts.json")]
    [TestCase("nullMine", "mines.json")]
    [TestCase("missingMine", "mines.json")]
    [TestCase("invalidMineIncome", "mines.json")]
    [TestCase("nullVisitEffect", "visitEffects.json")]
    [TestCase("duplicateVisitEffect", "visitEffects.json")]
    [TestCase("emptyMapObjectName", "mapObjects.json")]
    [TestCase("invalidMachine", "warMachines.json")]
    [TestCase("undeclaredReplacement", "creatures.json")]
    [TestCase("invalidVersionInfo", "creatures.json")]
    [TestCase("nullVersionInfo", "creatures.json")]
    [TestCase("invalidVersionStats", "creatures.json")]
    [TestCase("invalidHiddenBaseStats", "creatures.json")]
    [TestCase("duplicateModifier", "artifacts.json")]
    [TestCase("nullModifier", "artifacts.json")]
    [TestCase("invalidModifier", "artifacts.json")]
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
                if (error == "parentCycle") manifest.Sets["standard"] = manifest.Sets["standard"] with { Parent = "standard" };
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
                        "invalidVersionInfo" => creatures[0] with { VersionInfo = new() { [GameVersion.Unknown] = creatures[0].Info } },
                        "nullVersionInfo" => creatures[0] with { VersionInfo = null! },
                        "invalidVersionStats" => creatures[0] with { VersionInfo = new() { [GameVersion.HornOfTheAbyss] = creatures[0].Info with { Health = 0 } } },
                        "invalidHiddenBaseStats" => creatures[0] with
                        {
                            Info = creatures[0].Info with { Health = 0 },
                            VersionInfo = Enum.GetValues<GameVersion>().Where(v => v != GameVersion.Unknown).ToDictionary(v => v, _ => creatures[0].Info),
                        },
                        _ => creatures[0],
                    };
                if (value is FactionDefinition[] factions)
                {
                    if (error == "duplicateId") return factions.Append(factions[0]).ToArray();
                    if (error == "missingNeutral") return factions.Where(f => f.Id != "Neutral").ToArray();
                    factions[1] = error switch
                    {
                        "duplicateCode" => factions[1] with { Code = factions[0].Code },
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
                if (error == "missingUniversalSchool" && value is SchoolDefinition[] schools) return schools.Where(s => s.Id != "all").ToArray();
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
                    if (error == "nullModifier") artifacts[0] = artifacts[0] with { EffectInfo = artifacts[0].EffectInfo with { Modifiers = [null!] } };
                    if (error == "duplicateModifier") artifacts[0] = artifacts[0] with
                    {
                        EffectInfo = artifacts[0].EffectInfo with
                        {
                            Modifiers =
                        [new() { Attribute = Siniskalc.Heroes.Entities.ArtifactAttribute.Attack, Amount = 1 }, new() { Attribute = Siniskalc.Heroes.Entities.ArtifactAttribute.Attack, Amount = 2 }]
                        }
                    };
                    if (error == "invalidModifier") artifacts[0] = artifacts[0] with
                    {
                        EffectInfo = artifacts[0].EffectInfo with
                        {
                            Modifiers =
                        [new() { Attribute = (Siniskalc.Heroes.Entities.ArtifactAttribute)999, Amount = 1 }]
                        }
                    };
                    if (error == "unsupportedGrant") artifacts[0] = artifacts[0] with
                    {
                        EffectInfo = artifacts[0].EffectInfo with
                        {
                            SpellGrants =
                        [new SpellGrantDefinition { Kind = (SpellGrantKind)999, Spells = [], Description = "", School = null, Level = null }]
                        }
                    };
                }
            }

            if (value is MineDefinition[] mines)
            {
                if (error == "nullMine") mines[0] = null!;
                if (error == "missingMine") return mines.Skip(1).ToArray();
                if (error == "invalidMineIncome") mines[0] = mines[0] with { DailyAmount = -1 };
            }

            if (value is VisitEffectDefinition[] effects)
            {
                if (error == "nullVisitEffect") effects[0] = null!;
                if (error == "duplicateVisitEffect") return effects.Append(effects[0]).ToArray();
            }

            if (error == "emptyMapObjectName" && value is MapMetadataDefinition metadata)
                return metadata with { MerchantName = "" };
            if (error == "undeclaredReplacement" && value is CreatureDefinition[] replacements)
                replacements[0] = replacements[0] with { Replaces = true };
            return value;
        })).Load());
        Assert.That(exception!.Path, Does.EndWith(file));
        Assert.That(exception.Message, Does.Contain("HoMM3"));
    }

    [Test]
    public void CodeZeroStandInsAndScrollRolesDoNotDependOnDisplayNamesOrJsonOrder()
    {
        var catalog = new GameCatalogLoader(new ChangingReader((_, value) =>
        {
            if (value is Array array) Array.Reverse(array);
            if (value is FactionDefinition[] factions)
            {
                var at = Array.FindIndex(factions, f => f.Id == "Neutral");
                if (at >= 0) factions[at] = factions[at] with { Name = "Unaffiliated" };
            }

            if (value is CreatureDefinition[] creatures)
            {
                var at = Array.FindIndex(creatures, c => c.Code == 0);
                if (at >= 0) creatures[at] = creatures[at] with { Name = "Default creature" };
            }

            if (value is ArtifactDefinition[] artifacts)
            {
                var at = Array.FindIndex(artifacts, a => a.Code == 0);
                if (at >= 0) artifacts[at] = artifacts[at] with { Name = "Default artifact" };
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
