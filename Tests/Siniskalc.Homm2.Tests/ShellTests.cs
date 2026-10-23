namespace Siniskalc.Homm2.Tests
{
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Homm2.Views;
    using Siniskalc.Wpf.ViewModels;
    using Siniskalc.Wpf.Views;
    using PlayerColor = Siniskalc.Homm2.Entities.PlayerColor;

    /// <summary>
    /// The shell with the Heroes of Might and Magic II editor: opening a savegame shows its editor and its title. The shell with both games is tested in Siniskalc.Tests.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class ShellTests
    {
        private string folder = string.Empty;
        private TestApplicationServices services = null!;
        private ShellViewModel shell = null!;

        [SetUp]
        public void CreateApplicationServices()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-shell-games-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
            this.services = new TestApplicationServices(settingsPath: Path.Combine(this.folder, "settings.json"));
            this.shell = this.services.CreateShell();
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [Test]
        public void OpeningAHeroes2SavegameSwitchesTheTitleAndTheEditorToThoseOfTheGame()
        {
            var path = Saves.BrokenAllianceSave(10);

            this.shell.LoadGame(path);

            Assert.That(this.shell.Title, Is.EqualTo("Siniskalc - Heroes of Might and Magic II (Base game)"));
            Assert.That(this.shell.CurrentModule, Is.SameAs(this.services.Module));
            Assert.That(this.shell.Editor, Is.InstanceOf<Homm2EditorViewModel>());
            Assert.That(this.shell.IsGameLoaded, Is.True);
            Assert.That(this.shell.File, Is.EqualTo(path));
            Assert.That(this.shell.SaveFilter, Is.EqualTo("Heroes of Might and Magic II savegames |*.GM?;*.GX?"));
        }

        [Test]
        public void ThePriceOfLoyaltySavegameOpensTheSameWay()
        {
            this.shell.LoadGame(Saves.Path(Saves.ArraxTheJerk, "arrax_the_jerk_01_start"));

            var editor = (Homm2EditorViewModel)this.shell.Editor;
            Assert.That(editor.Overview!.MapName, Is.EqualTo("Arrax the Jerk"));
            Assert.That(editor.Overview.VersionName, Is.EqualTo("Price of Loyalty"));
            Assert.That(editor.Overview.Players.Select(p => p.Name), Is.EqualTo(new[] { "Blue", "Green" }));
        }

        [Test]
        public void ASavegameThatCannotBeReadLeavesTheGameThatWasOpen()
        {
            var broken = Path.Combine(this.folder, "BROKEN.GM1");
            File.WriteAllBytes(broken, Saves.Read(Saves.BrokenAllianceSave(1))[..0x3000]);
            this.shell.LoadGame(Saves.BrokenAllianceSave(1));

            Assert.That(() => this.shell.LoadGame(broken), Throws.InstanceOf<UnsupportedSavefileException>());

            Assert.That(this.shell.File, Is.EqualTo(Saves.BrokenAllianceSave(1)));
            Assert.That(this.shell.Title, Is.EqualTo("Siniskalc - Heroes of Might and Magic II (Base game)"));
        }

        [Test]
        public void EditsAreSavedWithAnUnchangedFileSizeAndAreBackedUp()
        {
            var path = Path.Combine(this.folder, "E01GOLD.GM1");
            File.Copy(Saves.BrokenAllianceSave(5), path);
            this.shell.LoadGame(path);
            var editor = (Homm2EditorViewModel)this.shell.Editor;

            editor.CurrentGame!.History.CreatePlayerSession(0).Update(p => p with { Resources = p.Resources.With(Resource.Gold, 99_999) });
            Assert.That(this.shell.HasPendingChanges, Is.True);
            this.shell.SaveGame();

            Assert.That(new FileInfo(path).Length, Is.EqualTo(new FileInfo(Saves.BrokenAllianceSave(5)).Length));
            Assert.That(this.shell.HasPendingChanges, Is.False);
            Assert.That(File.Exists(path + ".bak"), Is.True);
            Assert.That(File.ReadAllBytes(path + ".bak"), Is.EqualTo(File.ReadAllBytes(Saves.BrokenAllianceSave(5))));

            this.shell.ReloadGame();
            Assert.That(editor.CurrentGame!.History.Current.Players().GetModel(0).Resources.Gold, Is.EqualTo(99_999));
        }

        [Test]
        public void TheHexDumpOfAGameHasARegionForEveryHeroPlayerAndUsedCastle()
        {
            this.shell.LoadGame(Saves.BrokenAllianceSave(1));

            var dump = this.shell.CreateSaveHexDump();

            Assert.That(dump.Title, Does.Contain("broken_alliance_01_start"));
            Assert.That(dump.Regions.Count, Is.EqualTo(3 + 6 + 54 + 11));
            Assert.That(dump.Regions.Select(r => r.Name), Does.Contain("Maximus").And.Contain("Blue player").And.Contain("Xabran").And.Contain("Map name"));
            Assert.That(dump.Regions.OrderBy(r => r.Start).Zip(dump.Regions.OrderBy(r => r.Start).Skip(1)).All(pair => pair.First.End <= pair.Second.Start), Is.True);
        }

        [Test]
        public void TheWindowShowsTheEditorOfTheGameThatWasOpenWithoutBindingErrors()
        {
            var errors = new BindingErrorListener();
            PresentationTraceSources.Refresh();
            PresentationTraceSources.DataBindingSource.Listeners.Add(errors);
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;

            try
            {
                TestApplication.Ensure();

                var window = new ShellWindow(
                    new RecentFilesMenuViewModel(this.services.RecentFiles),
                    new ShellUrlLauncher(_ => { }),
                    this.services.About,
                    this.services.Help,
                    NoUserInteraction.Messages(),
                    new SavegameFileCommands(this.services.RecentFiles, NoUserInteraction.FileDialogs(), NoUserInteraction.Messages())) { DataContext = this.shell };
                window.Show();

                try
                {
                    Pump();

                    this.shell.LoadGame(Saves.BrokenAllianceSave(24));
                    Pump();
                    window.UpdateLayout();

                    Assert.That(window.Title, Is.EqualTo("Siniskalc - Heroes of Might and Magic II (Base game)"));
                    Assert.That(Find<Homm2EditorView>(window), Is.Not.Null);
                    Assert.That(Find<Homm2EditorView>(window)!.DataContext, Is.SameAs(this.shell.Editor));

                    var editor = (Homm2EditorViewModel)this.shell.Editor;
                    editor.CurrentGame!.History.CreatePlayerSession(0).Update(p => p with { Resources = p.Resources.With(Resource.Gold, 1) });
                    this.shell.IsPendingChangesOpen = true;
                    Pump();
                    Assert.That(this.shell.PendingChanges, Has.Count.EqualTo(1));
                    Assert.That(editor.Overview!.Players.First(p => p.Name == "Blue").Gold, Is.EqualTo("1"));

                    // Every page, on every tab (the hex tab too), binds without errors.
                    this.shell.IsAdvancedMode = true;
                    var hero = editor.CurrentGame.History.Current.Heroes().GetModels().First(h => h.Owner == PlayerColor.Blue).Id;
                    var castle = editor.CurrentGame.History.Current.Castles().GetModels().First(c => c.IsUsed && c.Owner == PlayerColor.Blue).Index;
                    var pages = new (Action Show, Func<TabbedPageViewModelBase> Page, int Tabs)[]
                    {
                        (() => editor.ShowPlayer(PlayerColor.Blue), () => (PlayerViewModel)editor.CurrentPage!, 5),
                        (() => editor.ShowHero(hero), () => (HeroViewModel)editor.CurrentPage!, 6),
                        (() => editor.ShowCastle(castle), () => (CastleViewModel)editor.CurrentPage!, 5),
                        (() => editor.ShowMap(), () => (MapViewModel)editor.CurrentPage!, 2),
                    };
                    foreach (var (show, page, tabs) in pages)
                    {
                        show();
                        for (var tab = 0; tab < tabs; tab++)
                        {
                            page().SelectedTab = tab;
                            Pump();
                            window.UpdateLayout();
                        }
                    }

                    editor.ShowOverview();
                    Pump();
                    window.UpdateLayout();
                }
                finally
                {
                    window.Close();
                }
            }
            finally
            {
                PresentationTraceSources.DataBindingSource.Listeners.Remove(errors);
            }

            Assert.That(errors.Messages, Is.Empty, string.Join(Environment.NewLine, errors.Messages));
        }

        private static void Pump() => Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.ContextIdle);

        private static T? Find<T>(DependencyObject root)
            where T : DependencyObject
        {
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match)
                {
                    return match;
                }

                if (Find<T>(child) is { } nested)
                {
                    return nested;
                }
            }

            return null;
        }

        private sealed class BindingErrorListener : TraceListener
        {
            public List<string> Messages { get; } = [];

            public override void Write(string? message)
            {
            }

            public override void WriteLine(string? message) => this.Messages.Add(message ?? string.Empty);
        }
    }
}
