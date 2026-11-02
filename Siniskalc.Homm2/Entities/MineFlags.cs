namespace Siniskalc.Homm2.Entities
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The flag a mine shows on the map of Heroes of Might and Magic II.
    /// </summary>
    public static class MineFlags
    {
        /// <summary>
        /// Gets the first picture of the flags of a kind of mine: the picture of the flag of a colour is this plus the number of the colour.
        /// </summary>
        /// <param name="kind">The kind of mine.</param>
        /// <returns>The picture.</returns>
        public static int PictureOf(MineKind kind) => kind switch
        {
            MineKind.Sawmill => 28,
            MineKind.AlchemistLab => 21,
            _ => 14,
        };
    }
}
