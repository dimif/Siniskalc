namespace Siniskalc.Wpf.ViewModels
{
    public abstract class PageViewModelBase : AbstractViewModelBase
    {
        private bool hasPendingChanges;

        protected PageViewModelBase(string title)
        {
            this.Title = title;
        }

        public string Title { get; }

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
