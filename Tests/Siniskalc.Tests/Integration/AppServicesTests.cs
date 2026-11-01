namespace Siniskalc.Tests.Integration
{
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Homm3.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The application as the user has it: the composition root builds the shell with both games, and opening a savegame of a game
    /// shows the editor and the title of that game. The behaviour of each game is tested in the test project of the game.
    /// </summary>
    [TestFixture]
    [Category(TestKinds.Integration)]
    internal sealed class AppServicesTests
    {
        private string folder = string.Empty;
        private AppServices services = null!;
        private ShellViewModel shell = null!;

        [SetUp]
        public void CreateApplicationServices()
        {
            this.folder = Path.Combine(Path.GetTempPath(), "siniskalc-app-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(this.folder);
            this.services = new AppServices(settingsPath: Path.Combine(this.folder, "settings.json"), loggerFactory: Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
            this.shell = this.services.CreateShellViewModel();
        }

        [TearDown]
        public void DeleteFolder() => Directory.Delete(this.folder, recursive: true);

        [Test]
        public void TheShellStartsOnTheFirstGameAndKnowsBoth()
        {
            Assert.That(this.shell.Modules.Select(m => m.Name), Is.EqualTo(new[] { "Heroes of Might and Magic III", "Heroes of Might and Magic II" }));
            Assert.That(this.shell.Title, Is.EqualTo("Siniskalc - Save game editor"));
            Assert.That(this.shell.OpenFilter, Does.StartWith("Savegames |*.CGM;*.GM*;*.GM?;*.GX?|"));
        }

        [Test]
        public void OpeningAHeroes2SavegameSwitchesTheTitleAndTheEditorToThoseOfTheGame()
        {
            var save = Saves.Homm2();

            this.shell.LoadGame(save);

            Assert.That(this.shell.Title, Is.EqualTo($"Siniskalc - Heroes of Might and Magic II (The Succession Wars) - {Path.GetFullPath(save)}"));
            Assert.That(this.shell.CurrentModule, Is.SameAs(this.services.Homm2));
            Assert.That(this.shell.Editor, Is.InstanceOf<Homm2EditorViewModel>());
        }

        [Test]
        public void OpeningAHeroes3SavegameAfterAHeroes2OneSwitchesBack()
        {
            this.shell.LoadGame(Saves.Homm2());

            this.shell.LoadGame(Saves.Homm3());

            Assert.That(this.shell.Title, Does.StartWith("Siniskalc - Heroes of Might and Magic III"));
            Assert.That(this.shell.CurrentModule, Is.SameAs(this.services.Homm3));
            Assert.That(this.shell.Editor, Is.InstanceOf<Homm3EditorViewModel>());
        }

        [Test]
        public void OnlyHeroes2RefusesALongSavegameName()
        {
            const string name = "a very long name for a savegame.GM1";

            Assert.That(this.services.Homm3.ValidateSavegameName(name), Is.Null);
            Assert.That(this.services.Homm2.ValidateSavegameName(name), Does.Contain("would not show in the game"));
        }
    }
}
