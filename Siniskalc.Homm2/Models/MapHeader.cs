namespace Siniskalc.Homm2.Models
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// What the savegame says about the map it is played on.
    /// </summary>
    /// <param name="Name">The name of the map.</param>
    /// <param name="Description">The description of the map.</param>
    /// <param name="Difficulty">The difficulty the map was made with, which is not the difficulty the game was started at.</param>
    /// <param name="Width">The width of the map in tiles (a map is square).</param>
    /// <param name="Height">The height of the map in tiles.</param>
    public sealed record MapHeader(string Name, string Description, GameDifficulty Difficulty, int Width, int Height)
    {
        /// <summary>
        /// Gets the size as the game names it, e.g. "Small (36 x 36)", or the squares alone when it is not a standard size.
        /// </summary>
        public string SizeText => MapSizes.Describe(this.Width);
    }
}
