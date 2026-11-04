namespace Siniskalc.Core
{
    using System;
    using System.Globalization;

    /// <summary>
    /// How the byte code of an entity is shown ("0x2A") and which search words find it.
    /// </summary>
    public static class ByteCodeText
    {
        /// <summary>
        /// Writes a byte code in hex, as the hex view of a savefile shows it.
        /// </summary>
        /// <param name="code">The byte code.</param>
        /// <returns>The code as "0x" and two hex digits.</returns>
        public static string Format(byte code) => "0x" + code.ToString("X2", CultureInfo.InvariantCulture);

        /// <summary>
        /// Tells whether a search word names a byte code: the hex form with or without "0x" ("2A", "0x2a"), or the decimal form ("42").
        /// The whole word has to be the code, so "4" does not find 0x42.
        /// </summary>
        /// <param name="code">The byte code.</param>
        /// <param name="word">One word of the search text.</param>
        /// <returns><see langword="true"/> if the word is the code in hex or in decimal.</returns>
        public static bool Matches(byte code, string word)
        {
            var hex = word.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? word[2..] : word;
            if (hex.Length > 0 && byte.TryParse(hex, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var asHex) && asHex == code)
            {
                return true;
            }

            return byte.TryParse(word, NumberStyles.None, CultureInfo.InvariantCulture, out var asDecimal) && asDecimal == code;
        }
    }
}
