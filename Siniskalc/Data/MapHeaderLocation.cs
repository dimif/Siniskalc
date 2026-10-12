namespace Siniskalc.Data
{
    /// <summary>
    /// Where the fields of the map's header are in a savefile, as found by <see cref="MapHeaderLocator"/>.
    /// </summary>
    /// <param name="NameLengthAt">The offset of the 16-bit length of the map's name, which the name follows.</param>
    /// <param name="NameLength">The length of the name in bytes.</param>
    /// <param name="DescriptionLengthAt">The offset of the 16-bit length of the description, which the description follows.</param>
    /// <param name="DescriptionLength">The length of the description in bytes.</param>
    public sealed record MapHeaderLocation(int NameLengthAt, int NameLength, int DescriptionLengthAt, int DescriptionLength)
    {
        /// <summary>
        /// Gets the offset of the byte with the difficulty of the map, right after the description.
        /// </summary>
        public int DifficultyAt => this.DescriptionLengthAt + 2 + this.DescriptionLength;

        /// <summary>
        /// Gets the offset of the 32-bit width of the map, which comes before the name.
        /// </summary>
        public int SizeAt => this.NameLengthAt - MapHeaderLocator.SizeBeforeName;

        /// <summary>
        /// Gets the offset of the byte that says whether the map has an underground level, right in front of the name.
        /// </summary>
        public int UndergroundAt => this.NameLengthAt - 1;
    }
}
