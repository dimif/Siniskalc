namespace Siniskalc.Homm3.ViewModels
{
    using Siniskalc.Heroes.Models;
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// The words for a level of the map, with the number the savefile holds in brackets: "Surface (0)" and "Underground (1)".
    /// </summary>
    public static class MapLevelText
    {
        /// <summary>
        /// Returns a place on the map such as "X: 59, Y: 9, Surface (0)".
        /// </summary>
        public static string Coordinates(int x, int y, int level) => $"X: {x}, Y: {y}, {Of(level)}";

        public static string Of(int level) => level switch
        {
            MapPosition.Above => $"Surface ({MapPosition.Above})",
            MapPosition.Underground => $"Underground ({MapPosition.Underground})",
            -1 => "None",
            _ => $"Level {level}",
        };
    }
}
