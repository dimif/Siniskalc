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
        public static readonly PlayerColor Blue = new("Blue", 0);
        public static readonly PlayerColor Green = new("Green", 1);
        public static readonly PlayerColor Red = new("Red", 2);
        public static readonly PlayerColor Yellow = new("Yellow", 3);
        public static readonly PlayerColor Orange = new("Orange", 4);
        public static readonly PlayerColor Purple = new("Purple", 5);
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
        public static PlayerColor FromByte(byte code) => FromCode<PlayerColor>(code) ?? Neutral;
    }
}
