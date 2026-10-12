namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// The bytes of the record of a castle. The chunk's name is the name of the castle, empty for a record the map does not use.
    /// </summary>
    public sealed class Homm2CastleChunk : BytesChunk
    {
        public Homm2CastleChunk(string name, byte[] bytes, int startIndex)
            : base(name, bytes, startIndex)
        {
        }

        public override Homm2CastleChunk WithBytes(byte[] bytes)
        {
            return new Homm2CastleChunk(this.Name, this.CheckReplacement(bytes), this.StartIndex);
        }
    }
}
