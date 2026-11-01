namespace Siniskalc.Homm2.Tests.Integration.Data
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Moq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.ViewModels;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Homm2.Views;

    [TestFixture]
    [Category(TestKinds.Integration)]
    [Apartment(ApartmentState.STA)]
    internal sealed class JsonFactionTests
    {
        [Test]
        public void AJsonOnlyFactionIsDiscoveredAndEditedThroughTheRealModuleWithoutReadingDefinitionsAgain()
        {
            var reader = new DefinitionFixtureReader();
            var module = Module(reader);
            var (bytes, layout, castleIndex) = SyntheticSave();
            var startupReads = reader.Reads;
            reader.Locked = true;
            var game = module.GameService.LoadGameFromBytes(bytes);
            var catalog = game.Catalog;
            var castle = game.History.Current.Castles().GetModel(castleIndex);
            var hero = game.History.Current.Heroes().GetModel(0);

            Assert.That(castle.Faction.Name, Is.EqualTo("Observatory"));
            Assert.That(hero.Faction, Is.SameAs(castle.Faction));
            Assert.That(castle.Garrison.Slots[0]!.Creature.Name, Is.EqualTo("Starling"));
            Assert.That(castle.WeeklyGrowth(1), Is.EqualTo(7));
            Assert.That(castle.Garrison.Slots[0]!.Creature.Info.Special!.Single().Name, Is.EqualTo("Luminous (presentation only)"));
            Assert.That(game.GetBytesToSave(), Is.EqualTo(bytes), "Recognizing new JSON codes must not change any original byte.");

            var castlePage = new CastleViewModel(game, castleIndex, _ => { });
            var heroPage = new HeroViewModel(game, 0, _ => { }, module.MaxSpellPointsCalculator);
            Assert.That(castlePage.Dwellings[5].Built.Choices, Is.EqualTo(new[] { "Not built", "Stargazer", "Astral Seer" }));
            Assert.That(castlePage.Buildings.Select(b => b.Name), Does.Contain("Astral Garden").And.Contain("Star Library"));
            Assert.That(heroPage.ArmyEditor.Finder.Items.Cast<object>().OfType<CreatureChoiceViewModel>().Any(c => c.Name == "Astral Seer"), Is.True);

            castlePage.Buildings.Single(b => b.Name == "Star Library").IsChecked = true;
            castlePage.Dwellings[5].Built.Selected = "Astral Seer";
            castlePage.GarrisonEditor.SendToSlot(catalog.Creatures.Find(72)!, 1, 11);
            heroPage.ArtifactsEditor.Finder.Choose(heroPage.ArtifactsEditor.Finder.Items.Cast<ArtifactChoiceViewModel>().Single(a => a.Name == "Astrolabe"));
            var spell = heroPage.SpellsEditor.Finder.Items.Cast<SpellRowViewModel>().Single(s => s.Name == "Star Ward");
            Assert.That(spell.Magic, Is.EqualTo("Celestial"));
            spell.IsLearned = true;

            var expected = (byte[])bytes.Clone();
            var at = layout.CastleStart(castleIndex);
            ByteBufferHelper.WriteUInt16(expected, at + CastleLayout.TownBuildings, (ushort)(TownBuildings.Castle | TownBuildings.Special));
            ByteBufferHelper.WriteUInt32(expected, at + CastleLayout.Dwellings, (uint)(Dwellings.Dwelling1 | Dwellings.Dwelling6Upgraded) | (1u << 16));
            expected[at + CastleLayout.GuildSpellCounts] = 4;
            expected[at + CastleLayout.GarrisonTypes] = 72;
            ByteBufferHelper.WriteUInt16(expected, at + CastleLayout.GarrisonCounts, 11);
            var heroAt = layout.HeroStart(0);
            expected[heroAt + HeroLayout.Artifacts] = 103;
            expected[heroAt + HeroLayout.PrimarySkills] += 4;
            expected[heroAt + HeroLayout.PrimarySkills + 2] += 2;
            expected[heroAt + HeroLayout.Spells + 65] = 1;
            Assert.That(game.GetBytesToSave(), Is.EqualTo(expected), "Every serialized byte is checked independently of the readers.");
            Assert.That(game.History.Current.Castles().GetModel(castleIndex).HasLibrary, Is.True);
            Assert.That(game.History.GetPendingChanges().Any(p => p.Change.ToString().Contains("Astrolabe", StringComparison.Ordinal)), Is.True);
            Assert.That(game.History.GetPendingChanges().Any(p => p.Change.ToString().Contains("Star Library", StringComparison.Ordinal)), Is.True);

            while (game.History.CanUndo)
            {
                game.History.Undo();
            }

            Assert.That(game.GetBytesToSave(), Is.EqualTo(bytes));
            Assert.That(game.History.GetPendingChanges(), Is.Empty);
            while (game.History.CanRedo)
            {
                game.History.Redo();
            }

            Assert.That(game.GetBytesToSave(), Is.EqualTo(expected));
            Assert.That(module.GameService.LoadGameFromBytes(expected).History.Current.Castles().GetModel(castleIndex).Faction.Name, Is.EqualTo("Observatory"));
            Assert.That(reader.Reads, Is.EqualTo(startupReads));
        }

        [Test]
        public void TheJsonOnlyFactionAndItsBuildingNamesAreActuallyRenderedByTheCastlePage()
        {
            TestApplication.Ensure();
            var module = Module(new DefinitionFixtureReader());
            var (bytes, _, castleIndex) = SyntheticSave();
            var game = module.GameService.LoadGameFromBytes(bytes);
            var viewModel = new CastleViewModel(game, castleIndex, _ => { }) { SelectedTab = 0 };
            var window = new Window { Content = new CastlePage(viewModel), Width = 1200, Height = 900 };
            window.Show();
            try
            {
                Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);
                window.UpdateLayout();
                var texts = Descendants<TextBlock>(window).Where(t => t.IsVisible).Select(t => t.Text).ToArray();

                Assert.That(texts, Does.Contain("Star Library").And.Contain("Astral Garden"));
                Assert.That(Descendants<CheckBox>(window).Count(), Is.GreaterThanOrEqualTo(2), "The shared building template must have rendered real controls.");
            }
            finally
            {
                window.Close();
            }
        }

        [Test]
        public void UnknownSaveCodesAreStillPreservedWithTheSyntheticCatalogUntilAnEditDisagrees()
        {
            var module = Module(new DefinitionFixtureReader());
            var (bytes, layout, castleIndex) = SyntheticSave();
            var castleAt = layout.CastleStart(castleIndex);
            var heroAt = layout.HeroStart(0);
            bytes[castleAt + CastleLayout.Faction] = 200;
            bytes[castleAt + CastleLayout.GarrisonTypes] = 240;
            bytes[heroAt + HeroLayout.Artifacts] = 240;
            var game = module.GameService.LoadGameFromBytes(bytes);

            Assert.That(game.History.Current.Castles().GetModel(castleIndex).Faction.IsNeutral, Is.True);
            Assert.That(game.GetBytesToSave(), Is.EqualTo(bytes));
            game.History.CreateCastleSession(castleIndex).Update(c => c with { Faction = game.Catalog.Factions.Find(6)! });
            game.History.CreateHeroSession(0).Update(h => h.WithArtifact(0, game.Catalog.Artifacts.Find(103)));
            var written = game.GetBytesToSave();

            Assert.That(written[castleAt + CastleLayout.Faction], Is.EqualTo(6));
            Assert.That(written[castleAt + CastleLayout.GarrisonTypes], Is.EqualTo(240), "An unrelated faction edit must keep the unknown creature byte.");
            Assert.That(written[heroAt + HeroLayout.Artifacts], Is.EqualTo(103));
        }

        private static Homm2ModuleFactory Module(DefinitionFixtureReader reader) =>
            new(Mock.Of<IFileBackupServiceProvider>(), new SavefileBytesPatcher(), new BanishingTextProvider(), reader);

        private static (byte[] Bytes, SaveLayout Layout, int Castle) SyntheticSave()
        {
            var original = TestServices.LoadThreedom(1);
            var layout = original.Layout;
            var bytes = (byte[])original.Bytes.Clone();
            var index = original.History.Current.Castles().GetModels().First(c => c.IsUsed).Index;
            var at = layout.CastleStart(index);
            bytes[at + CastleLayout.Faction] = 6;
            Array.Fill(bytes, (byte)255, at + CastleLayout.GarrisonTypes, Constants.ArmySlotCount);
            Array.Clear(bytes, at + CastleLayout.GarrisonCounts, Constants.ArmySlotCount * 2);
            bytes[at + CastleLayout.GarrisonTypes] = 66;
            ByteBufferHelper.WriteUInt16(bytes, at + CastleLayout.GarrisonCounts, 3);
            ByteBufferHelper.WriteUInt16(bytes, at + CastleLayout.TownBuildings, (ushort)TownBuildings.Castle);
            ByteBufferHelper.WriteUInt32(bytes, at + CastleLayout.Dwellings, (uint)(Dwellings.Dwelling1 | Dwellings.Dwelling6) | (1u << 16));
            Array.Clear(bytes, at + CastleLayout.Creatures, Constants.Castle.DwellingCount * 4);
            Array.Clear(bytes, at + CastleLayout.GuildSpellCounts, Constants.Castle.MageGuildLevels);
            bytes[at + CastleLayout.GuildSpellCounts] = 3;
            var heroAt = layout.HeroStart(0);
            Array.Fill(bytes, (byte)255, heroAt + HeroLayout.Artifacts, Constants.Hero.ArtifactSlotCount);
            Array.Clear(bytes, heroAt + HeroLayout.Spells, HeroLayout.SpellsLength);
            return (bytes, layout, index);
        }

        private static IEnumerable<T> Descendants<T>(DependencyObject root)
            where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match)
                {
                    yield return match;
                }

                foreach (var nested in Descendants<T>(child))
                {
                    yield return nested;
                }
            }
        }
    }
}
