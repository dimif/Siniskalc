namespace Siniskalc.Core.Data
{
    using System;

    /// <summary>
    /// Reads and writes enums that a savefile stores as one byte. A byte that is not a defined value is not an error when
    /// reading with <see cref="TryRead{TEnum}"/>, as savefiles of mods and unknown versions can hold any byte.
    /// </summary>
    public static class ByteEnum
    {
        public static bool TryRead<TEnum>(byte value, out TEnum result)
            where TEnum : struct, Enum
        {
            result = (TEnum)Enum.ToObject(typeof(TEnum), value);
            return Enum.IsDefined(result);
        }

        /// <summary>
        /// Reads a byte as a value of the enumeration, or <c>null</c> when it is none of its values (a "nobody" byte such as 0xFF, say).
        /// </summary>
        public static TEnum? ReadOrNull<TEnum>(byte value)
            where TEnum : struct, Enum
        {
            return TryRead<TEnum>(value, out var result) ? result : null;
        }

        /// <summary>
        /// Gives the byte to write for an optional value: the byte that is already there while it reads as <paramref name="value"/> (so a byte
        /// the editor does not understand is left alone), otherwise the value's own, or <paramref name="none"/> for <c>null</c>.
        /// </summary>
        public static byte WriteKeeping<TEnum>(TEnum? value, byte existing, byte none)
            where TEnum : struct, Enum
        {
            return Nullable.Equals(ReadOrNull<TEnum>(existing), value) ? existing : value is null ? none : Write(value.Value);
        }

        public static byte Write<TEnum>(TEnum value)
            where TEnum : struct, Enum
        {
            return Enum.IsDefined(value)
                ? Convert.ToByte(value)
                : throw new ArgumentOutOfRangeException(nameof(value), value, $"Value {value} is not a valid {typeof(TEnum).Name}");
        }
    }
}
