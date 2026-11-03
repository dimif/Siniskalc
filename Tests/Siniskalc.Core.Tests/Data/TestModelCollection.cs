namespace Siniskalc.Core.Tests.Data
{
    using System.Collections.Generic;
    using Siniskalc.Core.Data;

    /// <summary>
    /// A collection of raw chunks whose model is the first byte of each, with no changes to report.
    /// </summary>
    internal sealed class TestModelCollection : ModelChunkCollection<RawBytesChunk, TestModel, TestModelCollection.Reader, TestModelCollection.Writer>
    {
        public TestModelCollection(IEnumerable<RawBytesChunk> chunks)
            : base(chunks, new Reader(), new Writer(), (_, _, _) => [])
        {
        }

        public override TestModelCollection WithChunk(int index, RawBytesChunk chunk) => new(this.Replace(index, chunk));

        internal sealed class Reader : IDeserializer<RawBytesChunk, TestModel>
        {
            public TestModel Deserialize(RawBytesChunk chunk) => new(chunk.GetByteAt(0));
        }

        internal sealed class Writer : ISerializer<RawBytesChunk, TestModel>
        {
            public RawBytesChunk Serialize(RawBytesChunk chunk, TestModel model)
            {
                var bytes = chunk.ToArray();
                bytes[0] = model.Value;
                return chunk.WithBytes(bytes);
            }
        }
    }
}
