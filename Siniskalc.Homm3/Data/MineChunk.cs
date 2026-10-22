namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// The bytes of the record of a mine (see <see cref="MineLayout"/>). The chunk's name says what and where it is.
    /// </summary>
    public sealed class MineChunk : BytesChunk
    {
        public MineChunk(string name, byte[] bytes, int startIndex, int index)
            : base(name, bytes, startIndex)
        {
            this.Index = index;
        }

        /// <summary>
        /// Gets the place of the mine among the mines of the savegame.
        /// </summary>
        public int Index { get; }

        public override MineChunk WithBytes(byte[] bytes)
        {
            return new MineChunk(this.Name, this.CheckReplacement(bytes), this.StartIndex, this.Index);
        }
    }
}
