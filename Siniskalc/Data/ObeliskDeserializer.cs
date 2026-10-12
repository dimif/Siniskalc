namespace Siniskalc.Data
{
    using System.Linq;
    using Siniskalc.Models;

    /// <summary>
    /// Reads the obelisk flags of a savefile: one byte for each obelisk with a bit for every colour that visited it.
    /// </summary>
    public sealed class ObeliskDeserializer : IObeliskDeserializer
    {
        public ObeliskVisits Deserialize(ObeliskChunk chunk)
        {
            var count = chunk.GetByteAt(ObeliskLayout.CountOffset);
            return new ObeliskVisits(Enumerable.Range(0, count).Select(i => chunk.GetByteAt(ObeliskLayout.FlagsOffset + i)), chunk.Positions);
        }
    }
}
