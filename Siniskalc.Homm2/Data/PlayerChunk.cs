namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// The bytes of the record of a player. The chunk's name is the name of the colour.
    /// </summary>
    public sealed class PlayerChunk : BytesChunk
    {
        public PlayerChunk(string name, byte[] bytes, int startIndex)
            : base(name, bytes, startIndex)
        {
        }

        public override PlayerChunk WithBytes(byte[] bytes)
        {
            return new PlayerChunk(this.Name, this.CheckReplacement(bytes), this.StartIndex);
        }
    }
}
