namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using Siniskalc.Core.Data;

    /// <summary>
    /// A page that edits one chunk through an <see cref="IEditSession{TModel}"/>: it has pending changes when the bytes of the chunk
    /// differ from the savefile as loaded or saved, and a reset puts the chunk back to the model it was loaded with.
    /// </summary>
    /// <typeparam name="TModel">The model of the record.</typeparam>
    public abstract class SessionPageViewModelBase<TModel> : ResettablePageViewModelBase
        where TModel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionPageViewModelBase{TModel}"/> class.
        /// </summary>
        /// <param name="title">The title of the page.</param>
        /// <param name="session">The session that edits the chunk.</param>
        protected SessionPageViewModelBase(string title, IEditSession<TModel> session)
            : base(title, session.Events)
        {
            this.Session = session;
        }

        /// <summary>
        /// Gets the session that edits the chunk.
        /// </summary>
        protected IEditSession<TModel> Session { get; }

        /// <inheritdoc/>
        protected override void Reset() => this.Session.Update(_ => this.Session.Original);

        /// <summary>
        /// Sets whether the page has pending changes from the bytes of its chunk.
        /// </summary>
        protected void UpdateIsDirty() => this.HasPendingChanges = this.Session.IsDirty;
    }
}
