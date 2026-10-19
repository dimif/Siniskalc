namespace Siniskalc.Heroes.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The names of the owners a hero, a castle or a mine can have in a game: "Neutral" for nobody, and the colours of its players.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game.</typeparam>
    public static class OwnerNames<TColor>
        where TColor : AbstractEnumeration, IPlayerColor
    {
        /// <summary>
        /// The name of nobody.
        /// </summary>
        public const string Neutral = "Neutral";

        /// <summary>
        /// Gets the owners to pick from: "Neutral" and the player colours.
        /// </summary>
        public static IReadOnlyList<string> Choices { get; } = [Neutral, .. AbstractEnumeration.GetAll<TColor>().Select(c => c.Name)];

        /// <summary>
        /// Returns the name of an owner, "Neutral" for nobody.
        /// </summary>
        public static string Of(TColor? owner) => owner?.Name ?? Neutral;

        /// <summary>
        /// Returns the colour of a name, or <c>null</c> for "Neutral" and for a name that is no colour.
        /// </summary>
        public static TColor? Parse(string? name) => name is null ? null : AbstractEnumeration.FromName<TColor>(name);
    }
}
