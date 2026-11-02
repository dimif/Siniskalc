namespace Siniskalc.Homm2.Data
{
    using System.Linq;
    using System.Text;
    using Siniskalc.Core.Data;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the block of a sphinx (see <see cref="MapObjectLayout"/>).
    /// </summary>
    public sealed class SphinxDeserializer : IDeserializer<MapObjectChunk, SphinxModel>
    {
        private readonly GameCatalog catalog;

        /// <summary>
        /// Initializes a new instance of the <see cref="SphinxDeserializer"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        public SphinxDeserializer(GameCatalog catalog)
        {
            this.catalog = catalog;
        }

        /// <inheritdoc/>
        public SphinxModel Deserialize(MapObjectChunk chunk)
        {
            var bytes = chunk.ToArray();
            var count = bytes[MapObjectLayout.SphinxAnswerCount];
            return new SphinxModel(
                chunk.Index,
                chunk.Position,
                bytes[MapObjectLayout.SphinxActive] != 0,
                Enumerable.Range(0, 7).Select(i => (int)chunk.GetUInt32At(MapObjectLayout.SphinxReward + (i * 4))).ToList(),
                chunk.GetUInt16At(MapObjectLayout.SphinxArtifact),
                Enumerable.Range(0, count).Select(i => Text(bytes, MapObjectLayout.SphinxAnswers + (i * MapObjectLayout.SphinxAnswerSize), MapObjectLayout.SphinxAnswerSize)).ToList(),
                Text(bytes, MapObjectLayout.SphinxRiddle, bytes.Length - MapObjectLayout.SphinxRiddle),
                bytes.Length - MapObjectLayout.SphinxRiddle - 1,
                this.catalog);
        }

        private static string Text(byte[] bytes, int start, int length)
        {
            var end = System.Array.IndexOf(bytes, (byte)0, start, length);
            return Encoding.Latin1.GetString(bytes, start, (end < 0 ? start + length : end) - start);
        }
    }
}
