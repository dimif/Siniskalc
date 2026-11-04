namespace Siniskalc.Homm3.Tests.Integration.Data;

using System.Text;
using System.Threading;
using System.Windows;
using Moq;
using Siniskalc.Core.Data;
using Siniskalc.Core.Runtime;
using Siniskalc.Heroes.Models;
using Siniskalc.Heroes.ViewModels;
using Siniskalc.Heroes.Wpf.ViewModels;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Runtime;
using Siniskalc.Homm3.ViewModels;
using Siniskalc.Homm3.Views;

[TestFixture]
[Category(TestKinds.Integration)]
[Apartment(ApartmentState.STA)]
[NonParallelizable]
internal sealed class JsonFactionTests
{
    [Test]
    public void AJsonOnlyFactionTraversesDiscoveryModelsRenderingEditingSerializationAndHistory()
    {
        var reader = new FixtureReader();
        var services = new Homm3ModuleFactory(Encoding.Latin1, Mock.Of<IFileBackupServiceProvider>(), new SavefileBytesPatcher(), new BanishingTextProvider(), service => service, reader);
        reader.Closed = true;
        var catalog = services.VersionEntities.For(GameVersion.HornOfTheAbyss).Catalog;
        var faction = catalog.Factions.ById("FixtureTown");
        var scout = catalog.Creatures.ById("FixtureScout");
        var bytes = services.SavefileCodec.Read(Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "Hota", "castles_00_baseline.GM1"));
        var original = services.GameService.LoadGameFromBytes(bytes, "fixture.GM1");
        var castle = original.History.Current.Castles()[0];
        var nameEnd = castle.StartIndex + castle.NameEnd;
        bytes[castle.StartIndex + CastleLayout.Faction] = faction.Code;
        bytes[castle.StartIndex + CastleLayout.MageGuildLevel] = 0;
        BitConverter.GetBytes((uint)scout.Code).CopyTo(bytes, castle.StartIndex + CastleLayout.GarrisonTypes);
        BitConverter.GetBytes(3).CopyTo(bytes, castle.StartIndex + CastleLayout.GarrisonCounts);
        var village = Building.FromType(faction, BuildingType.VillageHall)!;
        var initialMask = (1UL << village.Code) | (1UL << 63);
        BitConverter.GetBytes(initialMask).CopyTo(bytes, nameEnd + CastleLayout.BuiltBuildingsFromNameEnd);
        BitConverter.GetBytes(initialMask).CopyTo(bytes, nameEnd + CastleLayout.VisibleBuildingsFromNameEnd);
        BitConverter.GetBytes(ulong.MaxValue).CopyTo(bytes, nameEnd + CastleLayout.EnabledBuildingsFromNameEnd);
        Array.Clear(bytes, nameEnd + CastleLayout.CreaturesAvailableFromNameEnd, 28);
        var heroChunk = original.History.Current.Heroes()[0];
        var offsets = new HeroOffsets(GameVersion.HornOfTheAbyss);
        bytes[heroChunk.StartIndex + offsets[HeroOffsetName.Class]] = catalog.HeroClasses.ById("FixtureChampion").Code;
        BitConverter.GetBytes((uint)catalog.Artifacts.ById("FixtureCharm").Code).CopyTo(bytes, heroChunk.StartIndex + offsets[HeroOffsetName.Helmet]);
        var game = services.GameService.LoadGameFromBytes(bytes, "fixture.GM1");
        var index = game.History.Current.Castles().FindByName(castle.Name);
        var session = game.History.CreateCastleSession(index);
        var hero = game.History.Current.Heroes().GetModel(0);
        Assert.Multiple(() =>
        {
            Assert.That(session.Current.Faction, Is.SameAs(faction));
            Assert.That(session.Current.Garrison.Slots[0]!.Creature, Is.SameAs(scout));
            Assert.That(hero.HeroClass.Faction, Is.SameAs(faction));
            Assert.That(hero.EquippedArtifacts.GetArtifact(ArtifactSlots.Helmet)!.EffectInfo.Atk, Is.EqualTo(2));
            Assert.That(hero.SpellBook.IsAvailable(catalog.Spells.ById("Haste")), Is.True);
            Assert.That(catalog.Spells.ById("Haste").School.Id, Is.EqualTo("fixtureSky"));
            Assert.That(services.VersionEntities.For(GameVersion.ShadowOfDeath).Catalog.Factions.Find(12), Is.Null);
        });

