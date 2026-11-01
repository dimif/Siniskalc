namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// An immutable slice of a savefile holding a single struct.
    /// The offsets of a game's layout are relative to the first byte of the chunk.
    /// A game's own chunk classes derive from it to add what is specific to their kind of struct, and must keep it when the bytes are replaced.
    /// </summary>
    /// <param name="name">The name of the entity the chunk models.</param>
    /// <param name="bytes">The bytes of the chunk. They are copied, so a later change to the array does not reach the chunk.</param>
    /// <param name="startIndex">The offset of the first byte of the chunk in the savefile.</param>
    /// <exception cref="NullReferenceException"><paramref name="bytes"/> is <see langword="null"/>.</exception>
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

        /// <summary>
        /// Gets the number of bytes of this chunk.
        /// </summary>
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
        /// <returns>A new array with the bytes of the chunk; changing it does not change the chunk.</returns>
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
        /// <returns>A new chunk of the same kind with <paramref name="bytes"/>; this chunk is not changed.</returns>
        /// <exception cref="ArgumentException"><paramref name="bytes"/> does not have the length of the chunk (see <see cref="CheckReplacement"/>).</exception>
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
        /// <exception cref="NullReferenceException"><paramref name="other"/> is <see langword="null"/>.</exception>
        public virtual bool HasSameBytes(BytesChunk other)
        {
            return this.bytes.AsSpan().SequenceEqual(other.bytes);
        }

        /// <summary>
        /// Copies the chunk bytes to <paramref name="destination"/> at <see cref="StartIndex"/>.
        /// </summary>
        /// <param name="destination">The bytes of the savefile to copy into.</param>
        /// <exception cref="ArgumentNullException"><paramref name="destination"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="destination"/> is too short to hold the chunk at <see cref="StartIndex"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><see cref="StartIndex"/> is negative.</exception>
        public void CopyTo(byte[] destination)
        {
            Buffer.BlockCopy(this.bytes, 0, destination, this.StartIndex, this.bytes.Length);
        }

        /// <summary>
        /// Returns a copy of part of the chunk bytes.
        /// </summary>
        /// <param name="offset">The offset of the first byte, relative to the start of the chunk.</param>
        /// <param name="length">The number of bytes to copy.</param>
        /// <returns>A new array of <paramref name="length"/> bytes starting at <paramref name="offset"/>.</returns>
        /// <exception cref="OverflowException"><paramref name="length"/> is negative.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is negative.</exception>
        /// <exception cref="ArgumentException"><paramref name="offset"/> plus <paramref name="length"/> goes past the end of the chunk.</exception>
        public byte[] GetBytesAt(int offset, int length)
        {
            var result = new byte[length];
            Buffer.BlockCopy(this.bytes, offset, result, 0, length);
            return result;
        }

        /// <summary>
        /// Returns the byte at <paramref name="offset"/>.
        /// </summary>
        /// <param name="offset">The offset of the byte, relative to the start of the chunk.</param>
        /// <returns>The byte at <paramref name="offset"/>.</returns>
        /// <exception cref="IndexOutOfRangeException"><paramref name="offset"/> is negative or not less than <see cref="Length"/>.</exception>
        public byte GetByteAt(int offset)
        {
            return this.bytes[offset];
        }

        /// <summary>
        /// Reads a signed 16-bit integer stored in the machine's byte order (little-endian on the platforms the editor runs on).
        /// </summary>
        /// <param name="offset">The offset of the first of the 2 bytes, relative to the start of the chunk.</param>
        /// <returns>The value stored at <paramref name="offset"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is negative or not less than <see cref="Length"/>.</exception>
        /// <exception cref="ArgumentException">Fewer than 2 bytes are left from <paramref name="offset"/> to the end of the chunk.</exception>
        public short GetInt16At(int offset)
        {
            return BitConverter.ToInt16(this.bytes, offset);
        }

        /// <summary>
        /// Reads a signed 32-bit integer stored in the machine's byte order (little-endian on the platforms the editor runs on).
        /// </summary>
        /// <param name="offset">The offset of the first of the 4 bytes, relative to the start of the chunk.</param>
        /// <returns>The value stored at <paramref name="offset"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is negative or not less than <see cref="Length"/>.</exception>
        /// <exception cref="ArgumentException">Fewer than 4 bytes are left from <paramref name="offset"/> to the end of the chunk.</exception>
        public int GetInt32At(int offset)
        {
            return BitConverter.ToInt32(this.bytes, offset);
        }

        /// <summary>
        /// Reads an unsigned 16-bit integer stored in the machine's byte order (little-endian on the platforms the editor runs on).
        /// </summary>
        /// <param name="offset">The offset of the first of the 2 bytes, relative to the start of the chunk.</param>
        /// <returns>The value stored at <paramref name="offset"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is negative or not less than <see cref="Length"/>.</exception>
        /// <exception cref="ArgumentException">Fewer than 2 bytes are left from <paramref name="offset"/> to the end of the chunk.</exception>
        public ushort GetUInt16At(int offset)
        {
            return BitConverter.ToUInt16(this.bytes, offset);
        }

        /// <summary>
        /// Reads an unsigned 64-bit integer stored in the machine's byte order (little-endian on the platforms the editor runs on).
        /// </summary>
        /// <param name="offset">The offset of the first of the 8 bytes, relative to the start of the chunk.</param>
        /// <returns>The value stored at <paramref name="offset"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is negative or not less than <see cref="Length"/>.</exception>
        /// <exception cref="ArgumentException">Fewer than 8 bytes are left from <paramref name="offset"/> to the end of the chunk.</exception>
        public ulong GetUInt64At(int offset)
        {
            return BitConverter.ToUInt64(this.bytes, offset);
        }

        /// <summary>
        /// Reads an unsigned 32-bit integer stored in the machine's byte order (little-endian on the platforms the editor runs on).
        /// </summary>
        /// <param name="offset">The offset of the first of the 4 bytes, relative to the start of the chunk.</param>
        /// <returns>The value stored at <paramref name="offset"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is negative or not less than <see cref="Length"/>.</exception>
        /// <exception cref="ArgumentException">Fewer than 4 bytes are left from <paramref name="offset"/> to the end of the chunk.</exception>
        public uint GetUInt32At(int offset)
        {
            return BitConverter.ToUInt32(this.bytes, offset);
        }

        /// <summary>
        /// Returns <paramref name="bytes"/> after checking that it can replace the bytes of this chunk, for
        /// implementations of <see cref="WithBytes"/>.
        /// </summary>
        /// <param name="bytes">The bytes that are to replace the bytes of the chunk.</param>
        /// <returns><paramref name="bytes"/>, as it was given (not copied).</returns>
        /// <exception cref="ArgumentException"><paramref name="bytes"/> does not have the length of the chunk.</exception>
        /// <exception cref="NullReferenceException"><paramref name="bytes"/> is <see langword="null"/>.</exception>
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
