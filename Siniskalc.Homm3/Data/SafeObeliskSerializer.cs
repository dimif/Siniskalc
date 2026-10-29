namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// Writes the obelisk flags: the byte of every obelisk, as the model holds it.
    /// </summary>
    public sealed class SafeObeliskSerializer : ISerializer<ObeliskChunk, ObeliskVisits<PlayerColor>>
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
