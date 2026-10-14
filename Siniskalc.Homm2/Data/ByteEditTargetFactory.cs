namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <inheritdoc/>
    public sealed class ByteEditTargetFactory : IByteEditTargetFactory
    {
        public IByteEditTarget ForHero(HeroEditSession session) =>
            new ChunkByteEditTarget<HeroChunk, HeroModel>(session, RecordRegions.Hero, () => session.Current.Name);

        public IByteEditTarget ForPlayer(PlayerEditSession session) =>
            new ChunkByteEditTarget<PlayerChunk, PlayerModel>(session, RecordRegions.Player, () => $"{session.Current.Color} player");

        public IByteEditTarget ForCastle(CastleEditSession session) =>
            new ChunkByteEditTarget<CastleChunk, CastleModel>(session, RecordRegions.Castle, () => session.Current.Name);
    }
}
