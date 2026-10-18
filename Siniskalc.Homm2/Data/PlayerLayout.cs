namespace Siniskalc.Homm2.Data
{
    /// <summary>
    /// Where the fields of a player are in its record, from the start of the record. There is a record for each of the six colours,
    /// whether the colour is in the game or not.
    /// </summary>
    public static class PlayerLayout
    {
        public const int RecordSize = 0xCF;

        /// <summary>
        /// The colour of the player (Blue 0 to Purple 5). The records are not in the order of the colours: the humans come first, in
        /// the order they play (a game with Green and Orange human has Green, Orange, Blue, Red, Yellow, Purple) - found in the saves of
        /// <c>TestData/Homm2/HumanCpu</c>.
        /// </summary>
        public const int Color = 0x01;

        public const int HeroCount = 0x02;

        /// <summary>
        /// The hero the player has selected, or 0xFF.
        /// </summary>
        public const int SelectedHero = 0x03;

        /// <summary>
        /// The heroes the player has, five bytes of hero numbers, the empty ones 0xFF.
        /// </summary>
        public const int Heroes = 0x05;

        /// <summary>
        /// The two heroes the tavern offers, as hero numbers.
        /// </summary>
        public const int Tavern = 0x0D;

        /// <summary>
        /// The days the player has left to own a castle or a town, or 0xFF while he owns one.
        /// </summary>
        public const int DaysLeft = 0x44;

        public const int CastleCount = 0x45;

        /// <summary>
        /// The castle the player has selected, or 0xFF.
        /// </summary>
        public const int SelectedCastle = 0x46;

        /// <summary>
        /// The castles the player has, as castle numbers, the empty ones 0xFF.
        /// </summary>
        public const int Castles = 0x48;

        /// <summary>
        /// The stock of wood, mercury, ore, sulfur, crystal, gems and gold, a 32-bit number each.
        /// </summary>
        public const int Resources = 0x90;

        /// <summary>
        /// What the player earns each day of wood, mercury, ore, sulfur, crystal and gems, a 32-bit number each.
        /// </summary>
        public const int Income = 0xAC;

        /// <summary>
        /// What the player earns each day of gold (32 bits).
        /// </summary>
        public const int GoldIncome = 0xC4;
    }
}
