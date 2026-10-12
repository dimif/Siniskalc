namespace Siniskalc.Models
{
    using Siniskalc.Entities;

    /// <summary>
    /// The few things about a hero that the navigation tree shows.
    /// </summary>
    /// <param name="Index">The hero's position in <see cref="SaveDocument.Heroes"/>.</param>
    /// <param name="HeroId">The hero's id: castles refer to a hero by it. See <see cref="HeroModel.HeroId"/>.</param>
    /// <param name="Name">The hero's name.</param>
    /// <param name="Owner">The player that owns the hero, or <c>null</c> if no player does.</param>
    /// <param name="HeroClass">The hero's class; one this editor does not know reads as Knight.</param>
    /// <param name="Level">The hero's level.</param>
    public sealed record HeroSummary(int Index, int HeroId, string Name, PlayerColor? Owner, HeroClass HeroClass, int Level)
    {
        /// <summary>
        /// Gets what to show under the name of the hero: its class and level.
        /// </summary>
        public string Description => $"{this.HeroClass.Name}, level {this.Level}";
    }
}
