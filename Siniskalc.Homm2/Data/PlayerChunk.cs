namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// The bytes of the record of a player. The chunk's name is the name of the colour.
    /// </summary>
    public sealed class PlayerChunk : BytesChunk
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerChunk"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="bytes">The bytes.</param>
        /// <param name="startIndex">The start index.</param>
        public PlayerChunk(string name, byte[] bytes, int startIndex)
            : base(name, bytes, startIndex)
        {
        }

        /// <inheritdoc/>
        public override PlayerChunk WithBytes(byte[] bytes)
        {
            return new PlayerChunk(this.Name, this.CheckReplacement(bytes), this.StartIndex);
        }
    }
}
