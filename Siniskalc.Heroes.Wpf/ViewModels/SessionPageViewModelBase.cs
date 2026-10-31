namespace Siniskalc.Heroes.Wpf.ViewModels
{
    using System;
    using Microsoft.Extensions.Logging;
    using Siniskalc.Core.Data;
    using Siniskalc.Core.Runtime;
    using Siniskalc.Heroes.Models;

    /// <summary>
    /// A page that edits one chunk through an <see cref="IEditSession{TModel}"/>: it has pending changes when the bytes of the chunk
    /// differ from the savefile as loaded or saved, and a reset puts the chunk back to the model it was loaded with. An edit the rules of
    /// the game refuse (<see cref="GameRuleException"/>), a reset among them, is not made, and <see cref="Notice"/> says why.
    /// </summary>
    /// <typeparam name="TModel">The model of the record.</typeparam>
    public abstract class SessionPageViewModelBase<TModel> : ResettablePageViewModelBase
        where TModel : class
    {
        private static readonly ILogger Logger = LogFactory.For(typeof(SessionPageViewModelBase<>));

        private string notice = string.Empty;

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
        /// Gets why the last edit of the page was not made, such as "Blue has the most heroes a player can have.", or an empty text. The next
        /// edit that is made clears it.
        /// </summary>
        public string Notice
        {
            get => this.notice;
            private set
            {
                if (this.notice != value)
                {
                    this.notice = value;
                    this.RaisePropertyChanged(nameof(this.Notice));
                    this.RaisePropertyChanged(nameof(this.HasNotice));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether there is a <see cref="Notice"/> to show.
        /// </summary>
        public bool HasNotice => this.notice.Length > 0;

        /// <summary>
        /// Gets the session that edits the chunk.
        /// </summary>
        protected IEditSession<TModel> Session { get; }

        /// <inheritdoc/>
        protected override void Reset() => this.TryUpdate(_ => this.Session.Original);

        /// <summary>
        /// Applies <paramref name="edit"/> through the session. When the rules of the game refuse it, nothing is changed, <see cref="Notice"/>
        /// says why and the result is false; a fault that is no rule of the game is not caught.
        /// </summary>
        /// <param name="edit">The edit of the model.</param>
        /// <returns>Whether the edit was made.</returns>
        protected bool TryUpdate(Func<TModel, TModel> edit)
        {
            try
            {
                this.Session.Update(edit);
                this.Notice = string.Empty;
                return true;
            }
            catch (GameRuleException ex)
            {
                // Only the rules of the game are told to the user; a fault (a bad index, say) is not made to look like one.
                Logger.LogInformation("Refused an edit of the page {Page}: {Reason}", this.Title, ex.Message);
                this.Notice = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Sets whether the page has pending changes from the bytes of its chunk.
        /// </summary>
        protected void UpdateIsDirty() => this.HasPendingChanges = this.Session.IsDirty;
    }
}
