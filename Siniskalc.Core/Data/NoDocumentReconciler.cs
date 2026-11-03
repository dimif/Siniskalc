namespace Siniskalc.Core.Data
{
    /// <summary>
    /// The reconciler of a savefile in which no record follows another: an edit is kept as it is.
    /// </summary>
    public sealed class NoDocumentReconciler : IDocumentReconciler
    {
        private NoDocumentReconciler()
        {
        }

        /// <summary>
        /// Gets the one instance.
        /// </summary>
        public static NoDocumentReconciler Instance { get; } = new();

        /// <inheritdoc/>
        public SaveDocument Reconcile(SaveDocument original, SaveDocument before, SaveDocument after) => after;
    }
}