        using var page = new CastleViewModel(session, services.ByteEditTargets, services.CastleBuildMessages, services.EffectLines);
        page.GarrisonEditor.Finder.Filter = faction.Name;
        Assert.That(page.GarrisonEditor.Finder.Items.Cast<CreatureChoiceViewModel>().Select(c => c.Name),
            Is.EqualTo(new[] { scout.Name, catalog.Creatures.ById("FixtureGuard").Name }));
        Assert.That(page.BuildingGroups.SelectMany(g => g.Rows).Select(r => r.Name), Does.Contain("Astral Nest").And.Contain("Astral HighNest"));
        TestApplication.Ensure();
        var view = new CastlePage(page);
        view.Measure(new Size(1200, 900));
        view.Arrange(new Rect(0, 0, 1200, 900));
        view.UpdateLayout();
        Assert.That(view.ActualWidth, Is.EqualTo(1200));
        var before = game.History.Current.ToBytes();
        session.Update(c =>
        {
            c = Siniskalc.Homm3.Models.CastleBuilder.Build(c, Building.FromType(faction, BuildingType.Fort)!);
            foreach (var id in new[] { "FixtureNest", "FixtureHorde", "FixtureHighNest" })
                c = Siniskalc.Homm3.Models.CastleBuilder.Build(c, catalog.Buildings.ById(id));
            return c.WithCreaturesWaiting(0, 17);
        });
        Assert.That(session.Current.Buildings, Does.Contain(catalog.Buildings.ById("FixtureHighHorde")));
        Assert.That(session.Current.BaseWeeklyGrowth(0), Is.EqualTo(9));
        Assert.That(game.History.GetPendingChanges().Select(c => c.Change.Description), Does.Contain("Astral HighNest"));
        var expected = before.ToArray();
        var builtMask = initialMask | (1UL << Building.FromType(faction, BuildingType.Fort)!.Code) | (1UL << 27);
        foreach (var id in new[] { "FixtureNest", "FixtureHighNest", "FixtureHorde", "FixtureHighHorde" })
            builtMask |= 1UL << catalog.Buildings.ById(id).Code;
        var visibleMask = builtMask & ~(1UL << catalog.Buildings.ById("FixtureNest").Code)
            & ~(1UL << catalog.Buildings.ById("FixtureHighNest").Code) & ~(1UL << catalog.Buildings.ById("FixtureHorde").Code);
        BitConverter.GetBytes(builtMask).CopyTo(expected, nameEnd + CastleLayout.BuiltBuildingsFromNameEnd);
        BitConverter.GetBytes(visibleMask).CopyTo(expected, nameEnd + CastleLayout.VisibleBuildingsFromNameEnd);
        BitConverter.GetBytes((ushort)17).CopyTo(expected, nameEnd + CastleLayout.UpgradedCreaturesAvailableFromNameEnd);
        var actual = game.History.Current.ToBytes();
        Assert.That(actual, Is.EqualTo(expected), "only the declared masks, companion bit and upgraded stock change; differences from name end: "
            + string.Join(", ", actual.Select((value, at) => (value, at)).Where(p => p.value != expected[p.at]).Select(p => $"{p.at - nameEnd}: {expected[p.at]:X2}/{p.value:X2}")));
        game.History.Undo();
        Assert.That(game.History.Current.ToBytes(), Is.EqualTo(before));
        Assert.That(game.History.GetPendingChanges(), Is.Empty);
        game.History.Redo();
        Assert.That(game.History.Current.ToBytes(), Is.EqualTo(expected));
        var reloaded = services.GameService.LoadGameFromBytes(expected, "reloaded.GM1");
        Assert.That(reloaded.History.Current.Castles().GetModel(index).CreaturesWaiting(0), Is.EqualTo(17));
        using var heroPage = new HeroViewModel(game.History.CreateHeroSession(0), services.EffectLines,
            services.SpellPoints.Create(GameVersion.HornOfTheAbyss), services.ByteEditTargets);
        var launcher = catalog.WarMachines.ById("FixtureLauncher");
        heroPage.GeneralInfoViewModel.WarMachineChoices.Single(c => c.Name == launcher.Name).IsChecked = true;
        var editedHero = game.History.Current.Heroes().GetModel(0);
        Assert.That(editedHero.WarMachines, Does.Contain(launcher));
        Assert.That(editedHero.WarMachines.Count(m => m.Slot == launcher.Slot), Is.EqualTo(1));
        Assert.That(game.History.Current.Heroes()[0].GetUInt32At(offsets[WarMachineOffsets.For(launcher)]), Is.EqualTo(launcher.Code));
        Assert.That(reader.Reads.Distinct().Count(), Is.EqualTo(reader.Reads.Count), "each definition file is read once during construction");
    }

    [Test]
    public void JsonOnlyCreaturesAreNamedByTheDwellingLoaderAndReadAndWrittenAsTheirOwnEntities()
    {
        var catalog = new GameCatalogLoader(new FixtureReader()).Load()[GameVersion.HornOfTheAbyss];
        var bytes = new byte[12 + DwellingLayout.RecordSize];
        bytes[10] = 1;
        const int start = 12;
        bytes[start + DwellingLayout.Creature] = 201;
        bytes[start + DwellingLayout.ObjectType] = DwellingLayout.CreatureGeneratorType;
        bytes[start + DwellingLayout.Owner] = DwellingLayout.NoOwner;
        for (var slot = 0; slot < MineLayout.SlotCount; slot++)
        {
            BitConverter.GetBytes(uint.MaxValue).CopyTo(bytes, start + DwellingLayout.Guard.TypeAt(slot));
        }

        var chunks = new DwellingBytesLoader(catalog, new MapObjectTablesLocator()).LoadDwellingChunks(bytes, []);
        Assert.That(chunks, Has.Count.EqualTo(1));
        Assert.That(chunks[0].Name, Does.Contain("Astral Scout"));
        var model = new DwellingDeserializer(catalog).Deserialize(chunks[0]);
        Assert.That(model.Creature, Is.SameAs(catalog.Creatures.ById("FixtureScout")));
        Assert.That(new SafeDwellingSerializer().Serialize(chunks[0], model).HasSameBytes(chunks[0]), Is.True);
    }

    [Test]
    public void MutatingDefinitionArraysAfterConstructionCannotChangeAnyPublishedSnapshot()
    {
        var reader = new FixtureReader();
        var catalogs = new GameCatalogLoader(reader).Load();
        var creature = catalogs[GameVersion.HornOfTheAbyss].Creatures.ById("Marksman");
        var before = creature.Info.Special!.Cast<CatalogEntity>().Select(a => a.Id).ToArray();
        reader.SourceCreatures!.Single(c => c.Id == "Marksman").Info.Special[0] = "not-a-real-ability";
        reader.SourceCreatures![0] = reader.SourceCreatures[0] with { Name = "Changed" };
        Assert.That(creature.Info.Special!.Cast<CatalogEntity>().Select(a => a.Id), Is.EqualTo(before));
        Assert.That(catalogs.Values.All(c => c.Creatures.ById("Peasant").Name == "Peasant"), Is.True);
    }

    private sealed class FixtureReader : IGameDefinitionReader
    {
        private readonly JsonGameDefinitionReader standard = new();
        private readonly JsonGameDefinitionReader fixtures = new(Path.Combine(TestContext.CurrentContext.TestDirectory, "CatalogData"));
        public bool Closed { get; set; }
        public List<string> Reads { get; } = [];
        public CreatureDefinition[]? SourceCreatures { get; private set; }
        public T Read<T>(string relativePath) where T : class
        {
            Assert.That(this.Closed, Is.False, "definitions must never be read after module construction");
            this.Reads.Add(relativePath);
            var result = relativePath == @"GameData\Homm3\manifest.json" ? this.fixtures.Read<T>(@"Synthetic\manifest.json")
                : relativePath.StartsWith(@"GameData\Homm3\synthetic\", StringComparison.Ordinal)
                    ? this.fixtures.Read<T>(@"Synthetic\" + Path.GetFileName(relativePath))
                    : this.standard.Read<T>(relativePath);
            if (relativePath == @"GameData\Homm3\standard\creatures.json") this.SourceCreatures = (CreatureDefinition[])(object)result;
            return result;
        }
    }
}
