namespace Siniskalc.Homm3.Views
{
    using System.Windows.Controls;

    /// <summary>
    /// Shows the mines and the guard of the mine that is picked of one owner. All behaviour is in <see cref="Siniskalc.Homm3.ViewModels.OwnedMapObjectsViewModel"/>; the control only binds to it.
    /// </summary>
    public partial class OwnedMinesCards : UserControl
    {
        public OwnedMinesCards()
        {
            this.InitializeComponent();
        }
    }
}
