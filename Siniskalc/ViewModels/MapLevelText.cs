namespace Siniskalc.ViewModels
{
    using Siniskalc.Entities;

    /// <summary>
    /// The words for a level of the map, with the number the savefile holds in brackets: "Surface (0)" and "Underground (1)".
    /// </summary>
    public static class MapLevelText
    {
        /// <summary>
        /// Returns a place on the map such as "X: 59, Y: 9, Surface (0)".
        /// </summary>
        public static string Coordinates(short x, short y, short level) => $"X: {x}, Y: {y}, {Of(level)}";

        public static string Of(short level) => level switch
        {
            Position.Above => $"Surface ({Position.Above})",
            Position.Underground => $"Underground ({Position.Underground})",
            -1 => "None",
            _ => $"Level {level}",
        };
    }
}
