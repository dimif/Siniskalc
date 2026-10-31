namespace Siniskalc.Core.Data
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Runtime;

    /// <summary>
    /// Splices fixed-length chunks back into the bytes of a savefile.
    /// The input bytes are never modified; a patched copy is returned.
    /// </summary>
    public sealed class SavefileBytesPatcher : ISavefileBytesPatcher
    {
        private static readonly ILogger<SavefileBytesPatcher> Logger = LogFactory.For<SavefileBytesPatcher>();

        public byte[] Patch(byte[] originalSave, IEnumerable<BytesChunk> chunks)
        {
            var result = (byte[])originalSave.Clone();

            var count = 0;
            foreach (var chunk in Validate(chunks, result.Length))
            {
                chunk.CopyTo(result);
                count++;
            }

            Logger.LogDebug("Patched {Chunks} chunks into the {Length} bytes of the savefile", count, result.Length);
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

            var count = 0;
            var changed = 0;
            foreach (var chunk in Validate(chunks, result.Length))
            {
                count++;
                var bytes = chunk.ToArray();
                for (var i = 0; i < bytes.Length; i++)
                {
                    if (bytes[i] != originalSave[chunk.StartIndex + i])
                    {
                        result[chunk.StartIndex + i] = bytes[i];
                        changed++;
                    }
                }
            }

            Logger.LogDebug("Wrote {Changed} changed bytes from {Chunks} chunks into the {Length} bytes of the savefile", changed, count, result.Length);
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
