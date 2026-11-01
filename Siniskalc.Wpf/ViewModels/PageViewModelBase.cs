namespace Siniskalc.Wpf.ViewModels
{
    public abstract class PageViewModelBase : AbstractViewModelBase
    {
        private bool hasPendingChanges;

        /// <summary>
        /// Initializes a new instance of the <see cref="PageViewModelBase"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        protected PageViewModelBase(string title)
        {
            this.Title = title;
        }

        /// <summary>
        /// Gets the title of the page.
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the page shows something that has been edited.
        /// </summary>
        public bool HasPendingChanges
        {
            get => this.hasPendingChanges;

            set
            {
                if (this.hasPendingChanges == value)
                {
                    return;
                }

                this.hasPendingChanges = value;
                this.RaisePropertyChanged(nameof(this.HasPendingChanges));
            }
        }
    }
}
