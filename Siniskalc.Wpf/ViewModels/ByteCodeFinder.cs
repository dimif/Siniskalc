namespace Siniskalc.Wpf.ViewModels
{
    using System.Collections.Generic;
    using System.Windows;

    /// <summary>
    /// What a finder that lists a catalog with the byte code of each entry has in common: the Code column and the words of the search hint.
    /// </summary>
    public static class ByteCodeFinder
    {
        /// <summary>
        /// The end of the search hint of a finder that also finds rows by their byte code.
        /// </summary>
        public const string HintSuffix = ", or a byte code (2A, 0x2A or 42)";

        /// <summary>
        /// Makes the Code column, which shows the byte of a row in hex.
        /// </summary>
        /// <param name="path">The property of a row that holds the code text.</param>
        /// <param name="noun">What a row is, in the singular, for the tooltip: "creature".</param>
        /// <returns>The column.</returns>
        public static FinderColumn CodeColumn(string path, string noun) =>
            new("Code", path, new GridLength(56), FinderColumnKind.MutedText, $"The byte that stands for the {noun} in a savefile, in hex");

        /// <summary>
        /// Puts the Code column left of the other columns.
        /// </summary>
        /// <param name="columns">The columns of the finder without the code.</param>
        /// <param name="path">The property of a row that holds the code text.</param>
        /// <param name="noun">What a row is, in the singular, for the tooltip: "creature".</param>
        /// <returns>The columns with the code first.</returns>
        public static IReadOnlyList<FinderColumn> WithCodeColumn(IReadOnlyList<FinderColumn> columns, string path, string noun) =>
            [CodeColumn(path, noun), .. columns];

        /// <summary>
        /// Adds the words about the byte code to a search hint when the finder shows codes.
        /// </summary>
        /// <param name="hint">The hint without the code.</param>
        /// <param name="showsCodes">Whether the finder shows and finds by byte code.</param>
        /// <returns>The hint.</returns>
        public static string Hint(string hint, bool showsCodes) => showsCodes ? hint + HintSuffix : hint;
    }
}
