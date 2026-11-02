namespace Siniskalc.Heroes.ViewModels
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// The words for a place on the map: "at 59, 9, Surface" in a game that has two levels, "at 59, 9" in one that has one.
    /// </summary>
    public static class MapLevelText
    {
        /// <summary>
        /// Returns a place on the map such as "at 59, 9, Surface".
        /// </summary>
        /// <param name="x">The column.</param>
        /// <param name="y">The row.</param>
        /// <param name="level">The level of the map.</param>
        /// <param name="position">The position.</param>
        /// <param name="hasLevels">Whether the map has two levels.</param>
        /// <returns>The text.</returns>
        public static string Coordinates(int x, int y, int level) => $"{Coordinates(x, y)}, {Of(level)}";

        /// <summary>
        /// Returns a place on a map that has no levels, such as "at 59, 9".
        /// </summary>
        /// <param name="x">The column.</param>
        /// <param name="y">The row.</param>
        /// <param name="level">The level of the map.</param>
        /// <param name="position">The position.</param>
        /// <param name="hasLevels">Whether the map has two levels.</param>
        /// <returns>The text.</returns>
        public static string Coordinates(int x, int y) => $"at {x}, {y}";

        /// <summary>
        /// Returns a place on the map with its level in a game whose map has two ("at 59, 9, Surface"), and without it in a game whose
        /// map has one ("at 59, 9").
        /// </summary>
        /// <param name="position">The place.</param>
        /// <param name="hasLevels">Whether the map of the game has two levels.</param>
        /// <param name="x">The column.</param>
        /// <param name="y">The row.</param>
        /// <param name="level">The level of the map.</param>
        /// <returns>The text.</returns>
        public static string Coordinates(MapPosition position, bool hasLevels) =>
            hasLevels ? Coordinates(position.X, position.Y, position.Level) : Coordinates(position.X, position.Y);

        /// <summary>
        /// Returns a place for a list that is already about one kind of thing, such as "At 59, 9, Surface" in a game that has two levels
        /// and "At 59, 9" in one that has one.
        /// </summary>
        /// <param name="position">The place.</param>
        /// <param name="hasLevels">Whether the map of the game has two levels.</param>
        /// <returns>The text.</returns>
        public static string At(MapPosition position, bool hasLevels) => $"At {Place(position, hasLevels)}";

        /// <summary>
        /// Returns a place for a sentence or a name, such as "59, 9, Underground" ("59, 9" on a map with one level), to follow "at".
        /// </summary>
        /// <param name="position">The place.</param>
        /// <param name="hasLevels">Whether the map of the game has two levels.</param>
        /// <param name="x">The column.</param>
        /// <param name="y">The row.</param>
        /// <param name="level">The level of the map.</param>
        /// <returns>The text.</returns>
        public static string Place(MapPosition position, bool hasLevels) =>
            hasLevels ? Place(position.X, position.Y, position.Level) : Place(position.X, position.Y);

        /// <summary>
        /// Returns a place on a map with levels for a sentence or a name, such as "59, 9, Underground".
        /// </summary>
        /// <param name="x">The column.</param>
        /// <param name="y">The row.</param>
        /// <param name="level">The level of the map.</param>
        /// <param name="position">The position.</param>
        /// <param name="hasLevels">Whether the map has two levels.</param>
        /// <returns>The text.</returns>
        public static string Place(int x, int y, int level) => $"{Place(x, y)}, {LevelName(level)}";

        /// <summary>
        /// Returns a place on a map with one level for a sentence or a name, such as "59, 9".
        /// </summary>
        /// <param name="x">The column.</param>
        /// <param name="y">The row.</param>
        /// <param name="level">The level of the map.</param>
        /// <param name="position">The position.</param>
        /// <param name="hasLevels">Whether the map has two levels.</param>
        /// <returns>The text.</returns>
        public static string Place(int x, int y) => $"{x}, {y}";

        /// <summary>
        /// Returns the name of a level of the map, such as "Surface" or "Underground".
        /// </summary>
        /// <param name="level">The level.</param>
        /// <returns>The name of the level.</returns>
        public static string Of(int level) => LevelName(level);

        private static string LevelName(int level) => level switch
        {
            MapPosition.Above => "Surface",
            MapPosition.Underground => "Underground",
            -1 => "None",
            _ => $"Level {level}",
        };
    }
}
