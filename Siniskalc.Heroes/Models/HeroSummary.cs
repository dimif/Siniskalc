namespace Siniskalc.Heroes.Models
{
    /// <summary>
    /// The few things about a hero that the navigation tree and the lists of heroes show.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game.</typeparam>
    /// <param name="Index">The hero's position in the savefile's list of heroes.</param>
    /// <param name="HeroId">The hero's id: castles and players refer to a hero by it.</param>
    /// <param name="Name">The hero's name.</param>
    /// <param name="Owner">The player that owns the hero, or <c>null</c> if no player does.</param>
    /// <param name="Kind">What the hero is: its class in the games that have classes, its faction in the others.</param>
    /// <param name="Level">The hero's level.</param>
    public sealed record HeroSummary<TColor>(int Index, int HeroId, string Name, TColor? Owner, string Kind, int Level)
        where TColor : struct
    {
        /// <summary>
        /// Gets what to show under the name of the hero: its kind and level.
        /// </summary>
        public string Description => $"{this.Kind}, Level {this.Level}";
    }
}
