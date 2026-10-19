namespace Siniskalc.Homm2.Entities
{
    using System.Collections.Generic;
    using Siniskalc.Heroes.Entities;

    /// <summary>
    /// The colours of the players, in the order of the savegame (the order of its tables of players): 0 for Blue.
    /// </summary>
    public sealed class Color : Homm2Enumeration, IPlayerColor
    {
        public static readonly Color Blue = new("Blue", 0);
        public static readonly Color Green = new("Green", 1);
        public static readonly Color Red = new("Red", 2);
        public static readonly Color Yellow = new("Yellow", 3);
        public static readonly Color Orange = new("Orange", 4);
        public static readonly Color Purple = new("Purple", 5);

        private Color(string name, byte code)
            : base(name, code)
        {
        }

        public static IReadOnlyList<Color> All => GetAll<Color>();

        public static Color? FromByte(byte code) => FromCode<Color>(code);
    }
}
