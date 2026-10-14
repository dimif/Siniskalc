namespace Siniskalc.Homm2.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The colours a player of Heroes of Might and Magic II can have, as <see cref="IPlayerColor"/>.
    /// </summary>
    public static class PlayerColors
    {
        private static readonly IReadOnlyList<PlayerColorInfo> AllColors = Enum.GetValues<Color>()
            .Select(c => new PlayerColorInfo(c.ToString(), (byte)c))
            .ToList();

        public static IReadOnlyList<IPlayerColor> All => AllColors;

        /// <summary>
        /// Returns <paramref name="color"/> as an <see cref="IPlayerColor"/>, or <c>null</c> when it is <c>null</c> (what nobody owns).
        /// </summary>
        public static IPlayerColor? Of(Color? color) => color is { } value ? AllColors.First(c => c.Code == (byte)value) : null;
    }
}
