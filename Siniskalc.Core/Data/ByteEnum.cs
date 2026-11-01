namespace Siniskalc.Core.Data
{
    using System;

    /// <summary>
    /// Reads and writes enums that a savefile stores as one byte. A byte that is not a defined value is not an error when
    /// reading with <see cref="TryRead{TEnum}"/>, as savefiles of mods and unknown versions can hold any byte.
    /// </summary>
    public static class ByteEnum
    {
        /// <summary>
        /// Reads a byte as a value of the enumeration and says whether it is one of the values the enumeration defines.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration the byte holds a value of.</typeparam>
        /// <param name="value">The byte read from the savefile.</param>
        /// <param name="result"><paramref name="value"/> converted to <typeparamref name="TEnum"/>, also when it is not a defined value (then the number is kept as it is).</param>
        /// <returns><see langword="true"/> if <paramref name="value"/> is a defined value of <typeparamref name="TEnum"/>; otherwise <see langword="false"/>.</returns>
        public static bool TryRead<TEnum>(byte value, out TEnum result)
            where TEnum : struct, Enum
        {
            result = (TEnum)Enum.ToObject(typeof(TEnum), value);
            return Enum.IsDefined(result);
        }

        /// <summary>
        /// Reads a byte as a value of the enumeration, or <c>null</c> when it is none of its values (a "nobody" byte such as 0xFF, say).
        /// </summary>
        /// <typeparam name="TEnum">The enumeration the byte holds a value of.</typeparam>
        /// <param name="value">The byte read from the savefile.</param>
        /// <returns>The value of <typeparamref name="TEnum"/> that <paramref name="value"/> stands for, or <see langword="null"/> if it is not a defined value.</returns>
        public static TEnum? ReadOrNull<TEnum>(byte value)
            where TEnum : struct, Enum
        {
            return TryRead<TEnum>(value, out var result) ? result : null;
        }

        /// <summary>
        /// Gives the byte to write for an optional value: the byte that is already there while it reads as <paramref name="value"/> (so a byte
        /// the editor does not understand is left alone), otherwise the value's own, or <paramref name="none"/> for <c>null</c>.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration the byte holds a value of.</typeparam>
        /// <param name="value">The value the model holds, or <see langword="null"/> for none.</param>
        /// <param name="existing">The byte that is in the savefile now.</param>
        /// <param name="none">The byte that stands for no value, written when <paramref name="value"/> is <see langword="null"/>.</param>
        /// <returns><paramref name="existing"/> if it reads as <paramref name="value"/> (a <see langword="null"/> value matches a byte that is no defined value);
        /// otherwise <paramref name="none"/> for a <see langword="null"/> <paramref name="value"/>, or the byte of <paramref name="value"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> has to be written and is not a defined value of <typeparamref name="TEnum"/>.</exception>
        /// <exception cref="OverflowException"><paramref name="value"/> has to be written and its number does not fit in a byte.</exception>
        public static byte WriteKeeping<TEnum>(TEnum? value, byte existing, byte none)
            where TEnum : struct, Enum
        {
            return Nullable.Equals(ReadOrNull<TEnum>(existing), value) ? existing : value is null ? none : Write(value.Value);
        }

        /// <summary>
        /// Gives the byte to write for a value of the enumeration.
        /// </summary>
        /// <typeparam name="TEnum">The enumeration the byte holds a value of.</typeparam>
        /// <param name="value">The value to store.</param>
        /// <returns>The number of <paramref name="value"/> as a byte.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is not a defined value of <typeparamref name="TEnum"/>.</exception>
        /// <exception cref="OverflowException">The number of <paramref name="value"/> does not fit in a byte.</exception>
        public static byte Write<TEnum>(TEnum value)
            where TEnum : struct, Enum
        {
            return Enum.IsDefined(value)
                ? Convert.ToByte(value)
                : throw new ArgumentOutOfRangeException(nameof(value), value, $"Value {value} is not a valid {typeof(TEnum).Name}");
        }
    }
}
