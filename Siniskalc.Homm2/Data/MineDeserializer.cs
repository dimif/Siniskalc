namespace Siniskalc.Homm2.Data
{
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads the record of a mine: its owner, kind, guardian and (from the chunk) where it is.
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
            return new(chunk.Index, (MineKind)chunk.GetByteAt(1), chunk.Position!, Bytes.ReadColor(chunk.GetByteAt(0)), this.catalog)
            {
                Guardian = this.ReadGuardian(chunk),
            };
        }

        private MineGuardian? ReadGuardian(MapObjectChunk chunk)
        {
            var spell = chunk.GetByteAt(MapObjectLayout.MineGuardianSpell);
            var count = chunk.GetByteAt(MapObjectLayout.MineGuardianCount);
            if (spell == 0xFF && count is 0 or 0xFF)
            {
                return null;
            }

            var creatureId = spell switch
            {
                62 => "EarthElemental",
                63 => "AirElemental",
                64 => "FireElemental",
                65 => "WaterElemental",
                _ => null,
            };
            var creature = creatureId is null ? null : this.catalog.Creatures.FirstOrDefault(c => c.Id == creatureId);
            if (creature is null || count == 0)
            {
                Logger.LogTrace("Mine guardian spell {SpellCode} and count {Count} are unrecognized and are kept for read-only display", spell, count);
                return new(spell, count, null);
            }

            return new(spell, count, creature);
        }
    }
}
