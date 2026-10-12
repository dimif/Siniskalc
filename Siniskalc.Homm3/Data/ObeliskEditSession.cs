namespace Siniskalc.Homm3.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Homm3.Models;

    /// <summary>
    /// A view onto the visited obelisks in a <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public sealed class ObeliskEditSession : EditSession<ObeliskChunk, ObeliskVisits>
    {
        public ObeliskEditSession(SaveDocumentHistory history, int index)
            : base(history, Homm3ChunkKinds.Obelisks, index, history.Current.Obelisks().Deserializer, history.Current.Obelisks().Serializer)
        {
        }
    }
}
