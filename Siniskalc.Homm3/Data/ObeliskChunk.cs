namespace Siniskalc.Homm3.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// The marker and the visited flags of the obelisks of the map: see <see cref="ObeliskLayout"/>.
    /// </summary>
    public sealed class ObeliskChunk : BytesChunk
    {
        public ObeliskChunk(string name, byte[] bytes, int startIndex, IReadOnlyList<MapPosition?>? positions = null)
            : base(name, bytes, startIndex)
        {
            this.Positions = positions ?? [];
        }

        /// <summary>
        /// Gets where each obelisk is on the map (the obelisk at the same place in the chunk), or null for one that was not found. Empty when the
        /// savefile has no list of objects that could be read. It is not part of the chunk's bytes.
        /// </summary>
        public IReadOnlyList<MapPosition?> Positions { get; }

        public override ObeliskChunk WithBytes(byte[] bytes)
        {
            return new ObeliskChunk(this.Name, this.CheckReplacement(bytes), this.StartIndex, this.Positions);
        }
    }
}
