namespace Siniskalc.Core.Data
{
    using System;
    using System.Buffers.Binary;

    /// <summary>
    /// Helpers for reading and writing little-endian primitive values to a byte buffer.
    /// </summary>
    public static class ByteBufferHelper
    {
        /// <summary>
        /// Reads an unsigned 32-bit integer stored little-endian.
        /// </summary>
        /// <param name="buffer">The bytes to read from.</param>
        /// <param name="offset">The index in <paramref name="buffer"/> of the first of the 4 bytes.</param>
        /// <returns>The value stored at <paramref name="offset"/> in <paramref name="buffer"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is negative, or <paramref name="buffer"/> has fewer than 4 bytes from <paramref name="offset"/> on (also when <paramref name="buffer"/> is <see langword="null"/> and <paramref name="offset"/> is not 0, or when it is empty).</exception>
        public static uint ReadUInt32(byte[] buffer, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(offset));

        /// <summary>
        /// Reads an unsigned 64-bit integer stored little-endian.
        /// </summary>
        /// <param name="buffer">The bytes to read from.</param>
        /// <param name="offset">The index in <paramref name="buffer"/> of the first of the 8 bytes.</param>
        /// <returns>The value stored at <paramref name="offset"/> in <paramref name="buffer"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is negative, or <paramref name="buffer"/> has fewer than 8 bytes from <paramref name="offset"/> on (also when <paramref name="buffer"/> is <see langword="null"/> and <paramref name="offset"/> is not 0, or when it is empty).</exception>
        public static ulong ReadUInt64(byte[] buffer, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(buffer.AsSpan(offset));

        /// <summary>
        /// Writes a signed 16-bit integer little-endian.
        /// </summary>
        /// <param name="buffer">The bytes to write to; 2 of them are changed.</param>
        /// <param name="offset">The index in <paramref name="buffer"/> of the first of the 2 bytes.</param>
        /// <param name="value">The value to store.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is negative, or <paramref name="buffer"/> has fewer than 2 bytes from <paramref name="offset"/> on (also when <paramref name="buffer"/> is <see langword="null"/> and <paramref name="offset"/> is not 0, or when it is empty). Nothing is written then.</exception>
        public static void WriteInt16(byte[] buffer, int offset, short value) => BinaryPrimitives.WriteInt16LittleEndian(buffer.AsSpan(offset), value);

        /// <summary>
        /// Writes an unsigned 16-bit integer little-endian.
        /// </summary>
        /// <param name="buffer">The bytes to write to; 2 of them are changed.</param>
        /// <param name="offset">The index in <paramref name="buffer"/> of the first of the 2 bytes.</param>
        /// <param name="value">The value to store.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is negative, or <paramref name="buffer"/> has fewer than 2 bytes from <paramref name="offset"/> on (also when <paramref name="buffer"/> is <see langword="null"/> and <paramref name="offset"/> is not 0, or when it is empty). Nothing is written then.</exception>
        public static void WriteUInt16(byte[] buffer, int offset, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset), value);

        /// <summary>
        /// Writes a signed 32-bit integer little-endian.
        /// </summary>
        /// <param name="buffer">The bytes to write to; 4 of them are changed.</param>
        /// <param name="offset">The index in <paramref name="buffer"/> of the first of the 4 bytes.</param>
        /// <param name="value">The value to store.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is negative, or <paramref name="buffer"/> has fewer than 4 bytes from <paramref name="offset"/> on (also when <paramref name="buffer"/> is <see langword="null"/> and <paramref name="offset"/> is not 0, or when it is empty). Nothing is written then.</exception>
        public static void WriteInt32(byte[] buffer, int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), value);

        /// <summary>
        /// Writes an unsigned 32-bit integer little-endian.
        /// </summary>
        /// <param name="buffer">The bytes to write to; 4 of them are changed.</param>
        /// <param name="offset">The index in <paramref name="buffer"/> of the first of the 4 bytes.</param>
        /// <param name="value">The value to store.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is negative, or <paramref name="buffer"/> has fewer than 4 bytes from <paramref name="offset"/> on (also when <paramref name="buffer"/> is <see langword="null"/> and <paramref name="offset"/> is not 0, or when it is empty). Nothing is written then.</exception>
        public static void WriteUInt32(byte[] buffer, int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset), value);

        /// <summary>
        /// Writes an unsigned 64-bit integer little-endian.
        /// </summary>
        /// <param name="buffer">The bytes to write to; 8 of them are changed.</param>
        /// <param name="offset">The index in <paramref name="buffer"/> of the first of the 8 bytes.</param>
        /// <param name="value">The value to store.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="offset"/> is negative, or <paramref name="buffer"/> has fewer than 8 bytes from <paramref name="offset"/> on (also when <paramref name="buffer"/> is <see langword="null"/> and <paramref name="offset"/> is not 0, or when it is empty). Nothing is written then.</exception>
        public static void WriteUInt64(byte[] buffer, int offset, ulong value) => BinaryPrimitives.WriteUInt64LittleEndian(buffer.AsSpan(offset), value);
    }
}
