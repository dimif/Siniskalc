namespace Siniskalc.Heroes.Models
{
    /// <summary>
    /// The wording of the notice that a player cannot take one more hero or castle because its list is full, the same in every game.
    /// </summary>
    public static class FullListNotice
    {
        /// <summary>
        /// Returns the notice, such as "Blue has the most heroes a player can have (8).".
        /// </summary>
        /// <param name="colorName">The name of the colour of the player.</param>
        /// <param name="plural">What the list holds, in the plural, such as "heroes" or "castles".</param>
        /// <param name="limit">The most the list can hold.</param>
        /// <returns>The notice.</returns>
        public static string For(string colorName, string plural, int limit) =>
            $"{colorName} has the most {plural} a player can have ({limit}).";
    }
}
