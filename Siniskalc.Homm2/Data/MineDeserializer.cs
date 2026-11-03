namespace Siniskalc.Homm2.Data
{
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the record of a mine: its owner, its kind and (from the chunk) where it is.
    /// </summary>
    public sealed class MineDeserializer : IDeserializer<MapObjectChunk, MineModel>
    {
        private static readonly ILogger<MineDeserializer> Logger = LogFactory.For<MineDeserializer>();

        private readonly GameCatalog catalog;

        /// <summary>
        /// Initializes a new instance of the <see cref="MineDeserializer"/> class.
        /// </summary>
        /// <param name="catalog">The catalog.</param>
        public MineDeserializer(GameCatalog catalog)
        {
            this.catalog = catalog;
        }

        /// <inheritdoc/>
        public MineModel Deserialize(MapObjectChunk chunk)
        {
            using var subject = Logger.BeginSubject("Mine", chunk.Index.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return new(chunk.Index, (MineKind)chunk.GetByteAt(1), chunk.Position!, Bytes.ReadColor(chunk.GetByteAt(0)), this.catalog);
        }
    }
}
