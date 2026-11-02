namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Finds the table of hero owners in a savefile (see <see cref="HeroOwnersLayout"/>).
    /// </summary>
    public interface IHeroOwnersBytesLoader
    {
        /// <param name="bytes">The savefile bytes.</param>
        /// <param name="gameVersion">The game version that wrote the savefile.</param>
        /// <param name="searchFrom">Where the date of the game is looked for from: the end of the players.</param>
        /// <returns>
        /// The table as one chunk, or nothing when the savefile's version has no known table or the bytes in front of the date are not one.
        /// </returns>
        IReadOnlyList<RecordChunk> LoadHeroOwnerChunks(byte[] bytes, GameVersion gameVersion, int searchFrom);
    }
}
