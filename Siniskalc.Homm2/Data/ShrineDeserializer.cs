namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads a shrine from the two bytes of its tile that hold the spell. A shrine a hero stands on has the marker of the hero there instead,
    /// and reads as a shrine of an unknown spell (-1).
    /// </summary>
    public sealed class ShrineDeserializer : IDeserializer<MapObjectChunk, ShrineModel>
    {
        private readonly GameCatalog catalog;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShrineDeserializer"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        public ShrineDeserializer(GameCatalog catalog)
        {
            this.catalog = catalog;
        }

        /// <inheritdoc/>
        public ShrineModel Deserialize(MapObjectChunk chunk)
        {
            var hidden = (chunk.Tag & MapObjectsReader.HiddenShrine) != 0;
            return new ShrineModel(chunk.Index, chunk.Tag & 3, chunk.Position!, hidden ? -1 : chunk.GetUInt16At(0) >> MapObjectLayout.ShrineSpellShift, this.catalog);
        }
    }
}
