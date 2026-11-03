namespace Siniskalc.Homm3.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm3.ViewModels;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// Shows the dwellings of one owner and the bytes of the dwelling that is picked. All behaviour is in
    /// <see cref="OwnedMapObjectsViewModel"/>; the control only binds to it and shows the bytes of the dwelling that is picked.
    /// </summary>
    public partial class OwnedDwellingsCard : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OwnedDwellingsCard"/> class.
        /// </summary>
        public OwnedDwellingsCard()
        {
            this.InitializeComponent();
            AdvancedModeVisibility.Apply(this.hexCard);
            _ = new SelectedRecordHexPresenter<OwnedMapObjectsViewModel>(
                this,
                this.hexCard,
                this.hexEditor,
                page => page,
                page => page.SelectedDwelling?.Index,
                page => page.CreateDwellingByteEditTarget());
        }
    }
}
