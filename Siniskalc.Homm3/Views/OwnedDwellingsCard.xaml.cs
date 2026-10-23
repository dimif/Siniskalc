namespace Siniskalc.Homm3.Views
{
    using System.Windows.Controls;

    /// <summary>
    /// Shows the dwellings of one owner. All behaviour is in <see cref="Siniskalc.Homm3.ViewModels.OwnedMapObjectsViewModel"/>; the control only binds to it.
    /// </summary>
    public partial class OwnedDwellingsCard : UserControl
    {
        public OwnedDwellingsCard()
        {
            this.InitializeComponent();
        }
    }
}
