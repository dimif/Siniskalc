namespace Siniskalc.Homm2.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Data;

    /// <summary>
    /// The bytes of the record of a castle. The chunk's name is the name of the castle, empty for a record the map does not use.
    /// </summary>
    public sealed class CastleChunk : BytesChunk
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CastleChunk"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="bytes">The bytes.</param>
        /// <param name="startIndex">The start index.</param>
        /// <param name="flags">The flags of the castle on the map (<see cref="CastleFlags"/>), none for a record the map does not use.</param>
        /// <param name="ownerMark">The byte of the table of the colour of each castle (<see cref="CastleFlags.OwnerTableStart"/>) that is this castle's, or <c>null</c> for a record the map does not use.</param>
        public CastleChunk(string name, byte[] bytes, int startIndex, IReadOnlyList<MapObjectPart>? flags = null, MapObjectPart? ownerMark = null)
            : base(name, bytes, startIndex)
        {
            this.Flags = flags ?? [];
            this.OwnerMark = ownerMark;
        }

        /// <summary>
        /// Gets the byte of the table that holds the colour of each castle (the minimap draws the castles from it), or <c>null</c>.
        /// </summary>
        public MapObjectPart? OwnerMark { get; }

        /// <summary>
        /// Gets the flags of the castle on the map, which show its owner.
        /// </summary>
        public IReadOnlyList<MapObjectPart> Flags { get; }

        /// <inheritdoc/>
        public override IEnumerable<BytesChunk> PatchParts
        {
            get
            {
                yield return this;
                foreach (var flag in this.Flags)
                {
                    yield return new RawBytesChunk(this.Name, flag.Bytes, flag.StartIndex);
                }

                if (this.OwnerMark != null)
                {
                    yield return new RawBytesChunk(this.Name, this.OwnerMark.Bytes, this.OwnerMark.StartIndex);
                }
            }
        }

        /// <inheritdoc/>
        public override CastleChunk WithBytes(byte[] bytes)
        {
            return new CastleChunk(this.Name, this.CheckReplacement(bytes), this.StartIndex, this.Flags, this.OwnerMark);
        }

        /// <summary>
        /// Returns the chunk with the class and picture of one flag replaced.
        /// </summary>
        /// <param name="index">The place of the flag among <see cref="Flags"/>.</param>
        /// <param name="bytes">The two bytes.</param>
        /// <returns>The new chunk; this one is not changed.</returns>
        public CastleChunk WithFlag(int index, byte[] bytes)
        {
            var flags = this.Flags.ToList();
            flags[index] = new MapObjectPart(flags[index].StartIndex, bytes);
            return new CastleChunk(this.Name, this.ToArray(), this.StartIndex, flags, this.OwnerMark);
        }

        /// <summary>
        /// Returns the chunk with the byte of its castle in the table of colours replaced.
        /// </summary>
        /// <param name="value">The colour byte, <c>FF</c> for nobody.</param>
        /// <returns>The new chunk; this one is not changed.</returns>
        public CastleChunk WithOwnerMark(byte value) =>
            this.OwnerMark is null ? this : new CastleChunk(this.Name, this.ToArray(), this.StartIndex, this.Flags, new MapObjectPart(this.OwnerMark.StartIndex, [value]));

        /// <inheritdoc/>
        public override bool HasSameBytes(BytesChunk other) =>
            base.HasSameBytes(other)
            && other is CastleChunk chunk
            && chunk.Flags.Count == this.Flags.Count
            && chunk.OwnerMark?.StartIndex == this.OwnerMark?.StartIndex
            && (chunk.OwnerMark is null || chunk.OwnerMark.Bytes.AsSpan().SequenceEqual(this.OwnerMark!.Bytes))
            && this.Flags.Zip(chunk.Flags).All(pair => pair.First.StartIndex == pair.Second.StartIndex && pair.First.Bytes.AsSpan().SequenceEqual(pair.Second.Bytes));
    }
}
