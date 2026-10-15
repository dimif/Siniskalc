namespace Siniskalc.Homm2.Data
{
    /// <summary>
    /// The words the differs of the three kinds of record share.
    /// </summary>
    public static class DifferText
    {
        public const string Empty = "empty";

        public const string None = "none";

        /// <summary>
        /// Gives the number of a hero or a castle (or <see cref="None"/>).
        /// </summary>
        public static string Number(int? number) => number?.ToString() ?? None;
    }
}
