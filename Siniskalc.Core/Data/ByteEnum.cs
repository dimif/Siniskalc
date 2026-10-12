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

        public static byte Write<TEnum>(TEnum value)
            where TEnum : struct, Enum
        {
            return Enum.IsDefined(value)
                ? Convert.ToByte(value)
                : throw new ArgumentOutOfRangeException(nameof(value), value, $"Value {value} is not a valid {typeof(TEnum).Name}");
        }
    }
}
