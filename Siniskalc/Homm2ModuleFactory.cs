namespace Siniskalc
{
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.ViewModels;
    using H2Data = Siniskalc.Homm2.Data;
    using H2Runtime = Siniskalc.Homm2.Runtime;

    /// <summary>
    /// Builds the components of Heroes of Might and Magic II, once, from what the games share, and the module that holds them.
    /// </summary>
    public sealed class Homm2ModuleFactory
    {
        private readonly BanishingTextProvider banishingText;

        /// <param name="backup">Keeps a copy of a savefile before it is replaced.</param>
        /// <param name="bytesPatcher">Applies the edits of a document to the bytes of a savefile.</param>
        /// <param name="banishingText">Words the countdown of a player with no town is shown with.</param>
        public Homm2ModuleFactory(IFileBackupServiceProvider backup, ISavefileBytesPatcher bytesPatcher, BanishingTextProvider banishingText)
        {
            this.banishingText = banishingText;

            var locator = new H2Data.SaveLayoutLocator();
            this.GameService = new H2Runtime.GameService(
                new H2Runtime.SavegameParser(
                    locator,
                    new H2Data.BytesLoader(),
                    new H2Data.HeaderReader(),
                    new H2Data.SaveDocumentFactory(
                        new H2Data.HeroDeserializer(),
                        new H2Data.SafeHeroSerializer(),
                        new H2Data.HeroDiffer(),
                        new H2Data.PlayerDeserializer(),
                        new H2Data.SafePlayerSerializer(),
                        new H2Data.PlayerDiffer(),
                        new H2Data.CastleDeserializer(),
                        new H2Data.SafeCastleSerializer(),
                        new H2Data.CastleDiffer(),
                        new H2Data.ObeliskDeserializer(),
                        new H2Data.SafeObeliskSerializer(),
                        new H2Data.ObeliskDiffer(),
                        bytesPatcher),
                    new H2Data.MapPlacesReader()),
                new RawSavefileCodec(),
                backup);
            this.SaveRegionProvider = new H2Data.SaveRegionProvider(locator);
            this.ByteEditTargets = new H2Data.ByteEditTargetFactory();
            this.Module = new H2Runtime.Homm2Module(locator, this.CreateEditor);
        }

        /// <summary>
        /// Gets the game module of Heroes of Might and Magic II.
        /// </summary>
        public IGameModule<Homm2EditorViewModel> Module { get; }

        public H2Runtime.IGameService GameService { get; }

        public ISaveRegionProvider SaveRegionProvider { get; }

        public H2Data.IByteEditTargetFactory ByteEditTargets { get; }

        /// <summary>
        /// Makes the editor of Heroes of Might and Magic II, with nothing loaded.
        /// </summary>
        public Homm2EditorViewModel CreateEditor() => new(this.GameService, this.SaveRegionProvider, this.ByteEditTargets, new Siniskalc.Homm2.ViewModels.NavigationBuilder(), this.banishingText);
    }
}
