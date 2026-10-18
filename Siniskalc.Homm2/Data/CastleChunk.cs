namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// The bytes of the record of a castle. The chunk's name is the name of the castle, empty for a record the map does not use.
    /// </summary>
    public sealed class CastleChunk : BytesChunk
    {
        public CastleChunk(string name, byte[] bytes, int startIndex)
            : base(name, bytes, startIndex)
        {
        }

        public override CastleChunk WithBytes(byte[] bytes)
        {
            return new CastleChunk(this.Name, this.CheckReplacement(bytes), this.StartIndex);
        }
    }
}
