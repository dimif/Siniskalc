namespace Siniskalc.Data
{
    using Siniskalc.Core.Data;

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
