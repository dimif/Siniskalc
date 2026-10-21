namespace Siniskalc.Homm2.Data
{
    using System.Linq;
    using Siniskalc.Core.Models;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads which castles have built today: a bit for each castle slot.
    /// </summary>
    public sealed class BuiltTodayDeserializer : IMapObjectDeserializer<BuiltTodayModel>
    {
        /// <inheritdoc/>
        public BuiltTodayModel Deserialize(MapObjectChunk chunk) =>
            new(chunk.Index, new Numbers(Enumerable.Range(0, Constants.Table.CastleSlotCount).Where(castle => (chunk.GetByteAt(castle / 8) & (1 << (castle % 8))) != 0)));
    }
}
