namespace Siniskalc.Core.Data
{
    using System;
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
        /// <typeparam name="T">The kind of entity the byte holds.</typeparam>
        /// <param name="value">The byte read from the savefile.</param>
        /// <returns>The entity of type <typeparamref name="T"/> whose code is <paramref name="value"/> (the first one declared when several share it), or <see langword="null"/> if there is none.</returns>
        public static T? Read<T>(byte value)
            where T : AbstractEnumeration
        {
            return AbstractEnumeration.FromCode<T>(value);
        }

        /// <summary>
        /// Gives the byte to write for an optional entity: the byte that is already there while it reads as <paramref name="value"/> (so a byte
        /// the editor does not understand is left alone), otherwise the entity's own, or <paramref name="none"/> for <c>null</c>.
        /// </summary>
        /// <typeparam name="T">The kind of entity the byte holds.</typeparam>
        /// <param name="value">The entity the model holds, or <see langword="null"/> for none.</param>
        /// <param name="existing">The byte that is in the savefile now.</param>
        /// <param name="none">The byte that stands for no entity, written when <paramref name="value"/> is <see langword="null"/>.</param>
        /// <returns><paramref name="existing"/> if it reads as <paramref name="value"/> (a <see langword="null"/> value matches a byte that is no entity);
        /// otherwise <paramref name="none"/> for a <see langword="null"/> <paramref name="value"/>, or the code of <paramref name="value"/>.</returns>
        public static byte WriteKeeping<T>(T? value, byte existing, byte none)
            where T : AbstractEnumeration
        {
            return Read<T>(existing) == value ? existing : value is null ? none : value.Code;
        }

        /// <summary>
        /// Gives the byte to write for an entity that has a stand-in for the bytes it does not know: the byte that is already there while it
        /// reads as <paramref name="value"/>, otherwise the entity's own.
        /// </summary>
        /// <typeparam name="T">The kind of entity the byte holds.</typeparam>
        /// <param name="value">The entity the model holds.</param>
        /// <param name="existing">The byte that is in the savefile.</param>
        /// <param name="standIn">What a byte that is no entity reads as.</param>
        /// <returns><paramref name="existing"/> if it reads as <paramref name="value"/> (a byte that is no entity reads as <paramref name="standIn"/>);
        /// otherwise the code of <paramref name="value"/>.</returns>
        /// <exception cref="NullReferenceException"><paramref name="value"/> is <see langword="null"/> and <paramref name="existing"/> does not read as nothing
        /// (the parameter is not declared nullable, so this is a misuse).</exception>
        public static byte WriteKeeping<T>(T value, byte existing, T standIn)
            where T : AbstractEnumeration
        {
            return (Read<T>(existing) ?? standIn) == value ? existing : value.Code;
        }
    }
}
