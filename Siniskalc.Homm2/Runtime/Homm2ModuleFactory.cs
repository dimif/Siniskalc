namespace Siniskalc.Homm2.Runtime
{
    using Siniskalc.Heroes.Data;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Heroes.Wpf.ViewModels;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;
    using Siniskalc.Homm2.ViewModels;

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

            var locator = new SaveLayoutLocator();
            var pendingChanges = new Homm2PendingChangesFactory();
            this.GameService = new GameService(
                new SavegameParser(
                    locator,
                    new BytesLoader(),
                    new HeaderReader(),
                    new SaveDocumentFactory(
                        new HeroDeserializer(),
                        new SafeHeroSerializer(),
                        new HeroDiffer(pendingChanges),
                        new PlayerDeserializer(),
                        new SafePlayerSerializer(),
                        new PlayerDiffer(pendingChanges),
                        new CastleDeserializer(),
                        new SafeCastleSerializer(),
                        new CastleDiffer(pendingChanges),
                        new ObeliskDeserializer(),
                        new SafeObeliskSerializer(),
                        new ObeliskDiffer<PlayerColor>(PlayerColor.All, pendingChanges.ForObelisks),
                        new MapObjectKit<MineModel>(new MineDeserializer(), new SafeMineSerializer(), new MineDiffer(pendingChanges)),
                        new MapObjectKit<ShrineModel>(new ShrineDeserializer(), new SafeShrineSerializer(), new ShrineDiffer(pendingChanges)),
                        new MapObjectKit<SphinxModel>(new SphinxDeserializer(), new SafeSphinxSerializer(), new SphinxDiffer(pendingChanges)),
                        new MapObjectKit<TurnModel>(new TurnDeserializer(), new SafeTurnSerializer(), new TurnDiffer(pendingChanges)),
                        new MapObjectKit<BuiltTodayModel>(new BuiltTodayDeserializer(), new SafeBuiltTodaySerializer(), new BuiltTodayDiffer(pendingChanges)),
                        bytesPatcher),
                    new MapPlacesReader(),
                    new MapObjectsReader()),
                new RawSavefileCodec(),
                backup);
            this.SaveRegionProvider = new SaveRegionProvider(locator);
            this.ByteEditTargets = new ByteEditTargetFactory();
            this.MaxSpellPointsCalculator = new MaxSpellPointsCalculator();
            this.Module = new Homm2Module(locator, this.CreateEditor);
        }

        /// <summary>
        /// Gets the game module of Heroes of Might and Magic II.
        /// </summary>
        public IGameModule<Homm2EditorViewModel> Module { get; }

        public IGameService GameService { get; }

        public ISaveRegionProvider SaveRegionProvider { get; }

        public IByteEditTargetFactory ByteEditTargets { get; }

        public IMaxSpellPointsCalculator<SecondarySkill, SecondarySkills> MaxSpellPointsCalculator { get; }

        /// <summary>
        /// Makes the editor of Heroes of Might and Magic II, with nothing loaded.
        /// </summary>
        public Homm2EditorViewModel CreateEditor() => new(this.GameService, this.SaveRegionProvider, this.ByteEditTargets, new NavigationBuilder(), this.banishingText, this.MaxSpellPointsCalculator);
    }
}
