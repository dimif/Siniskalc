namespace Siniskalc.Heroes.Models
{
    /// <summary>
    /// The words the descriptions of pending changes share, so the differs of every game and kind of record say the same things alike.
    /// </summary>
    public static class ChangeText
    {
        public const string Empty = "empty";

        public const string None = "none";

        public static string YesNo(bool value) => value ? "yes" : "no";

        /// <summary>
        /// Gives the number of a hero or a castle (or <see cref="None"/>).
        /// </summary>
        public static string Number(int? number) => number?.ToString() ?? None;
    }
}
