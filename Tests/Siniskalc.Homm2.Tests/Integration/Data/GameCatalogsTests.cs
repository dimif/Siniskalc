namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class GameCatalogsTests
    {
        [Test]
        public void StandardVersionsExplicitlyReuseTheSameImmutableSnapshot()
        {
            var catalogs = new GameCatalogs(new JsonGameDefinitionReader());

            Assert.That(catalogs.For(GameVersion.BaseGame), Is.SameAs(catalogs.For(GameVersion.PriceOfLoyalty)));
            Assert.That(catalogs.For(GameVersion.Unknown), Is.SameAs(catalogs.For(GameVersion.BaseGame)));
            Assert.That(catalogs.For(GameVersion.BaseGame).SetId, Is.EqualTo("standard"));
            Assert.That(catalogs.For(GameVersion.BaseGame).Artifacts.Count(a => a.IsAvailableIn(GameVersion.BaseGame)), Is.EqualTo(82));
            Assert.That(catalogs.For(GameVersion.PriceOfLoyalty).Artifacts.Count(a => a.IsAvailableIn(GameVersion.PriceOfLoyalty)), Is.EqualTo(99));
        }

        [Test]
        public void SeparateStartupSnapshotsNeverCompareTheirEntitiesAsEqual()
        {
            var first = new GameCatalogs(new JsonGameDefinitionReader()).For(GameVersion.BaseGame);
            var second = new GameCatalogs(new JsonGameDefinitionReader()).For(GameVersion.BaseGame);

            Assert.That(first.Creatures[0], Is.Not.EqualTo(second.Creatures[0]));
            Assert.That(first.Artifacts[0], Is.Not.EqualTo(second.Artifacts[0]));
            Assert.That(first.Factions[0], Is.Not.EqualTo(second.Factions[0]));
        }

        [Test]
        public void CatalogCollectionsAndTheirNestedAbilityAndEffectListsCannotBeMutated()
        {
            var catalog = TestCatalog.Standard;

            Assert.That(catalog.Creatures, Is.Not.InstanceOf<IList<Creature>>());
            Assert.That(() => ((IList<Siniskalc.Heroes.Entities.ICreatureAbility>)catalog.Creatures["Archer"].Info.Special!).Clear(), Throws.TypeOf<NotSupportedException>());
            Assert.That(() => ((IList<ArtifactEffect>)catalog.Artifacts[0].Effects).Clear(), Throws.TypeOf<NotSupportedException>());
            Assert.That(() => ((IList<int>)catalog.Factions[0].GuildSpellCounts).Clear(), Throws.TypeOf<NotSupportedException>());
        }

        [Test]
        public void MutatingDefinitionArraysAfterConstructionCannotChangeASnapshot()
        {
            var reader = new DefinitionMutationReader();
            AbilityDefinition[]? abilities = null;
            string[]? creatureAbilities = null;
            int[]? heroIds = null;
            reader.Replace<DefinitionFile<AbilityDefinition>>("abilities", f => { abilities = f.Entries; return f; });
            reader.Replace<DefinitionFile<CreatureDefinition>>("creatures", f => { creatureAbilities = f.Entries[1].Abilities; return f; });
            reader.Replace<DefinitionFile<FactionDefinition>>("factions", f => { heroIds = f.Entries[0].HeroIds; return f; });
            var catalog = new GameCatalogs(reader).For(GameVersion.BaseGame);
            abilities![0] = abilities[0] with { Name = "Changed" };
            creatureAbilities![0] = "Missing";
            heroIds![0] = 53;

            Assert.That(catalog.Abilities[0].Name, Is.EqualTo("Ranged"));
            Assert.That(catalog.Creatures["Archer"].Info.Special!.Single().Name, Is.EqualTo("Ranged"));
            Assert.That(catalog.FactionForHero(0).Name, Is.EqualTo("Knight"));
        }

        [Test]
        public void AChildSetAddsEntitiesAndReplacesWholeRecordsWithoutChangingItsParent()
        {
            var catalogs = new GameCatalogs(new DefinitionFixtureReader());
            var original = catalogs.For(GameVersion.BaseGame);
            var expansion = catalogs.For(GameVersion.PriceOfLoyalty);

            Assert.That(original.Factions.Find(6), Is.Null);
            Assert.That(expansion.Factions.Find(6)!.Name, Is.EqualTo("Observatory"));
            Assert.That(original.Spells.Find(65)!.Name, Is.EqualTo("Set Water Guardian"));
            Assert.That(expansion.Spells.Find(65)!.Name, Is.EqualTo("Star Ward"));
            Assert.That(original.FactionForHero(0).Name, Is.EqualTo("Knight"));
            Assert.That(expansion.FactionForHero(0).Name, Is.EqualTo("Observatory"));
            Assert.That(original.Spells.Find(65), Is.Not.EqualTo(expansion.Spells.Find(65)));
        }

        [Test]
        public void DuplicateStableIdsAreRejectedBeforeActivation() =>
            Refuses<AbilityDefinition>("abilities", f => f with { Entries = [.. f.Entries, f.Entries[0]] }, "Duplicate IDs");

        [Test]
        public void DuplicateSaveCodesAreRejectedBeforeActivation() =>
            Refuses<CreatureDefinition>("creatures", f => f with { Entries = [f.Entries[0] with { Code = f.Entries[1].Code }, .. f.Entries.Skip(1)] }, "Duplicate save codes");

        [Test]
        public void DuplicateOrderingIsRejectedBeforeActivation() =>
            Refuses<SpellDefinition>("spells", f => f with { Entries = [f.Entries[0] with { Order = f.Entries[1].Order }, .. f.Entries.Skip(1)] }, "Duplicate ordering");

        [Test]
        public void AnExplicitNullDisplayNameIsRejectedBeforeActivation() =>
            Refuses<AbilityDefinition>("abilities", f => f with { Entries = [f.Entries[0] with { Name = null! }, .. f.Entries.Skip(1)] }, ".name");

        [Test]
        public void AnExplicitNullCreatureStatRecordIsRejectedBeforeActivation() =>
            Refuses<CreatureDefinition>("creatures", f => f with { Entries = [f.Entries[0] with { Stats = null! }, .. f.Entries.Skip(1)] }, "Peasant");

        [Test]
        public void AnUnknownAbilityReferenceIsRejectedBeforeActivation() =>
            Refuses<CreatureDefinition>("creatures", f => f with { Entries = [f.Entries[0] with { Abilities = ["Missing"] }, .. f.Entries.Skip(1)] }, "Peasant.abilities");

        [Test]
        public void AnUnknownEffectReferenceIsRejectedBeforeActivation() =>
            Refuses<ArtifactDefinition>("artifacts", f => f with { Entries = [f.Entries[0] with { Effects = ["Missing"] }, .. f.Entries.Skip(1)] }, "UltimateBookOfKnowledge.effects");

        [Test]
        public void CreatureUpgradeCyclesAreRejectedBeforeActivation() =>
            Refuses<CreatureDefinition>("creatures", f => f with { Entries = [f.Entries[0] with { BaseCreature = f.Entries[0].Id }, .. f.Entries.Skip(1)] }, "Upgrade cycle");

        [Test]
        public void TheEmptyArmyCodeCannotBeAssignedToACreature() =>
            Refuses<CreatureDefinition>("creatures", f => f with { Entries = [f.Entries[0] with { Code = 255 }, .. f.Entries.Skip(1)] }, "Peasant");

        [Test]
        public void ASpellCannotEnlargeTheFixedSpellBitset() =>
            Refuses<SpellDefinition>("spells", f => f with { Entries = [f.Entries[0] with { Code = 66 }, .. f.Entries.Skip(1)] }, "Fireball");

        [Test]
        public void ASkillCannotEnlargeTheFixedSkillTable() =>
            Refuses<SkillDefinition>("skills", f => f with { Entries = [f.Entries[0] with { Code = 14 }, .. f.Entries.Skip(1)] }, "fixed table");

        [Test]
        public void AHeroMappingCannotEnlargeTheFixedHeroTable() =>
            Refuses<FactionDefinition>("factions", f => f with { Entries = [f.Entries[0] with { HeroIds = [54] }, .. f.Entries.Skip(1)] }, "Knight.heroIds");

        [Test]
        public void AmbiguousHeroMappingsAreRejectedBeforeActivation() =>
            Refuses<FactionDefinition>("factions", f => f with { Entries = [f.Entries[0] with { HeroIds = [9] }, .. f.Entries.Skip(1)] }, "heroIds");

        [Test]
        public void ACreatureCannotClaimAnUnrelatedDwellingFlag() =>
            Refuses<CreatureDefinition>("creatures", f => f with { Entries = [f.Entries[0] with { Dwelling = Dwellings.Dwelling6UpgradedTwice }, .. f.Entries.Skip(1)] }, "Peasant.dwelling");

        [Test]
        public void ADataDeclaredLibraryCannotOverflowTheFixedGuildSlots() =>
            Refuses<FactionDefinition>("factions", f => f with { Entries = [f.Entries[0] with { Library = true, GuildSpellCounts = [4, 3, 2, 2, 1] }, .. f.Entries.Skip(1)] }, "guildSpellCounts");

        [Test]
        public void EverySupportedMapObjectRoleMustHaveMetadata() =>
            Refuses<MapObjectDefinition>("map-objects", f => f with { Entries = f.Entries.Skip(1).ToArray() }, "role");

        [Test]
        public void DefinitionErrorsIdentifyTheGameVersionFileEntityAndProperty()
        {
            var reader = new DefinitionMutationReader();
            reader.Replace<DefinitionFile<CreatureDefinition>>("creatures", f => f with { Entries = [f.Entries[0] with { Abilities = ["Missing"] }, .. f.Entries.Skip(1)] });

            Assert.That(() => new GameCatalogs(reader), Throws.TypeOf<GameDefinitionException>()
                .With.Message.Contains("HoMM2")
                .And.Message.Contains("BaseGame")
                .And.Message.Contains("creatures.json")
                .And.Message.Contains("Peasant.abilities"));
        }

        [Test]
        public void TheNeutralRoleCannotBeMissing() =>
            Refuses<FactionDefinition>("factions", f => f with { Entries = f.Entries.Where(e => !e.Neutral).ToArray() }, "neutral");

        [Test]
        public void TheFallbackCreatureRoleCannotBeAmbiguous() =>
            Refuses<CreatureDefinition>("creatures", f => f with { Entries = [f.Entries[0], f.Entries[1] with { Fallback = true }, .. f.Entries.Skip(2)] }, "fallback");

        [Test]
        public void AnUnknownArtifactBehaviourIsRejectedBeforeActivation() =>
            Refuses<ArtifactDefinition>("artifacts", f => f with { Entries = [f.Entries[0] with { Role = (ArtifactRole)200 }, .. f.Entries.Skip(1)] }, "role");

        [Test]
        public void AStandardSetCannotReplaceAnEntityWithoutAParent() =>
            Refuses<AbilityDefinition>("abilities", f => f with { Replacements = [f.Entries[0]] }, "root set");

        [Test]
        public void StandardParentCyclesAreRejectedBeforeReadingAnyDomainFiles()
        {
            var reader = new DefinitionMutationReader();
            reader.Set(@"GameData\Homm2\manifest.json", new DefinitionManifest
            {
                SchemaVersion = 1,
                Versions = Enum.GetValues<GameVersion>().ToDictionary(v => v, _ => "cycle"),
                Sets = new() { ["cycle"] = new DefinitionSet { Parent = "cycle" } },
            });

            Assert.That(() => new GameCatalogs(reader), Throws.TypeOf<GameDefinitionException>().With.Message.Contains("cycle"));
        }

        [Test]
        public void ASpellOmittedFromTheCatalogIsPreservedUntilItsMageGuildSlotIsChanged()
        {
            var definitions = new DefinitionMutationReader();
            definitions.Replace<DefinitionFile<SpellDefinition>>("spells", file => file with
            {
                Entries = file.Entries.Where(spell => spell.Id != "Fireball").ToArray(),
            });
            var catalog = new GameCatalogs(definitions).For(GameVersion.BaseGame);
            var empty = new CastleChunk("Xabran", new byte[CastleLayout.RecordSize], 0);
            var baseline = new SafeCastleSerializer(TestCatalog.Standard).Serialize(empty, global::Siniskalc.Homm2.Tests.Data.DifferSamples.Castle()).ToArray();
            baseline[CastleLayout.GuildSpells] = 0;
            var chunk = new CastleChunk("Xabran", baseline, 0);
            var model = new CastleDeserializer(catalog).Deserialize(chunk);
            var writer = new SafeCastleSerializer(catalog);

            Assert.That(model.MageGuildSpell(1, 0), Is.Null);
            Assert.That(writer.Serialize(chunk, model).ToArray(), Is.EqualTo(baseline));
            Assert.That(
                writer.Serialize(chunk, model.WithMageGuildSpell(1, 0, catalog.Spells["Bless"])).ToArray()[CastleLayout.GuildSpells],
                Is.EqualTo(catalog.Spells["Bless"].Code - 1));
        }

        [Test]
        public void TheSharedPlaceVisitDifferNamesAndCountsEachChangedKindFromTheCatalog()
        {
            var definitions = new DefinitionMutationReader();
            definitions.Replace<DefinitionFile<PlaceDefinition>>("places", file => file with
            {
                Entries = file.Entries.Select(place => place.Kind == PlaceKind.Fort ? place with { Name = "Astral forts" } : place).ToArray(),
            });
            var catalog = new GameCatalogs(definitions).For(GameVersion.BaseGame);
            var original = global::Siniskalc.Homm2.Tests.Data.DifferSamples.Hero();
            var current = original with { Visited = original.Visited.With(PlaceKind.Fort, 2).With(PlaceKind.Gazebo, 4) };
            var changes = new HeroDiffer(new Homm2PendingChangesFactory(), catalog).Diff(0, original, current);

            Assert.That(changes.Select(change => change.Change.Description), Is.EquivalentTo(new[] { "Visited: astral forts", "Visited: gazebos" }));
            Assert.That(changes.Select(change => change.Change.After), Is.All.EqualTo("1"));
        }

        [Test]
        public void SharedMineRowsDispatchTheirNamesAndResourcesToTheDocumentCatalog()
        {
            var definitions = new DefinitionMutationReader();
            definitions.Replace<DefinitionFile<MineDefinition>>("mines", file => file with
            {
                Entries = file.Entries.Select(mine => mine.Id == "Sawmill" ? mine with { Name = "Astral timber yard", Resource = Siniskalc.Heroes.Entities.Resource.Crystal } : mine).ToArray(),
            });
            var catalog = new GameCatalogs(definitions).For(GameVersion.BaseGame);
            var model = new Siniskalc.Homm2.Models.MineModel(0, Siniskalc.Heroes.Entities.MineKind.Sawmill, new Siniskalc.Heroes.Models.MapPosition(1, 2), PlayerColor.Blue, catalog);
            var session = new Moq.Mock<IEditSession<Siniskalc.Homm2.Models.MineModel>>();
            session.SetupGet(edit => edit.Current).Returns(model);
            Siniskalc.Heroes.Wpf.ViewModels.MineRowViewModelBase row = new Siniskalc.Homm2.ViewModels.MineRowViewModel(session.Object, catalog, string.Empty);

            Assert.That(row.Kind, Is.EqualTo("Astral timber yard"));
            Assert.That(row.ResourceName, Is.EqualTo("Crystal"));
        }

        private static void Refuses<T>(string domain, Func<DefinitionFile<T>, DefinitionFile<T>> mutate, string diagnostic)
            where T : EntityDefinition
        {
            var reader = new DefinitionMutationReader();
            reader.Replace(domain, mutate);

            Assert.That(() => new GameCatalogs(reader), Throws.TypeOf<GameDefinitionException>().With.Message.Contains(domain).And.Message.Contains(diagnostic));
        }
    }
}
