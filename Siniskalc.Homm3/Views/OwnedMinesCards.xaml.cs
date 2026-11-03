namespace Siniskalc.Homm3.Views
{
    using System.Windows.Controls;
    using Siniskalc.Homm3.ViewModels;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// Shows the mines and the guard of the mine that is picked of one owner, and the bytes of that mine. All behaviour is in
    /// <see cref="OwnedMapObjectsViewModel"/>; the control only binds to it and shows the bytes of the mine that is picked.
    /// </summary>
    public partial class OwnedMinesCards : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OwnedMinesCards"/> class.
        /// </summary>
        public OwnedMinesCards()
        {
            this.InitializeComponent();
            AdvancedModeVisibility.Apply(this.hexCard);
            _ = new SelectedRecordHexPresenter<OwnedMapObjectsViewModel>(
                this,
                this.hexCard,
                this.hexEditor,
                page => page.Mines,
                page => page.Mines.SelectedMine?.Index,
                page => page.CreateMineByteEditTarget());
        }
    }
}
