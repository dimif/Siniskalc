namespace Siniskalc.Homm3.Entities
{
    using System.Collections.Generic;
    using System.Linq;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The colours of the players of a game mapped to their byte value in the savefiles.
    /// </summary>
    public sealed class PlayerColor : Homm3Enumeration, IPlayerColor
    {
        /// <summary>
        /// Red, number 0.
        /// </summary>
        public static readonly PlayerColor Red = new("Red", 0);

        /// <summary>
        /// Blue, number 1.
        /// </summary>
        public static readonly PlayerColor Blue = new("Blue", 1);

        /// <summary>
        /// Tan, number 2.
        /// </summary>
        public static readonly PlayerColor Tan = new("Tan", 2);

        /// <summary>
        /// Green, number 3.
        /// </summary>
        public static readonly PlayerColor Green = new("Green", 3);

        /// <summary>
        /// Orange, number 4.
        /// </summary>
        public static readonly PlayerColor Orange = new("Orange", 4);

        /// <summary>
        /// Purple, number 5.
        /// </summary>
        public static readonly PlayerColor Purple = new("Purple", 5);

        /// <summary>
        /// Teal, number 6.
        /// </summary>
        public static readonly PlayerColor Teal = new("Teal", 6);

        /// <summary>
        /// Pink, number 7.
        /// </summary>
        public static readonly PlayerColor Pink = new("Pink", 7);

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
