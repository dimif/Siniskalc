namespace Siniskalc.Core.Data
{
    using System;

    /// <summary>
    /// Edits one record of a savefile as a model, without a view model having to know the chunk behind it.
    /// </summary>
    /// <typeparam name="TModel">The model of the record.</typeparam>
    public interface IEditSession<TModel>
        where TModel : class
    {
        /// <summary>
        /// Gets the history of the savefile the record is in.
        /// </summary>
        SaveDocumentHistory History { get; }

        /// <summary>
        /// Gets what tells that the history of the savefile changed. A page listens to this, not to <see cref="History"/>, so that it can be
        /// shown with a session that has no savefile behind it.
        /// </summary>
        IHistoryEvents Events { get; }

        /// <summary>
        /// Gets the place of the record among the records of its kind.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// Gets the record as it is now.
        /// </summary>
        TModel Current { get; }

        /// <summary>
        /// Gets the record as it was loaded or last saved.
        /// </summary>
        TModel Original { get; }

        /// <summary>
        /// Gets whether the record differs from the savefile as it was loaded or last saved.
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Replaces the record with what <paramref name="edit"/> makes of it.
        /// </summary>
        /// <param name="edit">Returns the edited model.</param>
        void Update(Func<TModel, TModel> edit);
    }
}
