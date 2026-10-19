namespace Siniskalc.Core.Data
{
    using Siniskalc.Core.Entities;

    /// <summary>
    /// Reads and writes the byte a savefile holds for an entity of a game (a colour, a faction), where a byte that is no entity reads as
    /// nothing and is left alone while the model holds nothing.
    /// </summary>
    public static class ByteEnumerations
    {
        /// <summary>
        /// Reads a byte as an entity, or <c>null</c> when it is none of them (a "nobody" byte such as 0xFF, say).
        /// </summary>
        public static T? Read<T>(byte value)
            where T : AbstractEnumeration
        {
            return AbstractEnumeration.FromCode<T>(value);
        }

        /// <summary>
        /// Gives the byte to write for an optional entity: the byte that is already there while it reads as <paramref name="value"/> (so a byte
        /// the editor does not understand is left alone), otherwise the entity's own, or <paramref name="none"/> for <c>null</c>.
        /// </summary>
        public static byte WriteKeeping<T>(T? value, byte existing, byte none)
            where T : AbstractEnumeration
        {
            return Read<T>(existing) == value ? existing : value is null ? none : value.Code;
        }
    }
}
