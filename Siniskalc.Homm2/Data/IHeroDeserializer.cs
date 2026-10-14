namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the record of a hero as a model.
    /// </summary>
    public interface IHeroDeserializer : IDeserializer<HeroChunk, HeroModel>
    {
        /// <summary>
        /// Reads only the player the hero serves, which is cheaper than the whole model.
        /// </summary>
        Color? DeserializeOwner(HeroChunk chunk);
    }
}
