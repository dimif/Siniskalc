namespace Siniskalc.Homm2.ViewModels
{
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// One line of the overview of a savegame: what a player has.
    /// </summary>
    public sealed record PlayerSummary(Color Color, long Gold, int Heroes, int Castles, string Status)
    {
        /// <summary>
        /// Gets the colour of the marker beside the name of the player, e.g. "#C0392B".
        /// </summary>
        public string ColorHex => Palette.Of(this.Color);

        public string Name => this.Color.ToString();

        /// <summary>
        /// Gets what the player has, such as "2 heroes · 1 town".
        /// </summary>
        public string Holdings => $"{this.Heroes} {(this.Heroes == 1 ? "hero" : "heroes")} · {this.Castles} {(this.Castles == 1 ? "town" : "towns")}";
    }
}
