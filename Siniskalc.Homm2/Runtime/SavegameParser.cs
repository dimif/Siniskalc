namespace Siniskalc.Homm2.Runtime
{
    using System;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Homm2.Data;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// Builds a game from the bytes of a savegame of Heroes of Might and Magic II, the base game's or the Price of Loyalty's.
    /// </summary>
    public sealed class SavegameParser : ISavegameParser<Game>
    {
        private static readonly ILogger<SavegameParser> Logger = LogFactory.For<SavegameParser>();

        private readonly ISaveLayoutLocator locator;
        private readonly IBytesLoader loader;
        private readonly IHeaderReader headerReader;
        private readonly ISaveDocumentFactory documentFactory;
        private readonly IMapPlacesReader placesReader;
        private readonly IMapObjectsReader objectsReader;
        private readonly GameCatalogs catalogs;
        private readonly Func<GameVersion, ISaveDocumentFactory>? documentFactoryForVersion;

        public SavegameParser(ISaveLayoutLocator locator, IBytesLoader loader, IHeaderReader headerReader, ISaveDocumentFactory documentFactory, IMapPlacesReader placesReader, IMapObjectsReader objectsReader, GameCatalogs catalogs, Func<GameVersion, ISaveDocumentFactory>? documentFactoryForVersion = null)
        {
            this.locator = locator;
            this.loader = loader;
            this.headerReader = headerReader;
            this.documentFactory = documentFactory;
            this.placesReader = placesReader;
            this.objectsReader = objectsReader;
            this.catalogs = catalogs;
            this.documentFactoryForVersion = documentFactoryForVersion;
        }

        /// <inheritdoc/>
        public Game Parse(byte[] bytes, string? file, string? name = null)
        {
            var layout = this.locator.Locate(bytes);
            if (layout is null)
            {
                throw new UnsupportedSavefileException("This is not a Heroes of Might and Magic II savegame, or it comes from a version of the game this editor does not know.");
            }

            var heroes = this.loader.LoadHeroChunks(bytes, layout);
            var players = this.loader.LoadPlayerChunks(bytes, layout);
            var castles = this.loader.LoadCastleChunks(bytes, layout);
            var document = (this.documentFactoryForVersion?.Invoke(layout.Version) ?? this.documentFactory).Create(
                bytes,
                heroes,
                players,
                castles,
                this.loader.LoadObeliskChunks(bytes, layout),
                this.objectsReader.Read(bytes, layout));
            var game = new Game(file, bytes, layout, new SaveDocumentHistory(document), this.headerReader.Read(bytes, layout), name) { Catalog = this.catalogs.For(layout.Version), Places = this.placesReader.Read(bytes, layout) };
            Logger.LogInformation(
                "Opened {File} as Heroes II {Version}: {Heroes} heroes, {Castles} castles, {Players} players",
                LogPaths.Label(file, name),
                layout.Version,
                heroes.Count,
                castles.Count,
                players.Count);
            return game;
        }
    }
}
