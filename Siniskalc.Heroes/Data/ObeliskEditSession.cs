namespace Siniskalc.Heroes.Data
{
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Entities;
    using Siniskalc.Heroes.Entities;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// A view onto the visited obelisks in a <see cref="SaveDocumentHistory"/>. It names no chunk, so a view model can hold it.
    /// </summary>
    /// <typeparam name="TColor">The colour of the game.</typeparam>
    public sealed class ObeliskEditSession<TColor> : EditSession<ObeliskChunk, ObeliskVisits<TColor>>
        where TColor : AbstractEnumeration, IPlayerColor
    {
        /// <param name="history">The history the edits go to.</param>
        /// <param name="kind">The kind of chunk the game keeps its obelisks in.</param>
        /// <param name="index">The place of the obelisk record (0, as a savefile has one).</param>
        public ObeliskEditSession(SaveDocumentHistory history, ChunkKind kind, int index)
            : base(history, kind, index)
        {
        }
    }
}
