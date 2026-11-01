namespace Siniskalc.Core.Data
{
    /// <summary>
    /// A run of savefile bytes that belongs to a struct but sits outside the range of its own chunk.
    /// </summary>
    public sealed class RawBytesChunk : BytesChunk
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RawBytesChunk"/> class.
        /// </summary>
        /// <param name="name">The name of the struct the bytes belong to.</param>
        /// <param name="bytes">The bytes. They are copied.</param>
        /// <param name="startIndex">The offset of the first byte in the savefile.</param>
        public RawBytesChunk(string name, byte[] bytes, int startIndex)
            : base(name, bytes, startIndex)
        {
        }

        /// <inheritdoc/>
        public override RawBytesChunk WithBytes(byte[] bytes)
        {
            return new RawBytesChunk(this.Name, this.CheckReplacement(bytes), this.StartIndex);
        }
    }
}
