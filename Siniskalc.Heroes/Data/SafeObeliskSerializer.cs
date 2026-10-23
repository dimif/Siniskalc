namespace Siniskalc.Heroes.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Writes the visited flag of every obelisk the chunk says it has. Bytes outside those flags are left alone.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game.</typeparam>
    public sealed class SafeObeliskSerializer<TColor> : ISerializer<ObeliskChunk, ObeliskVisits<TColor>>
        where TColor : AbstractEnumeration, IPlayerColor
    {
        private readonly ObeliskFlagsLayout layout;

        public SafeObeliskSerializer(ObeliskFlagsLayout layout)
        {
            this.layout = layout;
        }

        /// <inheritdoc/>
        public ObeliskChunk Serialize(ObeliskChunk chunk, ObeliskVisits<TColor> visits)
        {
            var bytes = chunk.ToArray();
            var count = this.layout.CountOffset is { } offset ? bytes[offset] : chunk.Positions.Count;
            for (var i = 0; i < count && i < visits.Count; i++)
            {
                bytes[this.layout.FlagsOffset + i] = visits.Masks[i];
            }

            return chunk.WithBytes(bytes);
        }
    }
}
