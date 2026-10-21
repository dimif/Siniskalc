namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Makes the targets the hex editor of a page works on: the raw bytes of one hero, player or castle, with the fields that are known.
    /// The caller disposes the target.
    /// </summary>
    public interface IByteEditTargetFactory
    {
        IByteEditTarget ForHero(HeroEditSession session);

        IByteEditTarget ForPlayer(PlayerEditSession session);

        IByteEditTarget ForCastle(CastleEditSession session);
    }
}
