namespace Siniskalc.Homm2.Tests
{
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Threading;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Homm2.Views;
    using Siniskalc.Wpf.ViewModels;
    using Siniskalc.Wpf.Views;
    using Color = Siniskalc.Homm2.Entities.Color;

    /// <summary>
    /// The application with both games: opening a savegame of Heroes of Might and Magic II switches the shell to its editor and its title.
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    internal sealed class ShellGamesTests
    {
        private string folder = string.Empty;
        private AppServices services = null!;
        private ShellViewModel shell = null!;

        [SetUp]
        public void CreateApplicationServices()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-shell-games-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
            this.services = new AppServices(settingsPath: Path.Combine(this.folder, "settings.json"));
            this.shell = this.services.CreateShellViewModel();
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [Test]
        public void TheShellStartsOnTheFirstGameAndKnowsBoth()
        {
            Assert.That(this.shell.Modules.Select(m => m.Name), Is.EqualTo(new[] { "Heroes of Might and Magic III", "Heroes of Might and Magic II" }));
            Assert.That(this.shell.Title, Is.EqualTo("Siniskalc - Heroes of Might and Magic III"));
            Assert.That(this.shell.OpenFilter, Does.StartWith("Savegames |*.CGM;*.GM*;*.GM?;*.GX?|"));
        }

        [Test]
        public void OpeningAHeroes2SavegameSwitchesTheTitleAndTheEditorToThoseOfTheGame()
        {
            var path = Saves.BrokenAllianceSave(10);

            this.shell.LoadGame(path);

            Assert.That(this.shell.Title, Is.EqualTo("Siniskalc - Heroes of Might and Magic II"));
            Assert.That(this.shell.CurrentModule, Is.SameAs(this.services.Homm2));
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
            Assert.That(editor.Overview.Version, Is.EqualTo("Price of Loyalty"));
            Assert.That(editor.Overview.Players.Select(p => p.Color), Is.EqualTo(new[] { Color.Blue, Color.Green }));
        }

        [Test]
        public void ASavegameThatCannotBeReadLeavesTheGameThatWasOpen()
        {
            var broken = Path.Combine(this.folder, "BROKEN.GM1");
            File.WriteAllBytes(broken, Saves.Read(Saves.BrokenAllianceSave(1))[..0x3000]);
            this.shell.LoadGame(Saves.BrokenAllianceSave(1));

            Assert.That(() => this.shell.LoadGame(broken), Throws.InstanceOf<UnsupportedSavefileException>());

            Assert.That(this.shell.File, Is.EqualTo(Saves.BrokenAllianceSave(1)));
            Assert.That(this.shell.Title, Is.EqualTo("Siniskalc - Heroes of Might and Magic II"));
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
                if (Application.Current == null)
                {
                    var app = new App { ShutdownMode = ShutdownMode.OnExplicitShutdown };
                    app.InitializeComponent();
                }

                var window = new ShellWindow(
                    new RecentFilesMenuViewModel(this.services.RecentFiles),
                    new ShellUrlLauncher(_ => { }),
                    this.services.About,
                    this.services.Help,
                    new SilentMessageService(),
                    new SavegameFileCommands(this.services.RecentFiles, new NoFileDialogs(), new SilentMessageService())) { DataContext = this.shell };
                window.Show();

                try
                {
                    Pump();
                    Assert.That(window.Title, Is.EqualTo("Siniskalc - Heroes of Might and Magic III"));

                    this.shell.LoadGame(Saves.BrokenAllianceSave(24));
                    Pump();
                    window.UpdateLayout();

                    Assert.That(window.Title, Is.EqualTo("Siniskalc - Heroes of Might and Magic II"));
                    Assert.That(Find<Homm2EditorView>(window), Is.Not.Null);
                    Assert.That(Find<Homm2EditorView>(window)!.DataContext, Is.SameAs(this.shell.Editor));

                    var editor = (Homm2EditorViewModel)this.shell.Editor;
                    editor.CurrentGame!.History.CreatePlayerSession(0).Update(p => p with { Resources = p.Resources.With(Resource.Gold, 1) });
                    this.shell.IsPendingChangesOpen = true;
                    Pump();
                    Assert.That(this.shell.PendingChanges, Has.Count.EqualTo(1));
                    Assert.That(editor.Overview!.Players.First(p => p.Color == Color.Blue).Gold, Is.EqualTo(1));

                    // Every page, on every tab (the hex tab too), binds without errors.
                    this.shell.IsAdvancedMode = true;
                    var hero = editor.CurrentGame.History.Current.Heroes().GetModels().First(h => h.Owner == Color.Blue).HeroId;
                    var castle = editor.CurrentGame.History.Current.Castles().GetModels().First(c => c.IsUsed && c.Owner == Color.Blue).Index;
                    var pages = new (Action Show, Func<PageViewModel> Page, int Tabs)[]
                    {
                        (() => editor.ShowPlayer(Color.Blue), () => (PlayerViewModel)editor.CurrentPage!, 4),
                        (() => editor.ShowHero(hero), () => (HeroViewModel)editor.CurrentPage!, 6),
                        (() => editor.ShowCastle(castle), () => (CastleViewModel)editor.CurrentPage!, 5),
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

        private sealed class SilentMessageService : IMessageService
        {
            public void ShowError(string title, string message)
            {
            }

            public void ShowUnexpectedError(string title, string message, string details)
            {
            }

            public void ShowWarning(string title, string message)
            {
            }

            public void ShowInformation(string title, string message)
            {
            }

            public MessageAnswer AskYesNoCancel(string title, string message) => MessageAnswer.No;
        }

        private sealed class NoFileDialogs : IFileDialogs
        {
            public string? PickFileToOpen(string filter) => null;

            public string? PickFileToSave(string filter, string? fileName = null, string? defaultExtension = null) => null;
        }
    }
}
