namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Homm3.Entities;

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

        /// <summary>
        /// The number of places in the list of heroes of a player.
        /// </summary>
        public const int HeroIdPlaces = Constants.Player.HeroListSize;

        /// <summary>
        /// Where the ids of the two heroes the tavern offers are, one byte each, FF when the tavern has none.
        /// </summary>
        public const int TavernHeroes = 10;

        /// <summary>
        /// The number of heroes the tavern offers.
        /// </summary>
        public const int TavernHeroPlaces = 2;

        /// <summary>
        /// Where the number of towns of the player is; the index of the selected town follows (FF when none is).
        /// </summary>
        public const int TownCount = 23;

        /// <summary>
        /// Where the index of the town that is selected on the adventure map is, FF when none is.
        /// </summary>
        public const int SelectedTown = 24;

        /// <summary>
        /// Where the days left before the player is banished for owning no town are: FF while the player owns a town, 7 the day the last town
        /// is lost, one less each day, and 0 once the player is out of the game. Found by comparing saves of a player who lost his last town,
        /// regained one, lost it again and was banished (<c>TestData/Hota/Banishing</c>).
        /// </summary>
        public const int DaysLeft = 22;

        /// <summary>
        /// Resources are consecutive 32-bit integers in the order of <see cref="Resource"/>.
        /// </summary>
        public const int ResourcesOffset = HeadLength;

        /// <summary>
        /// The size of a resource in the savefile.
        /// </summary>
        public const int ResourceSize = 4;

        /// <summary>
        /// Where the flag byte that follows the resources by 44 bytes is. It is 1 in most players and 0 in some (Blue of the saves of
        /// <c>TestData/Hota/Banishing</c> and of <c>TestData/Hota/Mines</c>, never Red's); what makes it 0 is not known, so the Horn of the Abyss marker starts after it (Shadow of Death saves with it 0 were not seen, and its marker includes it).
        /// </summary>
        public const int FlagOffset = ResourcesOffset + 44;

        /// <summary>
        /// The layout of Horn of the Abyss savefiles. After the resources comes a flag byte (<see cref="FlagOffset"/>), then a 16 and three zero bytes.
        /// </summary>
        public static readonly PlayerLayout HornOfTheAbyss = new(stride: 149, markerOffset: FlagOffset + 1, marker: [0x10, 0x00, 0x00, 0x00], colorOffset: ResourcesOffset + 51);

        /// <summary>
        /// The layout of Shadow of Death savefiles. After the resources comes a 1 and two zero bytes (the flag byte of <see cref="FlagOffset"/> is part of the marker here: the Horn of the Abyss saves are the only ones seen with it 0).
        /// </summary>
        public static readonly PlayerLayout ShadowOfDeath = new(stride: 145, markerOffset: FlagOffset, marker: [0x01, 0x00, 0x00], colorOffset: ResourcesOffset + 47);

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

        /// <summary>
        /// Gets the bytes that follow the resources of every player; see <see cref="MarkerOffset"/>.
        /// </summary>
        public byte[] Marker { get; }

        /// <summary>
        /// Gets where the colour of the player plus one is, from the start of the chunk, so that the first colour is 1.
        /// </summary>
        public int ColorOffset { get; }

        /// <summary>
        /// Returns the layout of the players of <paramref name="gameVersion"/>, or null if it is not known.
        /// </summary>
        /// <param name="gameVersion">The version of the game.</param>
        /// <returns>The layout, or <see langword="null"/> if it is not known.</returns>
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
        /// <param name="colorByte">The byte, which is the colour plus one.</param>
        /// <returns>The colour.</returns>
        public static PlayerColor ColorFromByte(byte colorByte) => PlayerColor.FromByte((byte)(colorByte - 1)) ?? PlayerColor.All[0];

        /// <summary>
        /// Gets where a resource is in the chunk of a player.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns>The offset from the start of the chunk.</returns>
        public static int OffsetOf(Resource resource) => ResourcesOffset + ((int)resource * ResourceSize);
    }
}
