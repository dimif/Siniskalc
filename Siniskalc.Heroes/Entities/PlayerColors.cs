namespace Siniskalc.Heroes.Entities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The colours a player of a game can have, as <see cref="IPlayerColor"/>.
    /// </summary>
    /// <typeparam name="TColor">The enumeration of the player colours of the game, whose values are the codes of the savegame.</typeparam>
    public static class PlayerColors<TColor>
        where TColor : struct, Enum
    {
        private static readonly IReadOnlyList<PlayerColorInfo> AllColors = Enum.GetValues<TColor>()
            .Select(c => new PlayerColorInfo(c.ToString(), Convert.ToByte(c)))
            .ToList();

        public static IReadOnlyList<IPlayerColor> All => AllColors;

        /// <summary>
        /// Returns <paramref name="color"/> as an <see cref="IPlayerColor"/>, or <c>null</c> when it is <c>null</c> (what nobody owns).
        /// </summary>
        public static IPlayerColor? Of(TColor? color) => color is { } value ? AllColors.First(c => c.Code == Convert.ToByte(value)) : null;
    }
}
