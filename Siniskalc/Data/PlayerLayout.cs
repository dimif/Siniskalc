namespace Siniskalc.Data
{
    using Siniskalc.Entities;

    /// <summary>
    /// Where the fields of a player are in the savefile.
    /// </summary>
    /// <remarks>
    /// The savefile holds one fixed-size struct per player, one after the other, in colour order (a struct for every colour, in play or
    /// not). A struct starts <see cref="HeadLength"/> bytes before its first resource, with the heroes of the player: how many, which
    /// one is selected, their ids, the two heroes in the tavern, how many towns the player has and which one is selected. The resources
    /// and the rest follow. A player chunk is the whole struct, so it starts there.
    /// The positions were found by comparing saves of the same game, in which resources had been traded between players, and the
    /// head by comparing saves in which heroes were hired, garrisoned and defeated. The head is laid out the same in Shadow of Death and
    /// Horn of the Abyss savefiles for every colour (checked on every savefile in the repository). The struct size and the position
    /// of the colour differ between versions, so the layouts are only known for the versions that have been compared.
    /// </remarks>
    public sealed class PlayerLayout
    {
        /// <summary>
        /// How many bytes of the player's struct come before its first resource.
        /// </summary>
        public const int HeadLength = 97;

        /// <summary>
        /// Where the number of heroes of the player is: the ids of that many heroes follow at <see cref="HeroIds"/>.
        /// </summary>
        public const int HeroCount = 0;

        /// <summary>
        /// Where the id of the hero that is selected on the adventure map is, FF when none is.
        /// </summary>
        public const int SelectedHero = 1;

        /// <summary>
        /// Where the ids of the heroes of the player start: <see cref="HeroIdPlaces"/> bytes, FF for a place that is empty.
        /// A hero that is garrisoned in a castle is not in the list.
        /// </summary>
        public const int HeroIds = 2;

        public const int HeroIdPlaces = 8;

        /// <summary>
        /// Where the ids of the two heroes the tavern offers are, one byte each, FF when the tavern has none.
        /// </summary>
        public const int TavernHeroes = 10;

        public const int TavernHeroPlaces = 2;

        /// <summary>
        /// Where the number of towns of the player is; the index of the selected town follows (FF when none is).
        /// </summary>
        public const int TownCount = 23;

        public const int SelectedTown = 24;

        /// <summary>
        /// Resources are consecutive 32-bit integers in the order of <see cref="Resource"/>.
        /// </summary>
        public const int ResourcesOffset = HeadLength;

        public const int ResourceSize = 4;

        /// <summary>
        /// The layout of Horn of the Abyss savefiles. After the resources comes a 1 and a 16, then three zero bytes.
        /// </summary>
        public static readonly PlayerLayout HornOfTheAbyss = new(stride: 149, markerOffset: ResourcesOffset + 44, marker: [0x01, 0x10, 0x00, 0x00, 0x00], colorOffset: ResourcesOffset + 51);

        /// <summary>
        /// The layout of Shadow of Death savefiles. After the resources comes a 1 and two zero bytes.
        /// </summary>
        public static readonly PlayerLayout ShadowOfDeath = new(stride: 145, markerOffset: ResourcesOffset + 44, marker: [0x01, 0x00, 0x00], colorOffset: ResourcesOffset + 47);

        private PlayerLayout(int stride, int markerOffset, byte[] marker, int colorOffset)
        {
            this.Stride = stride;
            this.MarkerOffset = markerOffset;
            this.Marker = marker;
            this.ColorOffset = colorOffset;
        }

        /// <summary>
        /// Gets the distance between the same field of two consecutive players, which is also the size of a player chunk.
        /// </summary>
        public int Stride { get; }

        /// <summary>
        /// Gets where the marker that follows the resources starts, from the start of the chunk.
        /// It is what <see cref="PlayerBytesLoader"/> looks for to find the table of players.
        /// </summary>
        public int MarkerOffset { get; }

        public byte[] Marker { get; }

        /// <summary>
        /// Gets where the colour of the player plus one is, from the start of the chunk, so that the first colour is 1.
        /// </summary>
        public int ColorOffset { get; }

        /// <summary>
        /// Returns the layout of the players of <paramref name="gameVersion"/>, or null if it is not known.
        /// </summary>
        public static PlayerLayout? For(GameVersion gameVersion) => gameVersion switch
        {
            GameVersion.HornOfTheAbyss => HornOfTheAbyss,
            GameVersion.ShadowOfDeath => ShadowOfDeath,

            // Assumed to match Shadow of Death until savefiles of these versions have been compared.
            GameVersion.RestorationOfErathia => ShadowOfDeath,
            GameVersion.ArmageddonsBlade => ShadowOfDeath,
            _ => null,
        };

        /// <summary>
        /// Returns the colour of the player whose colour byte is <paramref name="colorByte"/>, which is the colour plus one.
        /// </summary>
        public static PlayerColor ColorFromByte(byte colorByte) => (PlayerColor)(colorByte - 1);

        public static int OffsetOf(Resource resource) => ResourcesOffset + ((int)resource * ResourceSize);
    }
}
