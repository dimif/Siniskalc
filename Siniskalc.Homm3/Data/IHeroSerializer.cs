namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Writes a hero back into its chunk.
    /// </summary>
    public interface IHeroSerializer : ISerializer<HeroChunk, HeroModel>
    {
    }
}
