namespace Siniskalc.Heroes.Data
{
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// Reads the obelisk flags of a savefile: one byte for each obelisk, with a bit for every colour that visited it.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game.</typeparam>
    public sealed class ObeliskDeserializer<TColor> : IDeserializer<ObeliskChunk, ObeliskVisits<TColor>>
        where TColor : AbstractEnumeration, IPlayerColor
    {
        private readonly ObeliskFlagsLayout layout;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObeliskDeserializer{TColor}"/> class.
        /// </summary>
        /// <param name="layout">Says which byte of the chunk holds the flags of each obelisk.</param>
        public ObeliskDeserializer(ObeliskFlagsLayout layout)
        {
            this.layout = layout;
        }

        /// <inheritdoc/>
        public ObeliskVisits<TColor> Deserialize(ObeliskChunk chunk)
        {
            var count = this.layout.CountOffset is { } offset ? chunk.GetByteAt(offset) : chunk.Positions.Count;
            return new ObeliskVisits<TColor>(Enumerable.Range(0, count).Select(i => chunk.GetByteAt(this.layout.FlagsOffset + i)), chunk.Positions);
        }
    }
}
