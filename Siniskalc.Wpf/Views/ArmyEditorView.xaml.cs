namespace Siniskalc.Wpf.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using Siniskalc.Wpf.ViewModels;

    /// <summary>
    /// The slots of an army and the finder to add creatures, shared by the creatures of a hero and the garrison of a castle, in every game.
    /// All behaviour is in an <c>ArmyViewModelBase</c>; the control only binds to it.
    /// </summary>
    public partial class ArmyEditorView : UserControl
    {
        public ArmyEditorView()
        {
            this.InitializeComponent();
        }

        // One way to upgrade changes the stack at once; several open a list of the creatures to choose from.
        private void UpgradeClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.DataContext is not StackSlotViewModel slot)
            {
                return;
            }

            if (!slot.HasSeveralUpgrades)
            {
                slot.UpgradeCommand.Execute(null);
                return;
            }

            var menu = new ContextMenu { PlacementTarget = button, Placement = PlacementMode.Bottom };
            foreach (var name in slot.Upgrades)
            {
                menu.Items.Add(new MenuItem { Header = name, Command = slot.UpgradeCommand, CommandParameter = name });
            }

            menu.IsOpen = true;
        }
    }
}
