namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// Makes the targets the hex editor of a page works on: the raw bytes of one hero, player or castle, with the fields that are known.
    /// The caller disposes the target.
    /// </summary>
    public interface IHomm2ByteEditTargetFactory
    {
        IByteEditTarget ForHero(Homm2HeroEditSession session);

        IByteEditTarget ForPlayer(Homm2PlayerEditSession session);

        IByteEditTarget ForCastle(Homm2CastleEditSession session);
    }
}
