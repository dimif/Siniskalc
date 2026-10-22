namespace Siniskalc.Core.Data
{
    /// <summary>
    /// The bytes of one record of a list of records of the same kind (a mine, a dwelling, the stock of a merchant), and the place of the record
    /// in that list. A game whose record carries more than that (the tile of the record, bytes of it that sit elsewhere) derives from it.
    /// </summary>
    public class RecordChunk : BytesChunk
    {
        /// <param name="name">The name shown for the record.</param>
        /// <param name="bytes">The bytes of the record.</param>
        /// <param name="startIndex">The offset of the record in the savefile.</param>
        /// <param name="index">The place of the record among the records of its kind.</param>
        public RecordChunk(string name, byte[] bytes, int startIndex, int index)
            : base(name, bytes, startIndex)
        {
            this.Index = index;
        }

        /// <summary>
        /// Gets the place of the record among the records of its kind.
        /// </summary>
        public int Index { get; }

        /// <inheritdoc/>
        public override RecordChunk WithBytes(byte[] bytes)
        {
            return new RecordChunk(this.Name, this.CheckReplacement(bytes), this.StartIndex, this.Index);
        }
    }
}
