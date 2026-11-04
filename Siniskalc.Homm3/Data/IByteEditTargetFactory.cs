namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// Creates the targets the hex editor works on. The caller disposes what it gets.
    /// </summary>
    public interface IByteEditTargetFactory
    {
        IByteEditTarget ForHero(HeroEditSession session);

        IByteEditTarget ForPlayer(PlayerEditSession session);

        IByteEditTarget ForCastle(CastleEditSession session);

        IByteEditTarget ForMine(SaveDocumentHistory history, int mineIndex);

        IByteEditTarget ForDwelling(SaveDocumentHistory history, int dwellingIndex);

        IByteEditTarget ForCreatureBank(SaveDocumentHistory history, int index);

        IByteEditTarget ForMerchant(SaveDocumentHistory history, int index);
    }
}
