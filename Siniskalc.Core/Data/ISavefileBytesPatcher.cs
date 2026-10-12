namespace Siniskalc.Core.Data
{
    using System.Collections.Generic;

    /// <summary>
    /// Writes the chunks of a savefile over its original bytes.
    /// </summary>
    public interface ISavefileBytesPatcher
    {
        byte[] Patch(byte[] originalSave, IEnumerable<BytesChunk> chunks);

        byte[] PatchChanges(byte[] originalSave, byte[] target, IEnumerable<BytesChunk> chunks);
    }
}
