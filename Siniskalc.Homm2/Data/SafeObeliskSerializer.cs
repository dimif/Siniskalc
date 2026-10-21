namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm2.Entities;
    using Siniskalc.Homm2.Models;

    /// <summary>
    /// Writes the obelisk flags: the byte of every obelisk the chunk has, as the model holds it. The bytes of the room the map does not use are left alone.
    /// </summary>
    public sealed class SafeObeliskSerializer : IObeliskSerializer
    {
        /// <inheritdoc/>
        public ObeliskChunk Serialize(ObeliskChunk chunk, ObeliskVisits<PlayerColor> visits)
        {
            var bytes = chunk.ToArray();
            for (var i = 0; i < chunk.Positions.Count && i < visits.Count; i++)
            {
                bytes[i] = visits.Masks[i];
            }

            return chunk.WithBytes(bytes);
        }
    }
}
