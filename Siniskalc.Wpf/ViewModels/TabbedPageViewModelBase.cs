namespace Siniskalc.Wpf.ViewModels
{
    /// <summary>
    /// A page with tabs, such as a hero, a player or a castle: which tab is shown is <see cref="SelectedTab"/>, counted from 0, and a
    /// panel of the page is shown while its number is the selected one.
    /// </summary>
    public abstract class TabbedPageViewModelBase : PageViewModelBase
    {
        private int selectedTab;

        /// <summary>
        /// Initializes a new instance of the <see cref="TabbedPageViewModelBase"/> class.
        /// </summary>
        /// <param name="title">The title.</param>
        protected TabbedPageViewModelBase(string title)
            : base(title)
        {
        }

        /// <summary>
        /// Gets or sets the tab shown, counted from 0 in the order of the tab strip.
        /// </summary>
        public int SelectedTab
        {
            get => this.selectedTab;
            set
            {
                if (this.selectedTab != value)
                {
                    this.selectedTab = value;
                    this.RaisePropertyChanged(nameof(this.SelectedTab));
                }
            }
        }
    }
}
