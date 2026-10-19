namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Reads whose turn it is: the byte is the number of a record; the chunk carries the colours of the records.
    /// </summary>
    public sealed class TurnDeserializer : IMapObjectDeserializer<TurnModel>
    {
        public TurnModel Deserialize(MapObjectChunk chunk)
        {
            var record = chunk.GetByteAt(0);
            return new TurnModel(chunk.Index, record, record < Constants.Table.PlayerCount ? Color.All[PlayerRecordOrder.Unpack(chunk.Tag, record)] : null);
        }
    }
}
