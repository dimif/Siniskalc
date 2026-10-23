namespace Siniskalc.Homm3.Views
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using Siniskalc.Homm3.ViewModels;
    using Siniskalc.Wpf.Views;

    /// <summary>
    /// Shows the dwellings of one owner and the bytes of the dwelling that is picked. All behaviour is in
    /// <see cref="OwnedMapObjectsViewModel"/>; the control only binds to it and shows the bytes of the dwelling that is picked.
    /// </summary>
    public partial class OwnedDwellingsCard : UserControl
    {
        private OwnedMapObjectsViewModel? viewModel;
        private int boundDwelling = -1;

        public OwnedDwellingsCard()
        {
            this.InitializeComponent();
            AdvancedModeVisibility.Apply(this.hexCard);
            this.DataContextChanged += this.OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.viewModel != null)
            {
                this.viewModel.PropertyChanged -= this.OnViewModelChanged;
            }

            this.viewModel = e.NewValue as OwnedMapObjectsViewModel;
            this.boundDwelling = -1;
            if (this.viewModel != null)
            {
                this.viewModel.PropertyChanged += this.OnViewModelChanged;
            }

            this.ShowBytes();
        }

        private void OnViewModelChanged(object? sender, PropertyChangedEventArgs e) => this.ShowBytes();

        // The bytes follow the dwelling that is picked; an edit of the same dwelling leaves the editor as it is.
        private void ShowBytes()
        {
            var picked = this.viewModel?.SelectedDwelling?.Index ?? -1;
            if (picked == this.boundDwelling)
            {
                return;
            }

            this.boundDwelling = picked;
            if (this.viewModel?.CreateDwellingByteEditTarget() is { } target)
            {
                this.hexEditor.Bind(target);
            }
            else
            {
                this.hexEditor.Unbind();
            }
        }
    }
}
