using Siniskalc;
using Siniskalc.Core.Data;
using Siniskalc.Core.Runtime;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Runtime;
using Siniskalc.Homm3.ViewModels;
using Siniskalc.Homm3.Views;
using Siniskalc.Wpf.ViewModels;
using Siniskalc.Wpf.Views;

namespace Siniskalc.Homm3.Tests
{
    /// <summary>
    /// The real components, built once by the composition root of the application, for tests that load savefiles. Tests of a
    /// single unit give it fakes or the pieces it needs instead.
    /// </summary>
    internal static class TestServices
    {
        private static readonly AppServices Services = new(settingsPath: Path.Combine(Path.GetTempPath(), "siniskalc-tests", "settings.json"));

        public static IGameService GameService => Services.GameService;

        public static ISavefileCodec SavefileCodec => Services.SavefileCodec;

        /// <summary>
        /// Returns a store of recent files that keeps its list in a file of its own, so a test never changes the settings of the user.
        /// </summary>
        public static RecentFilesStore CreateRecentFiles() => new(new SettingsStore(Path.Combine(Path.GetTempPath(), "siniskalc-tests", Guid.NewGuid().ToString("N"), "settings.json")));

        public static ISaveDocumentFactory DocumentFactory => Services.DocumentFactory;

        public static ICastleDeserializer CastleDeserializer => Services.CastleDeserializer;

        public static ICastleSerializer CastleSerializer => Services.CastleSerializer;

        public static IPlayerDeserializer PlayerDeserializer => Services.PlayerDeserializer;

        public static ISavefileBytesPatcher BytesPatcher => Services.BytesPatcher;

        public static ISaveRegionProvider SaveRegions => Services.SaveRegionProvider;

        public static IVersionSpecificEntitiesProviderFactory VersionEntities => Services.VersionEntities;

        public static IHeroRegionProvider HeroRegions => Services.HeroRegions;

        public static IPlayerRegionProvider PlayerRegions => Services.PlayerRegions;

        public static ICastleRegionProvider CastleRegions => Services.CastleRegions;

        public static IByteEditTargetFactory ByteEditTargets => Services.ByteEditTargets;

        public static EffectLineProvider EffectLines => Services.EffectLines;

        public static BanishingTextProvider BanishingText => Services.BanishingText;

        public static CastleBuildMessageProvider CastleBuildMessages => Services.CastleBuildMessages;

        public static ISpellPointsCalculator SpellPoints => Services.SpellPoints;

        /// <summary>
        /// Returns the main window showing <paramref name="viewModel"/>, with a launcher that opens nothing and messages that show nothing.
        /// </summary>
        public static ShellWindow CreateShellWindow(ShellViewModel shell, IMessageService? messages = null)
        {
            var recentFiles = CreateRecentFiles();
            var shownMessages = messages ?? new RecordingMessageService();
            var files = new SavegameFileCommands(recentFiles, new ScriptedFileDialogs(), shownMessages);
            return new ShellWindow(new RecentFilesMenuViewModel(recentFiles), new ShellUrlLauncher(_ => { }), Services.About, Services.Help, shownMessages, files) { DataContext = shell };
        }

        /// <summary>
        /// Returns the shell of the application as the user has it, with the editor of every game it can edit.
        /// </summary>
        public static ShellViewModel CreateShell() => Services.CreateShellViewModel();

        /// <summary>
        /// Returns a shell whose HoMM3 editor is <paramref name="editor"/>, so a test can load a game into the editor and look at the window.
        /// </summary>
        public static ShellViewModel CreateShell(Homm3EditorViewModel editor) => new([new Homm3Module(() => editor)], Services.Settings, AppInfo.Name);

        /// <summary>
        /// Writes <paramref name="castle"/> to <paramref name="chunk"/>.
        /// </summary>
        public static CastleChunk SerializeCastle(CastleChunk chunk, CastleModel castle) => CastleSerializer.Serialize(chunk, castle);

        public static Homm3EditorViewModel CreateHomm3Editor() => Services.CreateHomm3Editor();

        public static HeroViewModel CreateHeroViewModel(HeroEditSession session) => new(session, Services.EffectLines, Services.SpellPoints, Services.ByteEditTargets);
    }
}
