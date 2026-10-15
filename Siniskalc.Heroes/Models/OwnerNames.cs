namespace Siniskalc.Heroes.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The words for who owns a hero or a castle: the name of a player colour, or "Neutral" when no player does.
    /// </summary>
    /// <typeparam name="TColor">The enumeration of the player colours of a game.</typeparam>
    public static class OwnerNames<TColor>
        where TColor : struct, Enum
    {
        public const string Neutral = "Neutral";

        /// <summary>
        /// Gets the owners to pick from: "Neutral" and the player colours.
        /// </summary>
        public static IReadOnlyList<string> Choices { get; } = [Neutral, .. Enum.GetValues<TColor>().Select(c => c.ToString())];

        public static string Of(TColor? owner) => owner?.ToString() ?? Neutral;

        /// <summary>
        /// Returns the colour called <paramref name="name"/>, or null (no player) for "Neutral" and for anything that is no colour.
        /// </summary>
        public static TColor? Parse(string? name) => Enum.TryParse<TColor>(name, out var color) && Enum.IsDefined(color) ? color : null;
    }
}
