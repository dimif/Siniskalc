namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The colours of the players, in the order of the savegame (the order of its tables of players): 0 for Blue.
    /// </summary>
    public sealed class PlayerColor : Homm2Enumeration, IPlayerColor
    {
        /// <summary>
        /// Blue, number 0.
        /// </summary>
        public static readonly PlayerColor Blue = new("Blue", 0);

        /// <summary>
        /// Green, number 1.
        /// </summary>
        public static readonly PlayerColor Green = new("Green", 1);

        /// <summary>
        /// Red, number 2.
        /// </summary>
        public static readonly PlayerColor Red = new("Red", 2);

        /// <summary>
        /// Yellow, number 3.
        /// </summary>
        public static readonly PlayerColor Yellow = new("Yellow", 3);

        /// <summary>
        /// Orange, number 4.
        /// </summary>
        public static readonly PlayerColor Orange = new("Orange", 4);

        /// <summary>
        /// Purple, number 5.
        /// </summary>
        public static readonly PlayerColor Purple = new("Purple", 5);

        /// <summary>
        /// The owner of what no player has; its byte is 0xFF.
        /// </summary>
        public static readonly PlayerColor Neutral = new("Neutral", 255);

        private PlayerColor(string name, byte code)
            : base(name, code)
        {
        }

        /// <summary>
        /// Gets the colours of the players, which leaves out <see cref="Neutral"/>.
        /// </summary>
        public static IReadOnlyList<PlayerColor> All => GetAll<PlayerColor>().Where(c => c != Neutral).ToList();

        /// <inheritdoc/>
        public bool IsNeutral => this == Neutral;

        /// <summary>
        /// Returns the colour of a byte of a savefile; a byte that is no colour reads as <see cref="Neutral"/>.
        /// </summary>
        /// <param name="code">The byte.</param>
        /// <returns>The colour; the neutral colour when the byte is no colour.</returns>
        public static PlayerColor FromByte(byte code) => FromCode<PlayerColor>(code) ?? Neutral;
    }
}
