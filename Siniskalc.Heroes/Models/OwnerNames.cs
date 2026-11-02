namespace Siniskalc.Heroes.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The names of the owners a hero, a castle or a mine can have in a game: neutral for nobody, and the colours of its players.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game.</typeparam>
    public static class OwnerNames<TColor>
        where TColor : AbstractEnumeration, IPlayerColor
    {
        /// <summary>
        /// Gets the owner of what no player owns.
        /// </summary>
        public static TColor Neutral { get; } = AbstractEnumeration.GetAll<TColor>().Single(c => c.IsNeutral);

        /// <summary>
        /// Gets the owners to pick from: neutral first, then the player colours.
        /// </summary>
        public static IReadOnlyList<string> Choices { get; } = [Neutral.Name, .. AbstractEnumeration.GetAll<TColor>().Where(c => !c.IsNeutral).Select(c => c.Name)];

        /// <summary>
        /// Returns the name of an owner.
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <returns>The name.</returns>
        public static string Of(TColor owner) => owner.Name;

        /// <summary>
        /// Returns the colour of a name, or neutral for a name that is no colour.
        /// </summary>
        /// <param name="name">The name, or <see langword="null"/>.</param>
        /// <returns>The colour, or the neutral colour for no name or one that is none.</returns>
        public static TColor Parse(string? name) => name is null ? Neutral : AbstractEnumeration.FromName<TColor>(name) ?? Neutral;
    }
}
