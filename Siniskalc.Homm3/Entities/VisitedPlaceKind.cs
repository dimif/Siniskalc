namespace Siniskalc.Homm3.Entities
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A kind of place that gives a hero something once: the hero's record keeps a 32 bit number for the kind, with a bit set for each place of
    /// the kind the hero visited, the bit being the number of the place among the places of its kind. Found in the Horn of the Abyss saves of
    /// <c>TestData/Hota/VisitedPlaces</c>; the places of the older versions are expected to be the same and were not looked at.
    /// </summary>
    public sealed class VisitedPlaceKind
    {
        public static readonly VisitedPlaceKind LearningStone = new("Learning stones", 100, 79, "Learning stones: experience, once per hero");
        public static readonly VisitedPlaceKind MarlettoTower = new("Marletto towers", 23, 83, "A point of defense, once per hero");
        public static readonly VisitedPlaceKind GardenOfRevelation = new("Revelation gardens", 32, 87, "A point of knowledge, once per hero");
        public static readonly VisitedPlaceKind MercenaryCamp = new("Mercenary camps", 51, 91, "A point of attack, once per hero");
        public static readonly VisitedPlaceKind StarAxis = new("Star axes", 61, 95, "A point of spell power, once per hero");
        public static readonly VisitedPlaceKind TreeOfKnowledge = new("Trees of knowledge", 102, 99, "A level, once per hero, for a payment", numberMask: 0x1F);
        public static readonly VisitedPlaceKind LibraryOfEnlightenment = new("Libraries", 41, 103, "Libraries of enlightenment: two points of every primary skill for a hero of a high level, once per hero");
        public static readonly VisitedPlaceKind Arena = new("Arenas", 4, 107, "A point of attack or defense, once per hero");
        public static readonly VisitedPlaceKind SchoolOfMagic = new("Schools of magic", 47, 111, "A point of spell power or knowledge for a payment, once per hero");
        public static readonly VisitedPlaceKind SchoolOfWar = new("Schools of war", 107, 115, "A point of attack or defense for a payment, once per hero");
        public static readonly VisitedPlaceKind ColossusOfTheMagi = new("Colossi", null, 1098, "Colossi of the magi, Horn of the Abyss only: once per hero. The places are not listed: the savegame does not number them apart from the gazebos");
        public static readonly VisitedPlaceKind Gazebo = new("Gazebos", null, 1114, "Horn of the Abyss only: two thousand experience points, once per hero. The places are not listed: the savegame does not number them apart from the colossi");

        private VisitedPlaceKind(string name, int? objectType, int heroOffset, string toolTip, byte numberMask = 0xFF)
        {
            this.NumberMask = numberMask;
            this.Name = name;
            this.ObjectType = objectType;
            this.HeroOffset = heroOffset;
            this.ToolTip = toolTip;
        }

        /// <summary>
        /// Gets every kind, in the order of their place in the record of a hero (the two of Horn of the Abyss last).
        /// </summary>
        public static IReadOnlyList<VisitedPlaceKind> All { get; } =
        [
            LearningStone, MarlettoTower, GardenOfRevelation, MercenaryCamp, StarAxis, TreeOfKnowledge, LibraryOfEnlightenment, Arena, SchoolOfMagic, SchoolOfWar, ColossusOfTheMagi, Gazebo,
        ];

        public string Name { get; }

        /// <summary>
        /// Gets the number of the type of the map objects of the kind (what a hero standing on one reads at +11), or <c>null</c> when the
        /// places of the kind cannot be told from those of another kind.
        /// </summary>
        public int? ObjectType { get; }

        /// <summary>
        /// Gets where the 32 bit number of the kind is in the record of a hero.
        /// </summary>
        public int HeroOffset { get; }

        public string ToolTip { get; }

        /// <summary>
        /// Gets the bits of the number byte of a cell (and of the number a hero standing on a place reads) that are the number of the place. The tree of
        /// knowledge keeps flags in the others: its cell has 0 or 1 before a visit and 0x60 or 0x61 after one, and the hero standing on a tree reads 0x20 or 0x21.
        /// The three bytes after the number hold the payment of the tree (<c>20 05 00</c> at first, <c>2C 05 00</c> after a visit): it is the only kind whose
        /// cell does not end in zeros.
        /// </summary>
        public byte NumberMask { get; }

        /// <summary>
        /// Gets whether the cell of a place of the kind holds flags and a payment beside its number.
        /// </summary>
        public bool CellHasFlags => this.NumberMask != 0xFF;

        /// <summary>
        /// Gets whether the kind exists only in the Horn of the Abyss.
        /// </summary>
        public bool IsHotaOnly => this.HeroOffset > 1000;

        public int Index => All.ToList().IndexOf(this);
    }
}
