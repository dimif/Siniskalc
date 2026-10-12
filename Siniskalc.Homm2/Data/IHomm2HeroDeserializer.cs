namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the record of a hero as a model.
    /// </summary>
    public interface IHomm2HeroDeserializer : IDeserializer<Homm2HeroChunk, Homm2HeroModel>
    {
        /// <summary>
        /// Reads only the player the hero serves, which is cheaper than the whole model.
        /// </summary>
        Homm2Color? DeserializeOwner(Homm2HeroChunk chunk);
    }
}
