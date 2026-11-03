namespace Siniskalc.Homm3.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Builds a <see cref="Game"/> from the (decompressed) bytes of a savefile: finds the structs in it and what it says about the game.
    /// </summary>
    public sealed class SavegameParser : ISavegameParser<Game>
    {
        private static readonly ILogger<SavegameParser> Logger = LogFactory.For<SavegameParser>();

        private readonly ISavegameVersionDetector versionDetector;
        private readonly IHeroBytesLoader heroLoader;
        private readonly Func<GameVersion, ICastleBytesLoader> castleLoader;
        private readonly IPlayerBytesLoader playerLoader;
        private readonly IObeliskBytesLoader obeliskLoader;
        private readonly Func<GameVersion, IMineBytesLoader> mineLoader;
        private readonly Func<GameVersion, IDwellingBytesLoader> dwellingLoader;
        private readonly Func<GameVersion, IMerchantBytesLoader> merchantLoader;
        private readonly IHeroOwnersBytesLoader heroOwnersLoader;
        private readonly ITurnBytesLoader turnLoader;
        private readonly IMapPlacesLoader placesLoader;
        private readonly IGameDateLocator dateLocator;
        private readonly IPlayerControlLocator setupLocator;
        private readonly IMapHeaderLocator mapHeaderLocator;
        private readonly ISaveDocumentFactory documentFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SavegameParser"/> class.
        /// </summary>
        /// <param name="versionDetector">The version detector.</param>
        /// <param name="heroLoader">The hero loader.</param>
        /// <param name="castleLoader">The castle loader.</param>
        /// <param name="playerLoader">The player loader.</param>
        /// <param name="obeliskLoader">The obelisk loader.</param>
        /// <param name="mineLoader">The mine loader.</param>
        /// <param name="dwellingLoader">The dwelling loader.</param>
        /// <param name="merchantLoader">The merchant loader.</param>
        /// <param name="heroOwnersLoader">The hero owners loader.</param>
        /// <param name="turnLoader">The turn loader.</param>
        /// <param name="placesLoader">The places loader.</param>
        /// <param name="dateLocator">The date locator.</param>
        /// <param name="setupLocator">The setup locator.</param>
        /// <param name="mapHeaderLocator">The map header locator.</param>
        /// <param name="documentFactory">The document factory.</param>
        public SavegameParser(
            ISavegameVersionDetector versionDetector,
            IHeroBytesLoader heroLoader,
            Func<GameVersion, ICastleBytesLoader> castleLoader,
            IPlayerBytesLoader playerLoader,
            IObeliskBytesLoader obeliskLoader,
            Func<GameVersion, IMineBytesLoader> mineLoader,
            Func<GameVersion, IDwellingBytesLoader> dwellingLoader,
            Func<GameVersion, IMerchantBytesLoader> merchantLoader,
            IHeroOwnersBytesLoader heroOwnersLoader,
            ITurnBytesLoader turnLoader,
            IMapPlacesLoader placesLoader,
            IGameDateLocator dateLocator,
            IPlayerControlLocator setupLocator,
            IMapHeaderLocator mapHeaderLocator,
            ISaveDocumentFactory documentFactory)
        {
            this.versionDetector = versionDetector;
            this.heroLoader = heroLoader;
            this.castleLoader = castleLoader;
            this.playerLoader = playerLoader;
            this.obeliskLoader = obeliskLoader;
            this.mineLoader = mineLoader;
            this.dwellingLoader = dwellingLoader;
            this.merchantLoader = merchantLoader;
            this.heroOwnersLoader = heroOwnersLoader;
            this.turnLoader = turnLoader;
            this.placesLoader = placesLoader;
            this.dateLocator = dateLocator;
            this.setupLocator = setupLocator;
            this.mapHeaderLocator = mapHeaderLocator;
            this.documentFactory = documentFactory;
        }

        /// <inheritdoc/>
        public Game Parse(byte[] bytes, string? file, string? name = null)
        {
            var gameVersion = this.versionDetector.Detect(bytes);
            if (gameVersion == GameVersion.Unknown)
            {
                throw new UnsupportedSavefileException("This is not a Heroes of Might and Magic III savegame, or it comes from a version of the game this editor does not know.");
            }

            var heroChunks = this.heroLoader.LoadHeroChunks(bytes, gameVersion);

            var castleChunks = this.castleLoader(gameVersion).LoadCastleChunks(bytes);
            var setup = this.setupLocator.FindSetup(bytes, gameVersion);
            var allPlayerChunks = this.playerLoader.LoadPlayerChunks(bytes, gameVersion, heroChunks.Cast<BytesChunk>().Concat(castleChunks), includeInactive: true);
            var playerChunks = PlayersInTheGame.Of(PlayersInTheGame.WithActivity(allPlayerChunks), setup?.PlayerControls);

            var obeliskChunks = this.obeliskLoader.LoadObeliskChunks(bytes, playerChunks);
            var mineChunks = this.mineLoader(gameVersion).LoadMineChunks(bytes);
            var dwellingChunks = this.dwellingLoader(gameVersion).LoadDwellingChunks(bytes, heroChunks.Cast<BytesChunk>().Concat(castleChunks).Concat(playerChunks).Concat(obeliskChunks).Concat(mineChunks));

            var afterPlayers = playerChunks.Count == 0 ? 0 : playerChunks[^1].End;
            var metadata = new SaveMetadata<PlayerColor>(
                Date: this.dateLocator.Find(bytes, gameVersion, afterPlayers),
                Map: this.mapHeaderLocator.Find(bytes, gameVersion),
                Setup: setup);

            var offsets = new HeroOffsets(gameVersion);
            var document = this.documentFactory.Create(bytes, heroChunks, offsets, playerChunks, castleChunks, obeliskChunks, mineChunks, dwellingChunks, this.merchantLoader(gameVersion).LoadMerchantChunks(bytes), this.heroOwnersLoader.LoadHeroOwnerChunks(bytes, gameVersion, afterPlayers), this.turnLoader.LoadTurnChunks(bytes, gameVersion), allPlayerChunks.Except(playerChunks));
            var game = new Game(file, bytes, gameVersion, new SaveDocumentHistory(document), name, metadata, this.placesLoader.Load(bytes, document.Catalog()).WithStanding(StandingOn(document)));
            Logger.LogInformation(
                "Opened {File} as Heroes III {Version}: {Heroes} heroes, {Castles} castles, {Players} players, {Mines} mines, {Dwellings} dwellings",
                LogPaths.Label(file, name),
                gameVersion,
                heroChunks.Count,
                castleChunks.Count,
                playerChunks.Count,
                mineChunks.Count,
                dwellingChunks.Count);
            return game;
        }

        // A hero that stands on a place of a kind hides it on the map; its own record has the type of the place (+11), whether it can be visited
        // (+15) and the number of the place among those of its type (+16).
        private static IEnumerable<(VisitedPlaceKind Kind, int Number, MapPosition Position)> StandingOn(SaveDocument document)
        {
            var heroes = document.Heroes();
            for (var i = 0; i < heroes.Count; i++)
            {
                var kind = document.Catalog().VisitablePlaces.All.FirstOrDefault(k => k.ObjectType == (int)heroes[i].GetUInt32At(11));
                if (kind != null && heroes[i].GetByteAt(15) == 1 && heroes.GetModel(i).Position is { X: >= 0 } position)
                {
                    yield return (kind, heroes[i].GetByteAt(16) & kind.NumberMask, position);
                }
            }
        }
    }
}
