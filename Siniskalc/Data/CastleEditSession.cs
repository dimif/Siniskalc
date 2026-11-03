namespace Siniskalc.Data
{
    using System;
    using Siniskalc.Core.Data;
    using Siniskalc.Entities;
    using Siniskalc.Models;

    /// <summary>
    /// A view onto a single castle in a <see cref="SaveDocumentHistory"/>.
    /// </summary>
    public sealed class CastleEditSession : EditSession<CastleChunk, CastleModel>
    {
        private bool restoringOwner;

        public CastleEditSession(SaveDocumentHistory history, int castleIndex)
            : base(history, Homm3ChunkKinds.Castle, castleIndex, history.Current.Castles().Deserializer, history.Current.Castles().Serializer)
        {
        }

        public int CastleIndex => this.Index;

        /// <summary>
        /// Reverts ownership and restores the town's original position in its owner's list.
        /// </summary>
        public void RestoreOwner(PlayerColor? owner) => this.Restore(c => c with { Owner = owner });

        /// <summary>
        /// Restores the castle and its ownership bookkeeping in a single edit.
        /// </summary>
        public void Reset() => this.Restore(_ => this.History.Original.Castles().GetModel(this.Index));

        protected override SaveDocument ApplyToDocument(SaveDocument document, CastleChunk chunk, CastleChunk newChunk) =>
            CastleOwnership.Apply(document, this.History.Original, this.Index, chunk, newChunk, this.restoringOwner);

        private void Restore(Func<CastleModel, CastleModel> edit)
        {
            this.restoringOwner = true;
            try
            {
                this.Update(edit);
            }
            finally
            {
                this.restoringOwner = false;
            }
        }

    }
}
