namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Homm3.Entities;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// Where the table of who owns each hero is, and what its bytes say.
    /// </summary>
    /// <remarks>
    /// The table has a byte per hero id, in the order of the ids: the colour of the player who has the hero (0 Red to 7 Pink), FF for a hero
    /// nobody has, and 40 for a hero the game keeps from being drawn for a tavern: one a tavern offers (the twelve heroes of the taverns of the
    /// six players of <c>TestData/Hota/HeroOwners</c>) and the heroes of the campaigns (ids 145 to 155). A second table of as many bytes
    /// follows, FF in every savefile seen, and then <see cref="TailLength"/> bytes up to the date of the game, so the table is found from the
    /// date (<see cref="IGameDateLocator"/>). It has 156 bytes in Shadow of Death savefiles (the saves of <c>TestData/Roe</c> are among them:
    /// they are Restoration of Erathia maps saved by Shadow of Death) and 215 in Horn of the Abyss ones; that held for every savefile of
    /// <c>TestData</c>, where the table says the owner of every hero chunk, every hero of the list of a player has the colour of the player,
    /// and an owned hero missing from every list is garrisoned in a castle (<c>HeroOwnersBytesLoaderTests</c>). Restoration of Erathia,
    /// Armageddon's Blade and Chronicles savefiles were not seen, so it is not looked for in them.
    /// What the game changes in it: a defeated hero's byte becomes FF; a hired hero's 40 becomes the colour of the player, and the hero that
    /// takes its place in the tavern gets 40 (<c>hero_owners_00</c> to <c>01</c>, <c>02</c> to <c>03</c>). Hero 3 has 41 in every Horn of
    /// the Abyss save while nobody owns it and no tavern offers it: unexplained, so it reads as a hero kept out of play and is left alone.
    /// </remarks>
    public static class HeroOwnersLayout
    {
        /// <summary>
        /// The bytes between the end of the second table and the date: <c>00 08</c> (the same in every savefile seen, and checked), two
        /// 16-bit numbers that vary, a zero byte, and the four bytes of the marker of the date (<see cref="GameDateLocator"/>).
        /// </summary>
        public const int TailLength = 11;

        /// <summary>
        /// The first byte of the tail.
        /// </summary>
        public const byte TailFirst = 0x00;

        /// <summary>
        /// The second byte of the tail.
        /// </summary>
        public const byte TailSecond = 0x08;

        /// <summary>
        /// The byte of a hero nobody owns.
        /// </summary>
        public const byte Nobody = 0xFF;

        /// <summary>
        /// The bit of a hero the game keeps from being drawn for a tavern.
        /// </summary>
        public const byte ReservedBit = 0x40;

        /// <summary>
        /// The bits that no byte of the table had in any savefile seen, besides FF: all but <see cref="ReservedBit"/> and those of a colour (0 to 7).
        /// </summary>
        public const byte UnseenBits = 0xB8;

        /// <summary>
        /// Returns how many heroes the table of <paramref name="gameVersion"/> has, or 0 when it is not known.
        /// </summary>
        /// <param name="gameVersion">The version of the game.</param>
        /// <returns>The length in bytes of the table of owners.</returns>
        public static int LengthFor(GameVersion gameVersion) => gameVersion switch
        {
            GameVersion.HornOfTheAbyss => 215,
            GameVersion.ShadowOfDeath => 156,
            _ => 0,
        };

        /// <summary>
        /// Returns whether <paramref name="value"/> is a byte the table was seen with: FF, a colour, 40, or 40 with a colour.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns><see langword="true"/> if the byte says that a player has seen the hero.</returns>
        public static bool IsSeen(byte value) => value == Nobody || (value & UnseenBits) == 0;

        /// <summary>
        /// Returns what a byte of the table says. A colour with the <see cref="ReservedBit"/> (the 41 of hero 3) reads as a hero nobody owns that
        /// is kept out of play; a byte that was never seen reads as a hero nobody owns.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The ownership.</returns>
        public static HeroOwnership Read(byte value)
        {
            if (value == Nobody || !IsSeen(value))
            {
                return HeroOwnership.Free;
            }

            return (value & ReservedBit) != 0 ? HeroOwnership.Reserved : new HeroOwnership(PlayerColor.FromByte(value), false);
        }

        /// <summary>
        /// Returns the byte of an entry. A hero that has an owner is written with the colour alone: a hired hero loses the
        /// <see cref="ReservedBit"/> in the game.
        /// </summary>
        /// <param name="ownership">The ownership.</param>
        /// <returns>The byte.</returns>
        public static byte ByteOf(HeroOwnership ownership) => !ownership.Owner.IsNeutral
            ? ownership.Owner.Code
            : ownership.IsReserved ? ReservedBit : Nobody;
    }
}
