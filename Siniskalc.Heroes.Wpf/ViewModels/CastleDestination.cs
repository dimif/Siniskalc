namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using System.Linq;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// A castle a hero can be sent to, in any game: its name, the tile of its entrance and whether another hero stands there already.
    /// </summary>
    /// <param name="Name">The name of the castle.</param>
    /// <param name="Entrance">The tile in front of its entrance, which is where the hero stands to visit it.</param>
    /// <param name="PlaceText">Where the entrance is, as the lists write a place ("34, 2, Underground").</param>
    /// <param name="IsOccupied">Whether another hero stands on the entrance, so the hero cannot be sent there.</param>
    /// <param name="FactionName">The faction of the castle, such as "Knight", or nothing when it has none.</param>
    public sealed record CastleDestination(string Name, MapPosition Entrance, string PlaceText, bool IsOccupied, string FactionName = "")
    {
        /// <summary>
        /// Gets the line of the list for a screen reader: the name, the place and the faction, such as "Whiteshield  ·  30, 17  ·  Knight".
        /// </summary>
        public string Label => string.Join("  ·  ", new[] { this.Name, this.PlaceText, this.FactionName }.Where(t => t.Length > 0));

        /// <summary>
        /// Returns whether the name, the place or the faction of the castle contains <paramref name="text"/>, in any case. Any castle matches
        /// an empty text.
        /// </summary>
        public bool Matches(string text) =>
            text.Length == 0 || new[] { this.Name, this.PlaceText, this.FactionName }.Any(t => t.Contains(text, StringComparison.CurrentCultureIgnoreCase));

        /// <summary>
        /// Gets what the list says about the castle, or nothing when the hero can go there.
        /// </summary>
        public string ToolTip => this.IsOccupied ? "Another hero stands at the entrance of this castle" : "Move the hero to the entrance of this castle";
    }
}
