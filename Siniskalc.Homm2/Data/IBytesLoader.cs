namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Cuts the records of heroes, players and castles out of the bytes of a savegame, from where the layout says they are.
    /// </summary>
    public interface IBytesLoader
    {
        IReadOnlyList<HeroChunk> LoadHeroChunks(byte[] bytes, SaveLayout layout);

        IReadOnlyList<PlayerChunk> LoadPlayerChunks(byte[] bytes, SaveLayout layout);

        IReadOnlyList<CastleChunk> LoadCastleChunks(byte[] bytes, SaveLayout layout);

        /// <summary>
        /// Cuts the flags of the obelisks out: one chunk, or none when the savegame has no obelisks or its table of tiles was not found.
        /// </summary>
        IReadOnlyList<ObeliskChunk> LoadObeliskChunks(byte[] bytes, SaveLayout layout);
    }
}
