namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Splices fixed-length chunks back into the bytes of a savefile.
    /// The input bytes are never modified; a patched copy is returned.
    /// </summary>
    public sealed class SavefileBytesPatcher : ISavefileBytesPatcher
    {
        public byte[] Patch(byte[] originalSave, IEnumerable<BytesChunk> chunks)
        {
            var result = (byte[])originalSave.Clone();

            foreach (var chunk in Validate(chunks, result.Length))
            {
                chunk.CopyTo(result);
            }

            return result;
        }

        /// <summary>
        /// Writes over <paramref name="target"/> only the chunk bytes that differ from <paramref name="originalSave"/>.
        /// </summary>
        public byte[] PatchChanges(byte[] originalSave, byte[] target, IEnumerable<BytesChunk> chunks)
        {
            if (target.Length != originalSave.Length)
            {
                throw new ArgumentException("The target must have the same length as the original savefile.", nameof(target));
            }

            var result = (byte[])target.Clone();

            foreach (var chunk in Validate(chunks, result.Length))
            {
                var bytes = chunk.ToArray();
                for (var i = 0; i < bytes.Length; i++)
                {
                    if (bytes[i] != originalSave[chunk.StartIndex + i])
                    {
                        result[chunk.StartIndex + i] = bytes[i];
                    }
                }
            }

            return result;
        }

        private static IEnumerable<BytesChunk> Validate(IEnumerable<BytesChunk> chunks, int saveLength)
        {
            var patchedRanges = new List<(int Start, int End)>();

            foreach (var chunk in chunks)
            {
                if (chunk.StartIndex < 0 || chunk.End > saveLength)
                {
                    throw new ArgumentException(
                        $"Chunk '{chunk.Name}' range [{chunk.StartIndex}, {chunk.End}) is outside the savefile ({saveLength} bytes).");
                }

                foreach (var (start, end) in patchedRanges)
                {
                    if (chunk.StartIndex < end && start < chunk.End)
                    {
                        throw new ArgumentException($"Chunk '{chunk.Name}' overlaps another chunk.");
                    }
                }

                patchedRanges.Add((chunk.StartIndex, chunk.End));
                yield return chunk;
            }
        }
    }
}
