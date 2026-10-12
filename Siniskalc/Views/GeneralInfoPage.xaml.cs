namespace Siniskalc.Views
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using Siniskalc.ViewModels;

    /// <summary>
    /// Interaction logic for GeneralInfo.xaml
    /// </summary>
    public partial class GeneralInfoPage : Page
    {
        private bool artilleryWasChecked;

        public GeneralInfoPage(GeneralInfoPageViewModel viewModel)
        {
            this.InitializeComponent();

            // Radio groups span the whole window, so each page needs its own or heroes would uncheck each other's machines.
            var artilleryGroup = $"Artillery{Guid.NewGuid():N}";
            this.BallistaRadio.GroupName = artilleryGroup;
            this.Cannon.GroupName = artilleryGroup;

            this.DataContext = viewModel;
        }

        // A radio button cannot be unticked by clicking it, but the ballista or the cannon can be taken away like the other machines.
        private void RememberArtillery(object sender, RoutedEventArgs e) => this.artilleryWasChecked = sender is RadioButton { IsChecked: true };

        private void RememberArtilleryKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Space)
            {
                this.RememberArtillery(sender, e);
            }
        }

        private void UntickArtillery(object sender, RoutedEventArgs e)
        {
            if (this.artilleryWasChecked && sender is RadioButton radio)
            {
                this.artilleryWasChecked = false;
                radio.IsChecked = false;
            }
        }

        private void AddWarMachine(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is not GeneralInfoPageViewModel viewModel || e.Source is not ToggleButton component || component.Tag is not string tag)
            {
                return;
            }

            viewModel.UpdateWarMachine(tag, true);
        }

        private void RemoveWarMachine(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is not GeneralInfoPageViewModel viewModel || e.Source is not ToggleButton component || component.Tag is not string tag)
            {
                return;
            }

            viewModel.UpdateWarMachine(tag, false);
        }
    }
}
