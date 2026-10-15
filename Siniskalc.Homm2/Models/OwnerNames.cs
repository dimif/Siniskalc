namespace Siniskalc.Homm2.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Homm2.Entities;

    /// <summary>
    /// The words for who owns a hero or a castle: the name of a player colour, or "Neutral" when no player does.
    /// </summary>
    public static class OwnerNames
    {
        public const string Neutral = "Neutral";

        /// <summary>
        /// Gets the owners to pick from: "Neutral" and the player colours.
        /// </summary>
        public static IReadOnlyList<string> Choices { get; } = [Neutral, .. Enum.GetValues<Color>().Select(c => c.ToString())];

        public static string Of(Color? owner) => owner?.ToString() ?? Neutral;

        /// <summary>
        /// Returns the colour called <paramref name="name"/>, or null (no player) for "Neutral" and for anything that is no colour.
        /// </summary>
        public static Color? Parse(string? name) => Enum.TryParse<Color>(name, out var color) && Enum.IsDefined(color) ? color : null;
    }
}
