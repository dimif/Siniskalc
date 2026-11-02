namespace Siniskalc.Heroes.Wpf.Views
{
    using System.Windows.Controls;

    /// <summary>
    /// The card of the adventure spells that are active on a hero this turn. The data context is an <c>ActiveSpellsCardViewModel</c>.
    /// </summary>
    public partial class ActiveSpellsCard : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveSpellsCard"/> class.
        /// </summary>
        public ActiveSpellsCard()
        {
            this.InitializeComponent();
        }
    }
}
