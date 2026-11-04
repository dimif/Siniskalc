namespace Siniskalc.Homm3.Views
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// The slots of an army and the finder to add creatures, shared by the creatures of a hero and the garrison of a castle.
    /// All behaviour is in <see cref="Siniskalc.Homm3.ViewModels.ArmyViewModel"/>; the control only binds to it.
    /// </summary>
    public partial class ArmyEditor : UserControl
    {
        public static readonly DependencyProperty ShowFinderProperty = DependencyProperty.Register(
            nameof(ShowFinder), typeof(bool), typeof(ArmyEditor), new PropertyMetadata(true));

        /// <summary>
        /// Initializes a new instance of the <see cref="ArmyEditor"/> class.
        /// </summary>
        public ArmyEditor()
        {
            this.InitializeComponent();
        }

        public bool ShowFinder
        {
            get => (bool)this.GetValue(ShowFinderProperty);
            set => this.SetValue(ShowFinderProperty, value);
        }
    }
}
