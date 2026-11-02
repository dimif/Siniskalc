namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <inheritdoc/>
    public sealed class ByteEditTargetFactory : IByteEditTargetFactory
    {
        /// <inheritdoc/>
        public IByteEditTarget ForHero(SaveDocumentHistory history, int heroIndex)
        {
            var session = history.CreateHeroSession(heroIndex);
            return new ChunkByteEditTarget<HeroChunk, HeroModel>(session, RecordRegions.Hero, () => session.Current.Name);
        }

        /// <inheritdoc/>
        public IByteEditTarget ForPlayer(PlayerEditSession session) =>
            new ChunkByteEditTarget<PlayerChunk, PlayerModel>(session, RecordRegions.Player, () => $"{session.Current.Color} player");

        /// <inheritdoc/>
        public IByteEditTarget ForCastle(SaveDocumentHistory history, int castleIndex)
        {
            var session = history.CreateCastleSession(castleIndex);
            return new ChunkByteEditTarget<CastleChunk, CastleModel>(session, RecordRegions.Castle, () => session.Current.Name);
        }

        /// <inheritdoc/>
        public IByteEditTarget ForMine(SaveDocumentHistory history, int mineIndex)
        {
            var session = history.CreateMineSession(mineIndex);
            return new ChunkByteEditTarget<MapObjectChunk, MineModel>(session, RecordRegions.Mine);
        }

        /// <inheritdoc/>
        public IByteEditTarget ForShrine(SaveDocumentHistory history, int shrineIndex) =>
            new ChunkByteEditTarget<MapObjectChunk, ShrineModel>(history.CreateShrineSession(shrineIndex), RecordRegions.Shrine);

        /// <inheritdoc/>
        public IByteEditTarget ForSphinx(SaveDocumentHistory history, int sphinxIndex)
        {
            var session = history.CreateSphinxSession(sphinxIndex);
            return new ChunkByteEditTarget<MapObjectChunk, SphinxModel>(session, RecordRegions.Sphinx(session.Chunk.Length));
        }

        /// <inheritdoc/>
        public IByteEditTarget ForTurn(SaveDocumentHistory history) =>
            new ChunkByteEditTarget<MapObjectChunk, TurnModel>(history.CreateTurnSession(), RecordRegions.Turn);

        /// <inheritdoc/>
        public IByteEditTarget ForBuiltToday(SaveDocumentHistory history) =>
            new ChunkByteEditTarget<MapObjectChunk, BuiltTodayModel>(history.CreateBuiltTodaySession(), RecordRegions.BuiltToday);

        /// <inheritdoc/>
        public IByteEditTarget ForObelisks(SaveDocumentHistory history)
        {
            var session = history.CreateObeliskSession();
            return new ChunkByteEditTarget<ObeliskChunk, ObeliskVisits<PlayerColor>>(session, RecordRegions.Obelisks(session.Chunk.Positions.Count, session.Chunk.Length), () => "Obelisks");
        }
    }
}
