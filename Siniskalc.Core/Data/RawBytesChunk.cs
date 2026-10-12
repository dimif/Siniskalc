namespace Siniskalc.Core.Data
{
    /// <summary>
    /// A run of savefile bytes that belongs to a struct but sits outside the range of its own chunk.
    /// </summary>
    public sealed class RawBytesChunk : BytesChunk
    {
        public RawBytesChunk(string name, byte[] bytes, int startIndex)
            : base(name, bytes, startIndex)
        {
        }

        public override RawBytesChunk WithBytes(byte[] bytes)
        {
            return new RawBytesChunk(this.Name, this.CheckReplacement(bytes), this.StartIndex);
        }
    }
}
