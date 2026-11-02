namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// The bytes of the record of a hero, from its header to its end. The chunk's name is the name of the hero.
    /// </summary>
    public sealed class HeroChunk : BytesChunk
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HeroChunk"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="bytes">The bytes.</param>
        /// <param name="startIndex">The start index.</param>
        public HeroChunk(string name, byte[] bytes, int startIndex)
            : base(name, bytes, startIndex)
        {
        }

        /// <inheritdoc/>
        public override HeroChunk WithBytes(byte[] bytes)
        {
            return new HeroChunk(this.Name, this.CheckReplacement(bytes), this.StartIndex);
        }
    }
}
