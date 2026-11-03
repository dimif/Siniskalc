namespace Siniskalc.Heroes.Models
{
    /// <summary>
    /// The words the descriptions of pending changes share, so the differs of every game and kind of record say the same things alike.
    /// </summary>
    public static class ChangeText
    {
        /// <summary>
        /// The text of a slot that holds nothing.
        /// </summary>
        public const string Empty = "empty";

        /// <summary>
        /// The text of a list or a place that holds nothing.
        /// </summary>
        public const string None = "none";

        /// <summary>
        /// Returns a flag as the words a change shows.
        /// </summary>
        /// <param name="value">The flag.</param>
        /// <returns>"yes" or "no".</returns>
        public static string YesNo(bool value) => value ? "yes" : "no";

        /// <summary>
        /// Gives the number of a hero or a castle (or <see cref="None"/>).
        /// </summary>
        /// <param name="number">The number, or <see langword="null"/> for none.</param>
        /// <returns>The number as text, or the text for none.</returns>
        public static string Number(int? number) => number?.ToString() ?? None;
    }
}
