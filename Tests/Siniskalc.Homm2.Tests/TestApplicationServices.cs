namespace Siniskalc.Homm2.Tests
{
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Runtime;
    using Siniskalc.Homm2.ViewModels;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// What the application hands a HoMM2 editor and its window, built from the module factory of the game and the shared components of
    /// Siniskalc.Core, with the settings in a file of the test. It knows no other game.
    /// </summary>
    internal sealed class TestApplicationServices
    {
        private const string ApplicationName = "Siniskalc";

        private readonly Homm2ModuleFactory factory;

        public TestApplicationServices(string settingsPath)
        {
            this.Settings = new SettingsStore(settingsPath);
            this.RecentFiles = new RecentFilesStore(this.Settings);
            this.factory = new Homm2ModuleFactory(
                new FileBackupServiceProvider(this.Settings, new NumberedSavefileBackupService(NullLogger<NumberedSavefileBackupService>.Instance), new DisabledFileBackupService()),
                new SavefileBytesPatcher(),
                new BanishingTextProvider(),
                NullLoggerFactory.Instance);
        }

        public SettingsStore Settings { get; }

        public RecentFilesStore RecentFiles { get; }

        public AboutInfo About { get; } = new(ApplicationName, "test", "A savegame editor.", "https://example.invalid", "https://example.invalid", "MIT", []);

        public HelpLinks Help { get; } = new("https://example.invalid", "https://example.invalid");

        public IGameModule<Homm2EditorViewModel> Module => this.factory.Module;

        public Homm2EditorViewModel CreateEditor() => this.factory.CreateEditor();

        /// <summary>
        /// Makes a shell with the HoMM2 module only.
        /// </summary>
        public ShellViewModel CreateShell() => new([this.Module], this.Settings, ApplicationName, NullLogger<ShellViewModel>.Instance);
    }
}
