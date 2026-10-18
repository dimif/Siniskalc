namespace Siniskalc.Homm2.Data
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Which colour each record of the table of players is. The records are in the order the players play (the humans first), and each
    /// says its colour; a table whose records do not name the six colours is taken to be in the order of the colours.
    /// </summary>
    public static class PlayerRecordOrder
    {
        /// <summary>
        /// Returns the colour of each record, as the number of the colour.
        /// </summary>
        // TODO:LOG warning: the records of the players do not name the six colours, so they were taken in the order of the colours.
        public static IReadOnlyList<int> ColorsOf(byte[] bytes, SaveLayout layout)
        {
            var colors = Enumerable.Range(0, Constants.Table.PlayerCount).Select(record => (int)bytes[layout.PlayerStart(record) + PlayerLayout.Color]).ToList();
            var named = colors.Distinct().Count() == Constants.Table.PlayerCount && colors.All(c => c < Constants.Table.PlayerCount);
            return named ? colors : Enumerable.Range(0, Constants.Table.PlayerCount).ToList();
        }

        /// <summary>
        /// Packs the colours of the records into a number, three bits for each, the first record in the lowest bits.
        /// </summary>
        public static int Pack(IReadOnlyList<int> colors) => colors.Select((color, record) => color << (3 * record)).Sum();

        /// <summary>
        /// Gives the colour of a record from a number made by <see cref="Pack"/>.
        /// </summary>
        public static int Unpack(int packed, int record) => (packed >> (3 * record)) & 7;
    }
}
