namespace Siniskalc.Heroes.ViewModels
{
    using System;
    using System.Collections.Generic;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The colours the editor uses for the players, by the name of the player's colour. They are the editor's own colours, chosen to read
    /// well on the screen, and not what a game draws or a savefile holds.
    /// </summary>
    public static class PlayerPalette
    {
        public const string NoPlayer = "#8A93A6";

        private static readonly IReadOnlyDictionary<string, string> ByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Red"] = "#C0392B",
            ["Blue"] = "#2E6FD0",
            ["Tan"] = "#B08D57",
            ["Green"] = "#3C8D40",
            ["Orange"] = "#E07B20",
            ["Purple"] = "#7B4BA8",
            ["Teal"] = "#1E9C90",
            ["Pink"] = "#D96AA7",
            ["Yellow"] = "#D4A91C",
        };

        /// <summary>
        /// Gets the names of the colours the palette has.
        /// </summary>
        public static IEnumerable<string> Names => ByName.Keys;

        /// <summary>
        /// Returns the colour of <paramref name="color"/>, or the colour of what nobody owns when it is <c>null</c> or unknown.
        /// </summary>
        public static string Of(IPlayerColor? color) => color != null && ByName.TryGetValue(color.Name, out var hex) ? hex : NoPlayer;
    }
}
