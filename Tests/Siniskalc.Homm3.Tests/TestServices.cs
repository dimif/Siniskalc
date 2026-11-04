using Siniskalc.Heroes.Wpf.ViewModels;
using Moq;
using System.Text;
using Siniskalc.Core.Data;
using Siniskalc.Core.Runtime;
using Siniskalc.Heroes.Models;
using Siniskalc.Homm3.Data;
using Siniskalc.Homm3.Entities;
using Siniskalc.Homm3.Models;
using Siniskalc.Homm3.Runtime;
using Siniskalc.Homm3.Tests.Data;
using Siniskalc.Homm3.ViewModels;
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
        private const string ApplicationName = "Siniskalc";

        private static readonly SettingsStore Settings = new(Path.Combine(Path.GetTempPath(), "siniskalc-tests", "settings.json"));

        private static readonly ISavefileBytesPatcher Patcher = new SavefileBytesPatcher();

        private static readonly BanishingTextProvider Banishing = new();

        private static ISavegameService<Game> plainGameService = null!;

        private static readonly Homm3ModuleFactory Services = new(
            Encoding.Latin1,
            new FileBackupServiceProvider(Settings, new NumberedSavefileBackupService(), new DisabledFileBackupService()),
            Patcher,
            Banishing,
            service =>
            {
                plainGameService = service;
                return new CachingGameService(service, SavedGames.Cache);
            },
            new JsonGameDefinitionReader());

        private static readonly AboutInfo About = new(ApplicationName, "test", "A savegame editor.", "https://example.invalid", "https://example.invalid", "MIT", []);

        private static readonly HelpLinks Help = new("https://example.invalid", "https://example.invalid");

        /// <summary>
        /// Gets the service that parses the file or the bytes it is given each time. The editors of <see cref="CreateHomm3Editor"/> load through
        /// <see cref="CachingGameService"/> instead, which serves the files of <c>TestData</c> from <see cref="SavedGames"/>.
        /// </summary>
        public static ISavegameService<Game> GameService => plainGameService;

        public static ISavefileCodec SavefileCodec => Services.SavefileCodec;

        /// <summary>
        /// Returns a store of recent files that keeps its list in a file of its own, so a test never changes the settings of the user.
        /// </summary>
        public static RecentFilesStore CreateRecentFiles() => new(new SettingsStore(Path.Combine(Path.GetTempPath(), "siniskalc-tests", Guid.NewGuid().ToString("N"), "settings.json")));

        public static ISaveDocumentFactory DocumentFactory => Services.DocumentFactory;

        public static ITownMerchantStockLocator TownMerchantStockLocator => Services.TownMerchantStockLocator;

        public static ICastleDeserializer CastleDeserializer => Services.CastleDeserializer;

        public static ISerializer<CastleChunk, CastleModel> CastleSerializer => Services.CastleSerializer;

        public static IPlayerDeserializer PlayerDeserializer => Services.PlayerDeserializer;

        public static ISavefileBytesPatcher BytesPatcher => Patcher;

        public static ISaveRegionProvider SaveRegions => Services.SaveRegionProvider;

        public static IVersionSpecificEntitiesProviderFactory VersionEntities => Services.VersionEntities;

        public static IHeroRegionProvider HeroRegions => Services.HeroRegions;

        public static IPlayerRegionProvider PlayerRegions => Services.PlayerRegions;

        public static ICastleRegionProvider CastleRegions => Services.CastleRegions;

        public static IByteEditTargetFactory ByteEditTargets => Services.ByteEditTargets;

        public static EffectLineProvider EffectLines => Services.EffectLines;

        public static BanishingTextProvider BanishingText => Banishing;

        public static CastleBuildMessageProvider CastleBuildMessages => Services.CastleBuildMessages;

        public static ISpellPointsCalculatorFactory SpellPointsFactory => Services.SpellPoints;

        /// <summary>
        /// Returns the calculator of the spell points of a savefile of <paramref name="version"/>.
        /// </summary>
        public static IMaxSpellPointsCalculator<SecondarySkill, SecondarySkills> SpellPoints(GameVersion version) => Services.SpellPoints.Create(version);

        /// <summary>
        /// Returns the main window showing <paramref name="viewModel"/>, with a launcher that opens nothing and messages that show nothing.
        /// </summary>
        public static ShellWindow CreateShellWindow(ShellViewModel shell, IMessageService? messages = null, IUrlLauncher? launcher = null)
        {
            var recentFiles = CreateRecentFiles();
            var shownMessages = messages ?? CancellingMessages();
            var files = new SavegameFileCommands(recentFiles, new Mock<IFileDialogs>().Object, shownMessages);
            return new ShellWindow(new RecentFilesMenuViewModel(recentFiles), launcher ?? new ShellUrlLauncher(_ => { }), About, Help, shownMessages, files) { DataContext = shell };
        }

        private static IMessageService CancellingMessages()
        {
            var messages = new Mock<IMessageService>();
            messages.Setup(m => m.AskYesNoCancel(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageAnswer.Cancel);
            return messages.Object;
        }

        /// <summary>
        /// Returns a shell with the HoMM3 module only; the shell with both games is tested in the tests of the application.
        /// </summary>
        public static ShellViewModel CreateShell() => new([Services.Module], Settings, ApplicationName);

        /// <summary>
        /// Returns a shell whose HoMM3 editor is <paramref name="editor"/>, so a test can load a game into the editor and look at the window.
        /// </summary>
        public static ShellViewModel CreateShell(Homm3EditorViewModel editor) => new([new Homm3Module(() => editor)], Settings, ApplicationName);

        /// <summary>
        /// Writes <paramref name="castle"/> to <paramref name="chunk"/>.
        /// </summary>
        public static CastleChunk SerializeCastle(CastleChunk chunk, CastleModel castle) => CastleSerializer.Serialize(chunk, castle);

        public static Homm3EditorViewModel CreateHomm3Editor() => Services.CreateEditor();

        public static HeroViewModel CreateHeroViewModel(HeroEditSession session) => new(session, Services.EffectLines, Services.SpellPoints.Create(session.History.Current.GameVersion()), Services.ByteEditTargets);

        public static CastleViewModel CreateCastleViewModel(CastleEditSession session) => new(session, Services.ByteEditTargets, Services.CastleBuildMessages);
    }
}
