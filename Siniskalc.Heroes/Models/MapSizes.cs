namespace Siniskalc.Heroes.Models
{
    /// <summary>
    /// The names the games give the sizes of a map. A map is a square, so a size is its width in tiles.
    /// </summary>
    public static class MapSizes
    {
        /// <summary>
        /// Returns the size as the map editor names it, e.g. "Small (36 x 36)", just the squares ("40 x 40") when it is not a standard
        /// size, or nothing when <paramref name="size"/> is not a size.
        /// </summary>
        /// <param name="size">The width of the map in tiles.</param>
        /// <returns>The text.</returns>
        public static string Describe(int size) => size switch
        {
            36 => "Small (36 x 36)",
            72 => "Medium (72 x 72)",
            108 => "Large (108 x 108)",
            144 => "Extra large (144 x 144)",
            180 => "Huge (180 x 180)",
            216 => "Extra huge (216 x 216)",
            252 => "Giant (252 x 252)",
            _ => size > 0 ? $"{size} x {size}" : string.Empty,
        };
    }
}
