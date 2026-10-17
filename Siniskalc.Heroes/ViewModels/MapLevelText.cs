namespace Siniskalc.Heroes.ViewModels
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// The words for a place on the map: "X: 59, Y: 9, Surface (0)" in a game that has two levels, "X: 59, Y: 9" in one that has one.
    /// </summary>
    public static class MapLevelText
    {
        /// <summary>
        /// Returns a place on the map such as "X: 59, Y: 9, Surface (0)".
        /// </summary>
        public static string Coordinates(int x, int y, int level) => $"{Coordinates(x, y)}, {Of(level)}";

        /// <summary>
        /// Returns a place on a map that has no levels, such as "X: 59, Y: 9".
        /// </summary>
        public static string Coordinates(int x, int y) => $"X: {x}, Y: {y}";

        public static string Of(int level) => level switch
        {
            MapPosition.Above => $"Surface ({MapPosition.Above})",
            MapPosition.Underground => $"Underground ({MapPosition.Underground})",
            -1 => "None",
            _ => $"Level {level}",
        };
    }
}
