namespace Siniskalc.Core
{
    using System;
    using System.Numerics;

    /// <summary>
    /// The one place that checks whether a value is in range, clamps it, or rejects it. The limits come from <see cref="Constants"/>.
    /// Rejecting throws <see cref="ArgumentOutOfRangeException"/>, which callers and tests rely on.
    /// </summary>
    public static class RangeGuard
    {
        /// <summary>
        /// Returns whether <paramref name="value"/> is between <paramref name="min"/> and <paramref name="max"/>, both included.
        /// </summary>
        /// <typeparam name="T">The type of number.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="min">The least value allowed.</param>
        /// <param name="max">The greatest value allowed.</param>
        /// <returns><see langword="true"/> if <paramref name="min"/> &lt;= <paramref name="value"/> &lt;= <paramref name="max"/>; otherwise <see langword="false"/>.</returns>
        public static bool IsInRange<T>(T value, T min, T max)
            where T : INumber<T>
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Returns <paramref name="value"/> brought into the range from <paramref name="min"/> to <paramref name="max"/>.
        /// </summary>
        /// <typeparam name="T">The type of number.</typeparam>
        /// <param name="value">The value to clamp.</param>
        /// <param name="min">The least value allowed.</param>
        /// <param name="max">The greatest value allowed.</param>
        /// <returns><paramref name="min"/> if <paramref name="value"/> is below it, <paramref name="max"/> if it is above, else <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="min"/> is greater than <paramref name="max"/>.</exception>
        public static T Clamp<T>(T value, T min, T max)
            where T : INumber<T>
        {
            return T.Clamp(value, min, max);
        }

        /// <summary>
        /// Throws when <paramref name="value"/> is not between <paramref name="min"/> and <paramref name="max"/>, both included.
        /// </summary>
        /// <typeparam name="T">The type of number.</typeparam>
        /// <param name="value">The value to check.</param>
        /// <param name="min">The least value allowed.</param>
        /// <param name="max">The greatest value allowed.</param>
        /// <param name="paramName">The name of the parameter that holds <paramref name="value"/>.</param>
        /// <param name="message">What to tell the user; the default says which range is allowed.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is below <paramref name="min"/> or above <paramref name="max"/>.</exception>
        public static void ThrowIfOutOfRange<T>(T value, T min, T max, string paramName, string? message = null)
            where T : INumber<T>
        {
            if (!IsInRange(value, min, max))
            {
                throw new ArgumentOutOfRangeException(paramName, value, message ?? $"{paramName} must be {min} to {max}.");
            }
        }

        /// <summary>
        /// Throws when <paramref name="index"/> is not a position in a list of <paramref name="count"/> items.
        /// </summary>
        /// <param name="index">The position to check.</param>
        /// <param name="count">The number of items in the list.</param>
        /// <param name="paramName">The name of the parameter that holds <paramref name="index"/>.</param>
        /// <param name="message">What to tell the user; the default says which positions are allowed.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative or not less than <paramref name="count"/>.</exception>
        public static void ThrowIfNotIndex(int index, int count, string paramName, string? message = null)
        {
            if (index < 0 || index >= count)
            {
                throw new ArgumentOutOfRangeException(paramName, index, message ?? $"{paramName} must be 0 to {count - 1}.");
            }
        }
    }
}
