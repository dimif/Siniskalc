namespace Siniskalc.Heroes.Models
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// What the savefile says about the map it is played on.
    /// </summary>
    /// <param name="Name">The map's name.</param>
    /// <param name="Description">The map's description.</param>
    /// <param name="Difficulty">The difficulty the map was made with, which is not the difficulty the game was started at.</param>
    /// <param name="Size">The width of the map in tiles (it is square), or 0 when the savefile has a width that is not a standard size.</param>
    /// <param name="HasUnderground">Whether the map has an underground level; a game that has none keeps it false.</param>
    public sealed record MapHeader(string Name, string Description, GameDifficulty Difficulty, int Size = 0, bool HasUnderground = false)
    {
        /// <summary>
        /// Gets the size as the map editor names it, e.g. "Small (36 x 36)", or nothing when the size is not a standard one.
        /// </summary>
        public string SizeText => MapSizes.Describe(this.Size);
    }
}
