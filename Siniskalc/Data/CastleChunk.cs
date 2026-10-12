namespace Siniskalc.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// The chunk of a castle (a town), named after the castle. Offsets from <see cref="CastleLayout"/>.
    /// </summary>
    public sealed class CastleChunk : BytesChunk
    {
        public CastleChunk(string name, byte[] bytes, int startIndex)
            : base(name, bytes, startIndex)
        {
        }

        /// <summary>
        /// Gets the offset in the chunk just past the name, from which the offsets <c>...FromNameEnd</c> of
        /// <see cref="CastleLayout"/> are measured.
        /// </summary>
        public int NameEnd => CastleLayout.Name + this.GetUInt16At(CastleLayout.NameLength);

        public override CastleChunk WithBytes(byte[] bytes)
        {
            return new CastleChunk(this.Name, this.CheckReplacement(bytes), this.StartIndex);
        }
    }
}
