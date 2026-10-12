namespace Siniskalc.Data
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Core;
    using Siniskalc.Core.Data;

    /// <summary>
    /// The chunk of a hero, named after the hero.
    /// </summary>
    public sealed class HeroChunk : BytesChunk
    {
        /// <summary>
        /// The size of the header of a hero struct: the position, and the bytes that lead up to the biography and the faction.
        /// </summary>
        public const int HeaderLength = 26;

        /// <summary>
        /// Where the 32-bit length of the biography is in the header, which ends with it.
        /// </summary>
        public const int BiographyLengthOffset = HeaderLength - 4;

        private const int PositionLength = 6;

        private readonly byte[]? outsideHeader;

        /// <param name="headerBytes">
        /// The <see cref="HeaderLength"/> bytes of the hero header for a hero with a biography, otherwise null.
        /// </param>
        /// <param name="headerIndex">The offset in the savefile of the first byte of <paramref name="headerBytes"/>.</param>
        public HeroChunk(string name, byte[] bytes, int startIndex, string? biography = null, byte[]? headerBytes = null, int headerIndex = -1)
            : base(name, bytes, startIndex)
        {
            if (headerBytes != null && headerBytes.Length != HeaderLength)
            {
                throw new ArgumentException($"The header must be {HeaderLength} bytes long.", nameof(headerBytes));
            }

            this.Biography = biography;
            this.outsideHeader = (byte[]?)headerBytes?.Clone();
            this.HeaderIndex = headerBytes == null ? startIndex : headerIndex;
        }

        /// <summary>
        /// Gets the biography stored in the savefile in front of the hero, or null if the hero has none.
        /// It is not part of the chunk bytes, and is never written back.
        /// </summary>
        public string? Biography { get; }

        /// <summary>
        /// Gets the offset in the savefile of the hero header. This is the start of the chunk, except for a hero with a
        /// biography: the biography sits between the header and the rest of the struct, so the header is in front of the chunk.
        /// </summary>
        public int HeaderIndex { get; }

        /// <summary>
        /// Gets the six bytes of the hero position (X, Y and Z as 16-bit integers) when they are not at the start of the
        /// chunk, which is the case for a hero with a biography. Null when the position is at the start of the chunk.
        /// </summary>
        public byte[]? OutsidePositionBytes => this.outsideHeader?[..PositionLength];

        /// <summary>
        /// Gets the chunk, and the part of the header that is in front of it.
        /// </summary>
        public override IEnumerable<BytesChunk> PatchParts
        {
            get
            {
                yield return this;

                if (this.outsideHeader == null)
                {
                    yield break;
                }

                // A short biography leaves the end of the header inside the chunk, which then covers it.
                var length = Math.Min(HeaderLength, this.StartIndex - this.HeaderIndex);
                if (length > 0)
                {
                    yield return new RawBytesChunk($"{this.Name} header", this.outsideHeader[..length], this.HeaderIndex);
                }
            }
        }

        private byte[] OutsideHeader => this.outsideHeader ?? throw new InvalidOperationException("The hero header is inside the chunk.");

        /// <summary>
        /// Gets the byte at <paramref name="offset"/> in the hero header. Reads the chunk when the byte is in it, so the
        /// result follows edits made to the chunk.
        /// </summary>
        public byte GetHeaderByteAt(int offset)
        {
            CheckHeaderOffset(offset);

            return this.GetChunkOffset(offset) is { } chunkOffset ? this.GetByteAt(chunkOffset) : this.OutsideHeader[offset];
        }

        /// <summary>
        /// Returns a chunk like this one, with the byte at <paramref name="offset"/> in the hero header replaced.
        /// </summary>
        public HeroChunk WithHeaderByte(int offset, byte value)
        {
            CheckHeaderOffset(offset);

            if (this.GetChunkOffset(offset) is { } chunkOffset)
            {
                var bytes = this.ToArray();
                bytes[chunkOffset] = value;
                return this.WithBytes(bytes);
            }

            var header = (byte[])this.OutsideHeader.Clone();
            header[offset] = value;
            return new HeroChunk(this.Name, this.ToArray(), this.StartIndex, this.Biography, header, this.HeaderIndex);
        }

        public override HeroChunk WithBytes(byte[] bytes)
        {
            return new HeroChunk(this.Name, this.CheckReplacement(bytes), this.StartIndex, this.Biography, this.outsideHeader, this.HeaderIndex);
        }

        public override bool HasSameBytes(BytesChunk other)
        {
            return base.HasSameBytes(other)
                && other is HeroChunk hero
                && (this.outsideHeader ?? []).AsSpan().SequenceEqual(hero.outsideHeader ?? []);
        }

        private static void CheckHeaderOffset(int offset)
        {
            RangeGuard.ThrowIfNotIndex(offset, HeaderLength, nameof(offset), "The offset is not in the hero header.");
        }

        private int? GetChunkOffset(int headerOffset)
        {
            var chunkOffset = this.HeaderIndex + headerOffset - this.StartIndex;
            return this.outsideHeader == null || (chunkOffset >= 0 && chunkOffset < this.Length) ? chunkOffset : null;
        }
    }
}
