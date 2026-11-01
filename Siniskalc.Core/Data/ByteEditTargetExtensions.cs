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
        /// <param name="target">The bytes to change.</param>
        /// <param name="start">The index in the bytes of <paramref name="target"/> where the first of <paramref name="values"/> goes.</param>
        /// <param name="values">The bytes to write, in order. Those that would land past the end of the bytes are ignored.</param>
        /// <returns><see langword="true"/> if the bytes of <paramref name="target"/> changed and an undo step was recorded; <see langword="false"/> if
        /// <paramref name="start"/> is negative, <paramref name="values"/> is empty, or the bytes ended up unchanged.</returns>
        /// <exception cref="NullReferenceException"><paramref name="target"/> or <paramref name="values"/> is <see langword="null"/> (an extension
        /// method on a null instance reaches the call before any check).</exception>
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
        /// <param name="first">The first array to compare.</param>
        /// <param name="second">The second array to compare. If it is shorter or longer than <paramref name="first"/>, only the bytes both have are compared.</param>
        /// <returns>The indexes, in ascending order, at which <paramref name="first"/> and <paramref name="second"/> hold different bytes; empty if there are none.</returns>
        /// <exception cref="NullReferenceException"><paramref name="first"/> or <paramref name="second"/> is <see langword="null"/>.</exception>
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
