namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

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
