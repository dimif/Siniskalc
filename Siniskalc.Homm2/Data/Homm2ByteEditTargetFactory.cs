namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <inheritdoc/>
    public sealed class Homm2ByteEditTargetFactory : IHomm2ByteEditTargetFactory
    {
        public IByteEditTarget ForHero(Homm2HeroEditSession session) =>
            new ChunkByteEditTarget<Homm2HeroChunk, Homm2HeroModel>(session, Homm2RecordRegions.Hero, () => session.Current.Name);

        public IByteEditTarget ForPlayer(Homm2PlayerEditSession session) =>
            new ChunkByteEditTarget<Homm2PlayerChunk, Homm2PlayerModel>(session, Homm2RecordRegions.Player, () => $"{session.Current.Color} player");

        public IByteEditTarget ForCastle(Homm2CastleEditSession session) =>
            new ChunkByteEditTarget<Homm2CastleChunk, Homm2CastleModel>(session, Homm2RecordRegions.Castle, () => session.Current.Name);
    }
}
