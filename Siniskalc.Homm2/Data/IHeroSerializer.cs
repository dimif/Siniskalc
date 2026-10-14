namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Writes a model into the record of a hero.
    /// </summary>
    public interface IHeroSerializer : ISerializer<HeroChunk, HeroModel>
    {
    }
}
