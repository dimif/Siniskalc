namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Linq;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// A place a hero can be sent to, in any game: a castle, a mine or a dwelling of its player. It has a name, the tile the hero is sent to and
    /// whether another hero stands there already.
    /// </summary>
    /// <param name="Name">The name of the place, such as the name of a castle or "Gold mine".</param>
    /// <param name="Kind">What it is: "Castle", "Mine" or "Dwelling".</param>
    /// <param name="Entrance">The tile the hero is moved to: the tile in front of the entrance of a castle, and the tile of the record of a mine or a dwelling.</param>
    /// <param name="PlaceText">Where the tile is, as the lists write a place ("34, 2, Underground").</param>
    /// <param name="IsOccupied">Whether another hero stands on the tile, so the hero cannot be sent there.</param>
    /// <param name="Detail">The faction of a castle, such as "Knight"; nothing for a mine or a dwelling.</param>
    /// <param name="IsApproximate">Whether the tile is only near the entrance: the record of a mine does not say where it is entered.</param>
    public sealed record MapDestination(string Name, string Kind, MapPosition Entrance, string PlaceText, bool IsOccupied, string Detail = "", bool IsApproximate = false)
    {
        /// <summary>
        /// Gets the line of the list for a screen reader: the name, the kind, the place and the faction, such as "Whiteshield  ·  Castle  ·  30, 17  ·  Knight".
        /// </summary>
        public string Label => string.Join("  ·  ", new[] { this.Name, this.Kind, this.PlaceText, this.Detail }.Where(t => t.Length > 0));

        /// <summary>
        /// Gets what the list says about the place when the pointer rests on it.
        /// </summary>
        public string ToolTip => this.IsOccupied
            ? "Another hero stands on this tile"
            : this.IsApproximate
                ? "Move the hero to the tile of this place. The savegame does not say where it is entered, so the tile can be a few steps off."
                : "Move the hero to the entrance of this place";

        /// <summary>
        /// Returns whether the name, the kind, the place or the faction of the place contains <paramref name="text"/>, in any case. Any place
        /// matches an empty text.
        /// </summary>
        /// <returns><see langword="true"/> if the place matches.</returns>
        /// <param name="text">The text to look for.</param>
        public bool Matches(string text) =>
            text.Length == 0 || new[] { this.Name, this.Kind, this.PlaceText, this.Detail }.Any(t => t.Contains(text, StringComparison.CurrentCultureIgnoreCase));
    }
}
