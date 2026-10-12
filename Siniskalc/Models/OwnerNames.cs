namespace Siniskalc.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Entities;

    /// <summary>
    /// The words for who owns a hero or a castle: the name of a player colour, or "Neutral" when no player does.
    /// </summary>
    public static class OwnerNames
    {
        public const string Neutral = "Neutral";

        /// <summary>
        /// Gets the owners to pick from: "Neutral" and the player colours.
        /// </summary>
        public static IReadOnlyList<string> Choices { get; } = [Neutral, .. Enum.GetValues<PlayerColor>().Select(c => c.ToString())];

        public static string Of(PlayerColor? owner) => owner?.ToString() ?? Neutral;

        /// <summary>
        /// Returns the colour called <paramref name="name"/>, or null (no player) for "Neutral" and for anything that is no colour.
        /// </summary>
        public static PlayerColor? Parse(string? name) => Enum.TryParse<PlayerColor>(name, out var color) && Enum.IsDefined(color) ? color : null;
    }
}
