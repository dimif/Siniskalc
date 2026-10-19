namespace Siniskalc.Homm3.Entities
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The colours of the players of a game.
    /// </summary>
    public sealed class PlayerColor : Homm3Enumeration, IPlayerColor
    {
        public static readonly PlayerColor Red = new("Red", 0);
        public static readonly PlayerColor Blue = new("Blue", 1);
        public static readonly PlayerColor Tan = new("Tan", 2);
        public static readonly PlayerColor Green = new("Green", 3);
        public static readonly PlayerColor Orange = new("Orange", 4);
        public static readonly PlayerColor Purple = new("Purple", 5);
        public static readonly PlayerColor Teal = new("Teal", 6);
        public static readonly PlayerColor Pink = new("Pink", 7);

        private PlayerColor(string name, byte code)
            : base(name, code)
        {
        }

        public static IReadOnlyList<PlayerColor> All => GetAll<PlayerColor>();

        public static PlayerColor? FromByte(byte code) => FromCode<PlayerColor>(code);
    }
}
