namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An immutable slice of a savefile holding a single struct.
    /// The offsets of a game's layout are relative to the first byte of the chunk.
    /// A game's own chunk classes derive from it to add what is specific to their kind of struct, and must keep it when the bytes are replaced.
    /// </summary>
    public abstract class BytesChunk(string name, byte[] bytes, int startIndex)
    {
        private readonly byte[] bytes = (byte[])bytes.Clone();

        /// <summary>
        /// Gets the name of the entity modelled by this chunk.
        /// </summary>
        public string Name { get; } = name;

        /// <summary>
        /// Gets the offset of the first byte of this chunk in the savefile.
        /// </summary>
        public int StartIndex { get; } = startIndex;

        /// <summary>
        /// Gets the offset in the savefile just past the last byte of this chunk.
        /// </summary>
        public int End => this.StartIndex + this.bytes.Length;

        public int Length => this.bytes.Length;

        /// <summary>
        /// Gets what has to be written to the savefile for this chunk: the chunk itself, and any bytes of the same struct
        /// that sit outside its range.
        /// </summary>
        public virtual IEnumerable<BytesChunk> PatchParts
        {
            get { yield return this; }
        }

        /// <summary>
        /// Returns a copy of the chunk bytes.
        /// </summary>
        public byte[] ToArray()
        {
            return (byte[])this.bytes.Clone();
        }

        /// <summary>
        /// Returns a new chunk of the same kind, with the same name and location but the given bytes.
        /// The length must match, as the chunk occupies a fixed range of the savefile.
        /// </summary>
        /// <param name="bytes">
        /// The bytes to replace the chunk's bytes with.
        /// </param>
        public abstract BytesChunk WithBytes(byte[] bytes);

        /// <summary>
        /// Returns whether both chunks hold the same bytes.
        /// </summary>
        /// <param name="other">
        /// The other chunk to compare with.
        /// </param>
        /// <returns>
        /// <c>true</c> if the chunks have the same bytes, <c>false</c> otherwise.
        /// </returns>
        public virtual bool HasSameBytes(BytesChunk other)
        {
            return this.bytes.AsSpan().SequenceEqual(other.bytes);
        }

        /// <summary>
        /// Copies the chunk bytes to <paramref name="destination"/> at <see cref="StartIndex"/>.
        /// </summary>
        public void CopyTo(byte[] destination)
        {
            Buffer.BlockCopy(this.bytes, 0, destination, this.StartIndex, this.bytes.Length);
        }

        public byte[] GetBytesAt(int offset, int length)
        {
            var result = new byte[length];
            Buffer.BlockCopy(this.bytes, offset, result, 0, length);
            return result;
        }

        public byte GetByteAt(int offset)
        {
            return this.bytes[offset];
        }

        public short GetInt16At(int offset)
        {
            return BitConverter.ToInt16(this.bytes, offset);
        }

        public int GetInt32At(int offset)
        {
            return BitConverter.ToInt32(this.bytes, offset);
        }

        public ushort GetUInt16At(int offset)
        {
            return BitConverter.ToUInt16(this.bytes, offset);
        }

        public ulong GetUInt64At(int offset)
        {
            return BitConverter.ToUInt64(this.bytes, offset);
        }

        public uint GetUInt32At(int offset)
        {
            return BitConverter.ToUInt32(this.bytes, offset);
        }

        /// <summary>
        /// Returns <paramref name="bytes"/> after checking that it can replace the bytes of this chunk, for
        /// implementations of <see cref="WithBytes"/>.
        /// </summary>
        protected byte[] CheckReplacement(byte[] bytes)
        {
            if (bytes.Length != this.bytes.Length)
            {
                throw new ArgumentException(
                    $"Replacement bytes must have the same length as the chunk ({this.bytes.Length}), but were {bytes.Length}.",
                    nameof(bytes));
            }

            return bytes;
        }
    }
}
