namespace Siniskalc.Core.Data
{
    using System;
    using System.Buffers.Binary;

    /// <summary>
    /// Helpers for reading and writing little-endian primitive values to a byte buffer.
    /// </summary>
    public static class ByteBufferHelper
    {
        public static uint ReadUInt32(byte[] buffer, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(offset));

        public static ulong ReadUInt64(byte[] buffer, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(buffer.AsSpan(offset));

        public static void WriteInt16(byte[] buffer, int offset, short value) => BinaryPrimitives.WriteInt16LittleEndian(buffer.AsSpan(offset), value);

        public static void WriteUInt16(byte[] buffer, int offset, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(buffer.AsSpan(offset), value);

        public static void WriteInt32(byte[] buffer, int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), value);

        public static void WriteUInt32(byte[] buffer, int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(buffer.AsSpan(offset), value);

        public static void WriteUInt64(byte[] buffer, int offset, ulong value) => BinaryPrimitives.WriteUInt64LittleEndian(buffer.AsSpan(offset), value);
    }
}
