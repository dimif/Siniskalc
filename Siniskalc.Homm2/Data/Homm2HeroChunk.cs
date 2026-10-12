namespace Siniskalc.Homm2.Data
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// The bytes of the record of a hero, from its header to its end. The chunk's name is the name of the hero.
    /// </summary>
    public sealed class Homm2HeroChunk : BytesChunk
    {
        public Homm2HeroChunk(string name, byte[] bytes, int startIndex)
            : base(name, bytes, startIndex)
        {
        }

        public override Homm2HeroChunk WithBytes(byte[] bytes)
        {
            return new Homm2HeroChunk(this.Name, this.CheckReplacement(bytes), this.StartIndex);
        }
    }
}
