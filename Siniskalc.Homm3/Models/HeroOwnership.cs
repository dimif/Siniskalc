namespace Siniskalc.Homm3.Models
{
    using Siniskalc.Homm3.Entities;

    /// <summary>
    /// What the table of hero owners of the savefile says of one hero.
    /// </summary>
    /// <param name="Owner">The player who has the hero, or <see cref="PlayerColor.Neutral"/>.</param>
    /// <param name="IsReserved">
    /// Whether the game keeps the hero from being drawn for a tavern: a hero a tavern offers, or one the game keeps out of play. Only a hero
    /// nobody owns has it.
    /// </param>
    public sealed record HeroOwnership(PlayerColor Owner, bool IsReserved)
    {
        /// <summary>
        /// Gets the entry of a hero nobody owns that may be drawn for a tavern.
        /// </summary>
        public static HeroOwnership Free { get; } = new(PlayerColor.Neutral, false);

        /// <summary>
        /// Gets the entry of a hero nobody owns that the game keeps from being drawn for a tavern.
        /// </summary>
        public static HeroOwnership Reserved { get; } = new(PlayerColor.Neutral, true);
    }
}
