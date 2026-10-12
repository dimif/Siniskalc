namespace Siniskalc.Wpf.Views
{
    /// <summary>
    /// How the hex editor and the hex viewer write a byte as text.
    /// </summary>
    internal static class HexFormat
    {
        public static char ToAsciiPreview(byte value)
        {
            return value >= 32 && value <= 126 ? (char)value : '.';
        }

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
