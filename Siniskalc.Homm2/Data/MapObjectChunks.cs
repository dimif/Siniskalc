namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// The chunks of the objects with state of a map: its mines, shrines and sphinxes.
    /// </summary>
    public sealed record MapObjectChunks(IReadOnlyList<MapObjectChunk> Mines, IReadOnlyList<MapObjectChunk> Shrines, IReadOnlyList<MapObjectChunk> Sphinxes)
    {
        /// <summary>
        /// Gets the chunks of a map in which none were found.
        /// </summary>
        public static MapObjectChunks None { get; } = new([], [], []);
    }
}
