namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Cuts the records of heroes, players and castles out of the bytes of a savegame, from where the layout says they are.
    /// </summary>
    public interface IHomm2BytesLoader
    {
        IReadOnlyList<Homm2HeroChunk> LoadHeroChunks(byte[] bytes, Homm2SaveLayout layout);

        IReadOnlyList<Homm2PlayerChunk> LoadPlayerChunks(byte[] bytes, Homm2SaveLayout layout);

        IReadOnlyList<Homm2CastleChunk> LoadCastleChunks(byte[] bytes, Homm2SaveLayout layout);
    }
}
