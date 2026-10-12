namespace Siniskalc.Models
{
    using Siniskalc.Entities;

    /// <summary>
    /// What the savefile says about the map it was made on.
    /// </summary>
    /// <param name="Name">The map's name.</param>
    /// <param name="Description">The map's description.</param>
    /// <param name="Difficulty">The difficulty the map was made with, which is not the difficulty the game was started at.</param>
    /// <param name="Size">The width of the map in tiles (it is square), or 0 when the savefile has a width that is not a standard size.</param>
    /// <param name="HasUnderground">Whether the map has an underground level.</param>
    public sealed record MapHeader(string Name, string Description, GameDifficulty Difficulty, int Size = 0, bool HasUnderground = false)
    {
        /// <summary>
        /// Gets the size as the map editor names it, e.g. "Small (36 x 36)", or nothing when the size is not a standard one.
        /// </summary>
        public string SizeText => this.Size switch
        {
            36 => "Small (36 x 36)",
            72 => "Medium (72 x 72)",
            108 => "Large (108 x 108)",
            144 => "Extra large (144 x 144)",
            180 => "Huge (180 x 180)",
            216 => "Extra huge (216 x 216)",
            252 => "Giant (252 x 252)",
            _ => this.Size > 0 ? $"{this.Size} x {this.Size}" : string.Empty,
        };
    }
}
