namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// The bytes of the record of a dwelling (see <see cref="DwellingLayout"/>).
    /// </summary>
    public sealed class DwellingChunk : BytesChunk
    {
        public DwellingChunk(string name, byte[] bytes, int startIndex, int index)
            : base(name, bytes, startIndex)
        {
            this.Index = index;
        }

        /// <summary>
        /// Gets the place of the dwelling among the dwellings of the savegame.
        /// </summary>
        public int Index { get; }

        public override DwellingChunk WithBytes(byte[] bytes)
        {
            return new DwellingChunk(this.Name, this.CheckReplacement(bytes), this.StartIndex, this.Index);
        }
    }
}
