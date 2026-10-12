namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Helpers for <see cref="IByteEditTarget"/>.
    /// </summary>
    public static class ByteEditTargetExtensions
    {
        /// <summary>
        /// Writes <paramref name="values"/> starting at <paramref name="start"/> as one undo step. Values that do not fit are dropped.
        /// </summary>
        public static bool WriteBytes(this IByteEditTarget target, int start, IReadOnlyList<byte> values)
        {
            if (start < 0 || values.Count == 0)
            {
                return false;
            }

            return target.Update(bytes =>
            {
                for (var i = 0; i < values.Count && start + i < bytes.Length; i++)
                {
                    bytes[start + i] = values[i];
                }

                return bytes;
            });
        }

        /// <summary>
        /// Returns the offsets of the bytes that differ between the two arrays. They must have the same length.
        /// </summary>
        public static IReadOnlyList<int> FindDifferences(byte[] first, byte[] second)
        {
            var result = new List<int>();
            for (var i = 0; i < Math.Min(first.Length, second.Length); i++)
            {
                if (first[i] != second[i])
                {
                    result.Add(i);
                }
            }

            return result;
        }
    }
}
