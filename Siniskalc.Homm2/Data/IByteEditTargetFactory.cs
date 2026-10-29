namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// Makes the targets the hex editor of a page works on: the raw bytes of one hero, player or castle, or of a record of the map (a mine, a
    /// shrine, a sphinx, whose turn it is, the castles that built today, the obelisks), with the fields that are known. The caller disposes the target.
    /// </summary>
    public interface IByteEditTargetFactory
    {
        IByteEditTarget ForHero(HeroEditSession session);

        IByteEditTarget ForPlayer(PlayerEditSession session);

        IByteEditTarget ForCastle(CastleEditSession session);

        IByteEditTarget ForMine(SaveDocumentHistory history, int mineIndex);

        IByteEditTarget ForShrine(SaveDocumentHistory history, int shrineIndex);

        IByteEditTarget ForSphinx(SaveDocumentHistory history, int sphinxIndex);

        IByteEditTarget ForTurn(SaveDocumentHistory history);

        IByteEditTarget ForBuiltToday(SaveDocumentHistory history);

        IByteEditTarget ForObelisks(SaveDocumentHistory history);
    }
}
