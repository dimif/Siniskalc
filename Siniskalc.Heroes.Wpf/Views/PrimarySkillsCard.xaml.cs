namespace Siniskalc.Heroes.Wpf.Views
{
    using System.Windows.Controls;

    /// <summary>
    /// The card of the primary skills of a hero: the base, the total and what the artifacts add for each. The data context is a
    /// <c>PrimarySkillsCardViewModel</c>.
    /// </summary>
    public partial class PrimarySkillsCard : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimarySkillsCard"/> class.
        /// </summary>
        public PrimarySkillsCard()
        {
            this.InitializeComponent();
        }
    }
}
