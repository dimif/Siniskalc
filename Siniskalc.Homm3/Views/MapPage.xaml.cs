namespace Siniskalc.Homm3.Views
{
    using System.Windows.Controls;
    using System.Windows.Threading;
    using Siniskalc.Homm3.ViewModels;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// What the map keeps that no player owns: merchant stock and creature banks.
    /// </summary>
    public partial class MapPage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MapPage"/> class.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        public MapPage(MapViewModel viewModel)
        {
            this.InitializeComponent();

            this.DataContext = viewModel;
            AdvancedModeVisibility.Apply(this.bankHexCard);
            _ = new SelectedRecordHexPresenter<MapViewModel>(
                this,
                this.bankHexCard,
                this.bankHexEditor,
                page => page,
                page => page.SelectedBank?.Index,
                page => page.CreateBankByteEditTarget());
        }

        private void OnBankSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source == this.banks && this.banks.SelectedItem is { } selected)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
                {
                    if (this.banks.SelectedItem == selected && this.banks.ItemContainerGenerator.ContainerFromItem(selected) is ListBoxItem item)
                    {
                        item.BringIntoView();
                    }
                });
            }
        }
    }
}
