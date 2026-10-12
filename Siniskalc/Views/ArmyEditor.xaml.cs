namespace Siniskalc.Views
{
    using System.Windows.Controls;

    /// <summary>
    /// The slots of an army and the finder to add creatures, shared by the creatures of a hero and the garrison of a castle.
    /// All behaviour is in <see cref="Siniskalc.ViewModels.ArmyViewModel"/>; the control only binds to it.
    /// </summary>
    public partial class ArmyEditor : UserControl
    {
        public ArmyEditor()
        {
            this.InitializeComponent();
        }
    }
}
