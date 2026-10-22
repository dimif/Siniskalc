namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// The bytes of the list of artifacts a merchant has in store (see <see cref="MerchantLayout"/>). The chunk's name is the merchant's.
    /// </summary>
    public sealed class MerchantStockChunk : BytesChunk
    {
        public MerchantStockChunk(string name, byte[] bytes, int startIndex, int index)
            : base(name, bytes, startIndex)
        {
            this.Index = index;
        }

        /// <summary>
        /// Gets the place of the merchant among the merchants of the savegame: 0 the artifact merchants, 1 the black market.
        /// </summary>
        public int Index { get; }

        public override MerchantStockChunk WithBytes(byte[] bytes)
        {
            return new MerchantStockChunk(this.Name, this.CheckReplacement(bytes), this.StartIndex, this.Index);
        }
    }
}
