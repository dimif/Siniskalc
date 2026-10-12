namespace Siniskalc.Data
{
    using System.Collections.Generic;
    using Siniskalc.Entities;

    /// <summary>
    /// Finds the heroes in a savefile and loads each as a <see cref="HeroChunk"/>.
    /// </summary>
    public interface IHeroBytesLoader
    {
        /// <param name="bytes">The savefile bytes.</param>
        /// <param name="gameVersion">The version of the savefile, as <see cref="ISavegameVersionDetector.Detect"/> tells.</param>
        IReadOnlyList<HeroChunk> LoadHeroChunks(byte[] bytes, GameVersion gameVersion);
    }
}
