namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// The bytes of an object of the map (a mine, a shrine, a sphinx): the record or block that holds it, and the bytes that belong to
    /// the object but sit somewhere else (the flag of a mine, which is part of a tile). The position of the object is not part of the bytes.
    /// </summary>
    public sealed class MapObjectChunk : BytesChunk
    {
        public MapObjectChunk(string name, byte[] bytes, int startIndex, MapPosition? position, int index, int tag = 0, IReadOnlyList<MapObjectPart>? parts = null)
            : base(name, bytes, startIndex)
        {
            this.Position = position;
            this.Index = index;
            this.Tag = tag;
            this.Parts = parts ?? [];
        }

        /// <summary>
        /// Gets the tile of the object, when it was found.
        /// </summary>
        public MapPosition? Position { get; }

        /// <summary>
        /// Gets the place of the object in the list of the objects of its kind.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets a number the reader leaves for the deserializer: the circle of a shrine, the kind of a mine.
        /// </summary>
        public int Tag { get; }

        /// <summary>
        /// Gets the bytes of the object that are not in the chunk, each at a place of the savegame.
        /// </summary>
        public IReadOnlyList<MapObjectPart> Parts { get; }

        /// <inheritdoc/>
        public override IEnumerable<BytesChunk> PatchParts
        {
            get
            {
                yield return this;
                foreach (var part in this.Parts)
                {
                    yield return new RawBytesChunk(this.Name, part.Bytes, part.StartIndex);
                }
            }
        }

        public override MapObjectChunk WithBytes(byte[] bytes)
        {
            return new MapObjectChunk(this.Name, this.CheckReplacement(bytes), this.StartIndex, this.Position, this.Index, this.Tag, this.Parts);
        }

        /// <summary>
        /// Returns a chunk like this one with the bytes of one of its parts replaced.
        /// </summary>
        public MapObjectChunk WithPart(int index, byte[] bytes)
        {
            var parts = this.Parts.ToList();
            parts[index] = new MapObjectPart(parts[index].StartIndex, bytes);
            return new MapObjectChunk(this.Name, this.ToArray(), this.StartIndex, this.Position, this.Index, this.Tag, parts);
        }

        public override bool HasSameBytes(BytesChunk other) =>
            base.HasSameBytes(other)
            && other is MapObjectChunk chunk
            && chunk.Parts.Count == this.Parts.Count
            && this.Parts.Zip(chunk.Parts).All(pair => pair.First.StartIndex == pair.Second.StartIndex && pair.First.Bytes.AsSpan().SequenceEqual(pair.Second.Bytes));
    }
}
