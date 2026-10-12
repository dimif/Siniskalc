namespace Siniskalc.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// Reads a hero from its chunk.
    /// </summary>
    public interface IHeroDeserializer : IDeserializer<HeroChunk, HeroModel>
    {
        int DeserializeHeroId(HeroChunk chunk);

        HeroClass DeserializeClass(HeroChunk chunk);

        HeroLevel DeserializeLevel(HeroChunk chunk);

        PlayerColor? DeserializeOwner(HeroChunk chunk);
    }
}
