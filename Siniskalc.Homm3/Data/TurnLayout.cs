namespace Siniskalc.Homm3.Data
{
    /// <summary>
    /// Where the fields of the turn chunk are: the eight 32-bit human flags of the setup (1 for a human, in colour order), then the 32-bit
    /// colour whose turn it is (0 Red to 7 Pink). The flags are part of the chunk so that its serializer can refuse to give the turn to a
    /// computer player, which would make <see cref="PlayerControlLocator"/> miss the setup when the savefile is loaded again.
    /// </summary>
    public static class TurnLayout
    {
        /// <summary>
        /// The size of one human flag.
        /// </summary>
        public const int HumanFlagSize = PlayerControlLocator.HumanFlagSize;

        /// <summary>
        /// Where the colour whose turn it is starts, after the human flags.
        /// </summary>
        public const int CurrentPlayer = PlayerControlLocator.HumanFlagsLength;

        /// <summary>
        /// The size of the chunk: the human flags and the colour whose turn it is.
        /// </summary>
        public const int Length = CurrentPlayer + 4;
    }
}
