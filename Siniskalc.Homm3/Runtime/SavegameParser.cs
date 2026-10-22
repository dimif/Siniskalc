namespace Siniskalc.Homm3.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Data;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Builds a <see cref="Game"/> from the (decompressed) bytes of a savefile: finds the structs in it and what it says about the game.
    /// </summary>
    public sealed class SavegameParser : ISavegameParser
    {
        private readonly ISavegameVersionDetector versionDetector;
        private readonly IHeroBytesLoader heroLoader;
        private readonly ICastleBytesLoader castleLoader;
        private readonly IPlayerBytesLoader playerLoader;
        private readonly IObeliskBytesLoader obeliskLoader;
        private readonly IMineBytesLoader mineLoader;
        private readonly IDwellingBytesLoader dwellingLoader;
        private readonly IMapPlacesLoader placesLoader;
        private readonly IGameDateLocator dateLocator;
        private readonly IPlayerControlLocator setupLocator;
        private readonly IMapHeaderLocator mapHeaderLocator;
        private readonly ISaveDocumentFactory documentFactory;

        public SavegameParser(
            ISavegameVersionDetector versionDetector,
            IHeroBytesLoader heroLoader,
            ICastleBytesLoader castleLoader,
            IPlayerBytesLoader playerLoader,
            IObeliskBytesLoader obeliskLoader,
            IMineBytesLoader mineLoader,
            IDwellingBytesLoader dwellingLoader,
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
            this.placesLoader = placesLoader;
            this.dateLocator = dateLocator;
            this.setupLocator = setupLocator;
            this.mapHeaderLocator = mapHeaderLocator;
            this.documentFactory = documentFactory;
        }

        public Game Parse(byte[] bytes, string? file, string? name = null)
        {
            var gameVersion = this.versionDetector.Detect(bytes);
            if (gameVersion == GameVersion.Unknown)
            {
                // TODO:LOG warning: {file or name} ({bytes.Length} bytes) was refused as unsupported; log its first bytes (the header) to help recognise the version.
                throw new UnsupportedSavefileException("This is not a Heroes of Might and Magic III savegame, or it comes from a version of the game this editor does not know.");
            }

            var heroChunks = this.heroLoader.LoadHeroChunks(bytes, gameVersion);

            var castleChunks = this.castleLoader.LoadCastleChunks(bytes);
            var setup = this.setupLocator.FindSetup(bytes, gameVersion);
            var playerChunks = PlayersInTheGame.Of(
                this.playerLoader.LoadPlayerChunks(bytes, gameVersion, heroChunks.Cast<BytesChunk>().Concat(castleChunks)),
                setup?.PlayerControls);

            var obeliskChunks = this.obeliskLoader.LoadObeliskChunks(bytes, playerChunks);
            var mineChunks = this.mineLoader.LoadMineChunks(bytes);
            var dwellingChunks = this.dwellingLoader.LoadDwellingChunks(bytes, mineChunks, heroChunks.Cast<BytesChunk>().Concat(castleChunks).Concat(playerChunks).Concat(obeliskChunks).Concat(mineChunks));

            var metadata = new SaveMetadata(
                this.dateLocator.Find(bytes, gameVersion, playerChunks.Count == 0 ? 0 : playerChunks[^1].End),
                setup,
                this.mapHeaderLocator.Find(bytes, gameVersion));

            var offsets = new HeroOffsets(gameVersion);
            var document = this.documentFactory.Create(bytes, heroChunks, offsets, playerChunks, castleChunks, obeliskChunks, mineChunks, dwellingChunks);
            return new Game(file, bytes, gameVersion, new SaveDocumentHistory(document), name, metadata, this.placesLoader.Load(bytes).WithStanding(StandingOn(document)));
        }

        // A hero that stands on a place of a kind hides it on the map; its own record has the type of the place (+11), whether it can be visited
        // (+15) and the number of the place among those of its type (+16).
        private static IEnumerable<(VisitedPlaceKind Kind, int Number, MapPosition Position)> StandingOn(SaveDocument document)
        {
            var heroes = document.Heroes();
            for (var i = 0; i < heroes.Count; i++)
            {
                var kind = VisitedPlaceKind.All.FirstOrDefault(k => k.ObjectType == (int)heroes[i].GetUInt32At(11));
                if (kind != null && heroes[i].GetByteAt(15) == 1 && heroes.GetModel(i).Position is { X: >= 0 } position)
                {
                    yield return (kind, heroes[i].GetByteAt(16), position);
                }
            }
        }
    }
}
