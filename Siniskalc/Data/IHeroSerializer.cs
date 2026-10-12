namespace Siniskalc.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Models;

    /// <summary>
    /// Writes a hero back into its chunk.
    /// </summary>
    public interface IHeroSerializer : ISerializer<HeroChunk, HeroModel>
    {
    }
}
