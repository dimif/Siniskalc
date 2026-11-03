namespace Siniskalc.Wpf.Views
{
    /// <summary>
    /// How the hex editor and the hex viewer write a byte as text.
    /// </summary>
    internal static class HexFormat
    {
        /// <summary>
        /// Returns the character the ASCII column shows for a byte.
        /// </summary>
        /// <param name="value">The byte.</param>
        /// <returns>The character, or a stand-in for a byte that is not printable.</returns>
        public static char ToAsciiPreview(byte value)
        {
            return value >= 32 && value <= 126 ? (char)value : '.';
        }

        /// <summary>
        /// Returns the words that describe the character of a byte, for a tooltip.
        /// </summary>
        /// <param name="value">The byte.</param>
        /// <returns>The description.</returns>
        public static string DescribeChar(byte value)
        {
            return value switch
            {
                0 => "\\0",
                9 => "\\t",
                10 => "\\n",
                13 => "\\r",
                _ when value >= 32 && value <= 126 => ((char)value).ToString(),
                _ => ".",
            };
        }
    }
}
