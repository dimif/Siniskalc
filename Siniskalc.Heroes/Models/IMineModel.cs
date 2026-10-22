namespace Siniskalc.Heroes.Models
{
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// What a mine of the map has in any game of the series: its place among the mines, what it gives, who owns it and where it is. What only
    /// one game keeps of a mine (the guard of a HoMM3 mine) is on that game's model.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game, which is an enumeration of its own.</typeparam>
    public interface IMineModel<out TColor>
        where TColor : class, IPlayerColor
    {
        /// <summary>
        /// Gets the place of the mine among the mines of the savegame.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Gets what the mine gives.
        /// </summary>
        MineKind Kind { get; }

        /// <summary>
        /// Gets the player that owns the mine, or the colour <c>Neutral</c> for nobody.
        /// </summary>
        TColor Owner { get; }

        /// <summary>
        /// Gets the tile of the mine.
        /// </summary>
        MapPosition Position { get; }

        /// <summary>
        /// Gets the name of the mine with its place, as the pending changes name it: "Gold mine at X: 12, Y: 7", with the level in a game whose map
        /// has two ("Gold mine at X: 12, Y: 7, Underground (1)"); see <see cref="MineKinds.NameAt"/>.
        /// </summary>
        string Name { get; }
    }
}
