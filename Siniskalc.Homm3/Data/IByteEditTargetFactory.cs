namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Creates the targets the hex editor works on. The caller disposes what it gets.
    /// </summary>
    public interface IByteEditTargetFactory
    {
        IByteEditTarget ForHero(HeroEditSession session);

        IByteEditTarget ForPlayer(PlayerEditSession session);

        IByteEditTarget ForCastle(SaveDocumentHistory history, int castleIndex);
    }
}
