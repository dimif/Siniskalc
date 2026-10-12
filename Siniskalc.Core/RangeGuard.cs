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
        public static bool IsInRange<T>(T value, T min, T max)
            where T : INumber<T>
        {
            return value >= min && value <= max;
        }

        public static T Clamp<T>(T value, T min, T max)
            where T : INumber<T>
        {
            return T.Clamp(value, min, max);
        }

        /// <summary>
        /// Throws when <paramref name="value"/> is not between <paramref name="min"/> and <paramref name="max"/>, both included.
        /// </summary>
        /// <param name="message">What to tell the user; the default says which range is allowed.</param>
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
        public static void ThrowIfNotIndex(int index, int count, string paramName, string? message = null)
        {
            if (index < 0 || index >= count)
            {
                throw new ArgumentOutOfRangeException(paramName, index, message ?? $"{paramName} must be 0 to {count - 1}.");
            }
        }
    }
}
