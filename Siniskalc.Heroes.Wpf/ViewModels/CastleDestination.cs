namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// A castle a hero can be sent to, in any game: its name, the tile of its entrance and whether another hero stands there already.
    /// </summary>
    /// <param name="Name">The name of the castle.</param>
    /// <param name="Entrance">The tile in front of its entrance, which is where the hero stands to visit it.</param>
    /// <param name="PlaceText">Where the entrance is, as the lists write a place ("34, 2, Underground").</param>
    /// <param name="IsOccupied">Whether another hero stands on the entrance, so the hero cannot be sent there.</param>
    public sealed record CastleDestination(string Name, MapPosition Entrance, string PlaceText, bool IsOccupied)
    {
        /// <summary>
        /// Gets the line of the list: the name and the place, such as "Whiteshield  ·  30, 17".
        /// </summary>
        public string Label => $"{this.Name}  ·  {this.PlaceText}";

        /// <summary>
        /// Gets what the list says about the castle, or nothing when the hero can go there.
        /// </summary>
        public string ToolTip => this.IsOccupied ? "Another hero stands at the entrance of this castle" : "Move the hero to the entrance of this castle";
    }
}
