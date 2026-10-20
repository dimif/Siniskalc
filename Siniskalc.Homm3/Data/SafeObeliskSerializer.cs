namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Writes the obelisk flags: the byte of every obelisk, as the model holds it.
    /// </summary>
    public sealed class SafeObeliskSerializer : IObeliskSerializer
    {
        /// <inheritdoc/>
        public ObeliskChunk Serialize(ObeliskChunk chunk, ObeliskVisits<PlayerColor> visits)
        {
            var bytes = chunk.ToArray();
            var count = bytes[ObeliskLayout.CountOffset];
            for (var i = 0; i < count && i < visits.Count; i++)
            {
                bytes[ObeliskLayout.FlagsOffset + i] = visits.Masks[i];
            }

            return chunk.WithBytes(bytes);
        }
    }
}
